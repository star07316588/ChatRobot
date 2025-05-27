[HttpPost]
public ActionResult Delete(CerRegListVM vm)
{
    var userId = Session["userid"]?.ToString();
    var delRowIds = vm.DeleteIds; // string[]，來源於 checkbox 選擇項

    if (delRowIds != null && delRowIds.Any())
    {
        var service = new TestingService();

        foreach (var regNo in delRowIds)
        {
            // 將資料備份進刪除表
            service.BackupCerRegToDelete(regNo, userId);

            // 正式刪除 sbl_cer_reg 資料
            service.DeleteCerReg(regNo, userId);
        }

        TempData["SuccessMessage"] = "選取資料已成功刪除。";
    }
    else
    {
        TempData["ErrorMessage"] = "請先勾選欲刪除的資料。";
    }

    return RedirectToAction("RegList");
}