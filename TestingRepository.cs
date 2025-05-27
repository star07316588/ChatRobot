using System;
using System.Collections.Generic;
using System.Data;
using MesTAManagementSystem_New.App_Start;
using Oracle.DataAccess.Client;
using Dapper;
using NPOI.SS.UserModel;
using System.Linq;
using MesTAManagementSystem_New.ViewModels.Training.Testing;

namespace MesTAManagementSystem_New.Repositories
{
    public class TestingRepository
    {
        //private IDbConnection _connection = null;

        /// <summary>
        /// TestingRepository
        /// </summary>
        public TestingRepository()
        {
            //this._connection = new OracleConnection(AppConfig.ConnectString);

            //if (this._connection.State != ConnectionState.Open)
            //{
            //    this._connection.Open();
            //}
        }

        public IEnumerable<string> GetStationIdList()
        {
            using (IDbConnection connection = new OracleConnection(AppConfig.ConnectString))
            {
                connection.Open();
                var result = connection.Query<string>("SELECT station_Id FROM sbl_station");
                return result;
            }
        }

        public IEnumerable<string> GetCerItemIdList(string stationId)
        {
            using (IDbConnection connection = new OracleConnection(AppConfig.ConnectString))
            {
                connection.Open();
                var sql = @"SELECT cer_item_id 
                    FROM sbl_cer_item 
                    WHERE station_id = :StationId 
                      AND dlt_user IS NULL 
                      AND dlt_date IS NULL 
                    ORDER BY cer_item_id";

                var result = connection.Query<string>(sql, new { StationId = stationId });
                return result;
            }
        }

        public IEnumerable<string> GetTestSpecList(string TableName, string certItemId)
        {
            using (IDbConnection connection = new OracleConnection(AppConfig.ConnectString))
            {
                string sql = $"SELECT COUNT(*) FROM {TableName} WHERE cer_item_id = :CertItemId";
                var result = connection.Query<string>(sql, new { CertItemId = certItemId });
                return result;
            }
        }

        public List<QuestionVM> GetTestSpec(string TableName, string certItemId)
        {
            using (IDbConnection connection = new OracleConnection(AppConfig.ConnectString))
            {
                string sql = $"SELECT * FROM {TableName} WHERE cer_item_id = :CertItemId";
                var data = connection.Query<QuestionVM>(sql, new { CertItemId = certItemId }).ToList();
                return data;
            }
        }

        public string SaveToQuestionSpec(string filenames, string tableName, string certItemId, ISheet sheet)
        {
            var result = "";
            try
            {
                var files = filenames.Split(',');
                using (IDbConnection connection = new OracleConnection(AppConfig.ConnectString))
                {
                    connection.Open();
                    string sql = $"DELETE FROM {tableName} WHERE cer_item_id = :CertItemId";
                    int iExecute = connection.Execute(sql, new { CertItemId = certItemId });

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        if (row == null) continue;

                        // 依欄位順序讀取資料
                        string type = row.GetCell(0)?.ToString().Trim();
                        string no = row.GetCell(1)?.ToString().Trim();
                        string subNo = row.GetCell(2)?.ToString().Trim();
                        string subject = row.GetCell(3)?.ToString().Trim();
                        string fileName = row.GetCell(4)?.ToString().Trim();
                        string answer = row.GetCell(5)?.ToString().Trim();
                        string sponsor = row.GetCell(6)?.ToString().Trim();
                        string deleteFlag = row.GetCell(7)?.ToString().Trim();

                        sql = $@"
                    INSERT INTO sbl_question_spec (
                        CER_ITEM_ID, TYPE, NO, SUB_NO, SUBJECT, 
                        FILE_NAME, ANSWER, SPONSOR, DELETE_FLAG
                    ) VALUES (
                        :certItemId, :type, :no, :subNo, :subject,
                        :fileName, :answer, :sponsor, :deleteFlag
                    )";

                        using (var cmd = (OracleCommand)connection.CreateCommand())
                        {
                            cmd.CommandText = sql;
                            cmd.Parameters.Add(":certItemId", certItemId);
                            cmd.Parameters.Add(":type", type);
                            cmd.Parameters.Add(":no", no);
                            cmd.Parameters.Add(":subNo", subNo);
                            cmd.Parameters.Add(":subject", subject);
                            cmd.Parameters.Add(":fileName", fileName);
                            cmd.Parameters.Add(":answer", answer);
                            cmd.Parameters.Add(":sponsor", sponsor);
                            cmd.Parameters.Add(":deleteFlag", deleteFlag);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public string UpdateQuestion(string tableName, QuestionVM vm)
        {
            var result = "";
            try
            {
                using (IDbConnection connection = new OracleConnection(AppConfig.ConnectString))
                {
                    connection.Open();
                    string sql = $"SELECT * FROM {tableName} WHERE cer_item_id = :CertItemId";
                    var data = connection.Query<QuestionVM>(sql, new { CertItemId = vm.CertItemId }).ToList();

                    if (data.Count > 0)
                    {
                        sql = @"
                            UPDATE sbl_question_spec
                            SET
                                SUBJECT = :Subject,
                                FILE_NAME = :FileName,
                                ANSWER = :Answer,
                                SPONSOR = :Sponsor,
                                DELETE_FLAG = :Delete_Flag
                            WHERE
                                CER_ITEM_ID = :CertItemId AND
                                (TYPE = :Type OR (TYPE IS NULL AND :Type IS NULL)) AND
                                (NO = :No OR (NO IS NULL AND :No IS NULL)) AND
                                (SUB_NO = :SubNo OR (SUB_NO IS NULL AND :SubNo IS NULL))
                        ";

                        var param = new
                        {
                            vm.CertItemId,
                            vm.Type,
                            vm.No,
                            vm.SubNo,
                            vm.Subject,
                            vm.FileName,
                            vm.Answer,
                            vm.Sponsor,
                            vm.Delete_Flag
                        };

                        int affectedRows = connection.Execute(sql, param);
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

public void BackupCerReg(string cerRegNo, string userId)
{
    using (IDbConnection conn = new OracleConnection(AppConfig.ConnectString))
    {
        conn.Open();
        string sql = @"
            INSERT INTO sbl_cer_reg_delete (
                CER_REG_NO, CER_ID, EMP_ID, REG_DATE, SCORE_WRITING, SCORE_OPER, OPER_FAIL,
                CER_DATE, GRADE, TESTER, CRT_USER, CRT_DATE, UPT_USER, UPT_DATE,
                DLT_USER, DLT_DATE, CER_ITEM_ID, RETEST_NO, DATE_SET, WR_DATE,
                OP_DATE, WR_FILE, OP_FILE
            )
            SELECT
                CER_REG_NO, CER_ID, EMP_ID, REG_DATE, SCORE_WRITING, SCORE_OPER, OPER_FAIL,
                CER_DATE, GRADE, TESTER, CRT_USER, CRT_DATE, UPT_USER, UPT_DATE,
                :UserId, SYSDATE, CER_ITEM_ID, RETEST_NO, DATE_SET, WR_DATE,
                OP_DATE, WR_FILE, OP_FILE
            FROM sbl_cer_reg
            WHERE cer_reg_no = :CerRegNo";

        conn.Execute(sql, new { CerRegNo = cerRegNo, UserId = userId });
    }
}

public void DeleteCerReg(string cerRegNo, string userId)
{
    using (IDbConnection conn = new OracleConnection(AppConfig.ConnectString))
    {
        conn.Open();
        string sql = "DELETE FROM sbl_cer_reg WHERE cer_reg_no = :CerRegNo";
        conn.Execute(sql, new { CerRegNo = cerRegNo });
    }
}
    }
}
