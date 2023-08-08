using Blend.BuisnessEntities.Download.Asycuda;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Blend.BuisnessService.BlendCreation;

namespace Blend.BuisnessService.Download
{
    class DownloadFilesService : BaseManagement<MemoryStream>, IDownloadFileService
    {
        private string m_exePath = Path.GetDirectoryName(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
        private IBlendCreationService _blendCreatetionSvc;

        public DownloadFilesService(IBlendCreationService blendCreatetionSvc)
        {
            _blendCreatetionSvc = blendCreatetionSvc;
        }

        public string SaveAsycudaFile(int blendId)
        {
            try
            {
                var blend = _blendCreatetionSvc.GetBlendInfoById(blendId);
                if (blend == null)
                    return string.Empty;

                var file = m_exePath + "\\" + "Downloads" + "\\" + blend.BlendNo.Replace('/', '-') + ".xls";

                var mapper = new DataMapper<AsycudaDataSL>("GetAsycudaS", new Hashtable() { { "BlendId", blendId } });
                var result = mapper.ObjectList();

                var mapperb = new DataMapper<AsycudaDataBL>("GetAsycudaB", new Hashtable() { { "BlendId", blendId } });
                var resultblend = mapperb.ObjectList();

                if (result != null || resultblend != null)
                {
                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        excel.Workbook.Worksheets.Add("SL");
                        excel.Workbook.Worksheets.Add("BL");

                        var excelWorksheet = excel.Workbook.Worksheets["SL"];
                        List<string[]> headerRow = new List<string[]>() { new string[] { "Date of sale", "Sale Code", "Br Code", "Lot No",
                            "Inv No" ,"Garden Mark","Grade","Rate per KG","Quantity in KG","Total Value"}   };

                        string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";
                        excelWorksheet.Cells[headerRange].LoadFromArrays(headerRow);
                        var count = 2;
                        foreach (var item in result)
                        {
                            excelWorksheet.Cells["A" + count.ToString()].Value = item.SaleDate;
                            excelWorksheet.Cells["B" + count.ToString()].Value = Convert.ToInt16(item.SaleCode);
                            excelWorksheet.Cells["C" + count.ToString()].Value = item.BrokerCode;
                            excelWorksheet.Cells["D" + count.ToString()].Value = item.LotNo;
                            excelWorksheet.Cells["E" + count.ToString()].Value = item.Invoice;
                            excelWorksheet.Cells["F" + count.ToString()].Value = item.GardenMark;
                            excelWorksheet.Cells["G" + count.ToString()].Value = item.Grade;
                            excelWorksheet.Cells["H" + count.ToString()].Value = Convert.ToDecimal(item.Rate);
                            excelWorksheet.Cells["I" + count.ToString()].Value = Convert.ToDecimal(item.Qty);
                            excelWorksheet.Cells["J" + count.ToString()].Value = Convert.ToDecimal(item.Value);
                            count++;
                        }
                        excelWorksheet = excel.Workbook.Worksheets["BL"];
                        List<string[]> headerRowBL = new List<string[]>() { new string[] { "Date of Blend", "Blend No", "Blend Balance", "Blend Gain", "Blend Avg per KG" } };

                        string headerRangeBL = "A1:" + Char.ConvertFromUtf32(headerRowBL[0].Length + 64) + "1";
                        excelWorksheet.Cells[headerRangeBL].LoadFromArrays(headerRowBL);

                        count = 2;
                        foreach (var item in resultblend)
                        {
                            excelWorksheet.Cells["A" + count.ToString()].Value = item.ClosedDate;
                            excelWorksheet.Cells["B" + count.ToString()].Value = item.BlendNo;
                            excelWorksheet.Cells["C" + count.ToString()].Value = Convert.ToDecimal(item.BB);
                            excelWorksheet.Cells["D" + count.ToString()].Value = Convert.ToDecimal(item.BG);
                            excelWorksheet.Cells["E" + count.ToString()].Value = Convert.ToDecimal(item.ClosedRate);
                            count++;
                        }

                        FileInfo excelFile = new FileInfo(file);
                        //var tst = excel.Stream;
                        excel.SaveAs(excelFile);
                    }
                    return file;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogWrite("SaveAsycudaFile fails. Error " + ex.Message);
                return string.Empty;
            }

        }


        public string SaveSLAsTextFile(int blendId)
        {
            var blend = _blendCreatetionSvc.GetBlendInfoById(blendId);
            if (blend == null)
                return string.Empty;

            var filepath = m_exePath + "\\" + "Downloads" + "\\" + blend.BlendNo.Replace('/', '-') + "-SL" + ".txt";

            var mapper = new DataMapper<AsycudaDataSL>("GetAsycudaS", new Hashtable() { { "BlendId", blendId } });
            var result = mapper.ObjectList();
            if (result != null)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, true))
                {
                    file.WriteLine(@"Date of sale" + "\t" + @"Sale Code" + "\t" + @"Br Code" + "\t" + @"Lot No" + "\t" + @"Inv No" + "\t" + @"Garden Mark" + "\t" + @"Grade" + "\t" + @"Rate per KG" + "\t" + @"Quantity in KG" + "\t" + @"Total Value");
                    foreach (var item in result)
                    {
                        file.WriteLine(item.SaleDate + "\t" + item.SaleCode + "\t" + item.BrokerCode + "\t" + item.LotNo
                            + "\t" + item.Invoice + "\t" + item.GardenMark + "\t" + item.Grade + "\t" + item.Rate + "\t" + item.Qty + "\t" + item.Value);
                    }
                }
            }
            return filepath;
        }


        public string SaveBLAsTextFile(int blendId)
        {
            var blend = _blendCreatetionSvc.GetBlendInfoById(blendId);
            if (blend == null)
                return string.Empty;

            var filepath = m_exePath + "\\" + "Downloads" + "\\" + blend.BlendNo.Replace('/', '-') + "-BL" + ".txt";

            var mapperb = new DataMapper<AsycudaDataBL>("GetAsycudaB", new Hashtable() { { "BlendId", blendId } });
            var resultblend = mapperb.ObjectList();
            if (resultblend != null)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, true))
                {
                    file.WriteLine(@"Date of Blend" + "\t" + @"Blend No" + "\t" + @"Blend Balance" + "\t" + @"Blend Gain" + "\t" + @"Blend Avg per KG");
                    foreach (var item in resultblend)
                    {
                        file.WriteLine(item.ClosedDate + "\t" + item.BlendNo + "\t" + item.BB + "\t" + item.BG
                            + "\t" + item.ClosedRate);
                    }
                }
            }
            return filepath;
        }
    }
}
