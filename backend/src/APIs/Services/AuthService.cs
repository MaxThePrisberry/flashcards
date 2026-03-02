using Flashcards.APIs.DTOs.User;
using Flashcards.APIs.Requests.User;
using Flashcards.APIs.Responses;
using Flashcards.APIs.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Flashcards.APIs.Entities;
using Npgsql;

namespace Flashcards.APIs.Services.Auth {
    public class AuthService {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new();

        private readonly AppDbContext _dbContext;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _expiresInMinutes;
        private readonly SigningCredentials _signingCredentials;

        public AuthService(AppDbContext dbContext, IConfiguration configuration) {
            _dbContext = dbContext;
            _jwtIssuer = configuration["Jwt:Issuer"]!;
            _jwtAudience = configuration["Jwt:Audience"]!;
            _expiresInMinutes = int.Parse(configuration["Jwt:ExpiresInMinutes"] ?? "60");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        public async Task<AuthResponse> SignupAsync(SignupRequest request) {
            var normalizedEmail = request.Email.ToLowerInvariant();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User {
                UserId = Guid.NewGuid(),
                Username = request.DisplayName,
                Email = normalizedEmail,
                Password = passwordHash
            };

            _dbContext.Users.Add(user);
            try {
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) {
                throw new ConflictException("Email already registered.");
            }

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request) {
            var normalizedEmail = request.Email.ToLowerInvariant();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) {
                throw new UnauthorizedException("Invalid email or password.");
            }

            return BuildAuthResponse(user);
        }

        private AuthResponse BuildAuthResponse(User user) {
            var token = GenerateJwtToken(user);
            var expiresIn = _expiresInMinutes * 60;
            return new AuthResponse(
                token,
                expiresIn,
                new UserDTO(user.UserId, user.Email, user.Username, user.CreatedAt)
            );
        }

        private string GenerateJwtToken(User user) {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiresInMinutes),
                signingCredentials: _signingCredentials
            );

            return _tokenHandler.WriteToken(token);
        }
    }
}
