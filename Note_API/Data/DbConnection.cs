using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Note_API.Data
{
    public class DbConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DbConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DBConnection") ?? throw new InvalidOperationException("Database connection string is not configured.");
        }

        public IDbConnection CreateConnection()
        {
            try
            {
                return new SqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating database connection.", ex);
            }
        }
    }
}
