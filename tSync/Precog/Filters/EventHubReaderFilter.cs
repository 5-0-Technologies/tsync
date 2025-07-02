using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace tUtils.Filters.Input
{
    public class EventHubReaderFilter : InputChannelFilter<PartitionEvent>
    {
        private readonly EventHubConsumerClient eventHubConsumerClient;
        private readonly string connectionString;
        private readonly string consumerGroup;

        private IAsyncEnumerable<PartitionEvent> asyncEnumerable;
        private IAsyncEnumerator<PartitionEvent> asyncEnumerator;

        public EventHubReaderFilter(ChannelWriter<PartitionEvent> channelWriter, string connectionString, string consumerGroup) : base(channelWriter)
        {
            this.connectionString = connectionString;
            this.consumerGroup = consumerGroup;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or whitespace.", nameof(connectionString));
            }

            eventHubConsumerClient = new EventHubConsumerClient(consumerGroup, connectionString);
        }

        public override async Task Loop()
        {
            if (await asyncEnumerator.MoveNextAsync())
            {
                var value = asyncEnumerator.Current;
                await Writer.WriteAsync(value, cancellationTokenSource.Token);
            }
        }

        protected override void AfterRun()
        {
            base.AfterRun();
        }

        protected override void BeforeRun()
        {
            base.BeforeRun();
            asyncEnumerable = eventHubConsumerClient.ReadEventsAsync(false, cancellationToken: cancellationTokenSource.Token);
            asyncEnumerator = asyncEnumerable.GetAsyncEnumerator(cancellationTokenSource.Token);
        }
    }
}
