if(option.equals("delete")){    
    String[] delRowId=request.getParameterValues("chk");
    if(delRowId!=null){
	    for(int i=0;i<delRowId.length;i++){
	    	//Added by Jack on 2012/06/14 for JC201200174 <Start>
	    	//1. 若想保留原記錄，只更新Dlt_User與Dlt_Date，評估至少有5個功能需配合修正。
  			//   ( 如：術科成績登錄、線上考試、鑑定考試名單維護、鑑定考試成績登錄、鑑定考試簽名表... )
	    	//2. 當執行刪除功能，系統將該筆資料由Sbl_Cer_Reg 搬至Sbl_Cer_Reg_Delete，
      		//   供日後澄清到期執照系統未自動報考問題。
			if(con==null || con.isClosed()){
				con = DBConnection.getConnection();
			}
	    	sql="insert into sbl_cer_reg_delete "+
		    	"(CER_REG_NO,CER_ID,EMP_ID,REG_DATE,SCORE_WRITING,SCORE_OPER,OPER_FAIL,CER_DATE, "+
		    	" GRADE,TESTER,CRT_USER,CRT_DATE,UPT_USER,UPT_DATE,DLT_USER,DLT_DATE, "+
		    	" CER_ITEM_ID,RETEST_NO,DATE_SET,WR_DATE,OP_DATE,WR_FILE,OP_FILE) "+
		    	"select CER_REG_NO,CER_ID,EMP_ID,REG_DATE,SCORE_WRITING,SCORE_OPER,OPER_FAIL,CER_DATE, "+
		    	"       GRADE,TESTER,CRT_USER,CRT_DATE,UPT_USER,UPT_DATE,'" + userId + "',sysdate, "+
		    	"       CER_ITEM_ID,RETEST_NO,DATE_SET,WR_DATE,OP_DATE,WR_FILE,OP_FILE "+
		    	"  from sbl_cer_reg a where a.cer_reg_no = '"+delRowId[i]+"' ";

	    	strExecType = "鑑定考試名單維護_Save";//added by Jack on 2023/08/31.
			jdbc.setLogSysLog("SES", query_user, strExecType); //Added by Jack on 2023/08/31.    	
	    	jdbc.insertPreparation(sql,con);
	    	//Added by Jack on 2012/06/14 for JC201200174 <End>
	    	
	        sql=" DELETE FROM sbl_cer_reg "+
	            " WHERE cer_reg_no='"+delRowId[i]+"'";
			if(con==null || con.isClosed()){
				con = DBConnection.getConnection();
			}
	    	strExecType = "鑑定考試名單維護_Delete";//added by Jack on 2023/08/31.
			jdbc.setLogSysLog("SES", query_user, strExecType); //Added by Jack on 2023/08/31.    	
	        jdbc.deletePreparation(sql,con);
	    }
    }
    if(con!=null){
    	con.close();
    }
    response.sendRedirect("sblCerRegList.jsp");
}
