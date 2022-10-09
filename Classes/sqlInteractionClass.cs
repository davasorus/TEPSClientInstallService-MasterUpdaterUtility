using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEPSClientInstallService_UpdateUtility.Classes;

namespace TEPSClientInstallService_Master_UpdateUtility.Classes
{
    public class sqlInteractionClass
    {
        private loggingClass loggingClass = new loggingClass();

        public bool checkDB()
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection($"Initial Catalog = TylerClientIMS; Server = {configData.DBname}; Trusted_Connection=True;"))
                {
                    sqlConnection.Open();
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                    {
                        sqlConnection.Close();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task buildOutDB(string script)
        {
            try
            {
                if (script.Contains("1 Create DB.sql"))
                {
                    string tempScript = File.ReadAllText(script);
                    using (SqlConnection sqlConnection = new SqlConnection($"Initial Catalog = master; Server = {configData.DBname}; Trusted_Connection=True;"))
                    {
                        SqlCommand command = new SqlCommand(tempScript, sqlConnection);
                        command.Connection.Open();
                        if (command.Connection.State == ConnectionState.Open)
                        {
                            command.ExecuteNonQuery();
                            sqlConnection.Close();

                            loggingClass.logEntryWriter($"{Path.GetFileName(script)} was run successfully", "info");
                        }
                    }
                }
                else
                {
                    string tempScript = File.ReadAllText(script);
                    using (SqlConnection sqlConnection = new SqlConnection($"Initial Catalog = TylerClientIMS; Server = {configData.DBname}; Trusted_Connection=True;"))
                    {
                        SqlCommand command = new SqlCommand(tempScript, sqlConnection);
                        command.Connection.Open();
                        if (command.Connection.State == ConnectionState.Open)
                        {
                            command.ExecuteNonQuery();
                            sqlConnection.Close();

                            loggingClass.logEntryWriter($"{Path.GetFileName(script)} was run successfully", "info");
                        }
                    }

                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
