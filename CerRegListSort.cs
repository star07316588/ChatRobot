public ActionResult GetSortedTable(string empId, string stationId, string certItemId, string month, string sortType)
{
    var list = _TestingService.GetEmployeeList(empId, stationId, certItemId, month, sortType);
    return PartialView("_EmployeeTable", list);
}