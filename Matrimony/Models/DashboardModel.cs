using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matrimony.Models
{
    public class DashboardModel
    {
        public List<Profile> MatchingProfiles { get; set; }
        public List<Profile> AllProfiles { get; set; }
    }
}