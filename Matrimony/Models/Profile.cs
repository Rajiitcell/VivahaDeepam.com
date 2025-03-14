using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matrimony.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ViewerId { get; set; }
        public DateTime ViewDate { get; set; }
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
        public byte[] ProfileImage { get; set; }
        public string ProfileImageBase64
        {
            get
            {
                return ProfileImage != null ? Convert.ToBase64String(ProfileImage) : null;
            }
        }
        public byte[] image1 { get; set; }
        public string image1Base64
        {
            get
            {
                return image1 != null ? Convert.ToBase64String(image1) : null;
            }
        }
        public byte[] image2 { get; set; }
        public string image2Base64
        {
            get
            {
                return image2 != null ? Convert.ToBase64String(image2) : null;
            }
        }
        public byte[] image3 { get; set; }
        public string image3Base64
        {
            get
            {
                return image3 != null ? Convert.ToBase64String(image3) : null;
            }
        }
        public byte[] image4 { get; set; }
        public string image4Base64
        {
            get
            {
                return image4 != null ? Convert.ToBase64String(image4) : null;
            }
        }
        public bool IsInterestSent { get; set; }
        public string InterestStatus { get; set; }
        public string Weight { get; set; }
        public string MotherTongue { get; set; }
        public string Caste { get; set; }
        public string SubCaste { get; set; }
        public string Gothram { get; set; }
        public string Star { get; set; }
        public string Rasi { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string GuardianContact { get; set; }
        public string HighestEducation { get; set; }
        public string EducationDetails { get; set; }
        public string CompanyName { get; set; }
        public string AnnualIncome { get; set; }
        public string FatherOccupation { get; set; }
        public string MotherOccupation { get; set; }
        public string SiblingDetails { get; set; }
        public string Smoking { get; set; }
        public string Drinking { get; set; }
        public string Hobbies { get; set; }
        public string PreferredAgeRange { get; set; }
        public string PreferredHeightRange { get; set; }
        public string ReligionPreference { get; set; }
        public string CastePreference { get; set; }
        public string EducationPreference { get; set; }
        public string LocationPreference { get; set; }
        public int Age { get; set; }
        public List<Profile> MatchingProfiles { get; set; }
        public List<Profile> AllProfiles { get; set; }
        public bool IsPreferred { get; set; }
        public string Status { get; set; }
        public string IsEnable { get; set; }
        public string ApplicantNumber { get; set; }
    }
}