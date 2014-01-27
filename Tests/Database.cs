using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using LiteKit.Data;
using LiteKit.Data.Sqlite;

namespace Tests
{
	[TestFixture()]
	public class Database
	{
		string DBDir
		{
			get {
				return Environment.CurrentDirectory + "/newDir";
			}
		}

		string DBFile
		{
			get {
				return DBDir + "/file.db";
			}
		}

		string ConnString
		{
			get {
				return "URI=file:" + DBFile;
			}
		}

		[Test()]
		public void CreatesFileIfNotExists()
		{
			if ( string.IsNullOrEmpty( DBDir ) == false && Directory.Exists( DBDir ) )
				Directory.Delete( DBDir, true );
			if ( File.Exists( DBFile ) )
				File.Delete( DBFile );

			Assert.IsFalse( File.Exists( DBFile ) );
			SqliteConnection.CreateFile( DBFile );
			Assert.IsTrue( File.Exists( DBFile ) );
		}

		[Test()]
		public void ConnectsToFile()
		{
			SqliteConnection.CreateFile( DBFile );

			using ( var dbConnection = new SqliteConnection( ConnString ) )
			{
				Assert.IsNotNull( dbConnection );
				Assert.AreEqual( ConnectionState.Closed, dbConnection.State );

				dbConnection.Open();
				Assert.AreEqual( ConnectionState.Open, dbConnection.State );

				dbConnection.Close();
				Assert.AreEqual( ConnectionState.Closed, dbConnection.State );
			}
		}

		[Test()]
		public void CreatesCommand()
		{
			SqliteConnection.CreateFile( DBFile );
			using ( var dbConnection = new SqliteConnection( ConnString ) )
			{
				dbConnection.Open();

				using ( var command = dbConnection.CreateCommand() )
				{
					Assert.IsNotNull( command );
				}
			}
		}

		[Test()]
		public void ExecuteReader()
		{
			SqliteConnection.CreateFile( DBFile );
			using ( var dbConnection = new SqliteConnection( ConnString ) )
			{
				dbConnection.Open();

				const string QueryText = "SELECT 123 AS ItemA, 'Hello' AS ItemB UNION SELECT 456, 'World';";
				using ( var command = dbConnection.CreateCommand() )
				{
					command.CommandText = QueryText;
					Assert.AreEqual( QueryText, command.CommandText );

					using ( var reader = command.ExecuteReader() )
					{
						Assert.IsNotNull( reader );

						Assert.AreEqual( 2, reader.FieldCount );

						Assert.AreEqual( "ItemA", reader.GetName( 0 ) );
						Assert.AreEqual( "ItemB", reader.GetName( 1 ) );

						Assert.IsTrue( reader.Read() );
						Assert.AreEqual( 123, reader.GetInt32( 0 ) );
						Assert.AreEqual( "Hello", reader.GetString( 1 ) );

						Assert.IsTrue( reader.Read() );
						Assert.AreEqual( 456, reader.GetInt32( 0 ) );
						Assert.AreEqual( "World", reader.GetString( 1 ) );

						Assert.IsFalse( reader.Read() );
					}
				}
			}
		}

		[Test()]
		public void ExecuteScalar()
		{
			SqliteConnection.CreateFile( DBFile );
			using ( var dbConnection = new SqliteConnection( ConnString ) )
			{
				dbConnection.Open();

				const string QueryText = "SELECT sqlite_version();";
				using ( var command = dbConnection.CreateCommand() )
				{
					command.CommandText = QueryText;
					Assert.AreEqual( QueryText, command.CommandText );

					var result = command.ExecuteScalar();
					Assert.IsInstanceOfType( typeof( string ), result );
					Assert.Greater( ( (string) result ).Length, 0 );
				}
			}
		}

		[Test()]
		public void ExecuteNonQuery()
		{
			SqliteConnection.CreateFile( DBFile );
			using ( var dbConnection = new SqliteConnection( ConnString ) )
			{
				dbConnection.Open();

				var random = (new Random()).Next();

				const string QueryText = "DROP TABLE IF EXISTS TestNonQuery; CREATE TABLE TestNonQuery (val);";
				using ( var command = dbConnection.CreateCommand() )
				{
					command.CommandText = QueryText;
					Assert.AreEqual( QueryText, command.CommandText );

					command.ExecuteNonQuery();
				}

				string InsertQuery = string.Format( "INSERT INTO TestNonQuery (val) VALUES ({0});", random );
				using ( var command = dbConnection.CreateCommand() )
				{
					command.CommandText = InsertQuery;
					Assert.AreEqual( InsertQuery, command.CommandText );

					var result = command.ExecuteNonQuery();
					Assert.AreEqual( 1, result );
				}

				using ( var command = dbConnection.CreateCommand() )
				{
					command.CommandText = "SELECT val FROM TestNonQuery;";
					var val = command.ExecuteScalar();

					Assert.IsInstanceOfType( typeof(long), val );
					Assert.AreEqual( random, val );
				}
			}
		}

		[Test()]
		public void TypeSupport()
		{
			SqliteConnection.CreateFile( DBFile );
			using ( var dbConnection = new SqliteConnection( ConnString ) )
			{
				dbConnection.Open();

				const string QueryText = "SELECT NULL, 123, 456.789, 'Hello', CAST('World' AS BLOB);";
				using ( var command = dbConnection.CreateCommand() )
				{
					command.CommandText = QueryText;
					Assert.AreEqual( QueryText, command.CommandText );

					using ( var reader = command.ExecuteReader() )
					{
						Assert.AreEqual( 5, reader.FieldCount );
						reader.Read();

						Assert.IsNull( reader[0] );

						Assert.IsInstanceOfType( typeof( long ), reader[1] );
						Assert.AreEqual( 123, reader[1] );

						Assert.IsInstanceOfType( typeof( double ), reader[2] );
						Assert.AreEqual( 456.789, reader[2] );

						Assert.IsInstanceOfType( typeof( string ), reader[3] );
						Assert.AreEqual( "Hello", reader[3] );

						Assert.IsInstanceOfType( typeof( byte[] ), reader[4] );
						Assert.AreEqual( "World", System.Text.Encoding.UTF8.GetString( (byte[]) reader[4] ) );
					}
				}
			}
		}

		[Test()]
		public void ParametersSupport()
		{
			var bytesTest = new byte[]{ 1, 2, 3, 4, 5 };
			var values   = new object[] { null, true, (sbyte)123, (byte)123, (short)123, (ushort)123, (int)123, (uint)123, (long)123, (ulong)123, (float)123.25f, (double)123.25, (decimal)123.25d, "hello", 'c', bytesTest };
			var expected = new object[] { null,   1L,       123L,      123L,       123L,        123L,     123L,      123L,      123L,       123L,         123.25,         123.25,           123.25, "hello", "c", bytesTest };
			for ( int i = 0; i < values.Length; ++i )
			{
				SqliteConnection.CreateFile( DBFile );
				using ( var dbConnection = new SqliteConnection( ConnString ) )
				{
					dbConnection.Open();

					const string InitQueryText = "DROP TABLE IF EXISTS TestParameters; CREATE TABLE TestParameters (val);";
					using ( var command = dbConnection.CreateCommand() )
					{
						command.CommandText = InitQueryText;
						Assert.AreEqual( InitQueryText, command.CommandText );

						command.ExecuteNonQuery();
					}

					const string InsertQuery = "INSERT INTO TestParameters (val) VALUES (@val);";
					using ( var command = dbConnection.CreateCommand() )
					{
						command.CommandText = InsertQuery;
						Assert.AreEqual( InsertQuery, command.CommandText );

						var param = command.CreateParameter();
						Assert.IsNotNull( param );
						param.ParameterName = "@val";
						param.Value = values[i];
						command.Parameters.Add( param );

						var result = command.ExecuteNonQuery();
						Assert.AreEqual( 1, result );
					}

					using (var command = dbConnection.CreateCommand())
					{
						command.CommandText = "SELECT val FROM TestParameters;";
						var retVal = command.ExecuteScalar();

						if ( expected[i] != null )
							Assert.IsInstanceOfType( expected[i].GetType(), retVal );
						Assert.AreEqual( expected[i], retVal );
					}
				}
			}
		}

		[Test()]
		public void InsertTest()
		{
			SqliteConnection.CreateFile( DBFile );
			using ( var dbConnection = new SqliteConnection( ConnString ) )
			{
				dbConnection.Open();

				using ( var createTableCommand = dbConnection.CreateCommand() )
				{
					createTableCommand.CommandText = "DROP TABLE IF EXISTS InsertTest; CREATE TABLE InsertTest (key TEXT, revision INTEGER, value BLOB);";
					createTableCommand.ExecuteNonQuery();
				}

				using ( var insertContentCommand = dbConnection.CreateCommand() )
				{
					insertContentCommand.CommandText = "BEGIN; INSERT OR REPLACE INTO InsertTest (key, revision, value) VALUES (@key, @revision, @value); COMMIT;";

					var keyParam = insertContentCommand.CreateParameter();
					keyParam.ParameterName = "@key";
					keyParam.Value = "InsertTest:1";
					insertContentCommand.Parameters.Add( keyParam );

					var revParam = insertContentCommand.CreateParameter();
					revParam.ParameterName = "@revision";
					revParam.Value = 76543;
					insertContentCommand.Parameters.Add( revParam );

					var dataParam = insertContentCommand.CreateParameter();
					dataParam.ParameterName = "@value";
					dataParam.Value = new byte[]{ 1, 2, 3, 4, 5, 6, 7 };
					insertContentCommand.Parameters.Add( dataParam );

					insertContentCommand.ExecuteNonQuery();
				}

				using ( var queryCommand = dbConnection.CreateCommand() )
				{
					queryCommand.CommandText = "SELECT key, revision, value FROM InsertTest;";
					using ( var queryReader = queryCommand.ExecuteReader() )
					{
						Assert.IsTrue( queryReader.Read() );

						var key = queryReader.GetString( 0 );
						var rev = queryReader.GetInt32( 1 );
						var value = new byte[7];
						queryReader.GetBytes( 2, 0, value, 0, 7 );

						Assert.AreEqual( "InsertTest:1", key );
						Assert.AreEqual( 76543, rev );
						Assert.AreEqual( new byte[]{ 1, 2, 3, 4, 5, 6, 7 }, value );

						Assert.IsFalse( queryReader.Read() );
					}
				}
			}
		}
	}
}