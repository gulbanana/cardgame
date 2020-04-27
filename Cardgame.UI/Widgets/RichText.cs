using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Cardgame.API;
using Cardgame.Model;
using Cardgame.Model.ClientServer;
using Microsoft.AspNetCore.Components.Rendering;
using System.Text;

namespace Cardgame.UI.Widgets
{
    public class RichText : ComponentBase
    {
        [Inject] private IUserSession Session { get; set; }
        [Parameter] public string Text { get; set; }
        private TextModel Parsed;

        protected override void OnParametersSet()
        {
            Parsed = TextModel.Parse(Text);
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
                    builder.OpenElement(seq++, "span");
                        builder.AddAttribute(seq++, "class", "rich-text__bold");
                        seq = RenderTextNode(builder, bold.Child, seq);
                    builder.CloseElement();
                    break;

                case TextModel.Small small:
                    builder.OpenElement(seq++, "span");
                        builder.AddAttribute(seq++, "class", "rich-text__small");
                        seq = RenderTextNode(builder, small.Child, seq);
                    builder.CloseElement();
                    break;

                case TextModel.Run run:
                    builder.AddContent(seq++, $" {run.Text} ");
                    break;

                case TextModel.Error error:
                    builder.OpenElement(seq++, "span");
                        builder.AddAttribute(seq++, "class", "rich-text__error");
                        seq = RenderTextNode(builder, error.Child, seq);
                    builder.CloseElement();
                    break;

                case TextModel.Private p:
                    if (Session.Username == p.Owner)
                    {
                        seq = RenderTextNode(builder, p.Child, seq);
                    }
                    else
                    {
                        builder.AddContent(seq++, p.AltText);
                    }

                    break;

                case TextModel.Indent indent:
                    var indentBuilder = new StringBuilder();
                    for (var i = 0; i < indent.Level; i++)
                    {
                        indentBuilder.Append("... ");
                    }
                    builder.AddContent(seq++, indentBuilder.ToString());

                    break;

                case TextModel.Symbol symbol:
                    var largeSpanClass = symbol.IsLarge ? " rich-text__no-break--large" : "";
                    var largeImgClass = symbol.IsLarge ? " rich-text__symbol--large" : "";
                    builder.AddMarkupContent(seq++, $"<span class=\"rich-text__no-break{largeSpanClass}\">{symbol.Prefix}<img class=\"rich-text__symbol{largeImgClass}\" src=\"/_content/Cardgame.UI/symbols/{symbol.Name}.png\">{symbol.Suffix}</span>");
                    break;

                case TextModel.Card card:
                    var model = All.Cards.ByName(card.Name);
                    if (model.Types.Any())
                    {
                        var background = Backgrounds.FromTypes(model.Types);
                        var cost = model.GetCost(Array.Empty<IModifier>());
                        var set = All.Cards.GetSet(card.Name);
                        var value = (model as ITreasureCard)?.StaticValue;

                        builder.OpenComponent<WithTooltip>(seq++);
                            builder.AddAttribute(seq++, nameof(WithTooltip.Content), (RenderFragment)((contentBuilder) => {
                                contentBuilder.AddMarkupContent(seq++, $"{card.Prefix}<span style=\"background: {background}\">{Strings.TitleCase(card.Name)}</span>{card.Suffix}");
                            }));
                            builder.AddAttribute(seq++, nameof(WithTooltip.Tooltip), (RenderFragment)((tooltipBuilder) => {
                                tooltipBuilder.OpenComponent<Magnify>(seq++);
                                    tooltipBuilder.AddAttribute(seq++, nameof(Magnify.ChildContent), (RenderFragment)((cardBuilder) => {
                                        cardBuilder.OpenComponent<KingdomCard>(seq++);
                                            cardBuilder.AddAttribute(seq++, "Name", card.Name);
                                            cardBuilder.AddAttribute(seq++, "Types", model.Types);
                                            cardBuilder.AddAttribute(seq++, "Art", model.Art);
                                            cardBuilder.AddAttribute(seq++, "Cost", cost);
                                            cardBuilder.AddAttribute(seq++, "Text", model.Text);
                                            cardBuilder.AddAttribute(seq++, "Set", set);
                                        cardBuilder.CloseComponent();
                                    }));
                                tooltipBuilder.CloseComponent();
                            }));
                        builder.CloseComponent();
                    }
                    else
                    {
                        builder.OpenComponent<WithTooltip>(seq++);
                            builder.AddAttribute(seq++, nameof(WithTooltip.Content), (RenderFragment)((contentBuilder) => {
                                contentBuilder.AddContent(seq++, $"{card.Prefix}a {Strings.TitleCase(card.Name)} token{card.Suffix}");
                            }));
                            builder.AddAttribute(seq++, nameof(WithTooltip.Tooltip), (RenderFragment)((tooltipBuilder) => {
                                tooltipBuilder.OpenComponent<Magnify>(seq++);
                                    tooltipBuilder.AddAttribute(seq++, nameof(Magnify.ChildContent), (RenderFragment)((cardBuilder) => {
                                        cardBuilder.OpenComponent<RichText>(seq++);
                                            cardBuilder.AddAttribute(seq++, nameof(Text), model.Text);
                                        cardBuilder.CloseComponent();
                                    }));
                                tooltipBuilder.CloseComponent();
                            }));
                        builder.CloseComponent();
                    }
                    break;

                case TextModel.Player player:
                    if (Session.Username.Equals(player.Name))
                    {
                        builder.AddContent(seq++, $" {player.Prefix}You{player.Suffix} ");
                    }
                    else
                    {
                        builder.OpenComponent<PlayerLink>(seq++);                        
                        builder.AddAttribute(seq++, nameof(PlayerLink.Prefix), player.Prefix);
                        builder.AddAttribute(seq++, nameof(PlayerLink.Name), player.Name);
                        builder.AddAttribute(seq++, nameof(PlayerLink.Suffix), player.Suffix);
                        builder.CloseComponent();
                    }
                    break;

                case TextModel.Pronominal pro:
                    if (Session.Username.Equals(pro.Name))
                    {
                        builder.AddContent(seq++, $" {pro.Prefix}{pro.IfYou}{pro.Suffix} ");
                    }
                    else
                    {
                        builder.AddContent(seq++, $" {pro.Prefix}{pro.IfThem}{pro.Suffix} ");
                    }
                    break;

                default:
                    builder.AddContent(seq++,
                        node.ToString()
                    );
                    break;
            }

            return seq;
        }
    }
}