private readonly TestingService _service = new TestingService();

public ActionResult GetQuestionList(string tableName, string certItemId)
{
    var result = _service.GetTestSpec(tableName, certItemId);
    return View(result);
}

public void BackupCerRegToDelete(string cerRegNo, string userId)
{
    _repo.BackupCerReg(cerRegNo, userId);
}

public void DeleteCerReg(string cerRegNo, string userId)
{
    _repo.DeleteCerReg(cerRegNo, userId);
}