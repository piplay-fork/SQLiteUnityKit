using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MonoTouch
{
	[AttributeUsage(AttributeTargets.Method)]
	sealed class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute(Type t)
		{
		}
	}
}

namespace LiteKit.Data.Sqlite
{
	public class SqliteDbDataParameter : IDbDataParameter, ICloneable
	{
		SqliteCommand Parent { get; set; }

		public string ParameterName
		{
			get;
			set;
		}

		public object Value
		{
			get;
			set;
		}

		public byte Precision
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public byte Scale
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public int Size
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public DbType DbType
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public ParameterDirection Direction
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public bool IsNullable
		{
			get {
				throw new NotImplementedException();
			}
		}

		public string SourceColumn
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public DataRowVersion SourceVersion
		{
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public SqliteDbDataParameter( SqliteCommand sqliteCommand )
		{
			Parent = sqliteCommand;
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}

		internal void Apply( IntPtr nativeCommand )
		{
			var paramIndex = UnsafeNativeMethods.BindParameterIndex( nativeCommand, ParameterName );
			if ( paramIndex == 0 )
				return;

			var retVal = ApplyWithResultCode( nativeCommand, paramIndex );
			if ( retVal != UnsafeNativeMethods.ResultCode.OK )
				throw new SqliteException( SqliteConnection.GetError( Parent.NativeConnection ) + " - " + retVal );
		}

		UnsafeNativeMethods.ResultCode ApplyWithResultCode( IntPtr nativeCommand, int paramIndex )
		{
			if ( Value == null )
				return UnsafeNativeMethods.BindNull( nativeCommand, paramIndex );

			var objType = Value.GetType();
			if ( objType == typeof( byte[] ) )
			{
				var val = (byte[]) Value;
				return UnsafeNativeMethods.BindBlob( nativeCommand, paramIndex, val, val.Length, UnsafeNativeMethods.SpecialDestructor.Transient );
			}

			var typeCode = Type.GetTypeCode( objType );

			switch ( typeCode )
			{
				case TypeCode.Empty:
				case TypeCode.DBNull:
					return UnsafeNativeMethods.BindNull( nativeCommand, paramIndex );
				case TypeCode.Boolean:
					return UnsafeNativeMethods.BindInt( nativeCommand, paramIndex, ((bool) Value) ? 1 : 0 );
				case TypeCode.SByte:
					return UnsafeNativeMethods.BindInt( nativeCommand, paramIndex, (int)(sbyte) Value );
				case TypeCode.Byte:
					return UnsafeNativeMethods.BindInt( nativeCommand, paramIndex, (int)(byte) Value );
				case TypeCode.Int16:
					return UnsafeNativeMethods.BindInt( nativeCommand, paramIndex, (int)(short) Value );
				case TypeCode.UInt16:
					return UnsafeNativeMethods.BindInt( nativeCommand, paramIndex, (int)(ushort) Value );
				case TypeCode.Int32:
					return UnsafeNativeMethods.BindInt( nativeCommand, paramIndex, (int) Value );
				case TypeCode.UInt32:
					return UnsafeNativeMethods.BindInt64( nativeCommand, paramIndex, (long)(uint) Value );
				case TypeCode.Int64:
					return UnsafeNativeMethods.BindInt64( nativeCommand, paramIndex, (long) Value );
				case TypeCode.UInt64:
					return UnsafeNativeMethods.BindInt64( nativeCommand, paramIndex, (long)(ulong) Value );
				case TypeCode.Single:
					return UnsafeNativeMethods.BindDouble( nativeCommand, paramIndex, (double)(float) Value );
				case TypeCode.Double:
					return UnsafeNativeMethods.BindDouble( nativeCommand, paramIndex, (double) Value );
				case TypeCode.Decimal:
					return UnsafeNativeMethods.BindDouble( nativeCommand, paramIndex, (double)(decimal) Value );
				case TypeCode.String:
					return ApplyString( nativeCommand, paramIndex, (string) Value );
				case TypeCode.Char:
					return ApplyString( nativeCommand, paramIndex, ((char) Value).ToString() );
				default:
					throw new SqliteException( "Unhandled type " + typeCode );
			}
		}

		UnsafeNativeMethods.ResultCode ApplyString( IntPtr nativeCommand, int paramIndex, string val )
		{
			var strBytes = System.Text.Encoding.UTF8.GetBytes( val );
			return UnsafeNativeMethods.BindText( nativeCommand, paramIndex, strBytes, strBytes.Length, UnsafeNativeMethods.SpecialDestructor.Transient );
		}
	}
}