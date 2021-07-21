using BuildingSecureApi.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BuildingSecureApi.API.Services
{
    public class UserServices : IUserServicecs
    {

        private UserManager<IdentityUser> _userManger;
        private IConfiguration _config;
        private SignInManager<IdentityUser> _singInUser;

        public UserServices(UserManager<IdentityUser> userManger, SignInManager<IdentityUser> singInUser, IConfiguration config)
        {
            _userManger = userManger;
            _singInUser = singInUser;
            _config = config;
        }



        public async Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model)
        {
            if (model == null)
                throw new NullReferenceException("Register Model is null");
            if (model.Password != model.ConfimPassword)
                return new UserManagerResponse() { Message = "Confirm password doesn't match password", IsSuccess = false };

            var identityUser = new IdentityUser()
            {
                Email = model.Email,
                UserName = model.Email

            };

            var result = await _userManger.CreateAsync(identityUser, model.Password);
            if (result.Succeeded)
            {
                return new UserManagerResponse
                {
                    Message = "User  created successfully!",
                    IsSuccess = true,
                    UserName = identityUser.UserName

                };
            }
            else
            {
                return new UserManagerResponse
                {
                    Message = "User did not create",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }


        }

        public async Task<UserManagerResponse> GetUserAsync(string userName)
        {
            if (userName == null)
                throw new NullReferenceException("username is null");

            var user = await _userManger.FindByNameAsync(userName);
            if (user != null)
                return new UserManagerResponse() { Message = "user was found ", UserName = userName };
            return new UserManagerResponse() { Message = "user doesn't found ", UserName = userName };




        }

        public async Task<UserManagerResponse> LoginUserAsync(LoginViewModel loginViewModel)
        {
            return await GetJwtByCrossrd(loginViewModel);


        }

        public async Task<UserManagerResponse> GetJwt(LoginViewModel loginViewModel)
        {
            #region Validate the email and password
            var user = await _userManger.FindByEmailAsync(loginViewModel.Email);
            if (user == null)
                return new UserManagerResponse() { Message = $"user with email {loginViewModel.Email} doesn't exist ", IsSuccess = false };
            var checkPwd = await _userManger.CheckPasswordAsync(user, loginViewModel.Password);
            if (!checkPwd)
                return new UserManagerResponse() { Message = $"invalid pasword for {loginViewModel.Email} ", IsSuccess = false };

            #endregion
            #region Once we validate the email and password we generate Jwt token
            var claims = new[]
            {
                new Claim(ClaimTypes.Email,loginViewModel.Email),
             new Claim(ClaimTypes.NameIdentifier,user.Id )

            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["AuthSettings:key"]));
            var token = new JwtSecurityToken(issuer: _config["AuthSettings:Issuer"], audience: _config["AuthSettings:Audience"], claims: claims, expires: DateTime.UtcNow.AddSeconds(120), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512));

            var tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);
            return new UserManagerResponse() { Message = tokenAsString, IsSuccess = true, ExpiredDate = token.ValidTo.AddHours(1) };
            #endregion

        }

        public async Task<UserManagerResponse> GetJwtByCrossrd(LoginViewModel loginViewModel)
        {
            #region Validate the email and password
            var user = await _userManger.FindByEmailAsync(loginViewModel.Email);
            if (user == null)
                return new UserManagerResponse() { Message = $"user with email {loginViewModel.Email} doesn't exist ", IsSuccess = false };
            var checkPwd = await _userManger.CheckPasswordAsync(user, loginViewModel.Password);
            if (!checkPwd)
                return new UserManagerResponse() { Message = $"invalid pasword for {loginViewModel.Email} ", IsSuccess = false };

            #endregion
            #region Once we validate the email and password we generate Jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey=Encoding.ASCII.GetBytes(_config["AuthSettings:key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email,loginViewModel.Email)
                }),
                Expires = DateTime.UtcNow.AddSeconds(120),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new UserManagerResponse() { Message = tokenHandler.WriteToken(token), IsSuccess = true, ExpiredDate = token.ValidTo.AddHours(1) };
            #endregion
         
        }

    }
}
