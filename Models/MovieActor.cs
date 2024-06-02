using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MoviesAPI.Models
{
    public class MovieActor
    {
        [Column("movieid")]
        public int MovieId { get; set; }
        [JsonIgnore]

        public Movie Movie { get; set; }

        [Column("actorid")]
        public int ActorId { get; set; }
        [JsonIgnore]

        public Actor Actor { get; set; }

    }
}
