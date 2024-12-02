using Microsoft.Extensions.DependencyInjection;
using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace TvShowsCatalog.Tests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class TranslationsTests : UmbracoIntegrationTest
{
    private ITranslationService _translationService;

    [SetUp]
    public new void Setup()
    {
        _translationService = GetRequiredService<ITranslationService>();
    }
    
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<ITranslationService, TranslationService>();
    }
    
    [Test]
    public async Task Translate_English_Genre_To_Other_Language()
    {
        var translatedGenre = _translationService.GetTranslation("da-DK", "Horror");
        Assert.AreEqual("Gyser",translatedGenre);
    }

}