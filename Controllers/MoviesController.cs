using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.Models;
using System.Formats.Asn1;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region GetAll
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies()
        {

            var movies = await _context.movies
               .Include(m => m.Director)
               .Include(m => m.Genre)
               .Include(m => m.MovieActors)
            .ThenInclude(ma => ma.Actor)
               .Select(m => new MovieDto
               {
                   Name = m.Name,
                   GenreName = string.Join("- ", m.Genre.GenreName),
                   Description = m.Description,
                   DirectorName = m.Director.DirectorName,
                   Actors = string.Join(", ", m.MovieActors.Select(ma => ma.Actor.ActorName)),
                   Year = m.Year,
                   Runtime = m.Runtime,
                   Rating = m.Rating,
                   Votes = m.Votes,
                   Revenue = m.Revenue,
                   Metascore = m.Metascore
               })
               .ToListAsync();

            return movies;
        }
        #endregion

        #region Search
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<MovieDto>>> Search(string query)
        {
            query = query.Trim().ToLower();
            var movies = await _context.movies
                .Include(m => m.Director)
                .Include(m => m.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Where(m => m.Name.ToLower().Contains(query) ||
                            m.Director.DirectorName.ToLower().Contains(query) ||
                            m.MovieActors.Any(ma => ma.Actor.ActorName.ToLower().Contains(query)) ||
                            m.Year.ToString() == query)
                .Select(m => new MovieDto
                {
                    Name = m.Name,
                    GenreName = m.Genre.GenreName,
                    Description = m.Description,
                    DirectorName = m.Director.DirectorName,
                    Actors = string.Join(", ", m.MovieActors.Select(ma => ma.Actor.ActorName)), 
                    Year = m.Year,
                    Runtime = m.Runtime,
                    Rating = m.Rating,
                    Votes = m.Votes,
                    Revenue = m.Revenue,
                    Metascore = m.Metascore
                })
                .ToListAsync();

            return movies;
        }
        #endregion

        #region Upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var path = Path.GetTempFileName();  // Temporary file path - so i don't save it to server

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);  // Save the uploaded file to the temp file
            }

            var records = new List<MoviePost>(); // Created records to fill it with data received
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) // set to treat the CSV data
            {
                MissingFieldFound = null  // ignore any missing fields in the CSV data
            }))
            {
                csv.Context.RegisterClassMap<InlineClassMap>();
                records = csv.GetRecords<MoviePost>().ToList(); //  parses the CSV file into instances of the Movie class
            }

            var validRecords = records.AsParallel().Where(IsValidRecord).ToList();
            foreach (var record in records)
            {
                if (IsValidRecord(record))
                {
                    validRecords.Add(record);
                }
            }

            if (!validRecords.Any())
            {
                return BadRequest("No valid records found.");
            }

            foreach (var csvRecord in records)
            {
                var movie = new Movie
                {
                    Name = csvRecord.Name,
                    Description = csvRecord.Description,
                    Year = csvRecord.Year,
                    Runtime = csvRecord.Runtime,
                    Rating = csvRecord.Rating,
                    Votes = csvRecord.Votes,
                    Revenue = csvRecord.Revenue,
                    Metascore = csvRecord.Metascore,
                    DirectorId = ResolveDirectorId(csvRecord.Director),
                    GenreId = ResolveGenreId(csvRecord.Genre),
                    MovieActors = ResolveActors(csvRecord.Actors)
                };

                _context.movies.Add(movie);
            }
            await _context.SaveChangesAsync();


            return Ok(new { Count = validRecords.Count });
        }
        #endregion

        #region Delete
        [HttpDelete("DeleteAll")]
        public async Task<IActionResult> DeleteAll()
        {
            var allMovies = await _context.movies.Include(m => m.MovieActors).ToListAsync();

            _context.movies.RemoveRange(allMovies);
            await _context.SaveChangesAsync();  // This saves the changes and deletes the movies along with related movie actors

            return NoContent();
        }
        #endregion

        #region Helpers
        private bool IsValidRecord(MoviePost record)
        {
            // Example validation logic
            return !string.IsNullOrEmpty(record.Name);
        }

        private int? ResolveDirectorId(string directorName)
        {
            if (string.IsNullOrEmpty(directorName))
                return null;

            var director = _context.directors
                                   .FirstOrDefault(d => d.DirectorName.ToLower() == directorName.ToLower());

            if (director == null)
            {
                director = new Director { DirectorName = directorName };
                _context.directors.Add(director);
                _context.SaveChanges();  // Save changes to generate the ID for the new director
            }

            return director.DirectorId;
        }

        private int? ResolveGenreId(string genreName)
        {
            if (string.IsNullOrEmpty(genreName))
                return null;

            var genre = _context.genres
                                .FirstOrDefault(g => g.GenreName.ToLower() == genreName.ToLower());

            if (genre == null)
            {
                genre = new Genre { GenreName = genreName };
                _context.genres.Add(genre);
                _context.SaveChanges();  // Save changes to generate the ID for the new genre
            }

            return genre.GenreId;
        }
        private ICollection<MovieActor> ResolveActors(string actors)
        {
            var result = new List<MovieActor>();
            if (string.IsNullOrEmpty(actors))
                return result;

            var actorNames = actors.Split(',');
            foreach (var name in actorNames)
            {
                var trimmedName = name.Trim();
                var actor = _context.actors
                                    .FirstOrDefault(a => a.ActorName.ToLower() == trimmedName.ToLower());

                if (actor == null)
                {
                    actor = new Actor { ActorName = trimmedName };
                    _context.actors.Add(actor);
                    _context.SaveChanges();  // Save to get the ID
                }

                result.Add(new MovieActor { Actor = actor });
            }

            return result;
        }

        // had to add this to be able to read Runtime and Revenu
        class InlineClassMap : ClassMap<MoviePost>
        {
            public InlineClassMap()
            {
                Map(m => m.Name).Name("Name");
                Map(m => m.Genre).Name("Genre");
                Map(m => m.Description).Name("Description");
                Map(m => m.Director).Name("Director");
                Map(m => m.Actors).Name("Actors");
                Map(m => m.Year).Name("Year").Optional().TypeConverterOption.NullValues(""); // Handle empty strings for Year
                Map(m => m.Runtime).Name("Runtime (Minutes)");
                Map(m => m.Rating).Name("Rating");
                Map(m => m.Votes).Name("Votes");
                Map(m => m.Revenue).Name("Revenue (Millions)").Optional().TypeConverterOption.NullValues("");
                Map(m => m.Metascore).Name("Metascore").Optional().TypeConverterOption.NullValues("");
            }
        }
        #endregion

    }
}
