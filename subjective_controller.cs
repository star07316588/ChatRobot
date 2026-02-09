using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // 用於 SelectList
using MyPerformanceApp.Models;
using MyPerformanceApp.Services;
using MyPerformanceApp.ViewModels;
using System;
using System.Collections.Generic;

namespace MyPerformanceApp.Controllers
{
    public class SubjectiveController : Controller
    {
        private readonly ISubjectiveService _service;

        // 建構子注入 Service
        public SubjectiveController(ISubjectiveService service)
        {
            _service = service;
        }

        /// <summary>
        /// 頁面入口與查詢功能
        /// </summary>
        /// <param name="queryVm">前端傳來的查詢條件 (若為初次進入則為空)</param>
        /// <param name="action">分辨是按下 "Query" 還是單純頁面載入</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(SubjectiveViewModel queryVm, string action)
        {
            // 模擬取得當前登入者 (實務上從 User.Identity 或 Session 取得)
            string userId = User.Identity.IsAuthenticated ? User.Identity.Name : "TEST_USER";

            // 如果按下查詢按鈕 (action="Query")
            if (action == "Query")
            {
                // 1. 呼叫 Service 執行查詢邏輯
                var resultVm = _service.PerformQuery(queryVm, userId);
                
                // 2. 重新填寫下拉選單 (因為 HTTP 是無狀態的，PostBack 後 Dropdown 資料會消失，需重抓)
                //    這裡建議 Service 內部或透過 Helper 方法補齊 Dropdown 的資料源
                PopulateDropdowns(resultVm);

                return View(resultVm);
            }
            else
            {
                // 初次載入頁面 (Init)
                var initVm = _service.LoadPage(userId);
                
                // 填寫預設下拉選單
                PopulateDropdowns(initVm);
                
                return View(initVm);
            }
        }

        /// <summary>
        /// 處理存檔與送件 (Save / Close)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // 防止 CSRF 攻擊
        public IActionResult Save(SubjectiveViewModel vm, string saveAction)
        {
            string userId = User.Identity.IsAuthenticated ? User.Identity.Name : "TEST_USER";
            bool isClose = (saveAction == "Close");

            try
            {
                // 1. 呼叫 Service 執行存檔或結案
                _service.SaveData(vm, userId, isClose);

                // 2. 存檔成功後，重新查詢資料以顯示最新狀態
                //    這裡我們直接再次呼叫 PerformQuery，確保畫面與資料庫同步
                var resultVm = _service.PerformQuery(vm, userId);
                
                // 3. 補回下拉選單資料
                PopulateDropdowns(resultVm);

                return View("Index", resultVm);
            }
            catch (Exception ex)
            {
                // 發生錯誤時，保留使用者原本輸入的資料並顯示錯誤
                vm.Message = "Error: " + ex.Message;
                PopulateDropdowns(vm);
                return View("Index", vm);
            }
        }

        /// <summary>
        /// AJAX API: 處理連動下拉選單 (對應 JSP 的 subjective_option.jsp)
        /// </summary>
        /// <param name="type">請求類型: STATION, ITEM, DETAILITEM</param>
        /// <param name="param1">上層參數 (如 deptId)</param>
        /// <param name="param2">次層參數 (如 stationId)</param>
        /// <returns>JSON 格式的選項清單</returns>
        [HttpGet]
        public JsonResult GetCascadingOptions(string type, string param1, string param2)
        {
            List<string> options = new List<string>();

            // 根據類型呼叫 Service/Repository 取得對應資料
            switch (type.ToUpper())
            {
                case "SHIFT":
                    // param1 = deptId
                    options = _service.GetShiftOptions(param1); 
                    break;
                case "STATION":
                    // param1 = deptId
                    options = _service.GetStationOptions(param1);
                    break;
                case "ITEM":
                    // param1 = stationId, param2 = title
                    options = _service.GetItemOptions(param1, param2);
                    break;
                case "DETAILITEM":
                    // param1 = stationId, param2 = item
                    // 注意：此處可能還需要 Title，需視實際 Service 簽章調整
                    options = _service.GetDetailItemOptions(param1, param2);
                    break;
            }

            return Json(options);
        }

        /// <summary>
        /// 輔助方法：重新填入 View 所需的下拉選單物件 (SelectList)
        /// </summary>
        private void PopulateDropdowns(SubjectiveViewModel vm)
        {
            // 這裡將 List<string> 轉換為 MVC 的 SelectList 物件
            // 實務上這段邏輯也可以包在 ViewModel 內部或 Service 中

            // 基本固定選單 (Year, Month, Dept, Title) - 假設這些在 LoadPage/PerformQuery 已填入 vm 的 List 屬性
            if (vm.YearOptions != null)
                ViewBag.YearList = new SelectList(vm.YearOptions, vm.Query.Year);
            
            if (vm.MonthOptions != null)
                ViewBag.MonthList = new SelectList(vm.MonthOptions, vm.Query.Month);

            // 若 Service 沒有回傳 List，在此處呼叫 Repository 補齊 (視架構設計而定)
            // ex: vm.DeptOptions = _service.GetDeptOptions();
        }

        public ActionResult SaveData(SubjectiveResultVM searchInfo)
        {
            bool bresult = false;
            foreach (var data in searchInfo.Data)
            {
                bresult = _reportService.SubjectiveSave(data.Emp_Id, data.Year, data.Month, data.Item, data.DetailItem, data.Record, data.Score, data.Comments, data.Title, searchInfo.Data.Count, userId);
            }

            return Json(new { success = bresult });
        }
    }
}
