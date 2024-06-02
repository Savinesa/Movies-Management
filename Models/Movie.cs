using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json.Serialization;

namespace MoviesAPI.Models
{
    public class Movie
    {
        [Column("movieid")]
        public int MovieId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("directorid")]
        public int? DirectorId { get; set; }

        [JsonIgnore]
        public virtual Director Director { get; set; }

        [Column("genreid")]
        public int? GenreId { get; set; }

        [JsonIgnore]
        public virtual Genre Genre { get; set; }

        [Column("year")]
        public int? Year { get; set; }

        [Column("runtime")]
        public int? Runtime { get; set; }

        [Column("rating")]
        public decimal? Rating { get; set; }

        [Column("votes")]
        public int? Votes { get; set; }

        [Column("revenue")]
        public decimal? Revenue { get; set; }

        [Column("metascore")]
        public int? Metascore { get; set; }

        [JsonIgnore]
        public virtual ICollection<MovieActor> MovieActors { get; set; }
    }
}
