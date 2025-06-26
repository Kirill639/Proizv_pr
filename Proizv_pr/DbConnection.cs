using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proizv_pr
{
    public class DbConnection : IDisposable
    {
        private const string ConnectionString =
            "Host=172.20.7.53;Port=5432;Username=st3996;Password=pwd3996;Database=db3996_01";

        private readonly NpgsqlConnection _connection;

        public DbConnection()
        {
            _connection = new NpgsqlConnection(ConnectionString);
        }

        public void Open() => _connection.Open();
        public void Close() => _connection.Close();
        public NpgsqlCommand CreateCommand(string commandText) => new NpgsqlCommand(commandText, _connection);

        public void Dispose()
        {
            Close();
            _connection?.Dispose();
        }

        
        public object ExecuteScalar(string query, params NpgsqlParameter[] parameters)
        {
            var cmd = CreateCommand(query);
            try
            {
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
            finally
            {
                cmd.Dispose();
            }
        }

      
    }
}
