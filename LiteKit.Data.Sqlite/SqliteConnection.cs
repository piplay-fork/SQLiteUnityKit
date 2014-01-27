using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace LiteKit.Data.Sqlite
{
	public class SqliteConnection : IDbConnection
	{
		internal IntPtr NativeConnection { get; private set; }

		public string ConnectionString { get; set; }

		public ConnectionState State { get; private set; }
		
		public int ConnectionTimeout
		{
			get {
				throw new NotImplementedException();
			}
		}

		public string Database
		{
			get {
				throw new NotImplementedException();
			}
		}

		public SqliteConnection() : this( string.Empty )
		{
		}

		public SqliteConnection( string connectionString )
		{
			ConnectionString = connectionString;
			State = ConnectionState.Closed;
		}

		public static void CreateFile( string databaseFileName )
		{
			var dirName = Path.GetDirectoryName( databaseFileName );
			if ( string.IsNullOrEmpty( dirName ) == false && Directory.Exists( dirName ) == false )
				Directory.CreateDirectory( dirName );

			IntPtr conn;
			var retVal = UnsafeNativeMethods.Open( databaseFileName, out conn, UnsafeNativeMethods.OpenFlags.ReadWrite | UnsafeNativeMethods.OpenFlags.Create, null );
			if ( retVal != UnsafeNativeMethods.ResultCode.OK )
				throw new SqliteException( GetError( conn ) + " - " + retVal );
			UnsafeNativeMethods.Close( conn );
		}

		public IDbTransaction BeginTransaction( IsolationLevel il )
		{
			throw new NotImplementedException();
		}

		public IDbTransaction BeginTransaction()
		{
			throw new NotImplementedException();
		}

		public void ChangeDatabase( string databaseName )
		{
			throw new NotImplementedException();
		}

		public void Close()
		{
			if ( State != ConnectionState.Closed )
			{
				var retVal = UnsafeNativeMethods.Close( NativeConnection );
				if ( retVal != UnsafeNativeMethods.ResultCode.OK )
					throw new SqliteException( GetError( NativeConnection ) + " - " + retVal );
			}

			State = ConnectionState.Closed;
			NativeConnection = IntPtr.Zero;
		}

		public IDbCommand CreateCommand()
		{
			return new SqliteCommand( this );
		}

		public void Open()
		{
			if ( State != ConnectionState.Closed )
				throw new SqliteException( "There is already an open connection" );

			var connectionData = ParseConnectionString( ConnectionString );
			var openParam = string.Empty;
			if ( connectionData.ContainsKey( "uri" ) )
			{
				openParam = connectionData["uri"];
				if ( openParam.StartsWith( "file:" ) )
					openParam = openParam.Substring( 5 );
			}

			IntPtr conn;
			var retVal = UnsafeNativeMethods.Open( openParam, out conn, UnsafeNativeMethods.OpenFlags.ReadWrite | UnsafeNativeMethods.OpenFlags.URI, null );
			if ( retVal != UnsafeNativeMethods.ResultCode.OK )
				throw new SqliteException( GetError( conn ) + " - " + retVal + " ConnectionString: " + ConnectionString );
			NativeConnection = conn;

			State = ConnectionState.Open;
		}

		public void Dispose()
		{
			Close();
		}

		static SortedList<string, string> ParseConnectionString( string connectionString )
		{
			connectionString = connectionString.Replace( ',', ';' );
			var sortedList = new SortedList<string, string>( StringComparer.OrdinalIgnoreCase );
			var parameters = connectionString.Split( new[] {';'} );

			foreach ( var param in parameters )
			{
				var elements = param.Split( new[] { '=' } );
				if ( elements.Length != 2 )
					throw new SqliteException( "Invalid connection string " + connectionString );
				sortedList[elements[0].ToLowerInvariant()] = elements[1];
			}

			return sortedList;
		}

		internal static string GetError( IntPtr connection )
		{
			var res = UnsafeNativeMethods.ErrorMessage( connection );
			return GetNativeString( res );
		}

		internal static string GetNativeString( IntPtr stringPtr )
		{
			return Marshal.PtrToStringAnsi( stringPtr );
		}
	}
}