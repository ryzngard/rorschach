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
        public Message ParseMessage(MessageWrapper m)
        {
            if (m.StrippedText.Trim().ToLower().Equals("scoreboard"))
            {
                string output = "Name\t\tPoints\r";
                output += "====================\r";

                Kudo[] kudos = KudosData.GetAllKudos();

                if (kudos.Length == 0)
                {
                    return m.CreateReply("Nobody has given points yet!");
                }

                // Make sure to display in order
                Array.Sort(kudos);

                foreach (Kudo kudo in kudos)
                {
                    output += String.Format($"{kudo.Name}\t\t{kudo.Points}\n", kudo.Name, kudo.Points);
                }

                return m.CreateReply(output);
            }

            return null;
        }
    }
}