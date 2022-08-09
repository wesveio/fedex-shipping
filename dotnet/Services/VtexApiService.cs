using FedexShipping.Data;
using FedexShipping.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;
using Newtonsoft.Json;

namespace FedexShipping.Services
{
    public class VtexApiService : IVtexApiService
    {
        private readonly IIOServiceContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;

        public VtexApiService(IIOServiceContext context, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            this._context = context ??
                            throw new ArgumentNullException(nameof(context));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<ValidatedUser> ValidateUserToken(string token)
        {
            ValidatedUser validatedUser = null;
            ValidateToken validateToken = new ValidateToken
            {
                Token = token
            };

            var jsonSerializedToken = JsonConvert.SerializeObject(validateToken);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_ACCOUNT]}.vtexcommercestable.com.br/api/vtexid/credential/validate"),
                Content = new StringContent(jsonSerializedToken, Encoding.UTF8, Constants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];

            if (authToken != null)
            {
                request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();

            try
            {
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    validatedUser = JsonConvert.DeserializeObject<ValidatedUser>(responseContent);
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("ValidateUserToken", null, $"Error validating user token", ex);
            }

            return validatedUser;
        }

        public async Task<HttpStatusCode> IsValidAuthUser()
        {
            if (string.IsNullOrEmpty(_context.Vtex.AdminUserAuthToken))
            {
                return HttpStatusCode.Unauthorized;
            }

            ValidatedUser validatedUser = null;

            try {
                validatedUser = await this.ValidateUserToken(_context.Vtex.AdminUserAuthToken);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("IsValidAuthUser", null, "Error fetching user", ex);

                return HttpStatusCode.BadRequest;
            }

            bool hasPermission = validatedUser != null && validatedUser.AuthStatus.Equals("Success");

            if (!hasPermission)
            {
                _context.Vtex.Logger.Warn("IsValidAuthUser", null, "User Does Not Have Permission");

                return HttpStatusCode.Forbidden;
            }

            return HttpStatusCode.OK;
        }
    }
}