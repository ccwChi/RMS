using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using Dapper;
using MeasrueVendor.Repository;
using RecipeManageSystem.Models;

namespace RecipeManageSystem.Repository
{
    public class AlarmManageRepository : BaseRepository
    {
        public List<MachineGroup> GetMachineGroups()
        {
            const string sql = @"
                    SELECT
                      MachineGroupId,
                      MAX(GroupName)   AS GroupName,
                      MAX(Description) AS Description
                    FROM RMS.dbo.MachineGroup
                    GROUP BY MachineGroupId
                    ORDER BY MachineGroupId";
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<MachineGroup>(sql).ToList();
            }
        }

        public List<string> GetMachineGroupsById(int id)
        {
            const string sql = @"
                    SELECT DeviceId
                    FROM RMS.dbo.MachineGroup
                    WHERE MachineGroupId = @id";
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<string>(sql, new { id }).ToList();
            }
        }

        public int SaveMachineGroup(int groupId, string groupName, string description, List<string> devices, string user)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // 若是新增就先產生新的 MachineGroupId
                    var mgid = groupId == 0
                        ? conn.QuerySingle<int>("SELECT ISNULL(MAX(MachineGroupId),0)+1 FROM RMS.dbo.MachineGroup", transaction: tx)
                        : groupId;

                    // 刪除此群組舊機台
                    conn.Execute("DELETE FROM RMS.dbo.MachineGroup WHERE MachineGroupId = @mgid",
                        new { mgid }, tx);

                    // 插入新機台
                    const string insertSql = @"
                    INSERT INTO RMS.dbo.MachineGroup
                      (MachineGroupId, GroupName, Description, DeviceId, CreateBy)
                    VALUES
                      (@MachineGroupId, @GroupName, @Description, @DeviceId, @CreateBy)";
                    foreach (var dev in devices)
                    {
                        conn.Execute(insertSql, new
                        {
                            MachineGroupId = mgid,
                            GroupName = groupName,
                            Description = description,
                            DeviceId = dev,
                            CreateBy = user
                        }, tx);
                    }

                    tx.Commit();
                    return mgid;
                }
            }
        }

        public bool DeleteMachineGroup(int groupId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        conn.Execute("DELETE FROM RMS.dbo.MachineGroup WHERE MachineGroupId = @id",
                            new { id = groupId }, tx);
                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }


        public List<int> GetAlertGroupRoles(int alertGroupId)
        {
            const string sql = @"SELECT RoleId FROM RMS.dbo.AlertGroupRole WHERE AlertGroupId = @alertGroupId";
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<int>(sql, new { alertGroupId }).ToList();
            }
        }


        public void SaveAlertGroup(AlertGroupDto dto, string user)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // 如果是新群組，先取新的 AlertGroupId
                    var id = dto.AlertGroupId;
                    if (id == 0)
                    {
                        id = conn.QuerySingle<int>(@"
                            SELECT ISNULL(MAX(AlertGroupId),0) + 1 FROM RMS.dbo.AlertGroup",
                            transaction: tx);
                    }

                    // 先刪掉舊記錄（簡化做法）
                    conn.Execute(@"DELETE FROM RMS.dbo.AlertGroup WHERE AlertGroupId=@id", new { id }, tx);
                    conn.Execute(@"DELETE FROM RMS.dbo.AlertGroupRole WHERE AlertGroupId=@id", new { id }, tx);

                    // 插入 AlertGroup
                    conn.Execute(@"
                            INSERT INTO RMS.dbo.AlertGroup (AlertGroupId, GroupName, Description, CreateBy)
                            VALUES (@AlertGroupId, @GroupName, @Description, @CreateBy)",
                    new
                    {
                        AlertGroupId = id,
                        dto.GroupName,
                        dto.Description,
                        CreateBy = user
                    }, tx);

                    // 插入 AlertGroupRole
                    foreach (var roleId in dto.Roles ?? new List<int>())
                    {
                        conn.Execute(@"
                            INSERT INTO RMS.dbo.AlertGroupRole (AlertGroupId, RoleId)
                            VALUES (@AlertGroupId, @RoleId)",
                            new { AlertGroupId = id, RoleId = roleId }, tx);
                    }

                    tx.Commit();
                }
            }
        }

    }

}