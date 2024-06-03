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
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, "Error in loading the data " + ex);
            }
        }
        #endregion

        #region Search
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<MovieDto>>> Search(string query)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, "Error in searching the data " + ex);
            }
        }
        #endregion

        #region Upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            try
            {

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

<<<<<<< HEAD
                var path = Path.GetTempFileName();

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var records = new List<MoviePost>();
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null
                }))
                {
                    csv.Context.RegisterClassMap<InlineClassMap>();
                    records = csv.GetRecords<MoviePost>().ToList();
                }

                var validRecords = records.AsParallel().Where(IsValidRecord).ToList();

                if (!validRecords.Any())
                {
                    return BadRequest("No valid records found.");
                }

                var directors = _context.directors.ToDictionary(d => d.DirectorName.ToLower(), d => d);
                var genres = _context.genres.ToDictionary(g => g.GenreName.ToLower(), g => g);
                var actors = _context.actors.ToDictionary(a => a.ActorName.ToLower(), a => a);

                var newDirectors = new List<Director>();
                var newGenres = new List<Genre>();
                var newActors = new List<Actor>();

                foreach (var csvRecord in validRecords)
                {
                    ResolveEntity(directors, newDirectors, csvRecord.Director, name => new Director { DirectorName = name });
                    ResolveEntity(genres, newGenres, csvRecord.Genre, name => new Genre { GenreName = name });
                    ResolveActors(csvRecord.Actors, actors, newActors);
                }

                if (newDirectors.Any()) _context.directors.AddRange(newDirectors);
                if (newGenres.Any()) _context.genres.AddRange(newGenres);
                if (newActors.Any()) _context.actors.AddRange(newActors);

                await _context.SaveChangesAsync();

                var movies = new List<Movie>();
                foreach (var csvRecord in validRecords)
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
                        DirectorId = ResolveEntityId(directors, csvRecord.Director, "DirectorId"),
                        GenreId = ResolveEntityId(genres, csvRecord.Genre, "GenreId"),
                        MovieActors = ResolveActors(csvRecord.Actors, actors, new List<Actor>())
                    };

                    movies.Add(movie);
                }

                _context.movies.AddRange(movies);
                await _context.SaveChangesAsync();

                return Ok(new { Count = validRecords.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error in uploading the File " + ex);
            }
        }

       
        #endregion
=======
            var validRecords = records.AsParallel().Where(IsValidRecord).ToList();
            //foreach (var record in records)
            //{
            //    if (IsValidRecord(record))
            //    {
            //        validRecords.Add(record);
            //    }
            //}

            if (!validRecords.Any())
            {
                return BadRequest("No valid records found.");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
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
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }


                return Ok(new { Count = validRecords.Count });
            }
        }
            #endregion
>>>>>>> 013e57606df2a390dd0398d1d7eac712469ad84e

            #region Delete
            [HttpDelete("DeleteAll")]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                var allMovies = await _context.movies.Include(m => m.MovieActors).ToListAsync();

                _context.movies.RemoveRange(allMovies);
                await _context.SaveChangesAsync();  // This saves the changes and deletes the movies along with related movie actors

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error in deleting data " + ex);
            }
        }
        #endregion

        #region Helpers
        private bool IsValidRecord(MoviePost record)
        {
            return !string.IsNullOrEmpty(record.Name);
        }

        private void ResolveEntity<T>(Dictionary<string, T> existingEntities, List<T> newEntities, string name, Func<string, T> createEntity) where T : class
        {
            if (string.IsNullOrEmpty(name))
                return;

            var lowerName = name.ToLower();
            if (!existingEntities.ContainsKey(lowerName))
            {
                var entity = createEntity(name);
                newEntities.Add(entity);
                existingEntities[lowerName] = entity;
            }
        }

        private int? ResolveEntityId<T>(Dictionary<string, T> existingEntities, string name, string idPropertyName) where T : class
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var lowerName = name.ToLower();
            if (existingEntities.TryGetValue(lowerName, out var entity))
            {
                return (int)typeof(T).GetProperty(idPropertyName).GetValue(entity);
            }

            return null;
        }

        private ICollection<MovieActor> ResolveActors(string actors, Dictionary<string, Actor> existingActors, List<Actor> newActors)
        {
            var result = new List<MovieActor>();
            if (string.IsNullOrEmpty(actors))
                return result;

            var actorNames = actors.Split(',');
            foreach (var name in actorNames)
            {
                var trimmedName = name.Trim().ToLower();
                if (!existingActors.TryGetValue(trimmedName, out var actor))
                {
                    actor = new Actor { ActorName = name.Trim() };
                    newActors.Add(actor);
                    existingActors[trimmedName] = actor;
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
