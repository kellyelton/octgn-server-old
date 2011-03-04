using System;
using System.Collections.Generic;
using Octgn.Play;
using System.Linq;

namespace Octgn.Script
{
	delegate void GroupAction(Group group, Player player);
	delegate void CardAction(Card card, Player player);

	public class BadScriptException : Exception
	{
		public BadScriptException(string msg)
			: base(msg)
		{ }
	}

	static class ScriptEngine
	{
		internal static ScriptExecution CurrentScript;

		/*        internal static Card GetCard(string which)
						{
								string[] tokens = which.Split('.');
								object obj = null;
								for (int i = 0; i < tokens.Length; ++i)
										switch (tokens[i])
										{
												case "this":
														obj = (object)CurrentScript.thisCard ?? (object)CurrentScript.thisGroup; break;
												case "top":
														obj = (obj as Pile).TopCard; break;
												default:
														throw new BadScriptException("Unknown part: " + tokens[i]);
										}
								Card result = obj as Card;
								return result;
						}*/

		internal static Group GetGroup(string which)
		{
			string[] tokens = which.Split('.');
			object obj = null;
			for (int i = 0; i < tokens.Length; ++i)
			{
				if (tokens[i].StartsWith("#"))
				{
					string name = tokens[i].Substring(1);
					foreach (Octgn.Definitions.GroupDef def in Program.Game.Definition.PlayerDefinition.Groups)
						if (string.Compare(def.Name, name, true) == 0)
							return ((Player)obj).IndexedGroups[def.Id];
					throw new BadScriptException("Unknown group: " + name);
				}
				else
					switch (tokens[i])
					{
						case "me":
							obj = Octgn.Play.Player.LocalPlayer; break;
						case "hand":
							obj = ((Player)obj).Hand; break;
						case "table":
							obj = Program.Game.Table; break;
						case "this":
							obj = CurrentScript.thisGroup; break;
						default:
							throw new BadScriptException("Unknown part: " + tokens[i]);
					}
			}
			Group result = obj as Group;
			if (result == null) throw new BadScriptException("No group found for: " + which);
			return result;
		}

		internal static Player GetPlayer(string which)
		{
			string[] tokens = which.Split('.');
			object obj = null;
			for (int i = 0; i < tokens.Length; ++i)
				switch (tokens[i])
				{
					case "me":
						obj = Octgn.Play.Player.LocalPlayer; break;
					default:
						throw new BadScriptException("Unknown part: " + tokens[i]);
				}
			Player result = obj as Player;
			if (result == null) throw new BadScriptException("No player found for: " + which);
			return result;
		}

		internal static void SetVariable(string name, object value)
		{ SetVariable(name, value, CurrentScript); }

    internal static void SetVariable(string name, object value, ScriptExecution script)
    {
      if (value is int)
      {
        // First try inside the global variables
        if (Program.Game.Variables.ContainsKey(name))
        {
          Program.Game.Variables[name] = (int)value;
          return;
        }
        // Then try current player's variables
        if (Player.LocalPlayer.Variables.ContainsKey(name))
        {
          Player.LocalPlayer.Variables[name] = (int)value;
          return;
        }
      }
      // Else falls back on local script variables
      script.SetVariable(name, value);
    }

		internal static object GetVariable(string name)
		{
      int intValue;
      // First try inside the global variables
      if (Program.Game.Variables.TryGetValue(name, out intValue))
        return intValue;
      // Then try current player's variables
      if (Player.LocalPlayer.Variables.TryGetValue(name, out intValue))
        return intValue;
      // Else falls back on local script variables
			return CurrentScript.vars[name];
		}

		internal static T GetVariable<T>(string name)
		{ return (T)GetVariable(name); }

    internal static T GetLiteralOrVariable<T>(string text)
    {
        if (text.StartsWith("$")) return GetVariable<T>(text);
        return (T)Convert.ChangeType(text, typeof(T));            
    }

		internal static void PushFrame(Action[] actions)
		{
			if (actions == null || actions.Length == 0) return;
			CurrentScript.frames.Push(new ScriptFrame(actions));
		}

		internal static void Stop()
		{ CurrentScript = null; }

		internal static void Mute(bool muted)
		{ CurrentScript.muted = muted; }

		public static void Run(Action[] actions, Play.Group group)
		{
			CurrentScript = new ScriptExecution() { thisGroup = group };
			PushFrame(actions);
			RunLoop();
		}

		public static void Run(Action[] actions, Play.Group group, System.Windows.Point position)
		{
			CurrentScript = new ScriptExecution() 
			{ thisGroup = group, posX = position.X, posY = position.Y };			
			PushFrame(actions);
			RunLoop();
		}

		public static void Run(Action[] actions, IEnumerable<Play.Card> cards)
		{
			CurrentScript = new ScriptExecution() { thisCards = cards.ToList() };
			PushFrame(actions);
			RunLoop();
		}

    public static void Run(Action[] actions, IEnumerable<Play.Card> cards, System.Windows.Point position)
    {
      CurrentScript = new ScriptExecution() 
      { thisCards = cards.ToList(), posX = position.X, posY = position.Y };
      PushFrame(actions);
      RunLoop();
    }

		internal static ScriptExecution Suspend()
		{
			if (CurrentScript != null) CurrentScript.suspendedCount++;
			return CurrentScript;
		}

		internal static void Continue(ScriptExecution exec)
		{
			System.Diagnostics.Debug.Assert(CurrentScript == null);
			System.Diagnostics.Debug.Assert(exec.suspendedCount > 0);
			if (--exec.suspendedCount == 0)
			{
				CurrentScript = exec;
				RunLoop();
			}
		}

		private static void RunLoop()
		{
			while (CurrentScript.frames.Count > 0)
			{
				ScriptFrame frame = CurrentScript.frames.Peek();
				if (frame.ip < frame.actions.Length)
				{
					frame.actions[frame.ip++].Execute();
					// If the CurrentScript is null it has been Stopped (<stop /> action)
					if (CurrentScript == null) return;
					// Check if the script has been suspended
					if (CurrentScript.suspendedCount > 0)
					{
						CurrentScript = null;
						return;
					}
				}
				else
					CurrentScript.frames.Pop();
			}
			CurrentScript = null;
		}
	}

	internal class ScriptFrame
	{
		public int ip = 0;
		public Action[] actions;

		public ScriptFrame(Action[] actions)
		{ this.actions = actions; }
	}

	internal class ScriptExecution
	{
		public Dictionary<string, object> vars = new Dictionary<string, object>();
		public Stack<ScriptFrame> frames = new Stack<ScriptFrame>();
		public List<Card> thisCards;
		public Group thisGroup;
		public double posX, posY;
		public bool muted = false;
		public int suspendedCount = 0;
		private int uniqueId = 0;

		public void SetVariable(string name, object value)
		{ vars[name] = value; }

		public int GetUniqueId()
		{
			if (uniqueId == 0) uniqueId = ((int)Player.LocalPlayer.Id) << 16 | Program.Game.GetUniqueId();
			return uniqueId;
		}
	}
}
