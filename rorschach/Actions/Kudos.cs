using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using rorschach.Data;

namespace rorschach.Actions
{
    /// <summary>
    /// Adds a simple kudos type system. Doing '@rorschach: 20 @person "for being awesome"' records points for being awesome
    /// </summary>
    public class Kudos : IBotAction
    {
        public string GetCommandString()
        {
            return "Kudos";
        }

        public string HelpMessage()
        {
            return "<number> @name <reason>, to give someone kudos! Example: 20 @rorschach being the best damn bot there ever was";
        }

        public Message ParseMessage(MessageWrapper m)
        {
            // Check the text for the correct pattern
            const String pat = "^(?<negative>-{0,1})(?<points>[0-9]+)\\s@{0,1}(?<name>[\\w]+)\\s(?<reason>.+)";
            const String pat2 = "^@{0,1}(?<name>[\\w]+)\\s(?<negative>-{0,1})(?<points>[0-9]+)\\s(?<reason>.+)";
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            Regex r2 = new Regex(pat2, RegexOptions.IgnoreCase);

            Match match = r.Match(m.StrippedText);

            // Attempt to match with the alternative pattern
            if (!match.Success)
            {
                match = r2.Match(m.StrippedText);
            }

            if (match.Success)
            {
                // See if number is negative
                bool isNegative = match.Groups["negative"].Success && !String.IsNullOrEmpty(match.Groups["negative"].Value);
                
                if (!match.Groups[1].Success)
                {
                    return null;
                }

                int number = int.Parse(match.Groups["points"].Value);
                
                if (isNegative)
                {
                    number *= -1;
                }

                string reason = match.Groups["reason"].Value;
                string person = match.Groups["name"].Value;
                
                if (person.Equals(m.OriginalMessage.From.Name))
                {
                    // Give person -10 points
                    KudosData.StoreKudos(-10, person);
                    return m.CreateReply($"-10 points to {person} for trying to give themself kudos.");
                }

                // Something wonky is happening here with this always returning 0.
                // Possible mismach between @name and name.
                // TODO: Re-Enable this.
                //if (m.OriginalMessage.Participants.Where(p => p.Name.Equals(person)).Count() == 0)
                //{
                //    return m.CreateReply($"Can't give points to {person}, they aren't in this channel!");
                //}

                KudosData.StoreKudos(number, person);

                // Remove quotes from reason if it starts and ends with quotes
                reason = reason.Trim();
                if (reason.StartsWith("\"") && reason.EndsWith("\""))
                {
                    reason = reason.Substring(1, reason.Length - 2);
                }

                return m.CreateReply($"Giving @{person} {number} points for \"{reason}\"");
            }

            return null;
        }

    }
}