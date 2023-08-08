using Blend.BuisnessEntities.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.BuisnessService.Helper.Email
{
    public interface IEmailService
    {
        bool SendMail(EmailDto obj);
        IEnumerable<EmailHistory> GetEmailList(int refrenceId, int menuId);
        bool SaveEmailHistory(EmailHistory obj);
    }
}
