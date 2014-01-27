using System;
using System.Runtime.InteropServices;

namespace LiteKit.Data.Sqlite
{
	static class UnsafeNativeMethods
	{
		public enum ResultCode
		{
			OK = 0,
			Error = 1,
			Internal = 2,
			Permission = 3,
			Abort = 4,
			Busy = 5,
			Locked = 6,
			NoMemory = 7,
			ReadOnly = 8,
			Interrupt = 9,
			IOError = 10,
			Corrupt = 11,
			NotFound = 12,
			Full = 13,
			CantOpen = 14,
			Protocol = 15,
			Empty = 16,
			Schema = 17,
			TooBig = 18,
			Constraint = 19,
			Mismatch = 20,
			Misuse = 21,
			NoLFS = 22,
			Authorization = 23,
			Format = 24,
			Range = 25,
			NotADB = 26,
			Notice = 27,
			Warning = 28,
			Row = 100,
			Done = 101
		}

		[Flags]
		public enum OpenFlags
		{
			ReadOnly = 0x00000001,
			ReadWrite = 0x00000002,
			Create = 0x00000004,
			DeleteOnClose = 0x00000008,
			Exclusive = 0x00000010,
			AutoProxy = 0x00000020,
			URI = 0x00000040,
			Memory = 0x00000080,
			MainDB = 0x00000100,
			TempDB = 0x00000200,
			TransientDB = 0x00000400,
			MainJournal = 0x00000800,
			TempJournal = 0x00001000,
			SubJournal = 0x00002000,
			MasterJournal = 0x00004000,
			NoMutex = 0x00008000,
			FullMutex = 0x00010000,
			SharedCache = 0x00020000,
			PrivateCache = 0x00040000,
			WAL = 0x00080000
		}

		public enum DataType
		{
			Integer = 1,
			Float = 2,
			Text = 3,
			Blob = 4,
			Null = 5
		}

		public enum SpecialDestructor
		{
			Static = 0,
			Transient = -1
		}

		[DllImport("sqlite3", EntryPoint = "sqlite3_open_v2")]
		public static extern ResultCode Open (string filename, out IntPtr db, OpenFlags flags, string vfs);

		[DllImport("sqlite3", EntryPoint = "sqlite3_close")]
		public static extern ResultCode Close (IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_prepare_v2")]
		public static extern ResultCode Prepare (IntPtr db, string zSql, int nByte, out IntPtr ppStmpt, IntPtr pzTail);

		[DllImport("sqlite3", EntryPoint = "sqlite3_step")]
		public static extern ResultCode Step (IntPtr stmHandle);

		[DllImport("sqlite3", EntryPoint = "sqlite3_finalize")]
		public static extern ResultCode Finalize (IntPtr stmHandle);

		[DllImport("sqlite3", EntryPoint = "sqlite3_errmsg")]
		public static extern IntPtr ErrorMessage (IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_count")]
		public static extern int ColumnCount (IntPtr stmHandle);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_name")]
		public static extern IntPtr ColumnName (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_type")]
		public static extern DataType ColumnType (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_int")]
		public static extern int ColumnInt (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_int64")]
		public static extern long ColumnInt64 (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_text")]
		public static extern IntPtr ColumnText (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_double")]
		public static extern double ColumnDouble (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_blob")]
		public static extern IntPtr ColumnBlob (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_bytes")]
		public static extern int ColumnBytes (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_changes")]
		public static extern int Changes (IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_parameter_index")]
		public static extern int BindParameterIndex (IntPtr stmHandle, string name);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_null")]
		public static extern ResultCode BindNull (IntPtr stmHandle, int index);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_int")]
		public static extern ResultCode BindInt (IntPtr stmHandle, int index, int value);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_int64")]
		public static extern ResultCode BindInt64 (IntPtr stmHandle, int index, long value);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_text")]
		public static extern ResultCode BindText (IntPtr stmHandle, int index, byte[] value, int size, SpecialDestructor destructor);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_double")]
		public static extern ResultCode BindDouble (IntPtr stmHandle, int index, double value);

		[DllImport("sqlite3", EntryPoint = "sqlite3_bind_blob")]
		public static extern ResultCode BindBlob (IntPtr stmHandle, int index, byte[] blobPtr, int size, SpecialDestructor destructor);
	}
}