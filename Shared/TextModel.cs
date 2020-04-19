using System;
using System.Linq;
using System.Xml.Linq;

namespace Cardgame.Widgets
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

        public class Split : TextModel
        {
            public TextModel[] Children { get; set; }
        }
        
        public class Bold : TextModel
        {
            public TextModel Child { get; set; }
        }

        public class Small : TextModel
        {
            public TextModel Child { get; set; }
        }

        public class Run : TextModel
        {
            public string Text { get; set; }
        }

        public class Error : TextModel
        {
            public TextModel Child { get; set; }
        }

        public class Indent : TextModel
        {
            public int Level { get; set; }
        }

        public class Symbol : TextModel
        {
            public string Name { get; set; }
            public string Prefix { get; set; }
            public string Suffix { get; set; }
            public bool IsLarge { get; set; }
        }

        public class Card : TextModel
        {
            public string Name { get; set; }
            public string Prefix { get; set; }
            public string Suffix { get; set; }
        }

        public class Player : TextModel
        {
            public string Name { get; set; }
            public string Prefix { get; set; }
            public string Suffix { get; set; }
        }

        public class Pronominal : TextModel
        {
            public string Name { get; set; }
            public string IfYou { get; set; }
            public string IfThem { get; set; }
        }

        public static TextModel Parse(string xml)
        {
            try
            {
                var rooted = $"<root>{xml}</root>";
                var node = XDocument.Parse(rooted);
                return ParseContents(node.Root);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new Run { Text = xml };
            }
        }

        private static TextModel ParseElement(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "spans":
                    return new Spans { Children = element.Elements().Select(ParseElement).ToArray() };

                case "lines":
                    return new Lines { Children = element.Elements().Select(ParseElement).ToArray() };

                case "paras":
                    return new Paras { Children = element.Elements().Select(ParseElement).ToArray() };

                case "split":
                    return new Split { Children = element.Elements().Select(ParseElement).ToArray() };

                case "bold":
                    return new Bold { Child = ParseContents(element) };

                case "small":
                    return new Small { Child = ParseContents(element) };

                case "error":
                    return new Error { Child = ParseContents(element) };

                case "run":
                    return new Run { Text = element.Value };

                case "indent":
                    return new Indent { Level = int.Parse(element.Attribute("level").Value) } ;

                case "sym":
                    return new Symbol 
                    { 
                        Name = element.Value,
                        Prefix = element.Attribute("prefix")?.Value,
                        Suffix = element.Attribute("suffix")?.Value,
                        IsLarge = element.Attribute("large")?.Value == "true"
                    };

                case "card":
                    return new Card 
                    { 
                        Name = element.Value,
                        Prefix = element.Attribute("prefix")?.Value,
                        Suffix = element.Attribute("suffix")?.Value
                    };

                case "player":
                    return new Player 
                    { 
                        Name = element.Value,
                        Prefix = element.Attribute("prefix")?.Value,
                        Suffix = element.Attribute("suffix")?.Value
                    };

                case "if":
                    return new Pronominal 
                    { 
                        Name = element.Value, 
                        IfYou = element.Attribute("you").Value, 
                        IfThem = element.Attribute("them").Value 
                    };

                default:
                    return new Run { Text = element.ToString() };
            }
        }

        private static TextModel ParseContents(XElement element)
        {
            var children = element.Elements().Count();
            if (children == 0)
            {
                return new Run { Text = element.Value };
                
            }
            else if (children == 1)
            {
                return ParseElement(element.Elements().Single());
            }
            else
            {
                return new Spans { Children = element.Elements().Select(ParseElement).ToArray() };
            }
        }
    }
}