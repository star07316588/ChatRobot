namespace YourProject.ViewModels.OnlineExam
{
    public class PageExamListVM
    {
        // 基本資訊
        public string EmpId { get; set; }
        public string CerItemId { get; set; }
        public string StationId { get; set; }
        public string ExamRegNo { get; set; }

        // 題目資料（用來顯示題號與正確答案）
        public List<QuestionVM> NecessaryQuestions { get; set; } = new List<QuestionVM>();  // 必考題
        public List<QuestionVM> TrueFalseQuestions { get; set; } = new List<QuestionVM>();  // 是非題
        public List<QuestionVM> ChoiceQuestions { get; set; } = new List<QuestionVM>();     // 選擇題

        // 使用者作答
        public List<string> NecessaryAnswers { get; set; } = new List<string>();
        public List<string> TFAnswers { get; set; } = new List<string>();
        public List<string> ChooseAnswers { get; set; } = new List<string>();

        // 成績資訊
        public int TotalQuestions => NecessaryQuestions.Count + TrueFalseQuestions.Count + ChoiceQuestions.Count;
        public int CorrectCount { get; set; }
        public int Score { get; set; }

        // 顯示正確與否的比對
        public List<QuestionResultVM> NecessaryResults { get; set; } = new List<QuestionResultVM>();
        public List<QuestionResultVM> TFResults { get; set; } = new List<QuestionResultVM>();
        public List<QuestionResultVM> ChoiceResults { get; set; } = new List<QuestionResultVM>();
    }

    public class QuestionVM
    {
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
    }

    public class QuestionResultVM
    {
        public string Question { get; set; }
        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect => 
            !string.IsNullOrWhiteSpace(UserAnswer) &&
            UserAnswer.Trim().Equals(CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
