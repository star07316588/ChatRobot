
if(option.equals("add")){
	strExecType = "鑑定考試名單維護_新增";//added by Jack on 2023/08/31.
    sql=" SELECT cer_id,to_char(cer_date,'yyyymm')cer_date"+
        " FROM sbl_certificate"+
        " WHERE cer_item_id='"+cerItemId+"'";

    if(user.equals("OpSupervisor-Leader"))
        sql+=" AND to_char(cer_date,'yyyymm')=to_char(add_months(sysdate,1),'yyyymm')";
    else if(user.equals("OpTrainingGroup"))
        sql+=" AND to_char(cer_date,'yyyymm')=to_char(sysdate,'yyyymm')";
    else{
        if(monthly.equals("next_month"))
            sql+=" AND to_char(cer_date,'yyyymm')=to_char(add_months(sysdate,1),'yyyymm')";
        else
            sql+=" AND to_char(cer_date,'yyyymm')=to_char(sysdate,'yyyymm')";
    }
    jdbc.resetLogSysLog("SES", query_user, strExecType); //Added by Jack on 2023/08/31.
    rs=jdbc.queryData(sql,con);

    if(rs.next()){
        String cerId=rs.getString("cer_id");
        String cerDate=rs.getString("cer_date");

        sql=" SELECT cer_reg_no"+
            " FROM sbl_cer_reg"+
            " WHERE cer_id='"+cerId+"'"+
            " AND to_char(cer_date,'yyyymm')='"+cerDate+"'"+
            " AND emp_id='"+empId+"'"+
            " AND cer_item_id='"+cerItemId+"'";

        jdbc.resetLogSysLog("SES", query_user, strExecType); //Added by Jack on 2023/08/31.
        rs=jdbc.queryData(sql,con);
        if(rs.next()){
            message="資料已經存在!";
	        //Added by Jack on 2016/03/31 for JC201600028 <Start>
	        session.setAttribute("sMessage",message);
	        //Added by Jack on 2016/03/31 for JC201600028 <End>
        }else{
        	String sCert_Item_Id = request.getParameter("cert_item_id");
        	String sExam_Type = "";
        	String sWR="";
        	String sOP="";
        	
        	//Added by Jack on 2016/03/31 for JC201600028 <Start>
        	//3個個月內有訓練記錄才可新增..(不卡站別, 不用判斷到時分秒 .
        	//2016/05/05 Cherry UAT 後, 要求改為 sbl_training.training_date三個月內.
   		    //" and to_char(b.reg_date,'yyyymmdd') >= to_char(add_months(sysdate, -3),'yyyymmdd') ";
        	sql=" select b.* "+
        		  " from sbl_training_reg b "+
        		 " where b.training_no in "+
        		       " (select distinct a.training_no "+
        		          " from sbl_training a, sbl_training_reg b "+
        		         " where a.course_id = '"+cerItemId+"' "+
        		           " and a.training_no = b.training_no "+
        		           " and to_char(a.training_date , 'yyyymmdd') >= "+
        		           " to_char(add_months(sysdate, -3), 'yyyymmdd') "+
        		           " ) "+
        		   " and b.emp_id = '"+empId+"' "+
        		   " and b.training_flag = 'Y' ";
        		   //" and to_char(b.reg_date,'yyyymmdd') >= to_char(add_months(sysdate, -3),'yyyymmdd') ";
        	jdbc.resetLogSysLog("SES", query_user, strExecType); //Added by Jack on 2023/08/31.
        	rs=jdbc.queryData(sql,con);
            if(!rs.next()){
            	message="考試科目:"+cerItemId+", 三個月內沒有訓練紀錄，若要報名請洽訓練組! (" + empId + "," + stationId + ","+monthly+ ")" ;
            	//message="考試科目:"+cerItemId+", 三個月內沒有訓練紀錄，若要報名請洽訓練組! ";
            	//out.println("<script>alert('" + message +"');</script>");
            	session.setAttribute("sMessage",message);
            //Added by Jack on 2016/03/31 for JC201600028 <End>
            }else{
	        	sql = "SELECT exam_type FROM SBL_COURSE WHERE COURSE_ID='"+sCert_Item_Id+"'";
	        	jdbc.resetLogSysLog("SES", query_user, strExecType); //Added by Jack on 2023/08/31.
	        	ResultSet rs1 = jdbc.queryData(sql,con);
	        	
	        	if(rs1.next()){
	        		sExam_Type = rs1.getString("exam_type");
	        		if(sExam_Type==null){
	        			sExam_Type="";
	        		}
	        		if(sExam_Type.equals("WR")){
	        			sOP="NA";
	        		}else if(sExam_Type.equals("OP")){
	        			sWR="NA";
	        		}
	        	}
	        	if(rs1!=null){
		        	rs1.close();
	        	}
	            sql=" INSERT INTO sbl_cer_reg"+
	                " (cer_reg_no,cer_id,emp_id,reg_date,cer_date,crt_user,crt_date,cer_item_id,score_writing,score_oper)"+
	                " VALUES(cer_reg_seq.nextVal,"+
	                " '"+cerId+"',"+
	                " '"+empId+"',"+
	                " to_date(to_char(sysdate,'YYYY/MM/DD'),'YYYY/MM/DD'),"+
	                " to_date('"+cerDate+"','yyyymm'),"+
	                " '"+userId+"',"+
	                " sysdate,"+
	                " '"+cerItemId+"',"+
	                " '"+sWR+"',"+
	                " '"+sOP+"')";
				if(con==null||con.isClosed()){
					con = DBConnection.getConnection();
				}
				strExecType = "鑑定考試名單維護_新增";//added by Jack on 2023/08/31.
				jdbc.setLogSysLog("SES", query_user, strExecType); //Added by Jack on 2023/08/31.
	            jdbc.insertPreparation(sql,con);
          
	            flag=false;
		        //Added by Jack on 2016/03/31 for JC201600028 <Start>
		        message="資料新增完成!";
		        session.setAttribute("sMessage",message);
		        //Added by Jack on 2016/03/31 for JC201600028 <End>
            }
        }
      if(rs!=null){
    	  rs.close();
      }
      if(con!=null){
    	  con.close();
      }
    }
    else{
    	//Modified by Jack on 2016/03/31 for JC201600028 <Start>        
        //message="考試科目:"+cerItemId+",該月份不在鑑定作業清單內,新增失敗!";
        message="考試科目:"+cerItemId+",該月份不在鑑定作業清單內,新增失敗! \n" +
                "(工號:" + empId + ", 站別:" + stationId + ", 月份:"+monthly+ ")" ;
    	session.setAttribute("sMessage",message);
    	//Modified by Jack on 2016/03/31 for JC201600028 <End>        
    }
    if(flag){
        %>
        <html>
          <head>
            <script language="JavaScript">
                var examId="<%=cerItemId%>";
                alert("<%=message%>");
                history.go(-1);
            </script>;
          </head>
        </html>
        <%
    }
    else
    	if(rs!=null){
    		rs.close();
    	}
    	if(con!=null){
    		con.close();
    	}
        response.sendRedirect("sblCerRegList.jsp");
}
