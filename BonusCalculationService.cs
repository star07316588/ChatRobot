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

/// <summary>
/// 取得員工基本資料與其擁有的有效證照
/// </summary>
private Emp GetEmployeeWithLicenses(SqlConnection conn, SqlTransaction trans, string empId)
{
    // 1. 查詢員工基本資料
    string empSql = @"
        SELECT emp_id AS EmpId, station_id AS StationId, title AS Title 
        FROM sbl_emp 
        WHERE dlt_date IS NULL AND emp_id = @EmpId";
    
    var employee = conn.QueryFirstOrDefault<Emp>(empSql, new { EmpId = empId }, trans);

    if (employee != null)
    {
        // 2. 查詢該員工的有效證照資料 (完整對應原 Java 的 getLicenceByEmpId)
        // 注意：這裡將 Oracle 的 sysdate 替換為 SQL Server 的 GETDATE()
        string licSql = @"
            SELECT 
                cer_item_id AS CerItemId, 
                emp_id AS EmpId, 
                station_id AS StationId, 
                licence_type AS LicenceType
                -- TODO: 如果有其他需要用到的證照欄位，可以在這裡補上並加上 AS 別名
            FROM sbl_licence 
            WHERE valid_date >= GETDATE() 
              AND dlt_date IS NULL 
              AND licence_type = 'CL' 
              AND emp_id = @EmpId 
              AND station_id = @StationId";
        
        // Dapper 會將結果轉為 Licence 物件的集合
        var licenceList = conn.Query<Licence>(licSql, new { EmpId = empId, StationId = employee.StationId }, trans);

        // 轉為 Dictionary，Key = CerItemId, Value = Licence 物件本身 (精確重現原 map.put 邏輯)
        employee.Licenses = licenceList.ToDictionary(lic => lic.CerItemId, lic => lic);
    }

    return employee;
}

/// <summary>
/// 計算單一子項目的獎金 (簽名檔也要跟著更新，Dictionary 的 Value 變成 Licence 物件)
/// </summary>
private BonusItem DoCalculateBonus(SqlConnection conn, SqlTransaction trans, Dictionary<string, Licence> licenses, BonusItem item)
{
    BonusItem calculatedItem = new BonusItem
    {
        ItemId = item.ItemId,
        ItemName = item.ItemName,
        AGrade = 0,
        BGrade = 0
    };

    // 判斷該員工是否擁有對應的 cer_item_id 證照
    if (licenses != null && licenses.ContainsKey(item.RequiredLicenseId))
    {
        // 你甚至可以從 value 中取用該證照的詳細資訊，例如：
        // Licence myLicence = licenses[item.RequiredLicenseId];
        
        calculatedItem.AGrade = item.BaseAGrade; 
        calculatedItem.BGrade = item.BaseBGrade;
    }

    return calculatedItem;
}

namespace YourProject.Services
{
    public class Emp
    {
        public string EmpId { get; set; }
        public string StationId { get; set; }
        public string Title { get; set; }
        
        // 將原本的 Dictionary<string, string> 改為 Dictionary<string, Licence>
        public Dictionary<string, Licence> Licenses { get; set; } = new Dictionary<string, Licence>();
    }

    // 新增：完整對應 sbl_Licence 資料表的實體類別
    public class Licence
    {
        public string CerItemId { get; set; } // 證照項目 ID (作為 Dictionary 的 Key)
        public string EmpId { get; set; }
        public string StationId { get; set; }
        public string LicenceType { get; set; }
        // 依照需求可再擴充 ValidDate 等欄位
    }
    
    // ... (保留原本的 BonusData, BonusItem, BonusHistory)
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace YourProject.Services
{
    public partial class BonusCalculationService
    {
        private readonly string _connectionString;
        
        // 新增：為了 Jack 在 2023/08/03 加上的稽核需求，注入執行環境資訊
        private readonly string _sUserID;
        private readonly string _sPlatform;
        private readonly string _sExecFunction;

        public BonusCalculationService(string connectionString, string sUserID, string sPlatform, string sExecFunction)
        {
            _connectionString = connectionString;
            _sUserID = sUserID;
            _sPlatform = sPlatform;
            _sExecFunction = sExecFunction;
        }

        // ... (保留先前的 CalculateSingleEmployeeBonus 與 GetEmployeeWithLicenses) ...

        /// <summary>
        /// 取得「累加法」的獎金設定資料與其子項目 (更新版)
        /// </summary>
        private List<BonusData> GetAccumulationBonusData(SqlConnection conn, SqlTransaction trans)
        {
            // 1. 取得主檔 (需確保 Select 包含 id / data_id)
            string dataSql = @"
                SELECT 
                    id AS Id, 
                    station_id AS StationId, 
                    title_id AS TitleId, 
                    bonus_mode AS BonusMode, 
                    bonus_limit AS BonusLimit 
                FROM sbl_bonus_data 
                WHERE dlt_date IS NULL AND bonus_mode = '累加法'";

            var bonusDataList = conn.Query<BonusData>(dataSql, null, trans).ToList();

            // 2. 取得各主檔對應的子項目 (對應原本 Java 的 generateItems)
            string itemSql = "SELECT * FROM sbl_bonus_item WHERE data_id = @DataId";

            foreach (var data in bonusDataList)
            {
                // ----- <2> 實作 Jack 的 Rbl_Confidential_SysLog 紀錄 -----
                if (!string.IsNullOrEmpty(_sUserID) && !string.IsNullOrEmpty(_sExecFunction) && !string.IsNullOrEmpty(_sPlatform))
                {
                    // 在 Dapper 中，我們記錄帶有參數的 SQL 語法，這比 Java 原本的字串拼接更安全
                    InsertConfidentialSysLog(conn, trans, _sPlatform, _sUserID, _sExecFunction, itemSql + $" (DataId={data.Id})");
                }

                // 取得 Items
                var items = conn.Query<BonusItem>(itemSql, new { DataId = data.Id }, trans).ToList();

                foreach (var item in items)
                {
                    // 寫入環境變數
                    item.SUserID = _sUserID;
                    item.SPlatform = _sPlatform;
                    item.SExecFunction = _sExecFunction;

                    // 取得主/副證照設定 (需額外實作的 TODO)
                    item.MainLicences = GenerateMainLicence(conn, trans, item.Id);
                    item.SubLicences = GenerateSubLicence(conn, trans, item.Id);
                }

                data.BonusItems = items;
            }

            return bonusDataList;
        }

        /// <summary>
        /// 寫入機密系統日誌
        /// </summary>
        private void InsertConfidentialSysLog(SqlConnection conn, SqlTransaction trans, string platform, string userId, string execFunction, string sqlString)
        {
            string logSql = @"
                INSERT INTO Rbl_Confidential_SysLog (Platform, UserID, ExecFunction, SqlContent, LogTime) 
                VALUES (@Platform, @UserID, @ExecFunction, @SqlContent, GETDATE())";

            conn.Execute(logSql, new 
            { 
                Platform = platform, 
                UserID = userId, 
                ExecFunction = execFunction, 
                SqlContent = sqlString 
            }, trans);
        }

        /// <summary>
        /// 取得該獎金項目的主證照要求
        /// </summary>
        private List<LicenceRequirement> GenerateMainLicence(SqlConnection conn, SqlTransaction trans, string bonusItemId)
        {
            // TODO: 實作 "SELECT * FROM sbl_bonus_main_licence WHERE item_id = @ItemId"
            return new List<LicenceRequirement>();
        }

        /// <summary>
        /// 取得該獎金項目的副證照要求
        /// </summary>
        private List<LicenceRequirement> GenerateSubLicence(SqlConnection conn, SqlTransaction trans, string bonusItemId)
        {
            // TODO: 實作 "SELECT * FROM sbl_bonus_sub_licence WHERE item_id = @ItemId"
            return new List<LicenceRequirement>();
        }
    }
}

namespace YourProject.Services
{
    public class BonusData
    {
        public string Id { get; set; } // 新增：對應 sbl_bonus_data 的 PK (data_id)
        public string StationId { get; set; }
        public string TitleId { get; set; }
        public string BonusMode { get; set; }
        public int BonusLimit { get; set; }
        
        public List<BonusItem> BonusItems { get; set; } = new List<BonusItem>();
    }

    public class BonusItem
    {
        public string Id { get; set; } // 新增：BonusItem 本身的 PK
        public string DataId { get; set; } // 新增：對應 BonusData 的 FK
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        
        // 基礎設定獎金
        public int BaseAGrade { get; set; }
        public int BaseBGrade { get; set; }

        // 實際計算後的結果
        public int AGrade { get; set; }
        public int BGrade { get; set; }

        // 稽核用屬性 (由 Jack 新增)
        public string SUserID { get; set; }
        public string SPlatform { get; set; }
        public string SExecFunction { get; set; }

        // 存放主副證照的集合
        public List<LicenceRequirement> MainLicences { get; set; } = new List<LicenceRequirement>();
        public List<LicenceRequirement> SubLicences { get; set; } = new List<LicenceRequirement>();
    }

    // 新增：用來存放主/副證照設定的類別
    public class LicenceRequirement
    {
        public string ItemId { get; set; }
        public string LicenceId { get; set; }
        // 依照資料表實際欄位擴充...
    }
}

namespace YourProject.Services
{
    public class BonusItem
    {
        public string Id { get; set; } 
        public string DataId { get; set; } 
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        
        public int BaseAGrade { get; set; }
        public int BaseBGrade { get; set; }

        public int AGrade { get; set; }
        public int BGrade { get; set; }

        // 稽核用屬性
        public string SUserID { get; set; }
        public string SPlatform { get; set; }
        public string SExecFunction { get; set; }

        // 修改：因為撈出來的只有 cer_item_id，直接用 List<string> 儲存即可
        public List<string> MainLicences { get; set; } = new List<string>();
        public List<string> SubLicences { get; set; } = new List<string>();
    }
}

        /// <summary>
        /// 取得該獎金項目的證照要求 (整合主/副證照查詢邏輯)
        /// </summary>
        /// <param name="tableName">資料表名稱 (必須為 sbl_bonus_mlicence 或 sbl_bonus_slicence)</param>
        private List<string> GetLicenceItemIds(SqlConnection conn, SqlTransaction trans, string tableName, string dataId, string itemId)
        {
            // 1. 白名單防護：因為資料表名稱無法參數化，必須嚴格限制傳入的值以防止 SQL Injection
            if (tableName != "sbl_bonus_mlicence" && tableName != "sbl_bonus_slicence")
            {
                throw new ArgumentException("無效的資料表名稱");
            }

            // 2. 組裝 SQL (將 Table Name 寫入，其餘條件參數化)
            string sql = $"SELECT cer_item_id FROM {tableName} WHERE data_id = @DataId AND item_id = @ItemId";

            // 3. 實作 Jack 的 Rbl_Confidential_SysLog 紀錄
            if (!string.IsNullOrEmpty(_sUserID) && !string.IsNullOrEmpty(_sExecFunction) && !string.IsNullOrEmpty(_sPlatform))
            {
                // 為了稽核紀錄的完整性，我們將實際的值拼湊進 Log 字串中 (這僅用於 Log，不會拿去執行)
                string logSqlContent = $"Select cer_item_id from {tableName} where data_id = '{dataId}' and item_id = '{itemId}'";
                InsertConfidentialSysLog(conn, trans, _sPlatform, _sUserID, _sExecFunction, logSqlContent);
            }

            // 4. 執行查詢
            // Dapper 的 Query<string> 會自動把單一欄位 (cer_item_id) 的結果轉成 List<string>
            return conn.Query<string>(sql, new { DataId = dataId, ItemId = itemId }, trans).ToList();
        }

        /// <summary>
        /// 取得「累加法」的獎金設定資料與其子項目 (更新版)
        /// </summary>
        private List<BonusData> GetAccumulationBonusData(SqlConnection conn, SqlTransaction trans)
        {
            // ... (前面的 dataSql 查詢邏輯不變) ...

            string itemSql = "SELECT * FROM sbl_bonus_item WHERE data_id = @DataId";

            foreach (var data in bonusDataList)
            {
                // 紀錄 BonusItem 查詢的稽核 Log
                if (!string.IsNullOrEmpty(_sUserID) && !string.IsNullOrEmpty(_sExecFunction) && !string.IsNullOrEmpty(_sPlatform))
                {
                    InsertConfidentialSysLog(conn, trans, _sPlatform, _sUserID, _sExecFunction, itemSql + $" (DataId={data.Id})");
                }

                // 取得 Items
                var items = conn.Query<BonusItem>(itemSql, new { DataId = data.Id }, trans).ToList();

                foreach (var item in items)
                {
                    item.SUserID = _sUserID;
                    item.SPlatform = _sPlatform;
                    item.SExecFunction = _sExecFunction;

                    // 這裡呼叫我們剛剛寫好的整合方法！
                    item.MainLicences = GetLicenceItemIds(conn, trans, "sbl_bonus_mlicence", data.Id, item.Id);
                    item.SubLicences  = GetLicenceItemIds(conn, trans, "sbl_bonus_slicence", data.Id, item.Id);
                }

                data.BonusItems = items;
            }

            return bonusDataList;
        }

/// <summary>
/// 計算單一子項目的獎金 (根據員工擁有的證照與項目的主/副證照要求進行比對)
/// </summary>
private BonusItem DoCalculateBonus(SqlConnection conn, SqlTransaction trans, Dictionary<string, Licence> employeeLicenses, BonusItem item)
{
    // 預設該項目計算結果為 0
    BonusItem calculatedItem = new BonusItem
    {
        ItemId = item.ItemId,
        ItemName = item.ItemName,
        AGrade = 0,
        BGrade = 0
    };

    // 如果員工沒有任何證照，直接回傳 0 元
    if (employeeLicenses == null || !employeeLicenses.Any())
    {
        return calculatedItem;
    }

    // ---------------------------------------------------------
    // 1. 判斷主證照 (A Grade)
    // 邏輯：檢查員工的證照清單 (Keys) 中，是否包含該獎金項目的「任何一張」主證照
    // ---------------------------------------------------------
    if (item.MainLicences != null && item.MainLicences.Any())
    {
        // .Any() 會走訪 MainLicences，只要有任何一個 mainLic 存在於 employeeLicenses 字典中，就回傳 true
        bool hasMainLicence = item.MainLicences.Any(mainLic => employeeLicenses.ContainsKey(mainLic));
        
        if (hasMainLicence)
        {
            calculatedItem.AGrade = item.BaseAGrade; // 給予主證照對應的 A 級獎金
        }
    }

    // ---------------------------------------------------------
    // 2. 判斷副證照 (B Grade)
    // 邏輯：檢查員工的證照清單 (Keys) 中，是否包含該獎金項目的「任何一張」副證照
    // ---------------------------------------------------------
    if (item.SubLicences != null && item.SubLicences.Any())
    {
        bool hasSubLicence = item.SubLicences.Any(subLic => employeeLicenses.ContainsKey(subLic));
        
        if (hasSubLicence)
        {
            calculatedItem.BGrade = item.BaseBGrade; // 給予副證照對應的 B 級獎金
        }
    }

    return calculatedItem;
}

        /// <summary>
        /// 抓取必要執照資訊 (ReqNo:M200810044)
        /// 邏輯：查詢該員工「缺少」的必要證照數量，以及必要證照未齊全時的獎金上限
        /// </summary>
        /// <returns>回傳陣列: [0] = 缺少的必要證照數量 ("0" 代表已全部取得), [1] = 必要執照獎金上限</returns>
        private string[] GetNLBonusLimit(SqlConnection conn, SqlTransaction trans, string empId, string stationId, string titleId, string bonusMode)
        {
            // 預設回傳值：缺少 0 張 (已全部取得)，上限 0
            string[] result = new string[] { "0", "0" };

            // 1. 取得 DataId 與必要執照獎金上限 (nl_bonus_limit)
            string dataSql = @"
                SELECT id AS DataId, nl_bonus_limit AS NlBonusLimit 
                FROM sbl_bonus_data 
                WHERE station_id = @StationId 
                  AND title_id = @TitleId 
                  AND bonus_mode = @BonusMode";

            // Dapper 允許我們動態對應匿名類別，這裡用 dynamic 接取單筆結果
            var dataInfo = conn.QueryFirstOrDefault(dataSql, new { StationId = stationId, TitleId = titleId, BonusMode = bonusMode }, trans);

            if (dataInfo == null)
            {
                return result; // 找不到設定，直接回傳預設值
            }

            string dataId = dataInfo.DataId.ToString();
            result[1] = dataInfo.NlBonusLimit?.ToString() ?? "0";

            // 2. 計算「缺少」的必要證照數量 (NLCount)
            // 將原本 Oracle 的 (+) Outer Join 改寫為標準的 ANSI SQL (NOT EXISTS)
            // 語意：從「必要證照需求表(a)」中，挑出「員工有效證照表(b)」裡沒有的項目，然後計算數量
            string countSql = @"
                SELECT COUNT(*) 
                FROM sbl_bonus_nlicence a
                WHERE a.data_id = @DataId
                  AND NOT EXISTS (
                      SELECT 1 
                      FROM sbl_licence b 
                      WHERE b.cer_item_id = a.cer_item_id
                        AND b.emp_id = @EmpId 
                        AND b.station_id = @StationId 
                        AND b.licence_type = 'CL' 
                        AND b.valid_date > GETDATE() /* 如果實際底層還是 Oracle，請將 GETDATE() 改回 SYSDATE */
                        AND b.dlt_user IS NULL 
                        AND b.dlt_date IS NULL
                  )";

            // ExecuteScalar 專門用來取得單一值 (例如 COUNT 的結果)
            int missingCount = conn.ExecuteScalar<int>(countSql, new { DataId = dataId, EmpId = empId, StationId = stationId }, trans);
            
            result[0] = missingCount.ToString();

            return result;
        }


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace YourProject.Services
{
    public class BonusHistory
    {
        public string Id { get; set; } // 對應 Java 的 this.id
        public string EmpId { get; set; }
        public string DateMonth { get; set; }
        public int TotalBonus { get; set; }
        
        public List<BonusItem> Items { get; set; } = new List<BonusItem>();

        /// <summary>
        /// 儲存主檔與明細到資料庫 (完全依賴外層傳入的 SqlTransaction，內部不呼叫 Commit/Rollback)
        /// </summary>
        public void SaveToDB(SqlConnection conn, SqlTransaction trans)
        {
            // 1. 判斷資料是否存在 (對應原 selectCount 與 QueryBonusHistoryId)
            string checkSql = "SELECT id FROM sbl_bonus_history WHERE emp_id = @EmpId AND date_month = @DateMonth";
            var existingId = conn.QueryFirstOrDefault<string>(checkSql, new { EmpId = this.EmpId, DateMonth = this.DateMonth }, trans);

            if (!string.IsNullOrEmpty(existingId))
            {
                // 資料存在，走 Update 流程
                this.Id = existingId;
                Update(conn, trans);
            }
            else
            {
                // 資料不存在，走 Insert 流程
                Insert(conn, trans);
            }

            // 儲存子項目明細 (對應 items[i].SaveToHistoryDB)
            SaveItems(conn, trans);
        }

        private void Insert(SqlConnection conn, SqlTransaction trans)
        {
            // 取得 Sequence 序號 (對應原 BonusService.getSEQNumberString)
            // 注意：如果你們在 SQL Server 是用自動遞增欄位(IDENTITY)，這行就不需要，並在 INSERT 後用 SCOPE_IDENTITY() 取回 ID。
            this.Id = GetSequenceNumber(conn, trans, "sbl_bonus_history_seq");

            string sql = @"
                INSERT INTO sbl_bonus_history (id, emp_id, date_month, totalbonus, crt_date) 
                VALUES (@Id, @EmpId, @DateMonth, @TotalBonus, GETDATE())"; // C# 中用 GETDATE() 取代原本的 java.sql.Timestamp

            conn.Execute(sql, new 
            { 
                Id = this.Id, 
                EmpId = this.EmpId, 
                DateMonth = this.DateMonth, 
                TotalBonus = this.TotalBonus 
            }, trans);
        }

        private void Update(SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
                UPDATE sbl_bonus_history 
                SET totalbonus = @TotalBonus, 
                    upt_date = GETDATE() 
                WHERE id = @Id";

            conn.Execute(sql, new 
            { 
                TotalBonus = this.TotalBonus, 
                Id = this.Id 
            }, trans);
        }

        /// <summary>
        /// 儲存子項目明細 (取代原本散落在 BonusItem 裡的 SaveToHistoryDB)
        /// </summary>
        private void SaveItems(SqlConnection conn, SqlTransaction trans)
        {
            if (Items == null || !Items.Any()) return;

            // 針對明細，最安全且具冪等性 (Idempotent) 的做法是先檢查再決定 Insert/Update
            foreach (var item in Items)
            {
                string checkItemSql = "SELECT COUNT(1) FROM sbl_bonus_history_item WHERE history_id = @HistoryId AND item_id = @ItemId";
                int count = conn.ExecuteScalar<int>(checkItemSql, new { HistoryId = this.Id, ItemId = item.ItemId }, trans);

                if (count > 0)
                {
                    string updateSql = @"
                        UPDATE sbl_bonus_history_item 
                        SET a_grade = @AGrade, b_grade = @BGrade 
                        WHERE history_id = @HistoryId AND item_id = @ItemId";
                    
                    conn.Execute(updateSql, new { HistoryId = this.Id, ItemId = item.ItemId, AGrade = item.AGrade, BGrade = item.BGrade }, trans);
                }
                else
                {
                    string insertSql = @"
                        INSERT INTO sbl_bonus_history_item (history_id, item_id, a_grade, b_grade) 
                        VALUES (@HistoryId, @ItemId, @AGrade, @BGrade)";
                    
                    conn.Execute(insertSql, new { HistoryId = this.Id, ItemId = item.ItemId, AGrade = item.AGrade, BGrade = item.BGrade }, trans);
                }
            }
        }

        /// <summary>
        /// 模擬取得 Sequence 的方法 (請依據你們實際在 SQL Server 產生序號的方式調整)
        /// </summary>
        private string GetSequenceNumber(SqlConnection conn, SqlTransaction trans, string seqName)
        {
            // 這裡實作你們產生 ID 的邏輯。
            // 假設你們有一個自訂的儲存程序或 Function 來取號：
            // return conn.QueryFirst<string>("SELECT dbo.fn_GetNextSeq(@SeqName)", new { SeqName = seqName }, trans);
            
            return Guid.NewGuid().ToString("N"); // 暫用 Guid 模擬唯一碼
        }
    }
}
