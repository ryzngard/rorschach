﻿using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rorschach
{
    public class MessageWrapper
    {
        public Message OriginalMessage { get; private set; }
        public String StrippedText { get; private set; }

        public MessageWrapper(Message m)
        {
            this.OriginalMessage = m;
            this.StrippedText = StripText(m.Text);
        }

        private String StripText(String text)
        {
            // Remove the first @name 
            return text.Replace("@rorschach2:", "").Trim();
        }

        /// <summary>
        /// Easy access to creating a reply to the original message.
        /// </summary>
        /// <param name="v">Reply string</param>
        /// <returns></returns>
        internal Message CreateReply(string v)
        {
            return this.OriginalMessage.CreateReplyMessage(v);
        }
    }
}