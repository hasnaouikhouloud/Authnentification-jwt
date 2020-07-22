


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modele;
using ORM;
using ORM.ModelEmail;
using ORM.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace presentation.Controllers
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IJWTAuthenticationManager _userService;
        private readonly ILogger _logger;



        public UserController(UserManager<ApplicationUser> userManager, IJWTAuthenticationManager userService, ApplicationDbContext context
            , SignInManager<ApplicationUser> signInManager, ILogger<UserController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }



        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);
            try
            {


                var users = await _userManager.FindByNameAsync(model.Username);
                if (users == null)
                {
                    return NotFound();
                }

                if (response == null)
                {
                    users.AccessFailedCount = users.AccessFailedCount + 1;
                    await _userManager.UpdateAsync(users);

                    _logger.LogWarning("Username or password is incorrect");
                    return BadRequest(new { message = "Username or password is incorrect" });

                }
                var accessCountMaxx = "3";
                int accessCountMax = Int32.Parse(accessCountMaxx);

                if (users.AccessFailedCount > accessCountMax)
                {
                    _ = users.LockoutEnabled = true;
                    await _userManager.UpdateAsync(users);

                    _logger.LogWarning("User account locked out.");
                    return BadRequest(new { message = "User account locked out." });
                }

            }


            catch (IOException e)
            {
                Console.WriteLine($"Username or password is incorrect: '{e}'");
            }
            _logger.LogInformation("User logged in.");
            return Ok(response);
        }


        // helper methods





        [HttpPost("ForgetPassword")]
        [AllowAnonymous]

        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return BadRequest(new { message = "Email is incorrect" });
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
         
            
            //generatePassword
            var options = _userManager.Options.Password;

            int length = options.RequiredLength;

            bool nonAlphanumeric = options.RequireNonAlphanumeric;
            bool digit = options.RequireDigit;
            bool lowercase = options.RequireLowercase;
            bool uppercase = options.RequireUppercase;

            StringBuilder password = new StringBuilder();
            Random random = new Random();

            while (password.Length < length)
            {
                char c = (char)random.Next(32, 126);

                password.Append(c);

                if (char.IsDigit(c))
                    digit = false;
                else if (char.IsLower(c))
                    lowercase = false;
                else if (char.IsUpper(c))
                    uppercase = false;
                else if (!char.IsLetterOrDigit(c))
                    nonAlphanumeric = false;
            }

            if (nonAlphanumeric)
                password.Append((char)random.Next(33, 48));
            if (digit)
                password.Append((char)random.Next(48, 58));
            if (lowercase)
                password.Append((char)random.Next(97, 123));
            if (uppercase)
                password.Append((char)random.Next(65, 91));

            var code = password.ToString();
            var callback = Url.Action(nameof(AuthenticateAsync), "Account", new { code, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Récupération mot de passe",
                   $"Bonjour <B> { user.FirstName }  { user.LastName } </B>,<br>" +
                   $"Bienvenue sur la plateforme Gestion Utilisateur , votre mot de passe a été changé.<br>" +
                   $"Votre nouveau mot de passe est :<B>{ code }</B> <br>" +
                   " <a href='{ callback }'>Cliquez ici pour connecter</a>");
             _userService.SendEmail(message);

            var newpassword = _userManager.PasswordHasher.HashPassword(user,password.ToString());
            user.PasswordHash = newpassword;
            await _userManager.UpdateAsync(user);
            // If we got this far, something failed, redisplay form
            return Ok("message envoyéé");
        }



    }



}

