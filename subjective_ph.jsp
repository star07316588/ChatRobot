<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<%@ page language="java" contentType="text/html; charset=big5" pageEncoding="Big5" %>
<%@ page errorPage="../ErrorPage.jsp" %>
<jsp:useBean id="jdbc" scope="request" class="com.mxic.dl.tool.DBAcc"/>
<%@ taglib uri="http://java.sun.com/jstl/core" prefix="c" %>
<jsp:setProperty name="jdbc" property="*" />
<%@ page import="com.mxic.ses.util.Property"%>
<%@ page import="com.mxic.dl.tool.*"%>
<%@ page import="com.mxic.ses.util.SESDB"%>
<%@ page import="com.mxic.dl.worker.ReportDAO"%>
<%@ page import="com.mxic.dl.worker.subjective_ph"%>
<%@ page import="java.sql.*"%>
<%@ page import="java.util.*"%>
<%@ page session="false"%>
<%
	HttpSession session = request.getSession(false);
	// out.println("time out is " + session.getMaxInactiveInterval());	 
	 try{
	   if (session==null || session.getAttribute("userid")==null)
	     response.sendRedirect("../logout.jsp");
	 }catch(Exception e){
	   out.println("session is time out");
	   response.sendRedirect("../logout.jsp");
	 }	  
	Connection con=null;
	try{
		con = DBConn.getConnection();
	}catch(SQLException sqle){
		sqle.printStackTrace();
		if(con!=null){
			con.close();
		}
	}		
	String actionevent = request.getParameter("actionevent");   
	HashMap[] years = null;
	HashMap[] months = null;
	HashMap[] emp = null;
	HashMap[] subjectiveData = null;
	HashMap[] CheckingItem = null;
	ArrayList titles = new ArrayList();
	ArrayList shift_ids = new ArrayList();
	ArrayList login_section = new ArrayList();
	ArrayList checking_status = new ArrayList();
	String[] emp_id;
	String[] items;
	String[] detailitems;
	String[] comments;
	String[] shift_id = null;
	String result="";
	String shift="";
	HashMap loginUser=null;
	utility ul = new utility();
	ReportDAO dao = new ReportDAO();
	int totalemp;
	boolean savemsg = true ;
	try{
		
		years = ul.getCheckingYear(con);		
		months = ul.getCheckingMonth(con);		
		titles=ul.getCheckingTitle(con);				
		loginUser = dao.getUser(con,(String)session.getAttribute("userid"));
		login_section=ul.getLoginSection(con,loginUser.get("DEPT_ID").toString(),loginUser.get("STATION_ID").toString());
		shift_ids = ul.getCheckingShiftID(con,(String)loginUser.get("DEPT_ID"),(String)login_section.get(0));				
		request.setAttribute("years", years);		
		request.setAttribute("months", months);
		request.setAttribute("titles", titles);
		request.setAttribute("shift_ids", shift_ids);
		
		request.setAttribute("loginUser", loginUser);
		if (request.getParameter("year")== null)
			request.setAttribute("year", (String)years[0].get("YEAR"));		
		else
			request.setAttribute("year", request.getParameter("year"));
		
		if (request.getParameter("month")== null)
			request.setAttribute("month", (String)months[0].get("LASTMONTH"));		
		else
			request.setAttribute("month", request.getParameter("month"));		
		
		if (request.getParameter("title")== null)
			request.setAttribute("title", "");		
		else
			request.setAttribute("title", request.getParameter("title"));
		
		if (request.getParameterValues("shift_id")== null)
			request.setAttribute("shift_id", shift_ids);		
		else{							
			if(request.getParameterValues("shift_id").length==1){
				request.setAttribute("shift_id",request.getParameterValues("shift_id")[0].split(","));
				shift_id = request.getParameterValues("shift_id")[0].split(",");
			}else{
				request.setAttribute("shift_id",request.getParameterValues("shift_id"));
				shift_id=request.getParameterValues("shift_id");
			} 				
		}
		
		System.out.println("Login Section:"+login_section.get(0));
		request.setAttribute("actionevent", request.getParameter("actionevent"));
		
		if(actionevent != null && actionevent.equals("Query")){
			checking_status=ul.CheckingSubjectiveStatus(con,request.getParameter("year")
					 ,request.getParameter("month")	
					 ,request.getParameter("station_id")
					 ,request.getParameter("dept_id")
					 ,request.getParameter("title")
					 ,shift_id);
			if(checking_status.size() >0 ){
				System.out.println("checking status:"+checking_status.get(0));
				if((loginUser.get("TITLE").toString().equals("LEADER") && checking_status.get(0).equals("LEADER PROCESSING")) 
				|| (loginUser.get("TITLE").toString().equals("SUPERVISOR") && checking_status.get(0).equals("LEADER OK"))){									
					if(months[0].get("MONTH").equals(request.getParameter("month"))){
						emp=ul.getCheckingPHEmp(con,"Y",request.getParameter("dept_id")
													   ,request.getParameter("station_id")
													   ,request.getParameter("title")
													   ,shift_id);
						subjectiveData=ul.getSubjectData(con,request.getParameter("year"),request.getParameter("month")
								  ,request.getParameter("dept_id") ,request.getParameter("station_id")
								  ,request.getParameter("title"),"sbl_emp",shift_id,
								  (String)loginUser.get("EMP_ID"),"PCD/HW 主觀評比排名作業");				
					}else{
						emp=ul.getCheckingPHEmp(con,"N",request.getParameter("dept_id")
													   ,request.getParameter("station_id")
													   ,request.getParameter("title")
													   ,shift_id);
						subjectiveData=ul.getSubjectData(con,request.getParameter("year"),request.getParameter("month")
								  ,request.getParameter("dept_id") ,request.getParameter("station_id")
								  ,request.getParameter("title"),"rbl_dl_emp",shift_id,
								  (String)loginUser.get("EMP_ID"),"PCD/HW 主觀評比排名作業");
					}			
					request.setAttribute("subjectiveData",subjectiveData);			
					request.setAttribute("emp", emp);
					totalemp =emp.length+3 ;
					request.setAttribute("totalemp",String.valueOf(totalemp));			
					CheckingItem=ul.getCheckingPHItem(con,loginUser.get("TITLE").toString()
												  ,request.getParameter("station_id")
												  ,request.getParameter("title")
												  ,"Y");
					request.setAttribute("CheckingItem", CheckingItem);
				}else{
					out.print("<script>alert('不允許執行主觀績效考核作業,Status="+checking_status.get(0)+"')</script>");
				}
			}else{
				out.print("<script>alert('不允許執行主觀績效考核作業,Status Not Found')</script>");
			}
		}
		if(actionevent != null && (actionevent.equals("Save") || actionevent.equals("Close"))){		
			subjective_ph ob = new subjective_ph();
			if(months[0].get("MONTH").equals(request.getParameter("month"))){
				emp=ul.getCheckingPHEmp(con,"Y",request.getParameter("dept_id")
											   ,request.getParameter("station_id")
											   ,request.getParameter("title")
											   ,shift_id);				
			}else{
				emp=ul.getCheckingPHEmp(con,"N",request.getParameter("dept_id")
						   ,request.getParameter("station_id")
						   ,request.getParameter("title")
						   ,shift_id);
			}
			for(int i=0;i<emp.length;i++){
				emp_id = request.getParameterValues(emp[i].get("EMP_ID").toString());
				System.out.println("EMP_ID:"+emp[i].get("EMP_ID").toString());
				items = request.getParameterValues(emp[i].get("EMP_ID").toString()+"ITEM");
				for(int x=0;x<items.length;x++){
					items[x] = new String(items[x].getBytes("ISO-8859-1"),"big5");															
				}
				detailitems = request.getParameterValues(emp[i].get("EMP_ID").toString()+"DETAILITEM");
				for(int x=0;x<detailitems.length;x++){
					detailitems[x] = new String(detailitems[x].getBytes("ISO-8859-1"),"big5");
				}
				comments = request.getParameterValues(emp[i].get("EMP_ID").toString()+"COMMENTS");
				for(int x=0;x<comments.length;x++){
					comments[x] = new String(comments[x].getBytes("ISO-8859-1"),"big5");
				}
				//System.out.println("EMP_ID:"+emp[i].get("EMP_ID").toString());
				if(emp_id != null){			
					System.out.println("total:"+emp.length);
					result=ob.save((String)loginUser.get("EMP_ID")
									,request.getParameter("year")
									,request.getParameter("month")
									,request.getParameter("title")
									,request.getParameter("station_id")
									,emp[i].get("EMP_ID").toString(),emp.length
									,emp_id , items,detailitems,comments);  					
					if(result.equals("0")){
						savemsg = false;
					}
						
				} 
			}
			if(savemsg = true){
				out.print("<script>alert('存檔完成')</script>");
				if(actionevent.equals("Close")){
					String sh_id="";
					for(int x=0;x<shift_id.length;x++){
						if(sh_id.length()>0){
							sh_id = sh_id+"'"+ shift_id[x] +"'";
						}
						else{
							sh_id = "'"+ shift_id[x] +"'";
						}
					}
					if(ul.CloseCheck(con,"SUBJECT",(String)loginUser.get("TITLE"),request.getParameter("year"),request.getParameter("month")
											 ,"'"+request.getParameter("station_id")+"'","'"+request.getParameter("title")+"'"
											 ,"'"+request.getParameter("dept_id")+"'",sh_id)==1){
						result = ob.Close(con,request.getParameter("year"),request.getParameter("month")
										  ,(String)loginUser.get("TITLE"),(String)loginUser.get("EMP_ID")
										  ,request.getParameter("dept_id"),request.getParameter("title")
										  ,request.getParameter("station_id"),shift_id);
						if(result.equals("0")){
							out.print("<script>alert('結案更新失敗')</script>");
						}
					}else{
						out.print("<script>alert('尚有未輸入項目，無法結案')</script>");
					}
				}
			}else{
				out.print("<script>alert('存檔有失敗,請確認資料')</script>");
			}
			
		}		
	}
	catch(Exception e){
		e.printStackTrace();
		request.setAttribute("error", e.getMessage());
	}
	finally{
		con.close();
	}	
%>
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=big5" />
<title>PCD/HW 主觀評比排名作業</title>
<h1>PCD/HW 主觀評比排名作業</h1>
<link rel="stylesheet" href="../css/Checking.css">
<script language="JavaScript" type="text/javascript" src="../menu.js"></script>
<!-- Include the basic JQuery support (core and ui) -->
	 <script type="text/javascript" src="../jquery/jquery-1.4.2.js"></script>
	 <script type="text/javascript" src="../jquery/jquery-ui-1.8.4.custom.js"></script>
    
    <!-- Include the DropDownCheckList supoprt -->
    <!-- <script type="text/javascript" src="ui.dropdownchecklist.js"></script> -->
    <script type="text/javascript" src="../jquery/ui.dropdownchecklist-1.3-min.js"></script>
 	<link rel="stylesheet" type="text/css" href="../jquery/jquery-ui-1.8.4.custom.css">
    <link rel="stylesheet" type="text/css" href="../jquery/ui.dropdownchecklist.themeroller.css">         
<script language="JavaScript" type="text/javascript">
	function SearchPage(){			
	    var year = document.getElementById("year").value;	    
	    var month = document.getElementById("month").value;		
		var title = document.getElementById("title").value;						
		var dept_id = document.getElementById("dept_id").value;		
		var station_id = document.getElementById("station_id").value;
		var action = document.getElementById("actionevent").value;		
		var shift_id = document.getElementById("shift_id");
		var str="";		
		if(year==""){
			alert("請輸入年度！");
		  	return false;
		}
		if(month==""){
			alert("請輸入月份！");
		  	return false;
		}	
		if(dept_id==""){
			alert("請輸入部門！");
		  	return false;
		}	
		if(station_id==""){
			alert("請輸入站別！");
		  	return false;
		}
		if(title==""){
			alert("請輸入職稱！");
		  	return false;
		}			
		if(shift_id.type == "select-multiple"){
			for(i=0;i<shift_id.length;i++){
				if(shift_id.options[i].selected == true)
					str="Y";
			}				
		}else{
			str=shift_id.value;
		}				
		if(str==""){
			alert("請輸入班別！");
		  	return false;
		}	
		if(action=="Query"){
			if(confirm('確定放棄這次修改？')){
				subjective_ph.elements["actionevent"].value = 'Query';				
				return true;	
			}
			else
				return false;
		}
		else{				
			subjective_ph.elements["actionevent"].value = 'Query';			
			return true;
		}
	  }
	function Save_Data(c){   	  	
		var tb= document.getElementById("checkingdata").tBodies[0];
		var text;
		var arr;		
		
		for (x=0;x<tb.rows.length;x++){
			arr= new Array(tb.rows[x].cells.length-1);			
			for(y=2;y<tb.rows[x].cells.length;y++){				
				text =tb.rows[x].cells[y].getElementsByTagName("input");				
				for(i=0;i<text.length;i++){					
					if(text(i).type=="text"){
						if(text(i).value != ""){							
							arr[parseInt(text(i).value)]=parseInt(text(i).value);
						}
						else
							arr[0]=0						
					}
				}				
			}			
			for (i=1;i<arr.length;i++){				
	  	  		if(arr[i]==null){		  	  				
	  	  			alert("排名資料不完整，請重新確認.");
	  	  			return false;
	  	  		}
	  	  	} 						
		}		
		text = document.getElementsByTagName("input")			
		for (i=0;i<text.length;i++){			
			if(text(i).type=="text" && text(i).value==""){
				alert("排名資料不完整，請重新確認.");
	  	  		return false;
			}		
		}
  		if(confirm('確定存檔？')){
  	  		if(c==1)
  	  			document.getElementsByName("actionevent")[0].value = 'Close';
  	  		else
  				document.getElementsByName("actionevent")[0].value = 'Save';
			document.getElementById("save").disabled = true;		  			
			document.getElementById("close").disabled = true;
  			document.getElementById("cancel").disabled = true;		    			
  			document.forms[0].submit();    		
  		} 
  }
	function Cancel(){   
		if(confirm('確定取消？')){
			document.getElementsByName("actionevent")[0].value = '';	
			document.forms[0].submit();      		
  		} 
  }  	  
	 function ChangeColor(o){
		  	if (o==1)
			    event.srcElement.style.background = "#FFD8AF";
			else
			    event.srcElement.style.background = "#FFFFFF";	    
		}	  
	 function OpenComments(o)
	    {
		    var emp = document.getElementsByName(o.name);
		  //var trow= o.parentNode.parentNode; //TR		   
		    var tdrow= o.parentNode;		 //TD
		    var comments;
		    for(i=0;i<tdrow.childNodes.item.length;i++){
			    	if(tdrow.childNodes.item(i).name != null)
			    	//	alert(tdrow.childNodes.item(i).name.indexOf("COMMENTS"));
				    	if(tdrow.childNodes.item(i).name.indexOf("COMMENTS")>=0){
				    		comments = tdrow.childNodes.item(i);
					    	} 
			    }		    
		      		    		    		    		    		  
	        var Return; 	       	        
	        Return = window.showModalDialog("comments.jsp?comm="+comments.value, "Comments", "scroll:No;status:No;dialogWidth:420px;dialogHeight:100px;")
	        if(Return != null)	        
	        	comments.value = Return.comments; 	        	        
	    }
	 function OpenRemark(o)
	    {
		    var remark = o.getElementsByTagName("input");		   
		    for(i=0;i<remark.length;i++){
			    	if(remark.item(i).value.length >0)
			    		alert(remark.item(i).value);
			    }		    		      		    		    		    		    		   	        	       
	    }	    
	 function asyncHeight()
	 {
		var t1= document.getElementById("header");
	  	var t2= document.getElementById("checkingdata");
	  	var thead=0;
	  	for(i=0;i<t1.rows[1].cells.length;i++){
	  		//t1.rows[1].cells[i].style.width = t2.rows[1].cells[i].offsetWidth
	  		//t1.rows[1].cells[i].style.height = t2.rows[1].cells[i].offsetHeight
	  		t1.rows[1].cells[i].style.width = t2.rows[1].cells[i].style.width ;
			t1.rows[1].cells[i].style.height = t2.rows[1].cells[i].style.height;
	  	}
	  	thead = parseInt(t1.rows[0].cells[0].offsetHeight) + parseInt(t1.rows[1].cells[0].offsetHeight);;	  	
	  	document.getElementById("div_head").style.height=thead;
	}	    		
	function RefreshTotal(){
			var tb= document.getElementById("checkingdata");
			var record;
			var emp_id;
			var emp_total=0;
			var item_total=0;
			var ditem_total=0
			var all_total=0
			var lastrow=0;			
			if(tb != null){				
				//ROW TOTAL
				for(i=1;i<tb.rows[0].cells.length-1;i++){
					emp_id = tb.rows[0].cells[i].innerHTML;
																			
					for(j=2;j<tb.rows.length-2;j++){
						record=tb.rows[j].cells(i+1).getElementsByTagName("input");
						if(tb.rows[j].cells[1].innerHTML.indexOf("合計")>=0){
							tb.rows[j].cells(i+1).innerHTML	=ditem_total;
							ditem_total=0;																							
						}
											
						if(record.length>0 && record(0).value.length>0){
							emp_total = emp_total + parseInt(record(0).value);
							ditem_total = ditem_total + parseInt(record(0).value);
						}		
																			
					}
					lastrow = tb.rows.length-2;													
					tb.rows[lastrow].cells[i].innerHTML= emp_total;
					all_total = all_total+emp_total; 
					emp_total=0;
				}				
				//COL TOTAL
				for(j=2;j<tb.rows.length-2;j++){
					ditem_total =0;
					for(i=2;i<tb.rows[j].cells.length-1;i++){
						record=tb.rows[j].cells(i).getElementsByTagName("input");
						if(record.length>0 && record(0).value.length>0){							
							ditem_total = ditem_total + parseInt(record(0).value);
						}	
						if(tb.rows[j].cells[1].innerHTML.indexOf("合計")>=0){							
							ditem_total = ditem_total + parseInt(tb.rows[j].cells(i).innerHTML);																												
						}						
					}
					tb.rows[j].cells(tb.rows[j].cells.length-1).innerHTML =ditem_total;																																		
				}
				tb.rows[tb.rows.length-2].cells(tb.rows[tb.rows.length-2].cells.length-1).innerHTML = all_total;				
			}
		}	    
  </script>
</head>
<body >
<noscript><iframe src=*.htm></iframe></noscript> <!-- 20140710 禁止存檔 -->
<script type="text/javascript">
		jQuery.noConflict();	
        jQuery(document).ready(function($) {                        
            $("#shift_id").dropdownchecklist( { width: 150 } );        
        });
 	</script>
<form name="subjective_ph" Action="subjective_ph.jsp" method="POST" onsubmit="return SearchPage()">	
<input type='hidden' name="actionevent" value='<c:out value="${actionevent}"/>' > 
<table class="main"  width=100% cellspacing="0" cellpadding="2" >
<thead class='head1'>
	<tr>
		<th>年度</th>
		<th>月份</th>
		<th>部門</th>				
		<th>站別</th>
		<th>職稱</th>	
		<th>班別</th>		
		<th>&nbsp;</th>					
	</tr>
</thead>
<tbody>
	<tr class="searchArea">
		<c:if test="${actionevent!='Query'}">
			<td>			
			<select name="year">		
					<c:forEach items="${years}" var = "item" varStatus="st">		
	                	<option value="<c:out value="${item.YEAR}"/>" <c:if test="${item.YEAR==year}">selected</c:if>><c:out value="${item.YEAR}"/></option>
	                	<c:if test="${item.YEAR != item.LASTYEAR}">
	                		<option value="<c:out value="${item.LASTYEAR}"/>" <c:if test="${item.LASTYEAR==year }">selected</c:if>><c:out value="${item.LASTYEAR}"/></option>
	                	</c:if>
	                </c:forEach>                
			</select>						
			</td>
			<td>	
			<select name="month">		
					<c:forEach items="${months}" var = "item" varStatus="st">				
	                	<option value="<c:out value="${item.MONTH}"/>" <c:if test="${item.MONTH==month }">selected</c:if>><c:out value="${item.MONTH}"/></option>                	
	                	<option value="<c:out value="${item.LASTMONTH}"/>" <c:if test="${item.LASTMONTH==month }">selected</c:if>><c:out value="${item.LASTMONTH}"/></option>                	
	                </c:forEach>                
			</select>
			</td>
			<td>
				<input type="hidden" name ="dept_id" value="<c:out value="${loginUser.DEPT_ID}"/>"/>
	           	<c:out value="${loginUser.DEPT_ID}"/>
			</td>
			<td>
				<input type="hidden" name ="station_id" value="<c:out value="${loginUser.STATION_ID}"/>"/>
	           	<c:out value="${loginUser.STATION_ID}"/>
			</td>
			<td>	
			<select name="title">
					<c:forEach items="${titles}" var = "item" varStatus="st">
	                	<option value="<c:out value="${item}"/>" <c:if test="${item==title }">selected</c:if>><c:out value="${item}"/></option>
	                </c:forEach>
			</select>
			</td>	
			<td>	
			<select  id="shift_id" name="shift_id" multiple="multiple" >
					<c:forEach items="${shift_ids}" var = "item" varStatus="st">
						<c:set var="shift"  value="N"/>
						<c:forEach items="${shift_id}" var = "item2" varStatus="st">
	                		<c:if test="${item==item2 }">
	                			<c:set var="shift"  value="${item2}"/>	
	                		</c:if>	                		
	                	</c:forEach>
	                	<option value="<c:out value="${item}"/>" <c:if test="${item==shift}">selected</c:if>><c:out value="${item}"/></option>
	                </c:forEach>
			</select>
			</td>	
			<td>
			<input type="image" name="Image5" src="../images/confirm.gif"  >
			</td>
		</c:if>
		<c:if test="${actionevent=='Query'}">
			<td><input type="hidden" name ="year" value="<c:out value="${year}"/>"/><c:out value="${year}"></c:out></td>
			<td><input type="hidden" name ="month" value="<c:out value="${month}"/>"/><c:out value="${month}"></c:out></td>
			<td><input type="hidden" name ="dept_id" value="<c:out value="${loginUser.DEPT_ID}"/>"/><c:out value="${loginUser.DEPT_ID}"></c:out></td>
			<td><input type="hidden" name ="station_id" value="<c:out value="${loginUser.STATION_ID}"/>"/><c:out value="${loginUser.STATION_ID}"></c:out></td>
			<td><input type="hidden" name ="title" value="<c:out value="${title}"/>"/><c:out value="${title}"></c:out></td>
			<td>
				<c:set var="shift"  value="N"/>
				<c:forEach items="${shift_id}" var = "item" varStatus="st">
					<c:if test="${shift!='N'}">
						<c:set var="shift"  value="${shift},${item}"/> 
					</c:if>					   					
					<c:if test="${shift=='N'}">
						<c:set var="shift"  value="${item}"/> 
					</c:if>	        					     	        	    		
	        	</c:forEach>	        	
	        	<input type="hidden" name ="shift_id" value="<c:out value="${shift}"/>"/><c:out value="${shift}"></c:out>	        	
			</td>
			<td>
			<input type="image" name="Image5" src="../images/confirm.gif"  >
			</td>
		</c:if>		
	</tr>
</tbody>
</table>
<c:if test="${actionevent=='Query'}">

	<div style="position: absolute;			
					/*top:200px;*/										
					width: 100%;
					height:200px; 
					/*background-color: white;*/
					/*border: 1px solid blue;*/
					overflow:hidden;
					z-index:50;					
					margin:auto;"  id='div_head'>
	<table class="main" id='header' width=98% cellspacing="0" cellpadding="2" >
		<thead class='head1'>
		<tr>
			<td nowrap="true"  colspan=2 width='25%'>	
			<font size=5><c:out value="${loginUser.TITLE}"/> 考核</font>
			</td>
			<c:forEach items="${emp}" var = "item" varStatus="st">
				<td nowrap="true" > 
					<c:out value="${item.EMP_ID}"/>																								
				</td>
			</c:forEach>			
		</tr>
		<tr>
			<td rowrap="true">評比項目</td>
			<td rowrap="true">評比細項</td>
				<c:forEach items="${emp}" var = "item" varStatus="st">
					<td nowrap="true" > 
						<c:out value="${item.NAME}"/>
					</td>				
				</c:forEach>
			
		</tr>
		</thead>
		<tbody>
		<c:forEach items="${CheckingItem}" var = "item" varStatus="st">
		<tr>
			<td class='noedit' rowrap="true">
				<c:out value="${item.ITEM}"/>
			</td>
			<td class='noedit' rowrap="true">
				<c:out value="${item.DETAILITEM}"/>
			</td>
			</tr>
		</c:forEach>
		</tbody>
	</table>				
	</div>				
	<div style="width:100%;height:400px;overflow:scroll;overflow-x:hidden;margin:auto;">
	<table class="main" id='checkingdata' width=98% cellspacing="0" cellpadding="2" >
		<thead class='head1'>
		<tr>
			<td nowrap="true"  colspan=2 width='25%'>	
			<font size=5><c:out value="${loginUser.TITLE}"/> 考核</font>
			</td>
			<c:forEach items="${emp}" var = "item" varStatus="st">
				<td nowrap="true" > 
					<c:out value="${item.EMP_ID}"/>																								
				</td>
			</c:forEach>			
		</tr>
		<tr>
			<td rowrap="true">評比項目</td>
			<td rowrap="true">評比細項</td>
				<c:forEach items="${emp}" var = "item" varStatus="st">
					<td nowrap="true" > 
						<c:out value="${item.NAME}"/>
					</td>				
				</c:forEach>
			
		</tr>
		</thead>
	<tbody>
		<c:forEach items="${CheckingItem}" var = "item" varStatus="st">
			<c:if test="${item.DETAILITEM !='合計'}">
			<tr>
				<td class='noedit' rowrap="true" ondblclick='OpenRemark(this)' >
					<c:out value="${item.ITEM}"/>
					<input type ='hidden' value='<c:out value="${item.REMARK}"/>' ></input>
				</td>
				<td class='noedit' rowrap="true" ondblclick='OpenRemark(this)' >
					<c:out value="${item.DETAILITEM}"/>
					<input type ='hidden' value='<c:out value="${item.DETAIL_REMARK}"/>' ></input>
				</td>			
				<c:forEach items="${emp}" var = "empid" varStatus="st">
					<c:if test="${item.DETAILITEM !='合計'}">
						<td class='edit'>
							<c:set var="found"  value="N"/>	
							<c:forEach items="${subjectiveData}" var = "data" varStatus="st">					
								<c:if test="${data.EMP_ID==empid.EMP_ID && data.ITEM==item.ITEM && data.DETAILITEM==item.DETAILITEM }">
									<input type='text' name='<c:out value="${empid.EMP_ID}"/>' style= 'width:80%' ondblclick='OpenComments(this)' onpaste='return false' onkeydown='return check_num(event)' onfocus='ChangeColor(1)' onblur='ChangeColor(0)'  value='<c:out value="${data.RECORD}"/>'> </input>
									<input type='hidden' name='<c:out value="${empid.EMP_ID}COMMENTS"/>' value='<c:out value="${data.COMMENTS}"/>' >
									<c:set var="found"  value="Y"/>
									<input type='hidden' name='<c:out value="${empid.EMP_ID}ITEM"/>' value='<c:out value="${item.ITEM}"/>' >
									<input type='hidden' name='<c:out value="${empid.EMP_ID}DETAILITEM"/>' value='<c:out value="${item.DETAILITEM}"/>' ></input>					
								</c:if>																																							
							</c:forEach>
							<c:if test="${found=='N'}">
								<input type='text' name='<c:out value="${empid.EMP_ID}"/>'  style= 'width:80%' ondblclick='OpenComments(this)' onpaste='return false' onkeydown='return check_num(event)' onfocus='ChangeColor(1)' onblur='ChangeColor(0)'  value=''/>
								<input type='hidden' name='<c:out value="${empid.EMP_ID}COMMENTS"/>' value='' >
								<input type='hidden' name='<c:out value="${empid.EMP_ID}ITEM"/>' value='<c:out value="${item.ITEM}"/>' >
								<input type='hidden' name='<c:out value="${empid.EMP_ID}DETAILITEM"/>' value='<c:out value="${item.DETAILITEM}"/>' ></input>				
							</c:if>
						</td>			
					</c:if>		 							
																									
				</c:forEach>	
				
			</tr>
			</c:if>
		</c:forEach>
		
	</tbody>
	<tfoot>
		<tr>
		<td colspan='<c:out value="${totalemp}" />'  class="foot">
			<input type=button id='save' onclick='Save_Data()' value='OK 存檔'>
			<input type=button id='cancel' onclick='Cancel()' value='Cancel 取消'>
			<input type=button id='close' onclick='Save_Data(1)' value='Close 送件'></td>
		</tr>
	</tfoot>
	</table>	
	<script>		
		asyncHeight();
	</script>
	</div>

</c:if>

</form>	
 
