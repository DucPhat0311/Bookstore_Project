// QUAN_LY/Services/AdminServiceSQL.cs
using Microsoft.EntityFrameworkCore;
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QUAN_LY.Services
{
    public class AdminServiceSQL : IAdminService
    {
        private readonly BookStoreDbContext _context;

        public AdminServiceSQL()
        {
            _context = new BookStoreDbContext();
        }

        public async Task<List<Admin>> GetAllActiveAdminsAsync()
        {
            return await _context.Admins
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Admin> GetAdminByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

        public async Task<bool> AddAdminAsync(Admin admin)
        {
            if (await IsUsernameExistsAsync(admin.Username))
                return false;

            // Nên hash mật khẩu ở đây (dùng BCrypt hoặc tương tự)
            // Ví dụ: admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAdminAsync(Admin admin)
        {
            if (await IsUsernameExistsAsync(admin.Username, admin.AdminId))
                return false;

            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAdminAsync(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null || !admin.IsActive) return false;

            admin.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Admin>> SearchAdminsAsync(string keyword)
        {
            var query = _context.Admins.Where(a => a.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(a => a.Name.Contains(keyword) || a.Username.Contains(keyword));
            }

            return await query.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null)
        {
            return await _context.Admins
                .AnyAsync(a => a.Username == username && (excludeId == null || a.AdminId != excludeId));
        }
    }
}