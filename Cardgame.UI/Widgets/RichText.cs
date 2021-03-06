﻿using System;
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
        [CascadingParameter(Name = "Current")] public GameModel Game { get; set; }
        private TextModel model;

        protected override void OnParametersSet()
        {
            model = TextModel.Parse(Text);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            RenderNode(builder, model, 0);
        }

        private int RenderNode(RenderTreeBuilder builder, TextModel node, int seq)
        {
            if (node.IsStatic)
            {
                builder.AddMarkupContent(seq++, RenderStatic(node));
                return seq;
            }

            switch (node)
            {
                case TextModel.Spans spans:
                    foreach (var span in spans.Children)
                    {
                        seq = RenderNode(builder, span, seq);
                    }
                    break;

                case TextModel.Lines lines:
                    for (var i = 0; i < lines.Children.Length; i++)
                    {
                        seq = RenderNode(builder, lines.Children[i], seq);
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
                                seq = RenderNode(builder, para, seq);                        
                            builder.CloseElement();
                        }
                    builder.CloseElement();
                    break;

                case TextModel.Split split:
                    for (var i = 0; i < split.Children.Length; i++)
                    {
                        seq = RenderNode(builder, split.Children[i], seq);

                        if (i < split.Children.Length - 1)
                        {
                            var compactClass = split.IsCompact ? " rich-text__bar--compact" : "";
                            builder.AddMarkupContent(seq++, $"<div class=\"rich-text__bar{compactClass}\"></div>");                            
                        }
                    }
                    break;

                case TextModel.Bold bold:
                    builder.OpenElement(seq++, "span");
                        builder.AddAttribute(seq++, "class", "rich-text__bold");
                        seq = RenderNode(builder, bold.Child, seq);
                    builder.CloseElement();
                    break;

                case TextModel.Small small:
                    builder.OpenElement(seq++, "span");
                        builder.AddAttribute(seq++, "class", "rich-text__small");
                        seq = RenderNode(builder, small.Child, seq);
                    builder.CloseElement();
                    break;

                case TextModel.Error error:
                    builder.OpenElement(seq++, "span");
                        builder.AddAttribute(seq++, "class", "rich-text__error");
                        seq = RenderNode(builder, error.Child, seq);
                    builder.CloseElement();
                    break;

                case TextModel.Private @private:
                    var isOwner = Session.Username == @private.Owner || 
                                  (Game != null && Game.ActivePlayer == @private.Owner && Game.ControllingPlayer == Session.Username) || 
                                  (Game != null && !Game.Players.Contains(Session.Username));
                    if (isOwner)
                    {
                        seq = RenderNode(builder, @private.Child, seq);
                    }
                    else
                    {
                        builder.AddContent(seq++, @private.AltText);
                    }
                    break;

                case TextModel.Run run:
                    builder.AddContent(seq++, $" {run.Text} ");
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

                        builder.AddContent(seq++, " " + card.Prefix);
                        builder.OpenComponent<WithTooltip>(seq++);
                            builder.AddAttribute(seq++, nameof(WithTooltip.Content), (RenderFragment)((contentBuilder) => {
                                contentBuilder.AddMarkupContent(seq++, $"<span style=\"background: {background}\">{Strings.TitleCase(card.Name)}</span>");
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
                        builder.AddContent(seq++, card.Suffix + " ");
                    }
                    else
                    {
                        builder.AddContent(seq++, " " + card.Prefix);
                        builder.OpenComponent<WithTooltip>(seq++);
                            builder.AddAttribute(seq++, nameof(WithTooltip.Content), (RenderFragment)((contentBuilder) => {
                                contentBuilder.AddContent(seq++, $"a {Strings.TitleCase(card.Name)} token");
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
                        builder.AddContent(seq++, card.Suffix + " ");
                    }
                    break;

                case TextModel.Player player:
                    if (Session.Username.Equals(player.Name))
                    {
                        builder.AddContent(seq++, $" {player.Prefix}{(player.IsNonterminal ? "you" : "You")}{player.Suffix} ");
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

        private string RenderStatic(TextModel node)
        {
            var builder = new StringBuilder();

            switch (node)
            {
                case TextModel.Spans spans:
                    foreach (var span in spans.Children)
                    {
                        builder.Append(RenderStatic(span));
                    }
                    break;

                case TextModel.Lines lines:
                    for (var i = 0; i < lines.Children.Length; i++)
                    {
                        builder.Append(RenderStatic(lines.Children[i]));
                        if (i < lines.Children.Length - 1)
                        {
                            builder.Append("<br>");
                        }
                    }
                    break;

                case TextModel.Paras paras:
                    builder.Append("<div class=\"rich-text__paras\">");
                    foreach (var para in paras.Children)
                    {
                        builder.Append("<p>");
                        builder.Append(RenderStatic(para));
                        builder.Append("</p>");
                    }
                    builder.Append("</div>");
                    break;

                case TextModel.Split split:
                    var compactClass = split.IsCompact ? " rich-text__bar--compact" : "";
                    for (var i = 0; i < split.Children.Length; i++)
                    {
                        builder.Append(RenderStatic(split.Children[i]));
                        if (i < split.Children.Length - 1)
                        {
                            builder.Append($"<div class=\"rich-text__bar{compactClass}\"></div>");
                        }
                    }
                    break;

                case TextModel.Bold bold:
                    builder.Append("<span class=\"rich-text__bold\">");
                    builder.Append(RenderStatic(bold.Child));
                    builder.Append("</span>");
                    break;

                case TextModel.Small small:
                    builder.Append("<span class=\"rich-text__small\">");
                    builder.Append(RenderStatic(small.Child));
                    builder.Append("</span>");                    
                    break;

                case TextModel.Error error:
                    builder.Append("<span class=\"rich-text__error\">");
                    builder.Append(RenderStatic(error.Child));
                    builder.Append("</span>");
                    break;

                case TextModel.Private @private:
                    var isOwner = Session.Username == @private.Owner || 
                                  (Game != null && Game.ActivePlayer == @private.Owner && Game.ControllingPlayer == Session.Username) || 
                                  (Game != null && !Game.Players.Contains(Session.Username));
                    if (isOwner)
                    {
                        builder.Append(RenderStatic(@private.Child));
                    }
                    else
                    {
                        builder.Append($" {@private.AltText} ");
                    }
                    break;

                case TextModel.Run run:
                    builder.Append($" {run.Text} ");
                    break;

                case TextModel.Indent indent:
                    for (var i = 0; i < indent.Level; i++)
                    {
                        builder.Append("... ");
                    }
                    break;

                case TextModel.Symbol symbol:
                    var largeSpanClass = symbol.IsLarge ? " rich-text__no-break--large" : "";
                    var largeImgClass = symbol.IsLarge ? " rich-text__symbol--large" : "";
                    builder.Append($"<span class=\"rich-text__no-break{largeSpanClass}\">{symbol.Prefix}<img class=\"rich-text__symbol{largeImgClass}\" src=\"/_content/Cardgame.UI/symbols/{symbol.Name}.png\">{symbol.Suffix}</span>");
                    break;

                case TextModel.Pronominal pro:
                    if (Session.Username.Equals(pro.Name))
                    {
                        builder.Append($" {pro.Prefix}{pro.IfYou}{pro.Suffix} ");
                    }
                    else
                    {
                        builder.Append($" {pro.Prefix}{pro.IfThem}{pro.Suffix} ");
                    }
                    break;

                case TextModel.Player _:
                case TextModel.Card _:
                default:
                    builder.Append($" unsupported static node {node.GetType().Name} ");
                    break;
            }

            return builder.ToString();
        }
    }
}