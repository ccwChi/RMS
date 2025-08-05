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

        // 在 AlarmManageRepository.cs 中需要補充的方法

        public List<AlertGroupDto> GetAlertGroups()
        {
            const string sql = @"
                        SELECT 
                            ag.AlertGroupId, 
                            ag.GroupName, 
                            ag.Description, 
                            ag.IsActive,
                            ag.CreateBy, 
                            ag.CreateDate, 
                            ag.UpdateBy, 
                            ag.UpdateDate,
                            agr.RoleId
                        FROM RMS.dbo.AlertGroup ag
                        LEFT JOIN RMS.dbo.AlertGroupRole agr ON ag.AlertGroupId = agr.AlertGroupId
                        ORDER BY ag.AlertGroupId";

            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<AlertGroupDto>(sql).ToList();
            }
        }


        public AlertGroupDetailDto GetAlertGroupDetail(int alertGroupId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                // 取得基本資料和角色
                const string sqlBasic = @"
                    SELECT 
                        ag.AlertGroupId, 
                        ag.GroupName, 
                        ag.Description,
                        ag.IsActive,
                        agr.RoleId
                    FROM RMS.dbo.AlertGroup ag
                    LEFT JOIN RMS.dbo.AlertGroupRole agr ON ag.AlertGroupId = agr.AlertGroupId
                    WHERE ag.AlertGroupId = @alertGroupId";

                var detail = conn.QueryFirstOrDefault<AlertGroupDetailDto>(sqlBasic, new { alertGroupId });

                if (detail != null)
                {
                    // 取得對應的機台群組
                    const string sqlMachineGroups = @"
                        SELECT MachineGroupId 
                        FROM RMS.dbo.AlertGroupMachineGroup 
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
                        var id = dto.AlertGroupId;

                        if (id == 0)
                        {
                            // 新增：讓 IDENTITY 自動產生 ID
                            const string insertSql = @"
                        INSERT INTO RMS.dbo.AlertGroup (GroupName, Description, IsActive, CreateBy, CreateDate)
                        VALUES (@GroupName, @Description, @IsActive, @CreateBy, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() AS int);";

                            id = conn.QuerySingle<int>(insertSql, new
                            {
                                dto.GroupName,
                                dto.Description,
                                dto.IsActive,
                                CreateBy = user
                            }, tx);
                        }
                        else
                        {
                            // 編輯：先刪除舊的關聯記錄，再更新主記錄
                            conn.Execute(@"DELETE FROM RMS.dbo.AlertGroupRole WHERE AlertGroupId=@id", new { id }, tx);
                            conn.Execute(@"DELETE FROM RMS.dbo.AlertGroupMachineGroup WHERE AlertGroupId=@id", new { id }, tx);

                            // 更新 AlertGroup
                            conn.Execute(@"
                        UPDATE RMS.dbo.AlertGroup 
                        SET GroupName = @GroupName, 
                            Description = @Description, 
                            IsActive = @IsActive,
                            UpdateBy = @UpdateBy, 
                            UpdateDate = GETDATE()
                        WHERE AlertGroupId = @AlertGroupId",
                            new
                            {
                                dto.GroupName,
                                dto.Description,
                                dto.IsActive,
                                UpdateBy = user,
                                AlertGroupId = id
                            }, tx);
                        }

                        // 插入 AlertGroupRole（一對一）
                        if (dto.RoleId > 0)
                        {
                            conn.Execute(@"
                        INSERT INTO RMS.dbo.AlertGroupRole (AlertGroupId, RoleId, CreateBy, CreateDate)
                        VALUES (@AlertGroupId, @RoleId, @CreateBy, GETDATE())",
                                new { AlertGroupId = id, RoleId = dto.RoleId, CreateBy = user }, tx);
                        }

                        // 插入 AlertGroupMachineGroup（一對多）
                        foreach (var machineGroupId in dto.MachineGroupIds ?? new List<int>())
                        {
                            conn.Execute(@"
                        INSERT INTO RMS.dbo.AlertGroupMachineGroup (AlertGroupId, MachineGroupId, CreateBy, CreateDate)
                        VALUES (@AlertGroupId, @MachineGroupId, @CreateBy, GETDATE())",
                                new { AlertGroupId = id, MachineGroupId = machineGroupId, CreateBy = user }, tx);
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
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 先刪除關聯的記錄（CASCADE 應該會自動處理，但手動刪除更安全）
                        conn.Execute("DELETE FROM RMS.dbo.AlertGroupRole WHERE AlertGroupId = @id",
                            new { id = alertGroupId }, tx);

                        conn.Execute("DELETE FROM RMS.dbo.AlertGroupMachineGroup WHERE AlertGroupId = @id",
                            new { id = alertGroupId }, tx);

                        // 再刪除群組本身
                        conn.Execute("DELETE FROM RMS.dbo.AlertGroup WHERE AlertGroupId = @id",
                            new { id = alertGroupId }, tx);

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





    }

}