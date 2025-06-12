using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using MesTAManagementSystem_New.Models.Training.Testing;

namespace MesTAManagementSystem_New.Repositories
{
    public class TestingRepository
    {
        private readonly string _connectionString;

        public TestingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<ScoreQueryModel> GetShiftList()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT DISTINCT shift_id AS ShiftId
                    FROM sbl_emp
                    ORDER BY shift_id";

                return db.Query<ScoreQueryModel>(sql).AsList();
            }
        }
    }
}