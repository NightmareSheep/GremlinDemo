using ExRam.Gremlinq.Core;
using Microsoft.Azure.Cosmos;

namespace GremlinDemo
{
    internal class Program
    {
        private static readonly string PrimaryKey = "GMZOHqqdFWZIah1SZ66kFqtB8QRkewOPCf6geGpltl9xb1sxuJk61oKo9p2bmhZtRq9rjGwsp2Y2cnKHNhQGGA==";
        private static readonly string Database = "graph-database";
        private static readonly string Graph = "sample-graph";
        private static readonly string PartitionKeyPath = "/PartitionKey";
        private static readonly string GremlinEndpoint = "wss://gremlin-experiment.gremlin.cosmos.azure.com:443/";
        private static readonly string CosmosConnectionString = "AccountEndpoint=https://gremlin-experiment.documents.azure.com:443/;AccountKey=GMZOHqqdFWZIah1SZ66kFqtB8QRkewOPCf6geGpltl9xb1sxuJk61oKo9p2bmhZtRq9rjGwsp2Y2cnKHNhQGGA==;ApiKind=Gremlin;";

        private static async Task Main(string[] args)
        {
            // Create database and graph
            CosmosClient? client = new CosmosClient(CosmosConnectionString);
            DatabaseResponse? response = await client.CreateDatabaseIfNotExistsAsync(Database);
            Database? db = response.Database;
            await db.CreateContainerIfNotExistsAsync(Graph, PartitionKeyPath);

            // Prepare query engine
            IGremlinQuerySource? g =
                GremlinQuerySource.g.
                ConfigureEnvironment(env => env
                .UseModel(GraphModel.FromBaseTypes<Vertex, Edge>(lookup => lookup
                .IncludeAssembliesOfBaseTypes()))
                .UseCosmosDb(builder => builder
                .At(new Uri(GremlinEndpoint), Database, Graph)
                .AuthenticateBy(PrimaryKey)
                ));

            // Clean graph
            await g.E().Drop();
            await g.V().Drop();

            // We create the graph:
            // client1 <- location1 <- distribution -> location2 -> client2

            // Add vertices
            Client? client1 = await g.AddV(new Client("Client 1", 0, 0)).FirstAsync();
            Location? location1 = await g.AddV(new Location(1, 0)).FirstAsync();
            DistributionBox? distribution = await g.AddV(new DistributionBox(10, 2, 0)).FirstAsync();
            Location? location2 = await g.AddV(new Location(3, 0)).FirstAsync();
            Client? client2 = await g.AddV(new Client("Client 2", 0, 0)).FirstAsync();

            // Add edges
            Cable? cable1 = await g.V(location1.Id).AddE(new Cable(1)).To(query => query.V(client1.Id)).FirstAsync();
            Cable? cable2 = await g.V(distribution.Id).AddE(new Cable(1)).To(query => query.V(location1.Id)).FirstAsync();
            Cable? cable3 = await g.V(distribution.Id).AddE(new Cable(1)).To(query => query.V(location2.Id)).FirstAsync();
            Cable? cable4 = await g.V(location2.Id).AddE(new Cable(1)).To(query => query.V(client2.Id)).FirstAsync();

            // Run query
            Client[]? clients = await g.V(distribution.Id).Repeat(v => v.Out()).Emit().OfType<Client>();
            Console.WriteLine("Find clients connected to distribution");
            Console.WriteLine("Clients:");
            foreach (Client? c in clients)
            {
                Console.WriteLine(c.Name);
            }
            Console.WriteLine("");
            Console.WriteLine("done");
        }
    }
}