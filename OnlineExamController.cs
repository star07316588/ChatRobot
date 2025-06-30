public IActionResult Index()
{
    var model = new OnlineExamVM
    {
        EmpId = HttpContext.Session.GetString("userid") ?? "",
        CourseList = _examService.GetCourseListForUser(userId),
        StationList = new List<SelectListItem>() // 初始為空，依課程帶入
    };

    return View("OnlineExam", model);
}

[HttpPost]
public IActionResult StartExam(OnlineExamVM model)
{
    if (string.IsNullOrWhiteSpace(model.EmpId) || string.IsNullOrWhiteSpace(model.SelectedCourseId))
    {
        ModelState.AddModelError("", "請選擇工號與試題項目代碼");
        return View("OnlineExam", model);
    }

    // 重導向到考題頁面 PageExamList
    return RedirectToAction("PageExamList", new
    {
        empId = model.EmpId,
        cerItemId = model.SelectedCourseId,
        stationId = model.SelectedStationId,
        examRegNo = model.ExamRegNo
    });
}

public class OnlineExamController : Controller
    {
        // 顯示考試首頁（登入與題目載入）
        public IActionResult Index()
        {
            var vm = new OnlineExamVM();

            // 預設抓取使用者ID (可從登入Session或模擬資料)
            vm.EmpId = User.Identity?.Name ?? "TestUser";

            // 模擬課程清單
            vm.CerItemList = new List<SelectListItem> {
                new SelectListItem { Text = "COURSE001", Value = "COURSE001" },
                new SelectListItem { Text = "COURSE002", Value = "COURSE002" }
            };

            // 模擬站別選單
            vm.StationList = new List<SelectListItem> {
                new SelectListItem { Text = "A", Value = "A" },
                new SelectListItem { Text = "B", Value = "B" }
            };

            return View("OnlineExam", vm);
        }

        // 開始考試時載入題目與頁面
        [HttpPost]
        public IActionResult StartExam(OnlineExamVM vm)
        {
            var result = new PageExamListVM
            {
                EmpId = vm.EmpId,
                CerItemId = vm.SelectedCerItemId,
                StationId = vm.StationId,
                ExamRegNo = vm.ExamRegNo
            };

            // 模擬題庫（實際請查資料庫）
            result.NecessaryQuestions = new List<QuestionVM>
            {
                new QuestionVM { Question = "請輸入答案A", CorrectAnswer = "A" },
                new QuestionVM { Question = "請輸入答案B", CorrectAnswer = "B" }
            };
            result.TrueFalseQuestions = new List<QuestionVM>
            {
                new QuestionVM { Question = "太陽是星星嗎？", CorrectAnswer = "T" }
            };
            result.ChoiceQuestions = new List<QuestionVM>
            {
                new QuestionVM { Question = "地球是第幾顆行星？ (A)1 (B)3 (C)5", CorrectAnswer = "B" }
            };

            return View("PageExamList", result);
        }

        // 提交考卷並進行評分
        [HttpPost]
        public IActionResult SubmitExam(PageExamListVM model)
        {
            int score = 0;
            int correct = 0;

            // 必考題評分
            for (int i = 0; i < model.NecessaryQuestions.Count; i++)
            {
                var userAns = model.NecessaryAnswers.ElementAtOrDefault(i);
                var correctAns = model.NecessaryQuestions[i].CorrectAnswer;
                model.NecessaryResults.Add(new QuestionResultVM
                {
                    Question = model.NecessaryQuestions[i].Question,
                    UserAnswer = userAns,
                    CorrectAnswer = correctAns
                });
                if (userAns?.Trim().ToUpper() == correctAns.Trim().ToUpper())
                    correct++;
            }

            // 是非題評分
            for (int i = 0; i < model.TrueFalseQuestions.Count; i++)
            {
                var userAns = model.TFAnswers.ElementAtOrDefault(i);
                var correctAns = model.TrueFalseQuestions[i].CorrectAnswer;
                model.TFResults.Add(new QuestionResultVM
                {
                    Question = model.TrueFalseQuestions[i].Question,
                    UserAnswer = userAns,
                    CorrectAnswer = correctAns
                });
                if (userAns?.Trim().ToUpper() == correctAns.Trim().ToUpper())
                    correct++;
            }

            // 選擇題評分
            for (int i = 0; i < model.ChoiceQuestions.Count; i++)
            {
                var userAns = model.ChooseAnswers.ElementAtOrDefault(i);
                var correctAns = model.ChoiceQuestions[i].CorrectAnswer;
                model.ChoiceResults.Add(new QuestionResultVM
                {
                    Question = model.ChoiceQuestions[i].Question,
                    UserAnswer = userAns,
                    CorrectAnswer = correctAns
                });
                if (userAns?.Trim().ToUpper() == correctAns.Trim().ToUpper())
                    correct++;
            }

            model.CorrectCount = correct;
            model.Score = (int)((double)correct / model.TotalQuestions * 100);

            return View("ExamResult", model); // 可建立 ExamResult.cshtml 顯示詳細評分與結果
        }
    }
}