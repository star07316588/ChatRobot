using System; using System.Collections.Generic; using System.Data; using Oracle.ManagedDataAccess.Client; using YourNamespace.Models; // Replace with actual namespace

namespace YourNamespace.Repositories { public class ExamRepository { private readonly string _connectionString;

public ExamRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<ExamQuestion.QuestionResult> GetRandomTrueFalseAndChoiceQuestions(string cerItemId, int numberOfQuestions)
    {
        var results = new List<ExamQuestion.QuestionResult>();
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = @"
                SELECT * FROM (
                    SELECT subject, answer, type
                    FROM sbl_question_spec
                    WHERE type IN ('1', '2') AND cer_item_id = :cerItemId
                    ORDER BY DBMS_RANDOM.VALUE
                )
                WHERE ROWNUM <= :limit";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.Add(new OracleParameter("cerItemId", cerItemId));
                cmd.Parameters.Add(new OracleParameter("limit", numberOfQuestions));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new ExamQuestion.QuestionResult
                        {
                            Question = reader.GetString(0),
                            Answer = reader.GetString(1),
                            Type = reader.GetString(2) == "1" ? "TrueFalse" : "Choice"
                        });
                    }
                }
            }
        }
        return results;
    }

    public List<ExamQuestion.QuestionResult> GetLinkQuestions(string cerItemId, int linkSum, int linkSubSum)
    {
        var results = new List<ExamQuestion.QuestionResult>();
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = @"
                SELECT * FROM (
                    SELECT subject, answer, type, no
                    FROM sbl_question_spec
                    WHERE type = '3' AND cer_item_id = :cerItemId
                    ORDER BY DBMS_RANDOM.VALUE
                ) WHERE ROWNUM <= :totalSub";

            using (var cmd = new OracleCommand(sql, conn))
            {
                int totalNeeded = linkSum * linkSubSum;
                cmd.Parameters.Add(new OracleParameter("cerItemId", cerItemId));
                cmd.Parameters.Add(new OracleParameter("totalSub", totalNeeded));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new ExamQuestion.QuestionResult
                        {
                            Question = reader.GetString(0),
                            Answer = reader.GetString(1),
                            Type = "Link"
                        });
                    }
                }
            }
        }
        return results;
    }

    public int GetTotalExamTimeInSeconds(string cerItemId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            conn.Open();
            var sql = @"
                SELECT exam_time
                FROM sbl_course
                WHERE course_id = :cerItemId";

            using (var cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.Add(new OracleParameter("cerItemId", cerItemId));
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    return Convert.ToInt32(result) * 60; // convert minutes to seconds
                }
            }
        }
        return 600; // default 10 minutes
    }
}

}

