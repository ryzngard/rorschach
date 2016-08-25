using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using rorschach.Actions;
using System.Reflection;
using Microsoft.ApplicationInsights;
using System.Net.Http;
using System.Net;

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

        private static string GetMentions(Activity activity)
        {
            List<string> mentions = new List<string>();
            foreach (Mention m in activity.GetMentions())
            {
                mentions.Add(m.Mentioned.Name);
            }

            return String.Join(",", mentions);
        }

        private void LogEvent(String name, Dictionary<string, string> properties)
        {
            if (rorschach.Properties.Settings.Default.TelemetryEnabled)
            {
                var tc = new TelemetryClient();
                tc.TrackEvent(name, properties);
            }
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ArgumentNullException("activity"));
            }
            try
            {
                using (var wrapper = new ActivityWrapper(activity))
                {
                    // Log the message that comes in
                    {
                        var properties = new Dictionary<string, string>();
                        properties.Add("From", activity.From.Name);
                        properties.Add("Channel", activity.ChannelId);
                        properties.Add("Mentions", GetMentions(activity));

                        this.LogEvent("MessageReceived", properties);
                    }

                    if (activity.Type == ActivityTypes.Message)
                    {
                        bool containsMention = activity.
                            GetMentions().
                            Where(m => m.Mentioned.Name.ToLower().Equals("rorschach2")).Count() > 0;

                        if (containsMention)
                        {
                            if (StaticActions == null)
                            {
                                CreateActions();
                            }
                            string helpMessage;
                            bool isHelp = false;

                            if (wrapper.IsHelp())
                            {
                                helpMessage = "Try one of the following commands \n\n";
                                isHelp = true;
                            }
                            else
                            {
                                helpMessage = $"I could not parse '{activity.Text}', try one of the following instead: \n\n";
                            }


                            foreach (var botAction in StaticActions)
                            {
                                if (!isHelp)
                                {
                                    if (botAction.ParseMessage(wrapper))
                                    {
                                        var properties = new Dictionary<string, string>();
                                        properties.Add("From", activity.From.Name);
                                        properties.Add("Channel", activity.ChannelId);
                                        properties.Add("Command", botAction.GetCommandString());
                                        this.LogEvent("CommandExecuted", properties);
                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                }

                                helpMessage += botAction.HelpMessage() + "\n\n";
                            }

                            wrapper.SendReply(helpMessage);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                    }
                    else
                    {
                        return await HandleSystemMessage(activity);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e);
            }
        }

        private Task<HttpResponseMessage> HandleSystemMessage(Activity activity)
        {
            if (activity.Type == ActivityTypes.Ping)
            {
                return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK));
            }
            else if (activity.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK));
            }
            else if (activity.Type == "BotRemovedFromConversation")
            {
            }
            else if (activity.Type == "UserAddedToConversation")
            {
            }
            else if (activity.Type == "UserRemovedFromConversation")
            {
            }
            else if (activity.Type == "EndOfConversation")
            {
            }

            return Task.FromResult(Request.CreateResponse(HttpStatusCode.NoContent));
        }
    }
}