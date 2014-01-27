using System;

namespace LiteKit.Data
{
	public interface IDataReader : IDisposable, IDataRecord
	{
		//
		// Properties
		//
		int Depth
		{
			get;
		}

		bool IsClosed
		{
			get;
		}

		int RecordsAffected
		{
			get;
		}

		//
		// Methods
		//
		void Close();

		DataTable GetSchemaTable();

		bool NextResult();

		bool Read();
	}
}