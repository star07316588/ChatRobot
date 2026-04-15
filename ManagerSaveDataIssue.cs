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
