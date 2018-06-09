using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Study4.JiaSha.Bot.Dialogs;

namespace Study4.JiaSha.Bot
{
    public class RootBot : IBot
    {
        private readonly DialogSet _dialogs;

        public RootBot()
        {
            _dialogs = new DialogSet();
            _dialogs.Add("RootDialog", new RootDialog());
        }
     
        public async Task OnTurn(ITurnContext context)
        {
            // This bot is only handling Messages
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var state = context.GetConversationState<Dictionary<string, object>>();
                var dc = _dialogs.CreateContext(context, state);
                // Bump the turn count. 
                // state.TurnCount++;

                // Echo back to the user whatever they typed.
                // await context.SendActivity($"Hello World");
                await dc.Continue();
                if (!context.Responded)
                {
                    await dc.Begin("RootDialog");
                }
            }
            else
            {
                await HandleSystemMessageAsync(context);
            }
        }

        private async Task<Activity> HandleSystemMessageAsync(ITurnContext context)
        {
            if (context.Activity.Type == ActivityTypes.DeleteUserData)
            {

            }
            else if (context.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (context.Activity.MembersAdded.Any(o => o.Id == context.Activity.Recipient.Id))
                {
                    var reply = context.Activity.CreateReply("Hi!! 我是 GiGi，有甚麼可以幫忙的?");
                    ConnectorClient connector = new ConnectorClient(new Uri(context.Activity.ServiceUrl),
                        new MicrosoftAppCredentials("bd2b54aa-4b81-4ec2-84a9-298dcd30a242", "afvqKAPFYU643;:bzgT27~#"));
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (context.Activity.Type == ActivityTypes.ContactRelationUpdate)
            {

            }
            else if (context.Activity.Type == ActivityTypes.Typing)
            {

            }
            else if (context.Activity.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }    
}
