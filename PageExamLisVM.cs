using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace YourNamespace.ViewModels.Exam
{
    public class PageExamListVM
    {
        // 基本資訊
        public string WorkerId { get; set; }
        public string ExamId { get; set; }
        public string StationId { get; set; }
        public string ExamRegNo { get; set; }

        // 測驗用時
        public int SpendTime { get; set; }
        public int AllTime { get; set; }

        // 各類型題目的答案
        public List<string> UserNecessaryAnswers { get; set; } = new List<string>(); // 必考題
        public List<string> UserTrueFalseAnswers { get; set; } = new List<string>(); // 是非題
        public List<string> UserMultipleChoiceAnswers { get; set; } = new List<string>(); // 選擇題
        public List<string> UserLinkAnswers { get; set; } = new List<string>(); // 連連看
        public List<string> ImageFiles { get; set; } = new List<string>(); // 圖片檔

        // 後端用資料 (由 Session 傳入)
        public List<string> NecessaryQuestions { get; set; } = new List<string>();
        public List<string> NecessaryAnswers { get; set; } = new List<string>();
        public List<string> TrueFalseQuestions { get; set; } = new List<string>();
        public List<string> TrueFalseAnswers { get; set; } = new List<string>();
        public List<string> MultipleChoiceQuestions { get; set; } = new List<string>();
        public List<string> MultipleChoiceAnswers { get; set; } = new List<string>();

        public List<string> LinkQuestions { get; set; } = new List<string>();
        public List<string> LinkAnswers { get; set; } = new List<string>();

        // 連連看圖片
        public List<string> ExamImages { get; set; } = new List<string>();

        // 題組資訊 (例如每一題目大題)
        public List<ExamInfoGroup> ExamGroups { get; set; } = new List<ExamInfoGroup>();

        // 成績
        public int TotalScore { get; set; }
        public int UsedMinutes { get; set; }
        public int UsedSeconds { get; set; }

        // 成績分析輸出訊息
        public List<string> FeedbackMessages { get; set; } = new List<string>();
    }

    public class ExamInfoGroup
    {
        public string Title { get; set; }
        public List<string> Subjects { get; set; } = new List<string>();
        public List<string> Answers { get; set; } = new List<string>();
        public List<string> UserAnswers { get; set; } = new List<string>();
        public List<string> ImageFiles { get; set; } = new List<string>();
    }
}
