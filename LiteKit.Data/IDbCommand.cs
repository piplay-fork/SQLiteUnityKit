using System;

namespace LiteKit.Data
{
	public interface IDbCommand : IDisposable
	{
		//
		// Properties
		//
		string CommandText
		{
			get;
			set;
		}

		int CommandTimeout
		{
			get;
			set;
		}

		CommandType CommandType
		{
			get;
			set;
		}

		IDbConnection Connection
		{
			get;
			set;
		}

		IDataParameterCollection Parameters
		{
			get;
		}

		IDbTransaction Transaction
		{
			get;
			set;
		}

		UpdateRowSource UpdatedRowSource
		{
			get;
			set;
		}

		//
		// Methods
		//
		void Cancel();

		IDbDataParameter CreateParameter();

		int ExecuteNonQuery();

		IDataReader ExecuteReader();

		IDataReader ExecuteReader( CommandBehavior behavior );

		object ExecuteScalar();

		void Prepare();
	}
}