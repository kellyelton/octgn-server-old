using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;
using Octgn.Play;

namespace Octgn.Script
{
   public abstract class Condition
   {
      public abstract bool Evaluate();

      internal static Condition LoadFromXml(XElement xml)
      {
         switch (xml.Name.LocalName)
         {
            case "and": return And.LoadFromXml(xml);
            case "or": return Or.LoadFromXml(xml);
            case "not": return Not.LoadFromXml(xml);
            case "lt": return Lt.LoadFromXml(xml);
            case "eq": return Eq.LoadFromXml(xml);
            case "isRot90": return IsRot90.LoadFromXml(xml);
            case "isRot180": return IsRot180.LoadFromXml(xml);
            case "isFaceDown": return IsFaceDown.LoadFromXml(xml);
            case "confirm": return Confirm.LoadFromXml(xml);
            default: throw new Data.InvalidFileFormatException("Unknown condition tag: " + xml.Name.LocalName);
         }
      }
   }

   public class And : Condition
   {
      private Condition[] conditions;

      public override bool Evaluate()
      {
         foreach (Condition c in conditions)
            if (!c.Evaluate()) return false;
         return true;
      }

      internal new static And LoadFromXml(XElement xml)
      {
         return new And
         {
            conditions = xml.Elements()
                            .Select(x => Condition.LoadFromXml(x))
                            .ToArray()
         };
      }
   }

   public class Not : Condition
   {
      private Condition cond;

      public override bool Evaluate()
      {
         return !cond.Evaluate();
      }

      internal new static Not LoadFromXml(XElement xml)
      {
         return new Not
         {
            cond = Condition.LoadFromXml(xml.Elements().First())
         };
      }
   }

   public class Or : Condition
   {      
      private Condition[] conditions;

      public override bool Evaluate()
      {
         foreach (Condition c in conditions)
            if (c.Evaluate()) return true;
         return false;
      }

      internal new static Or LoadFromXml(XElement xml)
      {
         return new Or
         {
            conditions = xml.Elements()
                            .Select(x => Condition.LoadFromXml(x))
                            .ToArray()
         };
      }
   }

   public class Lt : Condition
   {
      private string left, right, type;

      public override bool Evaluate()
      {
         if (type == "int")
         {
            int l, r;
            if (left.StartsWith("$")) l = ScriptEngine.GetVariable<int>(left);
            else l = int.Parse(left);
            if (right.StartsWith("$")) r = ScriptEngine.GetVariable<int>(right);
            else r = int.Parse(right);
            return l < r;
         }
         else
            throw new BadScriptException("Unknown type: " + type);
      }

      internal new static Lt LoadFromXml(XElement xml)
      {
         return new Lt
         {
            left = xml.Attr<string>("left"),
            right = xml.Attr<string>("right"),
            type = xml.Attr<string>("type", "int")
         };
      }
   }

   public class Eq : Condition
   {
      private string left, right, type;

      public override bool Evaluate()
      {
         if (type == "int")
         {
            int l, r;
            if (left.StartsWith("$")) l = ScriptEngine.GetVariable<int>(left);
            else l = int.Parse(left);
            if (right.StartsWith("$")) r = ScriptEngine.GetVariable<int>(right);
            else r = int.Parse(right);
            return l == r;
         }
         else
            throw new BadScriptException("Unknown type: " + type);
      }

      internal new static Eq LoadFromXml(XElement xml)
      {
         return new Eq
         {
            left = xml.Attr<string>("left"),
            right = xml.Attr<string>("right"),
            type = xml.Attr<string>("type", "int")
         };
      }
   }

   public abstract class CardsCondition : Condition
   {
      protected CardFilter cardsFilter;

      public override bool Evaluate()
      {
         List<Card> cards = cardsFilter.GetCards();
         return EvaluateOnCards(cards);
      }

      protected abstract bool EvaluateOnCards(List<Card> cards);

      protected void LoadBaseXml(XElement xml)
      {
         cardsFilter = CardFilter.LoadFromXml(xml.Elements().First());
      }
   }

   public class IsRot90 : CardsCondition
   {
      protected override bool EvaluateOnCards(List<Card> cards)
      {
         foreach (Card card in cards)
            if ((card.Orientation & CardOrientation.Rot90) == 0) return false;
         return true;
      }

      internal new static IsRot90 LoadFromXml(XElement xml)
      {
         var res = new IsRot90();
         res.LoadBaseXml(xml);
         return res;
      }
   }

   public class IsRot180 : CardsCondition
   {
      protected override bool EvaluateOnCards(List<Card> cards)
      {
         foreach (Card card in cards)
            if ((card.Orientation & CardOrientation.Rot180) == 0) return false;
         return true;
      }

      internal new static IsRot180 LoadFromXml(XElement xml)
      {
         var res = new IsRot180();
         res.LoadBaseXml(xml);
         return res;
      }
   }

   public class IsFaceDown : CardsCondition
   {
      protected override bool EvaluateOnCards(List<Card> cards)
      {
         foreach (Card card in cards)
            if (card.FaceUp) return false;
         return true;
      }

      internal new static IsFaceDown LoadFromXml(XElement xml)
      {
         var res = new IsFaceDown();
         res.LoadBaseXml(xml);
         return res;
      }
   }

   public class Confirm : Condition
   {
      private string text;

      public override bool Evaluate()
      {
         string prompt =
             System.Text.RegularExpressions.Regex.Replace(text, @"{(\$.*?)}",
             delegate(System.Text.RegularExpressions.Match match)
             {
                return ScriptEngine.GetVariable(match.Value.Substring(1, match.Length - 2)).ToString();
             });
         return OCTGN.Confirm(prompt);
      }

      internal new static Confirm LoadFromXml(XElement xml)
      {
         return new Confirm { text = xml.Attr<string>("text") };
      }
   }
}