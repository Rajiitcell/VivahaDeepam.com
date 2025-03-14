using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matrimony.Models
{
    public class Announcements
    {
        public int AnnouncementId {  get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Height { get; set; }
        public string DateOfBirth { get; set; }
        public string Location { get; set; }
        public string Occupation { get; set; }
        public string Religion { get; set; }
        public string MaritalStatus { get; set; }
        public int UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime DateSent { get; set; }
    }
}