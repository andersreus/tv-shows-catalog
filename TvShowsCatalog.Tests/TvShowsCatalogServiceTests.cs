using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace TvShowsCatalog.Tests
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class TvShowsCatalogServiceTests : UmbracoIntegrationTest
    {
        private ITvMazeService _tvMazeService;

        [SetUp]
        public void Setup()
        {
            _tvMazeService = new TvMazeService();
        }

        [Test]
        public async Task GetAllAsync_ReturnsListOfTvMazeModelObjects()
        {
            var allShows = await _tvMazeService.GetAllAsync();
            Assert.IsTrue(allShows.Any());
        }
    }
}