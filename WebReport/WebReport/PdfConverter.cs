using PDfCreator;
using PDfCreator.Print;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PDfCreator.Models;
using ImsPosLibraryCore.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Dapper;
using System.Text.Json;

namespace WebReport
{
    public class PdfConverter
    {
        private InvoicePrinting inoicePrinting;
        private Invoice inv;
        private IWebHostEnvironment _env;
        public PdfConverter(IWebHostEnvironment env)
        {
            _env = env;
        }
        public  FunctionResponse OpenPdfReport(PdfReportParameter reportParameter)
        {
            var filename = reportParameter.filename;
            if (string.IsNullOrEmpty(filename)) return new FunctionResponse { status = "error", result = "Invalid file Name" };
            var reportDirectory = _env.WebRootPath;
            var reportFile = Path.Combine(reportDirectory, "IMSReport", filename);
            if (File.Exists(reportFile) == false) return new FunctionResponse { status = "error", result = "File not found" };
            string str = File.ReadAllText(reportFile);
            inoicePrinting = new InvoicePrinting();
            inv = inoicePrinting.LoadInvFromJsonString(str);
            if (inv.Document.DetailDataType == DataType.SQLServer)
            {
                if (!string.IsNullOrEmpty(inv.Document.QueryParameter))
                {
                    FunctionResponse res = CompareParameter(reportParameter.Parameter, inv.Document.QueryParameter);
                    if (res.status == "error") return res;
                    inoicePrinting.InputParameters = res.result as Dictionary<string, object>;
                    PrepareSqlReportData(inoicePrinting, inv, inoicePrinting.InputParameters);
                }

            }
            //var pdfFile = reportDirectory
            
            using (MemoryStream memStream = new MemoryStream())
            {
                
                inoicePrinting.PrintInvoice(inv, "",  memStream: memStream);
               
                return new FunctionResponse { status = "ok", result = memStream.ToArray() };
            }
           

        }

        public FunctionResponse OpenPdfReportMultipleParameter(PdfReportMultiParameter pdfReportMultiParameter)
        {
            var filename = pdfReportMultiParameter.filename;
            if (string.IsNullOrEmpty(filename)) return new FunctionResponse { status = "error", result = "Invalid file Name" };
            var reportDirectory = _env.WebRootPath;
            var reportFile = Path.Combine(reportDirectory, "IMSReport", filename);
            if (File.Exists(reportFile) == false) return new FunctionResponse { status = "error", result = "File not found" };
            string str = File.ReadAllText(reportFile);
            inoicePrinting = new InvoicePrinting();
            inv = inoicePrinting.LoadInvFromJsonString(str);
            List<byte[]> pdfFiles = new List<byte[]>();
            foreach (var parameter in pdfReportMultiParameter.Parameters)
            {
                if (inv.Document.DetailDataType == DataType.SQLServer)
                {
                    if (!string.IsNullOrEmpty(inv.Document.QueryParameter))
                    {
                        FunctionResponse res = CompareParameter(parameter, inv.Document.QueryParameter);
                        if (res.status == "error") return res;
                        inoicePrinting.InputParameters = res.result as Dictionary<string, object>;
                        if (pdfReportMultiParameter.IsSameDataQuery == true && inoicePrinting.DetailData !=null ) 
                        { PrepareSqlReportData(inoicePrinting, inv, inoicePrinting.InputParameters); }
                    }
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        inoicePrinting.PrintInvoice(inv, "", memStream: memStream);
                        pdfFiles.Add(memStream.ToArray());
                        //return new FunctionResponse { status = "ok", result = memStream.ToArray() };
                    }
                }
            }
            
            //var pdfFile = reportDirectory
            
            
            
            using (MemoryStream memoryStream = new MemoryStream())
            {
                inoicePrinting.MergePdfinMemory(memoryStream, pdfFiles);
                return new FunctionResponse { status = "ok", result = memoryStream.ToArray() };
            }

        }

        public FunctionResponse OpenMultiplePdfReport(List<PdfReportParameter> Parameters)
        {
            List<byte[]> pdfFiles = new List<byte[]>();
            foreach (var rptparameter in Parameters)
            {
                var filename = rptparameter.filename;
            if (string.IsNullOrEmpty(filename)) return new FunctionResponse { status = "error", result = "Invalid file Name" };
            var reportDirectory = _env.WebRootPath;
            var reportFile = Path.Combine(reportDirectory, "IMSReport", filename);
            if (File.Exists(reportFile) == false) return new FunctionResponse { status = "error", result = "File not found" };
            string str = File.ReadAllText(reportFile);
            inoicePrinting = new InvoicePrinting();
            inv = inoicePrinting.LoadInvFromJsonString(str);
           
                if (inv.Document.DetailDataType == DataType.SQLServer)
                {
                    if (!string.IsNullOrEmpty(inv.Document.QueryParameter))
                    {
                        FunctionResponse res = CompareParameter(rptparameter.Parameter, inv.Document.QueryParameter);
                        if (res.status == "error") return new FunctionResponse { status = "error", result = $"({filename}) {res}" };
                        inoicePrinting.InputParameters = res.result as Dictionary<string, object>;
                        PrepareSqlReportData(inoicePrinting, inv, inoicePrinting.InputParameters); 
                    }
                    
                }
                using (MemoryStream memStream = new MemoryStream())
                {
                    inoicePrinting.PrintInvoice(inv, "", memStream: memStream);
                    pdfFiles.Add(memStream.ToArray());
                    //return new FunctionResponse { status = "ok", result = memStream.ToArray() };
                }
            }

            //var pdfFile = reportDirectory



            using (MemoryStream memoryStream = new MemoryStream())
            {
                inoicePrinting.MergePdfinMemory(memoryStream, pdfFiles);
                return new FunctionResponse { status = "ok", result = memoryStream.ToArray() };
            }

        }
        private FunctionResponse CompareParameter(IDictionary<string,object> Arguments, string parameters)
        {
            string[] qparams = parameters.Split(",");
            Dictionary<string, Object> paramObject = new Dictionary<string, Object>();
            foreach (var qparam in qparams)
            {
                try
                {
                    var paramValue = Arguments[qparam];
                    if (paramValue == null)
                    {
                        string jsonParam = "{" + string.Join(",", qparam.Select(x => { var s = $"\"{x}\":\"{x}\""; return s; })) + "}";
                        return new FunctionResponse { status = "error", result = "Required parameter not found provide query parameter in following format '" + jsonParam + "'" };
                    }
                    JsonElement jparam = (JsonElement)paramValue;
                    paramObject.Add(qparam,GetObject(jparam));
                    
                }
                catch
                {
                    string jsonParam = "{" + string.Join(",", qparams.Select(x => { var s = $"\"{x}\":\"{x}\""; return s; })) + "}";
                    return new FunctionResponse { status = "error", result = "Required parameter not found provide query parameter in following format '" + jsonParam + "'" };
                }
            }
            return new FunctionResponse { status = "ok", result = paramObject };
        }

        private object GetObject(JsonElement jParam)
        {
            object result =null ;
            switch (jParam.ValueKind)
            {
                case JsonValueKind.Null:
                    result = null;
                    break;
                case JsonValueKind.Number:
                    result = jParam.GetDouble();
                    break;
                case JsonValueKind.False:
                    result = false;
                    break;
                case JsonValueKind.True:
                    result = true;
                    break;
                case JsonValueKind.Undefined:
                    result = null;
                    break;
                case JsonValueKind.String:
                    result = jParam.GetString();
                    break;
                
                    //case JsonValueKind.Object:
                    //    result = new ReflectionDynamicObject
                    //    {
                    //        RealObject = jParam
                    //    };
                    //    break;
                    //case JsonValueKind.Array:
                    //    result = srcData.EnumerateArray()
                    //        .Select(o => new ReflectionDynamicObject { RealObject = o })
                    //        .ToArray();
                    //break;
            }
            return result;
        }
        //private void LoadPdf(string ExportFileName = "")
        //{
        //    try
        //    {
        //        string DEST = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test1.pdf");
        //        if (string.IsNullOrEmpty(ExportFileName))
        //        {
        //            if (Directory.Exists(ExportFileName) == true)
        //            {
        //                DEST = ExportFileName;
        //            }
        //        }
        //        FileInfo file = new FileInfo(DEST);
        //        string jsonParam = "";
        //        IDictionary<string, object> dictParam = new Dictionary<string, object>();

        //        if (string.IsNullOrEmpty(inv.Document.QueryParameter) == false)
        //        {
        //            string paramString = inv.Document.QueryParameter;
        //            if (inoicePrinting.DetailData == null || inoicePrinting.DetailData.Count() == 0)
        //            {
        //                jsonParam = OpenParameter_Dialog(paramString);
        //                var jobjParam = JsonConvert.DeserializeObject<JObject>(jsonParam.ToUpper());
        //                dictParam = jobjParam.ToObject<Dictionary<string, object>>();
        //                inoicePrinting.InputParameters = dictParam;
        //                PrepareSqlReportData(inoicePrinting, inv, dictParam);
        //            }


        //        }


        //        inoicePrinting.PrintInvoice(inv, DEST);

        //        pdfDocumentViewer1.LoadFromFile(DEST);
        //    }
        //    catch (Exception ex)
        //    {

        //        MessageBox.Show(ex.Message);
        //    }
        //}

        private void PrepareSqlReportData(InvoicePrinting invPrint, Invoice inv, IDictionary<string, object> param)
        {
            var constring = inv.Document.ConnectionString;
            var reportQuery = inv.Document.ReportSource;
            var detailQuery = inv.Document.DetailSource;

            using (SqlConnection con = new SqlConnection(constring))
            {

                IDictionary<string, object> reportData = null;
                if (string.IsNullOrEmpty(reportQuery) == false)
                {
                    reportData = (IDictionary<string, object>)con.Query(reportQuery, param).FirstOrDefault();
                    invPrint.ReportData = reportData;
                }
                if (string.IsNullOrEmpty(detailQuery) == false)
                {
                    List<IDictionary<string, object>> detailData = con.Query(detailQuery, param).Select(row => (IDictionary<string, object>)row).ToList();
                    invPrint.DetailData = detailData;
                }


            }
        }
        //private string OpenParameter_Dialog(string paramString)
        //{
        //    if (string.IsNullOrEmpty(paramString)) return "";
        //    try
        //    {
        //        ParameterDialog pmDlg = new ParameterDialog();
        //        pmDlg.LoadParameters(paramString, inoicePrinting.InputParameters);
        //        if (pmDlg.ShowDialog() == DialogResult.OK)
        //        {
        //            string jsonParam = pmDlg.jsonParam;
        //            return jsonParam;
        //        }
        //        return "";
        //        //else
        //        //{
        //        //    string[] par = paramString.Split(',');
        //        //    string jsonParam = "{" + string.Join(",", par.Select(x => { var s = $"\"{x}:\"\""; return s; })) + "}";
        //        //    return jsonParam;
        //        //}
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}
    }
}
