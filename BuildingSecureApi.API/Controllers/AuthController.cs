using BuildingSecureApi.API.Extensions;
using BuildingSecureApi.API.Services;
using BuildingSecureApi.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingSecureApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IUserServicecs _userServicecs;

        public AuthController(IUserServicecs userServicecs)
        {
            _userServicecs = userServicecs;
        }


        [HttpGet("{userName}", Name = "UserName")]
        public async Task<IActionResult> GetUser(string userName)
        {
            try
            {
                var user = await _userServicecs.GetUserAsync(userName);
                if (user == null)
                    return NotFound(user);

                return Ok(user);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody]RegisterViewModel registerViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState.ValidateResponse());
                var user = await _userServicecs.RegisterUserAsync(registerViewModel);
                if (!user.IsSuccess)
                {
                    return BadRequest(user);
                }
                return CreatedAtRoute("UserName", new { user.UserName }, user);

            }
            catch (Exception ex)
            {

                //log error
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel loginViewModel)
        {
            
             try
             {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState.ValidateResponse());
                var user = await _userServicecs.LoginUserAsync(loginViewModel);
                if (!user.IsSuccess)
                    return Unauthorized(user);
                return Ok(user);

             }
             catch (Exception ex)
                {

                    //log error
                    return StatusCode(500, ex.Message);
                }
        }
    }
}
