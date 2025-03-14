using Matrimony.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using System.IO;

namespace Matrimony.Controllers
{
    public class AdminController : Controller
    {
        //SqlConnection con = new SqlConnection("Uid=sa;Password=123;Initial Catalog=Matrimony;Data Source=DESKTOP-EPALA8D");

        private string databasePath;
        private string connectionString;
        private SqlConnection con;

        public AdminController()
        {
            databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "TamilTechDB.mdf");

            connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={databasePath};Integrated Security=True;Connect Timeout=30;";

            con = new SqlConnection(connectionString);
        }
        SqlCommand cmd = null;
        SqlDataReader reader = null;

        public ActionResult AdminDashboard()
        {
            var dashboardData = new AdminDashboardStats();

            string totalUsersQuery = "Select Count(*) From UsersProfiles";
            SqlCommand totalUsersCmd = new SqlCommand(totalUsersQuery, con);
            con.Open();
            dashboardData.TotalUsers = (int)totalUsersCmd.ExecuteScalar();
            con.Close();

            string pendingRequestsQuery = "Select Count(*) From UserInterests WHERE InterestStatus = 'pending'";
            SqlCommand pendingRequestsCmd = new SqlCommand(pendingRequestsQuery, con);
            con.Open();
            dashboardData.PendingRequests = (int)pendingRequestsCmd.ExecuteScalar();
            con.Close();

            string recentViewedQuery = "Select Count(*) From ViewedProfiles WHERE ViewDate >= DATEADD(DAY, -7, GETDATE())";
            SqlCommand recentViewedCmd = new SqlCommand(recentViewedQuery, con);
            con.Open();
            dashboardData.RecentlyViewed = (int)recentViewedCmd.ExecuteScalar();
            con.Close();

            string canceledProfilesQuery = "Select Count(*) From CanceledProfiles";
            SqlCommand canceledProfilesCmd = new SqlCommand(canceledProfilesQuery, con);
            con.Open();
            dashboardData.CanceledProfiles = (int)canceledProfilesCmd.ExecuteScalar();
            con.Close();

            string maleUsersQuery = "Select Count(*) From UsersProfiles WHERE Gender = 'Male'";
            SqlCommand maleUsersCmd = new SqlCommand(maleUsersQuery, con);
            con.Open();
            dashboardData.MaleUsers = (int)maleUsersCmd.ExecuteScalar();
            con.Close();

            string femaleUsersQuery = "Select Count(*) From UsersProfiles WHERE Gender = 'Female'";
            SqlCommand femaleUsersCmd = new SqlCommand(femaleUsersQuery, con);
            con.Open();
            dashboardData.FemaleUsers = (int)femaleUsersCmd.ExecuteScalar();
            con.Close();

            string pendingInterestsQuery = "Select Count(*) From UserInterests WHERE InterestStatus = 'pending'";
            SqlCommand pendingInterestsCmd = new SqlCommand(pendingInterestsQuery, con);
            con.Open();
            dashboardData.PendingInterests = (int)pendingInterestsCmd.ExecuteScalar();
            con.Close();

            string acceptedInterestsQuery = "Select Count(*) From UserInterests WHERE InterestStatus = 'accepted'";
            SqlCommand acceptedInterestsCmd = new SqlCommand(acceptedInterestsQuery, con);
            con.Open();
            dashboardData.AcceptedInterests = (int)acceptedInterestsCmd.ExecuteScalar();
            con.Close();

            string rejectedInterestsQuery = "Select Count(*) From UserInterests WHERE InterestStatus = 'rejected'";
            SqlCommand rejectedInterestsCmd = new SqlCommand(rejectedInterestsQuery, con);
            con.Open();
            dashboardData.RejectedInterests = (int)rejectedInterestsCmd.ExecuteScalar();
            con.Close();

            return View(dashboardData);
        }

        public ActionResult Profiles(int userId)
        {
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

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
                        Weight = reader["Weight"].ToString(),
                        DateOfBirth = reader["DateOfBirth"].ToString(),
                        Location = reader["Location"].ToString(),
                        Occupation = reader["Occupation"].ToString(),
                        Religion = reader["Religion"].ToString(),
                        MaritalStatus = reader["MaritalStatus"].ToString(),
                        ProfileImage = reader["ProfileImage"] as byte[],
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
                        LocationPreference = reader["LocationPreference"].ToString(),
                    };


                    if (reader["ProfileImage"] != DBNull.Value)
                    {
                        profile.ProfileImage = reader["ProfileImage"] as byte[];
                    }
                    else
                    {
                        profile.ProfileImage = null;
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
            return View(profile);
        }

        public ActionResult LogOut()
        {
            return RedirectToAction("LogIn","Home");
        }
        
        public ActionResult EditUsersForm(Profile profile, string returnList)
        {
            con.Open();
            cmd = new SqlCommand("Update UsersProfiles set FirstName='"+ profile.FirstName+ "',LastName='" + profile.LastName+ "',Email='" + profile.Email + "',PhoneNumber='" + profile.PhoneNumber + "',Gender='" + profile.Gender + "',Occupation='" + profile.Occupation + "',Location='" + profile.Location+ "',MaritalStatus='" + profile.MaritalStatus+ "',Status='" + profile.Status+ "',IsEnable='" + profile.IsEnable+ "' where Id='" + profile.Id+"' ", con);
            cmd.ExecuteNonQuery();
            con.Close();

            if (returnList!=null)
            {
                return RedirectToAction(returnList);
            }
            return View(profile);
        }

        public ActionResult AllUsers()
        {
            con.Open();
            List<Profile> profiles = new List<Profile>();
            cmd = new SqlCommand("Select * from UsersProfiles", con);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var profile = new Profile()
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? "-" : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? "-" : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "-" : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "-" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "-" : reader.GetString(reader.GetOrdinal("Gender")),
                    Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? "-" : reader.GetString(reader.GetOrdinal("Height")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "-" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
                    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? "-" : reader.GetString(reader.GetOrdinal("Location")),
                    Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? "-" : reader.GetString(reader.GetOrdinal("Occupation")),
                    Religion = reader.IsDBNull(reader.GetOrdinal("Religion")) ? "-" : reader.GetString(reader.GetOrdinal("Religion")),
                    MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MaritalStatus")) ? "-" : reader.GetString(reader.GetOrdinal("MaritalStatus")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "-" : reader.GetString(reader.GetOrdinal("Status")),
                    IsEnable = reader.IsDBNull(reader.GetOrdinal("IsEnable")) ? "-" : reader.GetString(reader.GetOrdinal("IsEnable")),
                    ApplicantNumber = reader.IsDBNull(reader.GetOrdinal("ApplicantNumber")) ? "-" : reader.GetString(reader.GetOrdinal("ApplicantNumber"))
                };
                profiles.Add(profile);
            }
            reader.Close();
            con.Close();
            return View(profiles);
        }

        public ActionResult EditAllUsers(int Id)
        {
            con.Open();
            cmd = new SqlCommand("Select * from UsersProfiles where Id ='" + Id + "' ", con);
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var profile = new Profile()
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? "-" : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? "-" : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "-" : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "-" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "-" : reader.GetString(reader.GetOrdinal("Gender")),
                    Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? "-" : reader.GetString(reader.GetOrdinal("Height")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "-" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
                    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? "-" : reader.GetString(reader.GetOrdinal("Location")),
                    Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? "-" : reader.GetString(reader.GetOrdinal("Occupation")),
                    Religion = reader.IsDBNull(reader.GetOrdinal("Religion")) ? "-" : reader.GetString(reader.GetOrdinal("Religion")),
                    MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MaritalStatus")) ? "-" : reader.GetString(reader.GetOrdinal("MaritalStatus")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "-" : reader.GetString(reader.GetOrdinal("Status")),
                    IsEnable = reader.IsDBNull(reader.GetOrdinal("IsEnable")) ? "-" : reader.GetString(reader.GetOrdinal("IsEnable"))
                };
                reader.Close();
                con.Close();
                return View("EditUsersForm", profile);
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteAllUsers(Profile profile)
        {
            con.Open();
            cmd = new SqlCommand("Delete from UsersProfiles where Id='" + profile.Id + "' ", con);
            cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("AllUsers");
        }

        public ActionResult FemaleUsers()
        {
            con.Open();
            List<Profile> profiles = new List<Profile>();
            cmd = new SqlCommand("Select * from UsersProfiles where Gender='Female' ", con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var profile = new Profile()
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? "-" : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? "-" : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "-" : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "-" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "-" : reader.GetString(reader.GetOrdinal("Gender")),
                    Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? "-" : reader.GetString(reader.GetOrdinal("Height")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "-" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
                    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? "-" : reader.GetString(reader.GetOrdinal("Location")),
                    Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? "-" : reader.GetString(reader.GetOrdinal("Occupation")),
                    Religion = reader.IsDBNull(reader.GetOrdinal("Religion")) ? "-" : reader.GetString(reader.GetOrdinal("Religion")),
                    MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MaritalStatus")) ? "-" : reader.GetString(reader.GetOrdinal("MaritalStatus")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "-" : reader.GetString(reader.GetOrdinal("Status")),
                    ApplicantNumber = reader.IsDBNull(reader.GetOrdinal("ApplicantNumber")) ? "-" : reader.GetString(reader.GetOrdinal("ApplicantNumber")),
                    IsEnable = reader.IsDBNull(reader.GetOrdinal("IsEnable")) ? "-" : reader.GetString(reader.GetOrdinal("IsEnable"))
                };
                profiles.Add(profile);
            }
            reader.Close();
            con.Close();
            return View(profiles);
        }

        public ActionResult EditFemaleUsers(int Id)
        {
            con.Open();
            cmd = new SqlCommand("Select * from UsersProfiles where Id ='" + Id + "' ", con);
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var profile = new Profile()
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? "-" : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? "-" : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "-" : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "-" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "-" : reader.GetString(reader.GetOrdinal("Gender")),
                    Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? "-" : reader.GetString(reader.GetOrdinal("Height")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "-" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
                    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? "-" : reader.GetString(reader.GetOrdinal("Location")),
                    Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? "-" : reader.GetString(reader.GetOrdinal("Occupation")),
                    Religion = reader.IsDBNull(reader.GetOrdinal("Religion")) ? "-" : reader.GetString(reader.GetOrdinal("Religion")),
                    MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MaritalStatus")) ? "-" : reader.GetString(reader.GetOrdinal("MaritalStatus")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "-" : reader.GetString(reader.GetOrdinal("Status")),
                    IsEnable = reader.IsDBNull(reader.GetOrdinal("IsEnable")) ? "-" : reader.GetString(reader.GetOrdinal("IsEnable"))

                };
                reader.Close();
                con.Close();
                return View("EditUsersForm", profile);
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteFemaleUsers(Profile profile)
        {
            con.Open();
            cmd = new SqlCommand("Delete from UsersProfiles where Id='" + profile.Id + "' ", con);
            cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("FemaleUsers");
        }

        public ActionResult MaleUsers()
        {
            con.Open();
            List<Profile> profiles = new List<Profile>();
            cmd = new SqlCommand("Select * from UsersProfiles where Gender='Male' ", con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var profile = new Profile()
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? "-" : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? "-" : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "-" : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "-" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "-" : reader.GetString(reader.GetOrdinal("Gender")),
                    Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? "-" : reader.GetString(reader.GetOrdinal("Height")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "-" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
                    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? "-" : reader.GetString(reader.GetOrdinal("Location")),
                    Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? "-" : reader.GetString(reader.GetOrdinal("Occupation")),
                    Religion = reader.IsDBNull(reader.GetOrdinal("Religion")) ? "-" : reader.GetString(reader.GetOrdinal("Religion")),
                    MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MaritalStatus")) ? "-" : reader.GetString(reader.GetOrdinal("MaritalStatus")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "-" : reader.GetString(reader.GetOrdinal("Status")),
                    ApplicantNumber = reader.IsDBNull(reader.GetOrdinal("ApplicantNumber")) ? "-" : reader.GetString(reader.GetOrdinal("ApplicantNumber")),
                    IsEnable = reader.IsDBNull(reader.GetOrdinal("IsEnable")) ? "-" : reader.GetString(reader.GetOrdinal("IsEnable"))
                };
                profiles.Add(profile);
            }
            reader.Close();
            con.Close();
            return View(profiles);
        }

        public ActionResult EditMaleUsers(int Id)
        {
            con.Open();
            cmd = new SqlCommand("Select * from UsersProfiles where Id ='" + Id + "' ", con);
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var profile = new Profile()
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? "-" : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? "-" : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "-" : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "-" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "-" : reader.GetString(reader.GetOrdinal("Gender")),
                    Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? "-" : reader.GetString(reader.GetOrdinal("Height")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "-" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
                    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? "-" : reader.GetString(reader.GetOrdinal("Location")),
                    Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? "-" : reader.GetString(reader.GetOrdinal("Occupation")),
                    Religion = reader.IsDBNull(reader.GetOrdinal("Religion")) ? "-" : reader.GetString(reader.GetOrdinal("Religion")),
                    MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MaritalStatus")) ? "-" : reader.GetString(reader.GetOrdinal("MaritalStatus")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "-" : reader.GetString(reader.GetOrdinal("Status")),
                    IsEnable = reader.IsDBNull(reader.GetOrdinal("IsEnable")) ? "-" : reader.GetString(reader.GetOrdinal("IsEnable"))

                };
                reader.Close();
                con.Close();
                return View("EditUsersForm", profile);
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteMaleUsers(Profile profile)
        {
            con.Open();
            cmd = new SqlCommand("Delete from UsersProfiles where Id='" + profile.Id + "' ", con);
            cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("MaleUsers");
        }

        public ActionResult AddRole(UserRoles role)
        {
            if (role.RoleName != null)
            {
                con.Open();
                var id = 0;
                cmd = new SqlCommand("select * from UserRoles where RoleId='" + role.RoleId + "' ", con);
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    id = reader.GetInt32(reader.GetOrdinal("RoleId"));
                }
                reader.Close();
                if (id == 0)
                {
                    cmd = new SqlCommand("insert into UserRoles Values ('" + role.RoleName + "','" + role.Password + "')", con);
                }
                else
                {
                    cmd = new SqlCommand("Update UserRoles set RoleName='" + role.RoleName + "', Password= '"+ role.Password +"' where RoleId='" + role.RoleId + "' ", con);
                }
                cmd.ExecuteNonQuery();
                con.Close();
                return RedirectToAction("UserRoles");
            }
            return View();
        }

        public ActionResult UserRoles(UserRoles role)
        {
            con.Open();
            List<UserRoles> roles = new List<UserRoles>();
            cmd = new SqlCommand("Select * from UserRoles", con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                role = new UserRoles();
                role.RoleId = reader.GetInt32(reader.GetOrdinal("RoleId"));
                role.RoleName = reader.GetString(reader.GetOrdinal("RoleName"));
                role.Password = reader.GetString(reader.GetOrdinal("Password"));
                roles.Add(role);
            }
            reader.Close();
            con.Close();
            return View(roles);
        }

        public ActionResult EditUserRoles(int RoleId)
        {
            con.Open();
            UserRoles role = new UserRoles();
            cmd = new SqlCommand("Select * from UserRoles where RoleId ='" + RoleId + "' ", con);
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                role.RoleId = reader.GetInt32(reader.GetOrdinal("RoleId"));
                role.RoleName = reader.GetString(reader.GetOrdinal("RoleName"));
                role.Password = reader.GetString(reader.GetOrdinal("Password"));
                reader.Close();
                con.Close();
                return View("AddRole", role);
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult DeleteUserRoles(UserRoles role)
        {
            con.Open();
            cmd = new SqlCommand("Delete from UserRoles where RoleId='" + role.RoleId + "' ", con);
            cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("UserRoles");
        }

        public ActionResult PendingRequests()
        {
            string query = @"
            SELECT p.Id, p.FirstName, p.LastName, p.ProfileFor, p.Gender, p.ProfileImage, 
                   p.Location, p.Occupation, p.Religion, p.MaritalStatus, ui.UserId
            FROM UserInterests ui
            INNER JOIN UsersProfiles p ON ui.InterestedProfileId = p.Id
            WHERE ui.InterestStatus = 'Pending'";

            SqlCommand cmd = new SqlCommand(query, con);

            List<Profile> pendingProfiles = new List<Profile>();

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
                    ProfileImage = reader["ProfileImage"] as byte[],
                    UserId = (int)reader["UserId"]
                };

                pendingProfiles.Add(profile);
            }
            con.Close();

            return View(pendingProfiles);
        }

        public ActionResult RecentlyViewed()
        {
            List<Profile> recentlyViewedProfiles = new List<Profile>();

            string recentlyViewedQuery = @"
            SELECT p.Id, p.FirstName, p.LastName, p.ProfileImage, p.Location, p.Occupation, p.Religion, p.MaritalStatus, vp.ViewDate, vp.ViewerId
            FROM ViewedProfiles vp
            JOIN UsersProfiles p ON vp.ViewedProfileId = p.Id
            WHERE vp.ViewDate >= DATEADD(DAY, -7, GETDATE())";

            SqlCommand recentlyViewedCmd = new SqlCommand(recentlyViewedQuery, con);

            con.Open();
            SqlDataReader reader = recentlyViewedCmd.ExecuteReader();

            while (reader.Read())
            {
                recentlyViewedProfiles.Add(new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    ProfileImage = reader["ProfileImage"] as byte[],
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                    ViewDate = (DateTime)reader["ViewDate"],
                    ViewerId = (int)reader["ViewerId"] 
                });
            }

            con.Close();

            return View(recentlyViewedProfiles);
        }

        public ActionResult AllCanceledProfiles()
        {
            List<Profile> canceledProfiles = new List<Profile>();

            string canceledProfilesQuery = @"
            SELECT p.Id, p.FirstName, p.LastName, p.ProfileImage, p.Location, p.Occupation, p.Religion, p.MaritalStatus, cp.UserId 
            FROM CanceledProfiles cp
            JOIN UsersProfiles p ON cp.ProfileId = p.Id";
            SqlCommand canceledProfilesCmd = new SqlCommand(canceledProfilesQuery, con);

            con.Open();
            SqlDataReader reader = canceledProfilesCmd.ExecuteReader();
            while (reader.Read())
            {
                canceledProfiles.Add(new Profile
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    ProfileImage = reader["ProfileImage"] as byte[],
                    Location = reader["Location"].ToString(),
                    Occupation = reader["Occupation"].ToString(),
                    Religion = reader["Religion"].ToString(),
                    MaritalStatus = reader["MaritalStatus"].ToString(),
                    UserId = (int)reader["UserId"]
                });
            }
            con.Close();
            return View(canceledProfiles);
        }
        public ActionResult Messages(Contact contact)
        {
            con.Open();
            List<Contact> contacts = new List<Contact>();
            cmd = new SqlCommand("Select*from ContactMessages", con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                contact = new Contact();
                contact.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                contact.Name = reader.GetString(reader.GetOrdinal("Name"));
                contact.Email = reader.GetString(reader.GetOrdinal("Email"));
                contact.Message = reader.GetString(reader.GetOrdinal("Message"));
                contact.CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
                contact.Status = reader.GetString(reader.GetOrdinal("Status"));
                contact.Resolution = reader.GetString(reader.GetOrdinal("Resolution"));
                contacts.Add(contact);
            }
            reader.Close();
            con.Close();
            return View(contacts);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int id, string resolution)
        {
            con.Open();
            string query = "UPDATE ContactMessages SET Status = 'Completed', Resolution = @Resolution WHERE Id = @Id";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Resolution", resolution);

            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("Messages");
        }

        public ActionResult DeleteMessage(Contact contact)
        {
            con.Open();
            cmd = new SqlCommand("Delete from ContactMessages where Id='" + contact.Id + "' ", con);
            cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("Messages");
        }
        public ActionResult Announcement(Announcements announcements)
        {
            if (announcements.Title != null)
            {
                con.Open();
                
                SqlCommand cmd = new SqlCommand("INSERT INTO Announcements(Title, Description) VALUES(@Title, @Description)", con);
                cmd.Parameters.AddWithValue("@Title", announcements.Title);
                cmd.Parameters.AddWithValue("@Description", announcements.Description);
                cmd.ExecuteNonQuery();
                con.Close();

                List<string> userEmails = GetUserEmails();

                if (userEmails.Count > 0)
                {
                    SendAnnouncementEmail(userEmails, announcements.Title, announcements.Description);
                }
                TempData["SuccessMessage"] = "Your announcement has been successfully sent!";
                return RedirectToAction("Announcement");
            }
            return View();
        }

        private List<string> GetUserEmails()
        {
            List<string> emails = new List<string>();

            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT Email FROM UsersProfiles where Status='Searching' ", con);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                emails.Add(reader["Email"].ToString());
            }

            con.Close();
            return emails;
        }

        public void SendAnnouncementEmail(List<string> userEmails, string title, string description)
        {
            try
            {
                var fromAddress = new MailAddress("linganaikbhukya65@gmail.com", "Telugu Matrimony");
                const string fromPassword = "zbef sdfd lpjx woqh"; 
                string subject = "New Announcement: " + title;
                string body = $"Dear User,\n\nWe have a new announcement for you:\n\n<b>{title}</b>\n\n{description}\n\nBest Regards,\n TamilTech Matrimony Team";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                foreach (var email in userEmails)
                {
                    var toAddress = new MailAddress(email);

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
        public ActionResult ToPerticularUsers()
        {
            con.Open();
            List<Announcements> announcements = new List<Announcements>();
            cmd = new SqlCommand("Select * from UsersProfiles where Status='Searching'", con);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var announcement = new Announcements()
                {
                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? "-" : reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? "-" : reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "-" : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? "-" : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? "-" : reader.GetString(reader.GetOrdinal("Gender")),
                    Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? "-" : reader.GetString(reader.GetOrdinal("Height")),
                    DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "-" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
                    Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? "-" : reader.GetString(reader.GetOrdinal("Location")),
                    Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? "-" : reader.GetString(reader.GetOrdinal("Occupation")),
                    Religion = reader.IsDBNull(reader.GetOrdinal("Religion")) ? "-" : reader.GetString(reader.GetOrdinal("Religion")),
                    MaritalStatus = reader.IsDBNull(reader.GetOrdinal("MaritalStatus")) ? "-" : reader.GetString(reader.GetOrdinal("MaritalStatus")),
                };
                announcements.Add(announcement);
            }
            reader.Close();
            con.Close();
            return View(announcements);
        }
        [HttpPost]
        public JsonResult SendNotification(Announcements announcement)
        {
            try
            {
                string userEmail = GetUserEmail(announcement.UserId);

                if (string.IsNullOrEmpty(userEmail))
                {
                    return Json(new { success = false, error = "User email not found." });
                }

                con.Open();
                var cmd = new SqlCommand("INSERT INTO UserNotifications (UserId, Subject, Message) VALUES (@UserId, @Subject, @Message)", con);
                cmd.Parameters.AddWithValue("@UserId", announcement.UserId);
                cmd.Parameters.AddWithValue("@Subject", announcement.Subject);
                cmd.Parameters.AddWithValue("@Message", announcement.Message);
                cmd.ExecuteNonQuery();
                con.Close();

                SendEmail(userEmail, announcement.Subject, announcement.Message);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string GetUserEmail(int userId)
        {
            string userEmail = null;
            try
            {
                con.Open();
                var cmd = new SqlCommand("SELECT Email FROM UsersProfiles WHERE Id = @UserId", con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    userEmail = reader["Email"].ToString();
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve user email: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
            return userEmail;
        }

        private void SendEmail(string toEmail, string subject, string message)
        {
            try
            {
                var fromAddress = new MailAddress("linganaikbhukya65@gmail.com", "Telugu Matrimony");
                const string fromPassword = "zbef sdfd lpjx woqh";
                var smtpClient = new SmtpClient("smtp.gmail.com") 
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    EnableSsl = true,
                };

                string emailBody = $@"
                <html>
                <body>
                    <h2>Dear User,</h2>
                    <p>We have a new announcement for you:</p>
                    <h3>{subject}</h3>
                    <p>{message}</p>
                    <p>Best Regards,</p>
                    <p><strong>TamilTech Matrimony Team</strong></p>
                </body>
                </html>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("linganaikbhukya65@gmail.com"),
                    Subject = subject,
                    Body = emailBody,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send email: " + ex.Message);
            }
        }
    }
}