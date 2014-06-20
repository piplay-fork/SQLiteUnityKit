using System;
using System.IO;
using System.Collections;
using LiteKit.Data;
using LiteKit.Data.Sqlite;
using UnityEngine;

public class TestSQLiteKit : MonoBehaviour {

	[SerializeField]
	GUIText _guiText;

	string _dbFileName;

	void InsertVal(IDbCommand cmd, object val) {
		cmd.CommandText = "INSERT INTO Test (val) VALUES (@val);";
		var param = cmd.CreateParameter();
		param.ParameterName = "@val";
		param.Value = val;
		cmd.Parameters.Add(param);
		var cnt = cmd.ExecuteNonQuery();
		_guiText.text += "Inserted Count = " + cnt + "\n";
	}

	void Start () {
		try {
			_dbFileName = Application.persistentDataPath + "/database.sqlite";
			_guiText.text += "db=" + _dbFileName + "\n";
			if (!File.Exists(_dbFileName)) {
				SqliteConnection.CreateFile(_dbFileName);
			}
			using (var db = new SqliteConnection("URI=file:" + _dbFileName)) {
				db.Open();
				using (var cmd = db.CreateCommand()) {
					cmd.CommandText = "DROP TABLE IF EXISTS Test; CREATE TABLE Test (val);";
					var cnt = cmd.ExecuteNonQuery();
					_guiText.text += "Created Table = " + cnt + "\n";
				}
				using (var cmd = db.CreateCommand()) {
					InsertVal(cmd, 345);
					InsertVal(cmd, "Test");
				}
				using (var cmd = db.CreateCommand()) {
					cmd.CommandText = "SELECT val FROM Test;";
					var val = cmd.ExecuteScalar();
					_guiText.text += "Selected val = " + val + "\n";
				}
				using (var cmd = db.CreateCommand()) {
					cmd.CommandText = "SELECT val FROM Test;";
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							var val = reader.GetValue(0);
							_guiText.text += "Selected from Reader val = " + val + "\n";
						}
					}
				}
			}
		} catch (Exception ex) {
			_guiText.text += "exception=" + ex + "\n";
		}
	}
}
