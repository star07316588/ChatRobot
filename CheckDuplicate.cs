public bool CheckDuplicate(string cerId, string cerDate, string empId, string cerItemId)
{
    using (IDbConnection conn = new OracleConnection(AppConfig.ConnectString))
    {
        conn.Open();
        string sql = @"
            SELECT 1 FROM sbl_cer_reg
            WHERE cer_id = :CerId
              AND TO_CHAR(cer_date, 'yyyymm') = :CerDate
              AND emp_id = :EmpId
              AND cer_item_id = :CerItemId";

        var result = conn.QueryFirstOrDefault(sql, new { CerId = cerId, CerDate = cerDate, EmpId = empId, CerItemId = cerItemId });
        return result != null;
    }
}
