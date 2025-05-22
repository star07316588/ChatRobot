public void InsertRegistration(CertRegisterVM vm)
{
    using (IDbConnection conn = new OracleConnection(AppConfig.ConnectString))
    {
        conn.Open();
        string sql = @"
            INSERT INTO sbl_cer_reg (
                cer_reg_no, cer_id, emp_id, reg_date, cer_date,
                crt_user, crt_date, cer_item_id, score_writing, score_oper
            ) VALUES (
                cer_reg_seq.NEXTVAL,
                :CerId,
                :EmpId,
                SYSDATE,
                TO_DATE(:CerDate, 'yyyymm'),
                :CreateUser,
                SYSDATE,
                :CertItemId,
                :ScoreWR,
                :ScoreOP
            )";

        conn.Execute(sql, new
        {
            vm.CerId,
            vm.EmpId,
            vm.CerDate,
            vm.CreateUser,
            vm.CertItemId,
            vm.ScoreWR,
            vm.ScoreOP
        });
    }
}