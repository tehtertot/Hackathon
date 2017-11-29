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
                ViewBag.currentCompetitions = _compFactory.GetCurrentCompetitionsToJoin((int)HttpContext.Session.GetInt32("UserId"));
                IEnumerable<Competition> votingCompetitions = _compFactory.GetCurrentCompetitionsToVote((int)HttpContext.Session.GetInt32("UserId"));
                foreach (Competition c in votingCompetitions)
                {
                    c.StudentTeam = _compFactory.GetStudentTeam((int)HttpContext.Session.GetInt32("UserId"), c.CompetitionId);
                }
                ViewBag.votingCompetitions = votingCompetitions;
                return View();
            }
            else 
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
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

        [HttpPost]
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

        public IActionResult Edit(int teamId)
        {
            if (CheckUser())
            {
                Team team = _compFactory.GetTeam(teamId, (int)HttpContext.Session.GetInt32("UserId"));
                return View("EditTeam", team);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult EditTeam(Team t)
        {
            if (CheckUser())
            {
                Team userTeam = _compFactory.GetTeam(t.TeamId, (int)HttpContext.Session.GetInt32("UserId"));
                if (userTeam != null) 
                {
                    userTeam.TeamName = t.TeamName;
                    userTeam.ProjectTitle = t.ProjectTitle;
                    _compFactory.UpdateTeam(userTeam);
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
            if (CheckUser())
            {
                Competition c = _compFactory.GetCompetition(compId);
                //page displaying competition details, including teams and players
                //allowing 5 minutes per presentation
                if (c.End > DateTime.Now.AddMinutes(c.Teams.Count*5))
                {
                    ViewBag.message = "Voting will begin after presentations...";
                    return View("ShowCompetition", c);
                }

                //viewing for voting
                StudentCompetitionVote hasVoted = _compFactory.GetVote((int)HttpContext.Session.GetInt32("UserId"), compId);
                if (hasVoted == null)
                {
                    ViewBag.userTeamId = _compFactory.GetStudentTeamId((int)HttpContext.Session.GetInt32("UserId"), compId);
                    return View("Vote", c);
                }
                else
                {
                    ViewBag.message = "Your vote is in!";
                    return View("ShowCompetition", c);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult Vote(int team, int compId)
        {
            if (CheckUser()) {
                int studTeam = _compFactory.GetStudentTeamId((int)HttpContext.Session.GetInt32("UserId"), compId);
                if (team == studTeam) {
                    TempData["error"] = "Cannot vote for yourself!";
                    return RedirectToAction("ViewCompetition", new {compId = compId});
                }
                _compFactory.SaveVote((int)HttpContext.Session.GetInt32("UserId"), compId, team);

                return RedirectToAction("ViewCompetition", new {compId = compId});
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
