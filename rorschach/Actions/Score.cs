using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using rorschach.Data;

namespace rorschach.Actions
{
    public class Score : IBotAction
    {
        public string GetCommandString()
        {
            return "ScoreBoard";
        }

        public string HelpMessage()
        {
            return "scoreboard, to get the current kudos scoreboard";
        }

        public Message ParseMessage(MessageWrapper m)
        {
            if (m.StrippedText.Trim().ToLower().Equals("scoreboard"))
            {
                string output = "```";
                output += String.Format("{0, -20} {1}\n\n","Name", "Points");

                Kudo[] kudos = KudosData.GetAllKudos();

                if (kudos.Length == 0)
                {
                    return m.CreateReply("Nobody has given points yet!");
                }

                // Make sure to display in order
                Array.Sort(kudos);

                foreach (Kudo kudo in kudos)
                {
                    output += String.Format("{0, -20} {1}\n\n", kudo.Name, kudo.Points);
                }

                output += "```";
                return m.CreateReply(output);
            }

            return null;
        }
    }
}