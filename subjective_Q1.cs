這一段是撈取員工資料的sql
sSQL="select distinct a.dept_id,a.station_id,a.shift_id,a.emp_id,a.name, a.position_group,a.title ";
			sSQL+=" from rbl_dl_emp a ";
			sSQL+=" where.. //省略

這一段是撈取weighting, upperbound, lowerbound的sql
		sSQL="select a.weighting,replace(replace(a.remark,'''','&#39;'),'\"','&quot;') as remark,a.upperbound,a.lowerbound from Rbl_DL_item 
				a where station_id in "+s_id+" and title='"+title+"' and item='"+item+"' ";

我應該如何將這兩筆查詢出的結果塞在同一組model當中？
