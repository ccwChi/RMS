using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using RecipeManageSystem.Repository;
using RecipeManageSystem.Models;
using Dapper;

namespace RecipeManageSystem.Repository
{
    public class ParamRepository : BaseRepository
    {

        public List<Parameter> GetParameterList()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                string sql = @"SELECT ParamId, ParamName, Unit,  SectionCode, SequenceNo, IsActive, CreateDate, CreateBy, UpdateDate, UpdateBy
                               FROM RMS.dbo.Parameter
                               ORDER BY ParamName";
                return conn.Query<Parameter>(sql).ToList();

            }
        }

        public Parameter GetParameterById(int paramId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                        SELECT ParamId, ParamName, Unit,  SectionCode, SequenceNo, IsActive, CreateDate, CreateBy, UpdateDate, UpdateBy
                        FROM RMS.dbo.Parameter
                        WHERE ParamId = @paramId";
                return conn.QueryFirstOrDefault<Parameter>(sql, new { paramId });
            }
        }


        public void CreateParameter(Parameter parameter)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                string sql = @"INSERT INTO RMS.dbo.Parameter
                               (ParamName, Unit, SectionCode, SequenceNo, IsActive, CreateBy, CreateDate)
                               VALUES
                               (@ParamName, @Unit, @SectionCode, @SequenceNo, @IsActive, @CreateBy, GETDATE())";
                conn.Execute(sql, new
                {
                    parameter.ParamName,
                    parameter.Unit,
                    parameter.SectionCode,
                    parameter.SequenceNo,
                    parameter.IsActive,
                    parameter.CreateBy
                });
            }
        }

        public void UpdateParameter(Parameter parameter)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                  UPDATE RMS.dbo.Parameter
                     SET ParamName     = @ParamName,
                     Unit              = @Unit,
                     SectionCode       = @SectionCode,
                     SequenceNo        = @SequenceNo,
                     IsActive          = @IsActive,
                     UpdateBy          = @UpdateBy,
                     UpdateDate         = GETDATE()
                    WHERE ParamId = @ParamId";
                conn.Execute(sql, new
                {
                    parameter.ParamId,
                    parameter.ParamName,
                    parameter.Unit,
                    parameter.SectionCode,
                    parameter.SequenceNo,
                    parameter.IsActive,
                    parameter.UpdateBy
                });
            }
        }





    }
}