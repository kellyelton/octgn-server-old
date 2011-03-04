using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Octgn.Data;
using Octgn.Play;
using System;

namespace Octgn.Script
{
  public abstract class CardFilter
  {
    public abstract List<Card> GetCards();

    internal static CardFilter LoadFromXml(XElement xml)
    {
      switch (xml.Name.LocalName)
      {
        case "all": return All.LoadFromXml(xml);
        case "controlled": return ControlledBy.LoadFromXml(xml);
        case "owned": return OwnedBy.LoadFromXml(xml);
        case "property": return PropertyFilter.LoadFromXml(xml);
        case "this": return ThisCard.LoadFromXml(xml);
        case "top": return Top.LoadFromXml(xml);
        case "bottom": return Bottom.LoadFromXml(xml);
        case "at": return At.LoadFromXml(xml);
        case "random": return Random.LoadFromXml(xml);
        case "orientation": return OrientationFilter.LoadFromXml(xml);
        case "faceup": return FaceUpFilter.LoadFromXml(xml, true);
        case "facedown": return FaceUpFilter.LoadFromXml(xml, false);
        case "highlighted": return HighlightedFilter.LoadFromXml(xml);
        default: throw new InvalidFileFormatException("Unknown filter tag: " + xml.Name.LocalName);
      }
    }
  }

  public class ThisCard : CardFilter
  {
    public override List<Card> GetCards()
    { return ScriptEngine.CurrentScript.thisCards; }

    internal new static ThisCard LoadFromXml(XElement xml)
    { return new ThisCard(); }
  }

  public class All : CardFilter
  {
    private string group;

    public override List<Card> GetCards()
    {
      Group g = ScriptEngine.GetGroup(group);
      return new List<Card>(g.Cards);
    }

    internal new static All LoadFromXml(XElement xml)
    {
      return new All { group = xml.Attr<string>("group") };
    }
  }

  public class Top : CardFilter
  {
    private string pile, count;

    public override List<Card> GetCards()
    {
      int n = ScriptEngine.GetLiteralOrVariable<int>(count); //count.StartsWith("$") ? ScriptEngine.GetVariable<int>(count) : int.Parse(count);
      var p = (Pile)ScriptEngine.GetGroup(pile);
      return p.Take(n).ToList();
    }

    internal new static Top LoadFromXml(XElement xml)
    {
      return new Top
      {
        pile = xml.Attr<string>("pile"),
        count = xml.Attr<string>("count", "1")
      };
    }
  }

  public class Bottom : CardFilter
  {
    private string pile, count;

    public override List<Card> GetCards()
    {
      int n = count.StartsWith("$") ? ScriptEngine.GetVariable<int>(count) : int.Parse(count);
      var p = (Pile)ScriptEngine.GetGroup(pile);
      // Using p.Cards, because LINQ can optimize Skip on a List, but not on an IEnumerable.
      return p.Cards.Skip(p.Count - n).Reverse().ToList();
    }

    internal new static Bottom LoadFromXml(XElement xml)
    {
      return new Bottom
      {
        pile = xml.Attr<string>("pile"),
        count = xml.Attr<string>("count", "1")
      };
    }
  }

  public class At : CardFilter
  {
    private string group, position;

    public override List<Card> GetCards()
    {
      Group g = ScriptEngine.GetGroup(group);
      int n = position.StartsWith("$") ? ScriptEngine.GetVariable<int>(position) : int.Parse(position);
      n -= 1; // Script position are 1-indexed			
      var result = new List<Card>(1);
      if (n >= 0 && n < g.Count)
        result.Add(g[n]);
      return result;
    }

    internal new static At LoadFromXml(XElement xml)
    {
      return new At
      {
        group = xml.Attr<string>("group"),
        position = xml.Attr<string>("position")
      };
    }
  }

  public class Random : CardFilter
  {
    private string group, count;

    public override List<Card> GetCards()
    {
      int n = count.StartsWith("$") ? ScriptEngine.GetVariable<int>(count) : int.Parse(count);
      if (n <= 0) return new List<Card>(0);
      Group g = ScriptEngine.GetGroup(group);
      if (n >= g.Count) return new List<Card>(g.Cards);

      var res = new List<Card>(n);
      int max = g.Count;
      for (int i = 0; i < n; i++)
      {
        int index = Crypto.Random(max);
        Card c;
        do
        {
          c = g[index++];
          if (index > max) index = 0;
        } while (res.Contains(c));
        res.Add(c);
      }
      return res;
    }

    internal new static Random LoadFromXml(XElement xml)
    {
      return new Random
      {
        group = xml.Attr<string>("group"),
        count = xml.Attr<string>("count", "1")
      };
    }
  }

  public class ControlledBy : CardFilter
  {
    private string by;
    private CardFilter cards;

    public override List<Card> GetCards()
    {
      Player p = ScriptEngine.GetPlayer(by);
      List<Card> result = cards.GetCards();
      result.RemoveAll(c => c.Controller != p);
      return result;
    }

    internal new static ControlledBy LoadFromXml(XElement xml)
    {
      return new ControlledBy
      {
        by = xml.Attr<string>("by"),
        cards = CardFilter.LoadFromXml(xml.Elements().First())
      };
    }
  }

  public class OwnedBy : CardFilter
  {
    private string by;
    private CardFilter cards;

    public override List<Card> GetCards()
    {
      Player p = ScriptEngine.GetPlayer(by);
      List<Card> result = cards.GetCards();
      result.RemoveAll(c => c.Owner != p);
      return result;
    }

    internal new static OwnedBy LoadFromXml(XElement xml)
    {
      return new OwnedBy
      {
        by = xml.Attr<string>("by"),
        cards = CardFilter.LoadFromXml(xml.Elements().First())
      };
    }
  }

  public class PropertyFilter : CardFilter
  {
    public string Name { get; internal set; }
    public string Value { get; internal set; }
    public bool Negate { get; private set; }
    private CardFilter cards;

    public override List<Card> GetCards()
    {
      var result = cards.GetCards();
      for (int i = result.Count - 1; i >= 0; i--)
      {
        object actualValue = result[i].GetProperty(Name);
        if (actualValue == null)
          result.RemoveAt(i);
        else
        {
          bool comparison = string.Compare(actualValue.ToString(), Value, true) == 0;
          if (comparison == Negate)
            result.RemoveAt(i);
        }
      }
      return result;
    }

    internal new static PropertyFilter LoadFromXml(XElement xml)
    {
      // Sometimes this filter is used without an explicit source (e.g. in <newcard />)
      XElement child = xml.Elements().FirstOrDefault();
      return new PropertyFilter
      {
        Name = xml.Attr<string>("name"),
        Negate = xml.Attr<bool>("negate"),
        Value = xml.Attr<string>("value"),
        cards = child == null ? null : CardFilter.LoadFromXml(child)
      };
    }
  }

  public class OrientationFilter : CardFilter
  {
    private CardOrientation orientation;
    private bool exact, negate;
    private CardFilter cards;

    public override List<Card> GetCards()
    {
      var result = cards.GetCards();
      if (negate)
        result.RemoveAll(c => exact ?
          c.Orientation == orientation :
          (c.Orientation & orientation) != 0);
      else
        result.RemoveAll(c => exact ?
           c.Orientation != orientation :
           (c.Orientation & orientation) == 0);
      return result;
    }

    internal static new OrientationFilter LoadFromXml(XElement xml)
    {
      string rot = xml.Attr<string>("rot");
      CardOrientation orientation;
      switch (rot)
      {
        case "0": orientation = CardOrientation.Rot0; break;
        case "90": orientation = CardOrientation.Rot90; break;
        case "180": orientation = CardOrientation.Rot180; break;
        case "270": orientation = CardOrientation.Rot270; break;
        default:
          throw new Data.InvalidFileFormatException("Unknown rotation: " + rot);
      }
      return new OrientationFilter
      {
        orientation = orientation,
        exact = xml.Attr<bool>("exact"),
        negate = xml.Attr<bool>("negate"),
        cards = CardFilter.LoadFromXml(xml.Elements().First())
      };
    }
  }

  public class FaceUpFilter : CardFilter
  {
    private bool faceUp;
    private CardFilter cards;

    public override List<Card> GetCards()
    {
      var result = cards.GetCards();
      result.RemoveAll(c => c.FaceUp != faceUp);
      return result;
    }

    internal static FaceUpFilter LoadFromXml(XElement xml, bool keepFaceUp)
    {
      return new FaceUpFilter
      {
        faceUp = keepFaceUp,
        cards = CardFilter.LoadFromXml(xml.Elements().First())
      };
    }
  }

  public class HighlightedFilter : CardFilter
  {
    private string color;
    private CardFilter cards;

    public override List<Card> GetCards()
    {
      List<Card> result = cards.GetCards();
      if (color == "any")
        result.RemoveAll(c => !c.IsHighlighted);
      else
      {
        var parsedColor = color == "none" ?
           null :
           (System.Windows.Media.Color?)System.Windows.Media.ColorConverter.ConvertFromString(color);
        result.RemoveAll(c => c.HighlightColor != parsedColor);
      }
      return result;
    }

    internal new static HighlightedFilter LoadFromXml(XElement xml)
    {
      return new HighlightedFilter
      {
        color = xml.Attr<string>("color", "any"),
        cards = CardFilter.LoadFromXml(xml.Elements().First())
      };
    }
  }
}