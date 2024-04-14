using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Purchase
{
    public class SaleSummaryDto
    {
        public int NoOfLots { get; set; }
        public int BrokerId { get; set; }
        public string SaleDate { get; set; }
        public string BrokerName { get; set; }
        public decimal Weight { get; set; }
        public decimal Value { get; set; }
        public string SaleType { get; set; }
        public decimal NinetyPercVal { get; set; }
        public decimal TenPercVal { get; set; }
        public decimal HundredPercVal { get; set; }
    }

    public class PurchaseSheetDto
    {
        public string BoxNo { get; set; }
        public string TinNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string GardenMark { get; set; }
        public string SaleDate { get; set; }
        public string SaleType { get; set; }
        public string SaleCode { get; set; }
        public string BrokerName { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Packs { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
        public bool IsRepeated { get; set; }
        public decimal VirtualValue { get; set; }
        public decimal VirtualWeight { get; set; }
        public string TeaCertificates { get; set; }
    }

    public class CanisterSheetDto
    {
        public string TinNo { get; set; }
        public string BoxNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string GardenMark { get; set; }
        public string SaleDate { get; set; }
        public string SaleType { get; set; }
        public string BrokerName { get; set; }
        public string Rate { get; set; }
        public string LineQty { get; set; }
        public string RepeatedLineQty { get; set; }
        public string Qty { get; set; }
    }

    public class LineAdvanceFilterDto : BaseEntity
    {
        public string FromSaleDate { get; set; }
        public string ToSaleDate { get; set; }
        public string LotNo { get; set; }
        public string Standard { get; set; }
        public string Invoice { get; set; }
        public string BrokerCode { get; set; }
        public string GardenMark { get; set; }
    }
}
