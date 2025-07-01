using System; using System.Collections.Generic; using System.Data; using System.Data.SqlClient; using System.Linq; using Dapper; using MesTAManagementSystem_New.Models.Exam;

namespace MesTAManagementSystem_New.Repositories { public class ExamRepository { private readonly string _connectionString;

public ExamRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public List<string> GetCerItemIds(string empId)
    {
        using (var conn = CreateConnection())
        {
            string sql = @"SELECT a.cer_item_id
                            FROM sbl_cer_reg a
                            JOIN sbl_course b ON a.cer_item_id = b.course_id
                            WHERE a.emp_id = @empId
                              AND a.score_writing IS NULL
                              AND FORMAT(a.cer_date, 'yyyyMM') = FORMAT(GETDATE(), 'yyyyMM')
                              AND b.EXAM_TYPE LIKE 'WR%'";

            return conn.Query<string>(sql, new { empId }).ToList();
        }
    }

    public List<string> GetStationIds(string cerItemId)
    {
        using (var conn = CreateConnection())
        {
            string sql = @"SELECT a.station_id
                           FROM sbl_cer_item a
                           WHERE a.cer_item_id = @cerItemId
                             AND a.dlt_user IS NULL
                             AND a.dlt_date IS NULL
                           ORDER BY a.station_id";

            return conn.Query<string>(sql, new { cerItemId }).ToList();
        }
    }

    public ExamQuestions GetExamQuestions(string empId, string cerItemId)
    {
        using (var conn = CreateConnection())
        {
            // 注意：這裡只做出簡化版樣式，實作中可增加必要欄位與條件
            var questions = new ExamQuestions
            {
                NecessaryQuestions = conn.Query<QuestionVM>(@"SELECT subject AS QuestionText, answer AS Answer
                                                               FROM sbl_question_spec
                                                               WHERE type = '4' AND cer_item_id = @cerItemId",
                                                               new { cerItemId }).ToList(),

                TrueFalseQuestions = conn.Query<QuestionVM>(@"SELECT subject AS QuestionText, answer AS Answer
                                                               FROM sbl_question_spec
                                                               WHERE type = '1' AND cer_item_id = @cerItemId",
                                                               new { cerItemId }).ToList(),

                ChoiceQuestions = conn.Query<QuestionVM>(@"SELECT subject AS QuestionText, answer AS Answer
                                                              FROM sbl_question_spec
                                                              WHERE type = '2' AND cer_item_id = @cerItemId",
                                                              new { cerItemId }).ToList()
            };

            return questions;
        }
    }

    public void SaveExamResult(string empId, string cerItemId, List<string> answers)
    {
        using (var conn = CreateConnection())
        {
            // 此處僅示意，可以改為儲存至具體的資料表與更完整的內容
            string sql = @"INSERT INTO exam_results (emp_id, cer_item_id, answer, created_at)
                           VALUES (@empId, @cerItemId, @answer, GETDATE())";

            foreach (var answer in answers)
            {
                conn.Execute(sql, new { empId, cerItemId, answer });
            }
        }
    }
}

}

