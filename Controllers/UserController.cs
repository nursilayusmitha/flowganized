using Microsoft.AspNetCore.Mvc;
using Flowganized.Data;
using Flowganized.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MailKit.Net.Smtp;
using MimeKit;
using System.Text.RegularExpressions;
using BCrypt.Net;
using Flowganized.DTOs.Users;

namespace Flowganized.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public UserController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // =========================== PASSWORD HASHING =============================
    private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    private bool VerifyPassword(string input, string hashed) => BCrypt.Net.BCrypt.Verify(input, hashed);

    // =========================== AUTH =========================================
    private int GenerateRandomUserId()
    {
        var random = new Random();
        int id;

        do
        {
            id = random.Next(100_000_000, 999_999_999);
        } while (_context.Users.Any(u => u.UserId == id)); // Pastikan tidak ada yang pakai

        return id;
    }

    private async Task<int> GenerateRandomUserIdAsync()
    {
        var random = new Random();
        int id;
        do
        {
            id = random.Next(100_000_000, 999_999_999);
        } while (await _context.Users.AnyAsync(u => u.UserId == id));
        return id;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO request)
    {
        if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return BadRequest("Format email tidak valid.");

        if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_]{3,20}$"))
            return BadRequest("Username harus 3–20 karakter dan hanya huruf, angka, atau underscore.");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email sudah digunakan.");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username sudah digunakan.");

        if (request.Password.Length < 8)
            return BadRequest("Password minimal 8 karakter.");

        var newUser = new User
        {
            UserId = await GenerateRandomUserIdAsync(),
            Name = request.Name,
            Username = request.Username,
            Email = request.Email,
            Password = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        // NEW: Kirim email verifikasi
        var token = Guid.NewGuid().ToString();
        var expiredAt = DateTime.UtcNow.AddHours(1);
        var verification = new EmailVerificationToken
        {
            UserId = newUser.UserId,
            Token = token,
            ExpiredAt = expiredAt
        };
        _context.EmailVerificationTokens.Add(verification);
        await _context.SaveChangesAsync();

        var verifyUrl = $"https://yourfrontend.com/verify-email?token={token}";
        var htmlBody = LoadTemplate("VerifyEmail.html", new()
        {
            { "name", newUser.Name },
            { "link", verifyUrl }
        });
        await SendEmail(newUser.Email, "Verifikasi Akun Anda", htmlBody);

        return Ok("Registrasi berhasil. Silakan cek email untuk verifikasi.");
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var entry = await _context.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == token && !e.IsUsed);

        if (entry == null || entry.ExpiredAt < DateTime.UtcNow)
            return BadRequest("Token tidak valid atau kadaluarsa.");

        entry.User.IsVerified = true;
        entry.IsUsed = true;
        entry.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok("Email berhasil diverifikasi.");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO request)
    {
        if (request.Password.Length < 8)
            return BadRequest("Password minimal 8 karakter.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);
        if (user == null || !VerifyPassword(request.Password, user.Password))
            return Unauthorized("Email atau password salah.");

        if (!user.IsVerified)
            return Unauthorized("Email belum diverifikasi.");

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }


    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key tidak ditemukan.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()), // ⬅️ Tambahkan ini

            },
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ========================== CRUD ==========================================

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Users.Where(u => !u.IsDeleted).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.IsDeleted) return NotFound();
        return Ok(user);
    }

   [HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDTO updated)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null || user.IsDeleted) return NotFound();

    if (await _context.Users.AnyAsync(u => u.Username == updated.Username && u.UserId != id))
        return BadRequest("Username sudah digunakan oleh user lain.");

    if (await _context.Users.AnyAsync(u => u.Email == updated.Email && u.UserId != id))
        return BadRequest("Email sudah digunakan oleh user lain.");

    user.Name = updated.Name;
    user.Username = updated.Username;
    user.Email = updated.Email;
    user.PhoneNumber = updated.PhoneNumber;
    user.Gender = updated.Gender;
    user.BirthDate = updated.BirthDate;
    user.ImageName = updated.ImageName;
    user.Organization = updated.Organization;
    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return Ok("User diperbarui");
}



    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.IsDeleted) return NotFound();
        user.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Ok("User dihapus (soft delete)");
    }

    // ======================== RESET PASSWORD ===================================

    [HttpPost("request-password-reset")]
public async Task<IActionResult> RequestReset([FromBody] ResetRequestDTO request)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Email);
    if (user == null) return NotFound("Email/Username belum terdaftar.");

    var token = Guid.NewGuid().ToString();
    var expiredAt = DateTime.UtcNow.AddMinutes(15);

    var reset = new ResetToken { UserId = user.UserId, Token = token, ExpiredAt = expiredAt };
    _context.ResetTokens.Add(reset);
    await _context.SaveChangesAsync();

    var callbackUrl = $"https://yourfrontend.com/reset-password?token={token}";
    var htmlBody = LoadTemplate("ResetPasswordRequest.html", new()
    {
        { "name", user.Name },
        { "link", callbackUrl }
    });

    await SendEmail(user.Email, "Reset Password", htmlBody);
    return Ok("Link reset telah dikirim ke email.");
}
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO data)
{
    var tokenEntry = await _context.ResetTokens.Include(t => t.User)
        .FirstOrDefaultAsync(t => t.Token == data.Token && !t.IsUsed);

    if (tokenEntry == null || tokenEntry.ExpiredAt < DateTime.UtcNow)
        return BadRequest("Token tidak valid atau kadaluarsa.");

    var user = tokenEntry.User;

    if (VerifyPassword(data.NewPassword, user.Password))
        return BadRequest("Password baru tidak boleh sama dengan password lama.");

    if (data.NewPassword.Length < 8)
        return BadRequest("Password minimal 8 karakter.");

    if (data.NewPassword != data.ConfirmPassword)
        return BadRequest("Password dan konfirmasi tidak cocok.");

    user.Password = HashPassword(data.NewPassword);
    user.UpdatedAt = DateTime.UtcNow;
    tokenEntry.IsUsed = true;

    await _context.SaveChangesAsync();

    var successBody = LoadTemplate("ResetPasswordSuccess.html", new()
    {
        { "name", user.Name }
    });
    await SendEmail(user.Email, "Reset Password Berhasil", successBody);

    return Ok("Password berhasil direset.");
}


    [HttpGet("verify-token")]
    public async Task<IActionResult> VerifyResetToken([FromQuery] string token)
    {
        var tokenEntry = await _context.ResetTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

        if (tokenEntry == null || tokenEntry.ExpiredAt < DateTime.UtcNow)
            return BadRequest("Token tidak valid atau kadaluarsa.");

        return Ok("Token valid.");
    }

    // ======================== EMAIL ===========================================

    private async Task SendEmail(string email, string subject, string bodyHtml)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_config["Email:FromName"] ?? "Flowganized", _config["Email:FromEmail"] ?? "noreply@domain.com"));
        msg.To.Add(MailboxAddress.Parse(email));
        msg.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = bodyHtml };
        msg.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["Email:Host"] ?? throw new Exception("SMTP host not found"), int.Parse(_config["Email:Port"] ?? "465"), true);
        await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }

    private string LoadTemplate(string filename, Dictionary<string, string> data)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Templates", filename);
        var html = System.IO.File.ReadAllText(path);

        foreach (var pair in data)
            html = html.Replace($"{{{{{pair.Key}}}}}", pair.Value);

        return html;
    }
}
