using System;

namespace LiteKit.Data
{
	public interface IDbDataParameter : IDataParameter
	{
		//
		// Properties
		//
		byte Precision
		{
			get;
			set;
		}

		byte Scale
		{
			get;
			set;
		}

		int Size
		{
			get;
			set;
		}
	}
}