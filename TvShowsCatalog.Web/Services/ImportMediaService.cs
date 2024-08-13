using Microsoft.Extensions.Logging;
using TvShowsCatalog.Web.Models.ApiModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace TvShowsCatalog.Web.Services
{
	public class ImportMediaService : IImportMediaService
	{
		private readonly IMediaService _mediaService;
		private readonly MediaFileManager _mediaFileManager;
		private readonly IShortStringHelper _shortStringHelper;
		private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
		private readonly MediaUrlGeneratorCollection _mediaUrlGeneratorCollection;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ImportMediaService> _logger;
		private readonly HttpClient _httpClient;
		private readonly ICoreScopeProvider _coreScopeProvider;


		public ImportMediaService(IMediaService mediaService,MediaFileManager mediaFileManager, IShortStringHelper shortStringHelper, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, MediaUrlGeneratorCollection mediaUrlGenerators, IHttpClientFactory httpClientFactory, ILogger<ImportMediaService> logger, HttpClient httpClient, ICoreScopeProvider coreScopeProvider)
        {
             _mediaService = mediaService;
			_mediaFileManager = mediaFileManager;
			_shortStringHelper = shortStringHelper;
			_contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
			_mediaUrlGeneratorCollection = mediaUrlGenerators;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_httpClient = httpClient;
			_coreScopeProvider = coreScopeProvider;

		}
		public IMedia CreateMediaRootFolder()
		{
			var existingFolder = _mediaService.GetRootMedia().FirstOrDefault
				(m => m.Name == "Tv Maze images folder" && m.ContentType.Alias == Constants.Conventions.MediaTypes.Folder);

            if (existingFolder != null)
            {
                return existingFolder;
            }

            IMedia folder = _mediaService.CreateMedia("Tv Maze images folder", Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
			_mediaService.Save(folder);

			return folder;
		}

		public async Task<IMedia> ImportMediaAsync(TvMazeModel tvshow)
		{
			// Added a check if media is already imported? If content is gone but media is still there in the media section.
			if (tvshow == null || string.IsNullOrEmpty(tvshow.Image?.Original))
			{
				return null;
			}

			try
			{
				var mediaFolder = CreateMediaRootFolder();
				var fileFormat = GetImageFileFormat($"{tvshow.Image?.Original}");
				var fileName = $"{tvshow.Name}{fileFormat}";
				// using is for proper disposal of the web client
				using (var client = HttpClientFactory.Create())
				{
					var response = await client.GetAsync(tvshow.Image?.Original);
					if (!response.IsSuccessStatusCode)
					{
						return null;
					}

					using (var stream = await response.Content.ReadAsStreamAsync())
					{
						using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
						var media = _mediaService.CreateMedia(fileName, mediaFolder.Id, Constants.Conventions.MediaTypes.Image);
						media.SetValue(_mediaFileManager, _mediaUrlGeneratorCollection, _shortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File, fileName, stream);

						_mediaService.Save(media);
						scope.Complete();

						return media;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error getting media imported: ", ex);
				return null;
			}
		}

		public string GetImageFileFormat(string url)
		{
			var fileFormat = Path.GetExtension(url);
			return fileFormat;
		}

		public IEnumerable<IMedia> ImportBulkMedia(IEnumerable<TvMazeModel> tvshows)
		{
			using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
			List<IMedia> media = new List<IMedia>();
			foreach (var show in tvshows)
			{
				var image = ImportMediaAsync(show).GetAwaiter().GetResult();
				media.Add(image);
			}
			scope.Complete();
			return media;
		}
	}
}
