using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using TvShowsCatalog.Web.Data;
using TvShowsCatalog.Web.UI;
using Twilio;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .SetCustomMemberLoginPath()
.Build();

builder.Services.AddDbContext<ReviewContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("umbracoDbDSN")));

// Fra docs men det defaulter til sqlite når jeg køre migrations.
//builder.Services.AddUmbracoDbContext<ReviewContext>((serviceProvider, options) =>
//{
//    options.UseUmbracoDatabaseProvider(serviceProvider);
//});

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

var config = app.Services.GetRequiredService<IConfiguration>();
var configPath = "SmsDataService:Twilio:";

var accountSid = config[$"{configPath}AccountSid"];
var authToken = config[$"{configPath}AuthToken"];

TwilioClient.Init(accountSid, authToken);

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
