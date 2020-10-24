using System;
using System.Linq;
using MySql.Data.MySqlClient;

namespace TixFactory.Queue.Entities
{
	internal class QueueItemEntityFactory : IQueueItemEntityFactory
	{
		private const string _InsertQueueItemStoredProcedureName = "InsertQueueItem";
		private const string _GetQueueSizeStoredProcedureName = "GetQueueSize";
		private const string _ReleaseQueueItemStoredProcedureName = "ReleaseQueueItem";
		private const string _LeaseQueueItemStoredProcedureName = "LeaseQueueItem";
		private const string _DeleteQueueItemStoredProcedureName = "DeleteQueueItem";
		private readonly IDatabaseConnection _DatabaseConnection;
		private readonly IQueueEntityFactory _QueueEntityFactory;

		public QueueItemEntityFactory(IDatabaseConnection databaseConnection, IQueueEntityFactory queueEntityFactory)
		{
			_DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
			_QueueEntityFactory = queueEntityFactory ?? throw new ArgumentNullException(nameof(queueEntityFactory));
		}

		public void CreateQueueItem(string queueName, string data)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(data));
			}

			if (string.IsNullOrWhiteSpace(data) || data.Length > EntityValidation.MaxQueueDataLength)
			{
				throw new ArgumentException($"Value exceeds max length ({EntityValidation.MaxQueueDataLength})", nameof(data));
			}

			var queue = _QueueEntityFactory.GetOrCreateQueue(queueName);

			_DatabaseConnection.ExecuteInsertStoredProcedure<long>(_InsertQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id),
				new MySqlParameter("@_Data", data)
			});
		}

		public QueueItem LeaseQueueItem(string queueName, TimeSpan leaseExpiry)
		{
			var queue = _QueueEntityFactory.GetQueueByName(queueName);
			if (queue == null)
			{
				return null;
			}

			var queueItems = _DatabaseConnection.ExecuteReadStoredProcedure<QueueItem>(_LeaseQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id),
				new MySqlParameter("@_LockExpiration", DateTime.UtcNow + leaseExpiry)
			});

			return queueItems.FirstOrDefault();
		}

		public long GetQueueSize(string queueName)
		{
			var queue = _QueueEntityFactory.GetQueueByName(queueName);
			if (queue == null)
			{
				return 0;
			}

			var countResults = _DatabaseConnection.ExecuteReadStoredProcedure<CountResult>(_GetQueueSizeStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id)
			});

			var count = countResults.FirstOrDefault();
			if (count == null)
			{
				return 0;
			}

			return count.Count;
		}

		public bool ReleaseQueueItem(long id, Guid holderId)
		{
			var rowsModified = _DatabaseConnection.ExecuteWriteStoredProcedure(_ReleaseQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_ID", id),
				new MySqlParameter("@_HolderID", holderId.ToString())
			});

			return rowsModified == 1;
		}

		public bool DeleteQueueItem(long id, Guid holderId)
		{
			var rowsModified = _DatabaseConnection.ExecuteWriteStoredProcedure(_DeleteQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_ID", id),
				new MySqlParameter("@_HolderID", holderId.ToString())
			});

			return rowsModified == 1;
		}
	}
}
