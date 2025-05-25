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
        DocumentNo GetDocumentNo(int docType,int companyId);
        IEnumerable<DocumentNo> GetDocumentData();
        IEnumerable<DocumentNo> GetDocumentDataByModule(int moduleId, int companyId);
        bool ResetDocumentNo(DocumentNo documentInfo);
        bool IncrementDocumentNo(int docType);
    }
}
