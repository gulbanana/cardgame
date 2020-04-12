using System.Linq;
using System.Xml.Linq;

namespace Cardgame
{
    public abstract class TextModel
    {        
        public class Spans : TextModel
        {
            public TextModel[] Children { get; set; }
        }

        public class Lines : TextModel
        {
            public TextModel[] Children { get; set; }
        }

        public class Paras : TextModel
        {
            public TextModel[] Children { get; set; }
        }
        
        public class Block : TextModel
        {
            public TextModel Child { get; set; }
        }

        public class Run : TextModel
        {
            public string Text { get; set; }
        }

        public class Symbol : TextModel
        {
            public string Name { get; set; }
        }

        public class Card : TextModel
        {
            public string Name { get; set; }
        }

        public static TextModel Parse(string xml)
        {
            var node = XDocument.Parse(xml);
            return Parse(node.Root);
        }

        public static TextModel Parse(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "spans":
                    return new Spans { Children = element.Elements().Select(Parse).ToArray() };

                case "lines":
                    return new Lines { Children = element.Elements().Select(Parse).ToArray() };

                case "paras":
                    return new Paras { Children = element.Elements().Select(Parse).ToArray() };

                case "block":
                    return new Block { Child = Parse(element.Elements().Single()) };

                case "run":
                    return new Run { Text = element.Value };

                case "sym":
                    return new Symbol { Name = element.Value };

                case "card":
                    return new Card { Name = element.Value };

                default:
                    return new Run { Text = element.ToString() };
            }
        }
    }
}