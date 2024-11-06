using System.Diagnostics;
using System.Reflection.Metadata;
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
using Lucene.Net.Search;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;

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
		//private IDataTypeService _dataTypeService;
		//private IShortStringHelper _shortStringHelper;
		private ITemplateService _templateService;
		
		private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();
		private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();
		protected PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

		[SetUp]
		public void Setup()
		{
			_mediaService = GetRequiredService<IMediaService>();
			_tvMazeService = GetRequiredService<ITvMazeService>();
			_importMediaService = GetRequiredService<IImportMediaService>();
			_importContentService = GetRequiredService<IImportContentService>();
            _contentTypeService = GetRequiredService<IContentTypeService>();
            _contentService = GetRequiredService<IContentService>();
			//_dataTypeService = GetRequiredService<IDataTypeService>();
			//_shortStringHelper = GetRequiredService<IShortStringHelper>();
			_templateService = GetRequiredService<ITemplateService>();
		}

		protected override void CustomTestSetup(IUmbracoBuilder builder)
		{
			builder.Services.AddTransient<ITvMazeService, TvMazeService>();
			builder.Services.AddTransient<IImportMediaService, ImportMediaService>();
			builder.Services.AddTransient<IImportContentService, ImportContentService>();
		}

		[Test]
		public async Task Return_All_TvMazeModel_Objects()
		{
			var allShows = await _tvMazeService.GetAllAsync();
			Assert.IsTrue(allShows.Any());
		}

		[Test]
		public async Task Create_A_Single_Media_In_Backoffice()
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
		
		[ Test]
		public async Task Create_All_TvShows_As_Content_In_Backoffice()
        {
	        var genreElementType = new ContentTypeBuilder()
		        .WithAlias("genre")
		        .WithName("Genre")
		        .WithIsElement(true)
		        .AddPropertyType()
		        .WithAlias("indexNumber")
		        .WithName("Index Number")
		        .WithDataTypeId(Constants.DataTypes.Textbox)
		        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
		        .WithValueStorageType(ValueStorageType.Nvarchar)
		        .Done()
		        .AddPropertyType()
		        .WithAlias("title")
		        .WithName("Title")
		        .WithDataTypeId(Constants.DataTypes.Textbox)
		        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
		        .WithValueStorageType(ValueStorageType.Nvarchar)
		        .Done()
		        .Build();
	        _contentTypeService.Save(genreElementType);
	        var nestedBlockListDataType = await CreateBlockListDataType(genreElementType);
	        
            var tvShowContentType = new ContentTypeBuilder()
				.WithAlias("tVShow")
				.WithName("TV Show")
				.AddPropertyType()
				.WithAlias("genres")
				.WithName("Genres")
				.WithDataTypeId(nestedBlockListDataType.Id)
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
				.WithValueStorageType(ValueStorageType.Ntext)
				.Done()
				.AddPropertyType()
				.WithAlias("showSummary")
				.WithName("Show Summary")
				.WithDataTypeId(Constants.DataTypes.RichtextEditor)
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
				.WithValueStorageType(ValueStorageType.Ntext)
				.Done()
				.AddPropertyType()
				.WithAlias("showImage")
				.WithName("Show Image")
				// .WithDataTypeId(Constants.DataTypes.)
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MediaPicker3)
				.WithValueStorageType(ValueStorageType.Nvarchar)
				.Done()
				.Build();
			_contentTypeService.Save(tvShowContentType);

			var parentId = -1;
			var importAmount = 1000;
            var importedContent = await _importContentService.ImportContentAsync(parentId, importAmount);

			int count = _contentService.Count("tVShow");
			
			Assert.AreEqual(importAmount, count);
        }

		[Test]
		public async Task Create_All_TvShows_As_Content_In_Backoffice_With_Variants()
        {
	        var genreElementType = new ContentTypeBuilder()
		        .WithAlias("genre")
		        .WithName("Genre")
		        .WithIsElement(true)
		        .WithContentVariation(ContentVariation.Culture)
		        .AddPropertyType()
		        .WithAlias("indexNumber")
		        .WithName("Index Number")
		        .WithDataTypeId(Constants.DataTypes.Textbox)
		        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
		        .WithValueStorageType(ValueStorageType.Nvarchar)
		        .WithVariations(ContentVariation.Nothing)
		        .Done()
		        .AddPropertyType()
		        .WithAlias("title")
		        .WithName("Title")
		        .WithDataTypeId(Constants.DataTypes.Textbox)
		        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
		        .WithValueStorageType(ValueStorageType.Nvarchar)
		        .WithVariations(ContentVariation.Culture)
		        .Done()
		        .Build();
	        _contentTypeService.Save(genreElementType);
	        var nestedBlockListDataType = await CreateBlockListDataType(genreElementType);
	        
            var tvShowContentType = new ContentTypeBuilder()
				.WithAlias("tVShow")
				.WithName("TV Show")
				.WithContentVariation(ContentVariation.Culture)
				.AddPropertyType()
				.WithAlias("genres")
				.WithName("Genres")
				.WithDataTypeId(nestedBlockListDataType.Id)
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.BlockList)
				.WithValueStorageType(ValueStorageType.Ntext)
				.WithVariations(ContentVariation.Culture)
				.Done()
				.AddPropertyType()
				.WithAlias("showSummary")
				.WithName("Show Summary")
				.WithDataTypeId(Constants.DataTypes.RichtextEditor)
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RichText)
				.WithValueStorageType(ValueStorageType.Ntext)
				.WithVariations(ContentVariation.Nothing)
				.Done()
				.AddPropertyType()
				.WithAlias("showImage")
				.WithName("Show Image")
				// .WithDataTypeId(Constants.DataTypes.)
				.WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MediaPicker3)
				.WithValueStorageType(ValueStorageType.Nvarchar)
				.WithVariations(ContentVariation.Nothing)
				.Done()
				.Build();
			_contentTypeService.Save(tvShowContentType);

			var parentId = -1;
			var importAmount = 1000;
            var importedContent = _importContentService.ImportContentAsync(parentId, importAmount);

			int count = _contentService.Count("tVShow");
			Assert.AreEqual(importAmount, count);
        }

		[Test]
		public async Task Check_If_Content_Nodes_Has_Template()
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

			var parentId = -1;
			var importAmount = 1000;
            var importedContent = _importContentService.ImportContentAsync(parentId, importAmount);

			long totalRecords;
			var contentNodes = _contentService.GetPagedChildren(-1, 0, int.MaxValue, out totalRecords);

			foreach (var contentNode in contentNodes)
			{
				Assert.IsNotNull(contentNode.TemplateId, $"Content node with name {contentNode.Name} does not have a template attached");
			}
		}
		
		// Help from source code -> BlockListElementLevelVariationTests.cs
		private async Task<IDataType> CreateBlockListDataType(IContentType elementType)
			=> await CreateBlockEditorDataType(
				Constants.PropertyEditors.Aliases.BlockList,
				new BlockListConfiguration.BlockConfiguration[]
				{
					new() { ContentElementTypeKey = elementType.Key, SettingsElementTypeKey = elementType.Key }
				});
		
		protected async Task<IDataType> CreateBlockEditorDataType<T>(string propertyEditorAlias, T blocksConfiguration)
		{
			var dataType = new DataType(PropertyEditorCollection[propertyEditorAlias],
				ConfigurationEditorJsonSerializer)
			{
				ConfigurationData = new Dictionary<string, object> { { "blocks", blocksConfiguration } },
				Name = "My Block Editor",
				DatabaseType = ValueStorageType.Ntext,
				ParentId = Constants.System.Root,
				CreateDate = DateTime.UtcNow
			};
			
			await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
			return dataType;
		}
	}
}