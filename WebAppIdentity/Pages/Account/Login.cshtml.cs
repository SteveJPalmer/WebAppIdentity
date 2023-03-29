using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppIdentity.Pages.Account;

public class LoginModel : PageModel
{
    [BindProperty]
    public Credential Credential { get; set; }
    
    public void OnGet()
    {
        
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        //check if form valid
        if (!ModelState.IsValid) return Page();  //returns current view - login.cshtml
        
        /****************************************************************************
         *** section to create the Cookie, containing our new ClaimsPrincipal obj ***
         ****************************************************************************/
        //verify the credentials (hardcoded check for demo, typically need check DB user store
        if (Credential.UserName == "Steve" && Credential.Password == "pwd")
        {
            /* create the security context */
              //create the required claims
              var claims = new List<Claim>
              {
                  /* for testing Policies can simply change Claims locally */
                  new Claim(ClaimTypes.Name, "Steve"),
                  new Claim(ClaimTypes.Email, "admin@mywebsite.com"),
                  new Claim("EmploymentDate", "01/09/2022"),   // failure test "01/03/2023"
                  /* Department claims */
                  new Claim("Department", "HR"),
                  // new Claim("Department", "SALES"),
                  /* Position claims */
                  new Claim("Manager", "true"),
                  // new Claim("Admin", "true"),
              };
              //then.. add our claims list to an Identity (with an Authentication Type - 2nd arg - can call whatever want)
              var identity = new ClaimsIdentity(claims, "MyCookieAuth");
              
              //then.. add the identity to a ClaimsPrincipal (our security context)
              ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            /**************************************
             * encrypt, serialize & add to cookie *
             **************************************/  
            // is extension helper method we can use.. SignInAsync(AuthentictionType, ClaimsPrincipal, authProperties)  cf IAuthenticationService
            // will encrypt, serialize & add cookie into HttpContext for you :)
            // (optionally set & pass AuthenticationProperties to control Cookie persistence)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,         // 'true' enables user login to persist across browser instances
                                             // (sets Cookie expiry date - rather than the default 'session' Cookie)
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),       // adjust the Cookie expiry date
            };
            // KEY METHOD
            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
            //await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal, authProperties);    // version with properties

            //if all good, redirect to app splash page
            return RedirectToPage("/Index");   // returns index.cshtml view
        }

        return Page(); //if credentials fail, also current view - login.cshtml
    }
}

public class Credential
{
    [Required]
    [Display(Name = "User Name")]
    public string UserName { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}