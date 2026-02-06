這一段是撈取員工資料的sql
sSQL="select distinct a.dept_id,a.station_id,a.shift_id,a.emp_id,a.name, a.position_group,a.title ";
			sSQL+=" from rbl_dl_emp a ";
			sSQL+=" where.. //省略

這一段是撈取weighting, upperbound, lowerbound的sql
		sSQL="select a.weighting,replace(replace(a.remark,'''','&#39;'),'\"','&quot;') as remark,a.upperbound,a.lowerbound from Rbl_DL_item 
				a where station_id in "+s_id+" and title='"+title+"' and item='"+item+"' ";

我應該如何將這兩筆查詢出的結果塞在同一組model當中？

public SubjectiveResultVM GetSubjectiveData(SubjectiveQueryVM query)
{
    var result = new SubjectiveResultVM();

    // --- 1. 執行第一段 SQL (撈取 Config) ---
    // sSQL="select a.weighting... from Rbl_DL_item ..."
    string sqlConfig = @"
        SELECT weighting, replace(replace(remark,'''','&#39;'),'""','&quot;') as ItemRemark, 
               upperbound, lowerbound 
        FROM Rbl_DL_item 
        WHERE station_id = @StationId AND title = @Title AND item = @Item";
    
    // 假設您使用 Dapper 或 ADO.NET
    // result.Config = _repo.QuerySingle<SubjectiveConfig>(sqlConfig, new { ... });
    // 範例：
    result.Config = new SubjectiveConfig 
    { 
        Weighting = 20, 
        UpperBound = 90, 
        LowerBound = 80, 
        ItemRemark = "測試備註" 
    };

    // --- 2. 執行第二段 SQL (撈取員工清單) ---
    // sSQL="select distinct a.dept_id... from rbl_dl_emp a ..."
    // 這裡建議要把 Rbl_DL_Performance_subject Join 進來，才能同時取得已輸入的分數
    string sqlData = @"
        SELECT distinct a.dept_id, a.station_id, a.shift_id, a.emp_id, a.name, 
               a.position_group, a.title,
               b.record, b.score, b.comments, b.rowid
        FROM rbl_dl_emp a
        LEFT JOIN Rbl_DL_Performance_subject b 
               ON a.emp_id = b.emp_id 
              AND b.year = @Year AND b.month = @Month AND b.item = @Item
        WHERE ... (您的省略條件) ...
    ";

    // result.Data = _repo.Query<EmployeeGradeRow>(sqlData, new { ... }).ToList();
    // 範例：
    result.Data = new List<EmployeeGradeRow>(); // 放入 SQL 查詢結果

    return result;
}

[HttpPost]
public ActionResult QueryTable(SubjectiveQueryVM vm)
{
    // 取得複合模型
    SubjectiveResultVM result = _service.GetSubjectiveData(vm);

    // 這裡回傳的 JSON 結構會變成：
    // {
    //    "Config": { "Weighting": 20, "UpperBound": 90 ... },
    //    "Data": [ { "Emp_Id": "123", "Name": "Kevin" ... }, ... ]
    // }
    return Json(result);
}

success: function (response) {
    // response 就是那個大盒子

    // 1. 處理 Config (塞入上方資訊區與隱藏欄位)
    if (response.Config) {
        $('#lblWeighting').text(response.Config.Weighting);
        $('#lblRemark').text(response.Config.ItemRemark);
        
        // 寫入 hidden input 供前端計算公式使用
        $('#hidUpperBound').val(response.Config.UpperBound);
        $('#hidLowerBound').val(response.Config.LowerBound);
        $('#hidWeighting').val(response.Config.Weighting);
    }

    // 2. 處理 Data (塞入 jqGrid)
    // 注意：這裡要取 response.Data 才是 List
    var gridData = response.Data; 
    
    $('#hidTotalCount').val(gridData.length); // 紀錄總筆數

    $resultTable.jqGrid('clearGridData').jqGrid('setGridParam', {
        data: gridData
    }).trigger('reloadGrid');
}
