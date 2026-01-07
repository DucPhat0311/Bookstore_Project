using QUAN_LY.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace QUAN_LY_APP.Interfaces
{
    public interface IInventoryService
    {
        // Hàm xử lý toàn bộ quy trình nhập kho
        bool ProcessImport(int adminId, int publisherId, decimal totalCost, List<ImportDetail> items);
    }
}
