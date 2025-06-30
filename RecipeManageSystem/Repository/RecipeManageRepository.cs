using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using MeasrueVendor.Repository;
using RecipeManageSystem.Models;
using Dapper;
using System.Configuration;

namespace RecipeManageSystem.Repository
{
    public class RecipeManageRepository : BaseRepository
    {
        public bool SaveRecipe(RecipeTotalDto dto, string mode, string currentUser)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    int recipeId = dto.RecipeId;
                    if (recipeId == 0)
                    {
                        // 第一次建立，等同於 newVersion
                        recipeId = InsertNewHeader(dto, 1, currentUser, conn, tran);
                        InsertLog(recipeId, "Create", currentUser, conn, tran);
                    }
                    else if (mode == "save")
                    {
                        // 直接覆寫目前這個版本
                        UpdateHeader(dto, currentUser, conn, tran);
                        InsertLog(recipeId, "Update", currentUser, conn, tran);
                        conn.Execute("DELETE FROM RMS.dbo.RecipeDetail WHERE RecipeId = @recipeId", new { recipeId }, tran);
                    }
                    else if (mode == "newVersion")
                    {
                        // 把舊版停用
                        conn.Execute("UPDATE RMS.dbo.RecipeHeader SET IsActive = 0 WHERE RecipeId = @recipeId", new { recipeId }, tran);
                        // 取得舊版最新版次
                        int oldVer = conn.ExecuteScalar<int>("SELECT Version FROM RMS.dbo.RecipeHeader WHERE RecipeId = @recipeId", new { recipeId }, tran);
                        // 插入新一版，版次 +1
                        recipeId = InsertNewHeader(dto, oldVer + 1, currentUser, conn, tran);
                        InsertLog(recipeId, "NewVersion", currentUser, conn, tran);
                    }

                    // 不論哪種都要插入明細到新的 recipeId
                    const string sqlDetail = @"
                        INSERT INTO RMS.dbo.RecipeDetail
                          (RecipeId, ParamId, ParamName, StdValue, MaxValue, MinValue, BiasMethod, BiasValue, AlarmFlag)
                        VALUES
                          (@RecipeId, @ParamId, @ParamName, @StdValue, @MaxValue, @MinValue, @BiasMethod, @BiasValue, @AlarmFlag)";
                    foreach (var d in dto.RecipeDetails)
                    {
                        conn.Execute(sqlDetail, new
                        {
                            RecipeId = recipeId,
                            d.ParamId,
                            d.ParamName,
                            d.StdValue,
                            d.MaxValue,
                            d.MinValue,
                            d.BiasMethod,
                            d.BiasValue,
                            d.AlarmFlag
                        }, tran);
                    }

                    tran.Commit();
                    return true;
                }
            }
        }

        // 插入新 Header 的共用方法
        private int InsertNewHeader(RecipeTotalDto dto, int version, string user, IDbConnection conn, IDbTransaction tran)
        {
            string sql = @"
                    INSERT INTO RMS.dbo.RecipeHeader
                      (ProdNo, DeviceId, MoldNo, MaterialNo, ProdName, Remark, Version, IsActive, CreateBy)
                    VALUES
                      (@ProdNo, @DeviceId, @MoldNo, @MaterialNo, @ProdName, @Remark, @Version, 1, @CreateBy);
                    SELECT CAST(SCOPE_IDENTITY() AS int);";
            return conn.ExecuteScalar<int>(sql, new
            {
                dto.ProdNo,
                dto.DeviceId,
                dto.MoldNo,
                dto.MaterialNo,
                dto.ProdName,
                dto.Remark,
                Version = version,
                CreateBy = user
            }, tran);
        }

        // 更新 Header（不改版次、不停用）
        private void UpdateHeader(
            RecipeTotalDto dto,
            string user,
            IDbConnection conn,
            IDbTransaction tran)
        {
            conn.Execute(@"
                    UPDATE RMS.dbo.RecipeHeader
                    SET
                      ProdNo      = @ProdNo,
                      DeviceId    = @DeviceId,
                      MoldNo      = @MoldNo,
                      MaterialNo  = @MaterialNo,
                      ProdName    = @ProdName,
                      UpdateBy    = @UpdateBy,
                      UpdateDate  = GETDATE(),
                      Remark      = @Remark
                    WHERE RecipeId = @RecipeId;",
            new
            {
                dto.RecipeId,
                dto.ProdNo,
                dto.DeviceId,
                dto.MoldNo,
                dto.MaterialNo,
                dto.ProdName,
                dto.Remark,
                UpdateBy = user
            }, tran);
        }

        // 寫入 Log
        private void InsertLog(
            int recipeId,
            string op,
            string user,
            IDbConnection conn,
            IDbTransaction tran)
        {
            conn.Execute(@"
                INSERT INTO RMS.dbo.RecipeLog
                  (RecipeId, Operation, OperateBy, Remark)
                VALUES
                  (@RecipeId, @Op, @User, @Op);",
            new { RecipeId = recipeId, Op = op, User = user }, tran);
        }


        public List<RecipeTotalDto> GetRecipes(string prodNo, string deviceName)
        {
            using (var conn = new SqlConnection(mesString))
            {
                var sql = @"
                    SELECT 
                      h.RecipeId,
                      h.ProdNo,
                      h.ProdName,
                      h.MaterialNo,
                      h.DeviceId,
                      m.DeviceName,
                      h.MoldNo,
                      h.CreateBy,
                      h.CreateDate,
                      h.Version,
                      h.IsActive,
                      h.Remark
                    FROM RMS.dbo.RecipeHeader h
                    LEFT JOIN dbo.MES_MACHINE m ON h.DeviceId = m.DeviceID and m.StateFlag = 'Y'
                    WHERE (@prodNo IS NULL OR h.ProdNo  LIKE '%'+@prodNo+'%')
                      AND (@deviceName IS NULL OR m.DeviceName LIKE '%'+@deviceName+'%')
                      AND m.Plant in ('1101', '1103')
                    ORDER BY h.CreateDate DESC, h.DeviceId, h.ProdNo, h.MoldNo, h.MaterialNo;";

                return conn.Query<RecipeTotalDto>(
                    sql,
                    new
                    {
                        prodNo = string.IsNullOrEmpty(prodNo) ? null : prodNo,
                        deviceName = string.IsNullOrEmpty(deviceName) ? null : deviceName
                    }
                ).ToList();
            }
        }



        public List<RecipeDetail> GetRecipeDetails(int recipeId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    SELECT ParamName, StdValue, MaxValue, MinValue, BiasMethod, BiasValue, AlarmFlag
                    FROM RMS.dbo.RecipeDetail
                    WHERE RecipeId = @recipeId
                    AND AlarmFlag = 'Y'
                    ORDER BY RecipeDetailId";
                return conn.Query<RecipeDetail>(sql, new { recipeId }).ToList();
            }
        }



        public List<RecipeEditDto> GetParamDetailToEdit(string deviceId, string prodNo, string materialNo,string moldNo)
        {
            List<RecipeEditDto> result = new List<RecipeEditDto>();

            // 如果該參數已經有被設定過，讀取出來做更改。
            string sql = @"Select rh.RecipeId, rh.ProdNo, rh.ProdName, rh.MaterialNo, rh.DeviceId, rh.MoldNo, rh.Version, rh.IsActive, rh.Remark, rb.ParamId, rb.ParamName, rb.StdValue , rb.[MinValue] , rb.[MaxValue], rb.BiasMethod, rb.BiasValue, p.Unit
                           FROM RMS.dbo.RecipeHeader rh 
                           LEFT JOIN RMS.dbo.RecipeDetail rb on rb.RecipeId = rh.RecipeId 
                           LEFT JOIN RMS.dbo.Parameter p on rb.ParamId = p.paramId
                           WHERE rh.DeviceId = @deviceId ";

            if (!string.IsNullOrWhiteSpace(prodNo))
            {
                sql += @" and rh.ProdNo = @prodNo ";
            }

            if (!string.IsNullOrWhiteSpace(materialNo))
            {
                sql += @" and rh.MaterialNo = @materialNo ";
            }
            if (!string.IsNullOrWhiteSpace(moldNo))
            {
                sql += @" and rh.MoldNo = @moldNo ";
            }

            using (var conn = new SqlConnection(rmsString))
            {
                result = conn.Query<RecipeEditDto>(sql, new { deviceId, prodNo, materialNo, moldNo }).ToList();
            }

            if (result.Any())
            {
                return result;
            }


            // 如果該參數完全沒設定過，讀取應該寫的參數表出讓使用者填
            sql = @"Select p.ParamId, p.ParamName,p.Unit,p.SectionCode,p.SequenceNo,p.IsActive
                            FROM RMS.dbo.MachineParameter mp
                            left join RMS.dbo.[Parameter] p on mp.ParamId  = p.ParamId
                            where mp.DeviceId = @deviceId
                           ;";


            using (var conn = new SqlConnection(rmsString))
            {
                result = conn.Query<RecipeEditDto>(sql, new { deviceId }).ToList();
            }

            return result;
        }


        public List<RecipeVersionDto> GetRecipeVersions(string deviceId, string prodNo, string materialNo, string moldNo)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();

                const string sqlVersions = @"
                    SELECT 
                      h.RecipeId, h.Version, h.ProdNo, h.MaterialNo, h.MoldNo, h.DeviceId, h.Remark,h.CreateBy, h.CreateDate, h.UpdateBy, h.UpdateDate,
                      d.ParamId, d.StdValue, d.MaxValue, d.MinValue, d.BiasMethod, d.BiasValue, 
                      p.ParamName
                    FROM RMS.dbo.RecipeHeader h
                    JOIN RMS.dbo.RecipeDetail d 
                      ON h.RecipeId = d.RecipeId
                    LEFT JOIN RMS.dbo.Parameter p 
                      ON d.ParamId = p.ParamId
                    WHERE h.DeviceId   = @deviceId
                      AND h.ProdNo     = @prodNo
                      AND (@materialNo = '' OR h.MaterialNo = @materialNo)
                      AND (@moldNo     = '' OR h.MoldNo       = @moldNo)
                    ORDER BY h.Version DESC;
                    ";

                var lookup = new Dictionary<int, RecipeVersionDto>();

                // 1) 試著先撈出既有版本
                conn.Query<RecipeHeader, RecipeDetailDto, RecipeVersionDto>(sqlVersions, (hdr, det) =>
                    {
                        if (!lookup.TryGetValue(hdr.RecipeId, out var version))
                        {
                            version = new RecipeVersionDto
                            {
                                RecipeId = hdr.RecipeId,
                                DeviceId = hdr.DeviceId,
                                ProdNo = hdr.ProdNo,
                                MaterialNo = hdr.MaterialNo,
                                MoldNo = hdr.MoldNo,
                                Version = hdr.Version,
                                Remark = hdr.Remark,
                                UpdateBy = hdr.UpdateBy,
                                UpdateDate = hdr.UpdateDate,
                                CreateBy = hdr.CreateBy,
                                CreateDate = hdr.CreateDate,
                                Params = new List<RecipeDetailDto>()
                            };
                            lookup.Add(hdr.RecipeId, version);
                        }
                        version.Params.Add(det);
                        return version;
                    },
                    new
                    {
                        deviceId,
                        prodNo,
                        materialNo = materialNo ?? string.Empty,
                        moldNo = moldNo ?? string.Empty
                    },
                    splitOn: "ParamId")
                    .AsQueryable()  // to consume
                    .ToList();

                // 如果有既有版本就直接回傳
                if (lookup.Count > 0)
                {
                    return lookup.Values.ToList();
                }

                // 2) 沒有任何版本，表示全新建立 → 取 MachineParameter 列出所有可填參數
                const string sqlDefs = @"
                        SELECT 
                        p.ParamId, p.ParamName, p.Unit, p.SectionCode, p.SequenceNo, p.IsActive
                        FROM RMS.dbo.MachineParameter mp
                        JOIN RMS.dbo.Parameter p 
                          ON mp.ParamId = p.ParamId
                        WHERE mp.DeviceId = @deviceId;
                        ";

                var defs = conn.Query<RecipeDetailDto>(
                    sqlDefs, new { deviceId }).ToList();

                // 組成一個「空版本」讓前端建立
                var newVersion = new RecipeVersionDto
                {
                    RecipeId = 0,
                    DeviceId = deviceId,
                    ProdNo = prodNo,
                    MaterialNo = materialNo,
                    MoldNo = moldNo,
                    Version = 0,
                    Remark = string.Empty,
                    UpdateBy = null,
                    UpdateDate = null,
                    Params = defs.Select(d => new RecipeDetailDto
                    {
                        ParamId = d.ParamId,
                        ParamName = d.ParamName,
                        StdValue = string.Empty,
                        MaxValue = string.Empty,
                        MinValue = string.Empty,
                        BiasMethod = "customize",
                        BiasValue = string.Empty
                    }).ToList()
                };

                return new List<RecipeVersionDto> { newVersion };
            }
        }


    }
}