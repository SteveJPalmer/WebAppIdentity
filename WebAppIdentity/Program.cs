using Microsoft.AspNetCore.Authorization;
using WebAppIdentity.Authorization;

var builder = WebApplication.CreateBuilder(args);

/************************************************
 ***  Add the Authentication Handler & Config ***
 ************************************************/
// AddCookie() injects an Authentication Handler (concrete implementation of IAuthenticationService)
// pass in the Scheme name - cf Login.cshtml.cs > AuthorizationTYpe / Scheme name
// (defaultScheme is the Handler to be used for Authentication - as could be multiple AuthN handlers)
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{   // configuring our cookie
    options.Cookie.Name = "MyCookieAuth";   //Cookie name is more important part (this Cookie will contain out Security Context)
                                            //keep all names same to group everything together - typically create a constant
    
    //options.LoginPath = "/MyAccount/login";                // can specify location of login page if not in default "/Account/Login"
    //options.AccessDeniedPath = "/MyAccount/AccessDenied";  // can specify location of Access Denied page if not in default "/Account/AccessDenied"

    //options.ExpireTimeSpan = TimeSpan.FromSeconds(30);     // adjust the Cookie expiry date (or within SignInAsync())
});


/***********************************************
 ***  Add the Authorization Handler & Config ***
 ***********************************************/
builder.Services.AddAuthorization(options =>
{   
    // add all our policies -
    // AddPolicy() args: 1) the policy name,
    //                   2) how policy works (the AuthorizationPolicyBuilder Delegate/fn)
    options.AddPolicy("AdminOnly",                                                   // Admin claim must exist
        policy => policy.RequireClaim("Admin") );
    
    options.AddPolicy("MustBelongToSalesDepartment",                                 // Department claim must equal 'SALES'
        policy => policy.RequireClaim("Department", "SALES") );   
    
    options.AddPolicy("HRManagerOnly", policy => 
            policy.RequireClaim("Department", "HR")
                  .RequireClaim("Manager") );                                             // Chains 2 Requirements (AND relationship - both must pass)
            
    options.AddPolicy("HRManagerPastProbation", policy => 
        policy.RequireClaim("Department", "HR")
              .RequireClaim("Manager")
              .Requirements.Add(new HRManagerProbationRequirement(3)) ); 
});

//DI our custom AuthZ requirement Handler
builder.Services.AddSingleton<IAuthorizationHandler, HRManagerProbationRequirementHandler>();


// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


/**********************************************************************
 *** works with Authentication Handler added in AddAuthentication() ***
 **********************************************************************/
app.UseAuthentication();

/*********************************************************************
 *** works with Authorization Policies added in AddAuthorization() ***
 *********************************************************************/
app.UseAuthorization();

app.MapRazorPages();

app.Run();
