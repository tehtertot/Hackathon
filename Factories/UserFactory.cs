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

        public IEnumerable<User> GetAll()
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM users";
                dbConnection.Open();
                return dbConnection.Query<User>(query);
            }
        }

        public IEnumerable<Student> GetAllStudents()
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM students JOIN users ON students.UserId=users.UserId ORDER BY users.FirstName, users.LastName;";
                dbConnection.Open();
                return dbConnection.Query<Student>(query);
            }
        }

        public Student GetStudent(int id)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM students JOIN users ON students.UserId=users.UserId WHERE users.UserId={id};";
                dbConnection.Open();
                return dbConnection.Query<Student>(query).SingleOrDefault();
            }
        }

        public User GetUserById(int id)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = $"SELECT * FROM users WHERE UserId={id};";
                dbConnection.Open();
                return dbConnection.Query<User>(query).SingleOrDefault();
            }
        }

        public void UpdateUserPassword(User u) 
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "UPDATE users SET Password=@Password, ChangePassword=0 WHERE UserId=@UserId";
                dbConnection.Open();
                dbConnection.Execute(query, u);
            }
        }

        public void UpdateStudent(Student s)
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "INSERT INTO users SET FirstName=@FirstName, LastName=@LastName, Email=@Email, Password=@Password, ChangePassword=1;";
                string query2 = "INSERT INTO users SET StartDate=@StartDate, EndDate=@EndDate;";
                dbConnection.Open();
                dbConnection.Execute(query, s);
                dbConnection.Execute(query2, s);
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
                return dbConnection.Query<Student>(query);
            }
        }
    }
}