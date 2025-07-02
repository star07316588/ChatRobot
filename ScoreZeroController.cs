[HttpPost]
public ActionResult StartExam(string empId, string cerItemId)
{
    _examService.SetScoreWritingZero(empId, cerItemId);
    return Json(new { success = true });
}