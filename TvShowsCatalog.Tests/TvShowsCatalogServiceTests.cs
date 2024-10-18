using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using NPoco;
using TvShowsCatalog.Web.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Core;
using Bogus.DataSets;

namespace TvShowsCatalog.Tests
{
    [TestFixture]
	[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
	public class TvShowsCatalogServiceTests : UmbracoIntegrationTest
	{
		private ITvMazeService _tvMazeService;
		private IMediaService _mediaService;
		private IImportMediaService _importMediaService;
		private IImportContentService _importContentService;
		private IContentTypeService _contentTypeService;
		private IContentService _contentService;
		private IDataTypeService _dataTypeService;
		private IShortStringHelper _shortStringHelper;
		private ITemplateService _templateService;

		[SetUp]
		public void Setup()
		{
			_mediaService = GetRequiredService<IMediaService>();
			_tvMazeService = GetRequiredService<ITvMazeService>();
			_importMediaService = GetRequiredService<IImportMediaService>();
			_importContentService = GetRequiredService<IImportContentService>();
            _contentTypeService = GetRequiredService<IContentTypeService>();
            _contentService = GetRequiredService<IContentService>();
			_dataTypeService = GetRequiredService<IDataTypeService>();
			_shortStringHelper = GetRequiredService<IShortStringHelper>();
			_templateService = GetRequiredService<ITemplateService>();
		}

		protected override void CustomTestSetup(IUmbracoBuilder builder)
		{
			builder.Services.AddTransient<ITvMazeService, TvMazeService>();
			builder.Services.AddTransient<IImportMediaService, ImportMediaService>();
			builder.Services.AddTransient<IImportContentService, ImportContentService>();
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

		//[Test]
		//public void ImportBulkMedia_CreateAndSafeAllMediaInBackoffice()
		//{
		//	var allShows = _tvMazeService.GetAllAsync().GetAwaiter().GetResult();

		//	var mediaFolder = _importMediaService.CreateMediaRootFolder();

		//	_importMediaService.ImportBulkMedia(allShows);

		//	int count = _mediaService.CountChildren(mediaFolder.Id);

		//	Assert.AreEqual(240, count);
		//}

		[Test]
		public void ImportContent_CreateAndSaveAllTvShowsAsNodesInBackoffice()
        {
            var builder = new ContentTypeBuilder();
			var contentType = (ContentType)builder
				.WithAlias("tVShow")
				.WithName("TV Show")
				.WithSortOrder(1)
				.AddPropertyType()
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
				.WithValueStorageType(ValueStorageType.Ntext)
				.WithAlias("showSummary")
				.WithName("Show Summary")
				.WithSortOrder(1)
				.WithDataTypeId(Constants.DataTypes.RichtextEditor)
				.Done()
				.AddPropertyType()
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MediaPicker3)
				.WithValueStorageType(ValueStorageType.Nvarchar)
				.WithAlias("showImage")
				.WithName("Show Image")
				.WithSortOrder(2)
				.Done()
				.Build();


            if (contentType == null)
            {
                Assert.Fail("Content type is null.");
            }

            try
            {
                _contentTypeService.Save(contentType);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to save content type: {ex.Message}");
            }

            var importedContent = _importContentService.ImportContentAsync(-1);

			int count = _contentService.Count("tVShow");
			Assert.AreEqual(240, count);
        }

		[Test]
		public async Task ImportContent_CheckIfCreatedContentNodesHasTemplateAssigned()
		{
			var templateBuilder = new TemplateBuilder();
			var template = (Template)templateBuilder
				.WithAlias("show")
				.WithName("Show")
				.Build();

			await _templateService.CreateAsync(template,Constants.Security.SuperUserKey);

			var contentTypeBuilder = new ContentTypeBuilder();
            var contentType = (ContentType)contentTypeBuilder
				.WithAlias("tVShow")
                .WithName("TV Show")
                .WithSortOrder(1)
                .AddPropertyType()
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithAlias("showSummary")
                .WithName("Show Summary")
                .WithSortOrder(1)
                .WithDataTypeId(Constants.DataTypes.RichtextEditor)
                .Done()
                .AddPropertyType()
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MediaPicker3)
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .WithAlias("showImage")
                .WithName("Show Image")
                .WithSortOrder(2)
                .Done()
				.AddAllowedTemplate()
				.WithId(template.Id)
                .WithAlias("show")
				.WithName("Show")
                .Done()
				.WithDefaultTemplateId(template.Id)
                .Build();

			_contentTypeService.Save(contentType);

            var importedContent = _importContentService.ImportContentAsync(-1);

			long totalRecords;
			var contentNodes = _contentService.GetPagedChildren(-1, 0, int.MaxValue, out totalRecords);

			foreach (var contentNode in contentNodes)
			{
				Assert.IsNotNull(contentNode.TemplateId, $"Content node with name {contentNode.Name} does not have a template attached");
			}
		}
	}
}