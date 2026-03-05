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

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper; // 記得引入 Dapper

namespace YourProject.Services
{
    public partial class BonusCalculationService
    {
        // --------------------------------------------------------
        // 以下為原本 TODO 的實作
        // --------------------------------------------------------

        /// <summary>
        /// 取得員工基本資料與其擁有的證照
        /// </summary>
        private Emp GetEmployeeWithLicenses(SqlConnection conn, SqlTransaction trans, string empId)
        {
            // 1. 查詢員工基本資料
            string empSql = @"
                SELECT emp_id AS EmpId, station_id AS StationId, title AS Title 
                FROM sbl_emp 
                WHERE dlt_date IS NULL AND emp_id = @EmpId";
            
            // Dapper 的 QueryFirstOrDefault 會自動將查詢結果 mapping 到 Emp 物件
            var employee = conn.QueryFirstOrDefault<Emp>(empSql, new { EmpId = empId }, trans);

            if (employee != null)
            {
                // 2. 查詢該員工的證照資料 (假設資料表為 sbl_licence)
                string licSql = @"
                    SELECT licence_id, licence_name 
                    FROM sbl_licence 
                    WHERE emp_id = @EmpId AND station_id = @StationId";
                
                // 將查詢結果轉為 Dictionary (對應原本 Java 的 HashMap lics = employee[i].getLicence())
                var licenses = conn.Query(licSql, new { EmpId = empId, StationId = employee.StationId }, trans)
                                   .ToDictionary(row => (string)row.licence_id, row => (string)row.licence_name);
                
                employee.Licenses = licenses;
            }

            return employee;
        }

        /// <summary>
        /// 取得「累加法」的獎金設定資料與其子項目
        /// </summary>
        private List<BonusData> GetAccumulationBonusData(SqlConnection conn, SqlTransaction trans)
        {
            // 1. 取得主檔
            string dataSql = @"
                SELECT station_id AS StationId, title_id AS TitleId, bonus_mode AS BonusMode, bonus_limit AS BonusLimit 
                FROM sbl_bonus_data 
                WHERE dlt_date IS NULL AND bonus_mode = '累加法'";

            var bonusDataList = conn.Query<BonusData>(dataSql, null, trans).ToList();

            // 2. 取得各主檔對應的子項目 (對應原本 Java 的 bonusDatas[i].generateItems(conn))
            string itemSql = "SELECT * FROM sbl_bonus_item WHERE station_id = @StationId AND title_id = @TitleId";

            foreach (var data in bonusDataList)
            {
                data.BonusItems = conn.Query<BonusItem>(itemSql, new { StationId = data.StationId, TitleId = data.TitleId }, trans).ToList();
            }

            return bonusDataList;
        }

        /// <summary>
        /// 抓取必要執照資訊 (ReqNo:M200810044)
        /// 回傳陣列: [0] = 是否取得全部必要執照 ("" 或 "0" 代表是), [1] = 必要執照獎金上限
        /// </summary>
        private string[] GetNLBonusLimit(SqlConnection conn, SqlTransaction trans, string empId, string stationId, string title, string bonusMode)
        {
            // 這裡假設你有一個 Function 或是特定查詢可以取得此資訊
            // 由於原本的 Java 沒有提供這段 SQL，這邊用模擬的寫法
            string sql = @"
                SELECT IsAllRequiredObtained, RequiredLicenseLimit 
                FROM sbl_nl_limit 
                WHERE emp_id = @EmpId AND station_id = @StationId AND title_id = @TitleId";

            var result = conn.QueryFirstOrDefault(sql, new { EmpId = empId, StationId = stationId, TitleId = title }, trans);

            string[] nlBonusLimit = new string[2];
            if (result != null)
            {
                nlBonusLimit[0] = result.IsAllRequiredObtained?.ToString() ?? "";
                nlBonusLimit[1] = result.RequiredLicenseLimit?.ToString() ?? "0";
            }
            else
            {
                nlBonusLimit[0] = "0"; // 預設當作取得全部
                nlBonusLimit[1] = "0";
            }

            return nlBonusLimit;
        }

        /// <summary>
        /// 計算單一子項目的獎金
        /// </summary>
        private BonusItem DoCalculateBonus(SqlConnection conn, SqlTransaction trans, Dictionary<string, string> licenses, BonusItem item)
        {
            // 原本的 Java 程式碼這段是被封裝在 CalculateMode1 裡面
            // 這裡實作基本的計算邏輯範例：比對員工擁有的證照與該獎金項目的條件
            
            BonusItem calculatedItem = new BonusItem
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                AGrade = 0,
                BGrade = 0
            };

            // 假設邏輯：如果員工擁有該項目的證照，則給予對應的獎金
            if (licenses != null && licenses.ContainsKey(item.RequiredLicenseId))
            {
                // 具體計算邏輯依您的業務需求而定
                calculatedItem.AGrade = item.BaseAGrade; 
                calculatedItem.BGrade = item.BaseBGrade;
            }

            return calculatedItem;
        }
    }
}

namespace YourProject.Services
{
    public class Emp
    {
        public string EmpId { get; set; }
        public string StationId { get; set; }
        public string Title { get; set; }
        
        // 用 Dictionary 取代 HashMap 來存放證照
        public Dictionary<string, string> Licenses { get; set; } = new Dictionary<string, string>();
    }

    public class BonusData
    {
        public string StationId { get; set; }
        public string TitleId { get; set; }
        public string BonusMode { get; set; }
        public int BonusLimit { get; set; }
        
        public List<BonusItem> BonusItems { get; set; } = new List<BonusItem>();
    }

    public class BonusItem
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string RequiredLicenseId { get; set; }
        
        // 基礎設定獎金
        public int BaseAGrade { get; set; }
        public int BaseBGrade { get; set; }

        // 實際計算後的結果
        public int AGrade { get; set; }
        public int BGrade { get; set; }
    }

    public class BonusHistory
    {
        public string EmpId { get; set; }
        public string DateMonth { get; set; }
        public int TotalBonus { get; set; }
        public List<BonusItem> Items { get; set; } = new List<BonusItem>();

        /// <summary>
        /// 儲存計算結果到資料庫
        /// </summary>
        public void SaveToDB(SqlConnection conn, SqlTransaction trans)
        {
            string insertHistorySql = @"
                INSERT INTO sbl_bonus_history (emp_id, date_month, total_bonus) 
                VALUES (@EmpId, @DateMonth, @TotalBonus);
                SELECT SCOPE_IDENTITY();"; // 取得新增的 ID

            // 寫入主檔
            var historyId = conn.ExecuteScalar<int>(insertHistorySql, this, trans);

            // 寫入明細檔 (如果有需要記錄各個 Item 的金額)
            string insertItemSql = @"
                INSERT INTO sbl_bonus_history_item (history_id, item_id, a_grade, b_grade) 
                VALUES (@HistoryId, @ItemId, @AGrade, @BGrade)";

            foreach (var item in Items)
            {
                conn.Execute(insertItemSql, new 
                { 
                    HistoryId = historyId, 
                    ItemId = item.ItemId, 
                    AGrade = item.AGrade, 
                    BGrade = item.BGrade 
                }, trans);
            }
        }
    }
}
