                var pdfView = new ViewAsPdf("CreatePdf", vm)
                {
                    PageSize = Rotativa.Options.Size.A4,
                    CustomSwitches =
                        "--header-right \"General PD\\nMacronix Proprietary\" " +
                        "--header-font-size 9 " +
                        "--footer-center \"This document may contain personal data, and shall be used for MXIC's business only.\" " +
                        "--footer-font-size 8 " +
                        "--footer-line"
                };

var pdfView = new ViewAsPdf("CreatePdf", vm)
{
    PageSize = Rotativa.Options.Size.A4,
    CustomSwitches =
        "--header-right \"General PD\nMacronix Proprietary\" " + // 直接換行
        "--header-font-size 9 " +
        "--footer-center \"This document may contain personal data...\" " +
        "--footer-font-size 8 " +
        "--footer-line"
};
