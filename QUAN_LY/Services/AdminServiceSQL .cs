using Microsoft.EntityFrameworkCore;
using QUAN_LY.Interfaces;
using QUAN_LY.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QUAN_LY.Services
{
    public class AdminServiceSQL : IAdminService
    {
        private readonly BookStoreDbContext _context;

      
        public AdminServiceSQL(BookStoreDbContext context)
        {
            _context = context;
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

        public async Task<(bool Success, string Message)> AddAdminAsync(Admin admin)
        {
            try
            {
                if (await IsUsernameExistsAsync(admin.Username))
                    return (false, "Tên đăng nhập đã tồn tại");

              
                if (!string.IsNullOrEmpty(admin.Password))
                {
                   
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                }

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();
                return (true, "Thêm nhân viên thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateAdminAsync(Admin admin)
        {
            try
            {
                var existingAdmin = await _context.Admins.FindAsync(admin.AdminId);
                if (existingAdmin == null) return (false, "Không tìm thấy nhân viên");

              
                existingAdmin.Name = admin.Name;
                existingAdmin.Role = admin.Role;
                existingAdmin.Username = admin.Username; 

               
                if (!string.IsNullOrEmpty(admin.Password))
                {
                    existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                }

                await _context.SaveChangesAsync();
                return (true, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeactivateAdminAsync(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null) return (false, "Không tìm thấy nhân viên");

                admin.IsActive = false;
                await _context.SaveChangesAsync();
                return (true, "Đã xóa nhân viên");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<List<Admin>> SearchAdminsAsync(string keyword)
        {
            var query = _context.Admins.Where(a => a.IsActive);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string k = keyword.ToLower();
                query = query.Where(a => a.Name.ToLower().Contains(k) || a.Username.ToLower().Contains(k));
            }
            return await query.ToListAsync();
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null)
        {
            return await _context.Admins.AnyAsync(a => a.Username == username && (excludeId == null || a.AdminId != excludeId));
        }

        public async Task<bool> VerifyPasswordAsync(int adminId, string password)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null) return false;
            return BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
        }

        
        public async Task<Admin> LoginAsync(string username, string password)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == username && a.IsActive);
            if (admin == null) return null;

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
            return isPasswordCorrect ? admin : null;
        }
    }
}