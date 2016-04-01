using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace rorschach.Actions
{
    public class DiceRoller : IBotAction
    {
        // Use a static RNG to avoid seeding for each roll
        static RNGCryptoServiceProvider Gen = new RNGCryptoServiceProvider();
        static int GetRandomRoll(int size)
        {
            // Just base the number of bytes needed on the size of an int
            byte[] randomNumber = new byte[(sizeof(int)/8)+1];
            Gen.GetBytes(randomNumber);
            int rand = Convert.ToInt32(randomNumber[0]);
            return (rand % size) + 1;
        }

        public Message ParseMessage(MessageWrapper m)
        {
            if (m.StrippedText.ToLower().StartsWith("roll"))
            {
                int count = 1;
                int sides = 20;

                const String pat = @"roll (?<dice_count>[0-9]*)d(?<dice_size>[0-9]+)";
                Regex r = new Regex(pat, RegexOptions.IgnoreCase);

                Match match = r.Match(m.StrippedText);
                if (match.Success)
                {
                    

                    // The user provided a set of dice to roll
                    if (match.Groups["dice_count"].Success)
                    {
                        if (!int.TryParse(match.Groups["dice_count"].Value, out count))
                        {
                            return m.OriginalMessage.CreateReplyMessage($"Invalid dice count '{match.Groups["dice_count"]}' in text '{m.StrippedText}'");
                        }
                    }

                    if (match.Groups["dice_size"].Success)
                    {
                        if (!int.TryParse(match.Groups["dice_size"].Value, out sides))
                        {
                            return m.OriginalMessage.CreateReplyMessage($"Invalid dice size '{match.Groups["dice_size"]}' in '{m.StrippedText}");
                        }
                    }
                }

                if (count > 20)
                {
                    return m.OriginalMessage.CreateReplyMessage($"Go roll your own damn dice {m.OriginalMessage.From.Address}, I don't have time to roll {count} die for you!");
                }

                if (count == 1 && sides > 0)
                {
                    // Special case the text for 1 result 
                    return m.OriginalMessage.CreateReplyMessage($"Result from rolling {sides} sided dice: {GetRandomRoll(sides)}");
                }
                else if (count > 0 && sides > 0)
                {
                    string message = $"Rolling {count} {sides} sided dice: \n";
                    int total = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int result = GetRandomRoll(sides);
                        total += result;
                        string prefix = i > 0 ? "+" : "";
                        message += $"{prefix}[{result}]";
                    }

                    message += $"={total}";

                    return m.OriginalMessage.CreateReplyMessage(message);
                }
                else
                {
                    if (count <= 0)
                    {
                        return m.OriginalMessage.CreateReplyMessage("Cannot roll 0 dice, silly!");
                    }
                    else
                    {
                        return m.OriginalMessage.CreateReplyMessage("You get 0. You'll always get 0. You're worth nothing and your mother smells of elderberries");
                    }
                }
            }

            return null;
        }
    }
}