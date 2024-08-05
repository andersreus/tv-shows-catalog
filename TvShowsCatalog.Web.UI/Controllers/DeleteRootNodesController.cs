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
        public IActionResult DeleteAllRootNodes()
        {
            using (var contextReference = _contextFactory.EnsureUmbracoContext())
            {
                var rootNodes = _contentService.GetRootContent().ToList();
                foreach (var node in rootNodes)
                {
                    _contentService.Delete(node);
                }
            }

            return Ok("All root content nodes have been deleted.");
        }
    }
}
