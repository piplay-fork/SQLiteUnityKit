using System;
using System.Runtime.Serialization;

namespace LiteKit.Data
{
	public class DataTable
	{
		//
		// Constructors
		//
		public DataTable()
		{
			throw new NotImplementedException();
		}

		public DataTable( string tableName ) : this()
		{
		}

		protected DataTable( SerializationInfo info, StreamingContext context ) : this()
		{
		}

		public DataTable( string tableName, string tableNamespace ) : this( tableName )
		{
		}
	}
}