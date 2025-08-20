using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
using Flowganized.DTOs.Organization;
using Flowganized.Utils;
using Flowganized.Helpers;

namespace Flowganized.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public OrganizationController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDTO request)
    {
        // Ambil UserId dari JWT Token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized("Token tidak valid.");

        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized("UserId tidak valid.");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (user == null) return NotFound("User tidak ditemukan.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Buat Organization
            var organizationId = await IdGenerator.GenerateUniqueIdAsync<Organization>(_context, o => o.OrganizationId);
            var organization = new Organization
            {
                OrganizationId = organizationId,
                Name = request.Name,
                Description = request.Description,
                OwnerOrganizationId = userId,
                OrganizationType = request.OrganizationType,
                Status = OrganizationStatus.Active,
                AllowMultiDepartment = request.OrganizationType == OrganizationType.Business,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Organizations.Add(organization);

            // 2. Jalankan seeding data default
            var seeder = new OrganizationSeeder(_context);
            var seedingResult = await seeder.SeedDefaultDataAsync(organization);

            // 3. Buat UserOrganization untuk Owner
            var userOrgId = await IdGenerator.GenerateUniqueIdAsync<UserOrganization>(_context, uo => uo.UserOrganizationId);
            var ownerRoleId = seedingResult.OwnerRoleId;
            var generalDeptId = seedingResult.GeneralDepartmentId;

            var userOrg = new UserOrganization
            {
                UserOrganizationId = userOrgId,
                OrganizationId = organization.OrganizationId,
                UserId = userId,
                Status = MembershipStatus.Active,
                JoinType = JoinType.Created,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Position = new List<UserPosition> {
                    new() {
                        DepartmentId = generalDeptId,
                        RoleId = ownerRoleId
                    }
                }
            };
            _context.UserOrganizations.Add(userOrg);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
Console.WriteLine($"Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");


            return Ok(new { message = "Organisasi berhasil dibuat.", OrganizationId = organizationId });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Terjadi kesalahan saat membuat organisasi: {ex.Message}");
        }
    }
}
