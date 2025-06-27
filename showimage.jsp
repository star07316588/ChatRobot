<%@ page contentType="text/html; charset=Big5" pageEncoding="BIG5"%>
<%@ page errorPage="ExamErrMsg.jsp"%>
<%@ page import="com.mxic.ses.util.Property"%>
<%@ page import="java.io.*"%>
<%@ page session="false"%>
<%
	String fileName=request.getParameter("file");
	String temp=request.getParameter("temp");
  try{
		FileInputStream fis=null;
		if (temp != null)
			fis = new FileInputStream (Property.temp + File.separator + fileName);
		else
  		fis = new FileInputStream (Property.dirImage + File.separator + fileName);
		response.reset();
  	response.setContentType("image/jpeg");
		response.setHeader("Content-Disposition", "inline;filename=" + fileName);

  	OutputStream os=response.getOutputStream();

  	int byteRead;
    while(-1 != (byteRead = fis.read())) {
        os.write(byteRead);
    }
    if (fis != null)
        fis.close();
    os.flush();
		os.close();
//    response.setStatus( response.SC_OK );
    //response.flushBuffer();
  }catch(FileNotFoundException e1){
		response.reset();
		out.println("<script>alert('系統找不到指定的檔案');window.close()</script>");
  }catch(Exception e1){
		response.reset();
		out.println("<script>alert('檔案錯誤');window.close()</script>");
  }
%>
