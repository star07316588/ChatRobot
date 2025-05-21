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
    }
}