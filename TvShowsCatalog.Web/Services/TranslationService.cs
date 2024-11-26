namespace TvShowsCatalog.Web.Services;

public class TranslationService : ITranslationService
{
    // Dictionary med alle translations
    // TKey -> tvshow.Genre (en version), TValue -> Translated version (dk version)

    private Dictionary<string, string> genreTranslations = new()
    {
        { "Action", "Action" },
        { "Adult", "Voksen" },
        { "Adventure", "Eventyr" },
        { "Anime", "Anime" },
        { "Children", "Børn" },
        { "Comedy", "Komedie" },
        { "Crime", "Krimi" },
        { "DIY", "Gør-det-selv" },
        { "Drama", "Drama" },
        { "Espionage", "Spionage" },
        { "Family", "Familie" },
        { "Fantasy", "Fantasy" },
        { "Food", "Mad" },
        { "History", "Historie" },
        { "Horror", "Gyser" },
        { "Legal", "Lovlig" },
        { "Medical", "Medicinsk" },
        { "Music", "Musik" },
        { "Mystery", "Mysterie" },
        { "Nature", "Natur" },
        { "Romance", "Romantisk" },
        { "Science-Fiction", "Science-Fiction" },
        { "Sports", "Sport" },
        { "Supernatural", "Overnaturligt" },
        { "Thriller", "Thriller" },
        { "Travel", "Rejse" },
        { "War", "Krig" },
        { "Western", "Western" }
    };
    
    public string GetTranslation(string culture, string alias)
    {
        throw new NotImplementedException();
    }
}