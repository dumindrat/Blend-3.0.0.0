using Blend.BuisnessEntities.TeaCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.TeaCollection
{
    public interface ITeaCollectionService
    {
        IList<CollectionNote> GetAllCollectionNotes(string serchStr);
        CollectionNote GetAllCollectionNote();
        IList<StraightLIneCollection> GetStraightLineItems(string serchStr);
        IList<StraightLIneCollection> GetStraightLineByBoxByLotByInv(string boxNo,string lotNo,string  invNo);

        bool CreateCollectionNote(CollectionNote obj);
        bool MarkCollectionNoteItem(CollectionNoteItem obj);
        bool MarkCollectionNoteStoreLocation(CollectionNoteItem obj);
        IList<CollectionReportDto> GetCollectionNotePrint(int no);
    }
}
 