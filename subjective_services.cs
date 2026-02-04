using System.Linq;
using MyPerformanceApp.Models;
using MyPerformanceApp.Services.Repository;

namespace MyPerformanceApp.Services
{
    public class SubjectiveService : ISubjectiveService
    {
        private readonly ISubjectiveRepository _repo;

        public SubjectiveService(ISubjectiveRepository repo)
        {
            _repo = repo;
        }

        public SubjectiveViewModel LoadPage(string userId)
        {
            var vm = new SubjectiveViewModel();
            var user = _repo.GetUserProfile(userId);
            
            // 初始化下拉選單邏輯 (對應 JSP 一開始的 DB 查詢)
            // ...
            
            return vm;
        }

        public SubjectiveViewModel PerformQuery(SubjectiveViewModel vm, string userId)
        {
            var user = _repo.GetUserProfile(userId);
            
            // 1. 檢查 Status (對應 JSP Line 110-117)
            string status = _repo.GetStatus(vm.Query, "SUBJECT");
            
            // 權限判斷邏輯
            bool canEdit = false;
            if (user.Title == "LEADER" && status == "LEADER PROCESSING") canEdit = true;
            // ... 其他角色判斷
            
            if (!canEdit)
            {
                vm.Message = $"不允許執行主觀績效考核作業, Status={status}";
                vm.IsSaveDisabled = true;
                return vm;
            }

            // 2. 取得 Config (Remark, Weighting)
            vm.CurrentConfig = _repo.GetItemConfig(vm.Query.StationIds.FirstOrDefault(), vm.Query.Title, vm.Query.Item, vm.Query.DetailItem);

            // 3. 取得列表資料 (Join 後的結果)
            vm.DataGrid = _repo.GetGradeList(vm.Query, user.Section);

            vm.IsQueryDisabled = true; // 查詢後鎖定上方條件
            return vm;
        }

        public void SaveData(SubjectiveViewModel vm, string userId, bool isClose)
        {
            // 1. 資料驗證 (對應 JSP Line 56-60)
            // 檢查排名是否連續、是否重複等邏輯 (可在此處用 C# 實作)

            // 2. 計算分數 (對應 JSP Line 61: roundNumber 公式)
            // C# 實作: upperbound - ((rec-1)*((upperbound -lowerbound)/(ttlcnt-1)))
            int totalCount = vm.DataGrid.Count;
            foreach (var row in vm.DataGrid)
            {
                if (row.Record.HasValue && totalCount > 1)
                {
                    decimal range = vm.CurrentConfig.UpperBound - vm.CurrentConfig.LowerBound;
                    decimal step = range / (totalCount - 1);
                    row.Score = vm.CurrentConfig.UpperBound - ((row.Record.Value - 1) * step);
                    row.Score = Math.Round(row.Score.Value, 5);
                }
                else
                {
                    row.Score = vm.CurrentConfig.UpperBound;
                }

                // 3. 呼叫 Repository 存檔
                _repo.SaveGrade(row, vm.Query, userId);
                
                // 4. Log Confidential
                _repo.LogConfidential(userId, "主觀評比排名作業", $"Update RowId={row.RowId}...");
            }

            // 5. 如果是送件 (Close)
            if (isClose)
            {
                if (_repo.CheckAllItemsFilled(vm.Query))
                {
                    _repo.UpdateStatusToClose(vm.Query, "CurrentStatus", userId);
                    vm.Message = "存檔並結案完成";
                }
                else
                {
                    vm.Message = "尚有未輸入項目，無法結案";
                }
            }
            else
            {
                vm.Message = "存檔完成";
            }
        }
    }
}
