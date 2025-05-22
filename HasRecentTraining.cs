public bool HasRecentTraining(string empId, string cerItemId)
{
    using (IDbConnection conn = new OracleConnection(AppConfig.ConnectString))
    {
        conn.Open();
        string sql = @"
            SELECT 1
            FROM sbl_training_reg b
            WHERE b.training_no IN (
                SELECT a.training_no
                FROM sbl_training a
                JOIN sbl_training_reg b2 ON a.training_no = b2.training_no
                WHERE a.course_id = :CerItemId
                  AND TO_CHAR(a.training_date, 'yyyymmdd') >= TO_CHAR(ADD_MONTHS(SYSDATE, -3), 'yyyymmdd')
            )
            AND b.emp_id = :EmpId
            AND b.training_flag = 'Y'";

        var result = conn.QueryFirstOrDefault(sql, new { EmpId = empId, CerItemId = cerItemId });
        return result != null;
    }
}