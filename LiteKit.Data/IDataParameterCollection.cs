using System;
using System.Collections;

namespace LiteKit.Data
{
	public interface IDataParameterCollection : IList
	{
		//
		// Indexer
		//
		object this[ string parameterName ]
		{
			get;
			set;
		}

		//
		// Methods
		//
		bool Contains( string parameterName );

		int IndexOf( string parameterName );

		void RemoveAt( string parameterName );
	}
}