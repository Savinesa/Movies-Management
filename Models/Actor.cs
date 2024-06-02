using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MoviesAPI.Models
{
    public class Actor
    {
        [Column("actorid")]
        public int ActorId { get; set; }

        [Column("actorname")]
        public string ActorName { get; set; }

        [JsonIgnore]
        public virtual ICollection<MovieActor> MovieActors { get; set; }

    }
}
