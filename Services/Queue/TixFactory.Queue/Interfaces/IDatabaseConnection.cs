using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TixFactory.Queue
{
	internal interface IDatabaseConnection
	{
		T ExecuteInsertStoredProcedure<T>(string storedProcedureName, IReadOnlyCollection<MySqlParameter> mySqlParameters);

		IReadOnlyCollection<T> ExecuteReadStoredProcedure<T>(string storedProcedureName, IReadOnlyCollection<MySqlParameter> mySqlParameters)
			where T : class;

		int ExecuteWriteStoredProcedure(string storedProcedureName, IReadOnlyCollection<MySqlParameter> mySqlParameters);
	}
}
