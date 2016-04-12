using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rorschach.Actions
{
    /// <summary>
    /// Represents a possible action for the bot to take based on a message. Is provided with 
    /// a MessageWrapper object and given the opportunity to return a message to the user.
    /// </summary>
    interface IBotAction
    {
        /// <summary>
        /// Action to take for a message. Returns null if no action should be taken.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        Message ParseMessage(MessageWrapper m);

        /// <summary>
        /// Help message to be displayed for this action
        /// </summary>
        /// <returns></returns>
        String HelpMessage();

        /// <summary>
        /// Telemetry name for this action
        /// </summary>
        /// <returns></returns>
        String GetCommandString();
    }
}
