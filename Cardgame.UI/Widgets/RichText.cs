using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Cardgame.API;
using Cardgame.Model;
using Cardgame.Model.ClientServer;

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

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            switch (Parsed)
            {
                case TextModel.Spans spans:
                    foreach (var span in spans.Children)
                    {
                        __builder.AddContent(0, "            ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(1);
                        __builder.AddAttribute(2, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                               span
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(3, "\r\n");
                    }

                    break;

                case TextModel.Lines lines:
                    foreach (var line in lines.Children)
                    {
                        __builder.AddContent(4, "            ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(5);
                        __builder.AddAttribute(6, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                            line
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(7, "<br>\r\n");
                    }

                    break;

                case TextModel.Paras paras:
                    __builder.AddContent(8, "        ");
                    __builder.OpenElement(9, "div");
                    __builder.AddAttribute(10, "class", "rich-text__paras");
                    __builder.AddMarkupContent(11, "\r\n");
                    foreach (var para in paras.Children)
                    {
                        __builder.AddContent(12, "                ");
                        __builder.OpenElement(13, "p");
                        __builder.AddMarkupContent(14, "\r\n                    ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(15);
                        __builder.AddAttribute(16, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                            para
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(17, "\r\n                ");
                        __builder.CloseElement();
                        __builder.AddMarkupContent(18, "\r\n");
                    }
                    __builder.AddContent(19, "        ");
                    __builder.CloseElement();
                    __builder.AddMarkupContent(20, "\r\n");
                    break;

                case TextModel.Split split:
                    for (var i = 0; i < split.Children.Length; i++)
                    {
                        __builder.AddContent(21, "            ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(22);
                        __builder.AddAttribute(23, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                            split.Children[i]
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(24, "\r\n");
                        if (i < split.Children.Length - 1)
                        {
                            __builder.AddContent(25, "                ");
                            __builder.OpenElement(26, "div");
                            __builder.AddAttribute(27, "class", "rich-text__bar" + (
                                split.IsCompact ? " rich-text__bar--compact" : ""
                            ));
                            __builder.CloseElement();
                            __builder.AddMarkupContent(28, "\r\n");
                        }
                    }

                    break;

                case TextModel.Bold bold:
                    __builder.AddContent(29, "        ");
                    __builder.OpenElement(30, "span");
                    __builder.AddAttribute(31, "class", "rich-text__bold");
                    __builder.AddMarkupContent(32, "\r\n            ");
                    __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(33);
                    __builder.AddAttribute(34, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                        bold.Child
                    ));
                    __builder.CloseComponent();
                    __builder.AddMarkupContent(35, "\r\n        ");
                    __builder.CloseElement();
                    __builder.AddMarkupContent(36, "\r\n");

                    break;

                case TextModel.Small small:
                    __builder.AddContent(37, "        ");
                    __builder.OpenElement(38, "span");
                    __builder.AddAttribute(39, "class", "rich-text__small");
                    __builder.AddMarkupContent(40, "\r\n            ");
                    __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(41);
                    __builder.AddAttribute(42, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                        small.Child
                    ));
                    __builder.CloseComponent();
                    __builder.AddMarkupContent(43, "\r\n        ");
                    __builder.CloseElement();
                    __builder.AddMarkupContent(44, "\r\n");

                    break;

                case TextModel.Run run:
                    __builder.AddContent(45,
                       run.Text
                    );

                    break;

                case TextModel.Error error:
                    __builder.AddContent(46, "        ");
                    __builder.OpenElement(47, "span");
                    __builder.AddAttribute(48, "class", "rich-text__error");
                    __builder.AddMarkupContent(49, "\r\n            ");
                    __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(50);
                    __builder.AddAttribute(51, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                        error.Child
                    ));
                    __builder.CloseComponent();
                    __builder.AddMarkupContent(52, "\r\n        ");
                    __builder.CloseElement();
                    __builder.AddMarkupContent(53, "\r\n");

                    break;

                case TextModel.Private p:
                    if (Session.Username == p.Owner)
                    {
                        __builder.AddContent(54, "            ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.RichText>(55);
                        __builder.AddAttribute(56, "Parsed", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.TextModel>(
                            p.Child
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(57, "\r\n");
                    }
                    else
                    {
                        __builder.AddContent(58,
                            p.AltText
                        );
                    }

                    break;

                case TextModel.Indent indent:
                    for (var i = 0; i < indent.Level; i++)
                    {
                        __builder.AddContent(59, "...");
                    }

                    break;

                case TextModel.Symbol symbol:
                    __builder.AddContent(60, "        ");
                    __builder.OpenElement(61, "span");
                    __builder.AddAttribute(62, "class", "rich-text__no-break" + (
                        symbol.IsLarge ? " rich-text__no-break--large" : ""
                    ));
                    __builder.AddContent(63,
                        symbol.Prefix
                    );
                    __builder.OpenElement(64, "img");
                    __builder.AddAttribute(65, "class", "rich-text__symbol" + (
                        symbol.IsLarge ? " rich-text__symbol--large" : ""
                    ));
                    __builder.AddAttribute(66, "src", "/_content/Cardgame.UI/symbols/" + (symbol.Name) + ".png");
                    __builder.CloseElement();
                    __builder.AddContent(67,
                        symbol.Suffix
                    );
                    __builder.CloseElement();
                    __builder.AddMarkupContent(68, "\r\n");

                    break;

                case TextModel.Card card:
                    var model = All.Cards.ByName(card.Name);
                    if (model.Types.Any())
                    {
                        var background = Backgrounds.FromTypes(model.Types);
                        var cost = model.GetCost(Array.Empty<IModifier>());
                        var set = All.Cards.GetSet(card.Name);
                        var value = (model as ITreasureCard)?.StaticValue;
                        __builder.AddContent(69, "            ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.WithTooltip>(70);
                        __builder.AddAttribute(71, "Content", (Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {
                            __builder2.AddMarkupContent(72, "\r\n                    ");
                            __builder2.AddContent(73,
                                card.Prefix
                            );
                            __builder2.OpenElement(74, "span");
                            __builder2.AddAttribute(75, "style", "background:" + " " + (
                                background
                            ));
                            __builder2.AddContent(76,
                                Strings.TitleCase(card.Name)
                            );
                            __builder2.CloseElement();
                            __builder2.AddContent(77,
                                card.Suffix
                            );
                            __builder2.AddMarkupContent(78, "\r\n                ");
                        }
                        ));
                        __builder.AddAttribute(79, "Tooltip", (Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {
                            __builder2.AddMarkupContent(80, "\r\n                    ");
                            __builder2.OpenComponent<Cardgame.UI.Widgets.Magnify>(81);
                            __builder2.AddAttribute(82, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)((__builder3) => {
                                __builder3.AddMarkupContent(83, "\r\n                        ");
                                __builder3.OpenComponent<Cardgame.UI.Widgets.KingdomCard>(84);
                                __builder3.AddAttribute(85, "Name", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
                                    card.Name
                                ));
                                __builder3.AddAttribute(86, "Types", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.API.CardType[]>(
                                    model.Types
                                ));
                                __builder3.AddAttribute(87, "Art", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
                                    model.Art
                                ));
                                __builder3.AddAttribute(88, "Cost", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.API.Cost>(
                                    cost
                                ));
                                __builder3.AddAttribute(89, "Text", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
                                    model.Text
                                ));
                                __builder3.AddAttribute(90, "Set", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Cardgame.Model.CardSet?>(
                                    set
                                ));
                                __builder3.CloseComponent();
                                __builder3.AddMarkupContent(91, "\r\n                    ");
                            }
                            ));
                            __builder2.CloseComponent();
                            __builder2.AddMarkupContent(92, "\r\n                ");
                        }
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(93, "   \r\n");
                    }
                    else
                    {
                        __builder.AddContent(94, "            ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.WithTooltip>(95);
                        __builder.AddAttribute(96, "Content", (Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {
                            __builder2.AddMarkupContent(97, "\r\n                    ");
                            __builder2.AddContent(98,
                                card.Prefix
                            );
                            __builder2.AddContent(99, "a ");
                            __builder2.AddContent(100,
                                Strings.TitleCase(card.Name)
                            );
                            __builder2.AddContent(101, " token");
                            __builder2.AddContent(102,
                                card.Suffix
                            );
                            __builder2.AddMarkupContent(103, "\r\n                ");
                        }
                        ));
                        __builder.AddAttribute(104, "Tooltip", (Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {
                            __builder2.AddMarkupContent(105, "\r\n                    ");
                            __builder2.OpenComponent<Cardgame.UI.Widgets.Magnify>(106);
                            __builder2.AddAttribute(107, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)((__builder3) => {
                                __builder3.AddMarkupContent(108, "\r\n                        ");
                                __builder3.OpenComponent<Cardgame.UI.Widgets.RichText>(109);
                                __builder3.AddAttribute(110, "Model", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
                                    model.Text
                                ));
                                __builder3.CloseComponent();
                                __builder3.AddMarkupContent(111, "\r\n                    ");
                            }
                            ));
                            __builder2.CloseComponent();
                            __builder2.AddMarkupContent(112, "\r\n                ");
                        }
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(113, "   \r\n");
                    }
                    break;

                case TextModel.Player player:
                    if (Session.Username.Equals(player.Name))
                    {
                        __builder.AddContent(114,
                            player.Prefix
                        );
                        __builder.AddContent(115, "You");
                        __builder.AddContent(116,
                            player.Suffix
                        );
                    }
                    else
                    {
                        __builder.AddContent(117, "            ");
                        __builder.OpenComponent<Cardgame.UI.Widgets.PlayerLink>(118);
                        __builder.AddAttribute(119, "Name", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
                            player.Name
                        ));
                        __builder.AddAttribute(120, "Prefix", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
                            player.Prefix
                        ));
                        __builder.AddAttribute(121, "Suffix", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<System.String>(
                            player.Suffix
                        ));
                        __builder.CloseComponent();
                        __builder.AddMarkupContent(122, "\r\n");
                    }
                    break;

                case TextModel.Pronominal pro:
                    if (Session.Username.Equals(pro.Name))
                    {
                        __builder.AddContent(123,
                            pro.Prefix
                        );
                        __builder.AddContent(124,
                            pro.IfYou
                        );
                        __builder.AddContent(125,
                            pro.Suffix
                        );
                    }
                    else
                    {
                        __builder.AddContent(126,
                            pro.Prefix
                        );
                        __builder.AddContent(127,
                            pro.IfThem
                        );
                        __builder.AddContent(128,
                        pro.Suffix
                        );
                    }
                    break;

                default:
                    __builder.AddContent(129,
                        Parsed
                    );
                    break;
            }
        }
    }
}