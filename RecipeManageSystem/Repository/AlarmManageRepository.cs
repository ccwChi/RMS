using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using Dapper;
using RecipeManageSystem.Repository;
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

        // 新增一個方法來取得機台群組的詳細資訊（包含機台清單）
        public List<MachineGroupWithDevices> GetMachineGroupsWithDevices()
        {
            const string sql = @"
                            SELECT
                            MachineGroupId,
                            MAX(GroupName)   AS GroupName,
                            MAX(Description) AS Description,
                            STRING_AGG(DeviceId, ', ') AS DeviceList,
                            COUNT(DeviceId) AS DeviceCount
                            FROM RMS.dbo.MachineGroup
                            GROUP BY MachineGroupId
                            ORDER BY MachineGroupId";

            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<MachineGroupWithDevices>(sql).ToList();
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



        public List<AlertGroupSummaryDto> GetAlertGroups()
        {
            const string sql = @"
                SELECT 
                    MIN(ag.AlertGroupId) AS AlertGroupId,
                    ag.GroupName, 
                    ag.Description,
                    ag.MachineGroupId,
                    ag.IsActive,
                    MIN(ag.CreateBy) AS CreateBy, 
                    MIN(ag.CreateDate) AS CreateDate, 
                    MIN(ag.UpdateBy) AS UpdateBy, 
                    MIN(ag.UpdateDate) AS UpdateDate,
                    STRING_AGG(CAST(ag.RoleId AS VARCHAR), ',') AS RoleIds
                FROM RMS.dbo.AlertGroup ag
                GROUP BY ag.GroupName, ag.Description, ag.MachineGroupId, ag.IsActive
                ORDER BY MIN(ag.AlertGroupId)";

            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<AlertGroupSummaryDto>(sql).ToList();
            }
        }

        public AlertGroupDetailDto GetAlertGroupDetail(int alertGroupId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
            SELECT TOP 1
                ag.AlertGroupId,
                ag.GroupName, 
                ag.Description,
                ag.IsActive,
                ag.MachineGroupId
            FROM RMS.dbo.AlertGroup ag
            WHERE ag.AlertGroupId = @alertGroupId";

                var detail = conn.QueryFirstOrDefault<AlertGroupDetailDto>(sql, new { alertGroupId });

                if (detail != null)
                {
                    // 取得對應的角色
                    const string sqlRoles = @"
                SELECT DISTINCT RoleId 
                FROM RMS.dbo.AlertGroup 
                WHERE AlertGroupId = @alertGroupId";

                    detail.RoleIds = conn.Query<int>(sqlRoles, new { alertGroupId }).ToList();

                    // 取得對應的機台群組
                    const string sqlMachineGroups = @"
                SELECT DISTINCT MachineGroupId 
                FROM RMS.dbo.AlertGroup 
                WHERE AlertGroupId = @alertGroupId";

                    detail.MachineGroupIds = conn.Query<int>(sqlMachineGroups, new { alertGroupId }).ToList();
                }

                return detail;
            }
        }

        public void SaveAlertGroup(AlertGroupDto dto, string user)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 如果是新增，取得新的 AlertGroupId
                        int alertGroupId = dto.AlertGroupId;
                        if (alertGroupId == 0)
                        {
                            alertGroupId = conn.QuerySingle<int>(
                                "SELECT ISNULL(MAX(AlertGroupId), 0) + 1 FROM RMS.dbo.AlertGroup",
                                transaction: tx);
                        }

                        // 刪除舊記錄（使用 AlertGroupId）
                        conn.Execute(@"DELETE FROM RMS.dbo.AlertGroup WHERE AlertGroupId = @AlertGroupId",
                            new { AlertGroupId = alertGroupId }, tx);

                        // 插入新記錄
                        const string insertSql = @"
                    INSERT INTO RMS.dbo.AlertGroup 
                    (GroupName, Description, MachineGroupId, RoleId, IsActive, CreateBy, CreateDate)
                    VALUES 
                    (@GroupName, @Description, @MachineGroupId, @RoleId, @IsActive, @CreateBy, GETDATE())";

                        foreach (var roleId in dto.RoleIds ?? new List<int>())
                        {
                            foreach (var machineGroupId in dto.MachineGroupIds ?? new List<int>())
                            {
                                conn.Execute(insertSql, new
                                {
                                    AlertGroupId = alertGroupId,
                                    dto.GroupName,
                                    dto.Description,
                                    MachineGroupId = machineGroupId,
                                    RoleId = roleId,
                                    dto.IsActive,
                                    CreateBy = user
                                }, tx);
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool ToggleAlertGroupActive(int alertGroupId, bool isActive, string user)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
            UPDATE RMS.dbo.AlertGroup 
            SET IsActive = @IsActive, 
                UpdateBy = @UpdateBy, 
                UpdateDate = GETDATE()
            WHERE AlertGroupId = @AlertGroupId";

                var rowsAffected = conn.Execute(sql, new
                {
                    IsActive = isActive,
                    UpdateBy = user,
                    AlertGroupId = alertGroupId
                });

                return rowsAffected > 0;
            }
        }

        public bool DeleteAlertGroup(int alertGroupId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                try
                {
                    var rowsAffected = conn.Execute(
                        "DELETE FROM RMS.dbo.AlertGroup WHERE AlertGroupId = @alertGroupId",
                        new { alertGroupId });

                    return rowsAffected > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
    }

}