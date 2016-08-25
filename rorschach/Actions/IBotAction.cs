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
    /// a ActivityWrapper object and given the opportunity to return a message to the user.
    /// </summary>
    interface IBotAction
    {
        /// <summary>
        /// Action to take for a message.
        /// </summary>
        /// <param name="wrapper"></param>
        /// <returns>True if an action was taken</returns>
        bool ParseMessage(ActivityWrapper wrapper);

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
