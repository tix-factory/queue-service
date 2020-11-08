using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TixFactory.Collections;
using TixFactory.Configuration;
using TixFactory.Data.MySql;

namespace TixFactory.Queue.Entities
{
	internal class QueueEntityFactory : IQueueEntityFactory
	{
		private const string _InsertQueueStoredProcedureName = "InsertQueue";
		private const string _GetQueueByNameStoredProcedureName = "GetQueueByName";
		private const string _DeleteQueueStoredProcedureName = "DeleteQueue";
		private readonly TimeSpan _QueueCacheExpiry = TimeSpan.FromMinutes(1);
		private readonly IDatabaseConnection _DatabaseConnection;
		private readonly ExpirableDictionary<string, Queue> _QueuesByName;

		public QueueEntityFactory(IDatabaseConnection databaseConnection)
		{
			_DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
			_QueuesByName = new ExpirableDictionary<string, Queue>(
				dictionary: new ConcurrentDictionary<string, Queue>(StringComparer.OrdinalIgnoreCase),
				valueExpiration: new Setting<TimeSpan>(_QueueCacheExpiry),
				expirationPolicy: new Setting<ExpirationPolicy>(ExpirationPolicy.RenewOnRead));
		}

		public async Task<Queue> GetOrCreateQueue(string name, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
			}

			if (name.Length > EntityValidation.MaxQueueNameLength)
			{
				throw new ArgumentException($"{nameof(name)} cannot be longer than {EntityValidation.MaxQueueNameLength}", nameof(name));
			}

			var queue = await GetQueueByName(name, cancellationToken).ConfigureAwait(false);
			if (queue != null)
			{
				return queue;
			}

			var queueId = await _DatabaseConnection.ExecuteInsertStoredProcedureAsync<long>(_InsertQueueStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			}, cancellationToken).ConfigureAwait(false);

			_QueuesByName.Remove(name);

			return await GetQueueByName(name, cancellationToken).ConfigureAwait(false);
		}

		public async Task<Queue> GetQueueByName(string name, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(name) || name.Length > EntityValidation.MaxQueueNameLength)
			{
				return null;
			}

			if (_QueuesByName.TryGetValue(name, out var queue))
			{
				return queue;
			}

			var queues = await _DatabaseConnection.ExecuteReadStoredProcedureAsync<Queue>(_GetQueueByNameStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			}, cancellationToken).ConfigureAwait(false);

			queue = _QueuesByName[name] = queues.FirstOrDefault();
			return queue;
		}

		public async Task DeleteQueue(Queue queue, CancellationToken cancellationToken)
		{
			await _DatabaseConnection.ExecuteWriteStoredProcedureAsync(_DeleteQueueStoredProcedureName, new[]
			{
				new MySqlParameter(@"_ID", queue.Id)
			}, cancellationToken).ConfigureAwait(false);

			_QueuesByName.Remove(queue.Name);
		}
	}
}
