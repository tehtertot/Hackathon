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

        public List<Competition> AllCompetitions()
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM competitions";
                dbConnection.Open();
                return dbConnection.Query<Competition>(query).ToList();
            }
        }

        public IEnumerable<Competition> GetCurrentCompetitions(int userId) 
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM competitions WHERE end > now() AND competitionId NOT IN (SELECT CompetitionId FROM teams JOIN studentTeams ON teams.TeamId = studentTeams.TeamId WHERE studentTeams.StudentId = {userId});";
                dbConnection.Open();
                return dbConnection.Query<Competition>(query);
            }
        }

        public IEnumerable<Team> GetStudentTeams(int userId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM studentTeams JOIN teams ON studentTeams.TeamId = teams.TeamId JOIN competitions ON teams.CompetitionId = competitions.CompetitionId WHERE StudentId = {userId} AND competitions.end > now();";
                dbConnection.Open();
                return dbConnection.Query<StudentTeam, Team, Team>(query, (st, t) => {return t;}, splitOn: "TeamId");
            }
        }

        public Competition GetCompetition(int id)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM competitions WHERE CompetitionId = {id}";
                dbConnection.Open();
                return dbConnection.Query<Competition>(query).SingleOrDefault();
            }
        }

        public int SaveCompetition(Competition c)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "INSERT INTO competitions (CompetitionName, MaxSize, Start, End, CompetitionTypeId) VALUES (@CompetitionName, @MaxSize, @Start, @End, @CompetitionTypeId);";
                dbConnection.Open();
                dbConnection.Execute(query, c);
                return dbConnection.Query<int>("SELECT last_insert_id();").SingleOrDefault();
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