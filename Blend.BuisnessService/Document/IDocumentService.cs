using Blend.BuisnessEntities.TransDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Document
{
    public interface IDocumentService
    {
        DocumentNo GetDocumentNo(int docType);
        IEnumerable<DocumentNo> GetDocumentData();
        IEnumerable<DocumentNo> GetDocumentDataByModule(int moduleId);
        bool ResetDocumentNo(DocumentNo documentInfo);
        bool IncrementDocumentNo(int docType);
    }
}
