using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Blend.BuisnessEntities.Purchase;

namespace Blend.WebApp.Controllers.Tea.Purchases
{
    public class UploadFileController : BaseApi
    {
        // GET api/uploadfile
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/uploadfile/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/uploadfile
        public async Task<UploadData> Post()
        {
            var listData = new List<SraightLineUploadDto>();
            var processData = new UploadData { ItemList = listData };
            string sPath = "";
            sPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["UploadPath"]);
            try
            {

                System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;
                // THE DataTable TO HOLD EXCEL DATA.
                DataTable dt = new DataTable();
                // CHECK THE FILE COUNT.
                for (int iCnt = 0; iCnt <= hfc.Count - 1; iCnt++)
                {
                    System.Web.HttpPostedFile hpf = hfc[iCnt];

                    if (hpf.ContentLength > 0)
                    {
                        // CHECK THE FILE TYPE (YOU CAN CHECK WITH .xls ALSO).
                        if (hpf.FileName.EndsWith("csv"))
                        {
                            // SAVE THE FILES IN THE FOLDER.
                            var filePath = sPath + Path.GetFileName("sl_upload.csv");
                            hpf.SaveAs(filePath);
                            using (var reader = new StreamReader(filePath))
                            {
                                int count = 0;
                                int seqNo = 0;
                                var uploadLine = new SraightLineUploadDto();
                                while (!reader.EndOfStream)
                                {
                                    var line = reader.ReadLine();
                                    if (count > 0)
                                    {
                                        var values = line.Split(',');
                                        if (values.Count() == 14)
                                            listData.Add(new SraightLineUploadDto
                                            {
                                                Sequence = seqNo,
                                                Lot = values[1],
                                                Mark = values[3],
                                                Inv = values[4],
                                                Grade = values[5],
                                                Bags = String.Format("{0:0.00}", Convert.ToDecimal(values[6])),
                                                Net = String.Format("{0:0.00}", Convert.ToDecimal(values[7])),
                                                Qty = String.Format("{0:0.00}", Convert.ToDecimal(values[9])),
                                                Price = String.Format("{0:0.00}", Convert.ToDecimal(values[12])),
                                                RedQty = String.Format("{0:0.00}", GetRedQty(values[9], values[6], values[7])),
                                                Value = String.Format("{0:0.00}", GetPrice(values[9], values[12])),
                                                Status = SetStatus(values[9], values[6], values[7], values[1], values[3], values[4], values[5])
                                            });
                                    }
                                    count++;
                                    seqNo++;
                                }
                                processData.ItemList = listData;
                                processData.IsValid = HasInvalidData(listData);
                            }
                        }
                        else
                        {
                            processData.IsValid = false;
                            return processData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWrite(ex.Message, "Error");
                processData.ItemList = listData;
                processData.IsValid = false;
                return processData;
            }
            return processData;
        }

        private bool HasInvalidData(List<SraightLineUploadDto> listData)
        {
            var invalidData = listData.Where(c => c.Status.IsValid == false).ToList();
            if (invalidData.Count > 0) return false;
            return true;
        }

        private LineStatus SetStatus(string qty, string bags, string net, string lot, string mark, string inv, string grade)
        {
            var lineStatus = new LineStatus();
            if (GetRedQty(qty, bags, net) < 0) return new LineStatus { IsValid = false, Message = "Invalid Weight" };
            if (String.IsNullOrEmpty(lot)) return new LineStatus { IsValid = false, Message = "Invalid LotNo" };
            if (String.IsNullOrEmpty(mark)) return new LineStatus { IsValid = false, Message = "Invalid Garden Mark" }; ;
            if (String.IsNullOrEmpty(inv)) return new LineStatus { IsValid = false, Message = "Invalid Invoice" }; ;
            if (String.IsNullOrEmpty(grade)) return new LineStatus { IsValid = false, Message = "Invalid Grade" }; ;
            return new LineStatus { IsValid = true, Message = "Valid" }; ;
        }



        private Decimal GetRedQty(string qty, string packing, string nett)
        {
            var _qty = Convert.ToDecimal(qty);
            var _packing = Convert.ToDecimal(packing);
            var _nett = Convert.ToDecimal(nett);
            var redQty = (_packing * _nett) - _qty;
            return redQty;
        }

        private Decimal GetPrice(string qty, string price)
        {
            var _qty = Convert.ToDecimal(qty);
            var _price = Convert.ToDecimal(price);
            var value = (_qty * _price);
            return value;
        }

        // PUT api/uploadfile/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/uploadfile/5
        public void Delete(int id)
        {
        }
    }

    //public class InMemoryMultipartFormDataStreamProvider : MultipartStreamProvider
    //{
    //    private NameValueCollection _formData = new NameValueCollection();
    //    private List<HttpContent> _fileContents = new List<HttpContent>();

    //    // Set of indexes of which HttpContents we designate as form data  
    //    private Collection<bool> _isFormData = new Collection<bool>();

    //    /// <summary>  
    //    /// Gets a <see cref="NameValueCollection"/> of form data passed as part of the multipart form data.  
    //    /// </summary>  
    //    public NameValueCollection FormData
    //    {
    //        get { return _formData; }
    //    }

    //    /// <summary>  
    //    /// Gets list of <see cref="HttpContent"/>s which contain uploaded files as in-memory representation.  
    //    /// </summary>  
    //    public List<HttpContent> Files
    //    {
    //        get { return _fileContents; }
    //    }

    //    //public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
    //    //{
    //    //    // For form data, Content-Disposition header is a requirement  
    //    //    ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
    //    //    if (contentDisposition != null)
    //    //    {
    //    //        // We will post process this as form data  
    //    //        _isFormData.Add(String.IsNullOrEmpty(contentDisposition.FileName));

    //    //        return new MemoryStream();
    //    //    }

    //    //    // If no Content-Disposition header was present.  
    //    //    throw new InvalidOperationException(string.Format("Did not find required '{0}' header field in MIME multipart body part..", "Content-Disposition"));
    //    //}

    //    ///// <summary>  
    //    ///// Read the non-file contents as form data.  
    //    ///// </summary>  
    //    ///// <returns></returns>  
    //    //public override async Task ExecutePostProcessingAsync()
    //    //{
    //    //    // Find instances of non-file HttpContents and read them asynchronously  
    //    //    // to get the string content and then add that as form data  
    //    //    for (int index = 0; index < Contents.Count; index++)
    //    //    {
    //    //        if (_isFormData[index])
    //    //        {
    //    //            HttpContent formContent = Contents[index];
    //    //            // Extract name from Content-Disposition header. We know from earlier that the header is present.  
    //    //            ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
    //    //            string formFieldName = UnquoteToken(contentDisposition.Name) ?? String.Empty;

    //    //            // Read the contents as string data and add to form data  
    //    //            string formFieldValue = await formContent.ReadAsStringAsync();
    //    //            FormData.Add(formFieldName, formFieldValue);
    //    //        }
    //    //        else
    //    //        {
    //    //            _fileContents.Add(Contents[index]);
    //    //        }
    //    //    }
    //    //}

    //    ///// <summary>  
    //    ///// Remove bounding quotes on a token if present  
    //    ///// </summary>  
    //    ///// <param name="token">Token to unquote.</param>  
    //    ///// <returns>Unquoted token.</returns>  
    //    //private static string UnquoteToken(string token)
    //    //{
    //    //    if (String.IsNullOrWhiteSpace(token))
    //    //    {
    //    //        return token;
    //    //    }

    //    //    if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
    //    //    {
    //    //        return token.Substring(1, token.Length - 2);
    //    //    }

    //    //    return token;
    //    //}





    //}
}
