@model MyPerformanceApp.ViewModels.SubjectiveViewModel

@{
    ViewData["Title"] = "主觀績效考核作業";
    // 判斷是否鎖定畫面 (若已送出或無權限)
    var isReadOnly = Model.IsSaveDisabled ? "disabled" : "";
}

@if (!string.IsNullOrEmpty(Model.Message))
{
    <div class="alert alert-info alert-dismissible fade show" role="alert">
        @Model.Message
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}

<div class="card mb-3">
    <div class="card-header bg-primary text-white">
        <h5 class="mb-0">主觀績效考核查詢</h5>
    </div>
    <div class="card-body">
        <form method="get" asp-action="Index">
            <div class="row g-3">
                <div class="col-md-2">
                    <label class="form-label">Year</label>
                    <select asp-for="Query.Year" asp-items="@(new SelectList(Model.YearOptions))" class="form-select" disabled="@Model.IsQueryDisabled"></select>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Month</label>
                    <select asp-for="Query.Month" asp-items="@(new SelectList(Model.MonthOptions))" class="form-select" disabled="@Model.IsQueryDisabled"></select>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Dept</label>
                    <select asp-for="Query.DeptId" asp-items="@(new SelectList(Model.DeptOptions))" class="form-select" disabled="@Model.IsQueryDisabled"></select>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Title</label>
                    <select asp-for="Query.Title" asp-items="@(new SelectList(Model.TitleOptions))" class="form-select" disabled="@Model.IsQueryDisabled"></select>
                </div>
                
                <div class="col-md-2">
                    <label class="form-label">Shift</label>
                    <select asp-for="Query.ShiftIds" asp-items="@(new SelectList(Model.ShiftOptions))" class="form-select" multiple disabled="@Model.IsQueryDisabled"></select>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Station</label>
                    <select asp-for="Query.StationIds" asp-items="@(new SelectList(Model.StationOptions))" class="form-select" multiple disabled="@Model.IsQueryDisabled"></select>
                </div>

                <div class="col-md-3">
                    <label class="form-label">Item</label>
                    <select asp-for="Query.Item" asp-items="@(new SelectList(Model.ItemOptions))" class="form-select" id="ddlItem" disabled="@Model.IsQueryDisabled"></select>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Detail Item</label>
                    <select asp-for="Query.DetailItem" asp-items="@(new SelectList(Model.DetailItemOptions))" class="form-select" disabled="@Model.IsQueryDisabled"></select>
                </div>

                <div class="col-md-12 text-end mt-3">
                    @if (Model.IsQueryDisabled)
                    {
                        <a href="@Url.Action("Index")" class="btn btn-secondary">Reset 重設條件</a>
                    }
                    else
                    {
                        <button type="submit" name="action" value="Query" class="btn btn-primary">Query 查詢</button>
                    }
                </div>
            </div>
        </form>
    </div>
</div>

@if (Model.DataGrid != null && Model.DataGrid.Any())
{
    <div class="alert alert-secondary">
        <div class="row">
            <div class="col-md-4">
                <strong>權重 (Weighting):</strong> @(Model.CurrentConfig?.Weighting)%
            </div>
            <div class="col-md-4">
                <strong>分數範圍:</strong> @(Model.CurrentConfig?.LowerBound) ~ @(Model.CurrentConfig?.UpperBound)
            </div>
            <div class="col-md-4 text-end">
                <strong>考核狀態:</strong> <span class="badge bg-warning text-dark">@Model.HeadTitle</span>
            </div>
        </div>
        @if (!string.IsNullOrEmpty(Model.CurrentConfig?.ItemRemark))
        {
            <div class="mt-2 text-muted small">
                備註: @Model.CurrentConfig.ItemRemark / @Model.CurrentConfig.DetailItemRemark
            </div>
        }
    </div>

    <form method="post" asp-action="Save" id="gradeForm">
        <input type="hidden" asp-for="Query.Year" />
        <input type="hidden" asp-for="Query.Month" />
        <input type="hidden" asp-for="Query.DeptId" />
        <input type="hidden" asp-for="Query.Title" />
        <input type="hidden" asp-for="Query.Item" />
        <input type="hidden" asp-for="Query.DetailItem" />
        <input type="hidden" id="maxScore" value="@Model.CurrentConfig?.UpperBound" />
        <input type="hidden" id="minScore" value="@Model.CurrentConfig?.LowerBound" />
        <input type="hidden" id="totalCount" value="@Model.DataGrid.Count" />

        <div class="table-responsive">
            <table class="table table-striped table-hover table-bordered" id="dataTable">
                <thead class="table-light">
                    <tr>
                        <th scope="col">Dept</th>
                        <th scope="col">Station</th>
                        <th scope="col">Shift</th>
                        <th scope="col">Emp ID</th>
                        <th scope="col">Name</th>
                        <th scope="col">Position</th>
                        <th scope="col">Title</th>
                        <th scope="col" style="width: 100px;">Rank (排名)</th>
                        <th scope="col" style="width: 100px;">Score (分數)</th>
                        <th scope="col">Comments (評語)</th>
                    </tr>
                </thead>
                <tbody>
                    @for (int i = 0; i < Model.DataGrid.Count; i++)
                    {
                        <tr>
                            <td>
                                @Model.DataGrid[i].DeptId
                                <input type="hidden" asp-for="DataGrid[i].RowId" />
                                <input type="hidden" asp-for="DataGrid[i].EmpId" />
                                <input type="hidden" asp-for="DataGrid[i].DeptId" />
                            </td>
                            <td>@Model.DataGrid[i].StationId</td>
                            <td>@Model.DataGrid[i].ShiftId</td>
                            <td>@Model.DataGrid[i].EmpId</td>
                            <td>@Model.DataGrid[i].Name</td>
                            <td>@Model.DataGrid[i].PositionGroup</td>
                            <td>@Model.DataGrid[i].Title</td>
                            
                            <td>
                                <input type="number" class="form-control rank-input" 
                                       asp-for="DataGrid[i].Record" 
                                       data-index="@i"
                                       onchange="calculateScore(this)" 
                                       disabled="@Model.IsSaveDisabled" />
                            </td>
                            
                            <td>
                                <input type="number" step="0.01" class="form-control score-input" 
                                       asp-for="DataGrid[i].Score" 
                                       id="score_@i"
                                       readonly /> </td>
                            
                            <td>
                                <input type="text" class="form-control" 
                                       asp-for="DataGrid[i].Comments" 
                                       disabled="@Model.IsSaveDisabled" />
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>

        <div class="d-flex justify-content-center gap-3 mt-4 mb-5">
            @if (!Model.IsSaveDisabled)
            {
                <button type="submit" name="saveAction" value="Save" class="btn btn-success px-4">OK 存檔</button>
                <button type="submit" name="saveAction" value="Close" class="btn btn-danger px-4" onclick="return confirm('確定要送件結案嗎？結案後無法再修改。');">Close 送件</button>
            }
            <a href="@Url.Action("Index")" class="btn btn-secondary px-4">Cancel 取消</a>
        </div>
    </form>
}

@section Scripts {
    <script>
        // 前端即時運算邏輯 (對應 JSP 中的 JavaScript)
        function calculateScore(inputObj) {
            var rank = parseInt(inputObj.value);
            var index = inputObj.getAttribute('data-index');
            var scoreInput = document.getElementById('score_' + index);

            // 取得設定值
            var max = parseFloat(document.getElementById('maxScore').value);
            var min = parseFloat(document.getElementById('minScore').value);
            var total = parseInt(document.getElementById('totalCount').value);

            if (isNaN(rank) || rank <= 0) {
                scoreInput.value = "";
                return;
            }

            // 驗證排名是否超過總人數
            if (rank > total) {
                alert("排名不可大於總人數: " + total);
                inputObj.value = "";
                scoreInput.value = "";
                return;
            }

            // C# 的計算邏輯在前端的鏡像 (方便使用者即時預覽)
            // 公式: Max - ((Rank-1) * ((Max-Min) / (Total-1)))
            if (total > 1) {
                var step = (max - min) / (total - 1);
                var score = max - ((rank - 1) * step);
                // 四雪五入至小數點第二位
                scoreInput.value = score.toFixed(2);
            } else {
                scoreInput.value = max.toFixed(2);
            }
        }
        
        // 防止 Enter 鍵意外送出表單
        document.addEventListener('keypress', function (e) {
            if (e.keyCode === 13 || e.which === 13) {
                e.preventDefault();
                return false;
            }
        });
    </script>
}
