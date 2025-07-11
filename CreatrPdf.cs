public ActionResult ExamResult(ExamResultVM model)
{
    // 建立檔名與路徑
    string fileName = $"ExamResult_{model.EmpId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
    string folderPath = Server.MapPath("~/PDFResults/");
    string filePath = Path.Combine(folderPath, fileName);

    // 渲染 View 為 PDF
    var pdfView = new Rotativa.ViewAsPdf("ExamResult", model)
    {
        PageSize = Rotativa.Options.Size.A4
    };

    // 建立 PDF byte 並儲存於伺服器
    var pdfBytes = pdfView.BuildFile(ControllerContext);
    System.IO.File.WriteAllBytes(filePath, pdfBytes);

    // 回傳原本頁面
    return View("ExamResult", model);
}