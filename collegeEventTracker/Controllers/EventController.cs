using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using collegeEventTracker2.Models;
using Microsoft.AspNetCore.Authorization;
namespace collegeEventTracker2.Controllers
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : Controller
    {
        private readonly IConfiguration _config;

        public ValuesController(IConfiguration configuration)
        {
            _config = configuration;
        }

        // GET api/values
        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            string ConnectionString = _config.GetConnectionString("DefaultConnection");
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM events";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                List <Event> events = new List<Event>();
                while (reader.Read())
                {
                    Event myevent = new Event();
                      myevent.EventId = reader.GetInt32(0);
                    myevent.Title = reader.GetString(1);
               myevent.Description = reader.GetString(2);
            myevent.Venue = reader.GetString(3);
           
          
                    myevent.DateTime = reader.GetDateTime(4);
                    myevent.OrganizerId = reader.GetInt32(5);
            myevent.MaxParticipants = reader.GetInt32(6);
             myevent.IsTeamEvent =    reader.GetBoolean(7);
                    events.Add(myevent);
                }
                return (View(events));

            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public IActionResult UpdateEvent([FromBody] Event myevent)
        {
            string ConnectionString = _config.GetConnectionString("DefaultConnection");
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "UPDATE events SET Title = @Title, Description = @Description WHERE EventId = @EventId";
                MySqlCommand command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@Title", myevent.Title);
                command.Parameters.AddWithValue("@Description", myevent.Description);
                command.Parameters.AddWithValue("@EventId", myevent.EventId);
                command.ExecuteNonQuery();
                return Ok(new { message = "Event updated successfully" });
            }
        }
        [Authorize (Roles="Admin")]
        [HttpPost("add")]
        public IActionResult AddEvent([FromBody] Event newEvent)
        {
            if (newEvent == null)
                return BadRequest("Event data is null.");

            try
            {
                Console.WriteLine(newEvent);
                string ConnectionString = _config.GetConnectionString("DefaultConnection");
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO events ( Title, Description, Venue, DateTime, OrganizerId, MaxParticipants, IsTeamEvent) " +
                                   "VALUES ( @title, @description, @venue, @dateTime, @organizerId, @maxParticipants, @isTeamEvent)";
                    MySqlCommand command = new MySqlCommand(query, conn);
                   // command.Parameters.AddWithValue("@EventId", newEvent.EventId);
                    command.Parameters.AddWithValue("@title", newEvent.Title);
                    command.Parameters.AddWithValue("@description", newEvent.Description);
                    command.Parameters.AddWithValue("@venue", newEvent.Venue);
                    command.Parameters.AddWithValue("@dateTime", newEvent.DateTime);
                    command.Parameters.AddWithValue("@organizerId", newEvent.OrganizerId);
                    command.Parameters.AddWithValue("@maxParticipants", newEvent.MaxParticipants);
                    command.Parameters.AddWithValue("@isTeamEvent", newEvent.IsTeamEvent);

                    command.ExecuteNonQuery();
                    return Ok(new { message = "Event added successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [Authorize( Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteEvent(int id)
        {
            string ConnectionString = _config.GetConnectionString("DefaultConnection");
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "DELETE FROM events WHERE EventId = @id";
                MySqlCommand command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                    return Ok(new { message = "Event deleted successfully" });
                else
                    return NotFound(new { message = "Event not found" });
            }
        }
    }
}