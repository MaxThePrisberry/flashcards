using Flashcards.APIs.DTOs.User;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Flashcards.APIs.Entities;

namespace Flashcards.APIs.Services.Auth {
    public class AuthService {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext dbContext, IConfiguration configuration) {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<AuthResponse> SignupAsync(SignupRequest request) {
            // Check if user already exists
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null) {
                throw new Exception("User with this email already exists");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User {
                UserId = Guid.NewGuid(),
                Username = request.DisplayName,
                Email = request.Email,
                Password = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60") * 60; // Convert to seconds

            return new AuthResponse(
                token,
                expiresIn,
                new UserDTO(user.UserId, user.Email, user.Username, user.CreatedAt)
            );
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request) {
            // Find user by email
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null) {
                throw new Exception("Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) {
                throw new Exception("Invalid email or password");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60") * 60; // Convert to seconds

            return new AuthResponse(
                token,
                expiresIn,
                new UserDTO(user.UserId, user.Email, user.Username, user.CreatedAt)
            );
        }

        private string GenerateJwtToken(User user) {
            var secret = _configuration["Jwt:Secret"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
