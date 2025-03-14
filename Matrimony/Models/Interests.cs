using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matrimony.Models
{
    public class Interests
    {
        public int InterestId { get; set; }
        public int InterestedProfileId { get; set; }
        public int SenderId { get; set; }
        public string InterestType { get; set; }
        public byte[] ProfileImage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string Occupation { get; set; }
        public string Religion { get; set; }
        public string InterestStatus { get; set; }
        public string MaritalStatus { get; set; }
    }
}