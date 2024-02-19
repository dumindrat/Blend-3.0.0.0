using Blend.BuisnessEntities.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using Blend.DataModel.DataOperation;
using System.Collections;

namespace Blend.BuisnessService.Helper.Email
{
    public class EmailService : BaseManagement<EmailHistory>, IEmailService
    {
        public bool SendMail(EmailDto obj)
        {
            try
            {
                obj.From = ConfigurationManager.AppSettings["EmailAccount"];

                using (MailMessage mail = new MailMessage(obj.From, obj.To))
                {
                    mail.Subject = obj.Subject;
                    mail.Body = obj.Message + System.Environment.NewLine +  "[ DO NOT REPLY FOR THIS MESSAGE ]";
                    if (obj.IsAttachment)
                    {
                        string fileName = Path.GetFileName(obj.Attachment);
                        FileStream fs = new FileStream(obj.Attachment, FileMode.Open, FileAccess.Read);
                        MemoryStream ms = new MemoryStream();
                        fs.CopyTo(ms);
                        mail.Attachments.Add(new Attachment(obj.Attachment));
                    }
                    var EmailUser = ConfigurationManager.AppSettings["NetworkUser"];
                    var EmailPassword = ConfigurationManager.AppSettings["NetworkPassword"];
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = ConfigurationManager.AppSettings["SmptpHost"];
                    NetworkCredential networkCredential = new NetworkCredential(EmailUser, EmailPassword);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = networkCredential;
                    smtp.EnableSsl = false;
                    smtp.Port = Convert.ToInt16(ConfigurationManager.AppSettings["SmptpPort"]);
                    smtp.Send(mail);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }


        public IEnumerable<EmailHistory> GetEmailList(int refrenceId, int menuId)
        {
            try
            {
                var mapper = new DataMapper<EmailHistory>("GetEmailHistory", new Hashtable() { { "Referenceid", refrenceId }, { "MenuId", menuId } });
                var result = mapper.ObjectList();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogWrite("GetGradeList fails. Error " + ex.Message);
                throw ex;
            }
        }


        public bool SaveEmailHistory(EmailHistory obj)
        {
            SqlTransaction trans = null;
            var connection = MSSqlDataService.MsSQLConn;
            var cmd = new SqlCommand();
            try
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "sys_EmailHistoryInsert";
                cmd.Parameters.AddWithValue("@ReferenceId", obj.ReferenceId);
                cmd.Parameters.AddWithValue("@ReferenceNo", obj.ReferenceNo);
                cmd.Parameters.AddWithValue("@ToAddress", obj.ToAddress);
                cmd.Parameters.AddWithValue("@FromAddress", obj.FromAddress);
                cmd.Parameters.AddWithValue("@EmailBody", obj.EmailBody);
                cmd.Parameters.AddWithValue("@Subject", obj.Subject);
                cmd.Parameters.AddWithValue("@CC", obj.CC);
                cmd.Parameters.AddWithValue("@AttachmentPath", obj.AttachmentPath);
                cmd.Parameters.AddWithValue("@ModuleId", obj.MenuId);
                cmd.Parameters.AddWithValue("@User", obj.User);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
