public void SetScoreWritingZero(string empId, string cerItemId)
{
    using (var conn = new OracleConnection(_connectionString))
    {
        conn.Open();
        var sql = @"
            UPDATE sbl_cer_reg
            SET score_writing = 0
            WHERE emp_id = :empId
              AND cer_item_id = :cerItemId
              AND TO_CHAR(cer_date, 'yyyymm') = TO_CHAR(SYSDATE, 'yyyymm')
              AND score_writing IS NULL";

        using (var cmd = new OracleCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("empId", empId);
            cmd.Parameters.AddWithValue("cerItemId", cerItemId);
            cmd.ExecuteNonQuery();
        }
    }
}