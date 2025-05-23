
[HttpPost]
public ActionResult Add(CerRegListVM vm)
{
    vm.StationList = _TestingService.GetStationIdList().ToList();
    vm.StationOptions = vm.StationList.Select(x => new SelectListItem
    {
        Value = x,
        Text = x
    });

    vm.CerItemList = _TestingService.GetCerItemIdList(vm.StationId).ToList();
    vm.CerItemOptions = vm.CerItemList.Select(x => new SelectListItem
    {
        Value = x,
        Text = x,
    }).ToList();

    var role = deptId; // OpSupervisor-Leader / OpTrainingGroup / Admin
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
        return View("Index", vm);
    }

    var cerId = certInfo.Cer_Id;
    var cerDate = certInfo.Cer_Date;

    if (service.IsDuplicateRegistration(cerId, cerDate, empId, cerItemId))
    {
        message = "資料已經存在!";
        TempData["ErrorMessage"] = message;
        return View("Index", vm);
    }

    if (!service.HasTrainingRecordInThreeMonths(empId, cerItemId))
    {
        message = $"考試科目:{cerItemId}, 三個月內沒有訓練紀錄，若要報名請洽訓練組! ({empId}, {stationId}, {monthly})";
        TempData["ErrorMessage"] = message;
        return View("Index", vm);
    }

    var examType = service.GetExamType(cerItemId);
    var scoreWR = examType == "OP" ? "NA" : "";
    var scoreOP = examType == "WR" ? "NA" : "";

    service.InsertRegistration(new CerRegListVM
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
            return View("Index", vm);
        }
