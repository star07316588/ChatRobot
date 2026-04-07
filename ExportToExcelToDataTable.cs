        public ActionResult ExportToExcel(CerRegListVM vm)
        {
            string GetEmpIdShift = _testingService.GetShiftByEmpId(userId);
            var list = _testingService.GetEmployeeCertifications(SESGroup, GetEmpIdShift);

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("<table border='1'>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>部門</th><th>班別</th><th>站別</th><th>工號</th><th>姓名</th>");
            sb.AppendLine("<th>鑑定項目代碼</th><th>鑑定日期</th><th>鑑定類別</th>");
            sb.AppendLine("</tr>");

            foreach (var emp in list)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + emp.Dept_Id + "</td><td>" + emp.Shift_Id + "</td><td>" + emp.Station_Id + "</td><td>" + emp.Emp_Id + "</td><td>" + emp.Name + "</td>");
                sb.AppendLine("<td>" + emp.Cer_Item_Id + "</td><td>" + emp.Cer_Date + "</td><td>" + emp.Exam_Type + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            string filename = "TestingEmployee" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls";

            // 將字串轉成 byte array (使用 UTF8 編碼)
            var buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

            // 使用 MemoryStream 包裝回傳
            var stream = OfficeUtility.SaveDataTableToExcelMemoryStream(
                exportTable,
                "yyyy/MM/dd",
                SecurityLabel.Proprietary,
                PersonalLabel.General_PD);

            // 回傳檔案流，指定 content-type 和檔名
            return File(stream, "application/vnd.ms-excel", filename);
        }
