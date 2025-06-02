這一份是ScoreCompute.java完整內容，請協助修改成Controller並與現在的架構融合調整
若是有呼叫sql的部分，請協助調整呼叫方式為Controller→Services→Repositories

package com.mxic.ses.util;
import com.mxic.ses.worker.BADAO;
import com.mxic.ses.worker.User;
import com.mxic.ses.worker.Function;
import com.mxic.ses.worker.Group;
import com.mxic.ses.db.*;

import java.sql.*;
import java.util.*;

/**
 * Title: ScoreCompute.java
 * Description: Compute Operation & Writting score
 * Copyright: MXIC Copyright (c) 2008
 * Company: MXIC
 * @author: Donk Huang,CIT on 2008/01/24
 * @version 1.0
 */
public class ScoreCompute {
    private String sSql="";
    private java.sql.ResultSet oRS=null;
    private DBAccess jdbc= null;
    private String sToday="";
    private String sThisMonth="";
    private String sNextMonth="";
    private String sFileColumn="";
    private String sFileName="";
    private String sScoreStatus="";

    //Added by Jack on 2023/08/28 <Start>
    private String sPlatform = null;
    private String sUserID = null;
    private String sExecFunction = null;
    private boolean bNeedToLogSysLog = false;
    private String sExamResult = "";//16種結果
    
    public ScoreCompute(String insPlatform, String insUserID, String insExecFunction) {
    	sPlatform = insPlatform;
    	sUserID = insUserID;
    	sExecFunction = insExecFunction;
    	
    	bNeedToLogSysLog = true;
    	
        try{
            jdbc = new DBAccess();
        }
        catch(Exception Ee){
            System.err.println("Exception Message="+Ee.getMessage()+
                               "\n LocalizedMessage=" +Ee.getLocalizedMessage());
            Ee.printStackTrace();
            jdbc.close();
        }

        SetTime();
    }
    //Added by Jack on 2023/08/28 <End>
    
    public ScoreCompute() {
        try{
            jdbc = new DBAccess();
        }
        catch(Exception Ee){
            System.err.println("Exception Message="+Ee.getMessage()+
                               "\n LocalizedMessage=" +Ee.getLocalizedMessage());
            Ee.printStackTrace();
            jdbc.close();
        }

        SetTime();
    }
    
    private void SetTime(){
        try{
        	Connection con = null;
        	try{
        		con = DBConnection.getConnection();
        	}catch(Exception e){
        		e.printStackTrace();
        		con.close();
        	}
            //預防user修改PC時間值,自DB server取得今天日期
            sSql =" SELECT to_char(sysdate,'YYYY/MM/DD') FROM dual";
            oRS=jdbc.queryData(sSql,con);
            oRS.next();
            sToday = oRS.getString(1).trim();
            sThisMonth= sToday.substring(0,7);

            sSql =" SELECT to_char(add_months(sysdate,1),'YYYY/MM') FROM dual";
            oRS=jdbc.queryData(sSql,con);
            oRS.next();
            sNextMonth = oRS.getString(1).trim();
            oRS.close();
            con.close();
        }
        catch(SQLException Se){
            System.err.println("SQLException Message="+Se.getMessage()+
                               "\n LocalizedMessage=" +Se.getLocalizedMessage()+
                               "\n SQLState=" +Se.getSQLState());
            Se.printStackTrace();
            jdbc.close();
        }
    }

    /**
     * sql update學科&術科成績,計算結果
     *
     * @param m_User_id 成績評鑑人登入帳號,例:admin
     * @param m_EMP_id 應考人工號,5碼數字,例:01234
     * @param m_cer_item_id 學科&術科試題代碼,例:ADV-M3841A
     * @param m_ScoreType 成績類別,限WR&OP二類,不限大小寫,表示學科&術科
     * @param m_Score 成績,double宣告,可有小數,例:2.54
     * @param mArrayList PDF檔中需要顯示的訊息
     * @param m_ExamRegNo 考試注冊代碼
     * @param m_Station_id 站別
     * @return UpdateScore執行成功為true,失敗為false
     */
    public boolean UpdateScore(String m_User_id,
                               String m_EMP_id,
                               String m_cer_item_id,
                               String m_ScoreType,
                               String m_Score,
                               ArrayList mArrayList,
                               String m_ExamRegNo,
                               String m_Station_id){
        double dScore_WR = 0.0;
        double dScore_OP = 0.0;
        String sDate_WR = "";
        String sDate_OP = "";
        double dOverMonth_WR=0.0;
        double dOverMonth_OP=0.0;
        int item = 0;
        double passScore = 60.0;	// mao 20100513, JC201000093, default pass score 
        double agradeScore = 80.0;	// mao 20100513, JC201000093, default A grade score
        
        
        //.CreateLicenceUtil licence=new CreateLicenceUtil();
        CreateLicenceUtil licence=null;
        
      //Added by Jack on 2023/08/28
    	if (bNeedToLogSysLog){
    		licence=new CreateLicenceUtil(sPlatform, sUserID, sExecFunction);
    	}else{
    		licence=new CreateLicenceUtil();
    	}
    	
        Connection con = null;
        try{
        	con = DBConnection.getConnection();
        }catch(Exception e){
        	e.printStackTrace();
        	try{
        		con.close();
        	}catch(SQLException sqle){
        		sqle.printStackTrace();
        	}
        }

        try{
        	sSql = "SELECT nvl(pass,60) pass,nvl(agrade,80) agrade" +
        		" FROM sbl_course" +
        		" WHERE course_id = '" + m_cer_item_id + "'";
        	//Added by Jack on 2023/08/28
        	if (bNeedToLogSysLog){
        		BADAO.insertConfidential_SysLog(con, sPlatform, sUserID, sExecFunction+"_成績計算_1_GetPassScore", sSql + "--passScore="+passScore+",agradeScore="+agradeScore );
        	}
        	oRS = jdbc.queryData(sSql,con);
        	oRS.next();
        	passScore = oRS.getDouble("pass");
        	agradeScore = oRS.getDouble("agrade");
        }catch(Exception e){
        	e.printStackTrace();
        }

         //update學科&術科成績
        if (m_ScoreType.toUpperCase() == "WR") {
            sSql = " UPDATE sbl_cer_reg" +
                   " SET score_writing = '" + m_Score + "'" +
                   "    ,reg_date= to_date('" + sToday + "','YYYY/MM/DD')" +
                   "    ,wr_date = to_date('" + sToday + "','YYYY/MM/DD')" +
                   "    ,upt_date= sysdate" +
                   "    ,upt_user='" + m_User_id + "'" +
                   " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                   " AND emp_id = '" + m_EMP_id + "'"+
                   " AND cer_reg_no = '" + m_ExamRegNo + "'";

            sFileColumn="wr_file";
            sFileName=m_EMP_id + "_" + m_cer_item_id +"_" + "WR" +"_" + sToday.replaceAll("/","");
        }
        else if (m_ScoreType.toUpperCase() == "OP") {
            if(Double.parseDouble(m_Score) >=passScore){
                sSql = " UPDATE sbl_cer_reg" +
                       " SET score_oper = '" + m_Score + "'" +
                       "    ,oper_fail= 'P'" +
                       "    ,reg_date= to_date('" + sToday + "','YYYY/MM/DD')" +
                       "    ,op_date = to_date('" + sToday + "','YYYY/MM/DD')" +
                       "    ,upt_date= sysdate" +
                       "    ,upt_user='" + m_User_id + "'" +
                       "    ,tester='" + m_User_id + "'" +
                       " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                       " AND emp_id = '" + m_EMP_id + "'" +
                       " AND cer_reg_no = '" + m_ExamRegNo + "'";                
            }
            else{
                sSql = " UPDATE sbl_cer_reg" +
                       " SET score_oper = '" + m_Score + "'" +
                       "    ,oper_fail= 'F'" +
                       "    ,reg_date= to_date('" + sToday + "','YYYY/MM/DD')" +
                       "    ,op_date = to_date('" + sToday + "','YYYY/MM/DD')" +
                       "    ,upt_date= sysdate" +
                       "    ,upt_user='" + m_User_id + "'" +
                       "    ,tester='" + m_User_id + "'" +
                       " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                       " AND emp_id = '" + m_EMP_id + "'" +
                       " AND cer_reg_no = '" + m_ExamRegNo + "'";
            }

            sFileColumn="op_file";
            sFileName=m_EMP_id + "_" + m_cer_item_id +"_" + "OP" +"_" + sToday.replaceAll("/","");
        }
        else {
            return false;
        }

        try {
            jdbc.updatePreparation(sSql,con);
        	//Added by Jack on 2023/08/28
        	if (bNeedToLogSysLog){
        		BADAO.insertConfidential_SysLog(con, sPlatform, sUserID, sExecFunction+"_成績計算_2_UpdateScore", sSql);
        	}

            //取得最新的學科&術科成績與登錄成績時間
            sSql = " SELECT score_writing,score_oper,wr_date,op_date" +
                   " FROM sbl_cer_reg" +
                   " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                   " AND emp_id = '" + m_EMP_id + "'" +
                   " AND cer_reg_no = '" + m_ExamRegNo + "'";
        	//Added by Jack on 2023/08/28
        	if (bNeedToLogSysLog){
        		BADAO.insertConfidential_SysLog(con, sPlatform, sUserID, sExecFunction+"_成績計算_3_GetExamScore", sSql);
        	}
            oRS = jdbc.queryData(sSql,con);
            oRS.next();

            if(oRS.getString("score_writing")==null)
                dScore_WR=-1;//等待學科成績
            else if(oRS.getString("score_writing").toUpperCase().equals("NA"))
                dScore_WR=-2;//不需考學科
            else
                dScore_WR=Double.parseDouble(oRS.getString("score_writing"));

            if(oRS.getString("score_oper")==null)
                dScore_OP=-1;//等待術科成績
            else if(oRS.getString("score_oper").toUpperCase().equals("NA"))
                dScore_OP=-2;//不需考術科
            else
                dScore_OP=Double.parseDouble(oRS.getString("score_oper"));

            sDate_WR = oRS.getString("wr_date")==null?"":oRS.getString("wr_date").substring(0,10);
            sDate_OP = oRS.getString("op_date")==null?"":oRS.getString("op_date").substring(0,10);

            //Added by Jack on 2023/08/28
        	if (bNeedToLogSysLog){
        		BADAO.insertConfidential_SysLog(con, sPlatform, sUserID, sExecFunction+"_成績計算_4", "WR=" + dScore_WR + ",OP=" + dScore_OP + ",WR=" + sDate_WR + ",OP=" + sDate_OP);
        	}

            //取得學科登錄成績時間月份長度,做為學科及格但時間是否超過3個月判斷用途
            //取得間隔月數 modified by Jack on 2013/09/27.
            //sSql =" SELECT months_between(sysdate,to_date('" + sDate_WR + "','YYYY/MM/DD')) FROM dual";
            sSql =" SELECT months_between(trunc(sysdate, 'mm'), "+
		                 " trunc(to_date('" + sDate_WR + "','YYYY/MM/DD'), 'mm') ) "+
		            " FROM dual ";
            oRS = jdbc.queryData(sSql,con);
            oRS.next();
            dOverMonth_WR=oRS.getString(1)==null?-1:Double.parseDouble(oRS.getString(1));

            //取得術科登錄成績時間月份長度,做為術科及格但時間是否超過3個月判斷用途
            //取得間隔月數 modified by Jack on 2013/09/27.
            //sSql =" SELECT months_between(sysdate,to_date('" + sDate_OP + "','YYYY/MM/DD')) FROM dual";
            sSql =" SELECT months_between(trunc(sysdate, 'mm'), "+
			             " trunc(to_date('" + sDate_OP + "','YYYY/MM/DD'), 'mm') ) "+
			        " FROM dual ";
            oRS = jdbc.queryData(sSql,con);
            oRS.next();
            dOverMonth_OP=oRS.getString(1)==null?-1:Double.parseDouble(oRS.getString(1));

            //Added by Jack on 2023/08/28
        	if (bNeedToLogSysLog){
        		BADAO.insertConfidential_SysLog(con, sPlatform, sUserID, sExecFunction+"_成績計算_5", "Over3Month_WR=" + dOverMonth_WR + ",Over3Month_OP=" + dOverMonth_OP );
        	}
            
            sScoreStatus="F";//預設值為False,不及格

            //學科&術科成績各有Pass(>=60),False(<60),Null(-1),NA(-2) 四種,依排列組合後,有16種選擇條件
            if (dScore_WR >= passScore) {
                if (dScore_OP >= passScore) {
                    //條件1, WR=Pass,OP=Pass
                	sExamResult = "條件1 : WR=Pass,OP=Pass";//Added by Jack on 2023/08/28
                    if (dOverMonth_WR >= 4) {
                        //時間超過3個月,該項成績視同沒過
                    	//sNextMonth--> sThisMonth modified by Jack on 2013/09/27 
                    	//(目前：需等下個月才能重考 --> 變更：當月就允許重考。
                    	sExamResult = "條件1 : WR=Pass,OP=Pass-1";//Added by Jack on 2023/08/28
                        sSql = " UPDATE sbl_cer_reg" +
                               " SET score_writing = ''" +
                               "    ,grade='C'" +
                               "    ,cer_date= to_date('" + sThisMonth + "','YYYY/MM')" +
                               "    ,upt_date= sysdate" +
                               "    ,upt_user='" + m_User_id + "'" +
                               "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                               " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                               " AND emp_id = '" + m_EMP_id + "'" +
                               " AND cer_reg_no = '" + m_ExamRegNo + "'";
                    }
                    else if (dOverMonth_OP >= 4) {
                        //時間超過3個月,該項成績視同沒過
                    	//sNextMonth--> sThisMonth modified by Jack on 2013/09/27 
                    	//(目前：需等下個月才能重考 --> 變更：當月就允許重考。
                    	sExamResult = "條件1 : WR=Pass,OP=Pass-2";//Added by Jack on 2023/08/28
                        sSql = " UPDATE sbl_cer_reg" +
                               " SET score_oper = ''" +
                               "    ,grade='C'" +
                               "    ,cer_date= to_date('" + sThisMonth + "','YYYY/MM')" +
                               "    ,upt_date= sysdate" +
                               "    ,upt_user='" + m_User_id + "'" +
                               "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                               " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                               " AND emp_id = '" + m_EMP_id + "'" +
                               " AND cer_reg_no = '" + m_ExamRegNo + "'";
                    }
                    else{
                        if (dScore_WR >= agradeScore && dScore_OP >= agradeScore) {
                        	sExamResult = "條件1 : WR=Pass,OP=Pass-3-1(P)";//Added by Jack on 2023/08/28
                            sSql = " UPDATE sbl_cer_reg" +
                                " SET upt_date= sysdate" +
                                "    ,grade='A'" +
                                "    ,upt_user='" + m_User_id + "'" +
                                "    ," + sFileColumn + "='" + sFileName + "P.pdf" + "'" +
                                " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                                " AND emp_id = '" + m_EMP_id + "'" +
                                " AND cer_reg_no = '" + m_ExamRegNo + "'";
                        }
                        else{
                        	sExamResult = "條件1 : WR=Pass,OP=Pass-3-2(P)";//Added by Jack on 2023/08/28
                            sSql = " UPDATE sbl_cer_reg" +
                                 " SET upt_date= sysdate" +
                                 "    ,grade='B'" +
                                 "    ,upt_user='" + m_User_id + "'" +
                                 "    ," + sFileColumn + "='" + sFileName + "P.pdf" + "'" +
                                 " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                                 " AND emp_id = '" + m_EMP_id + "'" +
                                 " AND cer_reg_no = '" + m_ExamRegNo + "'";
                        }

                        sScoreStatus="P";//產生執照和獎金
                    }
                }
                else if (-1< dScore_OP && dScore_OP < passScore) {
                     //條件2, WR=Pass,OP=Fail
                	 sExamResult = "條件2 : WR=Pass,OP=Fail";//Added by Jack on 2023/08/28
                     if (dOverMonth_WR >= 4) {
                     	//sNextMonth--> sThisMonth modified by Jack on 2013/09/27 
                     	//(目前：需等下個月才能重考 --> 變更：當月就允許重考。
                    	 sExamResult = "條件2 : WR=Pass,OP=Fail-1";//Added by Jack on 2023/08/28
                         sSql = " UPDATE sbl_cer_reg" +
                             " SET score_writing = ''" +
                             "    ,score_oper = ''" +
                             "    ,grade='C'" +
                             "    ,cer_date= to_date('" + sThisMonth + "','YYYY/MM')" +
                             "    ,upt_date= sysdate" +
                             "    ,upt_user='" + m_User_id + "'" +
                             "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                             " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                             " AND emp_id = '" + m_EMP_id + "'" +
                             " AND cer_reg_no = '" + m_ExamRegNo + "'";
                     }
                     else{
                    	 sExamResult = "條件2 : WR=Pass,OP=Fail-2";//Added by Jack on 2023/08/28
                         sSql = " UPDATE sbl_cer_reg" +
                                " SET score_oper = ''" +
                                "    ,grade='C'" +
                                "    ,cer_date= to_date('" + sNextMonth + "','YYYY/MM')" +
                                "    ,upt_date= sysdate" +
                                "    ,upt_user='" + m_User_id + "'" +
                                "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                                " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                                " AND emp_id = '" + m_EMP_id + "'" +
                                " AND cer_reg_no = '" + m_ExamRegNo + "'";
                     }
                }
                else if(dScore_OP==-1){
                    //條件3, WR=Pass,OP=Null,等待成績中
                	sExamResult = "條件3 : WR=Pass,OP=Null,等待成績中";//Added by Jack on 2023/08/28
                    sSql = " UPDATE sbl_cer_reg" +
                           " SET upt_date= sysdate" +
                           "    ,upt_user='" + m_User_id + "'" +
                           "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                           " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                           " AND emp_id = '" + m_EMP_id + "'" +
                           " AND cer_reg_no = '" + m_ExamRegNo + "'";
                }
                else if(dScore_OP==-2){
                    //條件4, WR=Pass,OP=NA
                	sExamResult = "條件4 : WR=Pass,OP=NA";//Added by Jack on 2023/08/28
                    if (dOverMonth_WR >= 4) {
                        //時間超過3個月,該項成績視同沒過
                     	//sNextMonth--> sThisMonth modified by Jack on 2013/09/27 
                     	//(目前：需等下個月才能重考 --> 變更：當月就允許重考。                    	
                    	sExamResult = "條件4 : WR=Pass,OP=NA-1";//Added by Jack on 2023/08/28
                        sSql = " UPDATE sbl_cer_reg" +
                               " SET score_writing = ''" +
                               "    ,grade='C'" +
                               "    ,cer_date= to_date('" + sThisMonth + "','YYYY/MM')" +
                               "    ,upt_date= sysdate" +
                               "    ,upt_user='" + m_User_id + "'" +
                               "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                               " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                               " AND emp_id = '" + m_EMP_id + "'" +
                               " AND cer_reg_no = '" + m_ExamRegNo + "'";
                    }
                    else{
                        if (dScore_WR >= agradeScore) {
                        	sExamResult = "條件4 : WR=Pass,OP=NA-2-1(P)";//Added by Jack on 2023/08/28
                            sSql = " UPDATE sbl_cer_reg" +
                                " SET upt_date= sysdate" +
                                "    ,grade='A'" +
                                "    ,upt_user='" + m_User_id + "'" +
                                "    ," + sFileColumn + "='" + sFileName + "P.pdf" + "'" +
                                " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                                " AND emp_id = '" + m_EMP_id + "'" +
                                " AND cer_reg_no = '" + m_ExamRegNo + "'";
                        }
                        else{
                        	sExamResult = "條件4 : WR=Pass,OP=NA-2-2(P)";//Added by Jack on 2023/08/28
                            sSql = " UPDATE sbl_cer_reg" +
                                 " SET upt_date= sysdate" +
                                 "    ,grade='B'" +
                                 "    ,upt_user='" + m_User_id + "'" +
                                 "    ," + sFileColumn + "='" + sFileName + "P.pdf" + "'" +
                                 " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                                 " AND emp_id = '" + m_EMP_id + "'" +
                                 " AND cer_reg_no = '" + m_ExamRegNo + "'";
                        }

                        sScoreStatus="P";//產生執照和獎金
                    }
                }
            }
            else if (-1 < dScore_WR && dScore_WR < passScore) {
                if (dScore_OP >= passScore) {
                    //條件5, WR=Fail,OP=Pass
                	sExamResult = "條件5 : WR=Fail,OP=Pass";//Added by Jack on 2023/08/28
                    if (dOverMonth_OP >= 4) {
                        //sScore_OP=Pass卻超過3個月,視同全部沒過
                     	//sNextMonth--> sThisMonth modified by Jack on 2013/09/27 
                     	//(目前：需等下個月才能重考 --> 變更：當月就允許重考。                     
                    	sExamResult = "條件5 : WR=Fail,OP=Pass-1";//Added by Jack on 2023/08/28
                        sSql = " UPDATE sbl_cer_reg" +
                               " SET score_writing = ''" +
                               "    ,score_oper = ''" +
                               "    ,grade='C'" +
                               "    ,cer_date= to_date('" + sThisMonth + "','YYYY/MM')" +
                               "    ,upt_date= sysdate" +
                               "    ,upt_user='" + m_User_id + "'" +
                               "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                               " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                               " AND emp_id = '" + m_EMP_id + "'" +
                               " AND cer_reg_no = '" + m_ExamRegNo + "'";
                    }
                    else{
                    	sExamResult = "條件5 : WR=Fail,OP=Pass-2";//Added by Jack on 2023/08/28
                        sSql = " UPDATE sbl_cer_reg" +
                               " SET score_writing = ''" +
                               "    ,grade='C'" +
                               "    ,cer_date= to_date('" + sNextMonth + "','YYYY/MM')" +
                               "    ,upt_date= sysdate" +
                               "    ,upt_user='" + m_User_id + "'" +
                               "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                               " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                               " AND emp_id = '" + m_EMP_id + "'" +
                               " AND cer_reg_no = '" + m_ExamRegNo + "'";
                    }
                }
                else if (-1 <dScore_OP && dScore_OP < passScore) {
                    //條件6, WR=Fail,OP=Fail
                	sExamResult = "條件6 : WR=Fail,OP=Fail";//Added by Jack on 2023/08/28
                    sSql = " UPDATE sbl_cer_reg" +
                        " SET score_writing = ''" +
                        "    ,score_oper = ''" +
                        "    ,grade='C'" +
                        "    ,cer_date= to_date('" + sNextMonth + "','YYYY/MM')" +
                        "    ,upt_date= sysdate" +
                        "    ,upt_user='" + m_User_id + "'" +
                        "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                        " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                        " AND emp_id = '" + m_EMP_id + "'" +
                        " AND cer_reg_no = '" + m_ExamRegNo + "'";
                }
                else if(dScore_OP==-1){
                    //條件7, WR=Fail,OP=Null,等待成績中
                	sExamResult = "條件7 : WR=Fail,OP=Null,等待成績中";//Added by Jack on 2023/08/28
                    sSql = " UPDATE sbl_cer_reg" +
                           " SET upt_date= sysdate" +
                           "    ,upt_user='" + m_User_id + "'" +
                           "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                           " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                           " AND emp_id = '" + m_EMP_id + "'" +
                           " AND cer_reg_no = '" + m_ExamRegNo + "'";
                }
                else if(dScore_OP==-2){
                    //條件8, WR=Fail,OP=NA
                	sExamResult = "條件8 : WR=Fail,OP=NA";//Added by Jack on 2023/08/28
                    sSql = " UPDATE sbl_cer_reg" +
                        " SET score_writing = ''" +
                        "    ,grade='C'" +
                        "    ,cer_date= to_date('" + sNextMonth + "','YYYY/MM')" +
                        "    ,upt_date= sysdate" +
                        "    ,upt_user='" + m_User_id + "'" +
                        "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                        " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                        " AND emp_id = '" + m_EMP_id + "'" +
                        " AND cer_reg_no = '" + m_ExamRegNo + "'";
                }
            }
            else if(dScore_WR==-1){
                if(dScore_OP>=passScore){
                    //條件9, WR=Null,OP=Pass,等待成績中
                	sExamResult = "條件9 : WR=Null,OP=Pass,等待成績中";//Added by Jack on 2023/08/28
                    sSql = " UPDATE sbl_cer_reg" +
                           " SET upt_date= sysdate" +
                           "    ,upt_user='" + m_User_id + "'" +
                           "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                           " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                           " AND emp_id = '" + m_EMP_id + "'" +
                           " AND cer_reg_no = '" + m_ExamRegNo + "'";
                }
                else if(dScore_OP<passScore){
                    //條件10,WR=Null,OP=Fail,等待成績中
                	sExamResult = "條件10 : WR=Null,OP=Fail,等待成績中";//Added by Jack on 2023/08/28
                    sSql = " UPDATE sbl_cer_reg" +
                           " SET upt_date= sysdate" +
                           "    ,upt_user='" + m_User_id + "'" +
                           "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                           " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                           " AND emp_id = '" + m_EMP_id + "'" +
                           " AND cer_reg_no = '" + m_ExamRegNo + "'";
                }
                else if(dScore_OP==-1){
                    //條件11,WR=Null,OP=Null,無意義,不可能發生
                	sExamResult = "條件11 : WR=Null,OP=Null,無意義,不可能發生";//Added by Jack on 2023/08/28
                }
                else if(dScore_OP==-2){
                    //條件12,WR=Null,OP=NA,無意義,不可能發生
                	sExamResult = "條件12 : WR=Null,OP=NA,無意義,不可能發生";//Added by Jack on 2023/08/28
                }
            }
            else if(dScore_WR==-2){
                if(dScore_OP>=passScore){
                   //條件13,WR=NA,OP=Pass
                	sExamResult = "條件13 : WR=NA,OP=Pass";//Added by Jack on 2023/08/28
                   if (dOverMonth_OP >= 4) {
                       //時間超過3個月,該項成績視同沒過
                	   //sNextMonth--> sThisMonth modified by Jack on 2013/09/27 
                       //(目前：需等下個月才能重考 --> 變更：當月就允許重考。                      
                	   sExamResult = "條件13 : WR=NA,OP=Pass-1";//Added by Jack on 2023/08/28
                       sSql = " UPDATE sbl_cer_reg" +
                              " SET score_oper = ''" +
                              "    ,grade='C'" +
                              "    ,cer_date= to_date('" + sThisMonth + "','YYYY/MM')" +
                              "    ,upt_date= sysdate" +
                              "    ,upt_user='" + m_User_id + "'" +
                              "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                              " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                              " AND emp_id = '" + m_EMP_id + "'" +
                              " AND cer_reg_no = '" + m_ExamRegNo + "'";
                   }
                   else{
                       if (dScore_OP >= agradeScore) {
                    	   sExamResult = "條件13 : WR=NA,OP=Pass-2-1(P)";//Added by Jack on 2023/08/28
                           sSql = " UPDATE sbl_cer_reg" +
                               " SET upt_date= sysdate" +
                               "    ,grade='A'" +
                               "    ,upt_user='" + m_User_id + "'" +
                               "    ," + sFileColumn + "='" + sFileName + "P.pdf" + "'" +
                               " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                               " AND emp_id = '" + m_EMP_id + "'" +
                               " AND cer_reg_no = '" + m_ExamRegNo + "'";
                       }
                       else{
                    	   sExamResult = "條件13 : WR=NA,OP=Pass-2-2(P)";//Added by Jack on 2023/08/28
                           sSql = " UPDATE sbl_cer_reg" +
                                " SET upt_date= sysdate" +
                                "    ,grade='B'" +
                                "    ,upt_user='" + m_User_id + "'" +
                                "    ," + sFileColumn + "='" + sFileName + "P.pdf" + "'" +
                                " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                                " AND emp_id = '" + m_EMP_id + "'" +
                                " AND cer_reg_no = '" + m_ExamRegNo + "'";
                       }

                       sScoreStatus="P";//產生執照和獎金
                   }
               }
               else if(dScore_OP<passScore){
                   //條件14,WR=NA,OP=Fail
               		sExamResult = "條件14 : WR=NA,OP=Fail";//Added by Jack on 2023/08/28
                   sSql = " UPDATE sbl_cer_reg" +
                       " SET score_oper = ''" +
                       "    ,grade='C'" +
                       "    ,cer_date= to_date('" + sNextMonth + "','YYYY/MM')" +
                       "    ,upt_date= sysdate" +
                       "    ,upt_user='" + m_User_id + "'" +
                       "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                       " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                       " AND emp_id = '" + m_EMP_id + "'" +
                       " AND cer_reg_no = '" + m_ExamRegNo + "'";
               }
               else if(dScore_OP==-1){
                   //條件15,WR=NA,OP=Null,等待成績中
               		sExamResult = "條件15 : WR=NA,OP=Null,等待成績中";//Added by Jack on 2023/08/28
                   sSql = " UPDATE sbl_cer_reg" +
                          " SET upt_date= sysdate" +
                          "    ,upt_user='" + m_User_id + "'" +
                          "    ," + sFileColumn + "='" + sFileName + "F.pdf" + "'" +
                          " WHERE cer_item_id = '" + m_cer_item_id + "'" +
                          " AND emp_id = '" + m_EMP_id + "'" +
                          " AND cer_reg_no = '" + m_ExamRegNo + "'";
               }
               else if(dScore_OP==-2){
                   //條件16,WR=NA,OP=NA,無意義,不可能發生
                	sExamResult = "條件16 : WR=NA,OP=NA,無意義,不可能發生";//Added by Jack on 2023/08/28
               }
            }

            //執行16個條件中的1個sSql結果
            jdbc.updatePreparation(sSql,con);
            //Added by Jack on 2023/08/28
        	if (bNeedToLogSysLog){
        		BADAO.insertConfidential_SysLog(con, sPlatform, sUserID, sExecFunction+"_成績計算_6_UpdateScore", "/*" + sExamResult + "*/" + sSql );
        	}
            
            if(oRS!=null){
            	oRS.close();
            }
            //Marked by Jack on 2023/08/31. (搬到下方)
            //if(con!=null){
            //	con.close();
            //}            
            //jdbc.close();
            System.out.println("scoreCompute 16 sSql="+sSql +"\ndScore_WR="+dScore_WR +"\ndScore_OP="+dScore_OP);

            //PDF檔輸出路徑,Property.dirpdf=D://temp,若無設定則為C:\Documents and Settings\帳號\ses.property
            com.mxic.ses.util.createPdfUtil.genPdf(mArrayList, m_EMP_id, m_cer_item_id , m_ScoreType,sScoreStatus);

            //若sScoreStatus="P",產生執照和獎金
            if(sScoreStatus=="P"){
                //Added by Jack on 2023/08/28
            	if (bNeedToLogSysLog){
            		BADAO.insertConfidential_SysLog(con, sPlatform, sUserID, sExecFunction+"_成績計算_7_CreateLicence", m_EMP_id +"/"+m_cer_item_id+"/"+m_User_id+"/"+m_Station_id );
            	}
              licence.generateLicence(m_EMP_id,m_cer_item_id,m_User_id,m_Station_id);
            }
            //Added by Jack on 2023/08/31.
            if(con!=null){
             	con.close();
            }            
            jdbc.close();
              
            return true;
        }
        catch (SQLException Se) {
            System.err.println("SQLException Message="+Se.getMessage()+
                               "\n LocalizedMessage=" +Se.getLocalizedMessage()+
                               "\n SQLState=" +Se.getSQLState());
            Se.printStackTrace();
            try{
            	con.close();
            }catch(SQLException sqle){
            	sqle.printStackTrace();
            }
            jdbc.close();
            return false;
        }
    }
}
