using System;
using TixFactory.Configuration;
using TixFactory.Data.MySql;
using TixFactory.Logging;
using TixFactory.Operations;
using TixFactory.Queue.Entities;
using ISettings = TixFactory.Queue.Service.ISettings;

namespace TixFactory.Queue
{
	public class QueueOperations : IQueueOperations
	{
		public IAsyncOperation<AddQueueItemRequest, AddQueueItemResult> AddQueueItemOperation { get; }
		public IAsyncOperation<string, int> ClearQueueOperation { get; }
		public IAsyncOperation<LeaseQueueItemRequest, QueueItemResult> LeaseQueueItemOperation { get; }
		public IAsyncOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult> ReleaseQueueItemOperation { get; }
		public IAsyncOperation<ReleaseQueueItemRequest, ReleaseQueueItemResult> RemoveQueueItemOperation { get; }
		public IAsyncOperation<string, long> GetQueueSizeOperation { get; }
		public IAsyncOperation<string, long> GetHeldQueueSizeOperation { get; }

		public QueueOperations(ILogger logger, ISettings settings)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			var connectionString = new ManufacturedSetting<string>(() => settings.QueueConnectionString, refreshOnRead: true);
			var databaseConnection = new DatabaseConnection(connectionString, logger);
			var queueEntityFactory = new QueueEntityFactory(databaseConnection);
			var queueItemEntityFactory = new QueueItemEntityFactory(databaseConnection, queueEntityFactory);

			AddQueueItemOperation = new AddQueueItemOperation(queueItemEntityFactory);
			ClearQueueOperation = new ClearQueueOperation(queueItemEntityFactory);
			LeaseQueueItemOperation = new LeaseQueueItemOperation(queueItemEntityFactory, logger);
			ReleaseQueueItemOperation = new ReleaseQueueItemOperation(queueItemEntityFactory);
			RemoveQueueItemOperation = new RemoveQueueItemOperation(queueItemEntityFactory);
			GetQueueSizeOperation = new GetQueueSizeOperation(queueItemEntityFactory);
			GetHeldQueueSizeOperation = new GetHeldQueueSizeOperation(queueItemEntityFactory);
		}
	}
}
