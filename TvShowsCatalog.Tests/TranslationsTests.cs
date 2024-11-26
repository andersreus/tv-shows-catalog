using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Tests.Integration.Testing;

namespace TvShowsCatalog.Tests;

public class TranslationsTests : UmbracoIntegrationTest
{
    private ITranslationService _translationService;

    [SetUp]
    public new void Setup()
    {
        _translationService = GetRequiredService<ITranslationService>();
    }
    
    [Test]
    public async Task Translate_English_Genre_To_Other_Language()
    {
        _translationService.GetTranslation("en-US", "Comedy");
        Assert.AreEqual("Komedie","Comedy");
    }

}