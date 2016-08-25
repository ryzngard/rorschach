using Microsoft.Bot.Connector;
using rorschach.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace rorschach
{
    public class ActivityWrapper : IDisposable
    {
        public Activity OriginalActivity { get; private set; }
        public String StrippedText { get; private set; }

        private ConnectorClient connector;

        public ActivityWrapper(Activity m)
        {
            this.OriginalActivity = m;
            this.StrippedText = StripText(m.Text);
            this.connector = new ConnectorClient(new Uri(m.ServiceUrl));
        }

        private String StripText(String text)
        {
            string part = text.ReplaceFirst("@rorschach2", "");
            
            if (part.StartsWith(":"))
            {
                return part.Substring(1).Trim();
            }

            return part.Trim();
        }

        

        /// <summary>
        /// Easy access to creating a reply to the original message.
        /// </summary>
        /// <param name="reply">Reply string</param>
        /// <returns></returns>
        internal void SendReply(string reply)
        {
            var replyObject = this.OriginalActivity.CreateReply(reply);
            this.connector.Conversations.ReplyToActivity(replyObject);
        }

        internal async Task SendReplyAsync(string reply)
        {
            var replyObject = this.OriginalActivity.CreateReply(reply);
            await this.connector.Conversations.ReplyToActivityAsync(replyObject);
        }

        public void Dispose()
        {
            this.connector.Dispose();
        }

        internal bool IsHelp()
        {
            return this.StrippedText.ToLower().Equals("help");
        }
    }
}