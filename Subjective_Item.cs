// 在 SubjectiveResultVM.cs 或相關檔案中
public class SubjectiveConfig
{
    // 原有的
    public decimal Weighting { get; set; }
    public decimal UpperBound { get; set; }
    public decimal LowerBound { get; set; }
    public string ItemRemark { get; set; } // 對應 Rbl_DL_item 的 remark

    // *** 新增欄位以支援新的 Table ***
    public string HeadTitle { get; set; }       // 考核標題 (e.g. LEADER)
    public string ItemName { get; set; }        // 項目名稱
    public string DetailItemName { get; set; }  // 細項名稱
    public string DetailItemRemark { get; set; } // 對應 Rbl_DL_detailitem 的 remark
}

public SubjectiveConfig GetConfig(string station_id, string title, string item, string detailItem)
{
    var config = new SubjectiveConfig();
    
    // 填入基本名稱
    config.HeadTitle = title; // 或根據邏輯轉換
    config.ItemName = item;
    config.DetailItemName = detailItem;

    // 1. 查詢 Item 設定 (權重、上下限、Item備註)
    // 注意：這裡使用參數化查詢取代原本 JSP 的字串組裝
    string sqlItem = @"
        SELECT weighting, 
               replace(replace(remark,'''','&#39;'),'""','&quot;') as ItemRemark,
               upperbound, lowerbound 
        FROM Rbl_DL_item 
        WHERE station_id = :StationId AND title = :Title AND item = :Item";
    
    var itemResult = _db.QueryFirstOrDefault<SubjectiveConfig>(sqlItem, new { StationId = station_id, Title = title, Item = item });

    if (itemResult != null)
    {
        config.Weighting = itemResult.Weighting;
        config.UpperBound = itemResult.UpperBound;
        config.LowerBound = itemResult.LowerBound;
        config.ItemRemark = itemResult.ItemRemark;
    }

    // 2. 查詢 DetailItem 備註 (新增的部分)
    string sqlDetail = @"
        SELECT replace(replace(remark,'''','&#39;'),'""','&quot;') as DetailItemRemark 
        FROM Rbl_DL_detailitem 
        WHERE station_id = :StationId AND title = :Title AND item = :Item AND detailitem = :DetailItem";

    var detailResult = _db.QueryFirstOrDefault<string>(sqlDetail, new { StationId = station_id, Title = title, Item = item, DetailItem = detailItem });
    
    config.DetailItemRemark = detailResult;

    return config;
}
