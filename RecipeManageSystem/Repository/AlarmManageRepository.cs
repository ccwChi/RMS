using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using RecipeManageSystem.Repository;
using RecipeManageSystem.Models;

namespace RecipeManageSystem.Repository
{
    public class AlarmManageRepository : BaseRepository
    {
        // ===== 機台群組相關方法 =====
        public List<MachineGroup> GetMachineGroups()
        {
            const string sql = @"
                SELECT
                    MachineGroupId,
                    MAX(GroupName) AS GroupName,
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
                    var mgid = groupId == 0
                        ? conn.QuerySingle<int>("SELECT ISNULL(MAX(MachineGroupId),0)+1 FROM RMS.dbo.MachineGroup", transaction: tx)
                        : groupId;

                    conn.Execute("DELETE FROM RMS.dbo.MachineGroup WHERE MachineGroupId = @mgid",
                        new { mgid }, tx);

                    const string insertSql = @"
                        INSERT INTO RMS.dbo.MachineGroup
                        (MachineGroupId, GroupName, Description, DeviceId, CreateBy, CreateDate)
                        VALUES
                        (@MachineGroupId, @GroupName, @Description, @DeviceId, @CreateBy, GETDATE())";

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

        public MachineGroup GetMachineGroupById(int machineGroupId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string groupSql = @"
                    SELECT TOP 1
                        MachineGroupId,
                        GroupName,
                        Description,
                        CreateBy,
                        CreateDate,
                        UpdateBy,
                        UpdateDate
                    FROM RMS.dbo.MachineGroup
                    WHERE MachineGroupId = @MachineGroupId";

                var group = conn.QueryFirstOrDefault<MachineGroup>(groupSql, new { MachineGroupId = machineGroupId });

                if (group != null)
                {
                    const string deviceSql = @"
                        SELECT DeviceId
                        FROM RMS.dbo.MachineGroup
                        WHERE MachineGroupId = @MachineGroupId";

                    var devices = conn.Query<string>(deviceSql, new { MachineGroupId = machineGroupId }).ToList();
                    group.Devices = devices;
                }

                return group;
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

        public (bool canDelete, string reason) CanDeleteMachineGroup(int machineGroupId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                try
                {
                    const string checkAlertGroupSql = @"
                        SELECT COUNT(*) 
                        FROM RMS.dbo.AlertGroup 
                        WHERE MachineGroupId = @MachineGroupId";

                    var alertGroupCount = conn.QuerySingle<int>(checkAlertGroupSql, new { MachineGroupId = machineGroupId });

                    if (alertGroupCount > 0)
                    {
                        const string getAlertGroupNamesSql = @"
                            SELECT DISTINCT GroupName 
                            FROM RMS.dbo.AlertGroup 
                            WHERE MachineGroupId = @MachineGroupId";

                        var alertGroupNames = conn.Query<string>(getAlertGroupNamesSql, new { MachineGroupId = machineGroupId }).ToList();
                        var nameList = string.Join("、", alertGroupNames.Take(3));
                        var moreText = alertGroupNames.Count > 3 ? $" 等 {alertGroupNames.Count} 個" : "";

                        return (false, $"此機台群組正被警報群組「{nameList}{moreText}」使用，無法刪除");
                    }

                    const string checkExistsSql = @"
                        SELECT COUNT(*) 
                        FROM RMS.dbo.MachineGroup 
                        WHERE MachineGroupId = @MachineGroupId";

                    var existsCount = conn.QuerySingle<int>(checkExistsSql, new { MachineGroupId = machineGroupId });

                    if (existsCount == 0)
                    {
                        return (false, "找不到指定的機台群組");
                    }

                    return (true, "可以安全刪除");
                }
                catch (Exception ex)
                {
                    return (false, $"檢查時發生錯誤：{ex.Message}");
                }
            }
        }

        public MachineGroup CreateMachineGroupDto(int machineGroupId, string groupName, string description, List<string> devices, string operateBy)
        {
            return new MachineGroup
            {
                MachineGroupId = machineGroupId,
                GroupName = groupName,
                Description = description,
                Devices = devices ?? new List<string>(),
                CreateBy = operateBy,
                CreateDate = DateTime.Now
            };
        }

        // ===== 警報群組相關方法 =====

        /// <summary>
        /// 取得簡化的警報群組列表 (一個群組名稱+角色組合顯示為一筆)
        /// </summary>
        public List<AlertGroupSummaryDto> GetAlertGroupsSimplified()
        {
            const string sql = @"
                SELECT 
                    ag.GroupName,
                    ag.Description,
                    ag.RoleId,
                    r.RoleName,
                    ag.IsActive,
                    COUNT(DISTINCT ag.MachineGroupId) as MachineGroupCount,
                    STRING_AGG(CAST(mg.GroupName AS NVARCHAR(MAX)), ', ') as MachineGroupNames,
                    MIN(ag.CreateBy) as CreateBy,
                    MIN(ag.CreateDate) as CreateDate,
                    MAX(ag.UpdateBy) as UpdateBy,
                    MAX(ag.UpdateDate) as UpdateDate
                FROM RMS.dbo.AlertGroup ag
                LEFT JOIN RMS.dbo.Roles r ON ag.RoleId = r.RoleId
                LEFT JOIN (
                    SELECT DISTINCT MachineGroupId, GroupName 
                    FROM RMS.dbo.MachineGroup 
                ) mg ON ag.MachineGroupId = mg.MachineGroupId
                GROUP BY ag.GroupName, ag.Description, ag.RoleId, r.RoleName, ag.IsActive
                ORDER BY ag.GroupName, r.RoleName";

            using (var conn = new SqlConnection(rmsString))
            {
                var result = conn.Query<AlertGroupSummaryDto>(sql).ToList();

                // 加上序號
                for (int i = 0; i < result.Count; i++)
                {
                    result[i].RowNumber = i + 1;
                }

                return result;
            }
        }

        /// <summary>
        /// 根據群組名稱和角色取得警報群組列表
        /// </summary>
        public List<AlertGroupDto> GetAlertGroupsByNameAndRole(string groupName, int roleId)
        {
            const string sql = @"
                SELECT 
                    AlertGroupId,
                    GroupName,
                    Description,
                    MachineGroupId,
                    RoleId,
                    IsActive,
                    CreateBy,
                    CreateDate,
                    UpdateBy,
                    UpdateDate
                FROM RMS.dbo.AlertGroup
                WHERE GroupName = @GroupName AND RoleId = @RoleId
                ORDER BY AlertGroupId";

            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<AlertGroupDto>(sql, new { GroupName = groupName, RoleId = roleId }).ToList();
            }
        }

        /// <summary>
        /// 根據群組名稱和角色取得詳細資料 (用於編輯時載入)
        /// </summary>
        public AlertGroupDetailDto GetAlertGroupDetailByNameAndRole(string groupName, int roleId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                // 取得基本資料
                const string basicSql = @"
                    SELECT TOP 1
                        GroupName,
                        Description,
                        RoleId,
                        IsActive
                    FROM RMS.dbo.AlertGroup
                    WHERE GroupName = @GroupName AND RoleId = @RoleId";

                var detail = conn.QueryFirstOrDefault<AlertGroupDetailDto>(basicSql, new { GroupName = groupName, RoleId = roleId });

                if (detail != null)
                {
                    // 取得關聯的機台群組ID列表
                    const string machineGroupSql = @"
                        SELECT DISTINCT MachineGroupId
                        FROM RMS.dbo.AlertGroup
                        WHERE GroupName = @GroupName AND RoleId = @RoleId";

                    detail.MachineGroupIds = conn.Query<int>(machineGroupSql, new { GroupName = groupName, RoleId = roleId }).ToList();
                    detail.RoleIds = new List<int> { roleId };
                }

                return detail;
            }
        }

        /// <summary>
        /// 批量儲存警報群組 (一個角色對應多個機台群組)
        /// </summary>
        public List<AlertGroupDto> SaveAlertGroupBatch(AlertGroupBatchDto dto, string user)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 先刪除該群組名稱+角色的所有記錄
                        conn.Execute(@"
                            DELETE FROM RMS.dbo.AlertGroup 
                            WHERE GroupName = @GroupName AND RoleId = @RoleId",
                            new { GroupName = dto.GroupName, RoleId = dto.RoleId }, tx);

                        var savedGroups = new List<AlertGroupDto>();

                        // 為每個機台群組建立一筆記錄
                        const string insertSql = @"
                            INSERT INTO RMS.dbo.AlertGroup 
                            (GroupName, Description, MachineGroupId, RoleId, IsActive, CreateBy, CreateDate)
                            VALUES 
                            (@GroupName, @Description, @MachineGroupId, @RoleId, @IsActive, @CreateBy, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS int);";

                        foreach (var machineGroupId in dto.MachineGroupIds)
                        {
                            var alertGroupId = conn.QuerySingle<int>(insertSql, new
                            {
                                dto.GroupName,
                                dto.Description,
                                MachineGroupId = machineGroupId,
                                RoleId = dto.RoleId,
                                dto.IsActive,
                                CreateBy = user
                            }, tx);

                            savedGroups.Add(new AlertGroupDto
                            {
                                AlertGroupId = alertGroupId,
                                GroupName = dto.GroupName,
                                Description = dto.Description,
                                MachineGroupId = machineGroupId,
                                RoleId = dto.RoleId.Value,
                                IsActive = dto.IsActive,
                                CreateBy = user,
                                CreateDate = DateTime.Now
                            });
                        }

                        tx.Commit();
                        return savedGroups;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 根據群組名稱和角色刪除警報群組
        /// </summary>
        public bool DeleteAlertGroupsByNameAndRole(string groupName, int roleId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                try
                {
                    var rowsAffected = conn.Execute(@"
                        DELETE FROM RMS.dbo.AlertGroup 
                        WHERE GroupName = @GroupName AND RoleId = @RoleId",
                        new { GroupName = groupName, RoleId = roleId });

                    return rowsAffected > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 根據群組名稱和角色切換啟用狀態
        /// </summary>
        public bool ToggleAlertGroupsActive(string groupName, int roleId, bool isActive, string user)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    UPDATE RMS.dbo.AlertGroup 
                    SET IsActive = @IsActive, 
                        UpdateBy = @UpdateBy, 
                        UpdateDate = GETDATE()
                    WHERE GroupName = @GroupName AND RoleId = @RoleId";

                var rowsAffected = conn.Execute(sql, new
                {
                    IsActive = isActive,
                    UpdateBy = user,
                    GroupName = groupName,
                    RoleId = roleId
                });

                return rowsAffected > 0;
            }
        }
    }
}