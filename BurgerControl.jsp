<%@ page import="java.util.*"%>
<%@ page import="com.mxic.dl.util.Accessright"%>
<%@ page language="java" contentType="text/html; charset=big5" pageEncoding="Big5" %>
<jsp:useBean id="jdbc" scope="request" class="com.mxic.dl.tool.DBAcc"/>
<%@ taglib uri="http://java.sun.com/jstl/core" prefix="c" %>
<%@ page import="java.sql.*"%>
<%@ page import="com.mxic.dl.tool.*"%>
<%@ page import="com.mxic.dl.worker.ReportDAO"%>
<%@ page import="com.mxic.ses.util.SESDB"%>
<%@ page session="false"%>
<%
  HttpSession session = request.getSession(false);
  if (session==null) {
    response.sendRedirect(Property.getRoot()+"timeout.jsp?msg=Timout please login again");
  }
  else {
    String op = (String)session.getAttribute("op");
    if(op==null)
      response.sendRedirect(Property.getRoot()+"timeout.jsp?msg=Session is timeout");
    if (op.equals("N"))
        response.sendRedirect(Property.getRoot()+"timeout.jsp?msg=No Access Right");
}  
%>
<%
Connection con=null;
ArrayList years = new ArrayList();

HashMap loginUser=null;
HashMap[] data=null;
HashMap[] shift_id=null;
HashMap[] title=null;
String startyear="";
try{
	con = DBConn.getConnection();
	ReportDAO dao = new ReportDAO();
	  years = dao.getYear(con);	  
	  loginUser = dao.getUser(con,(String)session.getAttribute("userid"));
	if (request.getParameter("startyear")== null )
		request.setAttribute("startyear", dao.getCurrentDate(con,"y",0));	
	else
    	request.setAttribute("startyear", request.getParameter("startyear"));
			
	request.setAttribute("loginUser", loginUser);
	request.setAttribute("years", years);	
	startyear =request.getParameter("startyear");
	if(!"".equals(startyear) && startyear !=null){
		data=dao.getBudgetData(con,request.getParameter("startyear"),loginUser.get("DEPT_ID")+"",loginUser.get("EMP_ID")+"","產線績效預算年度管控表");
		request.setAttribute("data", data);
		shift_id=dao.getBudgetShift(con,loginUser.get("DEPT_ID")+"");
		request.setAttribute("shift_id", shift_id);
		title=dao.getBudgetTitle(con,loginUser.get("DEPT_ID")+"");
		request.setAttribute("title", title);
	}
			
}catch(SQLException e){
	e.printStackTrace();
	request.setAttribute("error", e.getMessage());
	
}catch(Exception e){
	e.printStackTrace();
	request.setAttribute("error", e.getMessage());
	
}finally{
	DBConn.close(con);
}
%>

<style type="text/css">
div
	{
	width:100%;
	height:540;
	position:absolute;
	margin:auto;		
	overflow:scroll;
	}
</style>

<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=big5" />
<title>Function Access</title>

<link rel="stylesheet" href="<c:url value="/DLOperation/css/report.css"/>">
</head>
<c:if test="${!empty error}">
<script>alert("<c:out value="${error }"/>");</script>
</c:if>
<body bgcolor="#FFFFFF" leftmargin="0" topmargin="0" marginwidth="0" marginheight="0">
<noscript><iframe src=*.htm></iframe></noscript> <!-- 20140710 禁止存檔 -->
<form action="BudgetControl.jsp" method="POST" >
<h1 class="mainTitle">產線績效預算年度管控表</h1>
        <table width="100%"  align="center" cellpadding='0' cellspacing='0'  border=1 bordercolor='#6BDC00'>
         <tr class='reportTitle'>
            <td > 年度</td>            
            <td > 部門 Dept</td>
            <td > &nbsp;</td>
          </tr>          
          <tr class='reportSearch'>
           <td > 
			<select name="startyear">
				<c:forEach items="${years}" var = "item" varStatus="st">
                	<option value="<c:out value="${item}"/>" <c:if test="${item==startyear }">selected</c:if>><c:out value="${item}"/></option>
                </c:forEach>
			</select>
            </td>                                    
            <td > <input type="hidden" name ="deptId" value="<c:out value="${loginUser.DEPT_ID}"/>"/>
            	   <c:out value="${loginUser.DEPT_ID}"/>
            </td>            
            <td style="text-align:left">&nbsp;&nbsp;&nbsp;&nbsp; 
            <input type="image" name="Image5" src="../images/search.gif" onclick="document.form[0].submit();" ></td>
          </tr>         
        </table>
        <c:if test="${!empty startyear}">
	    <div width="100%" align="center" style="border:0;height:540;overflow:scroll "> 		
        <table width="100%"  align="center" cellpadding='0' cellspacing='0'  border=1 bordercolor='#6BDC00'>
          <tr class='reportContentTitle'>
            <td rowspan=2 > 班別</td>
            <td rowspan=2 > 職稱</td>
            <td rowspan=2  > 費用 </td>
            <td colspan=12 width='84%'> 月份</td>            
          </tr>
          <tr class='reportContentTitle'>
            <td width='7%'> 1</td>
            <td width='7%'> 2</td>
            <td width='7%'> 3</td>
            <td width='7%'> 4</td>
            <td width='7%'> 5</td>
            <td width='7%'> 6</td>
            <td width='7%'> 7</td>
            <td width='7%'> 8</td>
            <td width='7%'> 9</td>
            <td width='7%'> 10</td>
            <td width='7%'> 11</td>
            <td width='7%'> 12</td>            
          </tr>          
          
          <c:forEach items="${data}" var = "item" varStatus="st">          
          <tr class='reportItemTr'>
	          <td > <c:out value="${item.SHIFT_ID}" escapeXml="false" /><c:if test="${empty item.SHIFT_ID}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.TITLE}" escapeXml="false" /><c:if test="${empty item.TITLE}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.TYPE}" escapeXml="false" /><c:if test="${empty item.TYPE}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.JAN}" escapeXml="false" /><c:if test="${empty item.JAN}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.FEB}" escapeXml="false" /><c:if test="${empty item.FEB}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.MAR}" escapeXml="false" /><c:if test="${empty item.MAR}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.APR}" escapeXml="false" /><c:if test="${empty item.APR}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.MAY}" escapeXml="false" /><c:if test="${empty item.MAY}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.JUN}" escapeXml="false" /><c:if test="${empty item.JUN}">&nbsp;</c:if></td>	          
	          <td > <c:out value="${item.JUL}" escapeXml="false" /><c:if test="${empty item.JUL}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.AUG}" escapeXml="false" /><c:if test="${empty item.AUG}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.SEP}" escapeXml="false" /><c:if test="${empty item.SEP}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.OCT}" escapeXml="false" /><c:if test="${empty item.OCT}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.NOV}" escapeXml="false" /><c:if test="${empty item.NOV}">&nbsp;</c:if></td>
	          <td > <c:out value="${item.DEC}" escapeXml="false" /><c:if test="${empty item.DEC}">&nbsp;</c:if></td>	          
            </tr>
	      </c:forEach>
        </table>
        </div>
        </c:if>
</form>
</body>
</html>
