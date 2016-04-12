using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rorschach.Data
{
    public class Kudo : TableEntity, IComparable<Kudo>
    {
        public Kudo(string name, int points)
        {
            this.Name = name;
            this.Points = points;
            this.PartitionKey = "scores";
            this.RowKey = this.Name;
        }

        public Kudo() { }

        public string Name { get; set; }
        public int Points { get; set; }

        public int CompareTo(Kudo other)
        {
            if (other.Points != this.Points)
            {
                return this.Points.CompareTo(other.Points) * -1;
            }
            else
            {
                return this.Name.CompareTo(other.Name);
            }
        }
    }

    public class KudosData
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
                this.Table = this.TableClient.GetTableReference("kudos");
                this.Table.CreateIfNotExists();
            }
        }

        public static void StoreKudos(int points, string person)
        {
            // Just strip @ by default. No names can have @ in them afaik
            person = person.Replace("@", "");

            // Parse the connection string and return a reference to the storage account.
            ConnectionData connectionData = new ConnectionData();

            

            TableOperation retrieveOperation = TableOperation.Retrieve<Kudo>("scores", person);
            TableResult result = connectionData.Table.Execute(retrieveOperation);

            Kudo kudo = (Kudo)result.Result;

            try
            {
                if (kudo != null)
                {
                    kudo.Points += points;

                    TableOperation insertOrUpdateOperation = TableOperation.InsertOrReplace(kudo);
                    connectionData.Table.Execute(insertOrUpdateOperation);
                }
                else
                {
                    kudo = new Kudo(person, points);

                    TableOperation insertOpertion = TableOperation.Insert(kudo);
                    connectionData.Table.Execute(insertOpertion);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Data);
                return;
            }
        }

        public static Kudo[] GetAllKudos()
        {
            ConnectionData connectionData = new ConnectionData();
            TableQuery<Kudo> query = new TableQuery<Kudo>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "scores"));
            return connectionData.Table.ExecuteQuery(query).ToArray();
        }
    }
}