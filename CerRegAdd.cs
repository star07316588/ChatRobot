[HttpPost]
public ActionResult Add(CerRegListVM vm)
{
    var userId = Session["userid"]?.ToString();
    var role = Session["role"]?.ToString(); // OpSupervisor-Leader / OpTrainingGroup / Admin
    var monthly = vm.Month;
    var empId = vm.EmpId;
    var cerItemId = vm.CertItemId;
    var stationId = vm.StationId;

    var message = "";
    var service = new TestingService();

    var certInfo = service.GetCertificateByCerItemId(cerItemId, role, monthly);
    if (certInfo == null)
    {
        message = $"考試科目:{cerItemId}, 該月份不在鑑定作業清單內, 新增失敗! (工號:{empId}, 站別:{stationId}, 月份:{monthly})";
        TempData["ErrorMessage"] = message;
        return RedirectToAction("RegList");
    }

    var cerId = certInfo.CerId;
    var cerDate = certInfo.CerDate;

    if (service.IsDuplicateRegistration(cerId, cerDate, empId, cerItemId))
    {
        message = "資料已經存在!";
        TempData["ErrorMessage"] = message;
        return RedirectToAction("RegList");
    }

    if (!service.HasTrainingRecordInThreeMonths(empId, cerItemId))
    {
        message = $"考試科目:{cerItemId}, 三個月內沒有訓練紀錄，若要報名請洽訓練組! ({empId}, {stationId}, {monthly})";
        TempData["ErrorMessage"] = message;
        return RedirectToAction("RegList");
    }

    var examType = service.GetExamType(cerItemId);
    var scoreWR = examType == "OP" ? "NA" : "";
    var scoreOP = examType == "WR" ? "NA" : "";

    service.InsertRegistration(new CertRegisterVM
    {
        CerId = cerId,
        EmpId = empId,
        CerDate = cerDate,
        CertItemId = cerItemId,
        ScoreWR = scoreWR,
        ScoreOP = scoreOP,
        CreateUser = userId
    });

    TempData["SuccessMessage"] = "資料新增完成!";
    return RedirectToAction("RegList");
}