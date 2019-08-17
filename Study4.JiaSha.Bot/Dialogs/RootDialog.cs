using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Study4.JiaSha.Bot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        private readonly FindFoodRecognizer _luisRecognizer;
        private readonly ILogger Logger;

        public RootDialog(
            FindFoodRecognizer luisRecognizer,
            FindFoodDialog findfoodDialog, 
            ILogger<RootDialog> logger)
            : base(nameof(RootDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(findfoodDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                FirstStepAsync,
                ActStepAsync,
                EndAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // 無設定 LUIS，結束
            //if (!_luisRecognizer.IsConfigured)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //        MessageFactory.Text(
            //            "NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', " +
            //            "'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.",
            //            inputHint: InputHints.IgnoringInput), cancellationToken);
            //    return await stepContext.EndDialogAsync(null, cancellationToken);
            //}

            var messageText = stepContext.Options?.ToString() ?? "請問有甚麼需要幫忙的嗎";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(
                nameof(TextPrompt), 
                new PromptOptions { Prompt = promptMessage }, 
                cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (string)stepContext.Result;

            switch (result)
            {
                case "hi":
                    await stepContext.Context.TraceActivityAsync("hi");
                    await stepContext.Context.SendActivityAsync("hi");
                    return await stepContext.NextAsync();
                case "我餓了":
                    await stepContext.Context.TraceActivityAsync("Find Food");
                    return await stepContext.BeginDialogAsync("FindFoodDialog", new FindFoodDetail(), cancellationToken);
                case "None":
                    await stepContext.Context.TraceActivityAsync("None");
                    await stepContext.Context.SendActivityAsync($"對不起，我不清楚您說甚麼");
                    return await stepContext.NextAsync();
                default:
                    await stepContext.Context.TraceActivityAsync("Utterance not find");
                    await stepContext.Context.SendActivityAsync($"對不起，我不清楚您說甚麼");
                    return await stepContext.NextAsync();
            }
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(
                InitialDialogId, 
                $"還有甚麼是我可以為您服務的嗎?", 
                cancellationToken);
        }
    }
}
