using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Study4.JiaSha.Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Study4.JiaSha.Bot.Dialogs
{
    public class FindFoodDialog : CancelAndHelpDialog
    {
        private readonly string _googleKey;

        public FindFoodDialog(IConfiguration configuration)
            : base(nameof(FindFoodDialog))
        {
            _googleKey = configuration["GoogleMapKey"];

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), ScorePromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SelectTypeAsync,
                InputeScoreAsync,
                CopmleteAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
               nameof(ChoicePrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("請選擇食物類型"),
                   Choices = ChoiceFactory.ToChoices(new List<string> { "餐廳", "咖啡", "夜店" }),
                   RetryPrompt = MessageFactory.Text("您輸入的資料不正確")
               },
               cancellationToken);
        }


        private async Task<DialogTurnResult> InputeScoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var detail = (FindFoodDetail)stepContext.Options;
            detail.FoodType = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(
                nameof(NumberPrompt<int>),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("請輸入你的最低評分 ( 1~5 )"),
                    RetryPrompt = MessageFactory.Text("請輸入 1 ~ 5 分間"),
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> CopmleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var detail = (FindFoodDetail)stepContext.Options;
            detail.Score = (int)stepContext.Result;

            await stepContext.Context.SendActivityAsync(
                $"你選擇的\n" +
                $"餐廳類別為 : {detail.FoodType}\n" +
                $"評分為 : {detail.Score}");

            var typeString = "";
            switch (detail.FoodType)
            {
                case "餐廳":
                    typeString = $"type=restaurant";
                    break;
                case "咖啡":
                    typeString = $"type=cafe";
                    break;
                case "夜店":
                    typeString = $"type=night_club";
                    break;
                default:
                    break;
            }

            var client = new HttpClient();
            var res = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/nearbysearch/json?" +
                                    $"location=31.170887,121.4036843&radius=10000&{typeString}" +
                                    //$"&location=25.040585,121.5648247&radius=1000&query={detail.FoodType}" +
                                    $"&language=zh-tw&key={_googleKey}");

            res.EnsureSuccessStatusCode();
            string json = await res.Content.ReadAsStringAsync();
            var restaurantResult = JsonConvert.DeserializeObject<RestaurantResult>(json);
            var activity = MessageFactory.Carousel(new List<Attachment>());

            foreach (var restaurant in restaurantResult.Results)
            {
                // 小於評分不列出
                if (restaurant.Rating < detail.Score)
                {
                    continue;
                }

                HeroCard heroCard = new HeroCard()
                {
                    Title = restaurant.Name,
                    Subtitle = $"{restaurant.Rating} starts.",
                    Buttons = new List<CardAction>()
                    {
                        new CardAction()
                        {
                            Title = "更多詳情",
                            Type = ActionTypes.OpenUrl,
                            Value = $"https://www.bing.com/search?q={restaurant.Name}"
                        }
                    }
                };

                if(restaurant.Photos != null)
                {
                    heroCard.Images = new List<CardImage>()
                    {
                        new CardImage() { Url =
                        $"https://maps.googleapis.com/maps/api/place/photo?maxwidth=200&photoreference=" +
                        $"{restaurant.Photos[0].PhotoReference}&key={_googleKey}"

                        }
                    };
                }

                activity.Attachments.Add(heroCard.ToAttachment());
               
            }

            await stepContext.Context.SendActivityAsync($"我找到了 {activity.Attachments.Count()} 家餐廳:");


            await stepContext.Context.SendActivityAsync(activity);

            return await stepContext.EndDialogAsync();

            
        }

        /// <summary>
        /// Score 驗證
        /// </summary>
        /// <param name="promptContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static Task<bool> ScorePromptValidatorAsync(
            PromptValidatorContext<int> promptContext, 
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                promptContext.Recognized.Succeeded && 
                promptContext.Recognized.Value > 0 && 
                promptContext.Recognized.Value < 6);
        }
    }
}
