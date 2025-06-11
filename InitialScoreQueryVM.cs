public ScoreQueryVM InitialScoreQueryVM(string loginUserId)
{
    var vm = new ScoreQueryVM();

    // 設定年份選單（近三年）
    int currentYear = DateTime.Now.Year;
    vm.YearOptions = Enumerable.Range(currentYear - 2, 3)
        .Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() })
        .ToList();

    // 設定月份選單（1-12）
    vm.MonthOptions = Enumerable.Range(1, 12)
        .Select(m => new SelectListItem { Value = m.ToString("D2"), Text = m.ToString("D2") })
        .ToList();

    // 預設起訖年月為本月
    vm.StartYear = currentYear.ToString();
    vm.StartMonth = DateTime.Now.Month.ToString("D2");
    vm.EndYear = currentYear.ToString();
    vm.EndMonth = DateTime.Now.Month.ToString("D2");

    // 下拉選單：站別、試題、班別
    vm.StationOptions = _repo.GetStationList();
    vm.CerItemOptions = new List<SelectListItem> { new SelectListItem { Text = "ALL", Value = "ALL" } };
    vm.ShiftOptions = _repo.GetShiftList();

    // 判斷登入者是否是 OpOperator
    if (IsOpOperator(loginUserId))
    {
        vm.EmpId = loginUserId;
        vm.IsReadOnlyEmpId = true;
    }

    return vm;
}