public class OnlineExamVM
{
    public string EmpId { get; set; }
    public string SelectedCourseId { get; set; }
    public string SelectedStationId { get; set; }

    public string ExamRegNo { get; set; }
    public string ExamStatus { get; set; } = "waiting";

    public int SpendTime { get; set; } = 0;
    public int AllTime { get; set; } = 0;

    public List<SelectListItem> CourseList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> StationList { get; set; } = new List<SelectListItem>();
}