// QUAN_LY/Interfaces/IAdminService.cs
using QUAN_LY.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QUAN_LY.Interfaces
{
    public interface IAdminService
    {
        Task<List<Admin>> GetAllActiveAdminsAsync();
        Task<Admin> GetAdminByIdAsync(int id);
        Task<bool> AddAdminAsync(Admin admin);
        Task<bool> UpdateAdminAsync(Admin admin);
        Task<bool> DeactivateAdminAsync(int id); 
        Task<List<Admin>> SearchAdminsAsync(string keyword);
        Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null);
    }
}