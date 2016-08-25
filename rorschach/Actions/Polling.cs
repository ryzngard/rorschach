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
            return "start poll <poll question>\n\nstop poll\n\nvote <vote text>\n\nactive poll";
        }

        public bool ParseMessage(ActivityWrapper wrapper)
        {

            bool handled = HandleMessage(wrapper, "^start poll (?<poll>.+)", (Match match) =>
            {
                bool success = PollData.StartPoll(match.Groups["poll"]?.Value);

                if (success)
                {
                    wrapper.SendReply($"Created new poll with the question: {match.Groups["poll"]}");
                }
                else
                {
                    wrapper.SendReply($"Unable to create new poll. Maybe one is already running? Or you gave a bull shit poll question");
                }
            });

            if (handled)
            {
                return handled;
            }

            handled = HandleMessage(wrapper, "^stop poll$", (Match match) =>
            {
                var active = PollData.GetActivePoll();
                bool success = PollData.SetActivePoll(null);
                if (success)
                {
                    Poll activePoll = PollData.GetPoll(active.ActivePollGuid);
                    wrapper.SendReply($"Stopped the poll.\n\n{OutputPoll(activePoll)}");
                }
                else
                {
                    wrapper.SendReply($"Go fuck yourself");
                }
            });

            if (handled)
            {
                return handled;
            }

            handled = HandleMessage(wrapper, "^vote (?<vote>.+)", (Match match) =>
            {
                bool success = PollData.StoreVote(wrapper.OriginalActivity.From.Name, match.Groups["vote"]?.Value);

                if (success)
                {
                    wrapper.SendReply("Submitted your vote");
                }
                else
                {
                    wrapper.SendReply("I'm sorry, I can't let you do that Dave.");
                }
            });

            if (handled)
            {
                return handled;
            }

            handled = HandleMessage(wrapper, "^active poll$", (Match match) =>
            {
                ActivePoll active = PollData.GetActivePoll();

                if (active == null)
                {
                    wrapper.SendReply("No active poll at this moment");
                }
                else
                {
                    Poll poll = PollData.GetPoll(active.ActivePollGuid);
                    wrapper.SendReply(OutputPoll(poll));
                }
            });

            return handled;
        }

        private bool HandleMessage(ActivityWrapper message, string pattern, Action<Match> action)
        {
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(message.StrippedText);
            if (m.Success)
            {
                action.Invoke(m);
                return true;
            }

            return false;
        }

        private string OutputPoll(Poll p)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```\n");
            sb.AppendLine($"Active poll question: {p.Text}\n");
            sb.AppendLine($"");

            var votes = PollData.GetVotesForPoll(p.PollGuid);
            if (votes != null)
            {
                // Get votes and counts
                Dictionary<string, List<string>> voteDict = new Dictionary<string, List<string>>();
                foreach (var vote in votes)
                {
                    string key = vote.Choice.ToLower();
                    if (voteDict.ContainsKey(key))
                    {
                        voteDict[key].Add(vote.Voter);
                    }
                    else
                    {
                        voteDict[key] = new List<string>(new string[] { vote.Voter });
                    }
                }

                // Transform into an ordered keyvalue pairing
                List<Tuple<string, int>> list = new List<Tuple<string, int>>();
                foreach (string key in voteDict.Keys)
                {
                    Tuple<string, int> t = Tuple.Create<string, int>(key, voteDict[key].Count);
                    list.Add(t);
                }

                var ordered = list.OrderByDescending(t => t.Item2);

                foreach (var t in ordered)
                {
                    sb.AppendLine($"    \"{t.Item1}\" : {t.Item2} : {string.Join("," ,voteDict[t.Item1])}\n");
                }
            }            

            sb.AppendLine("```");

            return sb.ToString();
        }
    }
}