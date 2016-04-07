using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rorschach.Actions
{
    interface IBotAction
    {
        Message ParseMessage(MessageWrapper m);
        String HelpMessage();
    }
}
