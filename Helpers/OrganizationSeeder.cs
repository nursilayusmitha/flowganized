using Flowganized.Models;
using Microsoft.EntityFrameworkCore;
using Flowganized.Utils;
using Flowganized.Data;

namespace Flowganized.Helpers;

public class OrganizationSeeder
{
    private readonly AppDbContext _context;

    public OrganizationSeeder(AppDbContext context)
    {
        _context = context;
    }

    public class SeedingResult
    {
        public int OwnerRoleId { get; set; }
        public int GeneralDepartmentId { get; set; }
    }

    public async Task<SeedingResult> SeedDefaultDataAsync(Organization organization)
    {
        var result = new SeedingResult();
        var orgId = organization.OrganizationId;

        // ===== HIERARCHY =====
        var hierarchyData = organization.OrganizationType == OrganizationType.Business
            ? new List<(string Code, string Name)> {
                ("1.0", "Owner"),
                ("1.1", "HighManagement"),
                ("1.2", "MiddleManagement"),
                ("1.3", "LowManagement"),
                ("1.4", "HighEmployee"),
                ("1.5", "Employee"),
              }
            : new List<(string Code, string Name)> {
                ("1.0", "Owner"),
                ("1.1", "Member"),
              };

        var hierarchyMap = new Dictionary<string, int>();
        foreach (var (code, name) in hierarchyData)
        {
            var id = await IdGenerator.GenerateUniqueIdAsync<Hierarchy>(_context, h => h.HierarchyId);
            _context.Hierarchies.Add(new Hierarchy
            {
                HierarchyId = id,
                OrganizationId = orgId,
                Code = code,
                Name = name,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            hierarchyMap[code] = id;
        }
        await _context.SaveChangesAsync();

        // ===== PAGE =====
        var pageList = new List<(string Name, string Code, string Route, string Icon)>
        {
            ("Dashboard", "1.1", "/dashboard", "mdihome"),
            ("Project", "1.2", "/project", "mdifolder"),
            ("Notes", "1.3", "/notes", "mdinote")
        };

        var pageIdList = new List<int>();
        foreach (var (name, code, route, icon) in pageList)
        {
            var pageId = await IdGenerator.GenerateUniqueIdAsync<Page>(_context, p => p.PageId);
            _context.Pages.Add(new Page
            {
                PageId = pageId,
                OrganizationId = orgId,
                Name = name,
                Code = code,
                Routes = route,
                Icon = icon,
                Props = new List<PageProp>
                {
                    new() { IconWidth = "24", IconHeight = "24", IconViewBox = "0 0 24 24" }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            pageIdList.Add(pageId);
        }
        await _context.SaveChangesAsync();

        // ===== DEPARTMENT =====
        var deptMap = new Dictionary<string, int>();
        if (organization.OrganizationType == OrganizationType.Business)
        {
            var departments = new[] {
                new { Name = "General", Parent = (string?)null, IsGeneral = true, Desc = "Department umum untuk semua keperluan manajemen. Wajib ada di setiap organisasi." },
                new { Name = "Financial", Parent = (string?)null, IsGeneral = false, Desc = "Kelola keuangan organisasi, termasuk budgeting, cashflow, dan pengeluaran." },
                new { Name = "Accounting", Parent = (string?)"Financial", IsGeneral = false, Desc = "Melakukan pencatatan dan pelaporan keuangan." },
                new { Name = "Marketing", Parent = (string?)null, IsGeneral = false, Desc = "Menangani pemasaran, campaign, dan branding organisasi." },
                new { Name = "Production", Parent = (string?)null, IsGeneral = false, Desc = "Mengelola lini produksi dan output bisnis utama organisasi." },
                new { Name = "Engineering", Parent = (string?)null, IsGeneral = false, Desc = "Bertanggung jawab atas pengembangan sistem dan teknologi." }
            };


            foreach (var dept in departments)
            {
                var id = await IdGenerator.GenerateUniqueIdAsync<Department>(_context, d => d.DepartmentId);
                deptMap[dept.Name] = id;
            }

            foreach (var dept in departments)
            {
                _context.Departments.Add(new Department
                {
                    DepartmentId = deptMap[dept.Name],
                    OrganizationId = orgId,
                    DepartmentParentId = dept.Parent != null ? deptMap[dept.Parent] : null,
                    Name = dept.Name,
                    Description = dept.Desc,
                    IsGeneral = dept.IsGeneral,
                    Status = DepartmentStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
        }
        else
        {
            var generalId = await IdGenerator.GenerateUniqueIdAsync<Department>(_context, d => d.DepartmentId);
            deptMap["General"] = generalId;
            _context.Departments.Add(new Department
            {
                DepartmentId = generalId,
                OrganizationId = orgId,
                Name = "General",
                Description = "Departemen umum untuk organisasi tipe General. Bisa digunakan untuk semua user.",
                IsGeneral = true,
                Status = DepartmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }
        await _context.SaveChangesAsync();
        result.GeneralDepartmentId = deptMap["General"];

        // ===== ROLE =====
        var roleList = organization.OrganizationType == OrganizationType.Business
            ? new List<(string Name, string Dept, string Hierarchy, bool AllDept)>
            {
                ("CEO", "General", "1.0", false),
                ("General Manager", "General", "1.1", false),
                ("Financial Manager", "Financial", "1.2", false),
                ("Marketing Manager", "Marketing", "1.2", false),
                ("Operation Manager", "Production", "1.2", false),
                ("IT Manager", "Engineering", "1.2", false),
                ("HR Manager", "General", "1.1", false),
                ("Team Leader", "", "1.3", true),
                ("Operation Staff", "Production", "1.4", false),
                ("Financial Team", "Financial", "1.4", false),
                ("Sales Team", "Marketing", "1.4", false),
                ("IT Engineers", "Engineering", "1.4", false),
                ("Employee", "", "1.5", true)
            }
            : new List<(string Name, string Dept, string Hierarchy, bool AllDept)>
            {
                ("Owner", "General", "1.0", false),
                ("Member", "General", "1.1", false)
            };

        var ownerRoleId = 0;

        foreach (var (name, dept, hCode, all) in roleList)
        {
            if (all)
            {
                foreach (var (deptName, deptId) in deptMap)
                {
                    if (deptName == "General") continue;

                    var roleId = await IdGenerator.GenerateUniqueIdAsync<Role>(_context, r => r.RoleId);
                    _context.Roles.Add(new Role
                    {
                        RoleId = roleId,
                        Name = name,
                        DepartmentId = deptId,
                        HierarchyId = hierarchyMap[hCode],
                        AccessPage = pageIdList,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }
            else
            {
                var roleId = await IdGenerator.GenerateUniqueIdAsync<Role>(_context, r => r.RoleId);
                _context.Roles.Add(new Role
                {
                    RoleId = roleId,
                    Name = name,
                    DepartmentId = deptMap[dept],
                    HierarchyId = hierarchyMap[hCode],
                    AccessPage = pageIdList,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });

                if (name == "CEO" || name == "Owner")
                    ownerRoleId = roleId;
            }
        }

        await _context.SaveChangesAsync();
        result.OwnerRoleId = ownerRoleId;

        return result;
    }
}
