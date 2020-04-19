using Microsoft.AspNetCore.Components;
using System;
using System.Text.Json;

namespace Cardgame.Choices
{
    public class ChoiceBase<TInput, TOutput> : ComponentBase
    {
        [Parameter] public bool Enabled { get; set; }
        [Parameter] public string Input { get; set; }
        [Parameter] public Action<string> Output { get; set; }
        protected TInput Model { get; private set; }

        protected override void OnParametersSet()
        {
            Model = JsonSerializer.Deserialize<TInput>(Input);
        }

        protected void EnterChoice(TOutput output)
        {
            Output(JsonSerializer.Serialize<TOutput>(output));
        }
    }
}