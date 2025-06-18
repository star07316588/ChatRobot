public class ReportService
{
    private readonly ReportRepository _repository;

    public ReportService(ReportRepository repository)
    {
        _repository = repository;
    }

    public List<Dictionary<string, object>> GetStatusData(
        string startYear, string startMonth,
        string endYear, string endMonth,
        string deptId, string stationId,
        string userId, string functionName)
    {
        return _repository.GetStatusData(
            startYear, startMonth, endYear, endMonth,
            deptId, stationId, userId, functionName);
    }
}