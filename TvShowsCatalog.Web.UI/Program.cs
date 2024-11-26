using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using TvShowsCatalog.Web.Data;
using TvShowsCatalog.Web.UI;
using Twilio;
using Umbraco.Cms.Infrastructure.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// if (builder.Environment.IsDevelopment())
// {
//     builder.Configuration.AddUserSecrets<Program>();
// }

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .SetCustomMemberLoginPath()
    .Build();

builder.Services.AddDbContext<ReviewContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("umbracoDbDSN")));

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

// var config = new ConfigurationBuilder()
//     .AddUserSecrets<Program>()
//     .Build();
//
// var configPath = "SmsDataService:Twilio:";
//
// var accountSid = config[$"{configPath}AccountSid"];
// var authToken = config[$"{configPath}AuthToken"];
//
// TwilioClient.Init(accountSid, authToken);

app.UseHttpsRedirection();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();