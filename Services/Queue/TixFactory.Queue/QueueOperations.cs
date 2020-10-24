using System;
using System.Data;
using MySql.Data.MySqlClient;
using TixFactory.Configuration;
using TixFactory.Logging;
using TixFactory.Operations;
using TixFactory.Queue.Entities;
using ISettings = TixFactory.Queue.Service.ISettings;

namespace TixFactory.Queue
{
	public class QueueOperations : IQueueOperations
	{
		private readonly ISettings _Settings;
		private readonly ILazyWithRetry<MySqlConnection> _MySqlConnection;

		public IOperation<AddQueueItemRequest, AddQueueItemResult> AddQueueItemOperation { get; }
		public IOperation<string, int> ClearQueueOperation { get; }

		public QueueOperations(ILogger logger, ISettings settings)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			_Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			var mySqlConnection = _MySqlConnection = new LazyWithRetry<MySqlConnection>(BuildConnection);
			var databaseConnection = new DatabaseConnection(mySqlConnection);
			var queueEntityFactory = new QueueEntityFactory(databaseConnection);
			var queueItemEntityFactory = new QueueItemEntityFactory(databaseConnection, queueEntityFactory);

			AddQueueItemOperation = new AddQueueItemOperation(queueItemEntityFactory);
			ClearQueueOperation = new ClearQueueOperation(queueItemEntityFactory);
		}

		private MySqlConnection BuildConnection()
		{
			var connectionString = _Settings.QueueConnectionString;
			var connection = new MySqlConnection(connectionString);
			connection.StateChange += ConnectionStateChange;
			connection.Open();

			return connection;
		}

		private void ConnectionStateChange(object sender, StateChangeEventArgs e)
		{
			switch (e.CurrentState)
			{
				case ConnectionState.Broken:
				case ConnectionState.Closed:
					_MySqlConnection.Refresh();
					return;
			}
		}
	}
}
