你要設定下載的 Excel 檔名為 TestingEmployee_yyyymmddhhmissfff.xls（非 .xml，若為 Excel 檔應為 .xls），以下是修改後的 Controller ExportToExcel 實作：


---

✅ Controller：加入動態檔名設定

[HttpPost]
public ActionResult ExportToExcel(CerRegListVM vm)
{
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

    string filename = $"TestingEmployee_{DateTime.Now:yyyyMMddHHmmssfff}.xls";

    Response.ClearContent();
    Response.Buffer = true;
    Response.AddHeader("content-disposition", $"attachment; filename={filename}");
    Response.ContentType = "application/vnd.ms-excel";
    Response.Charset = "";
    Response.ContentEncoding = System.Text.Encoding.UTF8;
    Response.Write(sb.ToString());
    Response.End();

    return new EmptyResult();
}


---

✅ 輸出特點：

匯出為 Excel .xls（可用舊版 Excel 開啟）

檔名為 TestingEmployee_20250522123045888.xls 格式

不需外掛程式或元件



---

是否想改為 xlsx 檔（需 EPPlus），或增加篩選條件、Excel 標題、匯出全欄寬等？我也可以幫你做。要進階優化嗎？

