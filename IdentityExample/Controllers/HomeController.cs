using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                    return RedirectToAction("Index");                
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
                var signInResult = await _signInManager.PasswordSignInAsync(
                    user: user,
                    password: password,
                    isPersistent: true,
                    lockoutOnFailure: false);

                if (signInResult.Succeeded)
                    return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}
