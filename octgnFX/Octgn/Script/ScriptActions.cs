using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;
using Octgn.Play;
using RE = System.Text.RegularExpressions;

namespace Octgn.Script
{
  public abstract class Action
  {
    public abstract void Execute();

    internal static Action LoadFromXml(XElement x)
    {
      switch (x.Name.LocalName)
      {
        case "if": return If.LoadFromXml(x);
        case "while": return While.LoadFromXml(x);
        case "count": return Count.LoadFromXml(x);
        case "set": return Set.LoadFromXml(x);
        case "dec": return Dec.LoadFromXml(x);
        case "inc": return Inc.LoadFromXml(x);
        case "rnd": return Rnd.LoadFromXml(x);
        case "move": return Move.LoadFromXml(x);
        case "setrot": return SetRot.LoadFromXml(x);
        case "setrot90": return SetRot90.LoadFromXml(x);
        case "setrot180": return SetRot180.LoadFromXml(x);
        case "togglerot90": return ToggleRot90.LoadFromXml(x);
        case "togglerot180": return ToggleRot180.LoadFromXml(x);
        case "turn": return Turn.LoadFromXml(x);
        case "highlight": return Highlight.LoadFromXml(x);
        case "target": return Target.LoadFromXml(x);
        case "shuffle": return Shuffle.LoadFromXml(x);
        case "stop": return Stop.LoadFromXml(x);
        case "mute": return Mute.LoadFromXml(x);
        case "print": return Print.LoadFromXml(x);
        case "addmarker": return AddMarker.LoadFromXml(x);
        case "removemarker": return RemoveMarker.LoadFromXml(x);
        case "setcounter": return SetCounter.LoadFromXml(x);
        case "getcounter": return GetCounter.LoadFromXml(x);
        case "question": return Question.LoadFromXml(x);
        case "newcard": return CreateCard.LoadFromXml(x);
        default:
          throw new Data.InvalidFileFormatException("Unknown action tag: " + x.Name.LocalName);
      }
    }
  }

  public class Count : Action
  {
    private string group, var;
    private CardFilter cardsFilter;

    public override void Execute()
    {
      if (group != null)
        ScriptEngine.SetVariable(var, ScriptEngine.GetGroup(group).Count);
      else if (cardsFilter != null)
        ScriptEngine.SetVariable(var, cardsFilter.GetCards().Count);
      else
        throw new Data.InvalidFileFormatException("Count action must have either have a group attribute or a card filter inside.");
    }

    internal new static Count LoadFromXml(XElement xml)
    {      
      return new Count
      {
        group = xml.Attr<string>("group"),
        var = xml.Attr<string>("var"),
        cardsFilter = xml.HasElements ? CardFilter.LoadFromXml(xml.Elements().First()) : null
      };
    }
  }

  public class Set : Action
  {
    private string var;
    private int value;

    public override void Execute()
    {
      ScriptEngine.SetVariable(var, value);
    }

    internal new static Set LoadFromXml(XElement xml)
    {
      return new Set
      {
        var = xml.Attr<string>("var"),
        value = xml.Attr<int>("value")
      };
    }
  }

  public class Dec : Action
  {
    private string variable, amountStr;

    public override void Execute()
    {
      int val = ScriptEngine.GetVariable<int>(variable);
      int amount = ScriptEngine.GetLiteralOrVariable<int>(amountStr);
      val -= amount;
      ScriptEngine.SetVariable(variable, val);
    }

    internal new static Dec LoadFromXml(XElement xml)
    {
      return new Dec
      {
        variable = xml.Attr<string>("var"),
        amountStr = xml.Attr<string>("amount", "1")
      };
    }
  }

  public class Rnd : Action
  {
    private string var, min, max;
    private int reqId;
    private ScriptExecution script;

    public override void Execute()
    {
      reqId = RandomRequest.GenerateId();
      int min = this.min.StartsWith("$") ? ScriptEngine.GetVariable<int>(this.min) : int.Parse(this.min);
      int max = this.max.StartsWith("$") ? ScriptEngine.GetVariable<int>(this.max) : int.Parse(this.max);
      RandomRequest.Completed += Continue;
      Program.Client.Rpc.RandomReq(reqId, min, max);
      script = Script.ScriptEngine.Suspend();
    }

    private void Continue(object sender, EventArgs e)
    {
      var req = (RandomRequest)sender;
      if (req.Id != reqId) return;
      RandomRequest.Completed -= Continue;

      if (!string.IsNullOrEmpty(var))
        ScriptEngine.SetVariable(var, req.Result, script);

      Script.ScriptEngine.Continue(script);
    }

    internal new static Rnd LoadFromXml(XElement xml)
    {
      return new Rnd
      {
        var = xml.Attr<string>("var"),
        min = xml.Attr<string>("min"),
        max = xml.Attr<string>("max")
      };
    }
  }

  public class Inc : Action
  {
    private string variable, amountString;

    public override void Execute()
    {
      int val = ScriptEngine.GetVariable<int>(variable);
      int amount = ScriptEngine.GetLiteralOrVariable<int>(amountString);
      val += amount;
      ScriptEngine.SetVariable(variable, val);
    }

    internal new static Inc LoadFromXml(XElement xml)
    {
      return new Inc
      {
        variable = xml.Attr<string>("var"),
        amountString = xml.Attr<string>("amount", "1")
      };
    }
  }

  public class If : Action
  {
    private Condition cond;
    private Script.Action[] thenActions, elseActions;

    public override void Execute()
    {
      if (cond.Evaluate())
        ScriptEngine.PushFrame(thenActions);
      else
        ScriptEngine.PushFrame(elseActions);
    }

    internal new static If LoadFromXml(XElement xml)
    {
      XElement thenChild = xml.Child("then");
      XElement elseChild = xml.Child("else");
      return new If
      {
        cond = Condition.LoadFromXml(xml.Elements().First()),
        thenActions = thenChild == null ? null : thenChild.Elements()
                                                          .Select(x => Action.LoadFromXml(x))
                                                          .ToArray(),
        elseActions = elseChild == null ? null : elseChild.Elements()
                                                          .Select(x => Action.LoadFromXml(x))
                                                          .ToArray()
      };
    }
  }

  public class While : Action
  {
    private Condition cond;
    private Script.Action[] doActions;

    public override void Execute()
    {
      if (cond.Evaluate())
      {
        ScriptEngine.PushFrame(new Action[] { this });
        ScriptEngine.PushFrame(doActions);
      }
    }

    internal new static While LoadFromXml(XElement xml)
    {
      return new While
      {
        cond = Condition.LoadFromXml(xml.Elements().First()),
        doActions = xml.Child("do").Elements()
                                   .Select(x => Action.LoadFromXml(x))
                                   .ToArray()
      };
    }
  }

  public class Stop : Action
  {
    public override void Execute()
    {
      ScriptEngine.Stop();
    }

    internal new static Stop LoadFromXml(XElement xml)
    { return new Stop(); }
  }

  public class Mute : Action
  {
    private bool muted = true;

    public override void Execute()
    {
      ScriptEngine.Mute(muted);
    }

    internal new static Mute LoadFromXml(XElement xml)
    {
      return new Mute { muted = xml.Attr<bool>("muted", true) };
    }
  }

  public class Print : Action
  {
    private string text;

    public override void Execute()
    {
      // Process special tokens:
      // {me} is passed through (handled by receiving client)
      // {$v} is a script variable value
      string sentText = RE.Regex.Replace(text, @"{(\$[^}]*)}", match => ScriptEngine.GetVariable(match.Groups[1].Value).ToString());
      // {this} may be a group or a card
      if (sentText.Contains("{this}"))
      {
        if (ScriptEngine.CurrentScript.thisGroup != null)
          sentText = sentText.Replace("{this}", ScriptEngine.CurrentScript.thisGroup.Id.ToString("'{#'0}"));
        else
        {
          // Cards are special, one message is sent for each card in the selection
          foreach (Card c in ScriptEngine.CurrentScript.thisCards)
          {
            string cardText = sentText.Replace("{this}", c.Id.ToString("\"'{#\"0\"}'\""));
            Program.Client.Rpc.PrintReq(cardText);
          }
          return;
        }
      }
      Program.Client.Rpc.PrintReq(sentText);
    }

    internal new static Print LoadFromXml(XElement xml)
    {
      return new Print { text = xml.Attr<string>("text") };
    }
  }

  public class Shuffle : Action
  {
    private string group;

    public override void Execute()
    {
      var pile = (Pile)ScriptEngine.GetGroup(group);
      bool isAsync = pile.Shuffle();
      if (isAsync)
      {
        pile.Shuffled += (new ShuffleCompletion { pile = pile, scriptState = Script.ScriptEngine.Suspend() }).Continuation;
      }
    }

    internal new static Shuffle LoadFromXml(XElement xml)
    {
      return new Shuffle { group = xml.Attr<string>("group") };
    }

    private class ShuffleCompletion
    {
      public Pile pile;
      public Script.ScriptExecution scriptState;

      public void Continuation(object sender, EventArgs e)
      {
        pile.Shuffled -= Continuation;
        Script.ScriptEngine.Continue(scriptState);
      }
    }
  }

  public class CreateCard : Action
  {
    private PropertyFilter propertyFilter;
    private bool clone;
    private bool persist;

    public override void Execute()
    {
      if (clone)
        Clone(ScriptEngine.CurrentScript.thisCards);
      else
        CreateNewCards();
    }

    private void Clone(List<Card> cards)
    {
      IEnumerable<CardModel> cardModels = cards.Select(c => c.Type.model).Where(m => m != null);
      int qty = cardModels.Count();
      int[] ids = new int[qty];
      ulong[] keys = new ulong[qty];
      Guid[] models = new Guid[qty];
      int[] xs = new int[qty], ys = new int[qty];

      var def = Program.Game.Definition.CardDefinition;
      var table = ScriptEngine.CurrentScript.thisGroup;

      int x = (int)ScriptEngine.CurrentScript.posX, y = (int)ScriptEngine.CurrentScript.posY;
      if (Player.LocalPlayer.InvertedTable)
      {
        x -= def.Width; y -= def.Height;
      }
      int offset = (int)(Math.Min(def.Width, def.Height) * 0.2);
      if (Program.GameSettings.UseTwoSidedTable && Play.Gui.TableControl.IsInInvertedZone(y))
        offset = -offset;

      int i = 0;
      foreach (var model in cardModels)
      {
        ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | model.Id.Condense();
        int id = Program.Game.GenerateCardId();

        new Play.Actions.CreateCard(Player.LocalPlayer, id, key, true, model, x, y, !persist).Do();

        x += offset; y += offset;
        ids[i] = id; keys[i] = key; models[i] = model.Id; xs[i] = x; ys[i] = y;
        ++i;
      }

      Program.Client.Rpc.CreateCardAt(ids, keys, models, xs, ys, true, persist);
    }

    private void CreateNewCards()
    {
      CardDlg dlg = new CardDlg(propertyFilter);
      dlg.Owner = System.Windows.Application.Current.MainWindow;
      if (dlg.ShowDialog().GetValueOrDefault())
      {
        int qty = dlg.Quantity;
        int[] ids = new int[qty];
        ulong[] keys = new ulong[qty];
        Guid[] models = new Guid[qty];
        int[] xs = new int[qty], ys = new int[qty];

        var def = Program.Game.Definition.CardDefinition;
        var table = ScriptEngine.CurrentScript.thisGroup;

        int x = (int)ScriptEngine.CurrentScript.posX, y = (int)ScriptEngine.CurrentScript.posY;
        if (Player.LocalPlayer.InvertedTable)
        {
          x -= def.Width; y -= def.Height;
        }
        int offset = (int)(Math.Min(def.Width, def.Height) * 0.2);
        if (Program.GameSettings.UseTwoSidedTable && Play.Gui.TableControl.IsInInvertedZone(y))
          offset = -offset;

        for (int i = 0; i < qty; ++i)
        {
          ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | dlg.SelectedCard.Id.Condense();
          int id = Program.Game.GenerateCardId();

          new Play.Actions.CreateCard(Player.LocalPlayer, id, key, true, dlg.SelectedCard, x, y, !persist).Do();

          x += offset; y += offset;
          ids[i] = id; keys[i] = key; models[i] = dlg.SelectedCard.Id; xs[i] = x; ys[i] = y;
        }

        Program.Client.Rpc.CreateCardAt(ids, keys, models, xs, ys, true, persist);
      }
    }

    internal new static CreateCard LoadFromXml(XElement xml)
    {
      var propertyXml = xml.Elements().FirstOrDefault(x => x.Name == "property");
      return new CreateCard
      {
        persist = xml.Attr<bool>("persist"),
        clone = xml.Elements().Single().Name == "this",
        propertyFilter = propertyXml != null ? PropertyFilter.LoadFromXml(propertyXml) : null
      };
    }
  }

  public class SetCounter : Action
  {
    private int counterIndex;
    private string value;

    public override void Execute()
    {
      Player.LocalPlayer.Counters[counterIndex].Value = ScriptEngine.GetLiteralOrVariable<int>(value);
    }

    internal new static SetCounter LoadFromXml(XElement xml)
    {
      string counterName = xml.Attr<string>("counter");
      int idx = Program.Game == null ? 0 : // This is null when installing the game, but at that time the scripts don't matter at all...
           Program.Game.Definition.PlayerDefinition.Counters
                  .Where(c => c.Name == counterName)
                  .Select((c, i) => i)
                  .Single();
      return new SetCounter
      {
        counterIndex = idx,
        value = xml.Attr<string>("value")
      };
    }
  }

  public class GetCounter : Action
  {
    private int counterIndex;
    private string variable;

    public override void Execute()
    {
      ScriptEngine.SetVariable(variable, Player.LocalPlayer.Counters[counterIndex].Value);
    }

    internal new static GetCounter LoadFromXml(XElement xml)
    {
      string counterName = xml.Attr<string>("counter");
      int idx = Program.Game == null ? 0 : // This is null when installing the game, but at that time the scripts don't matter at all...
           Program.Game.Definition.PlayerDefinition.Counters
                  .Where(c => c.Name == counterName)
                  .Select((c, i) => i)
                  .Single();
      return new GetCounter
      {
        counterIndex = idx,
        variable = xml.Attr<string>("var")
      };
    }
  }

  public abstract class CardsAction : Action
  {
    protected CardFilter cardsFilter;

    public override void Execute()
    {
      List<Card> cards = cardsFilter.GetCards();
      ExecuteOnCards(cards);
    }

    protected abstract void ExecuteOnCards(List<Card> cards);

    protected void LoadBaseFromXml(XElement xml)
    {
      cardsFilter = CardFilter.LoadFromXml(xml.Elements().First());
    }
  }

  public class Move : CardsAction
  {
    private string to;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      Group destination = ScriptEngine.GetGroup(to);
      foreach (Card c in cards)
        c.MoveTo(destination, true);
    }

    internal new static Move LoadFromXml(XElement xml)
    {
      var res = new Move { to = xml.Attr<string>("to") };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class SetRot90 : CardsAction
  {
    private string orientation;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      if (orientation == "0")
        foreach (Card c in cards)
          c.Orientation &= ~CardOrientation.Rot90;
      else if (orientation == "90")
        foreach (Card c in cards)
          c.Orientation |= CardOrientation.Rot90;
      else
        throw new BadScriptException("Invalid argument for orientation: " + orientation);
    }

    internal new static SetRot90 LoadFromXml(XElement xml)
    {
      var res = new SetRot90 { orientation = xml.Attr<string>("orientation") };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class SetRot180 : CardsAction
  {
    private string orientation;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      if (orientation == "0")
        foreach (Card c in cards)
          c.Orientation &= ~CardOrientation.Rot180;
      else if (orientation == "180")
        foreach (Card c in cards)
          c.Orientation |= CardOrientation.Rot180;
      else
        throw new BadScriptException("Invalid argument for orientation: " + orientation);
    }

    internal new static SetRot180 LoadFromXml(XElement xml)
    {
      var res = new SetRot180 { orientation = xml.Attr<string>("orientation") };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class SetRot : CardsAction
  {
    private string orientation;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      CardOrientation newOrientation;
      switch (orientation)
      {
        case "0":
          newOrientation = CardOrientation.Rot0;
          break;
        case "90":
          newOrientation = CardOrientation.Rot90;
          break;
        case "180":
          newOrientation = CardOrientation.Rot180;
          break;
        case "270":
          newOrientation = CardOrientation.Rot270;
          break;
        default:
          throw new BadScriptException("Invalid argument for orientation: " + orientation);
      }
      foreach (Card c in cards)
        c.Orientation = newOrientation;
    }

    internal new static SetRot LoadFromXml(XElement xml)
    {
      var res = new SetRot { orientation = xml.Attr<string>("orientation") };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class ToggleRot90 : CardsAction
  {
    protected override void ExecuteOnCards(List<Card> cards)
    {
      foreach (Card c in cards)
        c.Orientation ^= CardOrientation.Rot90;
    }

    internal new static ToggleRot90 LoadFromXml(XElement xml)
    {
      var res = new ToggleRot90();
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class ToggleRot180 : CardsAction
  {
    protected override void ExecuteOnCards(List<Card> cards)
    {
      foreach (Card c in cards)
        c.Orientation ^= CardOrientation.Rot180;
    }

    internal new static ToggleRot180 LoadFromXml(XElement xml)
    {
      var res = new ToggleRot180();
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class Turn : CardsAction
  {
    private string face;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      switch (face)
      {
        case "up":
          foreach (Card c in cards)
            c.FaceUp = true;
          return;
        case "down":
          foreach (Card c in cards)
            c.FaceUp = false;
          return;
        default:
          foreach (Card c in cards)
            c.FaceUp = !c.FaceUp;
          return;
      }
    }

    internal new static Turn LoadFromXml(XElement xml)
    {
      var res = new Turn { face = xml.Attr<string>("face") };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class AddMarker : CardsAction
  {
    private Guid? markerId;
    private string markerName;
    private string quantity;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      MarkerModel model;
      ushort count;

      if (markerId.HasValue)
      {
        model = Program.Game.GetMarkerModel(markerId.Value);
        var defaultMarkerModel = model as DefaultMarkerModel;
        if (defaultMarkerModel != null) defaultMarkerModel.SetName(markerName);
        count = (ushort)ScriptEngine.GetLiteralOrVariable<int>(quantity);
      }
      else
      {
        MarkerDlg dlg = new MarkerDlg();
        dlg.Owner = System.Windows.Application.Current.MainWindow;
        if (!dlg.ShowDialog().GetValueOrDefault()) return;
        model = dlg.MarkerModel;
        count = (ushort)dlg.Dlg.Quantity;
      }

      foreach (Card card in cards)
      {
        Program.Client.Rpc.AddMarkerReq(card, model.id, model.Name, count);
        card.AddMarker(model, count);
      }
    }

    internal new static AddMarker LoadFromXml(XElement xml)
    {
      var res = new AddMarker
      {
        markerId = xml.Attr<Guid?>("id", null),
        markerName = xml.Attr<string>("name"),
        quantity = xml.Attr<string>("qty", "1")
      };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class RemoveMarker : CardsAction
  {
    private Guid markerId;
    private string markerName;
    private string quantity;
    private string outVar;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      MarkerModel model = Program.Game.GetMarkerModel(markerId);
      var defaultModel = model as DefaultMarkerModel;
      if (defaultModel != null) defaultModel.SetName(markerName);
      int removed = 0;
      foreach (Card card in cards)
      {        
        var marker = card.FindMarker(model.id, model.Name);
        if (marker == null) continue;
        ushort count = (ushort)ScriptEngine.GetLiteralOrVariable<int>(quantity);
        removed += card.RemoveMarker(marker, count);
        Program.Client.Rpc.RemoveMarkerReq(card, model.id, model.Name, count);
      }
      if (outVar != null) ScriptEngine.SetVariable(outVar, removed);
    }

    internal new static RemoveMarker LoadFromXml(XElement xml)
    {
      var res = new RemoveMarker
      {
        markerId = xml.Attr<Guid>("id"),
        markerName = xml.Attr<string>("name"),
        quantity = xml.Attr<string>("qty", "1"),
        outVar = xml.Attr<string>("outVar")
      };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class Highlight : CardsAction
  {
    public string color;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      System.Windows.Media.Color? parsedColor;
      parsedColor = color == "none" ?
        null :
        (System.Windows.Media.Color?)System.Windows.Media.ColorConverter.ConvertFromString(color);
      foreach (Card c in cards) c.HighlightColor = parsedColor;
    }

    internal new static Highlight LoadFromXml(XElement xml)
    {
      var res = new Highlight { color = xml.Attr<string>("color") };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class Target : CardsAction
  {
    private bool active = true;

    protected override void ExecuteOnCards(List<Card> cards)
    {
      if (active)
        foreach (Card c in cards) c.Target();
      else
        foreach (Card c in cards) c.Untarget();
    }

    internal new static Target LoadFromXml(XElement xml)
    {
      var res = new Target { active = xml.Attr<bool>("active", true) };
      res.LoadBaseFromXml(xml);
      return res;
    }
  }

  public class Question : Action
  {
    private string answer, text, def, var;

    public override void Execute()
    {
      if (answer == "positiveInt")
        ScriptEngine.SetVariable(var, OCTGN.InputPositiveInt("Question", text, int.Parse(def)));
    }

    internal new static Question LoadFromXml(XElement xml)
    {
      return new Question
      {
        answer = xml.Attr<string>("answer"),
        text = xml.Attr<string>("text"),
        def = xml.Attr<string>("default"),
        var = xml.Attr<string>("var")
      };
    }
  }
}