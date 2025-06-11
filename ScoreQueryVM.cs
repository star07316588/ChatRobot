namespace MesTAManagementSystem_New.ViewModels.Training.Testing
{
    public class ScoreQueryVM
    {
        public string StationId { get; set; }
        public string CerItemId { get; set; }
        public string StartYear { get; set; }
        public string StartMonth { get; set; }
        public string EndYear { get; set; }
        public string EndMonth { get; set; }
        public string ShiftId { get; set; }
        public string EmpId { get; set; }

        public List<SelectListItem> StationOptions { get; set; }
        public List<SelectListItem> CerItemOptions { get; set; }
        public List<SelectListItem> YearOptions { get; set; }
        public List<SelectListItem> MonthOptions { get; set; }
        public List<SelectListItem> ShiftOptions { get; set; }

        public List<ScoreQueryModel> ResultList { get; set; } = new List<ScoreQueryModel>();
    }
}