using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Purchase
{
    public class StraightLine : BaseEntity, ICloneable
    {
        public long Id { get; set; }
        public long RepeatingId { get; set; }

        public long StockId { get; set; }
        public long RepeatingStockId { get; set; }

        public string BoxNo { get; set; }
        public int? SaleId { get; set; }
        public string SaleCode { get; set; }
        public string Invoice { get; set; }
        public string LotNo { get; set; }
        public int? BrokerId { get; set; }
        public string BrokerCode { get; set; }
        public string SaleType { get; set; }
        public string Grade { get; set; }
        public string Standard { get; set; }
        public string GardenMark { get; set; }
        public string TeaCertificates { get; set; }
        public decimal Net1 { get; set; }
        public decimal Pack1 { get; set; }
        public decimal Net2 { get; set; }
        public decimal Pack2 { get; set; }
        public byte[] RowVersion1 { get; set; }
        public byte[] RowVersion2 { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Packs { get; set; }
        public decimal? Weight { get; set; }
        public decimal? ReducedQty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Value { get; set; }
        public string TinNo { get; set; }

        public bool IsRepeated { get; set; }
        public decimal? VirtualWeight { get; set; }
        public decimal? VirtualValue { get; set; }

        //public List<TeaCertificates> TeaCertificateList { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class TeaCertificates
    {
        public string CertifiedName { get; set; }
        public bool IsCertified { get; set; }
    }

    public class SraightLineUploadStatus
    {
        public string LotNo { get; set; }
        public string BoxNo { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

    }

    public class UploadStatus
    {
        public int HttpCode { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<SraightLineUploadStatus> UploadData { get; set; }
    }

    public class UploadData
    {
        public List<SraightLineUploadDto> ItemList { get; set; }
        public bool IsValid { get; set; }
    }
    public class LineStatus
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
    public class SraightLineUploadDto
    {
        public int Sequence { get; set; }
        public string Sc { get; set; }
        public string St { get; set; }
        public string Lot { get; set; }
        public string Mark { get; set; }
        public string Inv { get; set; }
        public string Grade { get; set; }
        public string Bags { get; set; }
        public string Net { get; set; }
        public string RedQty { get; set; }
        public string Qty { get; set; }
        public string Price { get; set; }
        public string Value { get; set; }
        public LineStatus Status { get; set; }
    }

    public class UploadLog : BaseEntity
    {
        public string FileName { get; set; }
        public int SaleId { get; set; }
        public int BrokerId { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public int ErrorRecords { get; set; }
        public int TotalRecords { get; set; }
        public string UploadData { get; set; }

    }

    public class SaleUploadDto : BaseEntity
    {
        public string FileName { get; set; }
        public int SaleId { get; set; }
        public int BrokerId { get; set; }
        public string SaleCode { get; set; }
        public string SaleType { get; set; }
        public List<StraightLine> StraightLines { get; set; }
    }

    public class UploadStraightLine
    {
        public string LotNo { get; set; }
        public string InvNo { get; set; }
        public string Mark { get; set; }
        public string Grade { get; set; }
        public string Net { get; set; }
        public string Bag { get; set; }
        public string Qty { get; set; }
        public string Price { get; set; }
        public string ReducedQty { get; set; }
    }
}
