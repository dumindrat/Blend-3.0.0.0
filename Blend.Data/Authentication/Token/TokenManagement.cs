using Blend.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend.Authentication.Data.Token
{
    public class TokenManagement : BaseManagement<Token>, ITokenSvc
    {
        public Token GenerateToken(int userId)
        {
            throw new NotImplementedException();
        }

        public bool ValidateToken(string tokenId)
        {
            throw new NotImplementedException();
        }

        public bool Kill(string tokenId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteByUserId(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
