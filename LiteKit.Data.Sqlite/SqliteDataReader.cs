using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace LiteKit.Data.Sqlite
{
	public class SqliteDataReader : IDataReader, IEnumerable
	{
		static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		SqliteCommand Parent { get; set; }
		IntPtr NativeCommand { get; set; }

		public int Depth
		{
			get {
				throw new NotImplementedException();
			}
		}

		public int FieldCount
		{
			get;
			private set;
		}

		public bool IsClosed
		{
			get;
			private set;
		}

		public int RecordsAffected
		{
			get;
			private set;
		}

		internal SqliteDataReader( SqliteCommand sqliteCommand, IntPtr nativeCommand )
		{
			Parent = sqliteCommand;
			NativeCommand = nativeCommand;
			FieldCount = UnsafeNativeMethods.ColumnCount( nativeCommand );
		}

		public void Close()
		{
			var retVal = UnsafeNativeMethods.Finalize( NativeCommand );
			if ( retVal != UnsafeNativeMethods.ResultCode.OK )
				throw new SqliteException( SqliteConnection.GetError( Parent.NativeConnection ) + " - " + retVal );
			IsClosed = true;
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public bool NextResult()
		{
			return Read();
		}

		public bool Read()
		{
			var result = UnsafeNativeMethods.Step( NativeCommand );
			switch ( result )
			{
				case UnsafeNativeMethods.ResultCode.Row:
					return true;
				case UnsafeNativeMethods.ResultCode.Done:
					RecordsAffected = UnsafeNativeMethods.Changes( Parent.NativeConnection );
					return false;
				default:
					throw new SqliteException( "Unhandled result: " + result );
			}
		}

		public bool GetBoolean( int i )
		{
			return GetInt32( i ) != 0;
		}

		public byte GetByte( int i )
		{
			return (byte) GetInt32( i );
		}

		public long GetBytes( int i, long fieldOffset, byte[] buffer, int bufferoffset, int length )
		{
			if ( fieldOffset != 0 )
				throw new SqliteException( "Field offset not yet supported" );

			IntPtr blob = UnsafeNativeMethods.ColumnBlob( NativeCommand, i );
			int size = UnsafeNativeMethods.ColumnBytes( NativeCommand, i );
			length = Math.Min( size, length );
			Marshal.Copy( blob, buffer, bufferoffset, length );
			return length;
		}

		public char GetChar( int i )
		{
			return GetString( i )[0];
		}

		public long GetChars( int i, long fieldoffset, char[] buffer, int bufferoffset, int length )
		{
			var str = GetString( i ).ToCharArray( (int) fieldoffset, length );
			length = Math.Min( str.Length, length );
			Array.Copy( str, 0, buffer, bufferoffset, length );
			return length;
		}

		public IDataReader GetData( int i )
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName( int i )
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime( int i )
		{
			return Epoch + TimeSpan.FromMilliseconds( GetDouble( i ) );
		}

		public decimal GetDecimal( int i )
		{
			return (decimal) GetDouble( i );
		}

		public double GetDouble( int i )
		{
			return UnsafeNativeMethods.ColumnDouble( NativeCommand, i );
		}

		public Type GetFieldType( int i )
		{
			var dataType = UnsafeNativeMethods.ColumnType( NativeCommand, i );
			switch ( dataType )
			{
				case UnsafeNativeMethods.DataType.Integer:
					return typeof( long );
				case UnsafeNativeMethods.DataType.Float:
					return typeof( double );
				case UnsafeNativeMethods.DataType.Text:
					return typeof( string );
				case UnsafeNativeMethods.DataType.Blob:
					return typeof( byte[] );
				case UnsafeNativeMethods.DataType.Null:
					return typeof( DBNull );
				default:
					throw new SqliteException( "Unknown column type " + dataType );
			}
		}

		public float GetFloat( int i )
		{
			return (float) GetDouble( i );
		}

		public Guid GetGuid( int i )
		{
			throw new NotImplementedException();
		}

		public short GetInt16( int i )
		{
			return (short) GetInt32( i );
		}

		public int GetInt32( int i )
		{
			return UnsafeNativeMethods.ColumnInt( NativeCommand, i );
		}

		public long GetInt64( int i )
		{
			return UnsafeNativeMethods.ColumnInt64( NativeCommand, i );
		}

		public string GetName( int i )
		{
			var namePtr = UnsafeNativeMethods.ColumnName( NativeCommand, i );
			return SqliteConnection.GetNativeString( namePtr );
		}

		public int GetOrdinal( string name )
		{
			throw new NotImplementedException();
		}

		public string GetString( int i )
		{
			var stringPtr = UnsafeNativeMethods.ColumnText( NativeCommand, i );
			return SqliteConnection.GetNativeString( stringPtr );
		}

		public object GetValue( int i )
		{
			var columnType = GetFieldType( i );
			var typeCode = Type.GetTypeCode( columnType );

			if ( columnType == typeof( byte[] ) )
			{
				IntPtr blob = UnsafeNativeMethods.ColumnBlob( NativeCommand, i );
				int size = UnsafeNativeMethods.ColumnBytes( NativeCommand, i );
				var buffer = new byte[size];
				Marshal.Copy( blob, buffer, 0, size );
				return buffer;
			}

			switch ( typeCode )
			{
				case TypeCode.DBNull:
					return null;
				case TypeCode.Int64:
					return GetInt64( i );
				case TypeCode.Double:
					return GetDouble( i );
				case TypeCode.String:
					return GetString( i );
				default:
					throw new SqliteException( "Unhandled type code " + typeCode );
			}
		}

		public int GetValues( object[] values )
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull( int i )
		{
			return UnsafeNativeMethods.ColumnType( NativeCommand, i ) == UnsafeNativeMethods.DataType.Null;
		}

		public object this[ string name ]
		{
			get {
				throw new NotImplementedException();
			}
		}

		public object this[ int i ]
		{
			get {
				return GetValue( i );
			}
		}

		public void Dispose()
		{
			Close();
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}