using System; using System.Collections.Generic; using System.Data; using System.Data.OracleClient; using MesTAManagementSystem_New.Models.Exam;

namespace MesTAManagementSystem_New.Repositories { public class ExamRepository { private readonly string _connectionString;

public ExamRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<string> GetCerItemIds(string empId)
    {
        var result = new List<string>();
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = @"
                SELECT DISTINCT a.cer_item_id
                FROM sbl_cer_reg a
                JOIN sbl_course b ON a.cer_item_id = b.course_id
                WHERE a.emp_id = :empId
                  AND a.score_writing IS NULL
                  AND TO_CHAR(a.cer_date, 'yyyymm') = TO_CHAR(SYSDATE, 'yyyymm')
                  AND b.EXAM_TYPE LIKE 'WR%'";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("empId", empId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(reader.GetString(0));
                }
            }
        }
        return result;
    }

    public List<string> GetStationIds(string cerItemId)
    {
        var result = new List<string>();
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = @"
                SELECT station_id
                FROM sbl_cer_item
                WHERE cer_item_id = :cerItemId
                  AND dlt_user IS NULL AND dlt_date IS NULL";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("cerItemId", cerItemId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(reader.GetString(0));
                }
            }
        }
        return result;
    }

    public List<ExamQuestion> GetNecessaryQuestions(string cerItemId)
    {
        var result = new List<ExamQuestion>();
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = @"
                SELECT subject, answer
                FROM sbl_question_spec
                WHERE type = '4' AND cer_item_id = :cerItemId
                ORDER BY TO_NUMBER(REPLACE(no,'-','.'))";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("cerItemId", cerItemId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ExamQuestion
                        {
                            Question = reader.GetString(0),
                            Answer = reader.GetString(1),
                            Type = "Necessary"
                        });
                    }
                }
            }
        }
        return result;
    }

    public List<ExamQuestion> GetTrueFalseAndChoiceQuestions(string cerItemId, int limit)
    {
        var result = new List<ExamQuestion>();
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = $@"
                SELECT * FROM (
                    SELECT subject, answer, type
                    FROM sbl_question_spec
                    WHERE type IN ('1', '2') AND cer_item_id = :cerItemId
                    ORDER BY DBMS_RANDOM.VALUE
                ) WHERE ROWNUM <= {limit}";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("cerItemId", cerItemId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ExamQuestion
                        {
                            Question = reader.GetString(0),
                            Answer = reader.GetString(1),
                            Type = reader.GetString(2) == "1" ? "TrueFalse" : "Choice"
                        });
                    }
                }
            }
        }
        return result;
    }

    public bool CheckExamInCurrentMonth(string empId, string cerItemId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = @"
                SELECT COUNT(*)
                FROM sbl_cer_reg
                WHERE emp_id = :empId
                  AND cer_item_id = :cerItemId
                  AND TO_CHAR(cer_date, 'yyyymm') = TO_CHAR(SYSDATE, 'yyyymm')";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("empId", empId);
                cmd.Parameters.AddWithValue("cerItemId", cerItemId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }

    // 可繼續補上 Link 題型與成績儲存方法
}

public class ExamQuestion
{
    public string Question { get; set; }
    public string Answer { get; set; }
    public string Type { get; set; } // Necessary, TrueFalse, Choice, Link
}

}

