我在oracle下以下指令查詢顯示資料為0筆，照禮截斷字串1到8結果應該會一樣
select * from Tbl_Ws_Eqform_Basic a where substr(a.lotid, 1, 8) = '5P530600A5'
但下以下指令卻有資料，原因是什麼
select * from Tbl_Ws_Eqform_Basic a where a.lotid = '5P530600'
