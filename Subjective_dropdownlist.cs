//===================================================================================
//  下拉選單連動：當 Item 改變時，更新 DetailItem
//===================================================================================
$('#Item').change(function () {
    var selectedItem = $(this).val();
    
    // 取得其他必要的關聯參數 (視您的查詢邏輯而定)
    var stationId = $('#Station_Id').val(); 
    var title = $('#Title').val();

    // 1. 先清空 DetailItem 選單，只保留預設選項
    var $detailDropdown = $('#DetailItem');
    $detailDropdown.empty();
    $detailDropdown.append($('<option></option>').val('').text('All')); // 或 '請選擇'

    // 如果沒有選 Item，就不用去後端撈了
    if (!selectedItem) {
        return;
    }

    // 2. 發送 AJAX 請求取得細項
    $.ajax({
        url: '@Url.Action("GetDetailItemList", "Subjective")', // 請確認 Controller 名稱
        type: 'POST',
        // 傳送 JSON 格式參數
        contentType: 'application/json',
        data: JSON.stringify({ 
            stationId: stationId, 
            title: title, 
            item: selectedItem 
        }),
        success: function (data) {
            // data 預期是一個字串陣列 List<string>
            if (data && data.length > 0) {
                $.each(data, function (index, value) {
                    // 建立 <option value='xxx'>xxx</option>
                    $detailDropdown.append($('<option></option>').val(value).text(value));
                });
            }
        },
        error: function (xhr, status, error) {
            console.error("無法取得細項資料: " + error);
        }
    });
});

[HttpPost]
public JsonResult GetDetailItemList(string stationId, string title, string item)
{
    // 呼叫 Service 取得資料
    List<string> list = _subjectiveService.GetDetailItems(stationId, title, item);

    // 回傳 JSON 給前端
    return Json(list);
}
