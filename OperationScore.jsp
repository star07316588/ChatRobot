這是jsp的內容 請根據剛才的專案規劃協助幫我改寫c# razor並建立成index.cshtml的內容，一併建立適當ViewModel，部分功能也可改放在Controller

<%--
'================================================================================
' file: operationScore.jsp
'--------------------------------------------------------------------------------
' Description:  術科成績鑑定
'--------------------------------------------------------------------------------
' Author:       Donk Huang,CIT on 2008/01/18
'================================================================================
--%>
<%@ page contentType="text/html; charset=Big5" pageEncoding="BIG5"%>
<%@ page errorPage="ExamErrMsg.jsp"%>
<jsp:useBean id="jdbc" scope="request" class="com.mxic.ses.db.DBAccess"/>
<jsp:setProperty name="jdbc" property="*" />

<%@ page import="com.mxic.ses.util.Property"%>
<%@ page import="com.mxic.ses.util.ScoreCompute"%>
<%@ page import="com.mxic.ses.util.*" %>
<%@ page import="com.mxic.ses.db.*"%>
<%@ page import="java.util.*"%>
<%@ page import="java.sql.*"%>
<%@ page session="false"%>
<%
  request.setCharacterEncoding("Big5");
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
//宣告公用參數
  Connection con = null;
  String sSql="";
  java.util.Vector oVector=null;
  java.sql.ResultSet oRS=null;
  int iSumWeight=0;
  con = DBConnection.getConnection();
//預防user修改PC時間值,自DB server取得今天日期
  sSql =" select to_char(sysdate,'YYYY/MM/DD') from dual";
  oRS=jdbc.queryData(sSql,con);
  oRS.next();
  String sToday = oRS.getString(1).trim();
  String sThisMonth= sToday.substring(0,7);

//備存user所選擇的[站別區域] & [術科試題代碼] & [工號] & [登入帳號] & [分數]
  String station_id = request.getParameter("station_id");
  if (station_id==null)
      station_id="";

  String cer_item_id_s = request.getParameter("cer_item_id_s");
  if (cer_item_id_s == null){
      cer_item_id_s = "";
  }else{
	  cer_item_id_s = cer_item_id_s.replace('@',' ');
  }

  String mEMP_id = request.getParameter("EMP_id");
  if(mEMP_id==null)
      mEMP_id="";

  String sUserId = (String)session.getAttribute("userid");
  if (sUserId==null)
      sUserId="";

  String sHdnScore = (String)request.getParameter("hdnScore");
  if (sHdnScore==null)
      sHdnScore="";
  
  String sExamRegNo = (String)request.getParameter("ExamRegNo");
  if(sExamRegNo==null)
	  sExamRegNo="";
%>

<html>
<head>
<title>術科成績登錄</title>

<link rel="stylesheet" href="../../ses.css">
</head>
<body  bgcolor="#FFFFFF" leftmargin="0" topmargin="0" marginwidth="0" marginheight="0">
<form id=frmOperationScore name=frmOperationScore action ="operationScore.jsp" method="POST">
<input type="hidden" id="hdnPassStatus" name="hdnPassStatus" value="NoPass">
<%
//[評分送出]update時機在整個網頁refresh時,下select語法之前
//if(request.getParameter("btnScore")!=null)
{
  String sPassStatus = (String)request.getParameter("hdnPassStatus");
  if (sPassStatus==null)
      sPassStatus="";

  if(sHdnScore.length() >0 && mEMP_id.length() >0 && sPassStatus.equals("CheckPass")){
	  String strExecType = "術科成績登錄";
	  String query_user = (String)session.getAttribute("userid");
	  
      java.util.ArrayList oAL=new ArrayList();
      oAL.add("成績="+sHdnScore+"\n");
      oAL.add("基礎考題");

      sSql =" SELECT type, no, subject, weight" +
            " from sbl_operation_spec" +
            " where cer_item_id = '" + cer_item_id_s + "'" +
            " and delete_flag = 'N'" +
            " order by type desc, to_number(replace(no,'-','.'))";
      oRS=jdbc.queryData(sSql,con);
	  int i = 0;	
      while(oRS.next()){
          if(oRS.getString("type").trim().toUpperCase().equals("BASIC")){
              oAL.add(oRS.getString("no").trim() + ".  " +
                      oRS.getString("subject").trim() + "  " +
                      oRS.getString("weight").trim() +"% , "+request.getParameter("oRadio"+i));
              i++;
          }
      }

      oAL.add("\n進階考題");
/*以下方法會使oRS發生[不正確的僅轉送結果集作業],故重新執行queryData
      oRS.first();
      oRS.absolute(1);
      oRS.beforeFirst();
*/
      oRS=jdbc.queryData(sSql,con);
      while(oRS.next()){
          if(oRS.getString("type").trim().toUpperCase().equals("ADV")){
              oAL.add(oRS.getString("no").trim() + ".  " +
                      oRS.getString("subject").trim() + "  " +
                      oRS.getString("weight").trim() +"% , "+request.getParameter("oRadio"+i));
              i++;
          }
      }

      //Modified by Jack on 2023/08/21 for record Rbl_Confidential_SysLog.  
      //要用 String crt_user = (String)session.getAttribute("userid");  ==> 登入系統者.   		
      //ScoreCompute oSC=new ScoreCompute();
      ScoreCompute oSC=new ScoreCompute("SES", query_user, strExecType);
      oSC.UpdateScore(sUserId,mEMP_id,cer_item_id_s,"OP",sHdnScore,oAL,sExamRegNo,station_id);
      mEMP_id="";
  }
}
%>

<table><td><img src="../images/title-blank.jpg"><td class="title">術科成績登錄</table>
<hr size="1" width="800" noshade align=left>

<TABLE cellSpacing=1 cellPadding=1 width="80%" border=0>
  <TR>
    <TD>站別區域：</TD>
    <TD><SELECT id=station_id name=station_id onChange=document.forms[0].submit()>
        <OPTION></OPTION>
<%
//站別區域/清單方塊
sSql = "SELECT DISTINCT station_id FROM sbl_station ORDER BY station_id";
oVector = jdbc.selectData(sSql,con);

for(int i=0;i<oVector.size();i++){
    if(oVector.get(i).toString().equals(station_id))
        out.print("<OPTION selected>" + station_id + "</OPTION>");
    else
        out.print("<OPTION>" + oVector.get(i).toString() + "</OPTION>");
}
%>
        </SELECT>
    </TD>

    <TD>術科試題代碼：</TD>
    <TD><SELECT id=cer_item_id_s name=cer_item_id_s>
		<OPTION></OPTION>
<%
//術科試題代碼/清單方塊
if(request.getParameter("station_id") !=null){
    sSql = " SELECT DISTINCT cer_item_id" +
           " FROM sbl_cer_item a," +
                 "sbl_course b" +
           " WHERE a.station_id ='" + station_id + "' AND " +
                  "a.cer_item_id=b.course_id AND " +
                  "b.exam_type like '%OP%' "+
                  //Added by Jack Hsieh 2009/09/11 
                  //未刪除者, 才顯示. a.dlt_date is null
                  " AND a.dlt_date is null " +
           " ORDER BY cer_item_id";
    oVector = jdbc.selectData(sSql,con);
    String strCer_Item_Id = null;

    for(int i=0;i<oVector.size();i++){
    	if(oVector.get(i).toString().equals(cer_item_id_s)){
    		strCer_Item_Id = cer_item_id_s.replaceAll(" ","@");
    	    out.print("<OPTION value="+strCer_Item_Id+" selected>" + cer_item_id_s + "</OPTION>");
    	}else{
    		strCer_Item_Id = new String(oVector.get(i).toString()).replaceAll(" ","@");
    	    out.print("<OPTION value="+strCer_Item_Id+">" + oVector.get(i).toString() + "</OPTION>");
    	}
    }
}
%>
	    </SELECT>
	</TD>
    <TD><INPUT type=submit size=26 value=搜尋 name=smtSearch></TD>
  </TR>
</TABLE>
<hr size="1" width="800" noshade align=left>

<%
sSql =" SELECT cer_desc" +
      " from sbl_cer_item" +
      " where cer_item_id = '" + cer_item_id_s + "'" +
      " and dlt_user is null";
oVector = jdbc.selectData(sSql,con);

//取第1筆的CertificateDescription,若sbl_cer_item.cer_desc不存在,則顯示空白
if(oVector.size()>0){
    try{
        out.print("<P><FONT size=5><STRONG>[" + oVector.get(0).toString() + "] 術科考題</STRONG></FONT></P>");
        out.print("<INPUT type=hidden name=cer_item_desc value=" + oVector.get(0).toString() + ">");
    }
    catch(NullPointerException e){
        out.print("<P><FONT size=5><STRONG>術科考題</STRONG></FONT></P>");
    }
}
else{
    out.print("<P><FONT size=5><STRONG>術科考題</STRONG></FONT></P>");
}
%>

<TABLE cellSpacing=1 cellPadding=1 width="80%" border=1>
  <TR>
    <TD><STRONG>考試日期：</STRONG></TD>
<%
//日期填入欄位中
out.print("<TD>" + sToday + "</TD>");
out.print("<INPUT type=hidden id=today name=today value=" + sToday + ">");

// user選擇站別/術科試題代碼兩個條件之後,
//串出sbl_cer_reg.cer_date為當月份且Score_Oper欄位為null的工號list, 給user選擇
Vector vUserGroup = Accessright.getGroup(sUserId.toUpperCase());
sSql=" SELECT dept_id,shift_id FROM sbl_emp WHERE emp_id='"+sUserId.toUpperCase()+"'";
oRS=jdbc.queryData(sSql,con);
String sShiftId="";
String sDept="";  
if(oRS.next()){
	sDept = oRS.getString("dept_id");
    sShiftId=oRS.getString("shift_id");
}
sSql =" SELECT DISTINCT b.shift_id, b.emp_id, b.name,a.cer_reg_no" +
      " FROM sbl_cer_reg a, sbl_emp b" +
      " WHERE a.cer_item_id ='" + cer_item_id_s + "'" +
      " AND to_date(to_char(a.cer_date,'YYYYMM'),'YYYY/MM') = to_date('" + sThisMonth + "','YYYY/MM')" +
      " AND a.score_oper is null" +
      " AND a.emp_id = b.emp_id"; 
  	  //Added by Jack on 2016/03/31 for JC201600028 <Start>
 	  //只帶指定站別的user.
if (request.getParameter("station_id") != null){ 	  
	sSql = sSql  + " and b.station_id = '" + request.getParameter("station_id") +"' " ;
} 	  
 	  //Added by Jack on 2016/03/31 for JC201600028 <End>

//Modified by Jack on 2020/04/17 for BE#202000102: 組織改組 (MK310/MK320) , 漏改Rule的需求 <Start>
//由於組織變更, 原本 MK310 HW 打 MK310 Prod, 因為 HW 歸為 MK320, 造成部門對不到.
//if(vUserGroup.contains("OpHW")){
//	sSql=sSql+ " AND b.shift_id='"+sShiftId+"'"+
// 	       	   " AND b.dept_id='"+sDept+"'";
//}
if(vUserGroup.contains("OpHW")){
	sSql=sSql+ " AND b.shift_id='"+sShiftId+"'";
}
//Modified by Jack on 2020/04/17 for BE#202000102: 組織改組 (MK310/MK320) , 漏改Rule的需求 <End>
sSql = sSql + " ORDER BY b.emp_id";

oVector = jdbc.selectData(sSql,con);

//[班別]資料填入欄位,[工號]選擇後再填入[班別]值
out.print("<TD><STRONG>班別：</STRONG></TD>");
if(mEMP_id.equals("")){
    out.print("<TD>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</TD>");
}
else{
    for(int i=0;i<oVector.size()/4;i++){
        if(oVector.elementAt(i*4+1).toString().trim().equals(mEMP_id)){
            out.print("<TD>" + oVector.elementAt(i*4).toString() + "</TD>");
            out.print("<INPUT type=hidden name=shift_id value=" + oVector.elementAt(i*4).toString() + ">");
            out.print("<INPUT type=hidden name=ExamRegNo value=" + oVector.elementAt(i*4+3).toString() + ">");
        }
    }
}

//[工號]資料填入欄位,若有值,則為下拉清單選取
out.print("<TD><STRONG>工號：</STRONG></TD>");
if (oVector.size() >0){
    out.print("<TD><SELECT id=EMP_id name=EMP_id onChange=document.forms[0].submit()>");
    out.print("<OPTION></OPTION>");

    for(int i=0;i<oVector.size()/4;i++){
        if(oVector.elementAt(i*4+1).toString().equals(mEMP_id))
            out.print("<OPTION selected> " + mEMP_id + "</OPTION>");
        else
            out.print("<OPTION> " + oVector.elementAt(i*4+1).toString() + "</OPTION>");
    }
    out.print("</TD>");
}
else{
    out.print("<TD>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</TD>");
}

//[姓名]資料填入欄位,[工號]選擇後再填入[姓名]值
out.print("<TD><STRONG>姓名：</STRONG></TD>");
if(mEMP_id.equals(""))
{
    out.print("<TD>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</TD>");
}
else
{
    for(int i=0;i<oVector.size()/4;i++)
    {
        if(oVector.elementAt(i*4+1).toString().trim().equals(mEMP_id)){
            out.print("<TD>" + oVector.elementAt(i*4+2).toString() + "</TD>");
            out.print("<INPUT type=hidden name=EMP_name value=" + oVector.elementAt(i*4+2).toString() + ">");
        }
    }
}
%>
    <TD><STRONG>總分：</STRONG></TD>
    <TD><P id=oScore name=oScore>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</P></TD>
  </TR>
</TABLE>

<%
sSql =" SELECT type, no, subject, weight" +
      " from sbl_operation_spec" +
      " where cer_item_id = '" + cer_item_id_s + "'" +
      " and delete_flag = 'N'" +
      " order by type desc, to_number(replace(no,'-','.'))";
oVector = jdbc.selectData(sSql,con);

//累計[基礎考題]的百分比的權重
iSumWeight=0;
for(int i=0;i<oVector.size()/4;i++)
{
    if(oVector.elementAt(i*4).toString().trim().toUpperCase().equals("BASIC"))
        iSumWeight=iSumWeight + Integer.parseInt(oVector.elementAt(i*4+3).toString());
}
%>
<P><FONT size=5><STRONG>基礎考題<%=iSumWeight%>%(單項分數未達60即屬不及格)</STRONG></FONT></P>
<TABLE cellSpacing=1 cellPadding=1 width="80%" border=1 id=tblBasic name=tblBasic>
  <TR>
    <TD>&nbsp;&nbsp;&nbsp;</TD>
    <TD width="75%"><P align=center><FONT size=4><STRONG>題目</STRONG></FONT></P></TD>
    <TD><P align=center><FONT size=4><STRONG>權重</STRONG></FONT></P></TD>
    <TD><P align=center><FONT size=4><STRONG>100</STRONG></FONT></P></TD>
    <TD><P align=center><FONT size=4><STRONG>80</STRONG></FONT></P></TD>
    <TD><P align=center><FONT size=4><STRONG>60</STRONG></FONT></P></TD>
    <TD FONT style="BACKGROUND-COLOR: khaki"><P align=center><FONT size=4><STRONG>0</STRONG></FONT></FONT></P></TD>
  </TR>
<%
//題目填入欄位中
for(int i=0;i<oVector.size()/4;i++)
{
    if(oVector.elementAt(i*4).toString().trim().toUpperCase().equals("BASIC"))
    {
%>
  <TR>
    <TD><P align=center><%=oVector.elementAt(i*4+1).toString()%></P></TD>
    <TD><P align=left id=oSubject<%=i%> name=oSubject<%=i%> ><%=oVector.elementAt(i*4+2).toString().replaceAll("\n","<BR>")%></P></TD>
    <TD><P align=left><%=oVector.elementAt(i*4+3).toString()+"%"%></P></TD>
    <TD><P align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value=<%=100 * Integer.parseInt(oVector.elementAt(i*4+3).toString())/100%> onclick="DetectRadio()"></P></TD>
    <TD><P align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value=<%= 80 * Integer.parseInt(oVector.elementAt(i*4+3).toString())/100%> onclick="DetectRadio()"></P></TD>
    <TD><P align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value=<%= 60 * Integer.parseInt(oVector.elementAt(i*4+3).toString())/100%> onclick="DetectRadio()"></P></TD>
    <TD><P align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value="零分" onclick="DetectRadio()"></P></TD>
  </TR>
<%}}//For & If的大括號%>
</TABLE>

<%
//累計[進階考題]的百分比的權重
iSumWeight=0;
for(int i=0;i<oVector.size()/4;i++)
{
    if(oVector.elementAt(i*4).toString().trim().toUpperCase().equals("ADV"))
        iSumWeight=iSumWeight + Integer.parseInt(oVector.elementAt(i*4+3).toString());
}
%>
<P><FONT size=5><STRONG>進階考題<%=iSumWeight%>%</STRONG></FONT></P>
<TABLE cellSpacing=1 cellPadding=1 width="80%" border=1 id=tblAdvance name=tblAdvance>
  <TR>
    <TD>&nbsp;&nbsp;&nbsp;</TD>
    <TD align=center width="75%"><FONT size=4><STRONG>題目</STRONG></FONT></TD>
    <TD align=center><FONT size=4><STRONG>權重</STRONG></FONT></TD>
    <TD align=center><FONT size=4><STRONG>100</STRONG></FONT></TD>
    <TD align=center><FONT size=4><STRONG>80</STRONG></FONT></TD>
    <TD align=center><FONT size=4><STRONG>60</STRONG></FONT></TD>
    <TD FONT style="BACKGROUND-COLOR: khaki"><P align=center><FONT size=4><STRONG>0</STRONG></FONT></P></TD>
  </TR>

<%
//題目填入欄位中
for(int i=0;i<oVector.size()/4;i++)
{
    if(oVector.elementAt(i*4).toString().trim().toUpperCase().equals("ADV"))
    {
%>
  <TR>
    <TD align=center><%=oVector.elementAt(i*4+1).toString()%></TD>
    <TD align=left id=oSubject<%=i%> name=oSubject<%=i%> ><%=oVector.elementAt(i*4+2).toString().replaceAll("\n","<BR>")%></TD>
    <TD align=left><%=oVector.elementAt(i*4+3).toString()+"%"%></TD>
    <TD align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value=<%=100 * Integer.parseInt(oVector.elementAt(i*4+3).toString())/100%> onclick="DetectRadio()"></TD>
    <TD align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value=<%= 80 * Integer.parseInt(oVector.elementAt(i*4+3).toString())/100%> onclick="DetectRadio()"></TD>
    <TD align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value=<%= 60 * Integer.parseInt(oVector.elementAt(i*4+3).toString())/100%> onclick="DetectRadio()"></TD>
    <TD align=center><INPUT type=radio id=oRadio<%=i%> name=oRadio<%=i%> value=0 onclick="DetectRadio()"></TD>
  </TR>
<%  }
}
/*
if(oRS!=null){
  oRS.close();
}
if(con!=null){
  con.close();
}
*/
//JC900149 Jack 2009/10/21 Fix connection leak issue.
try{
    jdbc.close(); 
    if(oRS!=null){
    	oRS.close();
    }
    if(con!=null){
    	//JC900149 Jack 2009/10/21 Fix connection leak issue.
    	DBConnection.close(con);
    	if(con != null){
    		con.close();
    		con = null;
    	}
    }
}catch(SQLException sqle){
	sqle.printStackTrace();
    if(oRS!=null){
    	oRS.close();
    	oRS = null;
    }
	if(con!=null){
		con.close();
		con = null;
	}
}

%>
</TABLE>

<hr size="1" width="800" noshade align=left>
<INPUT type=button size=26 value=評分送出 id=btnScore name=btnScore onclick="SendCheck()">
<INPUT type=hidden id=hdnScore name=hdnScore value="">

<script language="JavaScript">
function DetectRadio()
{
    var iScore=0;

    for(i=0;i< <%= oVector.size()/4 %>;i++){
        var oR = document.getElementsByName("oRadio" + i);
        for(j=0; j<oR.length; j++){
            if (oR.item(j).checked == true)
                iScore += parseInt(oR.item(j).value);
        }
    }

    //若value="零分",則iScore=NaN=Not a Number,表示[基礎考題]/單項分數未達60即零分
    if(!parseInt(iScore))
        iScore=0;

    document.getElementsByName("oScore").item(0).innerText=iScore;
    document.getElementsByName("hdnScore").item(0).value=iScore;
}

function SendCheck()
{
    var bSendCheckPass=true;
    var bConfirmPass=true;
    var sMessage="";

    DetectRadio();

    for(i=0;i< <%=oVector.size()/4%>;i++){
        var oR=document.getElementsByName("oRadio" +i);
        var bRadioChoice=false;

        for(j=0;j<oR.length;j++){
            if(oR.item(j).checked==true)
                bRadioChoice=true;
        }

        if(bRadioChoice==false){
            var oS=document.getElementsByName("oSubject" +i);
            sMessage+="題目:" + oS.item(0).innerText +",未評分!\n";
            bSendCheckPass=false;
        }
    }

    var sScore=document.getElementsByName("hdnScore").item(0).value;
    if(sMessage!=""){
        sMessage="尚有題目未評分!\n目前評分:"+sScore+",是否仍送出成績?";
        bConfirmPass=confirm(sMessage);
        if(bConfirmPass)
            bSendCheckPass=true;
        else
            bSendCheckPass=false;
    }

    var oE=document.getElementById("EMP_id");
    if(oE!=null){
        if(oE.options[0].selected==true){
            alert("工號未選擇!");
            bSendCheckPass=false;
        }
    }

    if(sScore=="")
        bSendCheckPass=false;

    if(bSendCheckPass==true){
        document.getElementById('hdnPassStatus').value="CheckPass";
        document.frmOperationScore.submit();
    }
}
</script>

</form>
</body>
</html>
