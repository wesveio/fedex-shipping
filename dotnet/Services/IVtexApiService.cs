using FedexShipping.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace FedexShipping.Services
{
    public interface IVtexApiService
    {
        Task<HttpStatusCode> IsValidAuthUser();
        Task<ValidatedUser> ValidateUserToken(string token);
    }
}