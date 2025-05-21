using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MesTAManagementSystem_New.ViewModels.Training.Testing;
using MesTAManagementSystem_New.Repositories;
using NPOI.HSSF.UserModel;
using System.Text;
using System;
using System.Collections.Generic;


namespace MesTAManagementSystem_New.Controllers.Training.Testing
{
    public class QuestionController : Controller
    {
        private readonly BaseRepository _baseRepository;
        private readonly TestingRepository _testingRepository;
        public QuestionController()
        {
            this._baseRepository = new BaseRepository();
            this._testingRepository = new TestingRepository();
        }
        //
        // GET: /EmployeeUserMaintain/


        public ActionResult Index()
        {
            var vm = new QuestionVM();
            // 填充必要資料
            return View(vm); // 或 return View(vm) 如果View名稱同方法名
        }


        public ActionResult NewQuestion()
        {
            var stationList = _testingRepository.GetStationIdList().ToList();
            var viewModel = new QuestionVM
            {
                StationList = stationList,
                StationOptions = stationList.Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                })
            };
            // 這裡可以預先帶入下拉選單等初始化資料
            return View("NewQuestion", viewModel);
        }
        public ActionResult ModifyQuestion()
        {
            var stationList = _testingRepository.GetStationIdList().ToList();
            var viewModel = new QuestionVM
            {
                StationList = stationList,
                StationOptions = stationList.Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                })
            };
            // 這裡可以預先帶入下拉選單等初始化資料
            return View("ModifyQuestion", viewModel);
        }
        public JsonResult GetCerItemIdList(string stationId)
        {
            var certItemList = _testingRepository.GetCerItemIdList(stationId).ToList();
            var certItemOptions = certItemList.Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList();


            return Json(certItemOptions, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ModifyExam(string examSubject, string CertItemId)
        {
            string tableName = examSubject == "1" ? "sbl_question_spec" : "sbl_operation_spec";
            var data = _testingRepository.GetTestSpec(tableName, CertItemId);
            var vm = new QuestionVM
            {
                CertItemId = CertItemId,
                EditQuestions = data
            };


            return Json(vm, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveExam(List<QuestionVM> questions)
        {
            foreach (var q in questions)
            {
                string tableName = q.ExamSubject == "1" ? "sbl_question_spec" : "sbl_operation_spec";
                _testingRepository.UpdateQuestion(tableName, q);
            }


            return Json(new { success = true });
        }


        [HttpPost]
        public ActionResult Preview(HttpPostedFileBase txtFile, QuestionVM viewModel)
        {
            if (txtFile == null || txtFile.ContentLength == 0)
            {
                ModelState.AddModelError("", "請選擇 Excel 檔案！");
                return View("NewQuestion");
            }


            // 檢查副檔名
            var ext = Path.GetExtension(txtFile.FileName).ToLower();
            if (ext != ".xls")
            {
                ModelState.AddModelError("", "請上傳 .xls 格式的檔案！");
                return View("NewQuestion");
            }


            string fileName = Path.GetFileNameWithoutExtension(txtFile.FileName);
            string newFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
            string path = Path.Combine(Server.MapPath("~/Content/Excel/"), newFileName);
            txtFile.SaveAs(path);


            string htmlTable = ReadExcelToHtmlTable(txtFile.InputStream, viewModel);


            // 將 HTML 給 ViewModel 回傳給 Preview.cshtml 顯示
            viewModel.Html = htmlTable;
            viewModel.Filenames = newFileName;


            return View("Preview", viewModel);
        }




        private string ReadExcelToHtmlTable(Stream stream, QuestionVM viewModel)
        {
            var sb = new StringBuilder();
            var workbook = new HSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);


            sb.Append("<table border='1' cellpadding='5' cellspacing='0' id='QuestionTable'>");


            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;


                sb.Append("<tr>");
                for (int j = 0; j < row.LastCellNum; j++)
                {
                    var cell = row.GetCell(j);
                    string value = cell != null ? cell.ToString() : "";
                    sb.AppendFormat("<td>{0}</td>", HttpUtility.HtmlEncode(value));
                }
                sb.Append("</tr>");
            }


            sb.Append("</table>");


            // 處理圖片上傳（可多張）
            if (viewModel.Images != null && viewModel.Images.Count > 0)
            {
                foreach (var img in viewModel.Images)
                {
                    if (img != null && img.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(img.FileName);
                        string ext = Path.GetExtension(img.FileName);
                        string newFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
                        string path = Path.Combine(Server.MapPath("~/Content/Images/"), newFileName);
                        img.SaveAs(path);


                        string imgUrl = Url.Content("~/Content/Images/" + newFileName);
                        sb.Append($"<div><img src='{imgUrl}' style='max-width:500px; margin-top:10px;' /></div>");
                    }
                }
            }
            return sb.ToString();
        }


        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase txtFile, QuestionVM model)
        {
            if(model.Filenames == null)
            {
                if (txtFile == null || txtFile.ContentLength == 0)
                {
                    ModelState.AddModelError("", "請選擇 Excel 檔案！");
                    return View("NewQuestion");
                }


                // 檢查副檔名
                var ext = Path.GetExtension(txtFile.FileName).ToLower();
                if (ext != ".xls")
                {
                    ModelState.AddModelError("", "請上傳 .xls 格式的檔案！");
                    return View("NewQuestion");
                }


                string fileName = Path.GetFileNameWithoutExtension(txtFile.FileName);
                string newFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
                string path = Path.Combine(Server.MapPath("~/Content/Excel/"), newFileName);
                txtFile.SaveAs(path);
                model.Filenames = newFileName;
            }
            // model.CertItemId, model.Filenames, model.StationId 等都會自動帶入
            // 你可以直接用 model.Filenames 來找 Excel 檔案處理
            var excelPath = Path.Combine(Server.MapPath("~/Content/Excel/"), model.Filenames);
            var result = InsertExcelToOracle(excelPath, model.CertItemId);


            if (!string.IsNullOrEmpty(result))
            {
                TempData["Error"] = result;
                return RedirectToAction("Preview", model);
            }


            TempData["Success"] = "匯入成功";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public string InsertExcelToOracle(string excelPath, string certItemId)
        {
            string filenames = Path.GetFileNameWithoutExtension(excelPath);
            string stationId = Request["StationId"];
            string examSubject = Request["ExamSubject"];
            string excelFileName = "";
            string[] files = filenames?.Split(',') ?? new string[0];


            string tableName = examSubject == "1" ? "sbl_question_spec" : "sbl_operation_spec";
            var result = "";


            // check lacked pics
            if (examSubject == "1")
            {
                foreach (var file in files)
                {
                    if (file.ToLower().EndsWith(".xls"))
                    {
                        excelFileName = file;
                        break;
                    }
                }
                using (var fs = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
                {
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        ms.Position = 0;


                        excelPath = Path.Combine(Server.MapPath("~/Content/Excel/"), excelFileName);
                        var workbook = new HSSFWorkbook(ms);
                        var sheet = workbook.GetSheetAt(0);


                        result = _testingRepository.SaveToQuestionSpec(filenames, tableName, certItemId, sheet);
                    }
                }
            }


            return result;
        }
    }
}
