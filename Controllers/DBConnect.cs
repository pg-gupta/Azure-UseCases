using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using SimpleEchoBot.Models;

namespace Microsoft.Bot.Sample.SimpleEchoBot
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
            database = "fp510";
            uid = "fpuser";
            password = "510";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

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

       
        public List<Disease> getDiseases(List<string> symptoms)
        {
            string likesymptom = "";
            foreach (var symptom in symptoms)
            {
                likesymptom += " description LIKE '%" + symptom + "%' OR";
            }
            likesymptom = likesymptom.Remove(likesymptom.Length - 2);
            string symptomsquery = "SELECT disease_id FROM dim_symptoms WHERE" + likesymptom;

            string query = "SELECT  disease_id,disease_name,treatment,specializationid FROM dim_disease WHERE disease_id IN (" + symptomsquery + ") LIMIT 5";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            System.Collections.Generic.List<Disease> list = new List<Disease>();
            //Read the data and store them in the list
            Disease disease = null;
            while (dataReader.Read())
            {
                disease = new Disease(dataReader[0].ToString(), dataReader[1].ToString(), dataReader[2].ToString(), dataReader[3].ToString());
                list.Add(disease);
            }

            return list;

        }

        internal List<Medicine> getMedicines(List<string> diseases)
        {
            string diseasesString = "";
            foreach (var disease in diseases)
            {
                diseasesString += "'" + disease + "' ,";
            }
            diseasesString = diseasesString.Remove(diseasesString.Length - 1);
            string diseasesquery = "SELECT disease_id FROM dim_disease WHERE disease_name IN (" + diseasesString + ")";

            string query = "SELECT  medicine_id, medicine_name, description FROM dim_medicine WHERE disease_id IN (" + diseasesquery + ") LIMIT 5";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            System.Collections.Generic.List<Medicine> list = new List<Medicine>();
            //Read the data and store them in the list
            while (dataReader.Read())
            {
                Medicine medicine = new Medicine(dataReader[0].ToString(), dataReader[1].ToString(), dataReader[2].ToString());
                list.Add(medicine);
            }

            return list;


        }

        internal List<Doctor> getDoctors(List<string> specialization)
        {
            string specializationString = "";
            foreach (var spec in specialization)
            {
                specializationString += "'" + spec + "' ,";
            }
            specializationString = specializationString.Remove(specializationString.Length - 1);
            string doctorQuery = "SELECT fname, lname, age, sex, phonenumber,email,address,visithours FROM doctor WHERE specializationid IN (" + specializationString + ") LIMIT 5";

            MySqlCommand cmd = new MySqlCommand(doctorQuery, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            System.Collections.Generic.List<Doctor> list = new List<Doctor>();
            //Read the data and store them in the list
            while (dataReader.Read())
            {
                Doctor doctor = new Doctor(dataReader[0].ToString(), dataReader[1].ToString(), dataReader[2].ToString(),
                                           dataReader[3].ToString(), dataReader[4].ToString(), dataReader[5].ToString(),
                                           dataReader[6].ToString(), dataReader[7].ToString());
                list.Add(doctor);
            }

            return list;
        }
    }
}
