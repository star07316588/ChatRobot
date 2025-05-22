public string GetExamType(string cerItemId)
{
    using (IDbConnection conn = new OracleConnection(AppConfig.ConnectString))
    {
        conn.Open();
        string sql = "SELECT exam_type FROM sbl_course WHERE course_id = :CerItemId";
        return conn.QueryFirstOrDefault<string>(sql, new { CerItemId = cerItemId });
    }
}