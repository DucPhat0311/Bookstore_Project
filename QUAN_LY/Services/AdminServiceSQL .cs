
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

        public async Task<(bool Success, string Message)> AddAdminAsync(Admin admin)
        {
            try
            {
                // Validate business rules
                if (await IsUsernameExistsAsync(admin.Username))
                    return (false, "Tên đăng nhập đã tồn tại");

                if (string.IsNullOrEmpty(admin.Password))
                    return (false, "Mật khẩu không được để trống");

                // Hash password
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                admin.Password = null; // Clear plain password

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
                if (await IsUsernameExistsAsync(admin.Username, admin.AdminId))
                    return (false, "Tên đăng nhập đã tồn tại");

                var existing = await _context.Admins.FindAsync(admin.AdminId);
                if (existing == null)
                    return (false, "Không tìm thấy nhân viên");

                // Keep password hash if password not changed
                if (!string.IsNullOrEmpty(admin.Password))
                {
                    existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.Password);
                }

                existing.Name = admin.Name;
                existing.Username = admin.Username;
                existing.Role = admin.Role;
                existing.IsActive = admin.IsActive;

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
                if (!admin.IsActive) return (false, "Nhân viên đã nghỉ việc");
                if (admin.Role == "Super Admin") return (false, "Không thể xóa Super Admin");

                admin.IsActive = false;
                await _context.SaveChangesAsync();
                return (true, "Xóa nhân viên thành công");
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
                var lowerKeyword = keyword.ToLower();
                query = query.Where(a =>
                    a.Name.ToLower().Contains(lowerKeyword) ||
                    a.Username.ToLower().Contains(lowerKeyword));
            }

            return await query.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null)
        {
            return await _context.Admins
                .AnyAsync(a => a.Username == username.ToLower() &&
                              (excludeId == null || a.AdminId != excludeId));
        }

        public async Task<bool> VerifyPasswordAsync(int adminId, string password)
        {
            var admin = await _context.Admins.FindAsync(adminId);
            if (admin == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
        }
    }
}