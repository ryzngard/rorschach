using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;

namespace rorschach
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        static String HelpMessage = String.Join(
            Environment.NewLine,
            "roll - Rolls a d20");

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message" && message.To.IsBot.Value && message.To.Name.ToLower().Equals("rorschach"))
            {
                var returnMessage = PerformSlashCommand(message);
                if (returnMessage != null)
                {
                    return returnMessage;
                }
                else
                {
                    return message.CreateReplyMessage($"I could not parse '{message.Text}', try one of the following instead: \n\n" + HelpMessage);
                }
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message PerformSlashCommand(Message message)
        {
            if (message.Text.Length < 1)
            {
                return null;

            }

            var text = message.Text.ToLower().Trim();

            // For now, just do if statements. Maybe find a more elegant way to do this later
            if (text.Equals("roll"))
            {
                int diceResult = (new Random()).Next(20);
                return message.CreateReplyMessage($"Rolling d20... result is: {diceResult}");
            }
            return null;
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Hello earthlings, mind if I step in?";
                return reply;
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}