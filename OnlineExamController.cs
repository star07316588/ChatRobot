public IActionResult Index()
{
    var model = new OnlineExamVM
    {
        EmpId = HttpContext.Session.GetString("userid") ?? "",
        CourseList = _examService.GetCourseListForUser(userId),
        StationList = new List<SelectListItem>() // 初始為空，依課程帶入
    };

    return View("OnlineExam", model);
}

[HttpPost]
public IActionResult StartExam(OnlineExamVM model)
{
    if (string.IsNullOrWhiteSpace(model.EmpId) || string.IsNullOrWhiteSpace(model.SelectedCourseId))
    {
        ModelState.AddModelError("", "請選擇工號與試題項目代碼");
        return View("OnlineExam", model);
    }

    // 重導向到考題頁面 PageExamList
    return RedirectToAction("PageExamList", new
    {
        empId = model.EmpId,
        cerItemId = model.SelectedCourseId,
        stationId = model.SelectedStationId,
        examRegNo = model.ExamRegNo
    });
}