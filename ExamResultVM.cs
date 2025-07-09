public class ExamResultVM
{
    public string EmpId { get; set; }
    public string CerItemId { get; set; }
    public string StationId { get; set; }

    public int TotalScore { get; set; }
    public int NecessaryScore { get; set; }
    public int TrueFalseScore { get; set; }
    public int ChoiceScore { get; set; }
    public int LinkScore { get; set; }

    public List<AnsweredQuestion> NecessaryQuestions { get; set; } = new();
    public List<AnsweredQuestion> TrueFalseQuestions { get; set; } = new();
    public List<AnsweredQuestion> ChoiceQuestions { get; set; } = new();
    public List<AnsweredQuestion> LinkQuestions { get; set; } = new();
}

public class AnsweredQuestion
{
    public string Subject { get; set; }
    public string CorrectAnswer { get; set; }
    public string UserAnswer { get; set; }
    public bool IsCorrect => string.Equals(CorrectAnswer?.Trim(), UserAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
}