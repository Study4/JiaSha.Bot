using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Study4.JiaSha.Bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Study4.JiaSha.Bot.Dialogs
{
    public class FindFoodDialog : DialogContainer
    {
        private readonly string _googleKey;
        private readonly List<Choice> _foodChoice;

        public FindFoodDialog() : base("Start")
        {
            _googleKey = "[YourGoogleKey]";

            _foodChoice = new List<Choice>(){
                new Choice { Value = "餐廳" },
                new Choice { Value = "中式" },
                new Choice { Value = "日式" },

            };

            Dialogs.Add("Start",
                new WaterfallStep[]
                {
                    async (dc, args, next) =>
                    {
                        await dc.Prompt(
                            "choicePrompt","請選擇您想要的食物類型:",
                            new ChoicePromptOptions() {
                                Choices = _foodChoice,
                                RetryPromptString = "您輸入的資料不正確"
                            });
                    },
                    // 評分
                    async (dc, args, next) =>
                    {
                        var state = dc.Context.GetConversationState<Dictionary<string, object>>();
                        state.Add("FoodType",((ChoiceResult)args).Value.Value);

                        await dc.Prompt(
                            "ratingPrompt","請選擇您想要的最低評分: ( 1 ~ 5 )",
                            new PromptOptions() {
                                RetryPromptString = "您輸入的資料不正確"
                            });
                    },
                    // 完成
                    async (dc, args, next) =>
                    {
                        var state = dc.Context.GetConversationState<Dictionary<string, object>>();

                        var rating = ((NumberResult<int>)args).Value;

                        var client = new HttpClient();
                        var res = await client.GetAsync($"https://maps.googleapis.com/maps/api/place/nearbysearch/json?" +
                            $"location=25.040585,121.5648247&radius=1000&query={state["FoodType"]}&" +
                            $"language=zh-tw&key={_googleKey}");
                        res.EnsureSuccessStatusCode();

                        string json = await res.Content.ReadAsStringAsync();
                        var restaurantResult = JsonConvert.DeserializeObject<RestaurantResult>(json);

                        await dc.Context.SendActivity($"我找到了 {restaurantResult.Results.Count()} 家餐廳:");
                        var activity = MessageFactory.Carousel(new List<Attachment>());

                        foreach (var restaurant in restaurantResult.Results)
                        {
                            if(restaurant.Rating > rating)
                            {
                                HeroCard heroCard = new HeroCard()
                                {
                                    Title = restaurant.Name,
                                    Subtitle = $"{restaurant.Rating} starts.",
                                    Images = new List<CardImage>()
                                    {
                                        new CardImage() { Url =
                                        $"https://maps.googleapis.com/maps/api/place/photo?maxwidth=200&photoreference=" +
                                        $"{restaurant.Photos[0].PhotoReference}&key={_googleKey}"

                                        }
                                    },
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

                                activity.Attachments.Add(heroCard.ToAttachment());
                            }
                        }

                        await dc.Context.SendActivity(activity);
                        await dc.End();
                    }
                }
            );

            Dialogs.Add("choicePrompt", new Microsoft.Bot.Builder.Dialogs.ChoicePrompt(
                Culture.English, async (context, result) =>
            {
                if (!_foodChoice.Select(m => m.Value).Contains(result.Value.Value))
                {
                    await context.SendActivity("輸入不正確，請重新輸入");
                    result.Status = PromptStatus.NotRecognized;
                }
            })
            { Style = ListStyle.Auto });

            Dialogs.Add("ratingPrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(
                Culture.English, async (context, result) =>
            {
                if (result.Value < 0)
                {
                    await context.SendActivity("輸入錯誤");
                    result.Status = PromptStatus.TooSmall;
                }

                if (result.Value > 5)
                {
                    await context.SendActivity("輸入錯誤");
                    result.Status = PromptStatus.TooBig;
                }

            }));
        }
    }
}
