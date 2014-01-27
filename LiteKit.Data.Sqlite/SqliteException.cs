using System;

namespace LiteKit.Data.Sqlite
{
	public class SqliteException : Exception
	{
		public SqliteException( string message ) : base( message )
		{
		}
	}
}