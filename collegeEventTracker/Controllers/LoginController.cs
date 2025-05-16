using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using collegeEventTracker2.Models;

namespace collegeEventTracker2.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;

        public LoginController(IConfiguration configuration)
        {
            _config = configuration;
        }

        [HttpPost]
        public IActionResult Login([FromBody] User loginData)
        {
            if (loginData == null || string.IsNullOrEmpty(loginData.username) || string.IsNullOrEmpty(loginData.password))
                return BadRequest("Invalid login data.");

            string connStr = _config.GetConnectionString("DefaultConnection");
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT PasswordHash , role  FROM users WHERE name = @username LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", loginData.username);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedPasswordHash = reader["PasswordHash"].ToString();
                        string role = reader["role"].ToString();

                        // Use the retrieved values as needed
                        if (VerifyPassword(loginData.password, storedPasswordHash))
                        {
                            var token = GenerateJwtToken(loginData.username,role);
                            return Ok(new { token });
                        }
                       
                    }  
                    return Unauthorized("Invalid credentials"); 
                }
          
            }
         }
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Simple example (replace with real hash check)
            return enteredPassword == storedHash;
        }

        private string GenerateJwtToken(string username , string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] Users newUser)
        {
            if (newUser == null)
                return BadRequest("User data is null.");

            try
            {
                string connStr = _config.GetConnectionString("DefaultConnection");
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "INSERT INTO users (name, PasswordHash, Role ,email ) VALUES (@username, @password, @role ,@email)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", newUser.username);
                    cmd.Parameters.AddWithValue("@password", newUser.password); // Plaintext
                   cmd.Parameters.AddWithValue("@role", newUser.Role ?? "User"); // default to "User"
                   cmd.Parameters.AddWithValue("@email", newUser.email);
                    cmd.ExecuteNonQuery();

                    return Ok(new { message = "User registered successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}