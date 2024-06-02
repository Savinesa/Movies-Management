namespace MoviesAPI.Models
{
    public class MovieDto
    {
        public string Name { get; set; }
        public string GenreName { get; set; }
        public string Description { get; set; }
        public string DirectorName { get; set; }
        public string Actors { get; set; }  
        public int? Year { get; set; }
        public int? Runtime { get; set; }
        public decimal? Rating { get; set; }
        public int? Votes { get; set; }
        public decimal? Revenue { get; set; }
        public int? Metascore { get; set; }
    }

}
