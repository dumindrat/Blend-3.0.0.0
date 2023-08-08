using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessEntities.Email
{
    public class EmailDto : BaseEntity
    {
        public int ReferenceId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public List<string> CC { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
        public bool IsAttachment { get; set; }
        public Stream FileStream { get; set; }
    }

    public class Transactions
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class TransactionOwner : BaseEntity
    {
        public int TransId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public class EmailHistory : BaseEntity
    {
        public int ReferenceId { get; set; }
        public string ReferenceNo { get; set; }
        public string ToAddress { get; set; }
        public string FromAddress { get; set; }
        public string EmailBody { get; set; }
        public string Subject { get; set; }
        public string CC { get; set; }
        public string AttachmentPath { get; set; }
        public int MenuId { get; set; }
    }
}
