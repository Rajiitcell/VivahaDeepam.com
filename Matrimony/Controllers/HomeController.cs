using Matrimony.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Reflection;

namespace Matrimony.Controllers
{
    public class HomeController : Controller
    {
        //SqlConnection con = new SqlConnection("Uid=sa;Password=123;Initial Catalog=Matrimony;Data Source=DESKTOP-EPALA8D");
        
        private string databasePath;
        private string connectionString;
        private SqlConnection con;

        public HomeController()
        {
            databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "TamilTechDB.mdf");

            connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={databasePath};Integrated Security=True;Connect Timeout=30;";

            con = new SqlConnection(connectionString);
        }
        SqlCommand cmd = null;

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact(Contact contact)
        {
            if (contact.Name!=null)
            {
                con.Open();
                cmd = new SqlCommand("Insert Into ContactMessages(Name,Email,Message) Values('" + contact.Name + "','" + contact.Email + "','" + contact.Message + "')", con);
                cmd.ExecuteNonQuery();
                con.Close();
                TempData["SuccessMessage"] = "Your message has been sent successfully!";
                return RedirectToAction("Contact");
            }
            return View();
        }
       
        public ActionResult SuccessStories()
        {
            return View();
        }

        public ActionResult Registration(RegistrationModel registration)
        {
            if (registration.FirstName != null)
            {
                con.Open();
                cmd = new SqlCommand("Insert into UsersProfiles(FirstName,LastName,Email,PhoneNumber,Password,ProfileFor,Country,State,City,ApplicantNumber) Values('" + registration.FirstName + "','" + registration.LastName + "','" + registration.Email + "','" + registration.PhoneNumber + "','" + registration.Password + "','" + registration.ProfileFor + "','" + registration.Country + "','" + registration.State + "','" + registration.City + "','" + registration.ApplicantNumber + "')", con);
                cmd.ExecuteNonQuery();
                SendWelcomeEmail(registration.Email, registration.FirstName, registration.ApplicantNumber);
                con.Close();
                return RedirectToAction("LogIn");
            }
            return View();
        }
        public void SendWelcomeEmail(string userEmail, string firstName, string applicantNumber)
        {
            try
            {
                var fromAddress = new MailAddress("linganaikbhukya65@gmail.com", "Telugu Matrimony");
                var toAddress = new MailAddress(userEmail);
                const string fromPassword = "zbef sdfd lpjx woqh";
                const string subject = "Welcome to Tamil Tech Matrimony";
                string body = $"Dear {firstName},\n\nThank you for registering with us! We're excited to have you on board and wish you the best of luck in your journey.\n\nYour Applicant Number is: {applicantNumber}\n\nBest Regards,\n Tamil Tech Matrimony Team";
                
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine("SMTP Exception: " + smtpEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }

        public ActionResult LogIn(RegistrationModel registration)
        {
            ViewBag.ErrorMessage = null;

            if (Request.HttpMethod == "POST")
            {
                if (ModelState.IsValid)
                {
                    con.Open();

                    string passwordQuery = "SELECT * FROM UsersProfiles WHERE Email = @Email AND Password = @Password AND IsEnable = 'Enable'";
                    cmd = new SqlCommand(passwordQuery, con);
                    cmd.Parameters.AddWithValue("@Email", registration.Email ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Password", registration.Password ?? string.Empty);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Session["Id"] = reader["Id"];
                            Session["FirstName"] = reader["FirstName"];
                            Session["LastName"] = reader["LastName"];
                            Session["Email"] = reader["Email"];
                        }
                        reader.Close();
                        con.Close();
                        return RedirectToAction("Dashboard");
                    }
                    else
                    {
                        reader.Close();

                        string roleQuery = "SELECT RoleName FROM UserRoles WHERE RoleName = @RoleName AND Password = @Password";
                        cmd = new SqlCommand(roleQuery, con);
                        cmd.Parameters.AddWithValue("@RoleName", registration.Email ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Password", registration.Password ?? string.Empty);

                        SqlDataReader roleReader = cmd.ExecuteReader();

                        if (roleReader.HasRows)
                        {
                            roleReader.Read();
                            string roleName = roleReader["RoleName"].ToString();

                            if (roleName == "Admin")
                            {
                                roleReader.Close();
                                con.Close();
                                return RedirectToAction("AdminDashboard", "Admin");
                            }
                            else if (roleName == "Receptionist")
                            {
                                roleReader.Close();
                                con.Close();
                                return RedirectToAction("Receptionist");
                            }
                        }

                        ViewBag.ErrorMessage = "Invalid Credentials or account is Disabled.";
                    }
                }
            }

            return View();
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Please enter a valid email address.";
                return View();
            }

            con.Open();
            string query = "SELECT * FROM UsersProfiles WHERE Email = @Email";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Email", email);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                Random random = new Random();
                string resetCode = random.Next(100000, 999999).ToString();

                Session["ResetCode"] = resetCode;
                Session["ResetEmail"] = email;

                SendPasswordResetEmail(email, resetCode);

                reader.Close();
                con.Close();

                return RedirectToAction("ResetPassword");
            }
            else
            {
                reader.Close();
                con.Close();

                ViewBag.ErrorMessage = "Email not found. Please check and try again.";
                return View();
            }
        }
        public void SendPasswordResetEmail(string userEmail, string resetCode)
        {
            try
            {
                var fromAddress = new MailAddress("linganaikbhukya65@gmail.com", "Telugu Matrimony");
                var toAddress = new MailAddress(userEmail);
                const string fromPassword = "zbef sdfd lpjx woqh";
                const string subject = "Password Reset Code";
                string body = $"Dear user,\n\nYou have requested a password reset. Use the following code to reset your password: {resetCode}\n\nIf you did not request a password reset, please ignore this message.\n\nBest regards,\nYour App Name";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string enteredCode, string newPassword)
        {
            string storedCode = Session["ResetCode"] as string;
            string storedEmail = Session["ResetEmail"] as string;

            if (storedCode != null && enteredCode == storedCode)
            {
                con.Open();
                string updateQuery = "UPDATE UsersProfiles SET Password = @Password WHERE Email = @Email";
                cmd = new SqlCommand(updateQuery, con);
                cmd.Parameters.AddWithValue("@Password", newPassword);
                cmd.Parameters.AddWithValue("@Email", storedEmail);

                cmd.ExecuteNonQuery();
                con.Close();

                Session["ResetCode"] = null;
                Session["ResetEmail"] = null;

                TempData["SuccessMessage"] = "Your password has been reset successfully.";
                return RedirectToAction("LogIn");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid reset code.";
                return View();
            }
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("LogIn", "Home");
        }

        public ActionResult Dashboard()
        {
            List<Profile> profiles = new List<Profile>();
            var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

            if (loggedUserId == 0)
            {
                return RedirectToAction("LogIn");
            }

            string preferencesQuery = @"
            SELECT PreferredAgeRange, PreferredHeightRange, ReligionPreference, 
                   CastePreference, EducationPreference, LocationPreference
            FROM UsersProfiles 
            WHERE Id = @LoggedUserId";
            SqlCommand preferencesCmd = new SqlCommand(preferencesQuery, con);
            preferencesCmd.Parameters.AddWithValue("@LoggedUserId", loggedUserId);

            string preferredAgeRange = null, preferredHeightRange = null, religionPreference = null;
            string castePreference = null, educationPreference = null, locationPreference = null;

            con.Open();
            SqlDataReader preferencesReader = preferencesCmd.ExecuteReader();
            if (preferencesReader.Read())
            {
                preferredAgeRange = preferencesReader["PreferredAgeRange"].ToString();
                preferredHeightRange = preferencesReader["PreferredHeightRange"].ToString();
                religionPreference = preferencesReader["ReligionPreference"].ToString();
                castePreference = preferencesReader["CastePreference"].ToString();
                educationPreference = preferencesReader["EducationPreference"].ToString();
                locationPreference = preferencesReader["LocationPreference"].ToString();
            }
            con.Close();

            bool preferencesSet = !string.IsNullOrEmpty(preferredAgeRange) ||
                                  !string.IsNullOrEmpty(preferredHeightRange) ||
                                  !string.IsNullOrEmpty(religionPreference) ||
                                  !string.IsNullOrEmpty(castePreference) ||
                                  !string.IsNullOrEmpty(educationPreference) ||
                                  !string.IsNullOrEmpty(locationPreference);

            Session["UserPreferencesSet"] = preferencesSet;

            string loggedUserGender = null;

            string genderQuery = "SELECT Gender FROM UsersProfiles WHERE Id = @LoggedUserId";
            SqlCommand genderCmd = new SqlCommand(genderQuery, con);
            genderCmd.Parameters.AddWithValue("@LoggedUserId", loggedUserId);

            con.Open();
            SqlDataReader genderReader = genderCmd.ExecuteReader();
            if (genderReader.Read())
            {
                loggedUserGender = genderReader["Gender"].ToString();
            }
            con.Close();

            string query;
            if (string.IsNullOrEmpty(loggedUserGender))
            {
                
                query = @"
            SELECT Id, FirstName, LastName, Occupation, Height, ProfileImage, DateOfBirth, 
                   Religion, Caste, HighestEducation, Location, IsEnable, Status
            FROM UsersProfiles p
            WHERE p.Id != @LoggedUserId
            AND p.IsEnable = 'Enable'
            AND p.Status = 'Searching'
            AND p.Id NOT IN (SELECT ProfileId FROM CanceledProfiles WHERE UserId = @LoggedUserId)";
            }
            else
            {
               
                query = @"
            SELECT Id, FirstName, LastName, Occupation, Height, ProfileImage, DateOfBirth, 
                   Religion, Caste, HighestEducation, Location, IsEnable, Status
            FROM UsersProfiles p
            WHERE p.Id != @LoggedUserId
            AND p.Gender != @Gender
            AND p.IsEnable = 'Enable'
            AND p.Status = 'Searching'
            AND p.Id NOT IN (SELECT ProfileId FROM CanceledProfiles WHERE UserId = @LoggedUserId)";
            }

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@LoggedUserId", loggedUserId);
            cmd.Parameters.AddWithValue("@Gender", loggedUserGender);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string profileDateOfBirthStr = reader["DateOfBirth"].ToString();
                DateTime profileDateOfBirth;
                int age = 0;
                if (DateTime.TryParse(profileDateOfBirthStr, out profileDateOfBirth))
                {
                    age = DateTime.Now.Year - profileDateOfBirth.Year;
                    if (DateTime.Now < profileDateOfBirth.AddYears(age)) age--;
                }

                bool isPreferred = true;

                if (preferencesSet)
                {
                    if (!string.IsNullOrEmpty(preferredAgeRange))
                    {
                        var ageRange = preferredAgeRange.Split('-');
                        if (ageRange.Length == 2)
                        {
                            int minAge = int.Parse(ageRange[0]);
                            int maxAge = int.Parse(ageRange[1]);
                            if (age < minAge || age > maxAge)
                            {
                                isPreferred = false;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(preferredHeightRange))
                    {
                        var heightRange = preferredHeightRange.Split('-');
                        if (heightRange.Length == 2)
                        {
                            string minHeight = heightRange[0].Trim();
                            string maxHeight = heightRange[1].Trim();
                            string profileHeight = reader["Height"].ToString().Trim();

                            if (string.Compare(profileHeight, minHeight) < 0 || string.Compare(profileHeight, maxHeight) > 0)
                            {
                                isPreferred = false;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(religionPreference) && religionPreference != reader["Religion"].ToString())
                        isPreferred = false;

                    if (!string.IsNullOrEmpty(castePreference) && castePreference != reader["Caste"].ToString())
                        isPreferred = false;

                    if (!string.IsNullOrEmpty(educationPreference) && educationPreference != reader["HighestEducation"].ToString())
                        isPreferred = false;

                    if (!string.IsNullOrEmpty(locationPreference) && locationPreference.Trim().ToLower() != reader["Location"].ToString().Trim().ToLower())
                        isPreferred = false;
                }

                Profile profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Height = reader["Height"].ToString(),
                    ProfileImage = reader["ProfileImage"] as byte[],
                    Religion = reader["Religion"].ToString(),
                    Caste = reader["Caste"].ToString(),
                    HighestEducation = reader["HighestEducation"].ToString(),
                    Location = reader["Location"].ToString(),
                    DateOfBirth = profileDateOfBirthStr
                };

                profile.IsPreferred = isPreferred;

                if (isPreferred)
                    profiles.Insert(0, profile);
                else
                    profiles.Add(profile);
            }
            con.Close();

            ViewBag.PreferencesSet = preferencesSet;

            return View(profiles);
        }

        public ActionResult FullDetails(int id)
        {
            Profile profile = null;
            var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

            string query = "SELECT * FROM UsersProfiles WHERE Id = @Id";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Height = reader["Height"].ToString(),
                    DateOfBirth = reader["DateOfBirth"].ToString(),
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                };

                if (reader["ProfileImage"] != DBNull.Value)
                {
                    profile.ProfileImage = reader["ProfileImage"] as byte[];
                }
                if (reader["Image1"] != DBNull.Value)
                {
                    profile.image1 = reader["Image1"] as byte[];
                }

                if (reader["Image2"] != DBNull.Value)
                {
                    profile.image2 = reader["Image2"] as byte[];
                }

                if (reader["Image3"] != DBNull.Value)
                {
                    profile.image3 = reader["Image3"] as byte[];
                }

                if (reader["Image4"] != DBNull.Value)
                {
                    profile.image4 = reader["Image4"] as byte[];
                }
            }
            con.Close();

            if (profile == null)
            {
                return RedirectToAction("Error");
            }

            string checkViewQuery = "SELECT COUNT(*) FROM ViewedProfiles WHERE ViewerId = @ViewerId AND ViewedProfileId = @ViewedProfileId";
            SqlCommand checkViewCmd = new SqlCommand(checkViewQuery, con);
            checkViewCmd.Parameters.AddWithValue("@ViewerId", loggedUserId);
            checkViewCmd.Parameters.AddWithValue("@ViewedProfileId", id);

            con.Open();
            int viewRecordCount = Convert.ToInt32(checkViewCmd.ExecuteScalar());
            con.Close();

            if (viewRecordCount == 0)
            {
                string insertQuery = "INSERT INTO ViewedProfiles (ViewerId, ViewedProfileId, ViewDate) VALUES (@ViewerId, @ViewedProfileId, @ViewDate)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@ViewerId", loggedUserId);
                insertCmd.Parameters.AddWithValue("@ViewedProfileId", id);
                insertCmd.Parameters.AddWithValue("@ViewDate", DateTime.Now);

                con.Open();
                insertCmd.ExecuteNonQuery();
                con.Close();
                SendProfileViewedEmail(profile.Email, loggedUserId);
            }

            string checkInterestQuery = "SELECT InterestStatus FROM UserInterests WHERE UserId = @UserId AND InterestedProfileId = @ProfileId";
            SqlCommand checkInterestCmd = new SqlCommand(checkInterestQuery, con);
            checkInterestCmd.Parameters.AddWithValue("@UserId", loggedUserId);
            checkInterestCmd.Parameters.AddWithValue("@ProfileId", id);

            con.Open();
            string interestStatus = checkInterestCmd.ExecuteScalar()?.ToString(); 
            con.Close();

            if (string.IsNullOrEmpty(interestStatus))
            {
                ViewBag.InterestStatus = "Requested";
            }
            else
            {
                ViewBag.InterestStatus = interestStatus;
            }

            ViewBag.IsInterestSent = !string.IsNullOrEmpty(interestStatus);
            return View(profile);
        }

        public void SendProfileViewedEmail(string userEmail, int loggedUserId)
        {
            try
            {
                string userNameQuery = "SELECT FirstName, LastName, Email FROM UsersProfiles WHERE Id = @Id";
                SqlCommand nameCmd = new SqlCommand(userNameQuery, con);
                nameCmd.Parameters.AddWithValue("@Id", loggedUserId);

                con.Open();
                SqlDataReader reader = nameCmd.ExecuteReader();
                string firstName = string.Empty;
                string viewerEmail = string.Empty;

                if (reader.Read())
                {
                    firstName = reader["FirstName"].ToString();
                    viewerEmail = reader["Email"].ToString();
                }
                con.Close();

                var fromAddress = new MailAddress("linganaikbhukya65@gmail.com", "Tamil Tech Matrimony");
                var toAddressUser = new MailAddress(userEmail);
                const string fromPassword = "zbef sdfd lpjx woqh";
                const string subjectUser = "Your Profile Has Been Viewed";
                string bodyUser = $"Dear User,\n\nYour profile has been viewed by {firstName}.\n\nPlease log in to check your profile views and interactions.\n\n" +
                                  "Best Regards,\nTamil Tech Matrimony Team";

                const string subjectViewer = "You Have Viewed a Profile";
                string bodyViewer = $"Dear {firstName},\n\nYou have viewed a profile on Tamil Tech Matrimony.\n\n" +
                                    "Best Regards,\nTamil Tech Matrimony Team";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var messageUser = new MailMessage(fromAddress, toAddressUser)
                {
                    Subject = subjectUser,
                    Body = bodyUser
                })
                {
                    smtp.Send(messageUser);
                }

                var toAddressViewer = new MailAddress(viewerEmail);
                using (var messageViewer = new MailMessage(fromAddress, toAddressViewer)
                {
                    Subject = subjectViewer,
                    Body = bodyViewer
                })
                {
                    smtp.Send(messageViewer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }

        public Profile AddProfile()
        {
            var userId = Convert.ToInt32(Session["Id"]);
            Profile userProfile = null;
            string query = "SELECT * FROM UsersProfiles WHERE Id = @UserId";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    userProfile = new Profile
                    {
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        Height = reader["Height"].ToString(),
                        Weight = reader["Weight"].ToString(),
                        DateOfBirth = reader["DateOfBirth"].ToString(),
                        Location = reader["Location"].ToString(),
                        Occupation = reader["Occupation"].ToString(),
                        Religion = reader["Religion"].ToString(),
                        MaritalStatus = reader["MaritalStatus"].ToString(),
                        ProfileImage = reader["ProfileImage"] as byte[],
                        image1 = reader["image1"] as byte[],
                        image2 = reader["image2"] as byte[],
                        image3 = reader["image3"] as byte[],
                        image4 = reader["image4"] as byte[],
                        MotherTongue = reader["MotherTongue"].ToString(),
                        Caste = reader["Caste"].ToString(),
                        SubCaste = reader["SubCaste"].ToString(),
                        Gothram = reader["Gothram"].ToString(),
                        Star = reader["Star"].ToString(),
                        Rasi = reader["Rasi"].ToString(),
                        Address = reader["Address"].ToString(),
                        City = reader["City"].ToString(),
                        State = reader["State"].ToString(),
                        Country = reader["Country"].ToString(),
                        GuardianContact = reader["GuardianContact"].ToString(),
                        HighestEducation = reader["HighestEducation"].ToString(),
                        EducationDetails = reader["EducationDetails"].ToString(),
                        CompanyName = reader["CompanyName"].ToString(),
                        AnnualIncome = reader["AnnualIncome"].ToString(),
                        FatherOccupation = reader["FatherOccupation"].ToString(),
                        MotherOccupation = reader["MotherOccupation"].ToString(),
                        SiblingDetails = reader["SiblingDetails"].ToString(),
                        Smoking = reader["Smoking"].ToString(),
                        Drinking = reader["Drinking"].ToString(),
                        Hobbies = reader["Hobbies"].ToString(),
                        PreferredAgeRange = reader["PreferredAgeRange"].ToString(),
                        PreferredHeightRange = reader["PreferredHeightRange"].ToString(),
                        ReligionPreference = reader["ReligionPreference"].ToString(),
                        CastePreference = reader["CastePreference"].ToString(),
                        EducationPreference = reader["EducationPreference"].ToString(),
                        LocationPreference = reader["LocationPreference"].ToString()
                    };
                }
                con.Close();
            }
            return userProfile;
        }

        public byte[] ConvertBase64ToByteArray(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                return null;
            }

            if (base64String.Contains("data:image"))
            {
                int index = base64String.IndexOf("base64,", StringComparison.Ordinal);
                if (index >= 0)
                {
                    base64String = base64String.Substring(index + 7);
                }
            }

            try
            {
                return Convert.FromBase64String(base64String); 
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Debug.WriteLine("Base64 to byte[] conversion failed: " + ex.Message);
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProfile(Profile profile, string ProfileImageBase64, string image1Base64, string image2Base64, string image3Base64, string image4Base64)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("LogIn", "Home");
            }

            var userId = Convert.ToInt32(Session["Id"]);

            try
            {
                System.Diagnostics.Debug.WriteLine("Profile Image Base64: " + ProfileImageBase64);
                System.Diagnostics.Debug.WriteLine("Image 1 Base64: " + image1Base64);
                System.Diagnostics.Debug.WriteLine("Image 2 Base64: " + image2Base64);
                System.Diagnostics.Debug.WriteLine("Image 3 Base64: " + image3Base64);
                System.Diagnostics.Debug.WriteLine("Image 4 Base64: " + image4Base64);

                byte[] profileImageBytes = ConvertBase64ToByteArray(ProfileImageBase64);
                byte[] image1Bytes = ConvertBase64ToByteArray(image1Base64);
                byte[] image2Bytes = ConvertBase64ToByteArray(image2Base64);
                byte[] image3Bytes = ConvertBase64ToByteArray(image3Base64);
                byte[] image4Bytes = ConvertBase64ToByteArray(image4Base64);

                System.Diagnostics.Debug.WriteLine("Profile Image Bytes: " + (profileImageBytes != null ? profileImageBytes.Length.ToString() : "null"));
                System.Diagnostics.Debug.WriteLine("Image 1 Bytes: " + (image1Bytes != null ? image1Bytes.Length.ToString() : "null"));
                System.Diagnostics.Debug.WriteLine("Image 2 Bytes: " + (image2Bytes != null ? image2Bytes.Length.ToString() : "null"));
                System.Diagnostics.Debug.WriteLine("Image 3 Bytes: " + (image3Bytes != null ? image3Bytes.Length.ToString() : "null"));
                System.Diagnostics.Debug.WriteLine("Image 4 Bytes: " + (image4Bytes != null ? image4Bytes.Length.ToString() : "null"));

                string checkQuery = "SELECT COUNT(*) FROM UsersProfiles WHERE Id = @UserId";
                int profileExists = 0;

                using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    profileExists = (int)cmd.ExecuteScalar();
                    con.Close();
                }

                string query = string.Empty;

                if (profileExists > 0)
                {
                    query = "UPDATE UsersProfiles SET " +
                            "FirstName = @FirstName, " +
                            "LastName = @LastName, " +
                            "Email = @Email, " +
                            "PhoneNumber = @PhoneNumber, " +
                            "Gender = @Gender, " +
                            "Height = @Height, " +
                            "Weight = @Weight, " +
                            "DateOfBirth = @DateOfBirth, " +
                            "Location = @Location, " +
                            "Occupation = @Occupation, " +
                            "Religion = @Religion, " +
                            "MaritalStatus = @MaritalStatus, " +
                            "ProfileImage = @ProfileImage, " +
                            "image1 = @image1, " +
                            "image2 = @image2, " +
                            "image3 = @image3, " +
                            "image4 = @image4, " +
                            "MotherTongue = @MotherTongue, " +
                            "Caste = @Caste, " +
                            "SubCaste = @SubCaste, " +
                            "Gothram = @Gothram, " +
                            "Star = @Star, " +
                            "Rasi = @Rasi, " +
                            "Address = @Address, " +
                            "City = @City, " +
                            "State = @State, " +
                            "Country = @Country, " +
                            "GuardianContact = @GuardianContact, " +
                            "HighestEducation = @HighestEducation, " +
                            "EducationDetails = @EducationDetails, " +
                            "CompanyName = @CompanyName, " +
                            "AnnualIncome = @AnnualIncome, " +
                            "FatherOccupation = @FatherOccupation, " +
                            "MotherOccupation = @MotherOccupation, " +
                            "SiblingDetails = @SiblingDetails, " +
                            "Smoking = @Smoking, " +
                            "Drinking = @Drinking, " +
                            "Hobbies = @Hobbies, " +
                            "PreferredAgeRange = @PreferredAgeRange, " +
                            "PreferredHeightRange = @PreferredHeightRange, " +
                            "ReligionPreference = @ReligionPreference, " +
                            "CastePreference = @CastePreference, " +
                            "EducationPreference = @EducationPreference, " +
                            "LocationPreference = @LocationPreference " +
                            "WHERE Id = @UserId";
                }
                else
                {
                    query = "INSERT INTO UsersProfiles (FirstName, LastName, Email, PhoneNumber, Gender, Height, Weight, DateOfBirth, Location, Occupation, Religion, MaritalStatus, ProfileImage, image1, image2, image3, image4, MotherTongue, Caste, SubCaste, Gothram, Star, Rasi, Address, City, State, Country, GuardianContact, HighestEducation, EducationDetails, CompanyName, AnnualIncome, FatherOccupation, MotherOccupation, SiblingDetails, Smoking, Drinking, Hobbies, PreferredAgeRange, PreferredHeightRange, ReligionPreference, CastePreference, EducationPreference, LocationPreference) " +
                            "VALUES (@FirstName, @LastName, @Email, @PhoneNumber, @Gender, @Height, @Weight, @DateOfBirth, @Location, @Occupation, @Religion, @MaritalStatus, @ProfileImage, @image1, @image2, @image3, @image4, @MotherTongue, @Caste, @SubCaste, @Gothram, @Star, @Rasi, @Address, @City, @State, @Country, @GuardianContact, @HighestEducation, @EducationDetails, @CompanyName, @AnnualIncome, @FatherOccupation, @MotherOccupation, @SiblingDetails, @Smoking, @Drinking, @Hobbies, @PreferredAgeRange, @PreferredHeightRange, @ReligionPreference, @CastePreference, @EducationPreference, @LocationPreference)";
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FirstName", profile.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", profile.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", profile.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNumber", profile.PhoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender", profile.Gender ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Height", profile.Height ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Weight", profile.Weight ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DateOfBirth", profile.DateOfBirth ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", profile.Location ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Occupation", profile.Occupation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Religion", profile.Religion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaritalStatus", profile.MaritalStatus ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProfileImage", profileImageBytes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@image1", image1Bytes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@image2", image2Bytes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@image3", image3Bytes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@image4", image4Bytes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MotherTongue", profile.MotherTongue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Caste", profile.Caste ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SubCaste", profile.SubCaste ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gothram", profile.Gothram ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Star", profile.Star ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rasi", profile.Rasi ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", profile.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", profile.City ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", profile.State ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", profile.Country ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GuardianContact", profile.GuardianContact ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@HighestEducation", profile.HighestEducation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EducationDetails", profile.EducationDetails ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CompanyName", profile.CompanyName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AnnualIncome", profile.AnnualIncome ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FatherOccupation", profile.FatherOccupation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MotherOccupation", profile.MotherOccupation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SiblingDetails", profile.SiblingDetails ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Smoking", profile.Smoking ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Drinking", profile.Drinking ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Hobbies", profile.Hobbies ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PreferredAgeRange", profile.PreferredAgeRange ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PreferredHeightRange", profile.PreferredHeightRange ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReligionPreference", profile.ReligionPreference ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CastePreference", profile.CastePreference ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EducationPreference", profile.EducationPreference ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LocationPreference", profile.LocationPreference ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserId", userId);


                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                SendProfileUpdateEmail(profile.Email, profile.FirstName);
                return Json(new { redirectUrl = Url.Action("ViewProfile", "Home") });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);

                ViewBag.ErrorMessage = "An error occurred while saving the profile. Please try again.";
                return PartialView("_AddProfilePartial", profile);
            }
        }

        
        public void SendProfileUpdateEmail(string userEmail, string firstName)
        {
            try
            {
                var fromAddress = new MailAddress("linganaikbhukya65@gmail.com", "Tamil Tech Matrimony");
                var toAddress = new MailAddress(userEmail);
                const string fromPassword = "zbef sdfd lpjx woqh";
                const string subject = "Your Profile Has Been Updated";
                string body = $"<p>Dear {firstName},</p>" +
                              "<p>Your profile has been successfully updated.</p>" +
                              "<p>Thank you for keeping your information up-to-date!</p>" +
                              "<p>Best regards,<br>Tamil Tech Matrimony Team</p>";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public ActionResult ViewProfile()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var userId = Convert.ToInt32(Session["Id"]);
            Profile profile = null;

            string query = "SELECT * FROM UsersProfiles WHERE Id = @UserId";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    profile = new Profile
                    {
                        Id = (int)reader["Id"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        Height = reader["Height"].ToString(),
                        DateOfBirth = reader["DateOfBirth"].ToString(),
                        Location = reader["Location"].ToString(),
                        Occupation = reader["Occupation"].ToString(),
                        Religion = reader["Religion"].ToString(),
                        MaritalStatus = reader["MaritalStatus"].ToString(),

                    };

                    if (reader["ProfileImage"] != DBNull.Value)
                    {
                        profile.ProfileImage = reader["ProfileImage"] as byte[];
                        Session["ProfileImage"] = profile.ProfileImageBase64;
                    }

                    if (reader["Image1"] != DBNull.Value)
                    {
                        profile.image1 = reader["Image1"] as byte[];
                    }

                    if (reader["Image2"] != DBNull.Value)
                    {
                        profile.image2 = reader["Image2"] as byte[];
                    }

                    if (reader["Image3"] != DBNull.Value)
                    {
                        profile.image3 = reader["Image3"] as byte[];
                    }

                    if (reader["Image4"] != DBNull.Value)
                    {
                        profile.image4 = reader["Image4"] as byte[];
                    }
                }

                con.Close();
            }

            if (profile == null)
            {
                ViewBag.IsProfileEmpty = true;
                return View();
            }

            ViewBag.IsProfileEmpty = false;
            profile=AddProfile();
            return View(profile);
        }

        public ActionResult SendInterest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendInterest(int ProfileId, string interestType)
        {
            var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

            string checkQuery = "SELECT COUNT(*) FROM UserInterests WHERE UserId = @UserId AND InterestedProfileId = @ProfileId";
            SqlCommand checkCmd = new SqlCommand(checkQuery, con);
            checkCmd.Parameters.AddWithValue("@UserId", loggedUserId);
            checkCmd.Parameters.AddWithValue("@ProfileId", ProfileId);

            con.Open();
            int existingInterestCount = Convert.ToInt32(checkCmd.ExecuteScalar());
            con.Close();

            if (existingInterestCount > 0)
            {
                return Json(new { success = false, message = "Interest already sent." });
            }

            byte[] profileImageData = null;
            string imageQuery = "SELECT ProfileImage FROM UsersProfiles WHERE Id = @Id";
            SqlCommand imageCmd = new SqlCommand(imageQuery, con);
            imageCmd.Parameters.AddWithValue("@Id", ProfileId);

            con.Open();
            SqlDataReader reader = imageCmd.ExecuteReader();
            if (reader.Read() && reader["ProfileImage"] != DBNull.Value)
            {
                profileImageData = (byte[])reader["ProfileImage"];
            }
            con.Close();

            SqlParameter profileImageParam = new SqlParameter("@ProfileImage", System.Data.SqlDbType.VarBinary);
            if (profileImageData != null)
            {
                profileImageParam.Value = profileImageData;
            }
            else
            {
                profileImageParam.Value = DBNull.Value;
            }

            string query = "INSERT INTO UserInterests (UserId, InterestedProfileId, InterestType, InterestStatus) " +
                           "VALUES (@UserId, @ProfileId, @InterestType, 'Pending')";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);
            cmd.Parameters.AddWithValue("@ProfileId", ProfileId);
            cmd.Parameters.AddWithValue("@InterestType", interestType);
            cmd.Parameters.Add(profileImageParam);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            string profileEmailQuery = "SELECT Email FROM UsersProfiles WHERE Id = @Id";
            SqlCommand emailCmd = new SqlCommand(profileEmailQuery, con);
            emailCmd.Parameters.AddWithValue("@Id", ProfileId);

            con.Open();
            string profileEmail = emailCmd.ExecuteScalar()?.ToString();
            con.Close();

            if (!string.IsNullOrEmpty(profileEmail))
            {
                SendInterestEmail(profileEmail, loggedUserId, interestType);
            }

            return Json(new { success = true, message = "Interest sent successfully!" });
        }
        public void SendInterestEmail(string userEmail, int loggedUserId, string interestType)
        {
            try
            {
                string userNameQuery = "SELECT FirstName, LastName, Email FROM UsersProfiles WHERE Id = @Id";
                SqlCommand nameCmd = new SqlCommand(userNameQuery, con);
                nameCmd.Parameters.AddWithValue("@Id", loggedUserId);

                con.Open();
                SqlDataReader reader = nameCmd.ExecuteReader();
                string lastName = string.Empty;
                string senderEmail = string.Empty;

                if (reader.Read())
                {
                    lastName = reader["LastName"].ToString();
                    senderEmail = reader["Email"].ToString();
                }
                con.Close();

                var fromAddress = new MailAddress("linganaikbhukya65@gmail.com", "Tamil Tech Matrimony");
                var toAddressReceiver = new MailAddress(userEmail);
                const string fromPassword = "zbef sdfd lpjx woqh";
                const string subjectReceiver = "You Have Received an Interest Request";
                string bodyReceiver = $"Dear User,\n\nYou have received an interest from {lastName}.\n\nInterest Type: {interestType}\n\n" +
                                      "Please visit our website to view and respond to this interest.\n\nBest Regards,\n Tamil Tech Matrimony Team \n http://axtsoft.org";

                const string subjectSender = "You Have Sent an Interest";
                string bodySender = $"Dear {lastName},\n\nYou have successfully sent an interest on a profile.\n\n" +
                                    "Best Regards,\nTamil Tech Matrimony Team";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var messageReceiver = new MailMessage(fromAddress, toAddressReceiver)
                {
                    Subject = subjectReceiver,
                    Body = bodyReceiver
                })
                {
                    smtp.Send(messageReceiver);
                }

                var toAddressSender = new MailAddress(senderEmail);
                using (var messageSender = new MailMessage(fromAddress, toAddressSender)
                {
                    Subject = subjectSender,
                    Body = bodySender
                })
                {
                    smtp.Send(messageSender);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        public ActionResult YourInterests()
        {
            var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

            string query = @"
            SELECT ui.Id AS InterestId, ui.InterestedProfileId, ui.InterestType,ui.InterestStatus, up.ProfileImage, up.FirstName, up.LastName,
            up.Location, up.Occupation, up.Religion, up.MaritalStatus
            FROM UserInterests ui
            JOIN UsersProfiles up ON ui.InterestedProfileId = up.Id
            WHERE ui.UserId = @UserId";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);

            List<Interests> interests = new List<Interests>();
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var interest = new Interests
                {
                    InterestId = Convert.ToInt32(reader["InterestId"]),
                    InterestedProfileId = Convert.ToInt32(reader["InterestedProfileId"]),
                    InterestType = reader["InterestType"].ToString(),
                    InterestStatus = reader["InterestStatus"].ToString(),
                    ProfileImage = reader["ProfileImage"] != DBNull.Value ? (byte[])reader["ProfileImage"] : null,
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : null,
                    Occupation = reader["Occupation"] != DBNull.Value ? reader["Occupation"].ToString() : null,
                    Religion = reader["Religion"] != DBNull.Value ? reader["Religion"].ToString() : null,
                    MaritalStatus = reader["MaritalStatus"] != DBNull.Value ? reader["MaritalStatus"].ToString() : null,
                };
                interests.Add(interest);
            }
            con.Close();

            return View(interests);
        }
        public ActionResult InterestRequests()
        {
            var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

            string query = @"
            SELECT ui.Id AS InterestId, ui.UserId AS SenderId, ui.InterestedProfileId, ui.InterestType, ui.InterestStatus, 
            up.ProfileImage, up.FirstName, up.LastName, 
            up.Location, up.Occupation, up.Religion, up.MaritalStatus
            FROM UserInterests ui
            JOIN UsersProfiles up ON ui.UserId = up.Id
            WHERE ui.InterestedProfileId = @UserId"; 

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);

            List<Interests> interestRequests = new List<Interests>();
            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var interestRequest = new Interests
                {
                    InterestId = Convert.ToInt32(reader["InterestId"]),
                    SenderId = Convert.ToInt32(reader["SenderId"]),
                    InterestedProfileId = Convert.ToInt32(reader["InterestedProfileId"]),
                    InterestType = reader["InterestType"].ToString(),
                    InterestStatus = reader["InterestStatus"].ToString(),
                    ProfileImage = reader["ProfileImage"] != DBNull.Value ? (byte[])reader["ProfileImage"] : null,
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : null,
                    Occupation = reader["Occupation"] != DBNull.Value ? reader["Occupation"].ToString() : null,
                    Religion = reader["Religion"] != DBNull.Value ? reader["Religion"].ToString() : null,
                    MaritalStatus = reader["MaritalStatus"] != DBNull.Value ? reader["MaritalStatus"].ToString() : null,
                };
                interestRequests.Add(interestRequest);
            }
            con.Close();

            return View(interestRequests);
        }

        public ActionResult UpdateInterestRequestStatus()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UpdateInterestRequestStatus(int interestId, string intereststatus)
        {
            var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

            string profileEmailQuery = "SELECT Email FROM UsersProfiles WHERE Id = (SELECT UserId FROM UserInterests WHERE Id = @InterestId)";
            SqlCommand emailCmd = new SqlCommand(profileEmailQuery, con);
            emailCmd.Parameters.AddWithValue("@InterestId", interestId);

            con.Open();
            string userEmail = emailCmd.ExecuteScalar()?.ToString();
            con.Close();

            Console.WriteLine("Fetched Email: " + userEmail);

            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { success = false, message = "User email not found!" });
            }

            string query = "UPDATE UserInterests SET InterestStatus = @InterestStatus WHERE Id = @InterestId";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@InterestStatus", intereststatus);
                cmd.Parameters.AddWithValue("@InterestId", interestId);

                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                con.Close();

                if (rowsAffected > 0)
                {
                    string senderNameQuery = "SELECT LastName FROM UsersProfiles WHERE Id = (SELECT InterestedProfileId FROM UserInterests WHERE Id = @InterestId)";
                    SqlCommand nameCmd = new SqlCommand(senderNameQuery, con);
                    nameCmd.Parameters.AddWithValue("@InterestId", interestId);

                    con.Open();
                    string senderName = nameCmd.ExecuteScalar()?.ToString();
                    con.Close();

                    if (string.IsNullOrEmpty(senderName))
                    {
                        senderName = "Someone";
                    }

                    bool emailSent = SendEmail(userEmail, intereststatus, senderName);

                    return Json(new { success = true, message = "Interest request status updated successfully!", emailSent = emailSent });
                }
                else
                {
                    return Json(new { success = false, message = "Error updating status." });
                }
            }
        }
        private bool SendEmail(string toEmail, string status, string senderName)
        {
            try
            {
                if (string.IsNullOrEmpty(toEmail))
                {
                    Console.WriteLine("Error: Recipient email is null or empty.");
                    return false;
                }

                string fromEmail = "linganaikbhukya65@gmail.com";
                string password = "zbef sdfd lpjx woqh";
                string subject = "Interest Request Status Updated";

                string action = status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ? "accepted" : "rejected";
                string bodyForRecipient = $"{senderName} has {action} your interest request.";
                string bodyForSender = $"You have {action} the interest request from {toEmail}.";

                MailMessage mailToRecipient = new MailMessage();
                mailToRecipient.From = new MailAddress(fromEmail);
                mailToRecipient.To.Add(toEmail);
                mailToRecipient.Subject = subject;
                mailToRecipient.Body = bodyForRecipient;
                mailToRecipient.IsBodyHtml = true;

                MailMessage mailToSender = new MailMessage();
                mailToSender.From = new MailAddress(fromEmail);
                mailToSender.To.Add(fromEmail);
                mailToSender.Subject = subject;
                mailToSender.Body = bodyForSender;
                mailToSender.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(fromEmail, password);
                smtp.EnableSsl = true;

                Console.WriteLine($"Sending email to {toEmail} (recipient) and {fromEmail} (sender)...");
                smtp.Send(mailToRecipient);
                smtp.Send(mailToSender);
                Console.WriteLine("Emails sent successfully!");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email error: " + ex.Message);
                return false;
            }
        }

        public ActionResult Matches()
        {
            List<Profile> profiles = new List<Profile>();
            var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

            string genderQuery = "SELECT Gender FROM UsersProfiles WHERE Id = @Id";
            SqlCommand genderCmd = new SqlCommand(genderQuery, con);
            genderCmd.Parameters.AddWithValue("@Id", loggedUserId);

            con.Open();
            string loggedUserGender = genderCmd.ExecuteScalar()?.ToString();
            con.Close();

            if (loggedUserId == 0)
            {
                return RedirectToAction("LogIn");
            }

            string query = @"
            SELECT * FROM UsersProfiles p
            WHERE p.Id != @Id 
            AND p.IsEnable = 'Enable'
            AND p.Status = 'Searching'
            AND p.Id NOT IN (SELECT ProfileId FROM CanceledProfiles WHERE UserId = @UserId)";

            if (!string.IsNullOrEmpty(loggedUserGender))
            {
                query += " AND p.Gender != @Gender";
            }

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", loggedUserId);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);

            if (!string.IsNullOrEmpty(loggedUserGender))
            {
                cmd.Parameters.AddWithValue("@Gender", loggedUserGender);
            }

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Profile profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Height = reader["Height"].ToString(),
                    DateOfBirth = reader["DateOfBirth"].ToString(),
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                };

                if (reader["ProfileImage"] != DBNull.Value)
                {
                    profile.ProfileImage = reader["ProfileImage"] as byte[];
                }

                profiles.Add(profile);
            }

            con.Close();

            foreach (var profile in profiles)
            {
                string checkQuery = "SELECT InterestStatus FROM UserInterests WHERE UserId = @UserId AND InterestedProfileId = @ProfileId";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@UserId", loggedUserId);
                checkCmd.Parameters.AddWithValue("@ProfileId", profile.Id);

                con.Open();
                var result = checkCmd.ExecuteScalar()?.ToString();
                con.Close();

                if (result == "Pending")
                {
                    profile.InterestStatus = "Requested";
                }
                else if (result == "Accepted")
                {
                    profile.InterestStatus = "Accepted";
                }
                else if (result == "Rejected")
                {
                    profile.InterestStatus = "Rejected";
                }
                else
                {
                    profile.InterestStatus = "Not Sent";
                }
            }

            return View(profiles);
        }
        public ActionResult CancelInterest(int profileId)
        {
            var loggedUserId = Convert.ToInt32(Session["Id"]);
            string query = "INSERT INTO CanceledProfiles (UserId, ProfileId) VALUES (@UserId, @ProfileId)";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);
            cmd.Parameters.AddWithValue("@ProfileId", profileId);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("Matches");
        }
        public ActionResult IgnoredProfiles()
        {
            var loggedUserId = Convert.ToInt32(Session["Id"]);
            List<Profile> ignoredProfiles = new List<Profile>();

            string query = @"
            SELECT p.*
            FROM UsersProfiles p
            INNER JOIN CanceledProfiles c ON p.Id = c.ProfileId
            WHERE c.UserId = @UserId";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Profile profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Height = reader["Height"].ToString(),
                    DateOfBirth = reader["DateOfBirth"].ToString(),
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                };

                if (reader["ProfileImage"] != DBNull.Value)
                {
                    profile.ProfileImage = reader["ProfileImage"] as byte[];
                }

                ignoredProfiles.Add(profile);
            }

            con.Close();

            return View(ignoredProfiles);
        }
        public ActionResult RemoveFromIgnored(int profileId)
        {
            var loggedUserId = Convert.ToInt32(Session["Id"]);

            string queryCheck = "SELECT COUNT(*) FROM CanceledProfiles WHERE UserId = @UserId AND ProfileId = @ProfileId";
            SqlCommand cmdCheck = new SqlCommand(queryCheck, con);
            cmdCheck.Parameters.AddWithValue("@UserId", loggedUserId);
            cmdCheck.Parameters.AddWithValue("@ProfileId", profileId);

            con.Open();
            int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
            con.Close();

            if (count > 0)
            {
                string queryDelete = "DELETE FROM CanceledProfiles WHERE UserId = @UserId AND ProfileId = @ProfileId";
                SqlCommand cmdDelete = new SqlCommand(queryDelete, con);
                cmdDelete.Parameters.AddWithValue("@UserId", loggedUserId);
                cmdDelete.Parameters.AddWithValue("@ProfileId", profileId);

                con.Open();
                cmdDelete.ExecuteNonQuery();
                con.Close();
            }
            return RedirectToAction("IgnoredProfiles");
        }

        public ActionResult GetNotifications()
        {
            try
            {
                var loggedUserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0;

                if (loggedUserId == 0)
                {
                    return Json(new { success = false, message = "User not logged in." }, JsonRequestBehavior.AllowGet);
                }

                string query = @"
                SELECT vp.ViewerId, vp.ViewDate, up.FirstName, up.LastName, up.ProfileImage 
                FROM ViewedProfiles vp
                JOIN UsersProfiles up ON vp.ViewerId = up.Id
                WHERE vp.ViewedProfileId = @LoggedUserId
                ORDER BY vp.ViewDate DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@LoggedUserId", loggedUserId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<NotificationViewModel> notifications = new List<NotificationViewModel>();

                while (reader.Read())
                {
                    var viewDate = (DateTime)reader["ViewDate"];
                    var timeAgo = GetTimeAgo(viewDate);

                    byte[] profileImage = reader["ProfileImage"] as byte[];
                    string base64Image = profileImage != null ? Convert.ToBase64String(profileImage) : string.Empty;

                    notifications.Add(new NotificationViewModel
                    {
                        ViewerId = (int)reader["ViewerId"],
                        ViewerFirstName = reader["FirstName"].ToString(),
                        ViewerLastName = reader["LastName"].ToString(),
                        ViewerProfileImage = base64Image,
                        ViewDate = viewDate,
                        TimeAgo = timeAgo
                    });
                }

                con.Close();

                return Json(notifications, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetTimeAgo(DateTime viewDate)
        {
            var timeSpan = DateTime.Now - viewDate;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";
            if (timeSpan.TotalDays < 2)
                return "Yesterday";
            return $"{(int)timeSpan.TotalDays} day(s) ago";
        }
        public ActionResult AcceptedProfiles()
        {
            var loggedUserId = Convert.ToInt32(Session["Id"]);

            string query = @"
            SELECT p.Id, p.FirstName, p.LastName, p.ProfileFor, p.Gender, p.ProfileImage, 
                   p.Location, p.Occupation, p.Religion, p.MaritalStatus
            FROM UserInterests ui
            INNER JOIN UsersProfiles p ON ui.UserId = p.Id
            WHERE ui.InterestedProfileId = @InterestedProfileId AND ui.InterestStatus = 'Accepted'";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@InterestedProfileId", loggedUserId);

            List<Profile> acceptedProfiles = new List<Profile>();

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Profile profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                    ProfileImage = reader["ProfileImage"] as byte[]
                };
                acceptedProfiles.Add(profile);
            }
            con.Close();
            return View(acceptedProfiles);
        }

        public ActionResult RejectedProfiles()
        {
            var loggedUserId = Convert.ToInt32(Session["Id"]);

            string query = @"
            SELECT p.Id, p.FirstName, p.LastName, p.ProfileFor, p.Gender, p.ProfileImage, 
                   p.Location, p.Occupation, p.Religion, p.MaritalStatus
            FROM UserInterests ui
            INNER JOIN UsersProfiles p ON ui.UserId = p.Id
            WHERE ui.InterestedProfileId = @InterestedProfileId AND ui.InterestStatus = 'Rejected'";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@InterestedProfileId", loggedUserId);

            List<Profile> rejectedProfiles = new List<Profile>();

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Profile profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                    ProfileImage = reader["ProfileImage"] as byte[] 
                };

                rejectedProfiles.Add(profile);
            }
            con.Close();
            return View(rejectedProfiles);
        }
        public ActionResult AcceptedRequests()
        {
            var loggedUserId = Convert.ToInt32(Session["Id"]);

            string query = @"
            SELECT p.Id, p.FirstName, p.LastName, p.ProfileFor, p.Gender, p.ProfileImage, 
                   p.Location, p.Occupation, p.Religion, p.MaritalStatus
            FROM UserInterests ui
            INNER JOIN UsersProfiles p ON ui.InterestedProfileId = p.Id
            WHERE ui.UserId = @UserId AND ui.InterestStatus = 'Accepted'";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);

            List<Profile> acceptedProfiles = new List<Profile>();

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Profile profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                    ProfileImage = reader["ProfileImage"] as byte[]
                };
                acceptedProfiles.Add(profile);
            }
            con.Close();
            return View(acceptedProfiles);
        }

        public ActionResult RejectedRequests()
        {
            var loggedUserId = Convert.ToInt32(Session["Id"]);

            string query = @"
            SELECT p.Id, p.FirstName, p.LastName, p.ProfileFor, p.Gender, p.ProfileImage, 
                   p.Location, p.Occupation, p.Religion, p.MaritalStatus
            FROM UserInterests ui
            INNER JOIN UsersProfiles p ON ui.InterestedProfileId = p.Id
            WHERE ui.UserId = @UserId AND ui.InterestStatus = 'Rejected'";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@UserId", loggedUserId);

            List<Profile> rejectedProfiles = new List<Profile>();

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Profile profile = new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                    ProfileImage = reader["ProfileImage"] as byte[]
                };

                rejectedProfiles.Add(profile);
            }
            con.Close();
            return View(rejectedProfiles);
        }
    }
}
