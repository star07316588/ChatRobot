	public String getCurrentDate(Connection con,String datepart,int dateadd ) throws Exception {
		String result="";
		ResultSet rs;
		Statement stmt = null;
		String sql ="";
		try{
		stmt = con.createStatement();
		if (dateadd != 0){
			if (datepart.equals("y"))
				sql = "select to_char(add_months(sysdate,12*"+dateadd+"),'yyyy') as year,to_char(add_months(sysdate,12*"+dateadd+"),'mm') as month,to_char(add_months(sysdate,12*"+dateadd+"),'dd') as days from dual ";					
			else if(datepart.equals("m"))
				sql = "select to_char(add_months(sysdate,"+dateadd+"),'yyyy') as year,to_char(add_months(sysdate,"+dateadd+"),'mm') as month,to_char(add_months(sysdate,"+dateadd+"),'dd') as days from dual ";
			else if(datepart.equals("ym-1"))
				sql = "select to_char(add_months(sysdate,"+dateadd+"),'yyyy') as year,to_char(add_months(sysdate,"+dateadd+"),'mm') as month,to_char(add_months(sysdate,"+dateadd+"),'dd') as days from dual ";			
			else
				sql = "select to_char(sysdate"+dateadd+",'yyyy') as year,to_char(sysdate"+dateadd+",'mm') as month,to_char(sysdate"+dateadd+",'dd') as days from dual ";
			
		} 
		else
			sql = "select to_char(sysdate,'yyyy') as year,to_char(sysdate,'mm') as month,to_char(sysdate,'dd') as days from dual ";
		stmt.executeQuery(sql);				
		rs = stmt.getResultSet();
		while(rs.next()){
			if (datepart.equals("y"))
				result = rs.getString("year");					
			else if(datepart.equals("m"))
				result = rs.getString("month");
			else if(datepart.equals("ym"))
				result = rs.getString("year") + rs.getString("month");
			else if(datepart.equals("ym-1"))
				result = rs.getString("year");
			else
				result = rs.getString("days");
		}		
		}catch(Exception e){		    	
	    	e.printStackTrace();	    	
	    }finally{
	    	if(stmt != null)	    	
	    		stmt.close();
	    }
		return result;
	}	
