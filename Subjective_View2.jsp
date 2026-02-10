<table class="main"  width=100% cellspacing="0" cellpadding="2" >
	<tr>
	<td class='head2' rowspan=2 width='25%'>
	<input type=hidden id='headtitle' name='headtitle' value='<%=headtitle %>'>
	<font size=5><%=headtitle %> 考核</font>
	</td>
	<td class='head2' width='8%'>項目</td>
	<td class='noedit' width='18%'><%=item %></td>
	<td class='remark' colspan=3 align='left' ><input type=button value='說明' onclick="display_remark('item_remark')">
	<div id='item_remark' style="display:none">
	<%
		String s_id="";
		if(station_id.length()>0){	
			s_id=" ( ";
			String[] StrArrayid=station_id.split(",");
			  for(int i=0;i<StrArrayid.length;i++)
			  {				 
				  s_id=s_id + " '"+StrArrayid[i]+"' ,";
			  }
			  
			  s_id = s_id.substring(0,s_id.length()-1)+" ) ";				
		}
		else
			s_id ="('')";
	
		sSQL="select a.weighting,replace(replace(a.remark,'''','&#39;'),'\"','&quot;') as remark,a.upperbound,a.lowerbound from Rbl_DL_item a where station_id in "+s_id+" and title='"+title+"' and item='"+item+"' ";
		//System.out.println(sSQL);
		rs = jdbc.queryData(sSQL,con);	
		if(rs.next()){
			if(rs.getString("remark")!= null)
				out.print(rs.getString("remark"));
			else
				out.print("&nbsp;");				
			if(rs.getString("weighting") != null)
				out.print("<input type='hidden' id='weighting' name='weighting' value='"+rs.getString("weighting")+"'>");				
			else
				out.print("<input type='hidden' id='weighting' name='weighting' value='0'>");				
			
			if(rs.getString("upperbound") != null)
				out.print("<input type='hidden' id='upperbound' name='upperbound' value='"+rs.getString("upperbound")+"'>");				
			else
				out.print("<input type='hidden' id='upperbound' name='upperbound' value='0'>");				
			if(rs.getString("lowerbound") != null)
				out.print("<input type='hidden' id='lowerbound' name='lowerbound' value='"+rs.getString("lowerbound")+"'>");				
			else
				out.print("<input type='hidden' id='lowerbound' name='lowerbound' value='0'>");				
			
		}	
		jdbc.close();				
	%>
	</div>
	</td>

	</tr>
	<tr>
	<td class='head2' >細項</td>
	<td class='noedit' ><%= detailitem %></td>
	<td class='remark' colspan=3><input type=button value='說明' onclick="display_remark('detailitem_remark')" >
	<div id='detailitem_remark' style="display:none">
	<%
		sSQL="select replace(replace(a.remark,'''','&#39;'),'\"','&quot;') as remark from Rbl_DL_detailitem a where station_id in "+s_id+" and title='"+title+"' and item='"+item+"' and detailitem='"+detailitem+"' ";
		//System.out.println(sSQL);
		rs = jdbc.queryData(sSQL,con);	
		if(rs.next()){
			//System.out.println("remark:"+rs.getString("remark"));
			if(rs.getString("remark")!= null)
				out.print(rs.getString("remark"));
			else
				out.print("&nbsp;");	
		}
		jdbc.close();		
	%>
	</div>	
	</td>
	
	</tr>

</table>
