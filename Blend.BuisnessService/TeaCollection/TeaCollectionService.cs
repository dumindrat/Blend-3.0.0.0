using Blend.BuisnessEntities.TeaCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.TeaCollection
{
    public class TeaCollectionService : BaseManagement<CollectionNote>, ITeaCollectionService
    {

        public IList<CollectionNote> GetAllCollectionNotes(string serchStr)
        {
            var searchBy = serchStr == "all" ? "" : serchStr;
            var mapper = new DataMapper<CollectionNote>("GetCollectionNotes", new Hashtable() { { "Id", searchBy } });

            var result = mapper.ObjectList();
            return result;
        }


        public IList<StraightLIneCollection> GetStraightLineItems(string serchStr)
        {
            var searchBy = serchStr == "all" ? "" : serchStr;
            var mapper = new DataMapper<StraightLIneCollection>("GetCollectionNotesItems", new Hashtable() { { "BoxNo", searchBy } });

            var result = mapper.ObjectList();
            return result;
        }

        public bool CreateCollectionNote(BuisnessEntities.TeaCollection.CollectionNote obj)
        {
            dataOperation.CommandText("trn_CollectionNoteInsert");
            dataOperation.AddParam("@User", obj.User);
            dataOperation.DoExecute();
            return true;
        }


        public CollectionNote GetAllCollectionNote()
        {
            var mapper = new DataMapper<CollectionNote>("GetCollectionNote", new Hashtable());

            var result = mapper.Object();
            return result;
        }


        public bool MarkCollectionNoteItem(CollectionNoteItem obj)
        {
            dataOperation.CommandText("trn_CollectionNoteUpdateLines");
            dataOperation.AddParam("@StockReceived", obj.StockReceived);
            dataOperation.AddParam("@CollectionNoteNo", obj.CollectionNoteId);
            dataOperation.AddParam("@PurchaseId", obj.PurchaseId);
            dataOperation.AddParam("@CollectionDate", obj.CollectionDate);
            dataOperation.AddParam("@User", obj.User);
            dataOperation.DoExecute();
            return true;
        }

        public bool MarkCollectionNoteStoreLocation(CollectionNoteItem obj)
        {
            dataOperation.CommandText("trn_CollectionNoteMarkStoreLocation");
            dataOperation.AddParam("@PurchaseId", obj.PurchaseId);
            dataOperation.AddParam("@StoreLocation", obj.StoreLocation);
            dataOperation.AddParam("@User", obj.User);
            dataOperation.DoExecute();
            return true;
        }


        public IList<StraightLIneCollection> GetStraightLineByBoxByLotByInv(string boxNo, string lotNo, string invNo)
        {
            var searchBy = boxNo == "all" ? "" : boxNo;
            var searchLotNo = lotNo == "all" ? "" : lotNo;
            var searchInvNo = invNo == "all" ? "" : invNo;
            var mapper = new DataMapper<StraightLIneCollection>("GetCollectionNotesItems",
                new Hashtable() { { "BoxNo", searchBy }, { "LotNo", searchLotNo }, { "Invoice", searchInvNo } });

            var result = mapper.ObjectList();
            return result;
        }


        public IList<CollectionReportDto> GetCollectionNotePrint(int no)
        {
            var mapper = new DataMapper<CollectionReportDto>("PrintCollectionNote",
               new Hashtable() { { "CollectionNoteNo", no } });

            var result = mapper.ObjectList();
            return result;
        }
    }
}
