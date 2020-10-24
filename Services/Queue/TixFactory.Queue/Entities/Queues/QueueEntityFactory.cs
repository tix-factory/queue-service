using System;
using System.Collections.Concurrent;
using System.Linq;
using MySql.Data.MySqlClient;
using TixFactory.Collections;
using TixFactory.Configuration;

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

		public Queue GetOrCreateQueue(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
			}

			if (name.Length > EntityValidation.MaxQueueNameLength)
			{
				throw new ArgumentException($"{nameof(name)} cannot be longer than {EntityValidation.MaxQueueNameLength}", nameof(name));
			}

			var queue = GetQueueByName(name);
			if (queue != null)
			{
				return queue;
			}

			var queueId = _DatabaseConnection.ExecuteInsertStoredProcedure<long>(_InsertQueueStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			});

			_QueuesByName.Remove(name);

			return GetQueueByName(name);
		}

		public Queue GetQueueByName(string name)
		{
			if (string.IsNullOrWhiteSpace(name) || name.Length > EntityValidation.MaxQueueNameLength)
			{
				return null;
			}

			if (_QueuesByName.TryGetValue(name, out var queue))
			{
				return queue;
			}

			var queues = _DatabaseConnection.ExecuteReadStoredProcedure<Queue>(_GetQueueByNameStoredProcedureName, new[]
			{
				new MySqlParameter("@_Name", name)
			});

			queue = _QueuesByName[name] = queues.FirstOrDefault();
			return queue;
		}

		public void DeleteQueue(long id)
		{
			_DatabaseConnection.ExecuteWriteStoredProcedure(_DeleteQueueStoredProcedureName, new[]
			{
				new MySqlParameter(@"_ID", id)
			});

			foreach (var queue in _QueuesByName.Values)
			{
				if (queue.Id == id)
				{
					_QueuesByName.Remove(queue.Name);
					return;
				}
			}
		}
	}
}
