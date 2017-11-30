using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hackathon.Factories;
using Hackathon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

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
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            if (Hasher.VerifyHashedPassword(loggingIn, loggingIn.Password, u.Password) != 0) {
                //if user level is an admin
                HttpContext.Session.SetInt32("AccessLevel", loggingIn.AccessLevelId);
                HttpContext.Session.SetInt32("UserId", loggingIn.UserId);
                HttpContext.Session.SetString("Username", loggingIn.FirstName);
                HttpContext.Session.SetInt32("CurrStackId", _userFactory.GetCurrentStackId(loggingIn.UserId));
                if (loggingIn.ChangePassword) {
                    return RedirectToAction("ShowPasswordUpdate");
                }

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

        public IActionResult ShowPasswordUpdate() {
            return View("UpdatePassword");
        }

        public IActionResult UpdatePassword(ChangePassword cp) {
            if (ModelState.IsValid) {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                User u = _userFactory.GetUserById((int)HttpContext.Session.GetInt32("UserId"));
                if (Hasher.VerifyHashedPassword(u, u.Password, cp.CurrentPassword) != 0)
                {
                    u.Password = Hasher.HashPassword(u, cp.NewPassword);
                    _userFactory.UpdateUserPassword(u);
                    if (HttpContext.Session.GetInt32("AccessLevel") == 9) {
                        return RedirectToAction("AdminNav", "Admin");
                    }
                    else {
                        return RedirectToAction("Index", "Team");
                    }
                }
                else {
                    TempData["mismatch"] = "Current password is incorrect";
                    return View();
                }
            }
            else {
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }

        public IActionResult Error()
        {
            ViewData["UserLevel"] = 0;
            return View();
        }
    }
}
