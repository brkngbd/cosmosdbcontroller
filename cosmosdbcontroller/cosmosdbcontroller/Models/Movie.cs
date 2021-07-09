using System.Text.Json;

namespace cosmosdbcontroller
{
    public class Movie
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The movie item identifier.</value>
        public string id { get; set; } // in format "Movie title.Movie Year"

        /// <summary>Gets or sets the title.</summary>
        /// <value>The movie title.</value>
        public string Title { get; set; }

        /// <summary>Gets or sets the imdb rating.</summary>
        /// <value>The imdb rating of the movie.</value>
        public string ImdbRating { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
