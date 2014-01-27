using System;

namespace LiteKit.Data.Sqlite
{
	public class SqliteCommand : IDbCommand, ICloneable
	{
		internal SqliteConnection Parent { get; private set; }
		internal IntPtr NativeConnection { get; private set; }

		public string CommandText { get; set; }

		public int CommandTimeout
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public CommandType CommandType
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public IDbConnection Connection
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public IDataParameterCollection Parameters
		{
			get;
			private set;
		}

		public IDbTransaction Transaction
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public UpdateRowSource UpdatedRowSource
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		internal SqliteCommand( SqliteConnection sqliteConnection )
		{
			Parent = sqliteConnection;
			NativeConnection = sqliteConnection.NativeConnection;
			Parameters = new SqliteDataParameterCollection( this );
		}

		public void Cancel()
		{
			throw new NotImplementedException();
		}

		public IDbDataParameter CreateParameter()
		{
			return new SqliteDbDataParameter( this );
		}

		public int ExecuteNonQuery()
		{
			return ExecuteNonQuery( CommandText );
		}

		int ExecuteNonQuery( string commandText )
		{
			using ( var reader = ExecuteReader( CommandBehavior.SingleResult | CommandBehavior.SingleRow, commandText ) )
			{
				while ( reader.NextResult() )
				{
				}
				return reader.RecordsAffected;
			}
		}

		public IDataReader ExecuteReader()
		{
			return ExecuteReader( CommandBehavior.Default );
		}

		public IDataReader ExecuteReader( CommandBehavior behavior )
		{
			return ExecuteReader( behavior, CommandText );
		}

		IDataReader ExecuteReader( CommandBehavior behavior, string commandText )
		{
			var commands = commandText.Split( new[]{ ';' }, StringSplitOptions.RemoveEmptyEntries );
			for ( int i = 0; i < commands.Length - 1; ++i )
			{
				ExecuteNonQuery( commands[i] );
			}

			commandText = commands[commands.Length - 1];

			IntPtr nativeCommand;
			var retVal = UnsafeNativeMethods.Prepare( NativeConnection, commandText, commandText.Length, out nativeCommand, IntPtr.Zero );
			if ( retVal != UnsafeNativeMethods.ResultCode.OK )
				throw new SqliteException( SqliteConnection.GetError( NativeConnection ) + " - " + retVal );

			( (SqliteDataParameterCollection) Parameters ).Apply( nativeCommand );
			return new SqliteDataReader( this, nativeCommand );
		}

		public object ExecuteScalar()
		{
			return ExecuteScalar( CommandText );
		}

		object ExecuteScalar( string commandText )
		{
			using ( var reader = ExecuteReader( CommandBehavior.SingleResult | CommandBehavior.SingleRow, commandText ) )
			{
				if ( reader.Read() )
					return reader[0];
			}
			return null;
		}

		public void Prepare()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}
	}
}