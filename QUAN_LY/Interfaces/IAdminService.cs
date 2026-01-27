
using QUAN_LY.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QUAN_LY.Interfaces
{
    public interface IAdminService
    {
        Task<List<Admin>> GetAllActiveAdminsAsync();
        Task<Admin> GetAdminByIdAsync(int id);
        Task<(bool Success, string Message)> AddAdminAsync(Admin admin);
        Task<(bool Success, string Message)> UpdateAdminAsync(Admin admin);
        Task<(bool Success, string Message)> DeactivateAdminAsync(int id);
        Task<List<Admin>> SearchAdminsAsync(string keyword);
        Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null);
        Task<bool> VerifyPasswordAsync(int adminId, string password);
        Task<Admin> LoginAsync(string username, string password);
    }
}