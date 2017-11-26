using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Hackathon.Models;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace Hackathon.Factories
{
    public class StackFactory : IFactory<MonthlyStack>
    {
        private readonly IOptions<MySqlOptions> MySqlConfig;

        public StackFactory(IOptions<MySqlOptions> config) {
            MySqlConfig = config;
        }

        internal IDbConnection Connection
        {
            get 
            {
                return new MySqlConnection(MySqlConfig.Value.ConnectionString);
            }
        }

        public List<MonthlyLangStack> AllCurrentStacks()
        {
            using (IDbConnection dbConnection = Connection) {
                string query = "SELECT * FROM monthlyLangStacks JOIN languages ON monthlyLangStacks.LanguageId = languages.LanguageId JOIN monthlyStacks ON monthlyLangStacks.MonthlyStackId = monthlyStacks.MonthlyStackId JOIN users ON monthlyLangStacks.InstructorId = users.UserId";
                dbConnection.Open();
                return dbConnection.Query<MonthlyLangStack, Language, MonthlyStack, User, MonthlyLangStack>(query, (mls, lang, ms, ins) => { mls.Language = lang; mls.Stack = ms; mls.Instructor = ins; return mls; }, splitOn: "LanguageId, MonthlyStackId, UserId").ToList();
            }
        }
    }
}