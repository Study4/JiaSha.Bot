using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study4.JiaSha.Bot.Dialogs
{
    public class RootDialog : DialogContainer
    {
        public RootDialog() : base("Start")
        {
            Dialogs.Add("FindFoodDialog", new FindFoodDialog());
            Dialogs.Add("Start",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                         var result = dc.Context.Services.Get<RecognizerResult>
                            (LuisRecognizerMiddleware.LuisRecognizerResultKey);
                        var topIntent = result?.GetTopScoringIntent();
                        switch (topIntent?.intent)
                        {
                            case "hi":
                                await dc.Context.TraceActivity("hi");
                                await dc.Context.SendActivity("hi");
                                await dc.End();
                                break;
                            case "我餓了":
                                await dc.Context.TraceActivity("Find Food");
                                await dc.Begin("FindFoodDialog");
                                break;
                            case "None":
                                await dc.Context.TraceActivity("None");
                                await dc.Context.SendActivity($"對不起，我不清楚您說甚麼");
                                await dc.End();
                                break;
                            default:
                                await dc.Context.TraceActivity("Utterance not find");
                                await dc.Context.SendActivity($"對不起，我不清楚您說甚麼");
                                await dc.End();
                                break;
                        }
                    },
                    async (dc, args, next) =>
                    {
                        await dc.Context.SendActivity($"還有甚麼是我可以為您服務的嗎?");
                        await dc.End();
                    }
                }
            );
        }
    }
}
