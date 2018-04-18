using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace NotesBot.Controllers
{
    [Serializable]
    public class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public DBConnect()
        {
            Initialize();
        }

        private void Initialize()
        {
            server = "www.papademas.net";
            database = "510labs";
            uid = "db510";
            password = "510";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            // connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            //   static final String DB_URL = "jdbc:mysql://www.papademas.net:3306/510labs?autoReconnect=true&useSSL=false";
            //   static final String USER = "db510", PASS = "510";/
            connection = new MySqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            try
            {
                connection.Open();
            }
            catch (MySqlException e)
            {
                Console.Write(e.ToString());
            }
        }

        public void CloseConnection()
        {
            connection.Close();
        }

        public List<string> getData()
        {
            string query = "SELECT * FROM p_gupt_tab where id='id12101'";


            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            System.Collections.Generic.List<string> list = new List<string>();
            //Read the data and store them in the list
            while (dataReader.Read())
            {
                list.Add(dataReader[0] + "");
                list.Add(dataReader[1] + "");
                list.Add(dataReader[2] + "");
            }

            return list;

        }
    }
}
