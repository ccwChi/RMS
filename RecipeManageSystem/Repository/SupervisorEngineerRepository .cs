using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using RecipeManageSystem.Repository;
using RecipeManageSystem.Models;

namespace RecipeManageSystem.Repository
{
    public class SupervisorEngineerRepository : BaseRepository
    {
        public List<SupervisorEngineerRelation> GetAllRelations()
        {
            using (var conn = new SqlConnection(mesString))
            {
                string sql = @"
                    SELECT Id, SupervisorNo, SupervisorName, EngineerNo, EngineerName, 
                           EffectiveDate, ExpiryDate, IsActive, CreateTime, UpdateTime
                    FROM INJECT_SUPERVISOR_ENGINEER_RELATION 
                    ORDER BY SupervisorNo, EngineerNo";

                return conn.Query<SupervisorEngineerRelation>(sql).ToList();
            }
        }

        public SupervisorEngineerRelation GetRelationById(long id)
        {
            using (var conn = new SqlConnection(mesString))
            {
                string sql = @"
                    SELECT Id, SupervisorNo, SupervisorName, EngineerNo, EngineerName, 
                           EffectiveDate, ExpiryDate, IsActive, CreateTime, UpdateTime
                    FROM INJECT_SUPERVISOR_ENGINEER_RELATION 
                    WHERE Id = @id";

                return conn.QueryFirstOrDefault<SupervisorEngineerRelation>(sql, new { id });
            }
        }

        public void CreateRelation(SupervisorEngineerRelation relation)
        {
            using (var conn = new SqlConnection(mesString))
            {
                string sql = @"
                    INSERT INTO INJECT_SUPERVISOR_ENGINEER_RELATION 
                    (SupervisorNo, SupervisorName, EngineerNo, EngineerName, 
                     EffectiveDate, ExpiryDate, IsActive, CreateTime, UpdateTime)
                    VALUES 
                    (@SupervisorNo, @SupervisorName, @EngineerNo, @EngineerName, 
                     @EffectiveDate, @ExpiryDate, @IsActive, @CreateTime, @UpdateTime)";

                conn.Execute(sql, relation);
            }
        }

        public void UpdateRelation(SupervisorEngineerRelation relation)
        {
            using (var conn = new SqlConnection(mesString))
            {
                string sql = @"
                    UPDATE INJECT_SUPERVISOR_ENGINEER_RELATION 
                    SET SupervisorNo = @SupervisorNo,
                        SupervisorName = @SupervisorName,
                        EngineerNo = @EngineerNo,
                        EngineerName = @EngineerName,
                        EffectiveDate = @EffectiveDate,
                        ExpiryDate = @ExpiryDate,
                        IsActive = @IsActive,
                        UpdateTime = @UpdateTime
                    WHERE Id = @Id";

                conn.Execute(sql, relation);
            }
        }

        public void DeleteRelation(long id)
        {
            using (var conn = new SqlConnection(mesString))
            {
                string sql = "DELETE FROM INJECT_SUPERVISOR_ENGINEER_RELATION WHERE Id = @id";
                conn.Execute(sql, new { id });
            }
        }

        public List<Engineer> GetAllEngineers()
        {
            using (var conn = new SqlConnection(mesString))
            {
                string sql = @"
                        SELECT mu.UserNo as EngineerNo, mu.UserName as EngineerName, mu.DepartmentNo, mu.DepartmentName, md.FatherDepartmentNo, md.FatherDepartmentName, mu.Email 
                        FROM MES_USERS mu
                        INNER JOIN MES_DEPARTMENT md  on mu.DepartmentNo =  md.DepartmentNo
                        where mu.ExpirationDate is null
                        and md.FatherDepartmentName in ('製造一部', '製造四部')";

                return conn.Query<Engineer>(sql).ToList();
            }
        }
    }
}