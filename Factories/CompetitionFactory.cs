using Hackathon.Models;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;

namespace Hackathon.Factories
{
    public class CompetitionFactory : IFactory<Competition>
    {
        private readonly IOptions<MySqlOptions> MySqlConfig;

        public CompetitionFactory(IOptions<MySqlOptions> config) {
            MySqlConfig = config;
        }

        internal IDbConnection Connection
        {
            get 
            {
                return new MySqlConnection(MySqlConfig.Value.ConnectionString);
            }
        }

        //************************************************//
        //**************** COMPETITIONS ******************//
        //************************************************//

        public int SaveCompetition(Competition c)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "INSERT INTO competitions (CompetitionName, MaxSize, Start, End, CompetitionTypeId) VALUES (@CompetitionName, @MaxSize, @Start, @End, @CompetitionTypeId);";
                dbConnection.Open();
                dbConnection.Execute(query, c);
                return dbConnection.Query<int>("SELECT last_insert_id();").SingleOrDefault();
            }
        }

        public List<Competition> AllCompetitions()
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM competitions";
                dbConnection.Open();
                return dbConnection.Query<Competition>(query).ToList();
            }
        }

        public IEnumerable<Competition> GetCurrentCompetitionsToJoin(int userId) 
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM competitions WHERE end > now() AND competitionId NOT IN (SELECT CompetitionId FROM teams JOIN studentTeams ON teams.TeamId = studentTeams.TeamId WHERE studentTeams.StudentId = {userId});";
                dbConnection.Open();
                return dbConnection.Query<Competition>(query);
            }
        }

        public IEnumerable<Competition> GetCurrentCompetitionsToVote(int userId) 
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT competitions.* FROM students JOIN monthlyLangStacks ON CurrStackId=monthlyLangStacks.MonthlyLangStackId JOIN monthlylangstackcompetitions ON monthlylangstacks.MonthlyLangStackId=monthlylangstackcompetitions.MonthlyLangStackId JOIN competitions ON monthlylangstackcompetitions.CompetitionId=competitions.CompetitionId WHERE students.UserId={userId};";
                // string query = $"SELECT * FROM competitions WHERE DATE_ADD(end, INTERVAL 1 HOUR) > now() AND competitionId NOT IN (SELECT CompetitionId FROM teams JOIN studentTeams ON teams.TeamId = studentTeams.TeamId WHERE studentTeams.StudentId = {userId});";
                dbConnection.Open();
                // return dbConnection.Query<Student, MonthlyLangStack, MonthlyLangStackCompetition, Competition, Competition>(query, (s, mls, mlsc, c) => { return c; }, splitOn: "CurrStackId, MonthlyLangStackId, CompetitionId, CompetitionId");
                return dbConnection.Query<Competition>(query);
            }
        }

        public Competition GetCompetition(int id)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM competitions where CompetitionId={id};";
                // string query = $"SELECT * FROM competitions JOIN teams ON competitions.CompetitionId = teams.CompetitionId JOIN studentteams ON teams.TeamId = studentteams.TeamId WHERE CompetitionId = {id}";
                dbConnection.Open();
                // return dbConnection.Query<Competition, Team, StudentTeam, Competition>(query, (c, t, st) => {c.Teams = t; return c; }).SingleOrDefault();
                Competition c = dbConnection.Query<Competition>(query).SingleOrDefault();
                string query2 = $"SELECT * FROM teams WHERE CompetitionId={c.CompetitionId};";
                c.Teams = dbConnection.Query<Team>(query2).ToList();
                foreach (Team t in c.Teams)
                {
                    string query3 = $"SELECT * FROM studentteams JOIN students ON studentteams.studentid = students.userid JOIN users ON students.userid = users.userid WHERE TeamId={t.TeamId};";
                    t.Students = dbConnection.Query<StudentTeam, Student, User, Student>(query3, (st, s, u) => { s.UserInfo = u; return s; }, splitOn:"UserId").ToList();
                }
                return c;
            }
        }

        public Competition GetCompetitionWinners(int id)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM competitions WHERE CompetitionId={id};";
                dbConnection.Open();
                Competition comp = dbConnection.Query<Competition>(query).SingleOrDefault();
                string query2 = $"SELECT * FROM teams WHERE teams.CompetitionId={id};";
                List<Team> Teams = dbConnection.Query<Team>(query2).ToList();
                foreach (Team t in Teams)
                {
                    string query3 = $"SELECT Count(*) FROM studentsCompetitionVotes WHERE TeamId = {t.TeamId};";
                    t.VotesScored = dbConnection.Query<int>(query3).SingleOrDefault();
                }
                Teams = Teams.OrderByDescending(t => t.VotesScored).ToList();
                comp.Teams = Teams;
                return comp;
            }
        }

        //************************************************//
        //******************* TEAMS **********************//
        //************************************************//

        public IEnumerable<Team> GetStudentTeams(int userId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM studentTeams JOIN teams ON studentTeams.TeamId = teams.TeamId JOIN competitions ON teams.CompetitionId = competitions.CompetitionId WHERE StudentId = {userId} AND DATE_ADD(competitions.end, INTERVAL 1 HOUR) > now();";
                dbConnection.Open();
                return dbConnection.Query<StudentTeam, Team, Competition, Team>(query, (st, t, c) => { t.Competition=c; return t; }, splitOn: "TeamId, CompetitionId");
            }
        }

        public Team GetTeam(int teamId, int userId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM teams JOIN studentteams ON studentteams.teamid=teams.teamid WHERE studentteams.studentid={userId} AND teams.teamid={teamId};";
                dbConnection.Open();
                return dbConnection.Query<Team, StudentTeam, Team>(query, (t, st) => { return t; }, splitOn: "TeamId").SingleOrDefault();
            }
        }

        public Team GetStudentTeam(int userId, int compId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT teams.* FROM studentteams JOIN teams ON studentteams.teamId = teams.TeamId WHERE studentteams.studentid={userId} AND teams.CompetitionId={compId};";
                dbConnection.Open();
                return dbConnection.Query<Team>(query).SingleOrDefault();
            }
        }

        public int GetStudentTeamId(int userId, int compId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT studentteams.TeamId FROM studentteams JOIN teams ON studentteams.teamId = teams.TeamId WHERE studentteams.studentid={userId} AND teams.CompetitionId={compId};";
                dbConnection.Open();
                return dbConnection.Query<int>(query).SingleOrDefault();
            }
        }

        //************************************************//
        //******************* VOTES **********************//
        //************************************************//

        public void SaveVote(int userId, int compId, int teamId) 
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"INSERT INTO studentsCompetitionVotes (UserId, CompetitionId, TeamId) VALUES ({userId}, {compId}, {teamId});";
                dbConnection.Open();
                dbConnection.Execute(query);
            }
        }

        public StudentCompetitionVote GetVote(int userId, int compId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM studentsCompetitionVotes WHERE UserId={userId} AND CompetitionId={compId};";
                dbConnection.Open();
                return dbConnection.Query<StudentCompetitionVote>(query).SingleOrDefault();
            }
        }


        public void SaveMonthlyLangComp(MonthlyLangStackCompetition m) {
            using (IDbConnection dbConnection = Connection) {
                string query = "INSERT INTO monthlyLangStackCompetitions (CompetitionId, MonthlyLangStackId) VALUES (@CompetitionId, @MonthlyLangStackId);";
                dbConnection.Open();
                dbConnection.Execute(query, m);
            }
        }

        public List<CompetitionType> AllCompetitionTypes() 
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM competitionTypes";
                dbConnection.Open();
                return dbConnection.Query<CompetitionType>(query).ToList();
            }
        }

        public int SaveTeam(Team t)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "INSERT INTO teams (TeamName, ProjectTitle, CompetitionId) VALUES (@TeamName, @ProjectTitle, @CompetitionId);";
                dbConnection.Open();
                dbConnection.Execute(query, t);
                return dbConnection.Query<int>("SELECT last_insert_id();").SingleOrDefault();
            }
        }

        public void UpdateTeam(Team t)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "UPDATE teams SET TeamName=@TeamName, ProjectTitle=@ProjectTitle WHERE TeamId=@TeamId;";
                dbConnection.Open();
                dbConnection.Execute(query, t);
            }
        }

        public void SaveStudentTeam(int teamId, int studentId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"INSERT INTO studentTeams (TeamId, StudentId) VALUES ({teamId}, {studentId});";
                dbConnection.Open();
                dbConnection.Execute(query);
            }
        }
    }
}