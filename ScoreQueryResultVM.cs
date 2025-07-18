using System;

namespace MesTAManagementSystem_New.ViewModels.Training.Testing
{
    public class ScoreQueryResultVM
    {
        public string StationId { get; set; }              // 站別區域
        public string CerItemId { get; set; }              // 試題代碼
        public DateTime? WRDate { get; set; }              // 筆試日期
        public DateTime? OPDate { get; set; }              // 術科日期
        public string ShiftId { get; set; }                // 班別
        public string EmpId { get; set; }                  // 工號
        public string Name { get; set; }                   // 姓名
        public string ScoreWR { get; set; }                // 學科分數
        public string ScoreOP { get; set; }                // 術科分數
        public string WRFile { get; set; }                 // 學科成績檔案
        public string OPFile { get; set; }                 // 術科成績檔案
    }
}
