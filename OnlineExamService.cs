// Services/OnlineExamService.cs using System; using System.Collections.Generic; using MesTAManagementSystem_New.Models.Training.Exam; using MesTAManagementSystem_New.Repositories;

namespace MesTAManagementSystem_New.Services { public class OnlineExamService { private readonly OnlineExamRepository _repository;

public OnlineExamService()
    {
        _repository = new OnlineExamRepository();
    }

    public List<string> GetCerItemIds(string empId)
    {
        return _repository.FetchCerItemIds(empId);
    }

    public List<string> GetStationIds(string cerItemId)
    {
        return _repository.FetchStationIds(cerItemId);
    }

    public ExamQuestionVM GetExamQuestions(string empId, string cerItemId, string stationId)
    {
        return _repository.LoadExamQuestions(empId, cerItemId, stationId);
    }

    public void SaveExamResult(ExamSubmissionVM submission)
    {
        _repository.SaveExamResult(submission);
    }
}

}

