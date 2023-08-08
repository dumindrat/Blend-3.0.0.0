using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Stock
{
    public class StraightLineStockReportDto
    {
        public string AllocatedBlend { get; set; }
        public string Id { get; set; }
        public string TinNo { get; set; }
        public string BoxNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string BrokerCode { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string TeaCertificates { get; set; }
        public string GardenMark { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public string SaleDate { get; set; }
        public string NetWeight { get; set; }
        public string Packs { get; set; }
        public string Qty { get; set; }
        public string Rate { get; set; }
        public string Value { get; set; }
    }

    public class FullStockReportDto
    {
        public int SeqNo { get; set; }
        public string StockType { get; set; }
        public string AllocatedBlend { get; set; }
        public string Id { get; set; }
        public string TinNo { get; set; }
        public string BoxNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string BrokerCode { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string TeaCertificates { get; set; }
        public string GardenMark { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public string SaleDate { get; set; }
        public string NetWeight { get; set; }
        public string Packs { get; set; }
        public string Qty { get; set; }
        public string Rate { get; set; }
        public string Value { get; set; }
    }

    public class StraightLineAgeAnalysisReportDto
    {
        public string CollectionCode { get; set; }
        public string BoxNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string BrokerCode { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string GardenMark { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public string SaleDate { get; set; }
        public string NetWeight { get; set; }
        public string Packs { get; set; }
        public string Qty { get; set; }
        public string Rate { get; set; }
        public string Value { get; set; }
    }

    public class AgeAnalysisParam
    {
        public string AsAtDate { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
    }
}
