using BuildingSecureApi.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingSecureApi.API.Services
{
    public  interface IUserServicecs
    {
        Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model);
        Task<UserManagerResponse> GetUserAsync(string userName);
        Task<UserManagerResponse> LoginUserAsync(LoginViewModel loginViewModel);

    }
}
