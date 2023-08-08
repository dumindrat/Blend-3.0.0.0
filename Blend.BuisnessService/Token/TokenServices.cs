using System;
using System.Configuration;
using System.Linq;
using Blend.BuisnessService;
using Blend.BuisnessEntities;

namespace BusinessServices
{
    public class TokenServices : BaseManagement<TokenEntity>, ITokenServices
    {


        #region Public constructor.
        /// <summary>
        /// Public constructor.
        /// </summary>
        public TokenServices()
        {

        }
        #endregion


        #region Public member methods.

        /// <summary>
        ///  Function to generate unique token with expiry against the provided userId.
        ///  Also add a record in database for generated token.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TokenEntity GenerateToken(int userId)
        {
            string token = Guid.NewGuid().ToString();
            DateTime issuedOn = DateTime.Now;
            DateTime expiredOn = DateTime.Now.AddSeconds(
                                              Convert.ToDouble(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
            var tokendomain = new TokenEntity
                                  {
                                      UserId = userId,
                                      AuthToken = token,
                                      IssuedOn = issuedOn,
                                      ExpiresOn = expiredOn
                                  };

            dataOperation.CommandText("auth_TokenInsert");
            dataOperation.AddParam("User", userId);
            dataOperation.AddParam("Token", token);
            dataOperation.AddParam("IssueDate", issuedOn);
            dataOperation.AddParam("ExpireDate", expiredOn);
            dataOperation.DoExecute();

            var tokenModel = new TokenEntity()
                                 {
                                     UserId = userId,
                                     IssuedOn = issuedOn,
                                     ExpiresOn = expiredOn,
                                     AuthToken = token
                                 };

            return tokenModel;
        }

        /// <summary>
        /// Method to validate token against expiry and existence in database.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        public bool ValidateToken(string tokenId)
        {
            try
            {
                var mapper = new DataMapper<TokenEntity>("GetAuthToken", new TokenEntity { AuthToken = tokenId });

                var result = mapper.Object();
                if (result.AuthToken != null && !(DateTime.Now > result.ExpiresOn))
                {
                    result.ExpiresOn = result.ExpiresOn.AddSeconds(
                                                  Convert.ToDouble(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
                    dataOperation.CommandText("auth_TokenUpdateExpiresOn");
                    dataOperation.AddParam("Token", result.AuthToken);
                    dataOperation.AddParam("ExpireDate", result.ExpiresOn);
                    dataOperation.DoExecute();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Method to kill the provided token id.
        /// </summary>
        /// <param name="tokenId">true for successful delete</param>
        public bool Kill(string tokenId, string user)
        {
            dataOperation.CommandText("auth_TokenDelete");
            dataOperation.AddParam("User", user);
            dataOperation.AddParam("Token", tokenId);
            dataOperation.DoExecute();
            return true;
        }

        /// <summary>
        /// Delete tokens for the specific deleted user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>true for successful delete</returns>
        public bool DeleteByUserId(int userId)
        {
            //_unitOfWork.TokenRepository.Delete(x => x.UserId == userId);
            //_unitOfWork.Save();

            //var isNotDeleted = _unitOfWork.TokenRepository.GetMany(x => x.UserId == userId).Any();
            //return !isNotDeleted;;
            return false;
        }

        #endregion
    }
}
