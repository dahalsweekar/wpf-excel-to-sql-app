using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Configuration;
using System.Windows.Markup;

namespace ExcelToSQLTable
{
    class DB
    {
        private string _connectionString = string.Empty;
        public DB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string LoadDatabase()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["UserDB"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    conn.Close();
                }
                return "success";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting: {ex.Message}");
                return ex.Message.ToString();
            }
        }

        public (string, string) writeSQL(List<string> headerRow, List<List<string>> dataRows, string tableName)
        {
            string create_query = $"CREATE TABLE {tableName} ( Id INT IDENTITY(1,1) PRIMARY KEY,";
            string insert_query = $"INSERT INTO {tableName} (";
            foreach (var hrow in headerRow)
            {
                create_query += $"[{hrow}] VARCHAR(100),";
                insert_query += $"[{hrow}],";
            }
            create_query = create_query.Remove(create_query.Length - 1);
            create_query += ");";
            insert_query = insert_query.Remove(insert_query.Length - 1);
            insert_query += ") VALUES ";
            foreach (var row in dataRows)
            {
                insert_query += "(";
                foreach (var d in row)
                {
                    insert_query += $"'{parseText(d)}',";
                }
                insert_query = insert_query.Remove(insert_query.Length - 1);
                insert_query += "),";
            }
            insert_query = insert_query.Remove(insert_query.Length - 1);
            insert_query += ";";

            return (create_query, insert_query);
        }

        public string ConvertToTable(List<string> headerRow, List<List<string>> dataRows, string tableName)
        {
            string createTableSql = string.Empty;
            string insertTableSql = string.Empty;
            (createTableSql, insertTableSql) = writeSQL(headerRow, dataRows, tableName);
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand(insertTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                    return $"Parsing successful. Table {tableName} created successfully.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string parseText(string rawText)
        {
            string escaped = rawText.Replace("'", "''");
            return escaped;
        }
    }
}
