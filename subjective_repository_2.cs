using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient; // 或 Oracle.ManagedDataAccess.Client
using System.Text;
using System.Linq;
using MyPerformanceApp.Models;

namespace MyPerformanceApp.Services.Repository
{
    public class SubjectiveRepository : ISubjectiveRepository
    {
        private readonly string _connStr;

        public SubjectiveRepository(string connStr)
        {
            _connStr = connStr;
        }

        public UserProfile GetUserProfile(string empId)
        {
            // 對應 JSP: select emp_id, dept_id... from sbl_emp
            // 並包含取得 section 的邏輯
            string sql = @"
                SELECT e.emp_id, e.dept_id, e.shift_id, e.title, e.station_id, 
                       o.section
                FROM sbl_emp e
                LEFT JOIN rbl_dl_organization o 
                       ON e.dept_id = o.dept_id AND e.station_id = o.station_id AND o.deleteflag='N'
                WHERE e.emp_id = @EmpId";
            
            // 執行 SQL 並回傳 UserProfile 物件...
            return new UserProfile(); // 實作省略
        }

        public List<EmployeeGradeRow> GetGradeList(SubjectiveQuery query, string section)
        {
            // 這是 JSP 中最複雜的查詢部分 (Line 130-137 + Loop Line 143)
            // 這裡將其重構為一次取出的 SQL (Left Join)
            
            var sb = new StringBuilder();
            sb.Append(@"
                SELECT 
                    e.dept_id, e.station_id, e.shift_id, e.emp_id, e.name, e.position_group, e.title,
                    s.rowid, s.record, s.score, s.comments
                FROM rbl_dl_emp e
                LEFT JOIN Rbl_DL_Performance_subject s 
                    ON e.emp_id = s.emp_id 
                    AND s.year = @Year 
                    AND s.month = @Month 
                    AND s.item = @Item 
                    AND s.detailitem = @DetailItem
                WHERE e.section = @Section
                  AND e.dept_id = @DeptId
                  AND e.title = @Title
            ");

            // 動態條件 (對應 JSP Line 135-136)
            if (query.ShiftIds != null && query.ShiftIds.Any())
            {
                // 實務上請使用參數化陣列處理，這裡示意用 IN
                string shifts = string.Join("','", query.ShiftIds);
                sb.Append($" AND e.shift_id IN ('{shifts}')"); 
            }

            if (query.StationIds != null && query.StationIds.Any())
            {
                string stations = string.Join("','", query.StationIds);
                sb.Append($" AND e.station_id IN ('{stations}')");
            }

            sb.Append(" ORDER BY e.emp_id");

            // 執行 SQL
            // 使用 Dapper: connection.Query<EmployeeGradeRow>(sb.ToString(), query);
            return new List<EmployeeGradeRow>(); 
        }

        public SubjectiveItemConfig GetItemConfig(string stationId, string title, string item, string detailItem)
        {
            // 對應 JSP Line 108 & Line 109 (Remark 查詢)
            // 將兩個 SQL 合併或分別查詢後組裝
            string sql = @"
                SELECT weighting, remark as ItemRemark, upperbound, lowerbound 
                FROM Rbl_DL_item 
                WHERE title = @Title AND item = @Item AND station_id = @StationId";
            
            // 執行並回傳...
            return new SubjectiveItemConfig();
        }

        public void SaveGrade(EmployeeGradeRow row, SubjectiveQuery query, string modifierId)
        {
            // 對應 JSP Line 120 (Update) 和 Line 124 (Insert)
            string sql;
            if (!string.IsNullOrEmpty(row.RowId))
            {
                sql = @"
                    UPDATE Rbl_DL_Performance_subject 
                    SET record = @Record, score = @Score, comments = @Comments,
                        rankinga = FUN_GET_SUBJECT_RANKINGA(@Title, @Record, @TotalCount),
                        updateuserid = @ModifierId, updatetime = TO_CHAR(SYSDATE,'yyyymmdd hh24miss') || '000'
                    WHERE rowid = @RowId";
            }
            else
            {
                sql = @"
                    INSERT INTO Rbl_DL_Performance_subject 
                    (emp_id, year, month, item, detailitem, record, score, comments, rankinga, createuserid, createtime)
                    VALUES 
                    (@EmpId, @Year, @Month, @Item, @DetailItem, @Record, @Score, @Comments, 
                     FUN_GET_SUBJECT_RANKINGA(@Title, @Record, @TotalCount), 
                     @ModifierId, TO_CHAR(SYSDATE,'yyyymmdd hh24miss')||'000')";
            }

            // 執行 SQL...
        }

        public string GetStatus(SubjectiveQuery query, string type)
        {
            // 對應 JSP Line 111 (Status Check)
            string sql = @"
                SELECT status FROM Rbl_DL_Status 
                WHERE year = @Year AND month = @Month AND dept_id = @DeptId AND title = @Title AND type = 'SUBJECT'";
            
            // 加上 ShiftId 和 StationId 的動態條件...
            // 執行並回傳 status 字串
            return "LEADER PROCESSING"; // 範例回傳
        }
        
        public void LogConfidential(string empId, string action, string sqlContent)
        {
             // 對應 jdbc.insertconfidential
             // 實作 Insert into Confidential Table
        }

        public void UpdateStatusToClose(SubjectiveQuery query, string currentStatus, string modifierId)
        {
            // 對應 JSP Line 128 (Update Status)
            string nextStatus = "";
            if (currentStatus == "LEADER") nextStatus = "LEADER OK";
            else if (currentStatus == "SUPERVISOR") nextStatus = "SUPERVISOR OK";
            // ... 邏輯判斷
            
            string sql = "UPDATE rbl_dl_status SET status = @NextStatus ...";
            // 執行 SQL
        }

        public bool CheckAllItemsFilled(SubjectiveQuery query)
        {
            // 對應 utility.CloseCheck 
            // 這裡可能是一個 Stored Procedure 或者是複雜的 SQL count(*) 比較
            return true;
        }

        // ... GetProductionDepts, GetShifts 等 Dropdown 方法依照 JSP 邏輯實作 ...
        public List<string> GetProductionDepts() { return new List<string>(); }
        public List<string> GetShifts(string deptId, string section) { return new List<string>(); }
        public List<string> GetStations(string deptId, string section) { return new List<string>(); }
        public List<string> GetItems(string stationId, string title) { return new List<string>(); }
        public List<string> GetDetailItems(string stationId, string title, string item) { return new List<string>(); }
    }
}
