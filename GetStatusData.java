	public HashMap[] getStatusData(Connection con,String startyear,String startmonth,
									String endtyear,String endtmonth,String dept_id,String Station_id,
									String userid,String func) throws Exception{
		String sql = 
			"select a.* , decode(a.type,'OBJECT','客觀','SUBJECT','主觀') NEW_TYPE\n" +
			"  from Rbl_DL_Status a" +
			" where year between ? and ?\n" +
			" and month between ? and ?"; 
		String sql_ins = 
				"select a.* , decode(a.type,'OBJECT','客觀','SUBJECT','主觀') NEW_TYPE\n" +
				"  from Rbl_DL_Status a" +
				" where year between '"+startyear+"' and '"+endtyear+"' \n" +
				" and month between '"+startmonth+"' and '"+endtmonth+"' ";
		if (dept_id.length() > 0){
			sql =sql + " and dept_id = '"+dept_id+"' "; 
			sql_ins += " and dept_id = '"+dept_id+"' ";
		}
		if (Station_id.length() >0){
			sql =sql + " and Station_id in ("+Station_id+") ";	
			sql_ins += " and Station_id in ("+Station_id+") ";
		}
		sql =sql +" order by Year desc, Month desc, Type, Dept_ID,  Station_ID, Title ,Shift_id";
		sql_ins += " order by Year desc, Month desc, Type, Dept_ID,  Station_ID, Title ,Shift_id";
		try{
			DBAcc da =new DBAcc(con);   			
			da.insertconfidential("DL", userid, func, sql_ins);
		}
		catch(Exception e){
			e.printStackTrace();	
		}
		return  SESDB.qryHashMapBySql(con,sql,new Object[]{startyear,endtyear,startmonth,endtmonth});
	}
