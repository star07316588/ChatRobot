public class OnlineExamController : Controller { private readonly IExamService _examService;

public OnlineExamController(IExamService examService)
{
    _examService = examService;
}

[HttpPost]
public ActionResult StartExam(string empId, string cerItemId, string stationId)
{
    if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(cerItemId))
    {
        return Json(new { success = false, message = "缺少工號或試題代碼" });
    }

    // 將 score_writing 設為 0
    _examService.SetScoreWritingZero(empId, cerItemId);

    // 取得必要題（必考題）
    var necessaryQuestions = _examService.GetNecessaryQuestions(cerItemId);

    // 取得是非與選擇題
    var trueFalseAndChoiceQuestions = _examService.GetTrueFalseAndChoiceQuestions(cerItemId);

    // 隨機抽題
    var sampledQuestions = _examService.SampleTrueFalseAndChoiceQuestions(trueFalseAndChoiceQuestions);

    // 取得連連看題目
    var linkQuestions = _examService.GetLinkQuestions(cerItemId);

    // 計算總考試時間（以課程設定為準）
    int totalExamTime = _examService.GetTotalExamTime(cerItemId);

    // 建立 Session 或 TempData 傳入 PageExamList 使用
    TempData["EmpId"] = empId;
    TempData["CerItemId"] = cerItemId;
    TempData["StationId"] = stationId;
    TempData["ExamStartTime"] = DateTime.Now;
    TempData["TotalExamTime"] = totalExamTime;

    TempData["NecessaryQuestions"] = necessaryQuestions;
    TempData["TrueFalseQuestions"] = sampledQuestions.TrueFalse;
    TempData["ChoiceQuestions"] = sampledQuestions.Choice;
    TempData["LinkQuestions"] = linkQuestions;

    return Json(new { success = true });
}

}

