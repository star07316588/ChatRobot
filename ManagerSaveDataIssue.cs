c# mvc view 儲存時出現 發生錯誤: Internal Server Error

//View
    function saveData(isClose) {
        // 1. 確保 Grid 離開編輯狀態 (將正在編輯的 Cell 寫入 local data)
        var $grid = $resultTable;
        var editRowId = $grid.jqGrid('getGridParam', 'selrow');
        if (editRowId) {
            // 強制儲存當前焦點欄位
            $grid.jqGrid('saveCell', editRowId, 'Record');
            $grid.jqGrid('saveCell', editRowId, 'Comments');
        }

        // 2. 取得 Grid 所有資料 (這時候只有 Emp_Id, Score, Record, Comments...)
        var gridRows = $grid.jqGrid('getRowData');

        // 3. 取得外部查詢條件 (Context)
        var currentYear = $("#Year").val();
        var currentMonth = $("#Month").val();
        var currentDept = $("#Dept_Id").val(); // 若 Controller 需要
        var currentItem = $("#Item").val();
        var currentDetailItem = $("#DetailItem").val();
        var currentLoginTitle = $("#LoginTitle").val();
        var currentSelectedStationIds = $('#StationListBox').val();
        var currentSelectedShiftIds = $('#ShiftListBox').val();
        var currentSelectedTitleIds = $('#TitleListBox').val();

        // 4. [關鍵] 將查詢條件注入到每一筆資料列中
        // 因為您的 Controller 是用 data.Year, data.Month，所以每一列都要有這些欄位
        var dataToSend = [];
        $.each(gridRows, function(index, row) {
            // 複製一份 row 資料
            var newRow = $.extend({}, row);

            // 手動補上缺少的欄位
            newRow.Year = currentYear;
            newRow.Month = currentMonth;
            newRow.Item = currentItem;
            newRow.DetailItem = currentDetailItem;
            newRow.Dept_Id = currentDept;
            newRow.SelectedStationIds = currentSelectedStationIds;
            newRow.SelectedShiftIds = currentSelectedShiftIds;
            newRow.SelectedTitleIds = currentSelectedTitleIds;
            newRow.LoginTitle = currentLoginTitle;

            dataToSend.push(newRow);
        });

        // 5. 準備 Post 物件 (對應 ManagerResultVM)
        // 結構: { Data: [...], IsClose: true/false }
        var postData = {
            DataSend: dataToSend,
            IsClose: isClose 
        };

        $.ajax({
            url: '@Url.Action("SaveData")',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(postData), // 對應參數名稱 searchInfo
            beforeSend: function () {
                $.blockUI({ message: '資料處理中...' });
            },
            success: function (result) {
                if (result.success) {
                    alert(isClose ? '送件結案成功！' : '存檔成功！');
                    $submitButton.click();
                } else {
                    alert('作業失敗'); 
                }
            },
            error: function(xhr, status, error) {
                    alert('發生錯誤: ' + error);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    }


//controller
        public ActionResult SaveData(ManagerResultVM searchInfo)
        {
            bool bresult = false;

            // 1. 執行每一筆的存檔
            foreach (var data in searchInfo.Data)
            {
                // 確保 data 裡面的 Year, Month 等屬性有被 JS 正確填入
                bresult = _reportService.ManagerSave(data.Emp_Id, data.Year, data.Month, data.Item, data.DetailItem, data.Total, data.Record, data.Final, data.Comments, userId);
            }

            // 2. 若前端傳來 IsClose = true，則執行結案動作
            if (searchInfo.IsClose && bresult)
            {
                // 需要從 Data 中取第一筆來獲得 Year/Month 等資訊
                var firstRow = searchInfo.Data.FirstOrDefault();
                if (firstRow != null)
                {
                    string Title = string.Empty;
                    string ShiftId = string.Empty;
                    string Station_Id = string.Empty;
                    if (firstRow.SelectedTitleIds != null)
                    {
                        Title = "'" + string.Join("','", firstRow.SelectedTitleIds) + "'";
                    }
                    if (firstRow.SelectedShiftIds != null)
                    {
                        ShiftId = "'" + string.Join("','", firstRow.SelectedShiftIds) + "'";
                    }
                    if (firstRow.SelectedStationIds != null)
                    {
                        Station_Id = "'" + string.Join("','", firstRow.SelectedStationIds) + "'";
                    }
                    // 呼叫 Service 執行結案 (更新 Status)
                    _reportService.CloseManager(firstRow.LoginTitle, firstRow.Month, firstRow.Year, firstRow.Month, firstRow.Dept_Id, firstRow.Shift_Id, firstRow.Title, Station_Id);
                }
            }

            return Json(new { success = bresult });
        }

//ViewModel

    public class ManagerVM
    {
        public List<SelectListItem> YearList { get; set; }       // 年度
        public List<SelectListItem> MonthList { get; set; }       // 月份
        public List<SelectListItem> TitleIdList { get; set; }     // 部門代碼
        public List<SelectListItem> ShiftIdList { get; set; }     // 班別代碼
        public List<SelectListItem> StationIdList { get; set; }     // 班別代碼
        public List<SelectListItem> ItemList { get; set; }     // 項目代碼
        public List<SelectListItem> DetailItemList { get; set; }     // 細項代碼
        public List<string> SelectedTitleIds { get; set; }
        public List<string> SelectedShiftIds { get; set; }
        public List<string> SelectedStationIds { get; set; }
        [Display(Name = "年度")]
        public string Year { get; set; }       // 起年度
        [Display(Name = "月份")]
        public string Month { get; set; }       // 起月份
        [Display(Name = "部門")]
        public string Dept_Id { get; set; }     // 部門
        [Display(Name = "登入者職稱")]
        public string LoginTitle { get; set; } //登入者職稱
        [Display(Name = "職稱")]
        public string Title { get; set; }     // 職稱
        [Display(Name = "班別")]
        public string Shift_Id { get; set; }     // 班別
        [Display(Name = "站別")]
        public string Station_Id { get; set; }     // 站別
        [Display(Name = "項目")]
        public string Item { get; set; }     // 項目
        [Display(Name = "細項")]
        public string DetailItem { get; set; }     // 細項
        [Display(Name = "工號")]
        public string Emp_Id { get; set; }     // 工號
        [Display(Name = "姓名")]
        public string Name { get; set; }     // 姓名
        [Display(Name = "Section")]
        public string Section { get; set; }     // Section
        [Display(Name = "職等群組")]
        public string Position_Group { get; set; }     // 職等群組
        [Display(Name = "總分/獎金")]
        public string Total { get; set; }     //總分/獎金
        [Display(Name = "調整")]
        public string Record { get; set; }      //調整
        [Display(Name = "結算總分/獎金")]
        public string Final { get; set; }       //結算總分/獎金
        [Display(Name = "Comments")]
        public string Comments { get; set; }     // 評論
        [Display(Name = "主/客觀")]
        public string New_Type { get; set; }     // 主/客觀
        [Display(Name = "status")]
        public string Status { get; set; }     // status
        [Display(Name = "createuser")]
        public string CreateuserId { get; set; }     // createuser
        [Display(Name = "createtime")]
        public string Createtime { get; set; }     // createtime
        [Display(Name = "updateuser")]
        public string UpdateuserId { get; set; }     // updateuser
        [Display(Name = "updatetime")]
        public string Updatetime { get; set; }     // updatetime
        [Display(Name = "結案")]
        public bool IsClose { get; set; }

        public string RowId { get; set; }       // 若為空代表尚未新增
        public decimal Score { get; set; }      // 分數
    }

    public class ManagerConfig
    {
        public decimal Weighting { get; set; }
        public decimal UpperBound { get; set; }
        public decimal LowerBound { get; set; }
        public string ItemRemark { get; set; }
        public string HeadTitle { get; set; }       // 考核標題 (e.g. LEADER)
        public string ItemName { get; set; }        // 項目名稱
        public string DetailItemName { get; set; }  // 細項名稱
        public string DetailItemRemark { get; set; }
        public string BudgetRemark { get; set; }
        public string UsedbudgetRemark { get; set; }
        public decimal Budget { get; set; }
        public decimal Usedbudget { get; set; }
        public decimal Maximum { get; set; }
    }

    public class ManagerResultVM
    {
        public ManagerConfig Config { get; set; }
        public List<ManagerVM> Data { get; set; }

        public bool IsClose { get; set; }
    }

"<!DOCTYPE html>
<html>
    <head>
        <title>The JSON request was too large to be deserialized.</title>
        <meta name="viewport" content="width=device-width" />
        <style>
         body {font-family:"Verdana";font-weight:normal;font-size: .7em;color:black;} 
         p {font-family:"Verdana";font-weight:normal;color:black;margin-top: -5px}
         b {font-family:"Verdana";font-weight:bold;color:black;margin-top: -5px}
         H1 { font-family:"Verdana";font-weight:normal;font-size:18pt;color:red }
         H2 { font-family:"Verdana";font-weight:normal;font-size:14pt;color:maroon }
         pre {font-family:"Consolas","Lucida Console",Monospace;font-size:11pt;margin:0;padding:0.5em;line-height:14pt}
         .marker {font-weight: bold; color: black;text-decoration: none;}
         .version {color: gray;}
         .error {margin-bottom: 10px;}
         .expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }
         @media screen and (max-width: 639px) {
          pre { width: 440px; overflow: auto; white-space: pre-wrap; word-wrap: break-word; }
         }
         @media screen and (max-width: 479px) {
          pre { width: 280px; }
         }
        </style>
    </head>

    <body bgcolor="white">

            <span><H1>'/' 應用程式中發生伺服器錯誤。<hr width=100% size=1 color=silver></H1>

            <h2> <i>The JSON request was too large to be deserialized.</i> </h2></span>

            <font face="Arial, Helvetica, Geneva, SunSans-Regular, sans-serif ">

            <b> 描述: </b>在執行目前 Web 要求的過程中發生未處理的例外狀況。請檢閱堆疊追蹤以取得錯誤的詳細資訊，以及在程式碼中產生的位置。

            <br><br>

            <b> 例外狀況詳細資訊: </b>System.InvalidOperationException: The JSON request was too large to be deserialized.<br><br>

            <b>原始程式錯誤:</b> <br><br>

            <table width=100% bgcolor="#ffffcc">
               <tr>
                  <td>
                      <code>

在執行目前 Web 要求期間，產生未處理的例外狀況。如需有關例外狀況來源與位置的資訊，可以使用下列的例外狀況堆疊追蹤取得。</code>

                  </td>
               </tr>
            </table>

            <br>

            <b>堆疊追蹤:</b> <br><br>

            <table width=100% bgcolor="#ffffcc">
               <tr>
                  <td>
                      <code><pre>

[InvalidOperationException: The JSON request was too large to be deserialized.]
   System.Web.Mvc.EntryLimitedDictionary.Add(String key, Object value) +95
   System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value) +361
   System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value) +146
   System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value) +323
   System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value) +146
   System.Web.Mvc.JsonValueProviderFactory.GetValueProvider(ControllerContext controllerContext) +88
   System.Web.Mvc.ValueProviderFactoryCollection.GetValueProvider(ControllerContext controllerContext) +69
   System.Web.Mvc.ControllerBase.get_ValueProvider() +30
   System.Web.Mvc.ControllerActionInvoker.GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor) +62
   System.Web.Mvc.ControllerActionInvoker.GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor) +105
   System.Web.Mvc.Async.&lt;&gt;c__DisplayClass21.&lt;BeginInvokeAction&gt;b__19(AsyncCallback asyncCallback, Object asyncState) +743
   System.Web.Mvc.Async.WrappedAsyncResult`1.CallBeginDelegate(AsyncCallback callback, Object callbackState) +14
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout) +128
   System.Web.Mvc.Async.AsyncControllerActionInvoker.BeginInvokeAction(ControllerContext controllerContext, String actionName, AsyncCallback callback, Object state) +343
   System.Web.Mvc.Controller.&lt;BeginExecuteCore&gt;b__1c(AsyncCallback asyncCallback, Object asyncState, ExecuteCoreState innerState) +25
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallBeginDelegate(AsyncCallback callback, Object callbackState) +30
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout) +128
   System.Web.Mvc.Controller.BeginExecuteCore(AsyncCallback callback, Object state) +465
   System.Web.Mvc.Controller.&lt;BeginExecute&gt;b__14(AsyncCallback asyncCallback, Object callbackState, Controller controller) +18
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallBeginDelegate(AsyncCallback callback, Object callbackState) +20
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout) +128
   System.Web.Mvc.Controller.BeginExecute(RequestContext requestContext, AsyncCallback callback, Object state) +374
   System.Web.Mvc.Controller.System.Web.Mvc.Async.IAsyncController.BeginExecute(RequestContext requestContext, AsyncCallback callback, Object state) +16
   System.Web.Mvc.MvcHandler.&lt;BeginProcessRequest&gt;b__4(AsyncCallback asyncCallback, Object asyncState, ProcessRequestState innerState) +52
   System.Web.Mvc.Async.WrappedAsyncVoid`1.CallBeginDelegate(AsyncCallback callback, Object callbackState) +30
   System.Web.Mvc.Async.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout) +128
   System.Web.Mvc.MvcHandler.BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, Object state) +384
   System.Web.Mvc.MvcHandler.BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, Object state) +48
   System.Web.Mvc.MvcHandler.System.Web.IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData) +16
   System.Web.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute() +103
   System.Web.HttpApplication.ExecuteStepImpl(IExecutionStep step) +48
   System.Web.HttpApplication.ExecuteStep(IExecutionStep step, Boolean&amp; completedSynchronously) +159
</pre></code>

                  </td>
               </tr>
            </table>

            <br>

            <hr width=100% size=1 color=silver>

            <b>版本資訊:</b>&nbsp;Microsoft .NET Framework 版本:4.0.30319; ASP.NET 版本:4.7.4108.0

            </font>

    </body>
</html>
<!-- 
[InvalidOperationException]: The JSON request was too large to be deserialized.
   於 System.Web.Mvc.JsonValueProviderFactory.EntryLimitedDictionary.Add(String key, Object value)
   於 System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value)
   於 System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value)
   於 System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value)
   於 System.Web.Mvc.JsonValueProviderFactory.AddToBackingStore(EntryLimitedDictionary backingStore, String prefix, Object value)
   於 System.Web.Mvc.JsonValueProviderFactory.GetValueProvider(ControllerContext controllerContext)
   於 System.Web.Mvc.ValueProviderFactoryCollection.GetValueProvider(ControllerContext controllerContext)
   於 System.Web.Mvc.ControllerBase.get_ValueProvider()
   於 System.Web.Mvc.ControllerActionInvoker.GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
   於 System.Web.Mvc.ControllerActionInvoker.GetParameterValues(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
   於 System.Web.Mvc.Async.AsyncControllerActionInvoker.<>c__DisplayClass21.<BeginInvokeAction>b__19(AsyncCallback asyncCallback, Object asyncState)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncResult`1.CallBeginDelegate(AsyncCallback callback, Object callbackState)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout)
   於 System.Web.Mvc.Async.AsyncControllerActionInvoker.BeginInvokeAction(ControllerContext controllerContext, String actionName, AsyncCallback callback, Object state)
   於 System.Web.Mvc.Controller.<BeginExecuteCore>b__1c(AsyncCallback asyncCallback, Object asyncState, ExecuteCoreState innerState)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncVoid`1.CallBeginDelegate(AsyncCallback callback, Object callbackState)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout)
   於 System.Web.Mvc.Controller.BeginExecuteCore(AsyncCallback callback, Object state)
   於 System.Web.Mvc.Controller.<BeginExecute>b__14(AsyncCallback asyncCallback, Object callbackState, Controller controller)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncVoid`1.CallBeginDelegate(AsyncCallback callback, Object callbackState)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout)
   於 System.Web.Mvc.Controller.BeginExecute(RequestContext requestContext, AsyncCallback callback, Object state)
   於 System.Web.Mvc.Controller.System.Web.Mvc.Async.IAsyncController.BeginExecute(RequestContext requestContext, AsyncCallback callback, Object state)
   於 System.Web.Mvc.MvcHandler.<BeginProcessRequest>b__4(AsyncCallback asyncCallback, Object asyncState, ProcessRequestState innerState)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncVoid`1.CallBeginDelegate(AsyncCallback callback, Object callbackState)
   於 System.Web.Mvc.Async.AsyncResultWrapper.WrappedAsyncResultBase`1.Begin(AsyncCallback callback, Object state, Int32 timeout)
   於 System.Web.Mvc.MvcHandler.BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, Object state)
   於 System.Web.Mvc.MvcHandler.BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, Object state)
   於 System.Web.Mvc.MvcHandler.System.Web.IHttpAsyncHandler.BeginProcessRequest(Htt…"

<system.web.extensions>
  <scripting>
    <webServices>
      <jsonSerialization maxJsonLength="2147483647" />
    </webServices>
  </scripting>
</system.web.extensions>
