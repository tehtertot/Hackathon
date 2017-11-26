using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hackathon.Factories;
using Hackathon.Models;
using Microsoft.AspNetCore.Http;

namespace Hackathon.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserFactory _userFactory;

        public HomeController(UserFactory uf) {
            _userFactory = uf;
        }

        public IActionResult Index()
        {
            ViewData["UserLevel"] = 0;
            return View();
        }

        public IActionResult ShowLogin()
        {
            ViewData["UserLevel"] = 0;
            return View("Login");
        }

        public IActionResult Login(User u)
        {
            //validate email and password
            ViewData["UserLevel"] = 0;
            User loggingIn = _userFactory.LoginUser(u);
            if (loggingIn == null) {
                return RedirectToAction("ShowLogin");
            }
            if (loggingIn.Password == u.Password) {
                //if user level is an admin
                HttpContext.Session.SetInt32("AccessLevel", loggingIn.AccessLevelId);
                HttpContext.Session.SetInt32("UserId", loggingIn.UserId);
                HttpContext.Session.SetString("Username", loggingIn.FirstName);
                HttpContext.Session.SetInt32("CurrStackId", _userFactory.GetCurrentStackId(loggingIn.UserId));
                if (loggingIn.AccessLevelId == 9) {
                    return RedirectToAction("AdminNav", "Admin");
                }
                //if user level is a student
                else {
                    return RedirectToAction("Index", "Team");
                }
            }
            return RedirectToAction("ShowLogin", "Home");
        }

        public IActionResult About()
        {
            ViewData["UserLevel"] = 0;
            //hackathon descriptor?

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["UserLevel"] = 0;
            
            return View();
        }

        public IActionResult Error()
        {
            ViewData["UserLevel"] = 0;
            return View();
        }
    }
}
