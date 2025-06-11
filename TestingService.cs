using System.Collections.Generic;
using MesTAManagementSystem_New.Repositories;
using MesTAManagementSystem_New.ViewModels.Training.Testing;
using NPOI.SS.UserModel;

namespace MesTAManagementSystem_New.Services
{
    public class TestingService
    {
        private readonly TestingRepository _repo;

        public TestingService()
        {
            _repo = new TestingRepository();
        }

        public IEnumerable<string> GetStationIdList()
        {
            return _repo.GetStationIdList();
        }

        public IEnumerable<string> GetCerItemIdList(string stationId)
        {
            return _repo.GetCerItemIdList(stationId);
        }

        public IEnumerable<string> GetTestSpecList(string tableName, string certItemId)
        {
            return _repo.GetTestSpecList(tableName, certItemId);
        }

        public List<QuestionVM> GetTestSpec(string tableName, string certItemId)
        {
            return _repo.GetTestSpec(tableName, certItemId);
        }

        public string SaveToQuestionSpec(string filenames, string tableName, string certItemId, ISheet sheet)
        {
            return _repo.SaveToQuestionSpec(filenames, tableName, certItemId, sheet);
        }

        public string UpdateQuestion(string tableName, QuestionVM vm)
        {
            return _repo.UpdateQuestion(tableName, vm);
        }

public class CertInfo
{
    public string CerId { get; set; }
    public string CerDate { get; set; }
}

public class CertRegisterVM
{
    public string CerId { get; set; }
    public string EmpId { get; set; }
    public string CerDate { get; set; }
    public string CertItemId { get; set; }
    public string ScoreWR { get; set; }
    public string ScoreOP { get; set; }
    public string CreateUser { get; set; }
}

public class TestingService
{
    private readonly TestingRepository _repo = new TestingRepository();

    public CertInfo GetCertificateByCerItemId(string cerItemId, string role, string month)
    {
        return _repo.FetchValidCertificate(cerItemId, role, month);
    }

    public bool IsDuplicateRegistration(string cerId, string cerDate, string empId, string cerItemId)
    {
        return _repo.CheckDuplicate(cerId, cerDate, empId, cerItemId);
    }

    public bool HasTrainingRecordInThreeMonths(string empId, string cerItemId)
    {
        return _repo.HasRecentTraining(empId, cerItemId);
    }

    public string GetExamType(string certItemId)
    {
        return _repo.GetExamType(certItemId);
    }

    public void InsertRegistration(CertRegisterVM vm)
    {
        _repo.InsertRegistration(vm);
    }
}
    }

        public TestingService()
        {
            _repo = new TestingRepository();
        }

        public List<ScoreQueryResultVM> GetScoreQueryResult(
            string stationId,
            string cerItemId,
            string startYear,
            string startMonth,
            string endYear,
            string endMonth,
            string shiftId,
            string empId,
            string logUser)
        {
            return _repo.FetchScoreQueryResult(
                stationId, cerItemId, startYear, startMonth, endYear, endMonth, shiftId, empId, logUser);
        }
    }
}