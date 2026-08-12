using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Models;

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. REGISTER API
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            // Check karein email pehle se tou nahi hai
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Yeh email pehle se use mein hai.");
            }

            // Naya User banayen aur usko default 'User' (RoleId = 2) assign karein
            var newUser = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                // Password ko encrypt karke save kar rahe hain
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = 2
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Account successfully ban gaya hai!");
        }

        // 2. LOGIN API
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            // Database se user nikalien role ke sath
            var user = await _context.Users.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest("Email ya password ghalat hai.");
            }

            // Encrypted password ko match karein
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Email ya password ghalat hai.");
            }

            // Agar sab theek hai tou JWT Token generate karein
            string token = CreateToken(user);

            // Frontend ko Data aur Token dono bhej dein
            var responseData = new
            {
                Token = token,
                User = new UserResponseDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleName = user.Role!.RoleName
                }
            };

            return Ok(responseData);
        }

        // 3. JWT GENERATOR (Yeh method token banata hai)
        private string CreateToken(User user)
        {
            // Token mein kya kya maloomat chhupani hai (Claims)
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role!.RoleName) // Role authorization ke liye
            };

            // Secret key nikalien
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Key").Value!));

            // Algorithm set karein
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            // Token ki expiry aur details set karein (Misaal: 1 din ke liye valid)
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            // Token ko string mein convert karke return karein
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}