using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Core;
using Microsoft.Extensions.DependencyInjection;

namespace TvShowsCatalog.Tests
{
	[TestFixture]
	[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
	public class TvShowsCatalogServiceTests : UmbracoIntegrationTest
	{
		private ITvMazeService _tvMazeService;
		private IMediaService _mediaService;
		private IImportMediaService _importMediaService;

		[SetUp]
		public void Setup()
		{
			_mediaService = GetRequiredService<IMediaService>();
			_tvMazeService = GetRequiredService<ITvMazeService>();
			_importMediaService = GetRequiredService<IImportMediaService>();
		}

		protected override void CustomTestSetup(IUmbracoBuilder builder)
		{
			builder.Services.AddTransient<ITvMazeService, TvMazeService>();
			builder.Services.AddTransient<IImportMediaService, ImportMediaService>();
		}

		[Test]
		public async Task GetAllAsync_ReturnsListOfTvMazeModelObjects()
		{
			var allShows = await _tvMazeService.GetAllAsync();
			Assert.IsTrue(allShows.Any());
		}

		[Test]
		public async Task ImportMedia_CreateAndSafeASingleMediaInBackoffice()
		{
			var allShows = await _tvMazeService.GetAllAsync();

			var media =  await _importMediaService.ImportMediaAsync(allShows.First());

			var savedMedia = _mediaService.GetById(media.Id);

			Assert.IsNotNull(savedMedia);
		}

		[Test]
		public void ImportBulkMedia_CreateAndSafeAllMediaInBackoffice()
		{
			var allShows = _tvMazeService.GetAllAsync().GetAwaiter().GetResult();

			var mediaFolder = _importMediaService.CreateMediaRootFolder();

			_importMediaService.ImportBulkMedia(allShows);

			int count = _mediaService.CountChildren(mediaFolder.Id);

			Assert.AreEqual(240, count);
		}

		[Test]
		public void ImportContent_CreateAndSafeAllTvShowsAsNodesInBackoffice()
		{
			throw new NotImplementedException();
		}
	}
}