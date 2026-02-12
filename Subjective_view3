        //===================================================================================
        //  核心邏輯：資料存檔
        //===================================================================================
        function saveData(isClose) {
            var $grid = $resultTable;
            
            // 1. [重要] 強制讓目前正在編輯的 Cell 寫入 jqGrid 的本地資料 (Client Array)
            // 這樣才能確保使用者輸入的最新內容被包含在 getRowData 裡
            var editRowId = $grid.jqGrid('getGridParam', 'selrow');
            var editCell = $grid.jqGrid('getGridParam', 'iCol'); // 取得目前編輯的欄位 Index
            
            if (editRowId && editCell !== undefined) {
                // 強制保存目前焦點所在的儲存格
                $grid.jqGrid('saveCell', editRowId, editCell); 
            }
            
            // 2. 取得 Grid 所有資料 (此時已包含剛剛輸入的內容)
            var gridRows = $grid.jqGrid('getRowData');
            
            // 3. 取得外部查詢條件
            var currentYear = $("#Year").val();
            var currentMonth = $("#Month").val();
            var currentTitle = $("#Title").val();
            var currentItem = $("#Item").val();
            var currentDetailItem = $("#DetailItem").val();

            // 4. 將查詢條件注入到每一筆資料列中
            var dataToSend = [];
            $.each(gridRows, function(index, row) {
                var newRow = $.extend({}, row);
                newRow.Year = currentYear;
                newRow.Month = currentMonth;
                newRow.Title = currentTitle;
                newRow.Item = currentItem;
                newRow.DetailItem = currentDetailItem;
                dataToSend.push(newRow);
            });

            // 5. 準備 Post 物件
            var postData = {
                Data: dataToSend,
                IsClose: isClose
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
                        
                        // [修改重點] 
                        // 移除或註解掉下面這一行，不要重新查詢，這樣畫面上的資料就會保留
                        // $submitButton.click(); 
                        
                        // 如果是結案 (Close)，為了安全起見，通常會建議鎖定畫面或重新導向
                        if (isClose) {
                             // 例如：鎖定按鈕避免重複送出
                             $('#btn-save, #btn-close').prop('disabled', true);
                             // 或者這時才重新查詢以顯示唯讀狀態
                             $submitButton.click(); 
                        }

                    } else {
                        alert('作業失敗: ' + (result.message || '未知錯誤'));
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
