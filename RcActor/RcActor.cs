using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime.Migration;
using RcActor.Interfaces;

namespace RcActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>

    [StateMigration(StateMigration.Target)]
    [StatePersistence(StatePersistence.Persisted)]
    internal class RcActor : Actor, IRcActor, IRemindable
    {
        /// <summary>
        /// Initializes a new instance of RcActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public RcActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        public Task<byte[]> GetByteArrayAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<byte[]>("array", cancellationToken);
        }

        public Task<string> GetNameAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<string>("name", cancellationToken);
        }

        public Task SetByteArrayAsync(byte[] byteArray, CancellationToken cancellationToken)
        {
            return this.StateManager.AddOrUpdateStateAsync("array", byteArray, (key, value) => byteArray, cancellationToken);
        }

        public Task SetNameAsync(string name, CancellationToken cancellationToken)
        {
            return this.StateManager.AddOrUpdateStateAsync("name", name, (key, value) => name, cancellationToken);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName.Equals("Increment count"))
            {
                var count = await this.StateManager.GetStateAsync<int>("count", new CancellationToken());
                count += 5;
                await this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, new CancellationToken());
            }
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        //protected override Task OnActivateAsync()
        //{
            //ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            //return this.StateManager.TryAddStateAsync("count", 0);
        //}

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> IRcActor.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task IRcActor.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }
    }
}
