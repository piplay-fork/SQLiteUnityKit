using System;

namespace LiteKit.Data
{
	public interface IDbConnection : IDisposable
	{
		//
		// Properties
		//
		string ConnectionString
		{
			get;
			set;
		}

		int ConnectionTimeout
		{
			get;
		}

		string Database
		{
			get;
		}

		ConnectionState State
		{
			get;
		}

		//
		// Methods
		//
		IDbTransaction BeginTransaction( IsolationLevel il );

		IDbTransaction BeginTransaction();

		void ChangeDatabase( string databaseName );

		void Close();

		IDbCommand CreateCommand();

		void Open();
	}
}