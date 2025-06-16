using System; using System.Collections.Generic; using MesTAManagementSystem_New.Models.Report; using MesTAManagementSystem_New.Repositories;

namespace MesTAManagementSystem_New.Services { public class ReportService { private readonly ReportRepository _reportRepository;

public ReportService(ReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public IEnumerable<BudgetReportModel> GetBudgetData(string year, string deptId, string userId, string func)
    {
        if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(deptId))
        {
            throw new ArgumentException("Year and Department ID are required.");
        }

        return _reportRepository.GetBudgetData(year, deptId, userId, func);
    }
}

}


