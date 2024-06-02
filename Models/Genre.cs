using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesAPI.Models
{
    public class Genre
    {
        [Column("genreid")]
        public int GenreId { get; set; }

        [Column("genrename")]
        public string GenreName { get; set; }
    }
}
