using Hackathon.Models;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Hackathon.Factories
{
    public class UserFactory : IFactory<User>
    {
        private readonly IOptions<MySqlOptions> MySqlConfig;

        public UserFactory(IOptions<MySqlOptions> config) {
            MySqlConfig = config;
        }

        internal IDbConnection Connection
        {
            get 
            {
                return new MySqlConnection(MySqlConfig.Value.ConnectionString);
            }
        }

        public User LoginUser(User u)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM users WHERE email = @email";
                dbConnection.Open();
                return dbConnection.Query<User>(query, new {email = u.Email}).FirstOrDefault();
            }
        }

        public int AddUser(User u)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "INSERT INTO users (FirstName, LastName, Email, Password, AccessLevelId) VALUES (@FirstName, @LastName, @Email, @Password, @AccessLevelId);";
                dbConnection.Open();
                dbConnection.Execute(query, u);
                return dbConnection.Query<int>("SELECT last_insert_id();").SingleOrDefault();
            }
        }

        public void AddStudent(Student s)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "INSERT INTO students (UserId, StartDate, EndDate, CurrStackId) VALUES (@UserId, @StartDate, @EndDate, @CurrStackId);";
                dbConnection.Open();
                dbConnection.Execute(query, s);
            }
        }

        public int GetCurrentStackId(int UserId) 
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT currStackId FROM students WHERE UserId = {UserId};";
                dbConnection.Open();
                return dbConnection.Query<int>(query).SingleOrDefault();
            }
        }

        public List<Student> AllStudents()
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM users WHERE AccessLevelId=1";
                dbConnection.Open();
                return dbConnection.Query<Student>(query).ToList();
            }
        }

        public IEnumerable<Student> GetFellowStudents(int UserId, int StackId, int CompId)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM users JOIN students ON users.UserId = students.UserId WHERE students.CurrStackId = {StackId} AND students.UserId != {UserId} AND students.UserId NOT IN (SELECT StudentId FROM teams JOIN studentTeams ON teams.TeamId = studentTeams.TeamId WHERE CompetitionId = {CompId}) ORDER BY users.FirstName;";
                dbConnection.Open();
                return dbConnection.Query<Student, User, Student>(query, (s, u) => { s.UserInfo = u; return s; }, splitOn: "UserId");
            }
        }
    }
}