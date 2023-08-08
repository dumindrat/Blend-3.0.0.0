using Blend.BuisnessEntities.TransDocument;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Document
{
    public class DocumentService : BaseManagement<DocumentNo>, IDocumentService
    {
        public DocumentNo GetDocumentNo(int docType)
        {
            try
            {
                var mapper = new DataMapper<DocumentNo>("GetDocumentNo", new Hashtable() { { "doctypes", docType } });

                var result = mapper.Object();
                if (result != null)
                {
                    var docnumwithformat = (result.DocFormat + result.DocNo);
                    var docnumber = docnumwithformat.Substring(docnumwithformat.Length - result.DocFormat.Length) + result.Prefix;
                    result.FormattedDocNo = docnumber;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetContractList fails. Error " + ex.Message);
                throw ex;
            }
            

        }

        public bool ResetDocumentNo(DocumentNo documentInfo)
        {
            Validate(documentInfo);
            dataOperation.CommandText("trn_DocumentNoReset");
            dataOperation.AddParam("@doctype", documentInfo.DocType);
            dataOperation.AddParam("@DocNo", documentInfo.DocNo);
            dataOperation.AddParam("@Prefix", documentInfo.Prefix);
            dataOperation.AddParam("@DocFormat", documentInfo.DocFormat);
            dataOperation.AddParam("@User", documentInfo.User);
            dataOperation.DoExecute();
            return true;
        }

        public bool IncrementDocumentNo(int docType)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<DocumentNo> GetDocumentData()
        {
            try
            {
                var mapper = new DataMapper<DocumentNo>("GetDocuments", new Hashtable() { });

                var result = mapper.ObjectList();

                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetDocumentData fails. Error " + ex.Message);
                throw ex;
            }
            
        }

        private void Validate(DocumentNo documentInfo)
        {
            if (string.IsNullOrEmpty(documentInfo.DocNo.ToString()))
                throw new ArgumentNullException("Invalid Document No");
            if (string.IsNullOrEmpty(documentInfo.DocFormat.ToString()))
                throw new ArgumentNullException("Invalid Document Format");
            if (string.IsNullOrEmpty(documentInfo.Prefix.ToString()))
                throw new ArgumentNullException("Invalid Prefix");
        }


        public IEnumerable<DocumentNo> GetDocumentDataByModule(int moduleId)
        {
            var mapper = new DataMapper<DocumentNo>("GetDocuments", new Hashtable() { });
            var result = mapper.ObjectList();
            return result.Where(c => c.ModuleId == moduleId);
        }
    }
}
