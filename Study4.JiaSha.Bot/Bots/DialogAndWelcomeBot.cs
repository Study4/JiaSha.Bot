using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Study4.JiaSha.Bot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded, 
            ITurnContext<IConversationUpdateActivity> turnContext, 
            CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Old Version
                    //var reply = context.Activity.CreateReply("Hi!! 我是 GiGi，有甚麼可以幫忙的?");
                    //ConnectorClient connector = new ConnectorClient(new Uri(context.Activity.ServiceUrl),
                    //    new MicrosoftAppCredentials("bd2b54aa-4b81-4ec2-84a9-298dcd30a242", "afvqKAPFYU643;:bzgT27~#"));
                    //await connector.Conversations.ReplyToActivityAsync(reply);

                    //var welcomeCard = CreateAdaptiveCardAttachment(); // Use Card
                    //var response = MessageFactory.Attachment(welcomeCard); // Use Card
                    //await turnContext.SendActivityAsync(response, cancellationToken); // Use Card

                    await turnContext.SendActivityAsync(
                        $"Hi {member.Name}!! 我是 GiGi，有甚麼可以幫忙的?",
                        null,
                        InputHints.IgnoringInput,
                        cancellationToken);

                    await Dialog.RunAsync(
                        turnContext, 
                        ConversationState.CreateProperty<DialogState>("DialogState"), 
                        cancellationToken);
                }
            }
        }

        // Load attachment from embedded resource.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var cardResourcePath = "Study4.JiaSha.Bot.Cards.welcomeCard.json";

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                }
            }
        }
    }
}

