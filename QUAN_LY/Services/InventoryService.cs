using QUAN_LY.Model;

using System;

using System.Collections.Generic;

using System.Linq;



namespace QUAN_LY.Services

{ 
    public class InventoryService
    {
        public bool ProcessImport(int adminId, int publisherId, decimal totalCost, List<ImportDetail> items)
        {
            using (var _context = new BookStoreDbContext())
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Tạo phiếu nhập
                        var receipt = new ImportReceipt
                        {
                           AdminId = adminId,
                           PublisherId = publisherId,
                            ImportDate = DateTime.Now,
                            TotalCost = totalCost,
                           Status = 1
                        };
                        _context.ImportReceipts.Add(receipt);
                        _context.SaveChanges();
                        // 2. Duyệt từng món hàng
                        foreach (var item in items)
                        {
                            item.importId = receipt.Id;
                            _context.ImportDetails.Add(item);
                           // LOGIC CỘNG KHO: Tìm sách và tăng số lượng
                            var book = _context.Books.Find(item.BookId);
                            if (book != null)
                            {
                                book.Quantity += item.quantity;
                            }
                        }
                        _context.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    catch                    {
                        transaction.Rollback();
                        return false;
                    }

                }

            }

        }

    }

}