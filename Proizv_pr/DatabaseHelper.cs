using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Proizv_pr
{
    public class DatabaseHelper : IDisposable
    {
        private readonly DbConnection _dbConnection;

        public DatabaseHelper()
        {
            _dbConnection = new DbConnection();
        }
        public object ExecuteScalar(string query, params NpgsqlParameter[] parameters)
        {
            using (var command = _dbConnection.CreateCommand(query))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }
        public DataTable ExecuteQuery(string query, params NpgsqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            using (var command = _dbConnection.CreateCommand(query))
            {
                command.Parameters.AddRange(parameters);

                using (var adapter = new NpgsqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
        }
        public (string role, int id)? Authenticate(string login, string password)
        {
            try
            {
                _dbConnection.Open();

                var hashedPassword = CalculateMd5Hash(password);

                var teacherId = _dbConnection.ExecuteScalar(
                    "SELECT teacher_id FROM proizv_pr.teachers WHERE login = @login AND password = @hashedPassword",
                    new NpgsqlParameter("@login", login),
                    new NpgsqlParameter("@hashedPassword", hashedPassword));

                if (teacherId != null)
                    return ("teacher", Convert.ToInt32(teacherId));

                var studentId = _dbConnection.ExecuteScalar(
                    "SELECT student_id FROM proizv_pr.students WHERE login = @login AND password = @hashedPassword",
                    new NpgsqlParameter("@login", login),
                    new NpgsqlParameter("@hashedPassword", hashedPassword));

                if (studentId != null)
                    return ("student", Convert.ToInt32(studentId));

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
                throw;
            }
        }
        private string CalculateMd5Hash(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public void Dispose()
        {
            _dbConnection?.Dispose();
        }

    }
}
