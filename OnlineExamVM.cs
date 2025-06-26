namespace MesTAManagementSystem_New.ViewModels.Training.Testing
{
    public class OnlineExamVM
    {
        // 查詢條件
        public string StationId { get; set; }
        public string CerItemId { get; set; }
        public string EmpId { get; set; }
        public string Month { get; set; }
        
        // 顯示資訊
        public string Today { get; set; }           // 顯示今天的日期
        public IEnumerable<SelectListItem> StationOptions { get; set; }
        public IEnumerable<SelectListItem> CerItemOptions { get; set; }
        public IEnumerable<SelectListItem> EmpOptions { get; set; }

        // 考題內容
        public List<OperationQuestionVM> BasicQuestions { get; set; }
        public List<OperationQuestionVM> AdvQuestions { get; set; }

        // 使用者選擇的考題答案
        public Dictionary<int, string> SelectedAnswers { get; set; }

        // hidden fields
        public string HdnScore { get; set; }
        public string HdnPassStatus { get; set; }
        
        // Session/user info
        public string UserId { get; set; }
        public string UserShiftId { get; set; }
        public string UserDeptId { get; set; }
        
        // 送出後的狀態
        public bool IsSubmitted { get; set; }
    }

    public class OperationQuestionVM
    {
        public string Type { get; set; }    // "BASIC" 或 "ADV"
        public string No { get; set; }
        public string Subject { get; set; }
        public int Weight { get; set; }
        public int QuestionIndex { get; set; }
    }
}