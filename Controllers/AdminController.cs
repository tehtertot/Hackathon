using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hackathon.Factories;
using Hackathon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hackathon.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserFactory _userFactory;
        private readonly CompetitionFactory _compFactory;
        private readonly StackFactory _stackFactory;

        public AdminController(UserFactory uf, CompetitionFactory cf, StackFactory sf) {
            _userFactory = uf;
            _compFactory = cf;
            _stackFactory = sf;
        }

        public IActionResult AdminNav() {
            if (CheckUser() && (int)HttpContext.Session.GetInt32("AccessLevel") == 9)
            {
                ViewBag.AllMyCompetitions = _compFactory.GetMyCompetitions((int)HttpContext.Session.GetInt32("UserId"));
                return View("Admin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // /////////////// MANAGE STUDENTS //////////////////////////////////////////// //
        public IActionResult EditStudents() {
            if (CheckUser() && (int)HttpContext.Session.GetInt32("AccessLevel") == 9) 
            {
                IEnumerable<Student> allStudents = _userFactory.GetAllStudents();
                return View(allStudents);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult EditStudent(int id) {
            if (CheckUser() && (int)HttpContext.Session.GetInt32("AccessLevel") == 9) 
            {
                Student student = _userFactory.GetStudent(id);
                return View(student);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public void UpdateStudent(Student s) 
        {
            PasswordHasher<Student> Hasher = new PasswordHasher<Student>();
            s.Password = Hasher.HashPassword(s, s.Password);   
            Student fromDb = _userFactory.GetStudent(s.UserId);
            PropertyInfo[] properties = typeof(Student).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                if (p.GetValue(s) != p.GetValue(fromDb))
                {
                    p.SetValue(fromDb, p.GetValue(s));
                }
            }
            _userFactory.UpdateStudent(fromDb);
        }

        public IActionResult ShowAllStudents() {
            if (CheckUser() && (int)HttpContext.Session.GetInt32("AccessLevel") == 9) 
            {
                return View("AllStudents");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public void UploadStudents(IFormFile students) {
            Stream fileStream = students.OpenReadStream();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    string[] data = line.Split(',');
                    User u = new User {
                        FirstName = data[0],
                        LastName = data[1],
                        Email = data[2],
                        Password = data[3],
                        AccessLevelId = Convert.ToInt16(data[4])
                    };
                    int uId = _userFactory.AddUser(u);
                    
                    Student s = new Student {
                        UserId = uId,
                        StartDate = Convert.ToDateTime(data[5]),
                        CurrStackId = Convert.ToInt16(data[7])
                    };
                    if (data[6] != "") {
                        s.EndDate = Convert.ToDateTime(data[6]);
                    }
                    _userFactory.AddStudent(s);
                }
            }
        }

        // /////////////// MANAGE COMPETITIONS //////////////////////////////////////// //
        public IActionResult ShowCompetition(int compId) 
        {
            if (CheckUser() && (int)HttpContext.Session.GetInt32("AccessLevel") == 9)
            {
                Competition c = _compFactory.GetCompetitionWinners(compId);
                return View(c);
            }
            else
            {
                return View("Index", "Home");
            }
        }
        public IActionResult Competition()
        {
            if (CheckUser() && (int)HttpContext.Session.GetInt32("AccessLevel") == 9)
            {
                ViewBag.types = _compFactory.AllCompetitionTypes();
                ViewBag.stacks = _stackFactory.AllCurrentStacks();
                List<string> errors = HttpContext.Session.GetObjectFromJson<List<string>>("compErrors");
                if (errors == null) {
                    errors = new List<string>();
                }
                ViewBag.errors = errors;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult AddCompetition(string name, int type, int size, DateTime startdate, DateTime starttime, DateTime enddate, DateTime endtime, List<int> stacks) 
        {
            if (CheckUser() && (int)HttpContext.Session.GetInt32("AccessLevel") == 9)
            {
                HttpContext.Session.SetObjectAsJson("compErrors", new Dictionary<string, string>());
                //accept name, type, size, startdate, starttime, enddate, endtime
                DateTime start = startdate.Date.Add(starttime.TimeOfDay);
                DateTime end = enddate.Date.Add(endtime.TimeOfDay);
                
                Competition comp = new Competition{
                    CompetitionName = name,
                    CompetitionTypeId = type,
                    MaxSize = size,
                    Start = start,
                    End = end,
                };

                if (TryValidateModel(comp) && stacks.Count > 0) {
                    int compId = _compFactory.SaveCompetition(comp);
                    foreach (int s in stacks) {
                        MonthlyLangStackCompetition m = new MonthlyLangStackCompetition {
                            CompetitionId = compId,
                            MonthlyLangStackId = s
                        };
                        _compFactory.SaveMonthlyLangComp(m);
                    }
                    return RedirectToAction("AdminNav", "Admin");
                }
                else {
                    List<string> errors = new List<string>();
                    foreach (var v in ModelState.Values) {
                        if (v.Errors.Count > 0) {
                            errors.Add(v.Errors[0].ErrorMessage);
                        }
                    }
                    if (stacks.Count == 0) {
                        errors.Add("At least one stack must be selected");
                    }
                    HttpContext.Session.SetObjectAsJson("compErrors", errors);
                    return RedirectToAction("Competition", "Admin");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Error()
        {
            CheckUser();
            return View();
        }

        private bool CheckUser()
        {
            try {
                ViewData["UserLevel"] = (int)HttpContext.Session.GetInt32("AccessLevel");
                ViewData["UserId"] = (int)HttpContext.Session.GetInt32("UserId");
                ViewData["Username"] = HttpContext.Session.GetString("Username");
                return true;
            }
            catch {
                return false;
            }
        }

        // private void HashAllPasswords() {
        //     IEnumerable<User> allUsers = _userFactory.GetAll();
        //     PasswordHasher<User> Hasher = new PasswordHasher<User>();
        //     foreach (User u in allUsers) {
        //         u.Password = Hasher.HashPassword(u, u.Password);
        //         _userFactory.UpdateUser(u);
        //     }
        // }
    }
}