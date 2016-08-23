using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using rorschach.Data;
using System.Text;

namespace rorschach.Actions
{
    public class Polling : IBotAction
    {
        public string GetCommandString()
        {
            return "Poll";
        }

        public string HelpMessage()
        {
            return "start poll <poll question>\nstop poll\nvote <vote text>\nactive poll";
        }

        public Message ParseMessage(MessageWrapper m)
        {

            Message message = HandleMessage(m, "^start poll (?<poll>.+)", (Match match) =>
            {
                bool success = PollData.StartPoll(match.Groups["poll"]?.Value);

                if (success)
                {
                    return m.CreateReply($"Created new poll with the question: {match.Groups["poll"]}");
                }
                else
                {
                    return m.CreateReply($"Unable to create new poll. Maybe one is already running? Or you gave a bull shit poll question");
                }
            });

            if (message != null)
            {
                return message;
            }

            message = HandleMessage(m, "^stop poll$", (Match match) =>
            {
                var active = PollData.GetActivePoll();
                bool success = PollData.SetActivePoll(null);
                if (success)
                {
                    Poll activePoll = PollData.GetPoll(active.ActivePollGuid);
                    return m.CreateReply($"Stopped the poll.\n\n{OutputPoll(activePoll)}");
                }
                else
                {
                    return m.CreateReply($"Go fuck yourself");
                }
            });

            if (message != null)
            {
                return message;
            }

            message = HandleMessage(m, "^vote (?<vote>.+)", (Match match) =>
            {
                bool success = PollData.StoreVote(m.OriginalMessage.From.Name, match.Groups["vote"]?.Value);

                if (success)
                {
                    return m.CreateReply("Submitted your vote");
                }
                else
                {
                    return m.CreateReply("I'm sorry, I can't let you do that Dave.");
                }
            });

            if (message != null)
            {
                return message;
            }

            message = HandleMessage(m, "^active poll$", (Match match) =>
            {
                ActivePoll active = PollData.GetActivePoll();

                if (active == null)
                {
                    return m.CreateReply("No active poll at this moment");
                }
                else
                {
                    Poll poll = PollData.GetPoll(active.ActivePollGuid);
                    return m.CreateReply(OutputPoll(poll));
                }
            });

            return message;
        }

        private Message HandleMessage(MessageWrapper message, string pattern, Func<Match, Message> action)
        {
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(message.StrippedText);
            if (m.Success)
            {
                return action.Invoke(m);
            }

            return null;
        }

        private string OutputPoll(Poll p)
        {
            // Get votes and counts
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (var vote in p.Votes)
            {
                string key = vote.Choice.ToLower();
                if (counts.ContainsKey(key))
                {
                    counts[key] = counts[key] + 1;
                }
                else
                {
                    counts[key] = 1;
                }
            }

            // Transform into an ordered keyvalue pairing
            List<Tuple<string, int>> list = new List<Tuple<string, int>>();
            foreach (string key in counts.Keys)
            {
                Tuple<string, int> t = Tuple.Create<string, int>(key, counts[key]);
                list.Add(t);
            }

            var ordered = list.OrderByDescending(t => t.Item2);


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine($"Active poll question: {p.Text}");
            sb.AppendLine($"");

            foreach (var t in ordered)
            {
                sb.AppendLine($"    \"{t.Item1}\" : {t.Item2}");
            }

            sb.AppendLine("```");

            return sb.ToString();
        }
    }
}