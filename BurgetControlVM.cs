namespace MesTAManagementSystem_New.ViewModels.Report
{
    public class BudgetDataVM
    {
        public string Year { get; set; }       // 年度
        public string DeptId { get; set; }     // 部門代碼
        public string ShiftId { get; set; }    // 班別
        public string Title { get; set; }      // 職稱
        public string Type { get; set; }       // 預算 or 實績

        public decimal? Jan { get; set; }
        public decimal? Feb { get; set; }
        public decimal? Mar { get; set; }
        public decimal? Apr { get; set; }
        public decimal? May { get; set; }
        public decimal? Jun { get; set; }
        public decimal? Jul { get; set; }
        public decimal? Aug { get; set; }
        public decimal? Sep { get; set; }
        public decimal? Oct { get; set; }
        public decimal? Nov { get; set; }
        public decimal? Dec { get; set; }
    }
}