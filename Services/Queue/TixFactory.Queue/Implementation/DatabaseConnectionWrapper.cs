using System;
using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;
using TixFactory.Configuration;

namespace TixFactory.Queue
{
	internal class DatabaseConnectionWrapper
	{
		private readonly IReadOnlySetting<string> _ConnectionString;
		private readonly ILazyWithRetry<MySqlConnection> _ConnectionLazy;

		public SemaphoreSlim ConnectionLock { get; }

		public MySqlConnection Connection => _ConnectionLazy.Value;

		public DatabaseConnectionWrapper(IReadOnlySetting<string> connectionString)
		{
			_ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
			_ConnectionLazy = new LazyWithRetry<MySqlConnection>(BuildConnection);
			ConnectionLock = new SemaphoreSlim(1, 1);
		}

		private MySqlConnection BuildConnection()
		{
			var connectionString = _ConnectionString.Value;
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
					_ConnectionLazy.Refresh();
					return;
			}
		}
	}
}
