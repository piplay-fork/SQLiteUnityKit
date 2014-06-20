using System.IO;
using System.Collections;
using LiteKit.Data;
using LiteKit.Data.Sqlite;
using UnityEngine;

public class TestSQLiteKit : MonoBehaviour {

	static string _dbFileName = Application.persistentDataPath + "/database.sqlite";

	void InsertVal(IDbCommand cmd, object val) {
		cmd.CommandText = "INSERT INTO Test (val) VALUES (@val);";
		var param = cmd.CreateParameter();
		param.ParameterName = "@val";
		param.Value = val;
		cmd.Parameters.Add(param);
		var cnt = cmd.ExecuteNonQuery();
		Debug.Log ("Inserted Count = " + cnt);
	}

	void Start () {
		if (!File.Exists(_dbFileName)) {
			SqliteConnection.CreateFile(_dbFileName);
		}
		using (var db = new SqliteConnection("URI=file:" + _dbFileName)) {
			db.Open();
			using (var cmd = db.CreateCommand()) {
				cmd.CommandText = "DROP TABLE IF EXISTS Test; CREATE TABLE Test (val);";
				var cnt = cmd.ExecuteNonQuery();
				Debug.Log ("Created Table = " + cnt);
			}
			using (var cmd = db.CreateCommand()) {
				InsertVal(cmd, 345);
				InsertVal(cmd, "Test");
			}
			using (var cmd = db.CreateCommand()) {
				cmd.CommandText = "SELECT val FROM Test;";
				var val = cmd.ExecuteScalar();
				Debug.Log ("Selected val = " + val);
			}
			using (var cmd = db.CreateCommand()) {
				cmd.CommandText = "SELECT val FROM Test;";
				using (var reader = cmd.ExecuteReader()) {
					while (reader.Read()) {
						var val = reader.GetValue(0);
						Debug.Log ("Selected from Reader val = " + val);
					}
				}
			}
		}
	}
}
