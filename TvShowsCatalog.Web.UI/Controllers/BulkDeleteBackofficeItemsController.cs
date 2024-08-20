using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace TvShowsCatalog.Web.UI.Controllers
{
    public class BulkDeleteBackofficeItemsController : UmbracoApiController
    {
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _contextFactory;
        private readonly IMediaService _mediaService;
		private readonly ICoreScopeProvider _coreScopeProvider;

        public BulkDeleteBackofficeItemsController(IContentService contentService, IUmbracoContextFactory contextFactory, IMediaService mediaService, ICoreScopeProvider coreScopeProvider)
        {
            _contentService = contentService;
            _contextFactory = contextFactory;
            _mediaService = mediaService;
			_coreScopeProvider = coreScopeProvider;
        }

        // To make sure I only delete children of the tvshow maze list view parent, pass in the id of that
        [HttpPost]
        public IActionResult DeleteChildrenContentNodes(int parentNodeId)
        {
            try
            {
				using (var contextReference = _contextFactory.EnsureUmbracoContext())
				{
					var rootNode = _contentService.GetById(parentNodeId);
					if (rootNode != null)
					{
						var childNodes = _contentService.GetPagedChildren(rootNode.Id, 0, int.MaxValue, out long totalChildren).ToList();
						if (childNodes.Any())
						{
							using (var scope = _coreScopeProvider.CreateCoreScope())
							{
								foreach (var childNode in childNodes)
								{
									_contentService.Delete(childNode);
								}
								scope.Complete();
							}	
						}
					}
				}

				return Ok("All child nodes under root content nodes have been deleted.");
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while deleting child nodes.");
			}
            
        }


        [HttpPost]
		public IActionResult DeleteChildrenMediaItems()
		{
			using (var contextReference = _contextFactory.EnsureUmbracoContext())
			{
				var mediaItems = _mediaService.GetRootMedia().ToList();
				foreach (var item in mediaItems)
				{
					var childMediaItem = _mediaService.GetPagedChildren(item.Id, 0, int.MaxValue, out long totalChildren).ToList();
					if (childMediaItem.Any())
					{
						foreach (var childNode in childMediaItem)
						{
							_mediaService.Delete(childNode);
						}
					}
				}
			}

			return Ok("All child items under root media items/folders have been deleted.");
		}
	}
}
