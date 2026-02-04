using System;
using System.Collections.Generic;

namespace MyPerformanceApp.Models
{
    // 登入者與基本環境資訊
    public class UserProfile
    {
        public string EmpId { get; set; }
        public string DeptId { get; set; }
        public string ShiftId { get; set; }
        public string Title { get; set; } // HeadTitle
        public string StationId { get; set; }
        public string Section { get; set; }
        public bool IsProdSupervisor { get; set; }
    }

    // 查詢條件 (對應 JSP 上方的選單)
    public class SubjectiveQuery
    {
        public string Year { get; set; }
        public string Month { get; set; }
        public string DeptId { get; set; }
        public string Title { get; set; }
        public List<string> ShiftIds { get; set; } = new List<string>(); // 支援複選
        public List<string> StationIds { get; set; } = new List<string>(); // 支援複選
        public string Item { get; set; }
        public string DetailItem { get; set; }
    }

    // 評比項目設定 (對應 JSP 的 remark 區塊查詢結果)
    public class SubjectiveItemConfig
    {
        public decimal Weighting { get; set; }
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
        public string ItemRemark { get; set; }
        public string DetailItemRemark { get; set; }
    }

    // 主表格的一列資料 (整合了員工基本資料與評分資料)
    public class EmployeeGradeRow
    {
        // 員工基本資料 (唯讀)
        public string DeptId { get; set; }
        public string StationId { get; set; }
        public string ShiftId { get; set; }
        public string EmpId { get; set; }
        public string Name { get; set; }
        public string PositionGroup { get; set; }
        public string Title { get; set; }

        // 評分資料 (可編輯)
        public string RowId { get; set; } // 若為空代表尚未新增
        public int? Record { get; set; }  // 排名
        public decimal? Score { get; set; } // 分數
        public string Comments { get; set; }
    }
}
