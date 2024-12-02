namespace TvShowsCatalog.Web.Services;

public interface ITranslationService
{
    public string GetTranslation(string culture, string alias);
}