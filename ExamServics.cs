using System.Collections.Generic;
using YourProject.Models;
using YourProject.Repositories;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;

    public ExamService(IExamRepository examRepository)
    {
        _examRepository = examRepository;
    }

    public List<ExamQuestion.QuestionResult> GetNecessaryQuestions(string cerItemId)
    {
        return _examRepository.GetQuestionsByType(cerItemId, "4");
    }

    public List<ExamQuestion.QuestionResult> GetTrueFalseQuestions(string cerItemId)
    {
        return _examRepository.GetQuestionsByType(cerItemId, "1");
    }

    public List<ExamQuestion.QuestionResult> GetChoiceQuestions(string cerItemId)
    {
        return _examRepository.GetQuestionsByType(cerItemId, "2");
    }

    public List<ExamQuestion.QuestionResult> GetLinkQuestions(string cerItemId)
    {
        return _examRepository.GetQuestionsByType(cerItemId, "3");
    }

// Service: ExamService.cs
public class ExamService
{
    private readonly ExamRepository _examRepository;

    public ExamService(ExamRepository examRepository)
    {
        _examRepository = examRepository;
    }

    public List<ExamQuestion.QuestionResult> GetRandomTrueFalseAndChoiceQuestions(string cerItemId, int questionCount)
    {
        var allQuestions = _examRepository.GetTrueFalseAndChoiceQuestions(cerItemId);
        return allQuestions.OrderBy(q => Guid.NewGuid()).Take(questionCount).ToList();
    }

    public List<ExamQuestion.QuestionResult> GetLinkQuestions(string cerItemId, int linkSum, int linkSubSum)
    {
        return _examRepository.GetLinkQuestions(cerItemId, linkSum, linkSubSum);
    }

    public int GetTotalExamTimeInSeconds(string cerItemId)
    {
        int minutes = _examRepository.GetExamTimeMinutes(cerItemId);
        return minutes > 0 ? minutes * 60 : 600;
    }
}

}