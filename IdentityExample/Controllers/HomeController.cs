using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System.Threading.Tasks;

namespace IdentityExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // View methods.
        [HttpGet]
        public IActionResult Index() => View();

        [Authorize]
        [HttpGet]
        public IActionResult Secret() => View();

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult EmailVerification() => View();

        // Auth methods.
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Get user
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(
                    user: user,
                    password: password,
                    isPersistent: true, // I would like to set the cookie because I think it's cool
                    lockoutOnFailure: false); // Don't need to lockout

                if (signInResult.Succeeded)                
                    return RedirectToAction("Secret");                
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // Create an IdentityUser.
            var user = new IdentityUser()
            {
                UserName = username,
                Email = string.Empty
            };

            // Create the user with the user manager.
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Generate the email token for email verification.
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var link = Url.Action(
                    action: nameof(VerifyEmail),
                    controller: "Home",
                    values: new { userId = user.Id, code },
                    protocol: Request.Scheme,
                    host: Request.Host.ToString());

                await _emailService.SendAsync(
                    mailTo: "test@test.com",
                    subject: "Email Verification",
                    message: $"<a href=\"{link}\">Click here</a>",
                    isHtml: true);

                return RedirectToAction("EmailVerification");
            }

            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
                return View();

            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}
