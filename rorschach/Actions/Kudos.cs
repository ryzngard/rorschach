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
        public Message ParseMessage(MessageWrapper m)
        {
            // Check the text for the correct pattern
            //const String pat = "^(?<negative>-{0,1})(?<points>[0-9]+)\\s@(?<name>\\w)\\s\"(?<reason>[\\w])\"$";
            const String pat = "^(?<negative>-{0,1})(?<points>[0-9]+)\\s@{0,1}(?<name>[\\w]+)\\s(?<reason>.+)";
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);

            Match match = r.Match(m.StrippedText);
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

                KudosData.StoreKudos(number, person);

                return m.CreateReply($"Giving @{person} {number} points for \"{reason}\"");
            }

            return null;
        }

    }
}