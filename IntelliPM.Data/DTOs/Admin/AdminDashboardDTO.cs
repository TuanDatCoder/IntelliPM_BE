using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Admin
{
    public class AdminDashboardDTO
    {
        public decimal TotalRevenue { get; set; } // Tổng tiền order * 10%
        public int TotalUsers { get; set; } 
        public int TotalStores { get; set; }
    }
}
