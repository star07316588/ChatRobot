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
}