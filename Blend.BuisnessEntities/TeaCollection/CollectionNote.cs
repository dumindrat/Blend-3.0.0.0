using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.TeaCollection
{
    public class CollectionNote : BaseEntity
    {
        public int Id { get; set; }
        public string CollectionNo { get; set; }
        public string CreateDate { get; set; }

    }

    public class CollectionNoteItem : BaseEntity
    {
        public bool StockReceived { get; set; }
        public int CollectionNoteId { get; set; }
        public string CollectionNoteNo { get; set; }
        public int PurchaseId { get; set; }
        public string CollectionDate { get; set; }
        public string StoreLocation { get; set; }
    }

    public class StraightLIneCollection : BaseEntity
    {
        public int PurchaseId { get; set; }
        public string BoxNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string BrokerCode { get; set; }
        public string GardenMark { get; set; }
        public string SaleDate { get; set; }
        public string Grade { get; set; }
        //public string Standard { get; set; }
        public decimal NetWeight { get; set; }
        public decimal Packs { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
        public string CollectionNoteNo { get; set; }
        public int CollectionNoteId { get; set; }

        public string CollectionDate { get; set; }
        public string StoreLocation { get; set; }
    }

    public class CollectionReportDto
    {
        public string BoxNo { get; set; }
        public string LotNo { get; set; }
        public string Invoice { get; set; }
        public string BrokerCode { get; set; }
        public string GardenMark { get; set; }
        public string SaleDate { get; set; }
        public string SaleType { get; set; }
        public string SaleCode { get; set; }
        public string Grade { get; set; }
        public string NetWeight { get; set; }
        public string Packs { get; set; }
        public string Weight { get; set; }
        public string CollectionDate { get; set; }
        public string StockReceivedDate { get; set; }
        public int StockReceived { get; set; }
        public string StoreLocation { get; set; }

    }
}
