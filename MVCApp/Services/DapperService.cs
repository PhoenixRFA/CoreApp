using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MVCApp.Services
{
    public class DapperService : IDapperService
    {
        private readonly string _connectionString;

        public DapperService(string conn)
        {
            _connectionString = conn;
        }

        public void Create(User user)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            //db.Execute("INSERT INTO Users (Name, Age, Sex, IsMarried) VALUES(@Name, @Age, @Sex, @IsMarried)", user);
            int id = db.Query<int>("INSERT INTO Users (Name, Age, Sex, IsMarried) VALUES(@Name, @Age, @Sex, @IsMarried); SELECT CAST(SCOPE_IDENTITY() AS INT)", user).FirstOrDefault();
            user.Id = id;
        }

        public void Delete(int id)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            db.Execute("DELETE Users WHERE Id = @id", new {id});
        }

        public User Get(int id)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var res1 = db.Query<User>("SELECT * FROM Users WHERE Id = @id", new {id}).FirstOrDefault();
            var res2 = db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @id", new {id});

            return res1;
        }

        public List<User> GetUsers()
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            
            return db.Query<User>("SELECT * FROM Users").ToList();
        }

        public void Update(User user)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            db.Execute("UPDATE Users SET Name = @Name, Age = @Age, Sex = @Sex, IsMarried = @IsMarried WHERE Id = @Id", user);
        }

        public bool GetReturn(bool input)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters(new {input});
            //parameters.Add("input", input, DbType.Boolean, ParameterDirection.Input);
            parameters.Add("foo", null, direction: ParameterDirection.ReturnValue);

            db.Execute("ReturnTest", parameters, commandType: CommandType.StoredProcedure);
            int res = parameters.Get<int>("foo");
            
            return res == 1;
        }

        public bool GetScalar(bool input)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            byte res = db.ExecuteScalar<byte>("ReturnTestAlt", new {input}, commandType: CommandType.StoredProcedure);
            return res == 1;
        }

        public List<User> PassArray(int[] array)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            return db.Query<User>("SELECT * FROM Users Where Id IN @array", new {array}).ToList();
        }

        public List<User> GetNested()
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            
            return db.Query<User, Company, User>(
                    @"SELECT * FROM Users u
                            JOIN Companies c ON u.CompanyID = c.id",
                    (user, company) =>
                    {
                        user.Company = company;
                        return user;
                    },
                    splitOn: "Id")//CompanyID
                .ToList();
        }

        public (List<User> users, List<Company> companies) GetMultiple()
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            using SqlMapper.GridReader multyRes = db.QueryMultiple("select * from Users; select * from Companies");
            List<User> users = multyRes.Read<User>().ToList();
            List<Company> companies = multyRes.Read<Company>().ToList();

            return (users, companies);
        }
    }

    public interface IDapperService
    {
        void Create(User user);
        void Delete(int id);
        User Get(int id);
        List<User> GetUsers();
        void Update(User user);

        bool GetReturn(bool input);
        bool GetScalar(bool input);
        List<User> PassArray(int[] array);
        List<User> GetNested();
        (List<User> users, List<Company> companies) GetMultiple();
    }
}
