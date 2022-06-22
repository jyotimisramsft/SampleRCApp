using RcActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace ConsoleClientForRc
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var res = GetAllAsync().GetAwaiter().GetResult();
                var myActor = GetActor("Orderlord");
                //var task = myActor.GetCountAsync(new CancellationToken());
                var task = Task.Run(async () => await myActor.GetCountAsync(new CancellationToken())).ConfigureAwait(false);
                var result = task.GetAwaiter().GetResult();

                Thread.Sleep(10000);
                var task2 = Task.Run(async () => await myActor.GetCountAsync(new CancellationToken())).ConfigureAwait(false);
                var result2 = task2.GetAwaiter().GetResult();

                //var nameTask = Task.Run(async () => await myActor.GetNameAsync(new CancellationToken())).ConfigureAwait(false);
                //var name = nameTask.GetAwaiter().GetResult();

                //var arrTask = Task.Run(async () => await myActor.GetByteArrayAsync(new CancellationToken())).ConfigureAwait(false);
                //var arr = arrTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task<string> GetAllAsync()
        {
            ActorId partitionKey = new ActorId("1507609b-69f5-452c-bd3d-cc2f9850e058");
            var actorServiceProxy = ActorServiceProxy.Create(new Uri("fabric:/MigrationTestApp/RcActorService"), partitionKey);

            ContinuationToken continuationToken = null;
            CancellationToken cancellationToken = new CancellationToken(false);
            List<ActorInformation> activeActors = new List<ActorInformation>();
            string allActorId = "";
            string allMarker = "";

            do
            {
                PagedResult<ActorInformation> page = await actorServiceProxy.GetActorsAsync(continuationToken, cancellationToken);
                activeActors.AddRange(page.Items);
                continuationToken = page.ContinuationToken;
                allMarker += continuationToken == null ? "" : continuationToken.Marker;
            }
            while (continuationToken != null);

            for (int i = 0; i < activeActors.Count; i++)
            {
                allActorId += activeActors[i].ActorId.ToString() + "\n";
            }
            return activeActors.Count().ToString() + allMarker + "\n\n\n\n" + allActorId;
        }

        private static IRcActor GetActor(string userId)
        {
            return ActorProxy.Create<IRcActor>(
                new ActorId(userId),
                new Uri("fabric:/MigrationTestApp/RcActorService"));
        }
    }
}
