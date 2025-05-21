private readonly TestingService _service = new TestingService();

public ActionResult GetQuestionList(string tableName, string certItemId)
{
    var result = _service.GetTestSpec(tableName, certItemId);
    return View(result);
}
