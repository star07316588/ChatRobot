using System.Collections.Generic;
using MyPerformanceApp.Models;

namespace MyPerformanceApp.Services.Repository
{
    public interface ISubjectiveRepository
    {
        // 取得使用者資訊
        UserProfile GetUserProfile(string empId);
        
        // 取得下拉選單資料
        List<string> GetProductionDepts();
        List<string> GetShifts(string deptId, string section);
        List<string> GetStations(string deptId, string section);
        List<string> GetItems(string stationId, string title);
        List<string> GetDetailItems(string stationId, string title, string item);
        
        // 取得項目備註與設定
        SubjectiveItemConfig GetItemConfig(string stationId, string title, string item, string detailItem);

        // 檢查狀態 (能否查詢或關帳)
        string GetStatus(SubjectiveQuery query, string type);

        // 查詢主要評比名單 (取代 JSP 中 N+1 的迴圈查詢，改為 Join)
        List<EmployeeGradeRow> GetGradeList(SubjectiveQuery query, string section);

        // 存檔 (新增或更新)
        void SaveGrade(EmployeeGradeRow row, SubjectiveQuery query, string modifierId);

        // 寫入機密 Log (對應 jdbc.insertconfidential)
        void LogConfidential(string empId, string action, string sqlContent);

        // 結案更新狀態
        void UpdateStatusToClose(SubjectiveQuery query, string currentStatus, string modifierId);
        
        // 檢查是否所有項目都已輸入 (Utility.CloseCheck)
        bool CheckAllItemsFilled(SubjectiveQuery query);
    }
}
