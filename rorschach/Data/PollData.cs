using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rorschach.Data
{
    public class Poll : TableEntity, IComparable<Poll>
    {
        public Guid PollGuid { get; set; }
        public List<Vote> Votes { get; set; }

        public string Text { get; set; }

        public Poll(Guid guid, List<Vote> votes, string Text)
        {
            this.PollGuid = guid;
            this.Text = Text;
            this.Votes = votes;
            this.PartitionKey = "polls";
            this.RowKey = guid.ToString();
        }

        public Poll()
        {
        }

        public int CompareTo(Poll other)
        {
            return this.PollGuid.CompareTo(other.PollGuid);
        }
    }

    public class ActivePoll : TableEntity, IComparable<ActivePoll>
    {
        public Guid ActivePollGuid { get; set; }

        public ActivePoll(Guid guid)
        {
            this.ActivePollGuid = guid;
            this.PartitionKey = "activepoll";
            this.RowKey = "active";
        }

        // Since we only want to store one active poll, 
        // always consider them equal.
        public int CompareTo(ActivePoll other)
        {
            return 0;
        }
    }

    public class Vote
    {
        public string Voter { get; set; }
        public string Choice { get; set; }

        public Vote() { }

        public Vote(string voter, string choice)
        {
            this.Voter = voter;
            this.Choice = choice;
        }
    }

    public class PollData
    {
        class ConnectionData
        {
            public CloudStorageAccount Account { get; private set; }
            public CloudTableClient TableClient { get; private set; }
            public CloudTable Table { get; private set; }

            public ConnectionData()
            {
                this.Account = CloudStorageAccount.Parse(rorschach.Properties.Settings.Default.StorageConnectionString);
                this.TableClient = this.Account.CreateCloudTableClient();
                this.Table = this.TableClient.GetTableReference("Polling");
                this.Table.CreateIfNotExists();
            }
        }

        public static bool StoreVote(string voter, string choice)
        {
            // Just strip @ by default. No names can have @ in them afaik
            voter = voter.Replace("@", "");

            ActivePoll active = GetActivePoll();
            if (active == null)
            {
                return false;
            }

            ConnectionData connection = new ConnectionData();

            TableOperation retrieveOperation = TableOperation.Retrieve<Poll>("polls", active.ActivePollGuid.ToString());
            TableResult result = connection.Table.Execute(retrieveOperation);

            Poll poll = (Poll)result.Result;
            try
            {
                if (poll == null)
                {
                    return false;
                }

                Vote foundVote = poll.Votes.FirstOrDefault(v => v.Voter.Equals(voter, StringComparison.OrdinalIgnoreCase));
                if (foundVote != default(Vote))
                {
                    foundVote.Choice = choice;
                }
                else
                {
                    poll.Votes.Add(new Vote(voter, choice));
                }

                TableOperation insertOrUpdateOperation = TableOperation.InsertOrReplace(poll);
                connection.Table.Execute(insertOrUpdateOperation);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Data);
                return false;
            }

            return true;
        }

        internal static Poll GetPoll(Guid activePollGuid)
        {
            throw new NotImplementedException();
        }

        public static ActivePoll GetActivePoll()
        {
            ConnectionData connection = new ConnectionData();

            TableOperation retrieveOperation = TableOperation.Retrieve<ActivePoll>("activepoll", "active");
            TableResult result = connection.Table.Execute(retrieveOperation);

            ActivePoll activePoll = (ActivePoll)result.Result;

            return activePoll;
        }

        public static bool SetActivePoll(Guid? guid)
        {
            try
            {
                var activePoll = GetActivePoll();

                if (guid == null)
                {
                    if (activePoll?.ActivePollGuid != null)
                    {
                        // Remove the active poll
                        TableOperation removeOperation = TableOperation.Delete(activePoll);
                        ConnectionData connection = new ConnectionData();
                        connection.Table.Execute(removeOperation);
                    }
                }
                else
                {
                    // Change the active guid
                    if (activePoll == null)
                    {
                        activePoll = new ActivePoll(guid.Value);
                    }
                    else
                    {
                        activePoll.ActivePollGuid = guid.Value;
                    }

                    TableOperation insertOrUpdate = TableOperation.InsertOrReplace(activePoll);
                    ConnectionData connection = new ConnectionData();
                    connection.Table.Execute(insertOrUpdate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Data);
                return false;
            }

            return true;
        }

        public static bool StartPoll(string Text)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return false;
            }

            if (GetActivePoll() == null)
            {
                Guid pollGuid = Guid.NewGuid();
                ConnectionData connection = new ConnectionData();

                Poll poll = new Poll(pollGuid, new List<Vote>(), Text);

                try
                {   
                    TableOperation insertOpertion = TableOperation.Insert(poll);
                    connection.Table.Execute(insertOpertion);
                    return SetActivePoll(poll.PollGuid);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Data);
                    return false;
                }
            }

            // Default to false if there is an active poll
            return false;
        }
    }
}