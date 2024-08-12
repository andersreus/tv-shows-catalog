using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace TvShowsCatalog.Web.UI.Controllers
{
    public class DeleteRootNodesController : UmbracoApiController
    {
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _contextFactory;

        public DeleteRootNodesController(IContentService contentService, IUmbracoContextFactory contextFactory)
        {
            _contentService = contentService;
            _contextFactory = contextFactory;
        }

        [HttpPost]
        public IActionResult DeleteChildNodesUnderRoot()
        {
            using (var contextReference = _contextFactory.EnsureUmbracoContext())
            {
                var rootNodes = _contentService.GetRootContent().ToList();
                foreach (var rootNode in rootNodes)
                {
                    var childNodes = _contentService.GetPagedChildren(rootNode.Id, 0, int.MaxValue, out long totalChildren).ToList();
                    if (childNodes.Any())
                    {
                        foreach (var childNode in childNodes)
                        {
                            _contentService.Delete(childNode);
                        }
                    }
                }
            }

            return Ok("All child nodes under root content nodes have been deleted.");
        }
    }
}
