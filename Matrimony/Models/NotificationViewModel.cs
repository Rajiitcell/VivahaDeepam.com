using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matrimony.Models
{
    public class NotificationViewModel
    {
        public int ViewerId { get; set; }
        public string ViewerFirstName { get; set; }
        public string ViewerLastName { get; set; }
        public string ViewerProfileImage { get; set; }
        public DateTime ViewDate { get; set; }
        public string TimeAgo { get; set; }
    }

}