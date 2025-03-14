using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matrimony.Models
{
    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingRequests { get; set; }
        public int PendingInterests { get; set; }
        public int AcceptedInterests { get; set; }
        public int RejectedInterests { get; set; }
        public int RecentlyViewed { get; set; }
        public int CanceledProfiles { get; set; }
        public List<int> MostViewedProfiles { get; set; }
        public int ViewerId { get; set; }
        public int MaleUsers { get; set; }
        public int FemaleUsers { get; set; }
    }
}