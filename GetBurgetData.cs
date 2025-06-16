// ReportRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using MesTAManagementSystem_New.Models.Reports;

namespace MesTAManagementSystem_New.Repositories
{
    public class ReportRepository
    {
        private readonly IDbConnection _dbConnection;

        public ReportRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public List<BudgetReportModel> GetBudgetData(string year, string deptId, string userId, string func)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Year1", year);
            parameters.Add("Dept1", deptId);
            parameters.Add("Year2", year);
            parameters.Add("Dept2", deptId);

            string sql = @"
                SELECT * FROM (
                    SELECT a.year, a.dept_id, a.shift_id, a.title, 'Budget' AS type,
                           CASE a.title WHEN 'LEADER' THEN 1 WHEN 'TA' THEN 2 ELSE 3 END AS titleorder,
                           MAX(CASE month WHEN '01' THEN budget ELSE NULL END) AS jan,
                           MAX(CASE month WHEN '02' THEN budget ELSE NULL END) AS feb,
                           MAX(CASE month WHEN '03' THEN budget ELSE NULL END) AS mar,
                           MAX(CASE month WHEN '04' THEN budget ELSE NULL END) AS apr,
                           MAX(CASE month WHEN '05' THEN budget ELSE NULL END) AS may,
                           MAX(CASE month WHEN '06' THEN budget ELSE NULL END) AS jun,
                           MAX(CASE month WHEN '07' THEN budget ELSE NULL END) AS jul,
                           MAX(CASE month WHEN '08' THEN budget ELSE NULL END) AS aug,
                           MAX(CASE month WHEN '09' THEN budget ELSE NULL END) AS sep,
                           MAX(CASE month WHEN '10' THEN budget ELSE NULL END) AS oct,
                           MAX(CASE month WHEN '11' THEN budget ELSE NULL END) AS nov,
                           MAX(CASE month WHEN '12' THEN budget ELSE NULL END) AS dec
                    FROM (
                        SELECT t.year, t.month, t.dept_id, t.shift_id, t.title,
                               SUM(NVL(t.budget, 0)) AS budget,
                               SUM(NVL(t.actual, 0)) AS actual,
                               SUM(NVL(t.balance, 0)) AS balance
                        FROM dlowner.rbl_dl_budgetcontrol t
                        WHERE t.year = :Year1 AND t.dept_id = :Dept1
                        GROUP BY t.year, t.month, t.dept_id, t.shift_id, t.title
                    ) a
                    GROUP BY a.year, a.dept_id, a.shift_id, a.title
                    
                    UNION

                    SELECT a.year, a.dept_id, a.shift_id, a.title, 'Actual' AS type,
                           CASE a.title WHEN 'LEADER' THEN 1 WHEN 'TA' THEN 2 ELSE 3 END AS titleorder,
                           MAX(CASE month WHEN '01' THEN actual ELSE NULL END) AS jan,
                           MAX(CASE month WHEN '02' THEN actual ELSE NULL END) AS feb,
                           MAX(CASE month WHEN '03' THEN actual ELSE NULL END) AS mar,
                           MAX(CASE month WHEN '04' THEN actual ELSE NULL END) AS apr,
                           MAX(CASE month WHEN '05' THEN actual ELSE NULL END) AS may,
                           MAX(CASE month WHEN '06' THEN actual ELSE NULL END) AS jun,
                           MAX(CASE month WHEN '07' THEN actual ELSE NULL END) AS jul,
                           MAX(CASE month WHEN '08' THEN actual ELSE NULL END) AS aug,
                           MAX(CASE month WHEN '09' THEN actual ELSE NULL END) AS sep,
                           MAX(CASE month WHEN '10' THEN actual ELSE NULL END) AS oct,
                           MAX(CASE month WHEN '11' THEN actual ELSE NULL END) AS nov,
                           MAX(CASE month WHEN '12' THEN actual ELSE NULL END) AS dec
                    FROM (
                        SELECT t.year, t.month, t.dept_id, t.shift_id, t.title,
                               SUM(NVL(t.budget, 0)) AS budget,
                               SUM(NVL(t.actual, 0)) AS actual,
                               SUM(NVL(t.balance, 0)) AS balance
                        FROM dlowner.rbl_dl_budgetcontrol t
                        WHERE t.year = :Year2 AND t.dept_id = :Dept2
                        GROUP BY t.year, t.month, t.dept_id, t.shift_id, t.title
                    ) a
                    GROUP BY a.year, a.dept_id, a.shift_id, a.title
                )
                ORDER BY year, dept_id, shift_id, titleorder, type DESC
            ";

            // Optional: insert logging logic if required, ex:
            // _logService.InsertConfidential("DL", userId, func, sql);

            return _dbConnection.Query<BudgetReportModel>(sql, parameters).ToList();
        }
    }
}

