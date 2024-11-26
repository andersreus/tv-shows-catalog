using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Twilio.Rest.Verify.V2.Service;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;

namespace TvShowsCatalog.Web.UI.Twilio
{
    public sealed class AuthenticationSurfaceController : SurfaceController
    {
        IMemberService _memberService;
        IMemberManager _memberManager;
        IMemberSignInManager _memberSignInManager;
        IHttpContextAccessor _httpContextAccessor;

        private readonly string? _serviceSid;
        
        private readonly ILogger<AuthenticationSurfaceController> _logger;
        private readonly IHttpClientFactory _factory;

        public AuthenticationSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberService memberService,
            IMemberManager memberManager,
            IMemberSignInManager memberSignInManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<AuthenticationSurfaceController> logger,
            IHttpClientFactory factory) : base(umbracoContextAccessor, databaseFactory, services,
            appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberService = memberService;
            _memberManager = memberManager;
            _memberSignInManager = memberSignInManager;
            _httpContextAccessor = httpContextAccessor;
            _serviceSid = configuration["ServiceSid"];
            _logger = logger;
            _factory = factory;
        }

        public const string UserNameKey = "usernameKey";
        public const string CountryCodeKey = "countryCodeKey";


        [HttpPost]
        public async Task<IActionResult> Login(long member_number, string otp,
            string member_country_code, string redirect_url = "/")
        {
            if (!string.IsNullOrWhiteSpace(member_country_code) &&  
                member_number.ToString().Length > 3)
            {
                string username = $"+{member_country_code}{member_number}";
                if (string.IsNullOrWhiteSpace(otp))
                {
                    // send otp
                    VerificationResource.Create(
                        to: username,
                        channel: "sms",
                        locale: "en",
                        pathServiceSid: _serviceSid
                    );
                    _httpContextAccessor.HttpContext.Session.SetString(UserNameKey, member_number.ToString());

                    _httpContextAccessor.HttpContext.Session.SetString(CountryCodeKey, member_country_code);
                }
                else
                {
                    // validate otp
                    var verification = VerificationCheckResource.Create(
                        to: username,
                        code: otp,
                        pathServiceSid: _serviceSid
                    );


                    bool verified = verification?.Valid ?? false;
                    if (verified)
                    {
                        var member = _memberService.GetByUsername(username);
                        var fakeEmail = $"{member_country_code}_{member_number}@{member_country_code}.com";

                        if (member == null)
                        {
                            var memberIdentityUser = MemberIdentityUser.CreateNew(
                                username: username,
                                email: fakeEmail,
                                memberTypeAlias: "Member",
                                isApproved: true,
                                name: fakeEmail);

                            if (_memberManager.CreateAsync(
                                    user: memberIdentityUser,
                                    password: username).Result.Succeeded)
                            {
                                _memberManager.AddToRolesAsync(
                                    user: memberIdentityUser,
                                    roles: new List<string>() { "Everyone", $"{member_country_code}" }).Wait();
                            }

                            member = _memberService.GetByUsername(username);
                        }

                        if (_memberManager.ValidateCredentialsAsync(username: member.Username, password: member.Username)
                            .Result)
                        {
                            // Validate member credentials
                            var memberIdentityUser = _memberManager.FindByNameAsync(member.Username).Result;
                            _memberSignInManager.SignInAsync(user: memberIdentityUser, isPersistent: true).Wait();
                        }
                    }
                }
            }

            return Redirect(redirect_url);
        }

 

        [HttpPost]
        public IActionResult Logout()
        {
            _memberSignInManager.SignOutAsync();
            _httpContextAccessor.HttpContext.Session.Clear();
            return RedirectToCurrentUmbracoPage();
        }
    }
}
