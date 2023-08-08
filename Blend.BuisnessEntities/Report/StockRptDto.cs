using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Report
{
    public class StockRptDto
    {
        public string Grade { get; set; }
        public string Standard { get; set; }
        public bool IsWithValues { get; set; }
        public bool IsWithAllocatedLine { get; set; }
        public bool IsAllocatedLineOnly { get; set; }
        public bool IsAvailableLineOnly { get; set; }
        public bool IsWithRejectAndShortLine { get; set; }
        public bool IsExcelFormat { get; set; }
    }

    public class AgeAnalysisStockRptDto
    {
        public string AsAtDate { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public bool IsWithValues { get; set; }
        public bool IsExcelFormat { get; set; }
    }

    public class StoresAnalysis
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool IsExcelFormat { get; set; }
    }

    public class StoresAnalysisData
    {
        public string BoxNo { get; set; }
        public string CollectionDate { get; set; }
        public int ReceivedPacks { get; set; }
        public int DayCount { get; set; }
        public string ClosedDate { get; set; }
        public decimal Packs { get; set; }
        public decimal Qty { get; set; }
        public string Status { get; set; }
    }
}
