@model MesTAManagementSystem_New.ViewModels.DLOperation.Checking.SubjectiveVM
@{
    ViewBag.Title = "Subjective(主觀評比排名作業)";
    Layout = "~/Views/Shared/_LayoutMainWithBasic.cshtml";
}
@section styles
{
}

@section scripts
{
    @Scripts.Render("~/bundles/jquery-ui")
    @Scripts.Render("~/bundles/jqgrid")
    @Scripts.Render("~/bundles/jquery-blockui")
    <script type="text/javascript">
        //===================================================================================
        //  宣告區塊
        //===================================================================================
        var $resultTable = $('#result-table');
        var $resultTablePager = $('#result-table-pager');
        var $searchForm = $('#search-form');
        var $submitButton = $('#submit-button');
        var $searchDateTime = $('#BeginDate,#EndDate');
        var $calendarIcon = $('.form-control-feedback');

        $(document).ready(function () {
            //===================================================================================
            //  表格設定
            //===================================================================================
            $resultTable.jqGrid({
                datatype: 'local',
                autowidth: true,
                shrinkToFit: false,
                height: $('div.main').height() - 50,
                rownumbers: true,
                viewrecords: true,
                rowNum: 2000,
                rownumWidth: 35,
                rowList: [500, 1000, 2000],
                pager: $resultTablePager,
                colNames: ['@Html.DisplayNameFor(r => r.Dept_Id)',
                           '@Html.DisplayNameFor(r => r.Station_Id)',
                           '@Html.DisplayNameFor(r => r.Shift_Id)',
                           '@Html.DisplayNameFor(r => r.Emp_Id)',
                           '@Html.DisplayNameFor(r => r.Name)',
                           '@Html.DisplayNameFor(r => r.Position_Group)',
                           '@Html.DisplayNameFor(r => r.Title)',
                           '@Html.DisplayNameFor(r => r.Rankinga)',
                           '@Html.DisplayNameFor(r => r.Comments)',
                ],
                colModel: [
                    { name: 'Dept_Id', index: 'Dept_Id', align: "left", editable: false, width: '70' },
                    { name: 'Station_Id', index: 'Station_Id', align: "left", editable: false, width: '70' },
                    { name: 'Shift_Id', index: 'Shift_Id', align: "left", editable: false, width: '70' },
                    { name: 'Emp_Id', index: 'Emp_Id', align: "left", editable: false, width: '70' },
                    { name: 'Name', index: 'Name', align: "left", editable: false, width: '70' },
                    { name: 'Position_Group', index: 'Position_Group', align: "left", editable: false, width: '70' },
                    { name: 'Title', index: 'Title', align: "left", editable: false, width: '120' },
                    { name: 'Rankinga', index: 'Rankinga', align: "left", editable: false, width: '70' },
                    { name: 'Comments', index: 'Comments', align: "left", editable: false, width: '120' }
                ]
            });
            $resultTable.jqGrid('setFrozenColumns');
            $('.frozen-div.ui-state-default.ui-jqgrid-hdiv').css({ 'height': '23px' }); //Frozen  Header預設22px, 有時字會被推擠

            //===================================================================================
            //  綁定datetimepicker
            //===================================================================================
            $searchDateTime.datetimepicker({
                showTimepicker: false,
                changeMonth: true,
                changeYear: true,
                dateFormat: 'yy-mm-dd'
            });
            $calendarIcon.css('z-index', $searchDateTime.css("z-index")); //把icon拉上來

            //===================================================================================
            //  按鈕自訂
            //===================================================================================

            $resultTable.navGrid('#result-table-pager', {
                edit: false,
                add: false,
                del: false,
                search: false,
                refresh: false
            });
        });
        //===================================================================================
        //  表單查詢
        //===================================================================================
        $submitButton.click(function () {
            var vm = {
                Year: $("#Year").val(),
                Month: $("#Month").val(),
                Dept_Id: $("#Dept_Id").val(),
                Title: $("#Title").val(),
                Shift_Id: $("#Shift_Id").val(),
                Station_Id: $("#Station_Id").val(),
                Item: $("#Item").val(),
                DetailItem: $("#DetailItem").val(),
            };

            $.ajax({
                url: '@Url.Action("QueryTable")',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(vm),
                beforeSend: function () {
                    $.blockUI({ css: { cursor: 'wait'} });
                },
                statusCode: {
                    401: function () {
                        alert('登入期限已過，即將返回首頁');
                        window.location.replace('@Url.Action("Index", "Home")');
                    }
                },
                success: function (data) {
                    setTimeout(function () {
                        $resultTable.jqGrid('clearGridData').jqGrid('setGridParam', {
                            datatype: 'local',
                            data: data
                        }).trigger('reloadGrid');
                    }, 200);
                },
                complete: function () {
                    $.unblockUI();
                }
            });
        });
    </script>
}
@section left_sidebar
{
    <form id="search-form" action="#">
        <table width="100%">
            <thead>
                <tr>
                    <th colspan="2">
                        <i class="glyphicon glyphicon-search"></i>Find results with...
                    </th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.Year)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.DropDownListFor(model => model.Year, Model.YearList, new { @class = "form-control input-xs" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.Month)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.DropDownListFor(model => model.Month, Model.MonthList, new { @class = "form-control input-xs" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.Dept_Id)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.TextBoxFor(model => model.Dept_Id, new { @class = "form-control input-xs", @readonly = "readonly" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.Title)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.DropDownListFor(model => model.Title, Model.TitleList, new { @class = "form-control input-xs" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.Shift_Id)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.TextBoxFor(model => model.Shift_Id, new { @class = "form-control input-xs", @readonly = "readonly" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.Station_Id)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.TextBoxFor(model => model.Station_Id, new { @class = "form-control input-xs", @readonly = "readonly" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.Item)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.DropDownListFor(model => model.Item, Model.ItemList, new { @class = "form-control input-xs" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        @Html.DisplayNameFor(m => m.DetailItem)：
                    </td>
                    <td>
                        <div class="form-group margin0">
                            @Html.DropDownListFor(model => model.DetailItem, Model.DetailItemList, new { @class = "form-control input-xs" })
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                    </td>
                    <td>
                        <button id="submit-button" type="button" class="btn btn-xs btn-primary">
                            Search
                        </button>
                        <button id="reset-button" type="reset" class="btn btn-xs btn-default">
                            Reset
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>
    </form>
}
<table id="result-table">
</table>
<div id="result-table-pager">
</div>


<input type="hidden" id="hidWeighting" />
<input type="hidden" id="hidUpperBound" />
<input type="hidden" id="hidLowerBound" />
<input type="hidden" id="hidTotalCount" />

<div class="alert alert-info" style="margin-bottom: 5px; padding: 10px;">
    <div class="row">
        <div class="col-md-3">
            <strong>權重：</strong><span id="lblWeighting">--</span>%
        </div>
        <div class="col-md-3">
            <strong>分數範圍：</strong><span id="lblRange">-- ~ --</span>
        </div>
        <div class="col-md-6">
            <strong>備註：</strong><span id="lblRemark">--</span>
        </div>
    </div>
</div>

<div style="margin-bottom: 5px; text-align: right;">
    <button id="btn-calc" type="button" class="btn btn-info btn-sm">
        <i class="glyphicon glyphicon-refresh"></i> 重新計算分數
    </button>
    <button id="btn-save" type="button" class="btn btn-success btn-sm">
        <i class="glyphicon glyphicon-floppy-disk"></i> 暫存 (Save)
    </button>
    <button id="btn-close" type="button" class="btn btn-danger btn-sm">
        <i class="glyphicon glyphicon-send"></i> 送件結案 (Close)
    </button>
</div>


@section scripts
{
    @Scripts.Render("~/bundles/jquery-ui")
    @Scripts.Render("~/bundles/jqgrid")
    @Scripts.Render("~/bundles/jquery-blockui")
    <script type="text/javascript">
        //===================================================================================
        //  宣告區塊
        //===================================================================================
        var $resultTable = $('#result-table');
        var $resultTablePager = $('#result-table-pager');
        var $searchForm = $('#search-form');
        var $submitButton = $('#submit-button');
        var $searchDateTime = $('#BeginDate,#EndDate');
        
        // 按鈕
        var $btnSave = $('#btn-save');
        var $btnClose = $('#btn-close');
        var $btnCalc = $('#btn-calc');

        $(document).ready(function () {
            //===================================================================================
            //  表格設定 (jqGrid)
            //===================================================================================
            $resultTable.jqGrid({
                datatype: 'local',
                autowidth: true,
                shrinkToFit: false,
                height: $('div.main').height() - 180, // 扣除上方資訊區塊高度
                rownumbers: true,
                viewrecords: true,
                rowNum: 2000, // 考評通常一次全部顯示，不分頁
                rowList: [500, 1000, 2000],
                pager: $resultTablePager,
                cellEdit: true,           // 啟用儲存格編輯
                cellsubmit: 'clientArray',// 編輯後只存在前端記憶體，不直接 Post
                colNames: [
                           'RowId', // 隱藏的主鍵
                           '@Html.DisplayNameFor(r => r.Dept_Id)',
                           '@Html.DisplayNameFor(r => r.Station_Id)',
                           '@Html.DisplayNameFor(r => r.Shift_Id)',
                           '@Html.DisplayNameFor(r => r.Emp_Id)',
                           '@Html.DisplayNameFor(r => r.Name)',
                           '@Html.DisplayNameFor(r => r.Position_Group)',
                           '@Html.DisplayNameFor(r => r.Title)',
                           '排名 (Record)',  // 可編輯
                           '分數 (Score)',   // 自動計算
                           '評語 (Comments)' // 可編輯
                ],
                colModel: [
                    { name: 'RowId', index: 'RowId', hidden: true, key: true }, // 確保有 Key
                    { name: 'Dept_Id', index: 'Dept_Id', width: '60', sortable: true },
                    { name: 'Station_Id', index: 'Station_Id', width: '60', sortable: true },
                    { name: 'Shift_Id', index: 'Shift_Id', width: '50', sortable: true },
                    { name: 'Emp_Id', index: 'Emp_Id', width: '70', sortable: true },
                    { name: 'Name', index: 'Name', width: '70', sortable: true },
                    { name: 'Position_Group', index: 'Position_Group', width: '70', sortable: false },
                    { name: 'Title', index: 'Title', width: '100', sortable: true },
                    
                    // === 重點修改區 ===
                    { 
                        name: 'Record', index: 'Record', width: '80', align: 'center',
                        editable: true, 
                        editrules: { number: true, minValue: 1 },
                        formatter: 'integer',
                        classes: 'bg-warning' // 加個底色提示可編輯
                    },
                    { 
                        name: 'Score', index: 'Score', width: '80', align: 'right',
                        editable: false, // 分數由公式計算，不可手動改
                        formatter: 'number', formatoptions: { decimalPlaces: 2 }
                    },
                    { 
                        name: 'Comments', index: 'Comments', width: '200', 
                        editable: true,
                        edittype: 'textarea',
                        editoptions: { rows: "1" },
                        classes: 'bg-warning'
                    }
                ],
                afterSaveCell: function (rowid, cellname, value, iRow, iCol) {
                    // 當「排名」欄位被修改後，自動觸發計算
                    if (cellname === 'Record') {
                        calculateRowScore(rowid, value);
                    }
                }
            });
            
            // Frozen Columns 設定
            $resultTable.jqGrid('setFrozenColumns');
            
            //===================================================================================
            //  查詢按鈕點擊事件
            //===================================================================================
            $submitButton.click(function () {
                var vm = {
                    Year: $("#Year").val(),
                    Month: $("#Month").val(),
                    Dept_Id: $("#Dept_Id").val(),
                    Title: $("#Title").val(),
                    Shift_Id: $("#Shift_Id").val(),
                    Station_Id: $("#Station_Id").val(),
                    Item: $("#Item").val(),
                    DetailItem: $("#DetailItem").val(),
                };

                $.ajax({
                    url: '@Url.Action("QueryTable")', // 請確認 Controller 有此 Action
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(vm),
                    beforeSend: function () {
                        $.blockUI({ message: '查詢中...' });
                    },
                    success: function (response) {
                        // 假設後端回傳格式為 { Data: [...], Config: { Upper: 100, Lower: 60, Weight: 20... } }
                        // 如果只回傳 List，則 Config 需要另外處理
                        
                        // 1. 更新上方資訊區 (假設 response 包含 Config)
                        if (response.Config) {
                            $('#lblWeighting').text(response.Config.Weighting);
                            $('#lblRange').text(response.Config.LowerBound + ' ~ ' + response.Config.UpperBound);
                            $('#lblRemark').text(response.Config.ItemRemark);
                            
                            // 寫入隱藏欄位供計算用
                            $('#hidUpperBound').val(response.Config.UpperBound);
                            $('#hidLowerBound').val(response.Config.LowerBound);
                            $('#hidWeighting').val(response.Config.Weighting);
                        }

                        // 2. 載入 Grid 資料
                        // 注意：這裡假設 response.Data 是資料 List
                        var gridData = response.Data || response; 
                        
                        $('#hidTotalCount').val(gridData.length); // 紀錄總筆數供公式分母使用

                        $resultTable.jqGrid('clearGridData').jqGrid('setGridParam', {
                            data: gridData
                        }).trigger('reloadGrid');
                    },
                    complete: function () {
                        $.unblockUI();
                    }
                });
            });

            //===================================================================================
            //  存檔與送件按鈕
            //===================================================================================
            $btnSave.click(function() { saveData(false); });
            $btnClose.click(function() { 
                if(confirm('確定要送件結案嗎？送出後將無法修改。')) {
                    saveData(true); 
                }
            });
            
            // 手動重新計算按鈕 (防止有人貼上資料沒觸發 event)
            $btnCalc.click(function(){
                var ids = $resultTable.jqGrid('getDataIDs');
                for(var i=0; i < ids.length; i++){
                    var rowId = ids[i];
                    var rank = $resultTable.jqGrid('getCell', rowId, 'Record');
                    if(rank) calculateRowScore(rowId, rank);
                }
            });

            //===================================================================================
            //  下拉選單連動 (範例)
            //===================================================================================
            $('#Dept_Id').change(function() {
                // 這裡呼叫 AJAX 取得新的 Station / Shift List 並更新 Dropdown
                // updateDropdown('GetStation', $(this).val(), '#Station_Id');
            });
        });

        //===================================================================================
        //  核心邏輯：計算分數
        //===================================================================================
        function calculateRowScore(rowId, rankVal) {
            var rank = parseInt(rankVal);
            var max = parseFloat($('#hidUpperBound').val());
            var min = parseFloat($('#hidLowerBound').val());
            var total = parseInt($('#hidTotalCount').val());

            if (isNaN(rank) || isNaN(max) || isNaN(min) || isNaN(total)) return;

            // 驗證排名
            if (rank > total) {
                alert('排名不可大於總人數 (' + total + ')');
                $resultTable.jqGrid('setCell', rowId, 'Record', null); // 清空
                $resultTable.jqGrid('setCell', rowId, 'Score', null);
                return;
            }

            // 公式: Score = Max - ((Rank - 1) * ((Max - Min) / (Total - 1)))
            var score = 0;
            if (total > 1) {
                var step = (max - min) / (total - 1);
                score = max - ((rank - 1) * step);
            } else {
                score = max; // 只有一人時得最高分
            }

            // 四捨五入到小數點兩位
            score = Math.round(score * 100) / 100;

            // 更新 Grid
            $resultTable.jqGrid('setCell', rowId, 'Score', score);
        }

        //===================================================================================
        //  核心邏輯：資料存檔
        //===================================================================================
        function saveData(isClose) {
            // 1. 確保 Grid 離開編輯狀態 (將正在編輯的 Cell 寫入 local data)
            var lastEditRow = $resultTable.data('lastEditRow'); 
            // 由於使用 cellEdit，需確保焦點離開或強制存入
            // jqGrid cellEdit 模式下，saveRow 可能不適用，通常只要呼叫 getRowData 即可取得最新資料
            
            // 2. 取得所有資料
            var allData = $resultTable.jqGrid('getRowData');
            
            // 3. 準備 Post 物件
            var postData = {
                Items: allData,
                SearchCondition: {
                    Year: $("#Year").val(),
                    Month: $("#Month").val(),
                    Dept_Id: $("#Dept_Id").val(),
                    Title: $("#Title").val(),
                    Item: $("#Item").val(),
                    DetailItem: $("#DetailItem").val()
                },
                IsClose: isClose
            };

            $.ajax({
                url: '@Url.Action("SaveData")', // 請確認 Controller 有此 Action
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(postData),
                beforeSend: function () {
                    $.blockUI({ message: '資料處理中...' });
                },
                success: function (result) {
                    if (result.success) {
                        alert(isClose ? '結案成功！' : '存檔成功！');
                        // 重新查詢刷新頁面
                        $submitButton.click();
                    } else {
                        alert('失敗：' + result.message);
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
    </script>

            //===================================================================================
            //  按鈕事件綁定
            //===================================================================================
            
            // [OK 存檔] 按鈕
            $('#btn-save').click(function () {
                // 執行存檔，參數 isClose = false
                saveData(false);
            });

            // [Close 送件] 按鈕
            $('#btn-close').click(function () {
                // 跳出確認視窗，如同 JSP 的 confirm
                if (confirm('確定要送件結案嗎？送出後將無法修改。')) {
                    // 執行存檔並結案，參數 isClose = true
                    saveData(true);
                }
            });

            // [Cancel 取消] 按鈕
            $('#btn-cancel').click(function () {
                // 取消邏輯：重新載入 Grid 資料 (放棄未存檔的修改)
                // 這相當於重新按下查詢按鈕，還原到資料庫目前的狀態
                if (confirm('確定要取消變更並重新載入資料嗎？')) {
                    $submitButton.click(); 
                }
            });

            // ... (原本的 calculateRowScore 與 saveData 函式保持不變) ...

}

<div style="margin-top: 10px; margin-bottom: 20px; text-align: center;">
    <button id="btn-save" type="button" class="btn btn-primary" style="width: 100px;">
        OK 存檔
    </button>
    
    <button id="btn-cancel" type="button" class="btn btn-default" style="width: 100px; margin-left: 10px;">
        Cancel 取消
    </button>
    
    <button id="btn-close" type="button" class="btn btn-danger" style="width: 100px; margin-left: 10px;">
        Close 送件
    </button>
</div>

        function saveData(isClose) {
            // 1. 強制讓目前正在編輯的 Cell 失去焦點以寫入 Grid data
            var $grid = $resultTable;
            var editRowId = $grid.jqGrid('getGridParam', 'selrow');
            if (editRowId) {
                $grid.jqGrid('saveCell', editRowId, 'Record'); // 嘗試儲存當前焦點欄位
                $grid.jqGrid('saveCell', editRowId, 'Comments');
            }
            
            // 2. 取得所有資料
            var allData = $grid.jqGrid('getRowData');
            
            // 3. 準備傳送給後端的資料
            var postData = {
                Items: allData,
                SearchCondition: {
                    Year: $("#Year").val(),
                    Month: $("#Month").val(),
                    Dept_Id: $("#Dept_Id").val(),
                    Title: $("#Title").val(),
                    Item: $("#Item").val(),
                    DetailItem: $("#DetailItem").val()
                },
                IsClose: isClose // 傳遞是「存檔(false)」還是「送件(true)」
            };

            $.ajax({
                url: '@Url.Action("SaveData")',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(postData),
                beforeSend: function () {
                    $.blockUI({ message: '資料處理中...' });
                },
                success: function (result) {
                    if (result.success) {
                        alert(isClose ? '送件結案成功！' : '存檔成功！');
                        // 成功後重新查詢，刷新畫面
                        $submitButton.click();
                    } else {
                        alert('作業失敗：' + result.message);
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
