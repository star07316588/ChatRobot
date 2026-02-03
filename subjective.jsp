<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<%@ page language="java" contentType="text/html; charset=big5" pageEncoding="Big5" %>
<%@ page errorPage="../ErrorPage.jsp" %>
<jsp:useBean id="jdbc" scope="request" class="com.mxic.dl.tool.DBAcc"/>
<%@ taglib uri="http://java.sun.com/jstl/core" prefix="c" %>
<jsp:setProperty name="jdbc" property="*" />
<%@ page import="com.mxic.ses.util.Property"%>
<%@ page import="com.mxic.dl.tool.*"%>
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
%>

<%
Connection con=null;
try{
	con = DBConn.getConnection();
}catch(SQLException sqle){
	sqle.printStackTrace();
	if(con!=null){
		con.close();
	}
}	
	String sSQL ="";
	String sSQL2 ="";	
	String checkProdSupervisor=""; //Add by Sam on 2014030319 for DL績效管理系統強化運用II
	int ttlcnt=0;
	if (request.getParameter("ttlcnt")!=null )
		ttlcnt=Integer.parseInt(request.getParameter("ttlcnt"));
	//System.out.println("TOTAL COUNT:"+ttlcnt);
	String year = request.getParameter("year");
	String month = request.getParameter("month");
	String station_id = request.getParameter("station_id");
	String title = request.getParameter("title");   
  
	String shift_id = request.getParameter("shift_id"); 
	String dept_id = request.getParameter("dept_id"); 
	System.out.println("shift_id:"+shift_id);
	  
  	String actionevent = request.getParameter("actionevent");   
  	String empid = (String)session.getAttribute("userid");
  	String item="";
    //System.out.println(request.getParameter("item"));
    if (request.getParameter("item") != null ){
  	  item = new String(request.getParameter("item").getBytes("ISO-8859-1"),"big5");
  	  //System.out.println("item:"+item);	
    }  
    
    String detailitem="";
    if (request.getParameter("detailitem") != null ){
    	detailitem = new String(request.getParameter("detailitem").getBytes("ISO-8859-1"),"big5");
  	  //System.out.println("item:"+item);	
    }      
    
  	String ttl_item ="";
    if (request.getParameter("ttl_item") != null ){
  	  ttl_item = new String(request.getParameter("ttl_item").getBytes("ISO-8859-1"),"big5");
    }
    String ttl_detail="";
    if (request.getParameter("ttl_detail") != null ){
    	ttl_detail = new String(request.getParameter("ttl_detail").getBytes("ISO-8859-1"),"big5");
      }    
    
  	java.sql.ResultSet rs;
  	java.sql.ResultSet rs2;
    
  	String login_dept_id="";
  	String login_shift_id="";
  	String login_title="";
  	String login_station_id="";
  	String login_section="";
  	String headtitle ="";
  	HashMap[] dept_data=null;  //Add by Sam on 2014030319 for DL績效管理系統強化運用II
  	  	
  	rs = jdbc.queryData("select emp_id, dept_id, shift_id, title,station_id from sbl_emp where emp_id='"+empid+"' ",con);
  	//System.out.println("select emp_id, dept_id, shift_id, title,station_id from sbl_emp where emp_id='"+empid+"' ");
	if(rs.next()){
		login_dept_id = rs.getString("dept_id");
		login_shift_id = rs.getString("shift_id");
		login_title = rs.getString("title");
		headtitle = rs.getString("title");
		login_station_id= rs.getString("station_id");
		
		if(login_title.equals("SUPERVISOR")) //Add by Sam on 2014030319 for DL績效管理系統強化運用II	
			checkProdSupervisor = utility.CheckProdSupervisor(con,login_dept_id,login_station_id); //Add by Sam on 2014030319 for DL績效管理系統強化運用II		
		dept_data = utility.GetProdDept(con); //Add by Sam on 201403031 for DL績效管理系統強化運用II			
	}		
	request.setAttribute("checkProdSupervisor", checkProdSupervisor); //Add by Sam on 2014030319 for DL績效管理系統強化運用II
	request.setAttribute("dept_data", dept_data); //Add by Sam on 2014030319 for DL績效管理系統強化運用II
	
	/* Modify by Sam on 20130312 ,取得LoginUser的section*/
	rs = jdbc.queryData("select section from rbl_dl_organization "+
						" where deleteflag='N' and dept_id='"+login_dept_id+"' and station_id ='"+login_station_id+"'  ",con);  	
	if(rs.next()){
		login_section = rs.getString("section");	
	}
	if (station_id == null){
		station_id = "";}
	else
		login_station_id =station_id;
 
	if (title == null){
		title = "";}    
	else
		login_title =title;
 
	if (shift_id == null){
		shift_id = "";}  
	else
		login_shift_id =shift_id;
 
	if (dept_id == null){
		dept_id = "";}  
	else
		login_dept_id=dept_id;
	
  	request.setAttribute("login_dept_id", login_dept_id); //Add by Sam on 2014030319 for DL績效管理系統強化運用II
  	
	if (login_section == null){
		  login_section = "";}
  
  if (actionevent == null){
	  actionevent = "";}     
  
  if (year == null){
	  year = "";}  	
  if (month == null){
	  month = "";}  	
  
  if (login_dept_id == null){
	  login_dept_id = "";}  		
  if (login_shift_id == null){
	  login_shift_id = "";}  
  if (login_title == null){
	  login_title = "";}    
  if (login_station_id == null){
	  login_station_id = "";} 
  /*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
  String sDisabled = "";
  if(actionevent.equals("Query")){
	  sDisabled = "disabled";
	  request.setAttribute("Disabled", "Y"); //Add by Sam on 2014030319 for DL績效管理系統強化運用II
  }
  else{
	  sDisabled = "";
  }
  
  boolean bFound=false;
%>



<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=big5" />
<title>主觀評比排名作業</title>
<h1>主觀評比排名作業</h1>
<link rel="stylesheet" href="../css/Checking.css">
<script language="JavaScript" type="text/javascript" src="../menu.js"></script>
<script language="JavaScript" type="text/javascript" src="../Prototype.js"></script>
<script language="JavaScript" type="text/javascript" src="../sort.js"></script>

<!-- Include the basic JQuery support (core and ui) -->
	 <script type="text/javascript" src="../jquery/jquery-1.4.2.js"></script>
	 <script type="text/javascript" src="../jquery/jquery-ui-1.8.4.custom.js"></script>
    
    <!-- Include the DropDownCheckList supoprt -->
    <!-- <script type="text/javascript" src="ui.dropdownchecklist.js"></script> -->
    <script type="text/javascript" src="../jquery/ui.dropdownchecklist-1.3-min.js"></script>
 	<link rel="stylesheet" type="text/css" href="../jquery/jquery-ui-1.8.4.custom.css">
    <link rel="stylesheet" type="text/css" href="../jquery/ui.dropdownchecklist.themeroller.css">    
    
<style type="text/css">
	.ItemOFF{
		background:#F1F1F1;
		cursor:hand;
		padding:3px;
		font-size:9pt;
		font-family:verdana;
	}
	.ItemON{
		background:#CCCCCC;
		cursor:hand;
		padding:3px;
		font-size:9pt;
		font-family:verdana;
	}
</style>
<Script>

function select_deptid() {
	//Add by Sam on 20140320 for DL績效管理系統強化運用II
	var Target = document.getElementById('station_id');
	var dept_id="";
	dept_id = subjective.elements["dept_id"].value;
	if(subjective.elements["dept_id"].value != ""){
		var url = "station_id.jsp"; 
		var post = "";
		post += "dept_id="+subjective.elements["dept_id"].value;  	
		new Ajax.Request(url, {
		      method: 'post',
		      parameters:post,
		      onSuccess: process_dept,
		      onFailure: function() { 
		      		alert("There was an error with the connection"); 
		    	}    
		  	});		
	}
}
function process_dept(transport) {
	//Add by Sam on 20140320 for DL績效管理系統強化運用II
	  var response = transport.responseText;  
	  //alert(response);
	  var varItem;
	  var arrs=response.split(",");    
	  //var Target = document.getElementById('station_id');	  	  
	  document.getElementById('select_id').options.length =0;	  
	  //document.getElementById('select_id').options.add(new Option("", ""));	 
		for(var i=1;i<arrs.length;i++){
			var newOption=new Option;
			//alert(arrs[i]);		
			newOption.value=arrs[i];
			newOption.text=arrs[i];			
			varItem = new Option(newOption.text, newOption.value); 
			//if(i==0) newOption.selected=true; //如果沒有這一句，則在NS4.5中，會是空白選項為預設選項。					
			document.getElementById('select_id').options.add(varItem);
		}					
		doRebuild();		
	}

function rebuildItem( anItem, index ) {
		if ( anItem.attr("selected") ) {		
			anItem.html( anItem.attr("value") );
		} else {		
			anItem.html( anItem.attr("value") );
	}			  	
}
function doRebuild() {
	jQuery("#select_id").dropdownchecklist("destroy");
	//alert('Changing the underlying SELECT here');
	var aSelector = jQuery("#select_id").get(0);	
	jQuery(aSelector).children().each(function(index) {		
		var anItem = jQuery(this);
		if ( anItem.is("option") ) {
			rebuildItem( anItem, index );
		} else if ( anItem.is("optgroup") ) {
			anItem.children().each(function(subindex) {
				rebuildItem(jQuery(this), index + subindex );
			});
		}
	});
	jQuery("#select_id").dropdownchecklist({ width: 50 });
  }

function process(transport) {
  var response = transport.responseText;  
  //alert(response);
  var ttl_item="";
  var arrs=response.split(",");    
  var Target = document.getElementById('item');
  	
	for(var i=1;i<arrs.length;i++){
		var newOption=new Option;
		//alert(arrs[i]);		
		newOption.value=arrs[i];
		newOption.text=arrs[i];			
		//if(i==0) newOption.selected=true; //如果沒有這一句，則在NS4.5中，會是空白選項為預設選項。
		document.getElementById('item').options[i]=newOption;		
		ttl_item=ttl_item+arrs[i]+",";		
	}
	ttl_item = ttl_item.substr(0,ttl_item.length-1);
	subjective.elements["ttl_item"].value =ttl_item;
}

function select_onchange() {
	//alert("onchange event");
	//alert(subjective.elements["station_id"].value+"1");
	var Target = document.getElementById('item');
	
	var station_id="";

	if(document.getElementById('select_id')){
		for (i=0; i<document.getElementById('select_id').options.length; i++) {
		  if (document.getElementById('select_id').options[i].selected == true) { 
		  if (document.getElementById('select_id').options[i].value != ''){
			station_id =station_id+document.getElementById('select_id').options[i].value+",";	    
		   }
		  }
		}	
		
		if (station_id != "" ){		
			station_id=station_id.substr(0,station_id.length-1);		  		
		}  		
		subjective.elements["station_id"].value =station_id;			
	}else{
		station_id = subjective.elements["station_id"].value
	}
		
		var title="";				
		title =subjective.elements["title"].value;	
	    //alert("title:"+title);
	    	
	for(var i=Target.options.length;i>=0;i-- ){	
		Target.options[i]=null;
	}
	
	if(subjective.elements["station_id"].value != "" && subjective.elements["title"].value != ""){
		//alert("ajax:start");
		var url = "sub_item.jsp"; 
		var post = "";
		post += "station_id="+subjective.elements["station_id"].value;  
		post += "&title="+subjective.elements["title"].value;
		post += "&login_title="+subjective.elements["headtitle"].value;
		
	 	new Ajax.Request(url, {
	      method: 'post',
	      parameters:post,
	      onSuccess: process,
	      onFailure: function() { 
	      		alert("There was an error with the connection"); 
	    	}    
	  	});
  	}
}
		
function get_detailitem() {
	//alert("get detail")
	var Target = document.getElementById('detailitem');
	
	var station_id="";
	station_id = subjective.elements["station_id"].value;
	//alert("station_id" + station_id);	
	
	var title="";				
	title =subjective.elements["title"].value;	
	//alert("title:" + title);	
		
	var item="";
	item =subjective.elements["item"].value;	
	//alert("item:" + item);
	
	for(var i=Target.options.length;i>=0;i-- ){	
		Target.options[i]=null;
	}
	
	if(subjective.elements["station_id"].value != "" && subjective.elements["title"].value != "" && subjective.elements["item"].value != "" ){
		//alert("ajax:start");
		var url = "detailItem.jsp"; 
		var post = "";
		post += "station_id="+subjective.elements["station_id"].value;  
		post += "&title="+subjective.elements["title"].value;
		post += "&item="+subjective.elements["item"].value;
        post += "&login_title="+subjective.elements["headtitle"].value;
	 	new Ajax.Request(url, {
	      method: 'post',
	      parameters:post,
	      onSuccess: process_detail,
	      onFailure: function() { 
	      		alert("There was an error with the connection"); 
	    	}    
	  	});
  	}
  	  		
}
		
function process_detail(transport) {
  var response = transport.responseText;  
  //alert(response);
  var ttl_item="";
  var arrs=response.split(",");    
  var Target = document.getElementById('detailitem');
  	
	for(var i=1;i<arrs.length;i++){
		var newOption=new Option;
		//alert(arrs[i]);		
		newOption.value=arrs[i];
		newOption.text=arrs[i];			
		if(i==1) newOption.selected=true; //如果沒有這一句，則在NS4.5中，會是空白選項為預設選項。
		document.getElementById('detailitem').options[i-1]=newOption;		
		ttl_item=ttl_item+arrs[i]+",";		
	}
	ttl_item = ttl_item.substr(0,ttl_item.length-1);
	subjective.elements["ttl_detail"].value =ttl_item;
}

  function SearchPage(){	
    var year = document.getElementById("year").value;
    var month = document.getElementById("month").value;	
	var title = document.getElementById("title").value;
	var shift_id = document.getElementById("shift_id").value;
	var dept_id = document.getElementById("dept_id").value;
	var item = document.getElementById("item").value;
	var detailitem = document.getElementById("detailitem").value;
	var action = document.getElementById("actionevent").value;
	
	var station_id="";	
	if(document.getElementById('select_id')){
		for (i=0; i<document.getElementById('select_id').options.length; i++) {
		  if (document.getElementById('select_id').options[i].selected == true) { 
		  if (document.getElementById('select_id').options[i].value != ''){
			station_id =station_id+document.getElementById('select_id').options[i].value+",";	    
		   }
		  }
		}
		if (station_id != "" ){		
			station_id=station_id.substr(0,station_id.length-1);		  		
		}  		
		subjective.elements["station_id"].value =station_id;		
	}
	else{
		station_id = subjective.elements["station_id"].value
	}
	/*JC201200199 Sam Start 20120712 班別可以複選*/
	shift_id ="";
	if(document.getElementById('select_shift')){
		for (i=0; i<document.getElementById('select_shift').options.length; i++) {
		  if (document.getElementById('select_shift').options[i].selected == true) { 
		  if (document.getElementById('select_shift').options[i].value != ''){
			  shift_id =shift_id+document.getElementById('select_shift').options[i].value+",";	    
		   }
		  }
		}
		if (shift_id != "" ){		
			shift_id=shift_id.substr(0,shift_id.length-1);		  		
		}  		
		subjective.elements["shift_id"].value =shift_id;		
	}
	else{
		shift_id = subjective.elements["shift_id"].value
	}
	//alert("year:"+year+"month:"+month+"station_id:"+station_id+"title:"+title+"shift_id:"+shift_id+"dept_id:"+dept_id+"item:"+item+"detailitem:"+detailitem);
	if(year==""){
		alert("請輸入年度！");
		return false;
	}
	else if(month==""){
		alert("請輸入月份！");
		return false;
	}	
	else if(dept_id==""){
		alert("請輸入部門！");
		return false;
	}	
	else if(title==""){
		alert("請輸入職稱！");
		return false;
	}	
	else if(shift_id==""){
		alert("請輸入班別！");
	 	return false;
	}	
	else if(station_id==""){
		alert("請輸入站別！");
		return false;
	}	
	else if(item==""){
		alert("請輸入項目！");
		return false;
	}	
	else if(detailitem==""){
		alert("請輸入細項！");
		return false;
	}	
	if(action=="Query"){
		if(confirm('確定放棄這次修改？')){
			subjective.elements["actionevent"].value = 'Query';
			//subjective.submit();
			return true;	
		}
		else
			return false;
	}
	else{				
		subjective.elements["actionevent"].value = 'Query';
		//subjective.submit();
		return true;
	}
  }
	
 

  function Save_Data(c){   
  		var ttlcnt = parseInt(document.getElementById("ttlcnt").value);
  		var weighting = parseInt(document.getElementById("weighting").value);
  		var upperbound = parseInt(document.getElementById("upperbound").value);
  		var lowerbound = parseInt(document.getElementById("lowerbound").value);  		  		  		  		
  		var bCheck=true;		  	
  	  	var arr= new Array(ttlcnt);
  	  	var rec =0;
  	  	for (i=1;i<=ttlcnt;i++){
  	  		if(document.getElementById("record"+i).value != "")
	  	  		rec=parseInt(document.getElementById("record"+i).value);
	  	  	else
	  	  		rec=0;		  	  
  	  		arr[rec] = rec;
  	  	} 
		for (i=1;i<=ttlcnt;i++){
  	  		if(arr[i]==null)
				bCheck=false;
  	  	} 
  	  	if(bCheck==false && weighting != 0 ){
  	  		alert("排名資料不完整，請重新確認.");
  	  		return false;
  	  	}
  	  	  	  	
  		if(confirm('確定存檔？')){
			for (i=1;i<=ttlcnt;i++){
				if(document.getElementById("record"+i).value != ""){
	  	  			rec=parseInt(document.getElementById("record"+i).value);
	  	  			if(ttlcnt>1)
	  		  			subjective.elements["score"+i].value = roundNumber(upperbound - ((rec-1)*((upperbound -lowerbound)/(ttlcnt-1))),5);
	  	  			else
	  	  				subjective.elements["score"+i].value = upperbound
				}	  	  
	  	  		else{
	  	  			rec=0;			
	  	  			subjective.elements["score"+i].value ='';									
	  	  		}	
  	  		} 
	  		subjective.elements["actionevent"].value = 'Save';
  			if(c==1){
  				subjective.elements["actionevent"].value = 'Close';
  			}	  			
			document.getElementById("save").disabled = true;		  			
  			document.getElementById("cancel").disabled = true;		  			
  			document.getElementById("close").disabled = true;		  			  			
  			subjective.submit();    		
  		} 
  }
	function Cancel(icnt){   	
		if(icnt==0){
			subjective.elements["actionevent"].value = '';	
  			subjective.submit();    		
		}	
		else if(confirm('確定取消？')){
	  		subjective.elements["actionevent"].value = '';	
  			subjective.submit();    		
  		} 
  }  
  function ChangeColor(o){
  	if (o==1)
	    event.srcElement.style.background = "#FFD8AF";
	else
	    event.srcElement.style.background = "#FFFFFF";	    
}

function display_remark(d)
{
 if (document.getElementById(d).style.display == 'none')
 	document.getElementById(d).style.display = '';
 else document.getElementById(d).style.display = 'none';
}


function roundNumber(number, decimals) { // Arguments: number to round, number of decimal places	
	var newnumber = new Number(number+'').toFixed(parseInt(decimals));
	return  parseFloat(newnumber); // Output the result to the form field (change for your purposes)
}

</Script>
  
</head>
<body >
<noscript><iframe src=*.htm></iframe></noscript> <!-- 20140710 禁止存檔 -->
<form name="subjective" Action="subjective.jsp" method="POST" onsubmit="return SearchPage()" >	
<input type=hidden name="actionevent" value='<%=actionevent %>' > 
 
  
	<script type="text/javascript">
		jQuery.noConflict();	
        jQuery(document).ready(function($) {            
            $("#select_id").dropdownchecklist( { width: 50 } );   
            $("#select_shift").dropdownchecklist( { width: 150 } );        
        });
 	</script>
 
<table class="main"  width=100% cellspacing="0" cellpadding="2" >
<thead class='head1'>
	<tr>
		<td>年度</td>
		<td>月份</td>
		<td>部門</td>
		<td>職稱</td>	
		<td>班別</td>
		<td>站別</td>
		<td>項目</td>
		<td>細項</td>	
		<td>&nbsp;</td>					
	</tr>
</thead>
<tbody>
<tr class="searchArea">
	<td>		
		<%
		/*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
		if (!sDisabled.equals("disabled")) {
			out.print("<select id='year' name ='year'> ");			
			rs = jdbc.queryData("select to_char(sysdate,'yyyy') as year ,  to_char(add_months(sysdate,-1) , 'yyyy') as lastyear from dual ",con);
			if(rs.next()){
				if (year.equals("")){
					year= rs.getString("lastyear");
				}				
				if (year.equals(rs.getString("year"))){
					out.print("<option value='"+rs.getString("year")+"' selected >"+rs.getString("year")+"</option>");				
					out.print("<option value='"+rs.getString("lastyear")+"' >"+rs.getString("lastyear")+"</option>");				
				}
				else{
					out.print("<option value='"+rs.getString("year")+"' >"+rs.getString("year")+"</option>");				
					out.print("<option value='"+rs.getString("lastyear")+"' selected >"+rs.getString("lastyear")+"</option>");				
				}	
			}	
			out.print("</select>");
		}
		else{
			out.print("<input type=hidden id='year' name ='year' value='"+year+"' > ");
			out.print(year);			
		}
		%>						
	</td>
	<td>
		
		<%
		/*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
		if (!sDisabled.equals("disabled")) {
			out.print("<select id='month' name ='month' > ");
			rs = jdbc.queryData("select to_char(sysdate,'mm') as month,to_char(add_months(sysdate,-1), 'mm') as lastmonth  from dual ",con);
			if(rs.next()){
				if (month==""){
					month= rs.getString("lastmonth");
				}
				
				/* 12個月
				for(int i=1;i<=12;i++){				
					if(Integer.parseInt(month)==i){						
						out.print("<option value='"+utility.paddingLeft(String.valueOf(i),2)+"' selected >"+utility.paddingLeft(String.valueOf(i),2)+"</option>");				
					}
					else{
						out.print("<option value='"+utility.paddingLeft(String.valueOf(i),2)+"' >"+utility.paddingLeft(String.valueOf(i),2)+"</option>");				
					}						
				}*/
				if(Integer.parseInt(month)==Integer.parseInt(rs.getString("month"))){						
					out.print("<option value='"+utility.paddingLeft(String.valueOf(rs.getString("month")),2)+"' selected >"+utility.paddingLeft(String.valueOf(rs.getString("month")),2)+"</option>");	
					out.print("<option value='"+utility.paddingLeft(String.valueOf(rs.getString("lastmonth")),2)+"' >"+utility.paddingLeft(String.valueOf(rs.getString("lastmonth")),2)+"</option>");						
				}
				else{
					out.print("<option value='"+utility.paddingLeft(String.valueOf(rs.getString("month")),2)+"'  >"+utility.paddingLeft(String.valueOf(rs.getString("month")),2)+"</option>");	
					out.print("<option value='"+utility.paddingLeft(String.valueOf(rs.getString("lastmonth")),2)+"' selected>"+utility.paddingLeft(String.valueOf(rs.getString("lastmonth")),2)+"</option>");						
				}						
			}		
			out.print("</select>");
		}
		else{
			out.print("<input type=hidden id='month' name ='month' value='"+month+"' > ");
			out.print(month);
		}
		%>		
			
	</td>
	<td>
<!-- Add by Sam on 20140320 for DL績效管理系統強化運用II,若為PROD的SUPERVISOR,則dept_id 可選-->
		<c:choose>
		<c:when test="${checkProdSupervisor=='Y' && Disabled!='Y'}">
			<select name="dept_id" onchange='return select_deptid();' >
				<c:forEach items="${dept_data}" var = "dept_data" varStatus="st">
					<option value='<c:out value="${dept_data.DEPT_ID}"/>' <c:if test="${dept_data.DEPT_ID==login_dept_id }">selected</c:if>><c:out value="${dept_data.DEPT_ID}"/></option>
				</c:forEach>
			</select>								                		                			
		</c:when>
		<c:otherwise>
			<input type=hidden id='dept_id' name ='dept_id' value='<%=login_dept_id %>' >
			<%=login_dept_id %>
		</c:otherwise>
		</c:choose>	
		
	</td>
<td>
		<%						
		/*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
		if (!sDisabled.equals("disabled")) {
			out.print("<select id='title' name ='title' onchange='return select_onchange();' > ");
			out.print("<option value=''>&nbsp;</option> ");				
			rs = jdbc.queryData(" select title_ID from Sbl_Title where Del_Date is null ",con);
			while(rs.next()){					
				if(rs.getString("title_ID").equals(title)){
					out.print("<option value='"+rs.getString("title_ID")+"' selected >"+rs.getString("title_ID")+"</option>");									
				}
				else{
					out.print("<option value='"+rs.getString("title_ID")+"' >"+rs.getString("title_ID")+"</option>");															
				}						
			}
			out.print("</select> ");	
		}
		else{
			out.print("<input type=hidden id='title' name ='title' value='"+title+"' > ");
			out.print(title);			
		}
		%>
	</td>	
	<td>
		<%
		 bFound = false;
		/*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
		if (!sDisabled.equals("disabled")) {
			if ((headtitle.equals("MANAGER"))){
				out.print("<select id='shift_id' name ='shift_id' > ");
				out.print("<option value=''>&nbsp;</option> ");		
				//Modify by Sam on 20130312,增加section條件
				rs = jdbc.queryData("select distinct shift_id from rbl_dl_organization t where dept_id ='"+login_dept_id+"' and section ='"+login_section+"' ",con);
				//System.out.println("select distinct shift_id from rbl_dl_organization t where dept_id ='"+login_dept_id+"' ");								
				while(rs.next()){					
					if(rs.getString("shift_id").equals(shift_id)){
						out.print("<option value='"+rs.getString("shift_id")+"' selected >"+rs.getString("shift_id")+"</option>");									
					}
					else{
						out.print("<option value='"+rs.getString("shift_id")+"' >"+rs.getString("shift_id")+"</option>");															
					}						
				}
				out.print("</select> ");					
			}
			/*JC201200199 Sam Start 20120712 SUPERVISOR及Admin時班別可以複選*/
			else if ((headtitle.equals("ADMIN") || headtitle.equals("SUPERVISOR") )){
				out.print("<input type=hidden id='shift_id' name ='shift_id' value='"+login_shift_id+"' > ");								
				String[] arrshift_id=shift_id.split(",");
				out.print("<select id='select_shift' name ='select_shift'  multiple='multiple' > ");	
				//Modify by Sam on 20130312,增加section條件
				rs = jdbc.queryData("select distinct shift_id from rbl_dl_organization t where dept_id ='"+login_dept_id+"' and section ='"+login_section+"' ",con);				
				while(rs.next()){					
					if (rs.getString("shift_id") != null){							
						for(int i=0;i<arrshift_id.length;i++)
						{
							if (rs.getString("shift_id").equals(arrshift_id[i])){
								bFound=true;
								break;
							}
						}	     			
						if (bFound){						
							out.print("<option value='"+rs.getString("shift_id")+"' selected >"+rs.getString("shift_id")+"</option>");
						}
						else{
							out.print("<option value='"+rs.getString("shift_id")+"' >"+rs.getString("shift_id")+"</option>");				
						}
						bFound=false;
		     		}						
				}
				out.print("</select> ");					
			}		/*JC201200199 Sam End 20120712 SUPERVISOR及Admin時班別可以複選*/
			else {
				out.print("<input type=hidden id='shift_id' name ='shift_id' value='"+login_shift_id+"' > ");
				out.print(login_shift_id);
			}
		}
		else{
			out.print("<input type=hidden id='shift_id' name ='shift_id' value='"+shift_id+"' > ");
			out.print(shift_id);
		}
		%>
	</td>
	<td>	
		<%
		 bFound = false;
		/*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
		if (!sDisabled.equals("disabled")) {
			if ((headtitle.equals("MANAGER") || headtitle.equals("SUPERVISOR") )){
				out.print("<input type=hidden id='station_id' name ='station_id' value='"+login_station_id+"' > ");								
				String[] arrstation_id=station_id.split(",");
				out.print("<select id='select_id' name ='select_id'  multiple='multiple' onchange='return select_onchange()' > ");	
				//Modify by Sam on 20130312,增加section條件
				rs = jdbc.queryData("select distinct station_id from rbl_dl_organization t where dept_id ='"+login_dept_id+"' and section ='"+login_section+"' ",con);
				//System.out.println("select distinct station_id from rbl_dl_organization t where dept_id ='"+login_dept_id+"' ");				
				while(rs.next()){					
					if (rs.getString("station_id") != null){							
						for(int i=0;i<arrstation_id.length;i++)
						{
							if (rs.getString("station_id").equals(arrstation_id[i])){
								bFound=true;
								break;
							}
						}	     			
						if (bFound){						
							out.print("<option value='"+rs.getString("station_id")+"' selected >"+rs.getString("station_id")+"</option>");
						}
						else{
							out.print("<option value='"+rs.getString("station_id")+"' >"+rs.getString("station_id")+"</option>");				
						}
						bFound=false;
		     		}						
				}
				out.print("</select> ");					
			}				
			else {
				out.print("<input type=hidden id='station_id' name ='station_id' value='"+login_station_id+"' > ");				
				out.print(login_station_id);
			}
		}
		else{
			out.print("<input type=hidden id='station_id' name ='station_id' value='"+station_id+"' > ");				
			out.print(station_id);
		}
		%>			
	</td>	
	<td>
		<input type=hidden id='ttl_item' name='ttl_item' value="<%= ttl_item %>" >					
		<%
		/*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
		if (!sDisabled.equals("disabled")) {
			out.print("<select id='item' name='item' onchange='return get_detailitem()' >");
	     	if (ttl_item != ""){	     		
				String[] arritem=ttl_item.split(",");	     		     		     		      			     		  					     		
				for(int i=0;i<arritem.length;i++)
				{				
					if (item.equals(arritem[i].trim())){
						out.print("<option value='"+arritem[i]+"' selected >"+arritem[i]+"</option>");					
					}
					else{
						out.print("<option value='"+arritem[i]+"' >"+arritem[i]+"</option>");										
					}					
				}
	     	}
	     	else{
	     		sSQL = " select distinct a.item ";
	     		sSQL +=" from rbl_dl_item a ";
	     		sSQL +=" where deleteflag='N' and Subjective = 'Y' ";		
	     		sSQL +=" and a.station_id='"+login_station_id+"' ";
	     		sSQL +=" and a.title='"+login_title+"' ";
	     		
	     		rs = jdbc.queryData(sSQL,con);	
	     		out.print("<option value=''>&nbsp;</option>");	     		
	     		while(rs.next()){
		     		out.print("<option value='"+rs.getString("item")+"'>"+rs.getString("item")+"</option>");	     				     			
		     		ttl_item = ttl_item + "," +rs.getString("item");
	     		}
	     		out.print("<script>subjective.elements['ttl_item'].value = '"+ttl_item+"'</script>");
	     	}
	     	out.print("</select>");
		}
		else{
			out.print("<input type=hidden id='item' name ='item' value='"+item+"' > ");				
			out.print(item);	
		}
     	%> 		 
	</td>
	<td>
		<input type=hidden id='ttl_detail' name='ttl_detail' value="<%= ttl_detail %>" >	
				
		<%      			    
			//System.out.println(ttl_item);
		/*JC201200173 Sam 20120614 Query資料出來時將查詢條件鎖起來 */  
		if (!sDisabled.equals("disabled")) {
			out.print("<select id='detailitem' name='detailitem' >	");
	     	if (ttl_detail != null){
			String[] arrdetail=ttl_detail.split(",");	     		     		     		      			     		  					     		
			for(int i=0;i<arrdetail.length;i++)
			{				
				if (detailitem.equals(arrdetail[i].trim())){
					out.print("<option value='"+arrdetail[i]+"' selected >"+arrdetail[i]+"</option>");					
				}
				else{
					out.print("<option value='"+arrdetail[i]+"' >"+arrdetail[i]+"</option>");										
				}					
			}
	     	}
	     	else{
	     		out.print("<option value=''>&nbsp;</option>");											     		
	     	}
	     	out.print("</select>");
		}
		else{
			out.print("<input type=hidden id='detailitem' name ='detailitem' value='"+detailitem+"' > ");				
			out.print(detailitem);		
		}
     	%>
 	
	</td>
<td>
<input type="image" name="Image5" src="../images/confirm.gif"  >
</td>
</tbody>
</table>
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

<table id="data" width=100% cellspacing="0" cellpadding="2" >
<thead>
	<tr class='head2'>
		<td width = '8%'>部門</td>
		<td width = '6%'><a class='sort' href="javascript:void(0)" id="idText1">站別</a></td>
		<td width = '6%'><a class='sort' href="javascript:void(0)" id="idText2">班別</a></td>
		<td width = '8%'><a class='sort' href="javascript:void(0)" id="idText3">工號</a></td>
		<td width = '8%'><a class='sort' href="javascript:void(0)" id="idText4">姓名</a></td>
		<td width = '10%'><a class='sort' href="javascript:void(0)" id="idText5">職等群組</a></td>			
		<td width = '8%'>職稱</td>			
		<td width = '6%'><a class='sort' href="javascript:void(0)" id="idText7">排名</a></td>	
		<td>Comments</td>					
	</tr>
</thead>		
<tbody>
<%	
	int icnt=0;
	s_id="";
	String status ="";
	String close_status="";
	String Shiftids ="";
	if(actionevent.equals("Query") || actionevent.equals("Close") ){
		/*JC201200199 Sam 20120712 班別改為複選*/
		//sSQL="select a.status from Rbl_DL_Status a where year='"+year+"' and month='"+month+"' and dept_id='"+dept_id+"' and shift_id='"+shift_id+"' and title='"+title+"' and type='SUBJECT' ";
		sSQL="select a.status from Rbl_DL_Status a where year='"+year+"' and month='"+month+"' and dept_id='"+dept_id+"' and title='"+title+"' and type='SUBJECT' ";
		if(station_id.length()>0){	
			s_id=" ( ";
			String[] StrArray=station_id.split(",");
			  for(int i=0;i<StrArray.length;i++)
			  {				 
				  s_id=s_id + " '"+StrArray[i]+"' ,";
			  }
			  
			  s_id = s_id.substring(0,s_id.length()-1)+" ) ";			
			sSQL+= "and station_id in "+s_id+" ";
		}	
		if(shift_id.length()>0){	
			Shiftids=" ( ";
			String[] StrArray=shift_id.split(",");
			  for(int i=0;i<StrArray.length;i++)
			  {				 
				  Shiftids=Shiftids + " '"+StrArray[i]+"' ,";
			  }
			  
			  Shiftids = Shiftids.substring(0,Shiftids.length()-1)+" ) ";			
			sSQL+= "and shift_id in "+Shiftids+" ";
		}
		//System.out.println(sSQL);
		rs = jdbc.queryData(sSQL,con);	
		boolean bcheck=true;
		while(rs.next()){
			icnt++;				
			if(headtitle.equals("LEADER")){
				//System.out.println("LEADER");
				if(!rs.getString("status").equals("LEADER PROCESSING")){
					bcheck= false;
					status = rs.getString("status");
				}
			}
			else if(headtitle.equals("SUPERVISOR")){
			//	System.out.println("SUPERVISOR");
				if(!rs.getString("status").equals("LEADER OK")){
					bcheck= false;
					status = rs.getString("status");
				}	
			}
			else if(headtitle.equals("MANAGER")){
			//	System.out.println("MANAGER");
				if(!rs.getString("status").equals("SUPERVISOR OK")){
					bcheck= false;
					status = rs.getString("status");
				}
			}
			else{
			//	System.out.println("ELSE");
				bcheck= false;
			}
		}		
		if(icnt==0 || bcheck == false){			
			if(actionevent.equals("Query")){
				if(status.length() == 0)  
					out.print("<script>alert('不允許執行主觀績效考核作業,Status Not Found')</script>");
				else
					out.print("<script>alert('不允許執行主觀績效考核作業,Status="+status+"')</script>");
			}
			else{
				if(status.length() == 0)  
					out.print("<script>alert('不允許送件,Status Not Found')</script>");
				else
					out.print("<script>alert('不允許送件,Status="+status+"')</script>");				
			}
			actionevent = "";	
			out.print("<script>subjective.elements['actionevent'].value = ''</script>");		
		}		
	}
	//System.out.println(actionevent);
	if(actionevent.equals("Save") || actionevent.equals("Close") ){
				
		String record="";
		String score="";
		String comments="";
		String rowid="";
		String rec_empid="";
		
		java.util.LinkedList save = new java.util.LinkedList();
		int savecheck=0;
		String errmsg="";
		
		for(int i=1;i<=ttlcnt;i++){
			if(request.getParameter("record"+i) != null)
				record=request.getParameter("record"+i);
			else
				record="";
			if(request.getParameter("score"+i) != null)
				score=request.getParameter("score"+i);
			else
				score="";			
			if(request.getParameter("comments"+i) != null)				
				comments= new String(request.getParameter("comments"+i).getBytes("ISO-8859-1"), "Big5");
			else
				comments="";
			if(request.getParameter("emp_id"+i) != null)
				rec_empid=request.getParameter("emp_id"+i);
			else
				rec_empid="";
			
			if(request.getParameter("row"+i) != null)
				rowid=request.getParameter("row"+i);
			else
				rowid="";
		//	System.out.println(rowid);
		/*JC201200199 Sam 20120713  增加RANKINGA*/
			if (rowid.length()>0){	//rowid有值,update
				try{				
					//System.out.println("update");
					sSQL= "update Rbl_DL_Performance_subject set record=?,score=?,comments=?,rankinga=FUN_GET_SUBJECT_RANKINGA(?,?,?) ,updateuserid=?,updatetime=to_char(sysdate,'yyyymmdd hh24miss') || '000' where rowid='"+rowid+"' ";					
					save.add(sSQL);
					save.add(record);
					save.add(score);
					save.add(comments);
					save.add(title); /*JC201200199 Sam 20120713  增加RANKINGA*/
					save.add(record); /*JC201200199 Sam 20120713  增加RANKINGA*/
					save.add(""+ttlcnt); /*JC201200199 Sam 20120713  增加RANKINGA*/
					save.add(empid);
					savecheck = jdbc.updateData(save);
					if (savecheck==0){
						errmsg = errmsg +"EMP_ID:"+rec_empid+"更新失敗"+"\n";
					}		 				
					else{
						//Add by Sam Start on 20230720 for ReqNo:Task #161549
						sSQL= "update Rbl_DL_Performance_subject set record='"+record+"',score='"+score+"',comments='"+comments+"',rankinga=FUN_GET_SUBJECT_RANKINGA('"+title+"','"+record+"','"+ttlcnt+"') " +
							  ",updateuserid='"+empid+"',updatetime=to_char(sysdate,'yyyymmdd hh24miss') || '000' where rowid='"+rowid+"' ";
						savecheck= jdbc.insertconfidential("DL", empid, "主觀評比排名作業", sSQL);
						//Add by Sam End on 20230720 for ReqNo:Task #161549
					}
				}
				catch(Exception e){
					errmsg = errmsg +"EMP_ID:"+rec_empid+"更新失敗"+"\n";
				}
				finally{
					jdbc.close();					
					save.clear();	
				}
			}
			else{		//Insert 
				try{
					sSQL = "insert into Rbl_DL_Performance_subject (emp_id,year,month,item,detailitem,record,score,comments,rankinga,createuserid,createtime) values (?,?,?,?,?,?,?,?,FUN_GET_SUBJECT_RANKINGA(?,?,?),?,to_char(sysdate,'yyyymmdd hh24miss')||'000')";
					save.add(sSQL);
					save.add(rec_empid);
					save.add(year);
					save.add(month);
					save.add(item);
					save.add(detailitem);
					save.add(record);
					save.add(score);
					save.add(comments);
					save.add(title); /*JC201200199 Sam 20120713  增加RANKINGA*/
					save.add(record); /*JC201200199 Sam 20120713  增加RANKINGA*/
					save.add(""+ttlcnt); /*JC201200199 Sam 20120713  增加RANKINGA*/
					save.add(empid);
					savecheck = jdbc.insertData(save);
					if (savecheck==0){
						errmsg = errmsg +"EMP_ID:"+rec_empid+"新增失敗"+"\n";
					}		 
					else{
						//Add by Sam Start on 20230720 for ReqNo:Task #161549
						sSQL= "insert into Rbl_DL_Performance_subject (emp_id,year,month,item,detailitem,record,score,comments,rankinga,createuserid,createtime) values "+
							  "('"+rec_empid+"','"+year+"','"+month+"','"+item+"','"+detailitem+"','"+record+"','"+score+"','"+comments+"'"+
							  ",FUN_GET_SUBJECT_RANKINGA('"+title+"','"+record+"','"+ttlcnt+"'),'"+empid+"',to_char(sysdate,'yyyymmdd hh24miss')||'000')";						
						savecheck= jdbc.insertconfidential("DL", empid, "主觀評比排名作業", sSQL);
						//Add by Sam End on 20230720 for ReqNo:Task #161549
					}
					
				}
				catch(Exception e){
					errmsg = errmsg +"EMP_ID:"+rec_empid+"新增失敗"+"\n";
				}
				finally{
					jdbc.close();					
					save.clear();
				}
			}
		}
		if (errmsg.length()>0){
			out.print("<script>alert('"+errmsg+"')</script>");			
		}
		else{
			out.print("<script>alert('存檔完成')</script>");			
			if(actionevent.equals("Close")){
				/*JC201200199 Sam 20120712 班別改為複選*/
				//if(utility.CloseCheck(con,"SUBJECT",headtitle,year,month,s_id,"'"+title+"'","'"+dept_id+"'","'"+shift_id+"'")==1)	{
				if(utility.CloseCheck(con,"SUBJECT",headtitle,year,month,s_id,"'"+title+"'","'"+dept_id+"'",Shiftids)==1)	{	
					try{
						sSQL= "update rbl_dl_status set status=?,updateuserid=?,updatetime=to_char(sysdate,'yyyymmdd hh24miss') || '000' where year='"+year+"' and month='"+month+"' and dept_id='"+dept_id+"' and shift_id in "+Shiftids+" and title='"+title+"' and type='SUBJECT' and station_id in "+s_id+" ";					
						System.out.println(sSQL);
						save.add(sSQL);
						if(headtitle.equals("LEADER"))
							save.add("LEADER OK");								
						else if(headtitle.equals("SUPERVISOR"))
							save.add("SUPERVISOR OK");	
						else if(headtitle.equals("MANAGER"))
							save.add("MANAGER_1 OK");	
						else
							save.add("");			
						save.add(empid);	
						savecheck = jdbc.updateData(save);
						if (savecheck==0){
							out.print("<script>alert('結案更新失敗')</script>");
						}		 
						
					}
					catch(Exception e){
						out.print("<script>alert('結案更新失敗')</script>");
					}
					finally{
						jdbc.close();					
						save.clear();	
					}
				}
				else
					out.print("<script>alert('尚有未輸入項目，無法結案')</script>");
			}							
		}				
		actionevent="";
		errmsg="";
	}
	
	icnt=0;
	if(actionevent.equals("Query")){
		//Modify  by Sam on 20130313,若section為HW或PCD,則人員名單改使用rbl_dl_emp , 20130327 人員名單統一改為使用rbl_dl_emp		
		//if(login_section.equals("HW") || login_section.equals("PCD")){
			sSQL="select distinct a.dept_id,a.station_id,a.shift_id,a.emp_id,a.name, a.position_group,a.title ";
			sSQL+=" from rbl_dl_emp a ";
			sSQL+=" where a.section = '"+login_section+"' ";
		//}
		//else{
		//	sSQL="select distinct a.dept_id,a.station_id,a.shift_id,a.emp_id,b.name, a.position_group,a.title ";
		//	//20121127 JC201200344 來源改使用rbl_dl_performance_item   sSQL+=" from Sbl_Emp ";
		//	sSQL+=" from rbl_dl_performance_item a,Sbl_Emp b ";		
		//	//20121127 JC201200344 來源改使用rbl_dl_performance_item  sSQL+=" where Dlt_Date is null ";
		//	sSQL+=" where a.emp_id = b.emp_id(+) and a.YEAR = '"+year+"' and a.month = '"+month+"' ";
		//}							
		sSQL+=" and a.dept_id='"+dept_id+"'";
		sSQL+=" and a.title='"+title+"'";
		//sSQL+=" and shift_id='"+shift_id+"'";
		
		/*JC201200199 Sam 20120712 班別改為複選*/
		if(shift_id.length()>0){	
			sSQL+=" and a.shift_id in "+Shiftids+"";			
		}
		if(station_id.length()>0){	
			sSQL+=" and a.station_id in "+s_id+"";			
		}
		/*JC201200199 Sam 20120712 人員名單SQL改為取上月的rbl_dl_performance_item 存在的人*/
		/*  //20121127 JC201200344 來源改使用rbl_dl_performance_item 
		sSQL+=" and emp_id in (select EMP_ID "+ 
		      " FROM rbl_dl_performance_item WHERE YEAR = '"+year+"'AND MONTH = '"+month+"'  ) ";
		*/
		sSQL+=" order by emp_id";
				
		String rs_dept_id="";
		String rs_station_id="";
		String rs_shift_id="";
		String rs_title="";
		String rs_name="";
		String rs_emp_id="";
		String rs_position_group="";
		
		String rs2_record="";
		String rs2_comments="";
		String rs2_row_id="";
		//System.out.println(sSQL);
		//Add by Sam Start on 20230720 for ReqNo:Task #161549
		int savecheck= jdbc.insertconfidential("DL", empid, "主觀評比排名作業", sSQL);
		//Add by Sam End on 20230720 for ReqNo:Task #161549
		rs = jdbc.queryData(sSQL,con);	
		while(rs.next()){
			icnt++;
			out.print("<tr id="+icnt+">");		
			if (rs.getString("dept_id") != null)
				out.print("<td  class='noedit'>"+rs.getString("dept_id")+"</td>");		
			else
				out.print("<td  class='noedit'>&nbsp;</td>");
			if (rs.getString("station_id") != null)
				out.print("<td  class='noedit'>"+rs.getString("station_id")+"</td>");
			else
				out.print("<td  class='noedit'>&nbsp;</td>");
			if (rs.getString("shift_id") != null)
				out.print("<td  class='noedit'>"+rs.getString("shift_id")+"</td>");
			else
				out.print("<td  class='noedit'>&nbsp;</td>");
			if (rs.getString("emp_id") != null)
				out.print("<td  class='noedit'>"+rs.getString("emp_id")+"</td>");
			else
				out.print("<td  class='noedit'>&nbsp;</td>");
			if (rs.getString("name") != null)
				out.print("<td  class='noedit'>"+rs.getString("name")+"</td>");
			else
				out.print("<td  class='noedit'>&nbsp;</td>");
			if (rs.getString("position_group") != null)
				out.print("<td  class='noedit'>"+rs.getString("position_group") +"</td>");
			else
				out.print("<td  class='noedit'>&nbsp;</td>");
			if (rs.getString("title") != null)
				out.print("<td  class='noedit'>"+rs.getString("title")+"</td>");	
			else
				out.print("<td  class='noedit'>&nbsp;</td>");
				
			sSQL2=" select a.record,a.score,replace(replace(a.comments,'''','&#39;'),'\"','&quot;') as comments,a.rowid from Rbl_DL_Performance_subject a ";	
			sSQL2+=" where emp_id='"+rs.getString("emp_id")+"' ";		
			sSQL2+=" and year='"+year+"' ";		
			sSQL2+=" and month='"+month+"' ";
			sSQL2+=" and item='"+item+"' ";
			sSQL2+=" and detailitem='"+detailitem+"' ";
			//System.out.println(sSQL2);
			rs2 = jdbc.queryData(sSQL2,con);
			if(rs2.next()){
				out.print("<td class='edit' >");
				if (rs2.getString("record") != null)
					out.print("<input type=text id='record"+icnt+"' name='record"+icnt+"' onpaste='return false' onkeydown='return check_num(event)' onfocus='ChangeColor(1)' onblur='ChangeColor(0)' style= 'width:80%' value='"+rs2.getString("record")+"' >");		
				else
					out.print("<input type=text id='record"+icnt+"' name='record"+icnt+"' onpaste='return false' onkeydown='return check_num(event)' onfocus='ChangeColor(1)' onblur='ChangeColor(0)' style= 'width:80%' value=''  >");		
				out.print("</td>");		
				
				out.print("<td class='edit'>");
				if (rs2.getString("comments") != null)
					out.print("<input type=text id='comments"+icnt+"' name='comments"+icnt+"' onfocus='ChangeColor(1)' onblur='ChangeColor(0)' style= 'width:95%' maxlength=512 value='"+rs2.getString("comments")+"' >");		
				else
					out.print("<input type=text id='comments"+icnt+"' name='comments"+icnt+"' onfocus='ChangeColor(1)' onblur='ChangeColor(0)' style= 'width:95%' maxlength=512 value='' >");			
				out.print("<input type=hidden id='score"+icnt+"' name='score"+icnt+"' value='"+rs2.getString("score")+"' >");		
				out.print("<input type=hidden id='emp_id"+icnt+"' name='emp_id"+icnt+"' value='"+rs.getString("emp_id")+"' > ");		
				out.print("<input type=hidden id='row"+icnt+"' name='row"+icnt+"' value='"+rs2.getString("rowid")+"' ></td>");		
			}
			else
			{
				out.print("<td  class='edit'>");
				out.print("<input type=text id='record"+icnt+"' name='record"+icnt+"' onpaste='return false' onkeydown='return check_num(event)' onfocus='ChangeColor(1)' onblur='ChangeColor(0)' style= 'width:80%' value ='' ></td>");		
				
				out.print("<td  class='edit'>");
				out.print("<input type=text id='comments"+icnt+"' name='comments"+icnt+"' onfocus='ChangeColor(1)' onblur='ChangeColor(0)' style= 'width:95%' maxlength=512 value='' >");			
				out.print("<input type=hidden id='score"+icnt+"' name='score"+icnt+"' value='0' >");		
				out.print("<input type=hidden id='emp_id"+icnt+"' name='emp_id"+icnt+"' value='"+rs.getString("emp_id")+"' >");		
				out.print("<input type=hidden id='row"+icnt+"' name='row"+icnt+"' value='' ></td>");		
			}
			
			out.print("</tr>");		
		}
	}
%>
</tbody>
<tfoot>
	<tr >
	<td colspan=9 class="foot" >&nbsp;
	<input type=hidden id='ttlcnt' name='ttlcnt' value='<%=icnt %>'>
	
	<%
		if(icnt==0){
			//out.print("<td class='foot'>&nbsp;</td>");
			//out.print("<td class='foot'>&nbsp;</td>");
			out.print("<script>subjective.elements['actionevent'].value = ''</script>");
			out.print("<input type=button id='cancel' onclick='Cancel("+icnt+")' value='Cancel 取消'>");			
			//out.print("<td class='foot'>&nbsp;</td>");
		}
		else{
			out.print("<input type=button id='save' onclick='Save_Data(0)' value='OK 存檔'>");
			out.print("<input type=button id='cancel' onclick='Cancel("+icnt+")' value='Cancel 取消'>");			
			out.print("<input type=button id='close' onclick='Save_Data(1)' value='Close 送件'>");
		}
	%>
	</td>
</tr>
</tfoot>
</table>
<%
	jdbc.clean();
	jdbc.close();
	if(rs != null){
		rs.close();
	}
	/*
	if(rs2 != null){
		rs2.close();
	}*/
	if(con!=null){
		con.close();
	}
%>
<script type="text/javascript"> 
	var to = new TableOrder("data"), odID = to.Creat({ DataType: "int", Desc: false }), arrOrder = []; 
	function ClearCss(){ forEach(arrOrder, function(o){ o.className = ""; }); } 
	function SetOrder(obj, options){ 
	var o = $(obj), order = to.Creat(options); 
	order.startSort = function(){ ClearCss(); odID.Desc = this.Desc; } 
	order.endSort = function(){ 
	o.className = this.Desc ? "down" : "up";
	this.Desc = !this.Desc; 
	} 
	o.onclick = function(){ to.Sort(order, odID); return false; } 
	arrOrder.push(o);
	} 
	function Getvalue(o){ 
	return o.getElementsByTagName("input")[0].value; 
	} 	
	SetOrder("idText1", { Index: 1 });
	SetOrder("idText2", { Index: 2 });
	SetOrder("idText3", { Index: 3 });
	SetOrder("idText4", { Index: 4 });
	SetOrder("idText5", { Index: 5 });
	SetOrder("idText7", { Index: 7, DataType: "int", GetValue: Getvalue });
	
</script>

</form>
</body>
