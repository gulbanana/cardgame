using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Cardgame.API;
using Cardgame.Model;
using Cardgame.Model.ClientServer;
using Microsoft.AspNetCore.Components.Rendering;

namespace Cardgame.UI.Widgets
{
    public class RichText : ComponentBase
    {
        [Inject] private IUserSession Session { get; set; }
        [Parameter] public string Model { get; set; }
        [Parameter] public TextModel Parsed { get; set; }

        protected override void OnParametersSet()
        {
            if (Parsed == null)
            {
                Parsed = TextModel.Parse(Model);
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            RenderTextNode(builder, Parsed, 0);
        }

        private int RenderTextNode(RenderTreeBuilder builder, TextModel node, int seq)
        {
            switch (node)
            {
                case TextModel.Spans spans:
                    foreach (var span in spans.Children)
                    {
                        seq = RenderTextNode(builder, span, seq);
                    }
                    break;

                case TextModel.Lines lines:
                    for (var i = 0; i < lines.Children.Length; i++)
                    {
                        seq = RenderTextNode(builder, lines.Children[i], seq);
                        if (i < lines.Children.Length - 1)
                        {
                            builder.AddMarkupContent(seq++, "<br>");
                        }
                    }
                    break;

                case TextModel.Paras paras:
                    builder.OpenElement(seq++, "div");
                        builder.AddAttribute(seq++, "class", "rich-text__paras");
                        foreach (var para in paras.Children)
                        {
                            builder.OpenElement(seq++, "p");
                                seq = RenderTextNode(builder, para, seq);                        
                            builder.CloseElement();
                        }
                    builder.CloseElement();
                    break;

                case TextModel.Split split:
                    for (var i = 0; i < split.Children.Length; i++)
                    {
                        seq = RenderTextNode(builder, split.Children[i], seq);

                        if (i < split.Children.Length - 1)
                        {
                            var compactClass = split.IsCompact ? " rich-text__bar--compact" : "";
                            builder.AddMarkupContent(seq++, $"<div class=\"rich-text__bar{compactClass}\" />");                            
                        }
                    }
                    break;

                case TextModel.Bold bold:
                    builder.AddContent(seq++, "        ");
                    builder.OpenElement(seq++, "span");
                    builder.AddAttribute(seq++, "class", "rich-text__bold");
                    builder.AddMarkupContent(seq++, "\r\n            ");
                    builder.OpenComponent<RichText>(seq++);
                    builder.AddAttribute(seq++, "Parsed", (
                        bold.Child
                    ));
                    builder.CloseComponent();
                    builder.AddMarkupContent(seq++, "\r\n        ");
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "\r\n");

                    break;

                case TextModel.Small small:
                    builder.AddContent(seq++, "        ");
                    builder.OpenElement(seq++, "span");
                    builder.AddAttribute(seq++, "class", "rich-text__small");
                    builder.AddMarkupContent(seq++, "\r\n            ");
                    builder.OpenComponent<RichText>(seq++);
                    builder.AddAttribute(seq++, "Parsed", (
                        small.Child
                    ));
                    builder.CloseComponent();
                    builder.AddMarkupContent(seq++, "\r\n        ");
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "\r\n");

                    break;

                case TextModel.Run run:
                    builder.AddContent(seq++,
                       run.Text
                    );

                    break;

                case TextModel.Error error:
                    builder.AddContent(seq++, "        ");
                    builder.OpenElement(seq++, "span");
                    builder.AddAttribute(seq++, "class", "rich-text__error");
                    builder.AddMarkupContent(seq++, "\r\n            ");
                    builder.OpenComponent<RichText>(seq++);
                    builder.AddAttribute(seq++, "Parsed", (
                        error.Child
                    ));
                    builder.CloseComponent();
                    builder.AddMarkupContent(seq++, "\r\n        ");
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "\r\n");

                    break;

                case TextModel.Private p:
                    if (Session.Username == p.Owner)
                    {
                        builder.AddContent(seq++, "            ");
                        builder.OpenComponent<RichText>(seq++);
                        builder.AddAttribute(seq++, "Parsed", (
                            p.Child
                        ));
                        builder.CloseComponent();
                        builder.AddMarkupContent(seq++, "\r\n");
                    }
                    else
                    {
                        builder.AddContent(seq++,
                            p.AltText
                        );
                    }

                    break;

                case TextModel.Indent indent:
                    for (var i = 0; i < indent.Level; i++)
                    {
                        builder.AddContent(seq++, "...");
                    }

                    break;

                case TextModel.Symbol symbol:
                    builder.AddContent(seq++, "        ");
                    builder.OpenElement(seq++, "span");
                    builder.AddAttribute(seq++, "class", "rich-text__no-break" + (
                        symbol.IsLarge ? " rich-text__no-break--large" : ""
                    ));
                    builder.AddContent(seq++,
                        symbol.Prefix
                    );
                    builder.OpenElement(seq++, "img");
                    builder.AddAttribute(seq++, "class", "rich-text__symbol" + (
                        symbol.IsLarge ? " rich-text__symbol--large" : ""
                    ));
                    builder.AddAttribute(seq++, "src", "/_content/Cardgame.UI/symbols/" + (symbol.Name) + ".png");
                    builder.CloseElement();
                    builder.AddContent(seq++,
                        symbol.Suffix
                    );
                    builder.CloseElement();
                    builder.AddMarkupContent(seq++, "\r\n");

                    break;

                case TextModel.Card card:
                    var model = All.Cards.ByName(card.Name);
                    if (model.Types.Any())
                    {
                        var background = Backgrounds.FromTypes(model.Types);
                        var cost = model.GetCost(Array.Empty<IModifier>());
                        var set = All.Cards.GetSet(card.Name);
                        var value = (model as ITreasureCard)?.StaticValue;
                        builder.AddContent(seq++, "            ");
                        builder.OpenComponent<WithTooltip>(seq++);
                        builder.AddAttribute(seq++, "Content", (RenderFragment)((contentBuilder) => {
                            contentBuilder.AddMarkupContent(seq++, "\r\n                    ");
                            contentBuilder.AddContent(seq++,
                                card.Prefix
                            );
                            contentBuilder.OpenElement(seq++, "span");
                            contentBuilder.AddAttribute(seq++, "style", "background:" + " " + (
                                background
                            ));
                            contentBuilder.AddContent(seq++,
                                Strings.TitleCase(card.Name)
                            );
                            contentBuilder.CloseElement();
                            contentBuilder.AddContent(seq++,
                                card.Suffix
                            );
                            contentBuilder.AddMarkupContent(seq++, "\r\n                ");
                        }
                        ));
                        builder.AddAttribute(seq++, "Tooltip", (RenderFragment)((tooltipBuilder) => {
                            tooltipBuilder.AddMarkupContent(seq++, "\r\n                    ");
                            tooltipBuilder.OpenComponent<Magnify>(seq++);
                            tooltipBuilder.AddAttribute(seq++, "ChildContent", (RenderFragment)((cardBuilder) => {
                                cardBuilder.AddMarkupContent(seq++, "\r\n                        ");
                                cardBuilder.OpenComponent<KingdomCard>(seq++);
                                cardBuilder.AddAttribute(seq++, "Name", (
                                    card.Name
                                ));
                                cardBuilder.AddAttribute(seq++, "Types", (
                                    model.Types
                                ));
                                cardBuilder.AddAttribute(seq++, "Art", (
                                    model.Art
                                ));
                                cardBuilder.AddAttribute(seq++, "Cost", (
                                    cost
                                ));
                                cardBuilder.AddAttribute(seq++, "Text", (
                                    model.Text
                                ));
                                cardBuilder.AddAttribute(seq++, "Set", (
                                    set
                                ));
                                cardBuilder.CloseComponent();
                                cardBuilder.AddMarkupContent(seq++, "\r\n                    ");
                            }
                            ));
                            tooltipBuilder.CloseComponent();
                            tooltipBuilder.AddMarkupContent(seq++, "\r\n                ");
                        }
                        ));
                        builder.CloseComponent();
                        builder.AddMarkupContent(seq++, "   \r\n");
                    }
                    else
                    {
                        builder.AddContent(seq++, "            ");
                        builder.OpenComponent<WithTooltip>(seq++);
                        builder.AddAttribute(seq++, "Content", (RenderFragment)((contentBuilder) => {
                            contentBuilder.AddMarkupContent(seq++, "\r\n                    ");
                            contentBuilder.AddContent(seq++,
                                card.Prefix
                            );
                            contentBuilder.AddContent(seq++, "a ");
                            contentBuilder.AddContent(seq++,
                                Strings.TitleCase(card.Name)
                            );
                            contentBuilder.AddContent(seq++, " token");
                            contentBuilder.AddContent(seq++,
                                card.Suffix
                            );
                            contentBuilder.AddMarkupContent(seq++, "\r\n                ");
                        }
                        ));
                        builder.AddAttribute(seq++, "Tooltip", (RenderFragment)((tooltipBuilder) => {
                            tooltipBuilder.AddMarkupContent(seq++, "\r\n                    ");
                            tooltipBuilder.OpenComponent<Magnify>(seq++);
                            tooltipBuilder.AddAttribute(seq++, "ChildContent", (RenderFragment)((cardBuilder) => {
                                cardBuilder.AddMarkupContent(seq++, "\r\n                        ");
                                cardBuilder.OpenComponent<RichText>(seq++);
                                cardBuilder.AddAttribute(seq++, "Model", (
                                    model.Text
                                ));
                                cardBuilder.CloseComponent();
                                cardBuilder.AddMarkupContent(seq++, "\r\n                    ");
                            }
                            ));
                            tooltipBuilder.CloseComponent();
                            tooltipBuilder.AddMarkupContent(seq++, "\r\n                ");
                        }
                        ));
                        builder.CloseComponent();
                        builder.AddMarkupContent(seq++, "   \r\n");
                    }
                    break;

                case TextModel.Player player:
                    if (Session.Username.Equals(player.Name))
                    {
                        builder.AddContent(seq++,
                            player.Prefix
                        );
                        builder.AddContent(seq++, "You");
                        builder.AddContent(seq++,
                            player.Suffix
                        );
                    }
                    else
                    {
                        builder.AddContent(seq++, "            ");
                        builder.OpenComponent<PlayerLink>(seq++);
                        builder.AddAttribute(seq++, "Name", (
                            player.Name
                        ));
                        builder.AddAttribute(seq++, "Prefix", (
                            player.Prefix
                        ));
                        builder.AddAttribute(seq++, "Suffix", (
                            player.Suffix
                        ));
                        builder.CloseComponent();
                        builder.AddMarkupContent(seq++, "\r\n");
                    }
                    break;

                case TextModel.Pronominal pro:
                    if (Session.Username.Equals(pro.Name))
                    {
                        builder.AddContent(seq++,
                            pro.Prefix
                        );
                        builder.AddContent(seq++,
                            pro.IfYou
                        );
                        builder.AddContent(seq++,
                            pro.Suffix
                        );
                    }
                    else
                    {
                        builder.AddContent(seq++,
                            pro.Prefix
                        );
                        builder.AddContent(seq++,
                            pro.IfThem
                        );
                        builder.AddContent(seq++,
                        pro.Suffix
                        );
                    }
                    break;

                default:
                    builder.AddContent(seq++,
                        Parsed
                    );
                    break;
            }

            return seq;
        }
    }
}