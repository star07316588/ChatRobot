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
