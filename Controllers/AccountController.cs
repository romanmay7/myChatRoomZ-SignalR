
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using myChatRoomZ.Data.Models;
using myChatRoomZ.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace myChatRoomZ.Controllers
{
    public class AccountController : Controller
    {
        //private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<ChatUser> _signInManager;
        private readonly UserManager<ChatUser> _userManager;
        private readonly IConfiguration _config;
        public AccountController(
            //ILogger<AccountController> logger,
            SignInManager<ChatUser> signInManager,
            UserManager<ChatUser> userManager,
            IConfiguration config
            )
        {
            //_logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        //public IActionResult Login()
        //{

        //    if (this.User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Index", "App");
        //    }
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username,model.Password,false,false);
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        RedirectToAction(Request.Query["ReturnUrl"].First());
                    }
                    else
                    {
                        RedirectToAction("Shop", "App");
                    }

                }
            }
            ModelState.AddModelError("", "Failed to Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody]UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ChatUser user = new ChatUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.UserName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create new user ");
                }


                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {

                        RedirectToAction(Request.Query["ReturnUrl"].First());
                    }
                    return Ok();

                }
                else
                {
                    return BadRequest();
                    //RedirectToAction("Shop", "App");
                }
            }
            //ModelState.AddModelError("", "Failed to Create New User");
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Home", "App");
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody]LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                    if (result.Succeeded)
                    {
                        //Create the token
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti,new Guid().ToString())
                           // new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName)
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            _config["Tokens:Issuer"],
                            _config["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddMinutes(30),
                            signingCredentials: creds
                            );

                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo

                        };

                        return Created("", results);
                    }
                }
            }
            return BadRequest();
        }
    }
}
