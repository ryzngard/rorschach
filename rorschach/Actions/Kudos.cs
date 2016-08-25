using System;
using System.Text.RegularExpressions;
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

        public bool ParseMessage(ActivityWrapper wrapper)
        {
            // Check the text for the correct pattern
            const String pat = "^(?<negative>-{0,1})(\\+{0,1})(?<points>[0-9]+)\\s@{0,1}(?<name>[\\w]+)\\s(?<reason>.+)";
            const String pat2 = "^@{0,1}(?<name>[\\w]+)\\s(?<negative>-{0,1})(?<points>[0-9]+)\\s(?<reason>.+)";
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            Regex r2 = new Regex(pat2, RegexOptions.IgnoreCase);

            Match match = r.Match(wrapper.StrippedText);

            // Attempt to match with the alternative pattern
            if (!match.Success)
            {
                match = r2.Match(wrapper.StrippedText);
            }

            if (match.Success)
            {
                // See if number is negative
                bool isNegative = match.Groups["negative"].Success && !String.IsNullOrEmpty(match.Groups["negative"].Value);
                
                if (!match.Groups[1].Success)
                {
                    return false;
                }

                int number = int.Parse(match.Groups["points"].Value);
                
                if (isNegative)
                {
                    number *= -1;
                }

                string reason = match.Groups["reason"].Value;
                string person = match.Groups["name"].Value;
                
                if (person.Equals(wrapper.OriginalActivity.From.Name))
                {
                    // Give person -10 points
                    KudosData.StoreKudos(-10, person);
                    wrapper.SendReply($"-10 points to {person} for trying to give themself kudos.");
                    return true;
                }

                KudosData.StoreKudos(number, person);

                // Remove quotes from reason if it starts and ends with quotes
                reason = reason.Trim();
                if (reason.StartsWith("\"") && reason.EndsWith("\""))
                {
                    reason = reason.Substring(1, reason.Length - 2);
                }

                wrapper.SendReply($"Giving @{person} {number} points for \"{reason}\"");
                return true;
            }

            return false;
        }

    }
}