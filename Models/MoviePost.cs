using CsvHelper;
using System.Xml.Linq;

namespace MoviesAPI.Models
{
    public class MoviePost
    {
        public string Name { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public string Director { get; set; }
        public string Actors { get; set; }
        public int? Year { get; set; }
        public int? Runtime { get; set; }
        public decimal? Rating { get; set; }
        public int Votes { get; set; }
        public decimal? Revenue { get; set; }
        public int? Metascore { get; set; }

    }
}
