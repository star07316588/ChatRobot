	public HashMap[] getBudgetData(Connection con,String year,String dept_id,String userid,String func) throws Exception{
		ArrayList al = new ArrayList();		
		String sql_ins ="";
		String sql="   select a.year,a.dept_id,a.shift_id,a.title,'Budget' as type,decode(a.title,'LEADER',1,'TA',2,3) AS titleorder, " +
					"   max(decode(month,'01',budget,null)) as jan, " +
					"   max(decode(month,'02',budget,null)) as feb, " +
					"   max(decode(month,'03',budget,null)) as mar, " +
					"   max(decode(month,'04',budget,null)) as apr, " +
					"   max(decode(month,'05',budget,null)) as may, " +
					"   max(decode(month,'06',budget,null)) as jun, " +
					"   max(decode(month,'07',budget,null)) as jul, " +
					"   max(decode(month,'08',budget,null)) as aug, " +
					"   max(decode(month,'09',budget,null)) as sep, " +
					"   max(decode(month,'10',budget,null)) as oct, " +
					"   max(decode(month,'11',budget,null)) as nov, " +
					"   max(decode(month,'12',budget,null)) as dec " +				
				    "   from (select t.year,t.month,t.dept_id,t.shift_id,t.title, " +
					       "   sum(nvl(t.budget, 0)) as budget,sum(nvl(t.actual, 0)) as actual,sum(nvl(t.balance, 0)) as balance " +
					  "   from dlowner.rbl_dl_budgetcontrol t " ;
				sql_ins= sql;
				sql+= "   where t.year =? and t.dept_id=? "; 
				sql_ins+= "   where t.year ='"+year+"' and t.dept_id='"+dept_id+"' "; 
				
		String sql2=	"   group by t.year, t.month, t.dept_id, t.shift_id, t.title ) a " +
					"   group by a.year,a.dept_id,a.shift_id,a.title  " +
					"   union  " +
					"   select a.year,a.dept_id,a.shift_id,a.title,'Actual' as type,decode(a.title,'LEADER',1,'TA',2,3) AS titleorder, " +
					"   max(decode(month,'01',Actual,null)) as jan, " +
					"   max(decode(month,'02',Actual,null)) as feb, " +
					"   max(decode(month,'03',Actual,null)) as mar, " +
					"   max(decode(month,'04',Actual,null)) as apr, " +
					"   max(decode(month,'05',Actual,null)) as may, " +
					"   max(decode(month,'06',Actual,null)) as jun, " +
					"   max(decode(month,'07',Actual,null)) as jul, " +
					"   max(decode(month,'08',Actual,null)) as aug, " +
					"   max(decode(month,'09',Actual,null)) as sep, " +
					"   max(decode(month,'10',Actual,null)) as oct, " +
					"   max(decode(month,'11',Actual,null)) as nov, " +
					"   max(decode(month,'12',Actual,null)) as dec " +
					"   from (select t.year,t.month,t.dept_id,t.shift_id,t.title, " +
					       "   sum(nvl(t.budget, 0)) as budget,sum(nvl(t.actual, 0)) as actual,sum(nvl(t.balance, 0)) as balance " +
					"   from dlowner.rbl_dl_budgetcontrol t "; 
				sql_ins+=sql2;
				sql2+=  "   where t.year =? and t.dept_id=? " ;
				sql_ins+="   where t.year ='"+year+"' and t.dept_id='"+dept_id+"' " ;
				
				sql2+=	"   group by t.year, t.month, t.dept_id, t.shift_id, t.title ) a " +
						"   group by a.year,a.dept_id,a.shift_id,a.title  " +
						"   order by year,dept_id,shift_id, titleorder   ,type desc  ";
				sql_ins+="   group by t.year, t.month, t.dept_id, t.shift_id, t.title ) a " +
						"   group by a.year,a.dept_id,a.shift_id,a.title  " +
						"   order by year,dept_id,shift_id, titleorder   ,type desc  ";
		al.add(year);
		al.add(dept_id);
		al.add(year);
		al.add(dept_id);
		try{
			DBAcc da =new DBAcc(con);   
			da.insertconfidential("DL", userid, func, sql_ins);
		}
		catch(Exception e){
			e.printStackTrace();	
		}
		return SESDB.qryHashMapBySql(con,sql+sql2,(Object[])al.toArray(new Object[0]));
	}
