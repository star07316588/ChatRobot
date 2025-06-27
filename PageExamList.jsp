<%@ page contentType="text/html; charset=big5" %>
<%@ page errorPage="ExamErrMsg.jsp" %>
<jsp:useBean id="jdbc" scope="request" class="com.mxic.ses.db.DBAccess"/>
<jsp:setProperty name="jdbc" property="*" />
<%@ page import="java.util.*,java.text.*"%>
<%@ page import="java.math.BigDecimal"%>
<%@ page import="java.sql.*,com.mxic.ses.db.*"%>
<%@ page import="com.mxic.ses.util.Property"%>
<%@ page import="com.mxic.ses.util.ExamInfo"%>
<%@ page import="com.mxic.ses.util.ScoreCompute"%>
<%@ page session="false"%>
<html>
<head>
<title>Maintain Course</title>

</head>
<body>
<meta http-equiv="Content-Type" content="text/html; charset=big5">
<link rel="stylesheet" href="../../ses.css">
<table><td><img src="../images/title-blank.jpg"><td class="title">線上考試結果Online Testing result</table>
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
  String strExecType = "OnlineTest_Test";
  String query_user = (String)session.getAttribute("userid");

  //先判斷有無成績
//String WorkerId=request.getParameter("WorkerId");//工號
String WorkerId=request.getParameter("txtEmp_id");//工號
//String ExamId=request.getParameter("ExamId");//試題項目代碼
String ExamId=request.getParameter("sltCerItemId");//試題項目代碼
if(ExamId!=null){
	ExamId = ExamId.replace('@',' ');
}
String StationId=request.getParameter("sltStationId");//站別
String sExamRegNo=request.getParameter("sExamRegNo");//考試注冊代碼
String[] UserAns4=request.getParameterValues("txtNecessary");//必考題答案
String[] UserAns1=request.getParameterValues("TF");//是非題答案
String[] UserAns2=request.getParameterValues("Choose");//選擇題答案
String[] UserAns3=request.getParameterValues("LinkQues");//連連看答案
String[] imageFile=request.getParameterValues("imageFile");//連連看圖檔
Vector exam=(Vector)session.getAttribute("exam");//onlineExam.Form.id
session.removeAttribute("exam");
String userId=(String)session.getAttribute("userid");//登入帳號
int spendTime=Integer.parseInt(request.getParameter("spendTime"));//考試使用時間
int allTime=Integer.parseInt(request.getParameter("allTime"));//考試時間
//修改成績計算方式，改為以題數平均，若有小數，則採四捨五入計算。2008/10/27 for User Request
//double mulitWeight=60,linkWeight=40; //分數比重
double multiCorrect=0.0,linkCorrect=0.0;//,multiScore=0.0;
double multiWrong=0.0,linkWrong=0.0;
double multiTotal=0.0,linkTotal=0.0;
int min=0,sec=0;
String message="";
Connection con = null;
con = DBConnection.getConnection();
DecimalFormat df=new DecimalFormat("00.0");
ArrayList alList = new ArrayList();
String sql=" SELECT score_writing FROM sbl_cer_reg "+
           " WHERE emp_id='"+WorkerId+"'"+
           " AND cer_item_id='"+ExamId+"'"+
           " AND to_char(wr_date,'yyyymm')=to_char(sysdate,'yyyymm')";
ResultSet rs=jdbc.queryData(sql,con);

boolean flag=rs.next();
if(flag)
    out.print("你已經考過試/You have done test already");
else{
    int iItem=0;
    out.print("<hr size='1' noshade>");
    out.print("<font color='blue'>必考題 Necessary question:</font><br><br>");
    message+="必考題 Necessary question:\n\n";
    if(UserAns4!=null){
        for(int i=0;i<UserAns4.length;i++){
            iItem=i+1;
            String Qus=(String)session.getAttribute("Qus4"+iItem);
            String Ans=(String)session.getAttribute("Ans4"+iItem);
            session.removeAttribute("Qus4"+iItem);
            session.removeAttribute("Ans4"+iItem);
            if(Qus==null){
            	Qus="";
            }
            if(Ans==null){
            	Ans="";
            }
            if(Ans.trim().toUpperCase().equals(UserAns4[i].trim().toUpperCase())){
                out.print("&nbsp;&nbsp;&nbsp;第"+iItem+"題題目 (Questions No.:"+iItem+"):"+Qus+"<br><br>");
                message+="第"+iItem+"題題目:"+Qus+"\n";
                //message+="Questions No.:"+iItem+":"+Qus+"\n\n";
                out.print("<font color='green'>&nbsp;&nbsp;&nbsp;第"+iItem+"題您答對了(Questions No.:"+iItem+" correct),答案是(The answer):"+Ans+"</font><br><br>");
                message+="第"+iItem+"題您答對了,答案是"+Ans+"\n";
                message+="Questions No.:"+iItem+"corrct,The answer is "+Ans+"\n\n";
                multiCorrect++;
            }
            else{
                multiWrong++;
                out.print("&nbsp;&nbsp;&nbsp;第"+iItem+"題題目(Questions No.:"+iItem+"):"+Qus+"<br><br>");
                message+="第"+iItem+"題題目:"+Qus+"\n";
                //message+="Questions No.:"+iItem+":"+Qus+"\n\n";
                out.print("<font color='red'>&nbsp;&nbsp;&nbsp;第"+iItem+"題您答錯了(Questions No.:"+iItem+" incorrect),您選的答案是(Your answer):"+UserAns4[i]+",正確答案是(Correct answer):"+Ans+"</font><br><br>");
                message+="第"+iItem+"題您答錯了,您選的答案是:"+UserAns4[i]+",正確答案是:"+Ans+"\n";
                message+="Questions No.:"+iItem+" incorrect,Your answer:"+UserAns4[i]+",Correct answer:"+Ans+"\n\n";
            }
        }
    }

    iItem=0;
    out.print("<hr size='1' noshade>");
    out.print("<font color='blue'>1.是非題 Right and wrong question:</font><br><br>");
    message+="1.是非題 Right and wrong question:\n\n";
    if(UserAns1!=null){
        for(int i=0;i<UserAns1.length;i++){
            iItem=i+1;
            String Qus=(String)session.getAttribute("QusA"+iItem);
            String Ans=(String)session.getAttribute("AnsA"+iItem);
            session.removeAttribute("QusA"+iItem);
            session.removeAttribute("AnsA"+iItem);
            out.print("&nbsp;&nbsp;&nbsp;第"+iItem+"題題目(Questions No.:"+iItem+"):"+Qus+"<br>");
            message+="  第"+iItem+"題題目:"+Qus+"\n";
            //message+="  Questions No.:"+iItem+":"+Qus+"\n\n";
            if(Ans.trim().toUpperCase().equals(UserAns1[i].trim().toUpperCase())){
                out.print("<font color='green'>&nbsp;&nbsp;&nbsp;第"+iItem+"題您答對了(Questions No.:"+iItem+",answer correct),");
                message+="  第"+iItem+"題您答對了,";
                message+="  Questions No.:"+iItem+",answer correct,";
                if(Ans.equals("T")){
                    out.print("正確答案是○(The answer is ○.)</font><br><br>");
                    message+="正確答案是○\n";
                    message+="The answer is ○.\n\n";
                }
                else{
                    out.print("正確答案是╳(The answer is ╳.)</font><br><br>");
                    message+="正確答案是╳\n";
                    message+="The answer is ╳.\n\n";
                }
                multiCorrect++;
            }
            else{
								out.print("<font color='red'>");
                out.print("<font color='red'>&nbsp;&nbsp;&nbsp;第"+iItem+"題您答錯了(Questions No.:"+iItem+",answer incorrect),");
                message+="第"+iItem+"題您答錯了,";
                message+="Questions No.:"+iItem+",answer incorrect,";
                if(Ans.equals("T")){
                    out.print("正確答案是○(The answer is ○.)</font><br><br>");
                    message+="正確答案是○\n";
                    message+="The answer is ○.\n\n";
                }
                else{
                    out.print("正確答案是╳(The answer is ╳.)</font><br><br>");
                    message+="正確答案是╳\n";
                    message+="The answer is ╳.\n\n";
                }
								out.print("</font>");
                multiWrong++;
            }
        }
    }

    out.print("<hr size='1' noshade>");
    out.print("<font color='blue'>2.選擇題 Select question:</font><br><br>");
    message+="2.選擇題 Select question:\n\n";
    if(UserAns2!=null){
        for(int i=0;i<UserAns2.length;i++){
            iItem=i+1;
            String Qus=(String)session.getAttribute("QusB"+iItem);
            String Ans=(String)session.getAttribute("AnsB"+iItem);
            session.removeAttribute("QusB"+iItem);
            session.removeAttribute("AnsB"+iItem);
            if(Ans.trim().toUpperCase().equals(UserAns2[i].trim().toUpperCase())){
                out.print("&nbsp;&nbsp;&nbsp;第"+iItem+"題題目 (Questions No.:"+iItem+"):"+Qus+"<br><br>");
                message+="第"+iItem+"題題目:"+Qus+"\n";
                //message+="Questions No.:"+iItem+":"+Qus+"\n\n";
                out.print("<font color='green'>&nbsp;&nbsp;&nbsp;第"+iItem+"題您答對了(Questions No.:"+iItem+" correct),答案是(The answer):"+Ans+"</font><br><br>");
                message+="第"+iItem+"題您答對了,答案是"+Ans+"\n";
                message+="Questions No.:"+iItem+"corrct,The answer is "+Ans+"\n\n";
                multiCorrect++;
            }
            else{
                multiWrong++;
                out.print("&nbsp;&nbsp;&nbsp;第"+iItem+"題題目(Questions No.:"+iItem+"):"+Qus+"<br><br>");
                message+="第"+iItem+"題題目:"+Qus+"\n";
                //message+="Questions No.:"+iItem+":"+Qus+"\n\n";
                out.print("<font color='red'>&nbsp;&nbsp;&nbsp;第"+iItem+"題您答錯了(Questions No.:"+iItem+" incorrect),您選的答案是(Your answer):"+UserAns2[i]+",正確答案是(Correct answer):"+Ans+"</font><br><br>");
                message+="第"+iItem+"題您答錯了,您選的答案是:"+UserAns2[i]+",正確答案是:"+Ans+"\n";
                message+="Questions No.:"+iItem+" incorrect,Your answer:"+UserAns2[i]+",Correct answer:"+Ans+"\n\n";
            }
        }
    }

    out.print("<hr size='1' noshade>");
    out.print("<br><font color='blue'>3.連連看 Link question:</font><br><br>");
    alList.add(message);
    message="3.連連看 Link question:\n\n";
    if(UserAns3!=null){
        for(int i=0;i<UserAns3.length;i++){
            if(UserAns3[i].equals(""))
                UserAns3[i]="空白 Empty";
        }
    }
    Vector linkQus=(Vector)session.getAttribute("linkQues");
    Vector linkAns=(Vector) session.getAttribute("linkAns");
    session.removeAttribute("linkQues");
    session.removeAttribute("linkAns");
    /*
    for(int i=0;i<linkAns.size();i++){
        String link_ques=(String)linkQus.elementAt(i);
        String link_ans=(String)linkAns.elementAt(i);
    }
    */
    ExamInfo exam_info=null;
	int k=0;
	if(exam!=null){
	    for(int i=0;i<exam.size();i++){
	        exam_info=new ExamInfo();
	        exam_info=(ExamInfo)exam.elementAt(i);
	        out.print("&nbsp;&nbsp&nbsp;第"+(i+1)+"大題(Question No.:"+(i+1)+" subject):"+exam_info.getTitle()+"<br><br>");
	        message+="  第"+(i+1)+"大題:"+exam_info.getTitle()+"\n";
	        //message+="  Question No.:"+(i+1)+":"+exam_info.getTitle()+"\n\n";
	        Vector subject=exam_info.getSubject();
	        for(int j=0;j<subject.size();j++){
	            out.print("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;第"+(j+1)+"小題(Sub-question No.:"+(j+1)+"):"+subject.elementAt(j)+"<br><br>");
	            message+="   第"+(j+1)+"小題:"+subject.elementAt(j)+"\n\n";
	            //message+="   Sub-question"+(j+1)+":"+subject.elementAt(j)+"\n\n";
	            String link_ques=(String)linkQus.elementAt(k);
	            String link_ans=(String)linkAns.elementAt(k);
	            if(UserAns3!=null){
		            if(link_ans.trim().toUpperCase().equals(UserAns3[k].trim().toUpperCase())){
		                linkCorrect++;
		                out.print("<font color='green'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;您答對了! 正確答案是(Correct! The answer):"+link_ans+"</font><br><br>");
		                message+="您答對了,正確答案是"+link_ans+"\n";
		                message+="Correct,The answer:"+link_ans+"\n\n";
		            }
		            else{
		                linkWrong++;
		                out.print("<font color='red'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;您答錯了,您的答案是(Incorrect! Your answer):"+UserAns3[k]+",正確答案是(Correct answer):"+link_ans+"</font><br><br>");
		                message+="您答錯了,您的答案是:"+UserAns3[k]+",正確答案是:"+link_ans+"\n";
		                message+="Incorrect! Your answer:"+UserAns3[k]+",Correct answer:"+link_ans+"\n\n";
		            }
		            k++;
	            }else{
	            	return;
	            }
	        }
	        alList.add(message);
	        message="\n\n";
	        alList.add(message);
	        Vector examPic=exam_info.getImages();
	        for(int z=0;z<examPic.size();z++)
	            alList.add(examPic.elementAt(z));
	        
	    }
	}
    multiTotal=multiCorrect+multiWrong;
    //multiScore=mulitWeight*multiCorrect/multiTotal;
    //multiScore=Double.parseDouble(df.format(multiScore));
    //double linkScore =0.0D;
    if(exam!=null){
	    linkTotal=linkCorrect+linkWrong;
	    //linkScore=linkWeight*linkCorrect/linkTotal;
	    //linkScore=Double.parseDouble(df.format(linkScore));
    }
    out.print("<br>");
    double dTotalScore = 0.0D;
    dTotalScore=((multiCorrect+linkCorrect)/(multiTotal+linkTotal))*100;
    //總分四捨五入
    int totalScore = (new BigDecimal(dTotalScore).setScale(0, BigDecimal.ROUND_HALF_UP)).intValue();

    if(allTime-spendTime==0)
        spendTime=allTime;
    else
        spendTime=allTime-spendTime;

    min=(int)(Math.floor(spendTime/60));
    sec=(int)(Math.round(spendTime-Math.floor(spendTime/60)*60));
%>
您總共花了:&nbsp;<%=min%>&nbsp;分&nbsp;<%=sec%>&nbsp;秒&nbsp;總分是:<%=totalScore%>
<%
    //計算成績並產生PDF檔
    //Modified by Jack on 2023/08/22
    //ScoreCompute sc=new ScoreCompute();
	ScoreCompute sc=new ScoreCompute("SES", query_user, strExecType);
    sc.UpdateScore(userId,WorkerId,ExamId,"WR",Integer.toString(totalScore),alList,sExamRegNo,StationId);    
}//end of else
jdbc.clean();
jdbc.close();
if(rs!=null){
	rs.close();
}
if(con!=null){
	con.close();
}
%>
</body>
</html>
