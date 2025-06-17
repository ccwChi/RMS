using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using MeasrueVendor.Repository;
using RecipeManageSystem.Models;
using Dapper;

namespace RecipeManageSystem.Repository
{
    public class MachineParamRepository : BaseRepository
    {
        public List<Machine> GetMachineList()
        {
            using (var conn = new SqlConnection(mesString))
            {
                string sql = @"SELECT DeviceID, DeviceName, MpsSectionNo, StateFlag
                               FROM MES_MACHINE
                               WHERE StateFlag = 'Y'
                               ORDER BY DeviceID";
                return conn.Query<Machine>(sql).ToList();
            }
        }
        public List<MachineParameterView> GetAllMappings()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                string sql = @"
                    SELECT 
                        mp.MappingId,
                        mp.DeviceId,
                        mp.ParamId,
                        m.DeviceName,
                        p.ParamName
                    FROM RMS.dbo.MachineParameter mp
                    LEFT JOIN MES_DEV.dbo.MES_MACHINE m ON mp.DeviceId = m.DeviceID
                    LEFT JOIN RMS.dbo.Parameter p ON mp.ParamId = p.ParamId
                    Order by m.MpsSectionNo, mp.DeviceID ";
                    
                return conn.Query<MachineParameterView>(sql).ToList();
            }
        }

        public bool SaveMappings(MachineParameterDto dto)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    // 1. 刪除同一台機台所有舊設定
                    conn.Execute("DELETE FROM RMS.dbo.MachineParameter WHERE DeviceId = @DeviceId",
                                 new { dto.DeviceId }, tran);

                    // 2. 批次插入新的所有參數項
                    if (dto.Params != null && dto.Params.Any())
                    {
                        const string insertSql = @"
                            INSERT INTO RMS.dbo.MachineParameter(DeviceId, ParamId)
                            VALUES(@DeviceId, @ParamId)";
                        foreach (var pid in dto.Params)
                        {
                            conn.Execute(insertSql, new { DeviceId = dto.DeviceId, ParamId = pid }, tran);
                        }
                    }

                    tran.Commit();
                    return true;
                }
            }
        }

        public bool UpdateMapping(MachineParameter data)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                var sql = "UPDATE RMS.dbo.MachineParameter SET DeviceId=@DeviceId, ParamId=@ParamId WHERE MappingId=@MappingId";
                return conn.Execute(sql, data) > 0;
            }
        }

        public string GetDeviceIdByMappingId(int mappingId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.QueryFirstOrDefault<string>(
                    "SELECT DeviceId FROM RMS.dbo.MachineParameter WHERE MappingId = @mappingId",
                    new { mappingId }
                );
            }
        }

        public List<int> GetParamIdsByMachine(string deviceId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<int>(
                    "SELECT ParamId FROM RMS.dbo.MachineParameter WHERE DeviceId = @deviceId",
                    new { deviceId }
                ).ToList();
            }
        }
        
        
    }
}