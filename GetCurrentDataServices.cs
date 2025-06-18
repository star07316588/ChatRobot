using System;
using System.Threading.Tasks;
using MesTAManagementSystem_New.Repositories;

namespace MesTAManagementSystem_New.Services
{
    public class ReportService
    {
        private readonly ReportRepository _reportRepository;

        public ReportService(ReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// 取得系統日期（支援年/月/日 或 組合格式）
        /// </summary>
        /// <param name="datePart">y, m, d, ym, ym-1</param>
        /// <param name="dateAdd">加減幾年/月</param>
        /// <returns>字串型態的結果</returns>
        public async Task<string> GetCurrentDateAsync(string datePart, int dateAdd)
        {
            return await _reportRepository.GetCurrentDateAsync(datePart, dateAdd);
        }
    }
}