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
using System.Collections.Generic;
using rorschach.Actions;
using System.Reflection;

namespace rorschach
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        static String HelpMessage = String.Join(
            Environment.NewLine,
            "roll - Rolls a d20");

        static List<IBotAction> StaticActions;

        static void CreateActions()
        {
            StaticActions = new List<IBotAction>();
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.GetInterfaces().Contains(typeof(IBotAction))
                    && t.GetConstructor(Type.EmptyTypes) != null
                select Activator.CreateInstance(t) as IBotAction;

            StaticActions.AddRange(instances);
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                bool containsMention = message.
                    Mentions.
                    Where(m => m.Mentioned.IsBot.Value && m.Mentioned.Name.ToLower().Equals("rorschach2")).Count() > 0;

                bool privateChat = message.TotalParticipants == 2 && message.Participants.FirstOrDefault(p => p.Name.ToLower().Contains("rorschach")) != null;

                if (containsMention || privateChat)
                {
                    Message returnMessage;
                    MessageWrapper wrapper = new MessageWrapper(message);

                    if (StaticActions == null)
                    {
                        CreateActions();
                    }

                    string helpMessage = $"I could not parse '{message.Text}', try one of the following instead: \n\n";

                    foreach (var botAction in StaticActions)
                    {
                        returnMessage = botAction.ParseMessage(wrapper);
                        if (returnMessage != null)
                        {
                            return returnMessage;
                        }

                        helpMessage += botAction.HelpMessage() + "\n";
                    }
                    
                    return message.CreateReplyMessage(helpMessage);
                }
            }
            else
            {
                return HandleSystemMessage(message);
            }

            return null;
        }

        private Message PerformSlashCommand(Message message)
        {
            if (message.Text.Length < 1)
            {
                return null;

            }

            var text = message.Text.ToLower().Trim();

            
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