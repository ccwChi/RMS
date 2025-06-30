using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using MeasrueVendor.Repository;
using RecipeManageSystem.Models;

namespace RecipeManageSystem.Repository
{
    public class AlarmManageRepository : BaseRepository
    {





        // 取得所有接收人工號
        public List<AlertUser> GetAlertUsers()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    SELECT AlertId, UserNo, CreateBy, CreateDate
                    FROM RMS.dbo.AlertUser
                    ORDER BY CreateDate DESC";
                                    return conn.Query<AlertUser>(sql).ToList();
            }
        }

        // 新增一筆接收人
        public bool InsertAlertUser(string userNo, string createBy)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                                INSERT INTO RMS.dbo.AlertUser (UserNo, CreateBy)
                                VALUES (@UserNo, @CreateBy)";
                return conn.Execute(sql, new { UserNo = userNo, CreateBy = createBy }) > 0;
            }
        }

        // 刪除一筆接收人
        public bool DeleteAlertUser(int alertId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    DELETE FROM RMS.dbo.AlertUser
                    WHERE AlertId = @AlertId";
                return conn.Execute(sql, new { AlertId = alertId }) > 0;
            }
        }





    }
}