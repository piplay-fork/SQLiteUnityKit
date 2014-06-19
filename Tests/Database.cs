using Xunit;
using System;
using System.IO;
using LiteKit.Data;
using LiteKit.Data.Sqlite;

namespace Tests {
    public class Database {
        string DBDir {
            get {
                return Environment.CurrentDirectory + "/newDir";
            }
        }

        string DBFile {
            get {
                return DBDir + "/file.db";
            }
        }

        string ConnString {
            get {
                return "URI=file:" + DBFile;
            }
        }

        [Fact]
        public void CreatesFileIfNotExists() {
            if (string.IsNullOrEmpty(DBDir) == false && Directory.Exists(DBDir)) { Directory.Delete(DBDir, true); }
            if (File.Exists(DBFile)) { File.Delete(DBFile); }

            Assert.False(File.Exists(DBFile));
            SqliteConnection.CreateFile(DBFile);
            Assert.True(File.Exists(DBFile));
        }

        [Fact]
        public void ConnectsToFile() {
            SqliteConnection.CreateFile( DBFile );

            using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                Assert.NotNull( dbConnection );
                Assert.Equal( ConnectionState.Closed, dbConnection.State );

                dbConnection.Open();
                Assert.Equal( ConnectionState.Open, dbConnection.State );

                dbConnection.Close();
                Assert.Equal( ConnectionState.Closed, dbConnection.State );
            }
        }

        [Fact]
        public void CreatesCommand() {
            SqliteConnection.CreateFile( DBFile );
            using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                dbConnection.Open();

                using ( var command = dbConnection.CreateCommand() ) {
                    Assert.NotNull( command );
                }
            }
        }

        [Fact]
        public void ExecuteReader() {
            SqliteConnection.CreateFile( DBFile );
            using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                dbConnection.Open();

                const string QueryText = "SELECT 123 AS ItemA, 'Hello' AS ItemB UNION SELECT 456, 'World';";
                using ( var command = dbConnection.CreateCommand() ) {
                    command.CommandText = QueryText;
                    Assert.Equal( QueryText, command.CommandText );

                    using ( var reader = command.ExecuteReader() ) {
                        Assert.NotNull( reader );

                        Assert.Equal( 2, reader.FieldCount );

                        Assert.Equal( "ItemA", reader.GetName( 0 ) );
                        Assert.Equal( "ItemB", reader.GetName( 1 ) );

                        Assert.True( reader.Read() );
                        Assert.Equal( 123, reader.GetInt32( 0 ) );
                        Assert.Equal( "Hello", reader.GetString( 1 ) );

                        Assert.True( reader.Read() );
                        Assert.Equal( 456, reader.GetInt32( 0 ) );
                        Assert.Equal( "World", reader.GetString( 1 ) );

                        Assert.False( reader.Read() );
                    }
                }
            }
        }

        [Fact]
        public void ExecuteScalar() {
            SqliteConnection.CreateFile( DBFile );
            using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                dbConnection.Open();

                const string QueryText = "SELECT sqlite_version();";
                using ( var command = dbConnection.CreateCommand() ) {
                    command.CommandText = QueryText;
                    Assert.Equal( QueryText, command.CommandText );

                    var result = command.ExecuteScalar();
                    Assert.IsType( typeof( string ), result );
                    Assert.True(( (string) result ).Length > 0);
                }
            }
        }

        [Fact]
        public void ExecuteNonQuery() {
            SqliteConnection.CreateFile( DBFile );
            using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                dbConnection.Open();

                var random = (new Random()).Next();

                const string QueryText = "DROP TABLE IF EXISTS TestNonQuery; CREATE TABLE TestNonQuery (val);";
                using ( var command = dbConnection.CreateCommand() ) {
                    command.CommandText = QueryText;
                    Assert.Equal( QueryText, command.CommandText );

                    command.ExecuteNonQuery();
                }

                string InsertQuery = string.Format( "INSERT INTO TestNonQuery (val) VALUES ({0});", random );
                using ( var command = dbConnection.CreateCommand() ) {
                    command.CommandText = InsertQuery;
                    Assert.Equal( InsertQuery, command.CommandText );

                    var result = command.ExecuteNonQuery();
                    Assert.Equal( 1, result );
                }

                using ( var command = dbConnection.CreateCommand() ) {
                    command.CommandText = "SELECT val FROM TestNonQuery;";
                    var val = command.ExecuteScalar();

                    Assert.IsType( typeof(long), val );
                    Assert.Equal( (long)random, val );
                }
            }
        }

        [Fact]
        public void TypeSupport() {
            SqliteConnection.CreateFile( DBFile );
            using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                dbConnection.Open();

                const string QueryText = "SELECT NULL, 123, 456.789, 'Hello', CAST('World' AS BLOB);";
                using ( var command = dbConnection.CreateCommand() ) {
                    command.CommandText = QueryText;
                    Assert.Equal( QueryText, command.CommandText );

                    using ( var reader = command.ExecuteReader() ) {
                        Assert.Equal( 5, reader.FieldCount );
                        reader.Read();

                        Assert.Null( reader[0] );

                        Assert.IsType( typeof( long ), reader[1] );
                        Assert.Equal( (long)123, reader[1] );

                        Assert.IsType( typeof( double ), reader[2] );
                        Assert.Equal( 456.789, reader[2] );

                        Assert.IsType( typeof( string ), reader[3] );
                        Assert.Equal( "Hello", reader[3] );

                        Assert.IsType( typeof( byte[] ), reader[4] );
                        Assert.Equal( "World", System.Text.Encoding.UTF8.GetString( (byte[]) reader[4] ) );
                    }
                }
            }
        }

        [Fact]
        public void ParametersSupport() {
            var bytesTest = new byte[] { 1, 2, 3, 4, 5 };
            var values   = new object[] { null, true, (sbyte)123, (byte)123, (short)123, (ushort)123, (int)123, (uint)123, (long)123, (ulong)123, (float)123.25f, (double)123.25, (decimal)123.25d, "hello", 'c', bytesTest };
            var expected = new object[] { null,   1L,       123L,      123L,       123L,        123L,     123L,      123L,      123L,       123L,         123.25,         123.25,           123.25, "hello", "c", bytesTest };
            for ( int i = 0; i < values.Length; ++i ) {
                SqliteConnection.CreateFile( DBFile );
                using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                    dbConnection.Open();

                    const string InitQueryText = "DROP TABLE IF EXISTS TestParameters; CREATE TABLE TestParameters (val);";
                    using ( var command = dbConnection.CreateCommand() ) {
                        command.CommandText = InitQueryText;
                        Assert.Equal( InitQueryText, command.CommandText );

                        command.ExecuteNonQuery();
                    }

                    const string InsertQuery = "INSERT INTO TestParameters (val) VALUES (@val);";
                    using ( var command = dbConnection.CreateCommand() ) {
                        command.CommandText = InsertQuery;
                        Assert.Equal( InsertQuery, command.CommandText );

                        var param = command.CreateParameter();
                        Assert.NotNull( param );
                        param.ParameterName = "@val";
                        param.Value = values[i];
                        command.Parameters.Add( param );

                        var result = command.ExecuteNonQuery();
                        Assert.Equal( 1, result );
                    }

                    using (var command = dbConnection.CreateCommand()) {
                        command.CommandText = "SELECT val FROM TestParameters;";
                        var retVal = command.ExecuteScalar();

                        if ( expected[i] != null )
                        { Assert.IsType( expected[i].GetType(), retVal ); }
                        Assert.Equal( expected[i], retVal );
                    }
                }
            }
        }

        [Fact]
        public void InsertTest() {
            SqliteConnection.CreateFile( DBFile );
            using ( var dbConnection = new SqliteConnection( ConnString ) ) {
                dbConnection.Open();

                using ( var createTableCommand = dbConnection.CreateCommand() ) {
                    createTableCommand.CommandText = "DROP TABLE IF EXISTS InsertTest; CREATE TABLE InsertTest (key TEXT, revision INTEGER, value BLOB);";
                    createTableCommand.ExecuteNonQuery();
                }

                using ( var insertContentCommand = dbConnection.CreateCommand() ) {
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
                    dataParam.Value = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
                    insertContentCommand.Parameters.Add( dataParam );

                    insertContentCommand.ExecuteNonQuery();
                }

                using ( var queryCommand = dbConnection.CreateCommand() ) {
                    queryCommand.CommandText = "SELECT key, revision, value FROM InsertTest;";
                    using ( var queryReader = queryCommand.ExecuteReader() ) {
                        Assert.True( queryReader.Read() );

                        var key = queryReader.GetString( 0 );
                        var rev = queryReader.GetInt32( 1 );
                        var value = new byte[7];
                        queryReader.GetBytes( 2, 0, value, 0, 7 );

                        Assert.Equal( "InsertTest:1", key );
                        Assert.Equal( 76543, rev );
                        Assert.Equal( new byte[] { 1, 2, 3, 4, 5, 6, 7 }, value );

                        Assert.False( queryReader.Read() );
                    }
                }
            }
        }
    }
}