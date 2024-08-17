using Microsoft.AspNetCore.Mvc;
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

        public BulkDeleteBackofficeItemsController(IContentService contentService, IUmbracoContextFactory contextFactory, IMediaService mediaService)
        {
            _contentService = contentService;
            _contextFactory = contextFactory;
            _mediaService = mediaService;
        }

        // This was only for the time where tvshows was the only root node. Now there is Home root node.

        //[HttpPost]
        //public IActionResult DeleteChildrenContentNodes()
        //{
        //    using (var contextReference = _contextFactory.EnsureUmbracoContext())
        //    {
        //        var rootNodes = _contentService.GetRootContent().ToList();
        //        foreach (var rootNode in rootNodes)
        //        {
        //            var childNodes = _contentService.GetPagedChildren(rootNode.Id, 0, int.MaxValue, out long totalChildren).ToList();
        //            if (childNodes.Any())
        //            {
        //                foreach (var childNode in childNodes)
        //                {
        //                    _contentService.Delete(childNode);
        //                }
        //            }
        //        }
        //    }

        //    return Ok("All child nodes under root content nodes have been deleted.");
        //}


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
