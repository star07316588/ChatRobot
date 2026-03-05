using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace YourProject.Services
{
    public class BonusCalculationService
    {
        private readonly string _connectionString;

        // 透過建構子注入連線字串
        public BonusCalculationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 單一員工獎金計算主流程 (整合了原本散落的 doSingleCalculateProcess 與 doCalculateProcess)
        /// </summary>
        public void CalculateSingleEmployeeBonus(string empId)
        {
            // 使用 using 確保連線正確關閉與釋放
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // 開啟資料庫交易
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 取得員工基本資料與證照 (整合原 getEmpByCondition)
                        var employee = GetEmployeeWithLicenses(conn, transaction, empId);
                        if (employee == null) return;

                        // 2. 取得獎金設定資料 - 累加法 (整合原 generateInit)
                        var bonusDataList = GetAccumulationBonusData(conn, transaction);

                        // 3. 執行計算邏輯
                        foreach (var data in bonusDataList)
                        {
                            // 檢查站別與職稱是否符合
                            if (employee.StationId != data.StationId || employee.Title != data.TitleId)
                            {
                                continue; // 不符合則跳過，避免過深的巢狀 if
                            }

                            int totalBonus = 0;
                            var historyItems = new List<BonusItem>();

                            // 計算各子項目獎金
                            foreach (var item in data.BonusItems)
                            {
                                var calculatedItem = DoCalculateBonus(conn, transaction, employee.Licenses, item);
                                historyItems.Add(calculatedItem);
                                totalBonus += calculatedItem.AGrade + calculatedItem.BGrade;
                            }

                            // --- 處理獎金上限與必要證照邏輯 (ReqNo:M200810044) ---
                            
                            // 1. 先與基本獎金上限比較
                            totalBonus = Math.Min(totalBonus, data.BonusLimit);

                            // 2. 抓取必要執照資訊
                            var nlLimits = GetNLBonusLimit(conn, transaction, employee.EmpId, data.StationId, data.TitleId, data.BonusMode);
                            
                            // nlLimits[0]: "0" 或空值代表已取得全部必要執照。若非這兩者，代表未全數取得。
                            // nlLimits[1]: 未全數取得時的獎金上限。
                            if (!string.IsNullOrEmpty(nlLimits[0]) && nlLimits[0] != "0")
                            {
                                int requiredLicenseLimit = int.Parse(nlLimits[1]);
                                totalBonus = Math.Min(totalBonus, requiredLicenseLimit);
                            }

                            // 4. 建立歷史紀錄並儲存
                            var history = new BonusHistory
                            {
                                EmpId = employee.EmpId,
                                DateMonth = DateTime.Now.ToString("yyyyMM"),
                                TotalBonus = totalBonus,
                                Items = historyItems
                            };

                            history.SaveToDB(conn, transaction);
                        }

                        // 全部成功後 Commit
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // 發生錯誤時 Rollback
                        transaction.Rollback();
                        // TODO: 這裡可以加入 NLog 或 log4net 等日誌記錄
                        System.Diagnostics.Debug.WriteLine($"Error calculating bonus for {empId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        #region 資料存取與輔助方法 (Data Access Stubs)
        // 以下為原本外部呼叫的資料存取方法，建議使用 SqlCommand + 參數化查詢，或使用 Dapper/Entity Framework

        private Emp GetEmployeeWithLicenses(SqlConnection conn, SqlTransaction trans, string empId)
        {
            // TODO: 實作 "select emp_id, station_id, title from sbl_emp where dlt_date is null and emp_id = @EmpId"
            // 並去抓取對應的 Licence
            throw new NotImplementedException();
        }

        private List<BonusData> GetAccumulationBonusData(SqlConnection conn, SqlTransaction trans)
        {
            // TODO: 實作 "select * from sbl_bonus_data where dlt_date is null and bonus_mode = '累加法'"
            // 並將結果 Mapping 到 BonusData 物件中
            throw new NotImplementedException();
        }

        private BonusItem DoCalculateBonus(SqlConnection conn, SqlTransaction trans, Dictionary<string, string> licenses, BonusItem item)
        {
            // TODO: 實作子項目計算邏輯
            throw new NotImplementedException();
        }

        private string[] GetNLBonusLimit(SqlConnection conn, SqlTransaction trans, string empId, string stationId, string title, string bonusMode)
        {
            // TODO: 實作抓取必要證照限制邏輯
            throw new NotImplementedException();
        }
        #endregion
    }
}
