using System;
using System.Collections;
using System.Collections.Generic;

namespace LiteKit.Data.Sqlite
{
	public class SqliteDataParameterCollection : IDataParameterCollection
	{
		SqliteCommand Parent { get; set; }

		IList<SqliteDbDataParameter> Parameters { get; set; }

		public bool IsFixedSize
		{
			get {
				throw new NotImplementedException();
			}
		}

		public bool IsReadOnly
		{
			get {
				throw new NotImplementedException();
			}
		}

		public int Count
		{
			get {
				throw new NotImplementedException();
			}
		}

		public bool IsSynchronized
		{
			get {
				throw new NotImplementedException();
			}
		}

		public object SyncRoot
		{
			get {
				throw new NotImplementedException();
			}
		}

		public SqliteDataParameterCollection( SqliteCommand sqliteCommand )
		{
			Parent = sqliteCommand;
			Parameters = new List<SqliteDbDataParameter>();
		}

		public bool Contains( string parameterName )
		{
			throw new NotImplementedException();
		}

		public int IndexOf( string parameterName )
		{
			throw new NotImplementedException();
		}

		public void RemoveAt( string parameterName )
		{
			throw new NotImplementedException();
		}

		public object this[ string parameterName ]
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public int Add( object value )
		{
			var dbParam = value as SqliteDbDataParameter;
			if ( dbParam == null )
				throw new ArgumentException();

			Parameters.Add( dbParam );
			return Parameters.Count - 1;
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains( object value )
		{
			throw new NotImplementedException();
		}

		public int IndexOf( object value )
		{
			throw new NotImplementedException();
		}

		public void Insert( int index, object value )
		{
			throw new NotImplementedException();
		}

		public void Remove( object value )
		{
			throw new NotImplementedException();
		}

		public void RemoveAt( int index )
		{
			throw new NotImplementedException();
		}

		public object this[ int index ]
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public void CopyTo( Array array, int index )
		{
			throw new NotImplementedException();
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		internal void Apply( IntPtr nativeCommand )
		{
			foreach ( var param in Parameters )
				param.Apply( nativeCommand );
		}
	}
}

