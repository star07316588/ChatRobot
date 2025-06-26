<%@ page contentType="text/html; charset=big5" %>
<%@ page errorPage="ExamErrMsg.jsp" %>
<jsp:useBean id="jdbc" scope="request" class="com.mxic.ses.db.DBAccess"/>
<jsp:setProperty name="jdbc" property="*" />
<%@ page import="java.util.*,java.sql.*,com.mxic.ses.db.*,com.mxic.ses.worker.*"%>
<%@ page import="com.mxic.ses.util.Property"%>
<%@ page import="com.mxic.ses.util.ExamInfo"%>
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
  String strExecType = "OnlineTest_Query";
  String query_user = (String)session.getAttribute("userid");

String userId=(String)session.getAttribute("userid");
if(userId==null)
    userId="";
String sExamStatus=request.getParameter("hdnExamStatus");
if(sExamStatus==null)
    sExamStatus="";
String EmpId=request.getParameter("txtEmp_id");
if(EmpId==null)
    EmpId="";
String CerItemId=request.getParameter("sltCerItemId");
if(CerItemId==null){
    CerItemId="";
}else{
	CerItemId = CerItemId.replace('@',' ');
}
String sStationId=request.getParameter("sltStationId");
if(sStationId==null){
	sStationId="";
}
String sExamRegNo=request.getParameter("sExamRegNo");//考試注冊代碼
if(sExamRegNo==null){
	sExamRegNo="";
}

%>

<html>
<head>
<title>Maintain Course</title>
<meta http-equiv="Content-Type" content="text/html; charset=big5">
<link rel="stylesheet" href="../../ses.css">

<script language="JavaScript">
function getEmpId()
{
 var empId=document.getElementById("txtEmp_id").value;
 javascript:location.href='onlineExam.jsp?txtEmp_id='+empId;
}
function getStationId()
{
 var empId=document.getElementById("txtEmp_id").value;
 var cer_item_id=document.getElementById("sltCerItemId").value;
 javascript:location.href='onlineExam.jsp?txtEmp_id='+empId+'&sltCerItemId='+cer_item_id;
}

function check()
{
 var flag=document.getElementById('hdnExamStatus').value;

 if(flag=="finished")
    return true;
 else
    return false;
//    alert("並無任何考試!\n No any Test!");
}

function startExam()
{
  var emp_id=document.getElementById("txtEmp_id").value;
  var cer_item_id=document.getElementById("sltCerItemId").value;
  var station_id=document.getElementById("sltStationId").value;
  var sExamRegNo=document.getElementById("ExamRegNo").value;
  if(emp_id=="")
    alert("錯誤,工號不能為空白!\n Notice!EMP id is empty!");
  else if(cer_item_id=="")
    alert("錯誤,試題項目代碼不能為空白!\n Notice! Course is empty!");
  else
    javascript:location.href='onlineExam.jsp?hdnExamStatus=testing'+'&txtEmp_id='+emp_id+'&sltCerItemId='+cer_item_id+'&sltStationId='+station_id+'&sExamRegNo='+sExamRegNo;
}

function FinishExam(evt)
{
    var sExamStatus=document.getElementById('hdnExamStatus').value;
    var evt  = (evt) ? evt : ((event) ? event : null);
	var node = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null);
	if ((evt.keyCode == 13) && (node.type=="text")) { return false; }
    
    if(sExamStatus=="testing"){
        document.getElementById('hdnExamStatus').value='finished';//[交卷]後,考試狀態設為finished        
		document.Exam.txtEmp_id.disabled=!(document.Exam.txtEmp_id.disabled);        
		document.Exam.sltCerItemId.disabled=!(document.Exam.sltCerItemId.disabled);
		document.Exam.sltStationId.disabled=!(document.Exam.sltStationId.disabled);
		document.Exam.submit();
	}
}

function getButton()
{
  if(event.button==2)
    alert("滑鼠右鍵失效!\n Mouse right click was useless!");
}
//document.onmousedown=getButton;

function CheckExamStatus()
{
    var sExamStatus=document.getElementById('hdnExamStatus').value;

    if(sExamStatus=="testing"){
		event.returnValue ="[確定]:放棄本次考試,評分將以零分計算!\n"+
						   "[取消]:繼續考試,回到線上考試畫面\n\n"+
                   	 	   "Please confirm and abandon this test?\n"+
 						   "Press [確定(OK)],the socre will be 0!\n"+
                           "press [取消(Cancel)],continue test";
   }
}
</script>
<script language="javascript" type="text/javascript">
function stopRKey(evt) {
	var evt  = (evt) ? evt : ((event) ? event : null);
	var node = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null);
	if ((evt.keyCode == 13) && (node.type=="text")) { return false; }
}
document.onkeypress = stopRKey;
</script>
</head>
<body bgcolor="#FFFFFF" leftmargin="0" topmargin="0" marginwidth="0" marginheight="0" onbeforeunload="CheckExamStatus()">
<form id="Exam" name="Exam" action="pageExamList.jsp" method="post" onSubmit="return check()">
<input type="hidden" id="hdnExamStatus" name="hdnExamStatus" value="waiting">
<input type="hidden" id="spendTime" name="spendTime" value="0">
<input type="hidden" id="allTime" name="allTime" value="0">

 <table width="600" border="0" cellspacing="0" cellpadding="5" height="174">
    <table><td><img src="../images/title-blank.jpg"><td class="title">線上考試</table>
    <tr>
      <td colspan="120"><font size="5" color="red"><div id="showTime" width="800" align="right"></div></font></td>
    </tr>
    <tr valign="top">
      <td height="75">
        <hr size="1" noshade>
        <table width="600" cellpadding="0" cellspacing="0" border="1" bordercolor="black" align="center">
          <tr>
            <td width="120" bgcolor="#666666">
              <div align="center"><font color="#FFFFFF"><b>工號 EMP ID</b></font></div>
            </td>
            <td width="120" bgcolor="#FFFFFF">
            <div align="center"><font size="2">
              <input type="text" name="txtEmp_id" id="txtEmp_id" size="10" value="<%=userId%>" disabled='true' onChange="getEmpId()">
            </font></div>
            </td>
             <td width="200" bgcolor="#666666">
              <div align="center"><font color="#FFFFFF"><b>試題項目代碼 Course</b></font></div>
            </td>
            <td width="80" bgcolor="#FFFFFF">
              <div align="center"><font size="3">
               <select id="sltCerItemId" name="sltCerItemId" onChange=getStationId()>               	
<%
//由工號帶出試題項目代碼 & 考題時間
Connection con = null;
ResultSet rs = null;
ResultSet rs2 = null;
ResultSet rs3 = null;
Statement stmt = null;
String strCerItemId = null;
con = DBConnection.getConnection();
String sEmpStationId =null;//Added by Jack on 2016/03/31 for JC201600028.
if(userId!=null){
    String sql= " SELECT a.cer_item_id,a.cer_reg_no,b.exam_time" +
	            " FROM sbl_cer_reg a,sbl_course b" +
                " WHERE a.emp_id = '"+userId+"'" +
                " AND a.score_writing is null" +
                " AND to_char(a.cer_date, 'yyyymm') = to_char(sysdate, 'yyyymm')" +
                " AND a.cer_item_id = b.course_id ";
	if(!CerItemId.equals("")){
                sql = sql +" AND a.cer_item_id = '" +CerItemId + "'";
	}
                sql = sql +" AND b.EXAM_TYPE LIKE 'WR%'"+
                " ORDER BY cer_item_id";
    rs = jdbc.queryData(sql,con);
	if(CerItemId.length()==0){
		out.print("<option value=''>&nbsp;</option>");
	    while(rs.next()){
	    	strCerItemId = rs.getString("cer_item_id").replaceAll(" ","@");
	        out.print("<option value='"+strCerItemId+"'>"+rs.getString("cer_item_id")+"</option>");
	    }
	}else{
		strCerItemId = CerItemId.replace('@',' ');
		out.print("<option value='"+strCerItemId+"'>"+CerItemId+"</option>");
	}
    int iExam_Time=0;
    int iCerItemCount=0;

    //Modified by Jack on 2023/08/21 for record Rbl_Confidential_SysLog.  
    //要用 String crt_user = (String)session.getAttribute("userid");  ==> 登入系統者.   		
    //rs=jdbc.queryData(sql,con);
    rs=jdbc.queryData(sql,con, "SES", query_user, strExecType+"_1");
    
    //rs.first();//發生[不正確的僅轉送結果集作業: first]
    while(rs.next()){
        iExam_Time=rs.getInt("exam_time");//考試時間,DB中則為null,則iExam_Time=0
        if(iExam_Time==0)
            iExam_Time=600;//若無考試時間,預計時間為600=10分鐘
        else
            iExam_Time=iExam_Time*60;

        //ExamItem & ExamTime 當做2維陣使用,試題項目代碼對齊該考題時間
        strCerItemId = rs.getString("cer_item_id").replaceAll(" ","@");
        //out.print("<input type=hidden  name=ExamItem value="+rs.getString("cer_item_id")+">");
        out.print("<input type=hidden  name=ExamItem value="+strCerItemId+">");
        out.print("<input type=hidden  name=ExamTime value="+iExam_Time+">");
        out.print("<input type=hidden  name=ExamRegNo value="+rs.getString("cer_reg_no")+">");
        iCerItemCount++;
    }

    if(iCerItemCount==0){
        out.print("<option>無 Empty</option>");
        out.print("<input type=hidden  name=ExamItem value=''>");
        out.print("<input type=hidden  name=ExamTime value=''>");
        out.print("<input type=hidden  name=ExamRegNo value=''>");
    }
    out.print("<input type=hidden  name=sExamRegNo value=''");
}
%>
               </select>
               </font></div>
            </td>
            <td  width="200" bgcolor="#666666"><div align="center"><font color="#FFFFFF"><b>站別</b></font></div></td>            
            <td>
              <div align="center"><font size="3">
               <select id="sltStationId" name="sltStationId">
<%
if(!userId.equals("") && !CerItemId.equals("")){
	//Added by Jack on 2016/03/31 for JC20160002 <Start>
	//預設將站點選取User當站(Ex : V/W, 選'W').
	//<1> 有user的站點時, 預設帶user的站點。
	//<2> 沒有user的站點時, 預設帶空白。
	String sql= null;
	sql= " select station_id from sbl_emp where emp_id = '"+userId+"' ";
	rs = jdbc.queryData(sql,con);
	if (rs.next()){
		sEmpStationId = rs.getString("station_id");
	}
	//Added by Jack on 2016/03/31 for JC20160002 <End>
	
    sql= " SELECT a.station_id" +
    " FROM sbl_cer_item a" +
    " WHERE a.cer_item_id = '"+CerItemId+"'" +
    "   AND a.dlt_user is null "+
    "   AND a.dlt_date is null "+
    " ORDER BY a.cer_item_id";
    //Modified by Jack on 2023/08/21 for record Rbl_Confidential_SysLog.  
    //要用 String crt_user = (String)session.getAttribute("userid");  ==> 登入系統者.   		
	//rs = jdbc.queryData(sql,con);
	rs = jdbc.queryData(sql,con, "SES", query_user, strExecType+"_2");
	if(sStationId.length()==0){
		out.print("<option value='' selected> </option>");//------
		
		while(rs.next()) 
			if (sEmpStationId!= null && rs.getString("station_id").equalsIgnoreCase(sEmpStationId)){
				out.print("<option value='"+rs.getString("station_id")+"' selected>"+rs.getString("station_id")+"</option>");
			}else{
				out.print("<option value='"+rs.getString("station_id")+"'>"+rs.getString("station_id")+"</option>");
			}
	}else{
		out.print("<option value='"+sStationId+"'>"+sStationId+"</option>");
	}
}
%>               
               </select>
               </font></div>            
            </td>
            <td>
              <input type="button" value="開始測驗 Start exam" id="Bnt" onClick="startExam()">              
            </td>
          </tr>
        </table>
    </table>
<br>
<%
Vector QuesA=new Vector();
Vector QuesB=new Vector();
if(sExamStatus.equals("testing"))
{
    /*
    ********************************
    先進去 sbl_cer_reg 找看有無考試日期登錄
    *********************************
    */
    int totalQues=0;
    String sql=" SELECT emp_id FROM sbl_cer_reg"+
               " WHERE emp_id='"+EmpId+"'"+
               " AND cer_item_id='"+CerItemId+"'"+
               " AND to_char(wr_date,'yyyymm')=to_char(sysdate,'yyyymm')"+
               " AND to_char(cer_date,'yyyymm')=to_char(sysdate,'yyyymm')";
    //Modified by Jack on 2023/08/21 for record Rbl_Confidential_SysLog.  
    //要用 String crt_user = (String)session.getAttribute("userid");  ==> 登入系統者.   		
    //rs=jdbc.queryData(sql,con);
    rs=jdbc.queryData(sql,con, "SES", query_user, strExecType+"_3_1");

    //進入題庫看有無考題
    //Type:1-->是非題,2-->選擇,3-->連連看,4-->必考    
    sql=" SELECT COUNT(cer_item_id) AS total"+
        " FROM sbl_question_spec"+
        " WHERE type IN ('1','2','3','4')"+
        " AND cer_item_id='"+CerItemId+"'";
    //rs2=jdbc.queryData(sql,con);
    rs2=jdbc.queryData(sql,con, "SES", query_user, strExecType+"_3_2");
    rs2.next();
    totalQues=rs2.getInt("total");

    //判斷鑑定考試只能當月考,不可跨月考
    sql=" SELECT emp_id FROM sbl_cer_reg"+
        " WHERE emp_id='"+EmpId+"'"+
        " AND cer_item_id='"+CerItemId+"'"+
        " AND to_char(cer_date,'yyyymm')=to_char(sysdate,'yyyymm')";
    //rs2=jdbc.queryData(sql,con);
    rs2=jdbc.queryData(sql,con, "SES", query_user, strExecType+"_3_3");

    if(!rs2.next()){
        out.print("試題項目代碼(Course):"+CerItemId+"<BR>本月份並無考試!This month don't test.");
        rs2.close();
    }
    else if(rs.next())//如果有表示成績已經登錄
        out.print("成績已登錄,目前尚無考試! <BR> Test already finshed.");
    else if(totalQues==0)//如果total question==0 表示該考試科目目前在sbl_question_spec並無考題
        out.print("題庫目前並無考題! <BR> Course not found!");
    else{
        //去sbl_course撈出選擇是非、選擇要出幾題 & 考試時間
        sql="select sample_choice_sum from sbl_course where course_id='"+CerItemId+"'";
        //rs=jdbc.queryData(sql,con);
        rs=jdbc.queryData(sql,con, "SES", query_user, strExecType+"_3_4_choice");

        int choice_sum=0;
        if(rs.next())
            choice_sum=rs.getInt("sample_choice_sum");//總共要出5題是非跟選擇
        else
            out.print("科目:"+CerItemId+",抽樣題數未設定! <BR> Please inform Administrator to fix problem");

        boolean chcQus[]=new boolean[totalQues];
        //撈出是非、選擇題目及其答案
        sql=" SELECT subject,answer,type AS ExamType,"+
            " (SELECT count(*) FROM sbl_question_spec WHERE type IN ('1','2') AND cer_item_id='"+CerItemId+"') total"+ 
            " FROM sbl_question_spec"+
            " WHERE type IN ('1','2')"+
            " AND cer_item_id='"+CerItemId+"'"+
            " ORDER BY type,to_number(replace(no,'-','.'))";
        stmt = con.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,ResultSet.CONCUR_READ_ONLY);
        rs3 = stmt.executeQuery(sql);
        int check_list = BADAO.insertConfidential_SysLog(con, "SES", query_user, strExecType+"_3_5", sql);
        //rs=jdbc.queryData(sql,con); mark by Nelson on 2008/3/17
		if(rs3.next()){
			totalQues = rs3.getInt("total");
		}
		int counter=0,i=0,j=0,x=0,y=0,chcNum=0;
        if(totalQues>0){//有是非選擇題庫時，才隨機挑選題目	        
	        int seeds=totalQues-1;
	        //初始化題目
	        int[] choiceList=new int[totalQues]; //用一個陣列存總題數(例:8題)
	        for(i=0;i<totalQues;i++)
	            choiceList[i]=i+1;
	
	        //亂數產出題目並打亂排序
	        for(i=0;i<totalQues;i++)
	         {
	          y=(int)(Math.random()*seeds)+1;
	          x=choiceList[i];
	          choiceList[i]=choiceList[y];
	          choiceList[y]=x;
	         }
	
	         x=0;
	         y=0;
	        //開始選題目(選六題出來)
	        for(i=0;i<choice_sum;i++)
	        {
	          rs3.absolute(choiceList[i]);
	          String type=rs3.getString("ExamType");
	          String Ans=rs3.getString("answer");
	          String Qus=rs3.getString("subject");
	
	          if(type.equals("1"))
	             {
	              QuesA.add(Qus);
	              x=x+1;
	              session.setAttribute("QusA"+x,Qus);
	              session.setAttribute("AnsA"+x,Ans);
	             }
	          else
	             {
	              QuesB.add(Qus);
	              y=y+1;
	              session.setAttribute("QusB"+y,Qus);
	              session.setAttribute("AnsB"+y,Ans);
	             }
	        }
	        rs3.close();
	        rs3=null;
	        stmt.close();
        }
        //撈出必考題題目
        sql=" SELECT subject, answer"+
            " FROM sbl_question_spec"+
            " WHERE type IN ('4')"+
            " AND cer_item_id='"+CerItemId+"'"+
            " ORDER BY type,to_number(replace(no,'-','.'))";
        //rs=jdbc.queryData(sql,con);
        rs=jdbc.queryData(sql,con, "SES", query_user, strExecType+"_3_6_must");
//<table width="800" border="1" cellspacing="0" cellpadding="5" height="74">
//<table width="600" cellpadding="0" cellspacing="0" border="1" bordercolor="black" align="center">
//<TABLE cellSpacing=0 cellPadding=0 width="80%" border=1 bordercolor=black>
%>
        <br>
        <b>必考題 Necessary question:</b>
        <table width="800" border="1" cellspacing="0" cellpadding="0" bordercolor="black">
          <table>
<%
        i=1;
		y=0;
        while(rs.next()){
        	//add by Nelson start on 2008/03/18
			y++;        	
            String Ans4=rs.getString("answer");
            String Qus4=rs.getString("subject");        	
            session.setAttribute("Qus4"+y,Qus4);
            session.setAttribute("Ans4"+y,Ans4);        
			//add by Nelson end on 2008/03/18
%>
              <tr>
                <td>
                    &nbsp;&nbsp;
                    <INPUT type=text name=txtNecessary value="" maxLength=1 size=2>
                    <input type="hidden" id="hdnNeceAns" name="hdnNeceAns" value=<%=rs.getString("answer")%>>
                </td>

                  <td><%=i++%>.<%=rs.getString("subject").replaceAll("\n","<BR>")%></td>
              </tr>
<%
        }
%>
          </table>
        </table>
        <br>
        <hr size="1" noshade>
        <b>1.是非題 Right and wrong question:</b>
        <table width="800" border="1" cellspacing="0" cellpadding="0" bordercolor="black">
          <table>
<%
        for( i=0;i<QuesA.size();i++){
            String subject=(String)QuesA.elementAt(i);
%>
            <tr>
              <td>
                &nbsp;&nbsp;
                <select name="TF"><option value="T">○</option>
                                  <option value="F">╳</option>
              </td>
              <td><%=i+1%>.<%=subject.replaceAll("\n","<BR>")%></td>
            </tr>
<%
        }//for( i=0;i<QuesA.size();i++){
%>
          </table>
        </table>

        <br>
        <hr size="1" noshade>
        <b>2.選擇題 Select question:</b>
        <table width="800" border="1" cellspacing="0" cellpadding="0" bordercolor="black">
          <table>
<%
        for( i=0;i<QuesB.size();i++){
            String subject=(String)QuesB.elementAt(i);
%>
            <tr>
              <td>
                &nbsp;&nbsp;
                <INPUT type=text name=Choose value="" maxLength=1 size=2>
              </td>
              <td><%=i+1%>.<%=subject.replaceAll("\n","<BR>")%></td>
            </tr>
<%
        }//for( i=0;i<QuesB.size();i++){
%>
          </table>
        </table>

        <br>
        <hr size="1" noshade>
        <b>3.連連看 Link question:</b>
        <table  width="1200" border="1" cellspacing="0" cellpadding="0" bordercolor="black">
          <table border="0" cellspacing="0" cellpadding="0">
<%
      ExamInfo examInfo=new ExamInfo();
      Random Rnd=new Random();
      Vector exam=new Vector();
      String img="";

      //此sql是去撈出連連看要出的題數跟子題數
      sql=" SELECT sample_link_sum,sample_link_sub_sum"+
          " FROM sbl_course"+
          " WHERE course_id='"+CerItemId+"'";
      
      //rs=jdbc.queryData(sql,con);
      rs=jdbc.queryData(sql,con, "SES", query_user, strExecType+"_3_7_link");
      int linkSubSum=0,linkSum=0,subLinkNum=0;
      if(rs.next()){
          linkSum=rs.getInt("sample_link_sum"); //要出的大題
          linkSubSum=rs.getInt("sample_link_sub_sum");//每大題要選出幾題子題來出題
      }

      if(linkSum==0 || linkSubSum==0){
          //throw new Exception("科目:"+CerItemId+",連連看抽樣題數為0或連連看抽樣子題數為0"+",\n請到[鑑定作業][鑑定項目維護],修正設定!");
      }else{

	      //此sql是去撈出連連看要出幾題出來
	      //to_number() added by Jack on 2011/06/23 for fix bug JC201100165 : 
	      //若必考題數>9會有問題, 因為max(no)永遠為9, 會執行以下的throw new Exception("科目:".....)
	      sql=" SELECT max(to_number(no))max_sample_link_no FROM sbl_question_spec"+
	          " WHERE type='3'"+
	          " AND cer_item_id='"+CerItemId+"'";
	      rs=jdbc.queryData(sql,con);
      
	      rs.next();
	      int max_sample_link_no=rs.getInt("max_sample_link_no");
	
	      if(linkSum>max_sample_link_no){
	    	  if(rs!=null){
	    	  	rs.close();
	    	  }
	    	  if(con!=null){
	    	  	con.close();
	    	  }
	          throw new Exception("科目:"+CerItemId+",連連看抽樣題數量="+linkSum+"大於題庫實際數量="+max_sample_link_no+ ",\n請到[鑑定作業][鑑定項目維護],修正設定!");
	      }
	
	      int temp=0;
	      counter=0;
	      boolean[] Qus=new boolean[max_sample_link_no];
	      int[] NoOfQues=new int[linkSum];
	      while(true){
	          temp=Rnd.nextInt(max_sample_link_no)+1;
	          if(Qus[temp-1]!=true){
	              Qus[temp-1]=true;
	              NoOfQues[counter]=temp;
	              counter++;
	          }
	          if(counter==linkSum)
	              break;
	      }
	      int iItem=0;
	      Vector linkTitle=new Vector();
	      Vector linkQues=new Vector();
	      Vector linkAns=new Vector();
	      for(i=0;i<NoOfQues.length;i++){
	          //此sql是去撈出連連看要出幾題子題出來
   	          //to_number() added by Jack on 2011/06/23 for fix bug JC201100165 : 
	          //若必考題數>9會有問題, 因為max(no)永遠為9, 會執行以下的throw new Exception("科目:".....)
	          sql=" SELECT max(to_number(sub_no)) AS max_sub_no FROM sbl_question_spec"+
	              " WHERE type='3'"+
	              " AND no='"+NoOfQues[i]+"'"+
	              " AND cer_item_id='"+CerItemId+"'";
	          rs=jdbc.queryData(sql,con);
	
	          rs.next();
	          int maxSubNum=rs.getInt("max_sub_no");
	          if(linkSubSum>maxSubNum){
				  if(rs!=null){
					  rs.close();
				  }
				  if(con!=null){
					  con.close();
				  }
	              throw new Exception("科目:"+CerItemId+",連連看抽樣子題數量="+linkSubSum+"大於題庫實際小題數量="+maxSubNum+ ",\n請到[鑑定作業][鑑定項目維護],修正設定!");
	          }
	          Qus=new boolean[maxSubNum];
	          int[] quesSubSum=new int[linkSubSum];
	          counter=0;
	          while(true)
	          {
	              subLinkNum=Rnd.nextInt(maxSubNum)+1; //亂出出題
	              if(Qus[subLinkNum-1]!=true){
	                  Qus[subLinkNum-1]=true;
	                  quesSubSum[counter]=subLinkNum;
	                  counter++;
	              }
	             if(counter==linkSubSum)break;
	          }
	          //撈出連連看大標題出來
	          sql=" SELECT subject FROM sbl_question_spec"+
	              " WHERE cer_item_id='"+CerItemId+"'" +
	              " AND type='3'"+
	              " AND no='"+NoOfQues[i]+"'"+
	              " AND sub_no is null";
	          rs=jdbc.queryData(sql,con);
	
	          rs.next();
	          String title=rs.getString("subject");
	          iItem=iItem+1;
	          if(iItem==1){
		          out.print("<tr><td>第"+iItem+"題:"+title+"</td></tr>");
	          }else{
	        	  out.print("<tr><td><hr></td><td><hr></td></tr><tr><td>第"+iItem+"題:"+title+"</td></tr>");
	          }
	          examInfo.setTitle(title);
	          for(j=0;j<quesSubSum.length;j++)
	          {
	               //撈出連連看子標題出來
	              sql=" SELECT subject,file_name,answer,no"+
	                  " FROM sbl_question_spec"+
	                  " WHERE cer_item_id='"+CerItemId+"'" +
	                  " AND type='3'"+
	                  " AND no='"+NoOfQues[i]+"'"+
	                  " AND sub_no='"+quesSubSum[j]+"'";
	              rs=jdbc.queryData(sql,con);
	
	              while(rs.next())
	              {
	                String subject=rs.getString("subject");
	                String answer=rs.getString("answer");
	                examInfo.setSubject(subject);
	%>
	        <!--連連看開始------------------------------------------------------------->
	            <tr>
	              <td width="300" valign="top">
	                <input type="text" id="LinkQues" name="LinkQues" size="2"  maxLength="1" value="">
	                <br>
	                題目<%=(j+1)%>:<%=subject%>
	              </td>              
	<%				
					linkQues.add((j+1)+":"+subject);
					linkAns.add(answer);
					if(j==0){
	                        //******************************************************************************
	                        //此段程式是帶出圖形
	                                  sql=" SELECT file_name FROM sbl_question_spec"+
	                                      " WHERE type='3'"+
	                                      " AND no='"+NoOfQues[i]+"'"+
	                                      " AND sub_no is null"+
	                                      " AND cer_item_id='"+CerItemId+"'";
	                                  rs3=jdbc.queryData(sql,con);
	
	                                  rs3.next();
	                                  String files=rs3.getString("file_name");
	                                  if(files==null)
	                                      files="";
	
	                                  if(files!=""){
	                                      StringTokenizer st=new StringTokenizer(files,",");
	
	                                      while(st.hasMoreTokens())
	                                      {
	                                          String  fileName=st.nextToken();
	                                          img="第"+(i+1)+"題圖片:"+
	                                               "<br><img name=imageFile id=imageFile onload='reSizeImg()' "+
	                                               "src='showimage.jsp?file=" + fileName+"'></br><br>";
	                                          examInfo.setImages(fileName);
	                                      }
	                                  }
	
	                        //******************************************************************************/                      
	                  out.print("<td><font color="+"white"+">oooooo</font>"+img+"</td>");
					}else{
						img="";
					}
	              }//end of while
	          }//end of for
	          exam.add(examInfo);
	          examInfo=new ExamInfo();          
	       }//for Loop
	        session.setAttribute("linkQues",linkQues); //連連看題目
	        session.setAttribute("linkAns",linkAns);  //連連看答案
	        session.setAttribute("exam",exam);        //連連看題目跟答案
      }
%>
              
            </tr>
            <br>
          </table>
        </table>
<%
        //防止考試user挑題目,進入[線上考試 Online Testing]後,先打零分表示考試中
        sql = " UPDATE sbl_cer_reg" +
              " SET score_writing = '0'" +
              "    ,upt_date= sysdate" +
              "    ,upt_user='" + EmpId + "'" +
              " WHERE cer_item_id = '" + CerItemId + "'" +
              " AND emp_id = '" + EmpId + "'" + 
              " AND cer_reg_no = '" + sExamRegNo + "'";
        jdbc.updatePreparation(sql);
        int check_list2 = BADAO.insertConfidential_SysLog(con, "SES", query_user, strExecType+"_開始考試", sql);

    }//End of else(
%>

<script language="JavaScript">
//此段JavaScript執行時機在sExamStatus.equals("testing")
    document.getElementById('Bnt').disabled=true;
    document.getElementById('txtEmp_id').disabled=true;
    document.getElementById('sltCerItemId').disabled=true;
    document.getElementById('sltStationId').disabled=true;    
    document.getElementById('hdnExamStatus').value='testing';//[開始測驗]後到[交卷]前,考試狀態設為testing

    //抓取user[開始測驗]所選考題的考試時間
    var sCerItemId=document.getElementById("sltCerItemId").value;
    var oExamItem=document.getElementsByName("ExamItem");
    var oExamTime=document.getElementsByName("ExamTime");
    var oExamRegNo=document.getElementsByName("ExamRegNo");
    for(i=0;i<oExamItem.length;i++){
        if(oExamItem.item(i).value.replace(/@/g," ")==sCerItemId){
            document.getElementById("allTime").value=parseInt(oExamTime.item(i).value);
            document.getElementById("sExamRegNo").value=oExamRegNo.item(i).value;
        }            
    }

    var allTime=parseInt(document.getElementById('ExamTime').value);
    var spendTime=allTime;
    startTime=setInterval("setTime()",1000); //時間開始計時
</script>

<%
}//End of if(sExamStatus.equals("testing"))

jdbc.clean();
jdbc.close();
if(rs!=null){
	rs.close();
}
if(rs2!=null){
	rs2.close();
}
if(rs3!=null){
	rs3.close();
}
if(stmt!=null){
	stmt.close();
}
if(con!=null){
	con.close();
}
%>
<script language="JavaScript">
function setTime()
{
    //時間到自動submit
    if(spendTime<=0){
        clearInterval(startTime);
        alert("時間到(Time Out)!");
        FinishExam(event);
        document.getElementById("spendTime").value=allTime;
        Exam.submit();
        spendTime=10;
    }
    //開始計算時間(倒數)
    else{
        spendTime--;
        min=Math.floor(spendTime/60);
        sec=Math.round(spendTime-Math.floor(spendTime/60)*60);
        showTime.innerText="剩餘時間(Left time):"+min+" Min "+sec+" sec";
        var time=min*60+sec;
        document.getElementById("spendTime").value=time;
    }
}

//調整圖片的大小
function reSizeImg()
{
/*
    width=window.screen.width*0.9/2;
    picWidth=parseInt(document.getElementById('imageFile').width);
    if(picWidth>width)
        document.getElementById('imageFile').width=width;
*/
}
var startTime=0;//時間倒數計時用途
</script>
<%
if(sExamStatus!=null && sExamStatus.equals("testing")){
	out.println("<br><br><center><input type=\"button\" value=\"交卷 Finish exam\" id=\"FBnt\" onClick=\"FinishExam(event)\"></center>");
}
%>
</form>
</body>
</html>
