using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.BlendCreation
{
    [Serializable]
    public class BlendSheetDto
    {
        public long Id { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string BrokerCode { get; set; }
        public string GardenMark { get; set; }
        public string SaleDate { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string TeaCertificates { get; set; }
        public decimal Packs { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
        public string TinNo { get; set; }
    }

    public class BlendSheetParam
    {
        public string ContractName { get; set; }
        public string StandardName { get; set; }
        public string BlendNo { get; set; }
        public string DateOfCompletion { get; set; }
        public string ClosedRate { get; set; }
        public string ExportQty { get; set; }
        public string BB { get; set; }
        public string BG { get; set; }
        public string STDBG { get; set; }
        public string BL { get; set; }
        public bool IsClosed { get; set; }
        public bool IsConverted { get; set; }
        public string TrialVersion { get; set; }
    }
}
