[HttpPost]
public ActionResult ExportToExcel(CerRegListVM vm)
{
    // 查詢要匯出的資料
    var list = _TestingService.GetEmployeeList(vm.EmpId, vm.StationId, vm.CertItemId, vm.Month, vm.SortType);

    var sb = new System.Text.StringBuilder();

    sb.AppendLine("<table border='1'>");
    sb.AppendLine("<tr>");
    sb.AppendLine("<th>部門</th><th>班別</th><th>站別</th><th>工號</th><th>姓名</th>");
    sb.AppendLine("<th>鑑定項目</th><th>證照編號</th><th>鑑定月份</th><th>類別</th><th>本月</th>");
    sb.AppendLine("</tr>");

    foreach (var emp in list)
    {
        sb.AppendLine("<tr>");
        sb.AppendLine($"<td>{emp.Dept_Id}</td><td>{emp.Shift_Id}</td><td>{emp.Station_Id}</td><td>{emp.Emp_Id}</td><td>{emp.Name}</td>");
        sb.AppendLine($"<td>{emp.Cer_Item_Id}</td><td>{emp.Cer_Reg_No}</td><td>{emp.Cer_Date}</td><td>{emp.Exam_Type}</td><td>{emp.This_Month}</td>");
        sb.AppendLine("</tr>");
    }

    sb.AppendLine("</table>");

    return Content(sb.ToString(), "application/vnd.ms-excel", System.Text.Encoding.UTF8);
}