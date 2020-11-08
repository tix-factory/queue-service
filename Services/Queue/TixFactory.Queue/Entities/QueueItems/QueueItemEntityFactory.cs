using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TixFactory.Data.MySql;

namespace TixFactory.Queue.Entities
{
	internal class QueueItemEntityFactory : IQueueItemEntityFactory
	{
		private const string _InsertQueueItemStoredProcedureName = "InsertQueueItem";
		private const string _GetQueueSizeStoredProcedureName = "GetQueueSize";
		private const string _GetHeldQueueSizeStoredProcedureName = "GetHeldQueueSize";
		private const string _ReleaseQueueItemStoredProcedureName = "ReleaseQueueItem";
		private const string _LeaseQueueItemStoredProcedureName = "LeaseQueueItem";
		private const string _ClearQueueStoredProcedureName = "ClearQueue";
		private const string _DeleteQueueItemStoredProcedureName = "DeleteQueueItem";
		private readonly IDatabaseConnection _DatabaseConnection;
		private readonly IQueueEntityFactory _QueueEntityFactory;

		public QueueItemEntityFactory(IDatabaseConnection databaseConnection, IQueueEntityFactory queueEntityFactory)
		{
			_DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
			_QueueEntityFactory = queueEntityFactory ?? throw new ArgumentNullException(nameof(queueEntityFactory));
		}

		public async Task CreateQueueItem(string queueName, string data, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(data));
			}

			if (string.IsNullOrWhiteSpace(data) || data.Length > EntityValidation.MaxQueueDataLength)
			{
				throw new ArgumentException($"Value exceeds max length ({EntityValidation.MaxQueueDataLength})", nameof(data));
			}

			var queue = await _QueueEntityFactory.GetOrCreateQueue(queueName, cancellationToken).ConfigureAwait(false);

			await _DatabaseConnection.ExecuteInsertStoredProcedureAsync<long>(_InsertQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id),
				new MySqlParameter("@_Data", data)
			}, cancellationToken).ConfigureAwait(false);
		}

		public async Task<QueueItem> LeaseQueueItem(string queueName, TimeSpan leaseExpiry, CancellationToken cancellationToken)
		{
			var queue = await _QueueEntityFactory.GetQueueByName(queueName, cancellationToken).ConfigureAwait(false);
			if (queue == null)
			{
				return null;
			}

			var queueItems = await _DatabaseConnection.ExecuteReadStoredProcedureAsync<QueueItem>(_LeaseQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id),
				new MySqlParameter("@_LockExpiration", DateTime.UtcNow + leaseExpiry)
			}, cancellationToken).ConfigureAwait(false);

			return queueItems.FirstOrDefault();
		}

		public async Task<long> GetQueueSize(string queueName, CancellationToken cancellationToken)
		{
			var queue = await _QueueEntityFactory.GetQueueByName(queueName, cancellationToken).ConfigureAwait(false);
			if (queue == null)
			{
				return 0;
			}

			return await _DatabaseConnection.ExecuteCountStoredProcedureAsync(_GetQueueSizeStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id)
			}, cancellationToken).ConfigureAwait(false);
		}

		public async Task<long> GetHeldQueueSize(string queueName, CancellationToken cancellationToken)
		{
			var queue = await _QueueEntityFactory.GetQueueByName(queueName, cancellationToken).ConfigureAwait(false);
			if (queue == null)
			{
				return 0;
			}

			return await _DatabaseConnection.ExecuteCountStoredProcedureAsync(_GetHeldQueueSizeStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id)
			}, cancellationToken).ConfigureAwait(false);
		}

		public async Task<int> ClearQueue(string queueName, CancellationToken cancellationToken)
		{
			var queue = await _QueueEntityFactory.GetQueueByName(queueName, cancellationToken).ConfigureAwait(false);
			if (queue == null)
			{
				return 0;
			}

			return await _DatabaseConnection.ExecuteWriteStoredProcedureAsync(_ClearQueueStoredProcedureName, new[]
			{
				new MySqlParameter("@_QueueID", queue.Id)
			}, cancellationToken).ConfigureAwait(false);
		}

		public async Task<bool> ReleaseQueueItem(long id, Guid holderId, CancellationToken cancellationToken)
		{
			var rowsModified = await _DatabaseConnection.ExecuteWriteStoredProcedureAsync(_ReleaseQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_ID", id),
				new MySqlParameter("@_HolderID", holderId.ToString())
			}, cancellationToken).ConfigureAwait(false);

			return rowsModified == 1;
		}

		public async Task<bool> DeleteQueueItem(long id, Guid holderId, CancellationToken cancellationToken)
		{
			var rowsModified = await _DatabaseConnection.ExecuteWriteStoredProcedureAsync(_DeleteQueueItemStoredProcedureName, new[]
			{
				new MySqlParameter("@_ID", id),
				new MySqlParameter("@_HolderID", holderId.ToString())
			}, cancellationToken).ConfigureAwait(false);

			return rowsModified == 1;
		}
	}
}
