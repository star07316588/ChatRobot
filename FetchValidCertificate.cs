public CertInfo FetchValidCertificate(string cerItemId, string role, string month)
{
    using (IDbConnection conn = new OracleConnection(AppConfig.ConnectString))
    {
        conn.Open();
        string sql = @"
            SELECT cer_id, TO_CHAR(cer_date, 'yyyymm') AS cer_date
            FROM sbl_certificate
            WHERE cer_item_id = :CerItemId";

        if (role == "OpSupervisor-Leader")
            sql += " AND TO_CHAR(cer_date, 'yyyymm') = TO_CHAR(ADD_MONTHS(SYSDATE, 1), 'yyyymm')";
        else if (role == "OpTrainingGroup")
            sql += " AND TO_CHAR(cer_date, 'yyyymm') = TO_CHAR(SYSDATE, 'yyyymm')";
        else
            sql += month == "next_month"
                ? " AND TO_CHAR(cer_date, 'yyyymm') = TO_CHAR(ADD_MONTHS(SYSDATE, 1), 'yyyymm')"
                : " AND TO_CHAR(cer_date, 'yyyymm') = TO_CHAR(SYSDATE, 'yyyymm')";

        return conn.QueryFirstOrDefault<CertInfo>(sql, new { CerItemId = cerItemId });
    }
}
