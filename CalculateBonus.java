以下這段Java是人員獎金計算的方式
請將以下這段JAVA改寫成C# MVC .net framework 4.0.0 的語法
我認為他的架構複雜且拆成很多function，看能不能整合一下謝謝

    public static void doSingleCalculateProcess(String emp_id){
        Connection conn = null;
        try{
            conn = DBConnection.getConnection();
            DBConnection.setConnectionAutoCommit(conn,false);
            HashMap map = new HashMap();
            map.put("emp_id",emp_id);
            Emp[] employee = Emp.getEmpByCondition(conn,map);
            if(employee != null && employee.length>0){
                for(int i=0;i<employee.length;i++){
                    doSingleCalculateProcess(conn,employee[i]);
                }
            }
        }catch(Exception ex){
            System.out.println(ex);
            DBConnection.rollback(conn);
        }finally{
            DBConnection.commit(conn);
            DBConnection.close(conn);
        }
    }

    public static void doSingleCalculateProcess(Connection conn, Emp employee)throws Exception{
            //計算模式 累加法
            CalculateMode1 mode1 = new CalculateMode1();
            mode1.doSingleCalculateProcess(conn,employee);
    }

  public static Emp[] getEmpByCondition(Connection conn,HashMap where)throws Exception{
      String sql = "select emp_id, station_id, title from sbl_emp where dlt_date is null "+SQLStem.getWhereStmt(where,false);
      System.out.println(sql);
      HashMap[] rs = BonusService.getDataBySqlStr(conn,sql);
      ArrayList tmp = new ArrayList();
      if(rs!=null && rs.length>0){
          for(int i=0;i<rs.length;i++){
              Emp emp = new Emp();
              emp.generateColumnValue(rs[i]);
              emp.setLicence(Licence.getLicenceByEmpId(conn,emp.getEmp_id(),emp.getStation_id()));
              tmp.add(emp);
          }
      }
      return (Emp[]) tmp.toArray(new Emp[0]);
  }

    public static StringBuffer getWhereStmt(HashMap map,boolean whereNeed){
            StringBuffer whereBuffer = getWhereStmt(map);
            if(whereBuffer.length() > 0){
                  if(!whereNeed){
                        whereBuffer.delete(0,7);
                        whereBuffer.insert(0," and ");
                  }
            }
            return whereBuffer;
    }

    public static HashMap[] getDataBySqlStr(Connection conn, String sql) throws Exception{
              PreparedStatement ps = null;
              ResultSet rs = null;
              ArrayList tmp = new ArrayList();
              ps = conn.prepareStatement(sql);
              rs = ps.executeQuery();
              int row = 0;
              while(rs.next()){
                  tmp.add(setRStoHashMapStr(rs));
                  row++;
              }
              return (HashMap[]) tmp.toArray(new HashMap[0]);
      }

    public void generateInit(Connection conn,HashMap where)throws Exception{
            StringBuffer sql = new StringBuffer();
            sql.append("select * from sbl_bonus_data where dlt_date is null and bonus_mode = '累加法' ");
            sql.append(SQLStem.getWhereStmt(where,false));
            System.out.println(sql);
            ArrayList tmp = BonusService.getObjectArrayBySql(conn,sql.toString(),BonusData.class);
            this.setBonusDatas((BonusData[]) tmp.toArray(new BonusData[0]));
            if(this.bonusDatas!=null && this.bonusDatas.length>0){
                for(int i=0;i<this.bonusDatas.length;i++){
                    this.bonusDatas[i].generateItems(conn);
                }
            }
    }

    public void doCalculateProcess(Connection conn, Emp[] employee, BonusData data)throws Exception{
        BonusItem[] items = data.getBonusItems();
        if(items!=null && items.length>0){
            if (employee != null && employee.length > 0) {
                for (int i = 0; i < employee.length; i++) {
                    if (employee[i].getStation_id().equals(data.getStation_id()) &&
                        employee[i].getTitle().equals(data.getTitle_id())) {
                        HashMap lics = employee[i].getLicence();
                        BonusHistory bHistory = new BonusHistory();
                        bHistory.setEmp_id(employee[i].getEmp_id());
                        bHistory.setDate_month(DateUtil.getDateTime("yyyyMM"));
                        ArrayList tmp = new ArrayList();
                        int totalBonus = 0;
                        for(int j=0;j<items.length;j++){
                            BonusItem hItem = doCalculateBonus(conn,lics,items[j]);
                            tmp.add(hItem);
                            int aGrade = Integer.parseInt(hItem.getA_grade());
                            int bGrade = Integer.parseInt(hItem.getB_grade());
                            totalBonus = totalBonus + aGrade + bGrade;
                        }
                        
                        //add by Nelson start on 2008/12/09 for ReqNo:M200810044
                        //抓取必要執照資訊
                        String[] NLBonusLimit = new String[2];
                        NLBonusLimit = getNLBonusLimit(employee[i].getEmp_id(),data.getStation_id(),data.getTitle_id(),data.getBonus_mode());
                        if(Integer.parseInt(data.getBonus_limit())>totalBonus){                        	
                        }else{
                        	totalBonus = Integer.parseInt(data.getBonus_limit());
                        }
                        if(NLBonusLimit[0].equals("")||NLBonusLimit[0].equals("0")){//取得全部必要執照，則不作任何動作                       	
                        }else{//必要執照沒有全部取得時
                        	if(totalBonus>Integer.parseInt(NLBonusLimit[1])){//必要執照沒有全部得且主要執照獎金大於必要執照獎金時，則以必要執照獎金上限為主。
                        		totalBonus = Integer.parseInt(NLBonusLimit[1]);
                        	}
                        }
                        //add by Nelson start on 2008/12/09 for ReqNo:M200810044
                        
                        //mark by Nelson start on 2008/12/09 for ReqNo:M200810044
                        /*
                        if(Integer.parseInt(data.getBonus_limit())>totalBonus){
                            bHistory.setTotalBonus(String.valueOf(totalBonus));
                        }else{
                            bHistory.setTotalBonus(data.getBonus_limit());
                        }
                        */
                        //mark by Nelson end on 2008/12/09 for ReqNo:M200810044
                        
                        bHistory.setTotalBonus(String.valueOf(totalBonus));
                        bHistory.setBonusItems((BonusItem[]) tmp.toArray(new BonusItem[0]));
                        bHistory.SaveToDB(conn);
                    }
                }
            }
        }
    }


  public static HashMap getLicenceByEmpId(Connection conn,String emp_id,String station_id) throws Exception{
      StringBuffer sql = new StringBuffer();
      //sql.append("SELECT * from sbl_Licence where valid_date >= sysdate and dlt_date is null ");//LAI-MARK-20060329
      sql.append("SELECT * from sbl_Licence where valid_date >= sysdate and dlt_date is null and licence_type='CL' ");//LAI-ADD-20060329
      sql.append("and emp_id ='" + emp_id +"' ");
      sql.append("and station_id ='" + station_id +"' ");
      System.out.println(sql);
      Licence[] lc = getLicenceBySql(conn,sql.toString());
      HashMap map = new HashMap();
      if(lc!=null && lc.length>0){
          for(int i=0;i<lc.length;i++){
              map.put(lc[i].getCer_item_id(),lc[i]);
          }
      }
    return map;
  }

    public void generateItems(Connection conn)throws Exception{
        String sql = "Select * from sbl_bonus_item where data_id = '" + id + "'";
        System.out.println(sql);
        //Added by Jack on 2023/08/03 <Start>
        //----- <2> 將完整的 SQL insert into Rbl_Confidential_SysLog.
        if (this.sUserID!=null && this.sExecFunction!=null & this.sPlatform!=null){
  	  		int check_list = BonusService.insertConfidential_SysLog(this.sPlatform, this.sUserID, this.sExecFunction, sql);
        }
        //Added by Jack on 2023/08/03 <End>
        ArrayList ls = BonusService.getObjectArrayBySql(conn,sql, BonusItem.class);
        this.setBonusItems((BonusItem[]) ls.toArray(new BonusItem[0]));
        if(this.bonusItems!=null && this.bonusItems.length>0){
            for(int i=0;i<this.bonusItems.length;i++){
            	//Added by Jack on 2023/08/02 <Start>
            	this.bonusItems[i].setsUserID(this.sUserID);
            	this.bonusItems[i].setPlatform(this.sPlatform);
            	this.bonusItems[i].setExecFunction(this.sExecFunction);
            	//Added by Jack on 2023/08/02 <End>
                this.bonusItems[i].generateMainLicence(conn);
                this.bonusItems[i].generateSubLicence(conn);
            }
        }
    }

    public void generateMainLicence(Connection conn)throws Exception{
    	//Created by Jack on 2023/08/01 <b>先Insert Rbl_Confidential_SysLog, 再呼叫原本的 queryBonusData(String station, String title, String mode) <Start>
    	if(this.sUserID != null && this.sExecFunction != null && this.sPlatform != null){
            BonusLicence bean = new BonusLicence(conn,this.data_id,this.id,"sbl_bonus_mlicence", sPlatform, sUserID, sExecFunction);
            this.setMLicence(bean);
    	//Created by Jack on 2023/08/01 <b>先Insert Rbl_Confidential_SysLog, 再呼叫原本的 queryBonusData(String station, String title, String mode) <End>
    	}else{
	        BonusLicence bean = new BonusLicence(conn,this.data_id,this.id,"sbl_bonus_mlicence");
	        this.setMLicence(bean);
    	}
    }

    public void generateSubLicence(Connection conn)throws Exception{
    	//Created by Jack on 2023/08/01 <b>先Insert Rbl_Confidential_SysLog, 再呼叫原本的 queryBonusData(String station, String title, String mode) <Start>
    	if(this.sUserID != null && this.sExecFunction != null && this.sPlatform != null){
            BonusLicence bean = new BonusLicence(conn,this.data_id,this.id,"sbl_bonus_slicence", sPlatform, sUserID, sExecFunction);
            this.setSLicence(bean);
    	//Created by Jack on 2023/08/01 <b>先Insert Rbl_Confidential_SysLog, 再呼叫原本的 queryBonusData(String station, String title, String mode) <End>
    	}else{
	        BonusLicence bean = new BonusLicence(conn,this.data_id,this.id,"sbl_bonus_slicence");
	        this.setSLicence(bean);
        }
    }

    public BonusLicence(Connection conn,String data_id,String item_id,String table, String sPlatform, String sUserID, String sExecFunction) throws Exception {
        this.table=table;
        this.data_id=data_id;
        this.item_id=item_id;
        String sql = "Select cer_item_id from "+table+" where data_id = '"+data_id+"' and item_id = '"+item_id+"'";
        List rs = BonusService.getSingleColumnDataBySql(conn,sql);
        if(rs!=null && rs.size()>0){
            this.setCer_item_id((String[]) rs.toArray(new String[0]));
        }
        //Added by Jack on 2023/08/03 <Start>
        //----- <2> 將完整的 SQL insert into Rbl_Confidential_SysLog.
  		int check_list = BonusService.insertConfidential_SysLog(sPlatform, sUserID, sExecFunction, sql);
        //Added by Jack on 2023/08/03 <End>
    }


    public String[] getNLBonusLimit(String sEmpId,String sStation_Id,String sTitle_id,String sBonus_Mode){    	
    	String[] sReturn = new String[2];
    	Connection conn = null;
    	PreparedStatement pstmt = null;
    	ResultSet rs = null;
    	String sSql = null;
    	String sData_Id = "";
    	String sNLCount = "";
    	String sNLBonusLimit = "";    	
    	try{
    		conn = DBConnection.getConnection();
    		sSql = "select id,nl_bonus_limit from sbl_bonus_data "+
    			   "where station_id=? and "+
    			         "title_id=? and "+
    			         "bonus_mode=?";
    		pstmt = conn.prepareStatement(sSql);
    		pstmt.setString(1, sStation_Id);
    		pstmt.setString(2, sTitle_id);
    		pstmt.setString(3, sBonus_Mode);
    		rs = pstmt.executeQuery();
    		if(rs.next()){
    			sData_Id = rs.getString("id");
    			sNLBonusLimit = rs.getString("nl_bonus_limit");
    		}
    		rs.close();
    		pstmt.close();
    		
    		sSql = "select count(*) NLCount "+
    			   "from (select cer_item_id from sbl_bonus_nlicence sbn where sbn.data_id=?) a,"+
    					"(select * from sbl_licence sl where sl.emp_id=? and sl.station_id=? and sl.licence_type='CL' and sl.valid_date>sysdate and sl.dlt_user is null and sl.dlt_date is null) b "+
    			   "where a.cer_item_id = b.cer_item_id(+) and "+
    			   		 "b.cer_item_id is null";
    		pstmt = conn.prepareStatement(sSql);
    		pstmt.setString(1,sData_Id);
    		pstmt.setString(2,sEmpId);
    		pstmt.setString(3,sStation_Id);
    		rs = pstmt.executeQuery();
    		if(rs.next()){
    			sNLCount = rs.getString("NLCount");
    		}
    		rs.close();
    		pstmt.close();
    	}catch(SQLException sqle){
    		sqle.printStackTrace();
    		try{
	    		if(rs!=null){
	    			rs.close();
	    		}
	    		if(pstmt!=null){
	    			pstmt.close();
	    		}
	    		if(conn!=null){
	    			conn.close();
	    		}    		
    		}catch(SQLException sqle1){
    			sqle1.printStackTrace();
    		}    		
    	}catch(Exception ex){
    		ex.printStackTrace();
    	}finally{
    		try{
    			if(rs!=null){
    				rs.close();
    			}
    			if(pstmt!=null){
    				pstmt.close();
    			}
    			if(conn!=null){
    				conn.close();
    			}
    		}catch(SQLException sqle){
    			sqle.printStackTrace();
    		}
    	}
    	sReturn[0] = sNLCount;
    	sReturn[1] = sNLBonusLimit;
    	return sReturn; 
    }


    public void SaveToDB(Connection conn){
        try{
            if(this.selectCount(conn)>0){
                this.update(conn);
            }else{
                this.insert(conn);
            }
        }catch(Exception ex){
            DBConnection.rollback(conn);
        }finally{
            DBConnection.commit(conn);
        }
    }

    public void insert(Connection conn)throws Exception{
        HashMap value = new HashMap();
        this.setId(BonusService.getSEQNumberString(conn,"sbl_bonus_history_seq"));
        value.put("id", this.id);
        value.put("emp_id", this.emp_id);
        value.put("date_month", this.date_month);
        value.put("totalbonus", this.totalBonus);
        value.put("crt_date", new java.sql.Timestamp(System.currentTimeMillis()));
        SQLStem.insert(conn, this.table, value);
        BonusItem[] items = this.getBonusItems();
        if (items != null && items.length > 0) {
            for (int i = 0; i < items.length; i++) {
                items[i].setHistory_id(this.id);
                items[i].SaveToHistoryDB(conn);
            }
        }
    }

    public void update(Connection conn)throws Exception{
        this.QueryBonusHistoryId(conn,this.getEmp_id(),this.getDate_month());
        HashMap value = new HashMap();
        value.put("totalbonus", this.totalBonus);
        value.put("upt_date", new java.sql.Timestamp(System.currentTimeMillis()));
        HashMap condition = new HashMap();
        condition.put("emp_id", this.emp_id);
        condition.put("date_month", this.date_month);
        SQLStem.update(conn,this.table, value, condition);
        BonusItem[] items = this.getBonusItems();
        if (items != null && items.length > 0) {
            for (int i = 0; i < items.length; i++) {
                items[i].setHistory_id(this.id);
                items[i].SaveToHistoryDB(conn);
            }
        }
    }

    public void SaveToHistoryDB(Connection conn)throws Exception{
            HashMap value = new HashMap();
            value.put("history_id",this.history_id);
            value.put("item_id",this.id);
            SQLStem.delete(conn,this.htable,value);
            value.put("a_grade",this.a_grade);
            value.put("b_grade",this.b_grade);
            SQLStem.insert(conn,this.htable,value);
            BonusLicence mlic = this.getMLicence();
            BonusLicence slic = this.getSLicence();
            if(mlic!=null){
                mlic.setHistory_id(this.getHistory_id());
                mlic.setItem_id(this.getId());
                mlic.setTable("sbl_bonus_hmlicence");
                mlic.SaveToHistoryDB(conn);
            }
            if(slic!=null){
                slic.setHistory_id(this.getHistory_id());
                slic.setItem_id(this.getId());
                slic.setTable("sbl_bonus_hslicence");
                slic.SaveToHistoryDB(conn);
            }
    }

    public void SaveToHistoryDB(Connection conn)throws Exception{HashMap value = new HashMap();
            value.put("history_id",this.history_id);
            value.put("item_id",this.item_id);
            SQLStem.delete(conn,this.table,value);
            if(this.cer_item_id!=null){
                for (int i = 0; i < this.cer_item_id.length; i++) {
                    value.put("cer_item_id", this.cer_item_id[i]);
                    SQLStem.insert(conn, this.table, value);
                }
            }
    }
