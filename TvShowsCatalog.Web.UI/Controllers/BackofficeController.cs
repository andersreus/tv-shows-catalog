using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace TvShowsCatalog.Web.UI.Controllers;

// UmbracoApiController is obsolete in Umbraco 14. So I used the "Controller" base class instead from .net core.
[ApiController]
[Route("/umbraco/api/getbackofficeinfo")]
public class BackofficeController : Controller
{
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;

    public BackofficeController(IContentService contentService, IMediaService mediaService)
    {
        _contentService = contentService;
        _mediaService = mediaService;
    }
    
    [HttpGet("getamountoftvshowsinbackoffice")]
    public int GetAmountOfTVShowsInBackOffice()
    {
        var count = _contentService.Count("tvShow");
        return count;
    }
    
    [HttpGet("getamountoftvshowimagesinmediasection")]
    public int GetAmountOfTvShowImagesInMediasection()
    {
        var count = _mediaService.Count("image");
        return count;
    }
}