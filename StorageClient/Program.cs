using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StorageClient
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudStorageAccount storageAccount = 
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            var refTable = tableClient.GetTableReference("MatchInfo");

        }
    }
}
