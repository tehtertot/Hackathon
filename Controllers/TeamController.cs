using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hackathon.Factories;
using Hackathon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hackathon.Controllers
{
    public class TeamController : Controller
    {
        private readonly UserFactory _userFactory;
        private readonly CompetitionFactory _compFactory;
        private readonly StackFactory _stackFactory;

        public TeamController(UserFactory uf, CompetitionFactory cf, StackFactory sf) {
            _userFactory = uf;
            _compFactory = cf;
            _stackFactory = sf;
        }
        
        public IActionResult Index()
        {
            if (CheckUser())
            {
                ViewBag.currentCompetitions = _compFactory.GetCurrentCompetitions((int)HttpContext.Session.GetInt32("UserId"));
                ViewBag.currentTeams = _compFactory.GetStudentTeams((int)HttpContext.Session.GetInt32("UserId"));
                return View();
            }
            else 
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult MakeTeam(int competition)
        {
            if (CheckUser()) 
            {
                ViewBag.selectedComp = competition;
                Competition c = _compFactory.GetCompetition(competition);
                ViewBag.numOthers = c.MaxSize - 1;
                // query for students not yet on a team
                ViewBag.allStudents = _userFactory.GetFellowStudents((int)HttpContext.Session.GetInt32("UserId"), (int)HttpContext.Session.GetInt32("CurrStackId"), competition);
                return View("Team");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult RegisterTeam(string name, string title, List<int> members, int competition)
        {
            if (CheckUser()) 
            {
                // accept name, title, and members from form
                bool valid = true;
                if (name != null && title != null && (members.Count + 1) <= 3) {
                    if (name.Trim().Length == 0 || title.Trim().Length == 0) {
                        valid = false;
                    }
                }
                else
                {
                    valid = false;
                }
                if (valid) {
                    // create team
                    Team t = new Team {
                        TeamName = name,
                        ProjectTitle = title,
                        CompetitionId = competition
                    };
                    int tId = _compFactory.SaveTeam(t);
                    foreach (int m in members) {
                        _compFactory.SaveStudentTeam(tId, m);
                    }
                    _compFactory.SaveStudentTeam(tId, (int)HttpContext.Session.GetInt32("UserId"));
                }
                return RedirectToAction("Index", "Team");
            }
            else 
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult ViewCompetition(int compId) 
        {
            Competition c = _compFactory.GetCompetition(compId);
            //page displaying competition details, including teams and players
            //allowing 5 minutes per presentation
            if (c.End > DateTime.Now.AddMinutes(c.Teams.Count*5))
            {
                return View("ShowCompetition", c);
            }

            //viewing for voting
            ViewBag.userTeamId = _compFactory.GetStudentTeamId((int)HttpContext.Session.GetInt32("UserId"), compId);
            return View("Vote", c);
        }

        public IActionResult Vote(int team, int compId)
        {
            if (CheckUser()) {
                int studTeam = _compFactory.GetStudentTeamId((int)HttpContext.Session.GetInt32("UserId"), compId);
                if (team == studTeam) {
                    TempData["error"] = "Cannot vote for yourself!";
                    return RedirectToAction("ViewCompetition");
                }
                _compFactory.SaveVote((int)HttpContext.Session.GetInt32("UserId"), team, studTeam);

                return RedirectToAction("ViewCompetition");
            }
            else {
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
    }
}
