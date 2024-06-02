using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesAPI.Models
{
    public class Director
    {
        [Column("directorid")]

        public int DirectorId { get; set; }
        [Column("directorname")]

        public string DirectorName { get; set; }
    }
}
