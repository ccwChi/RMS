using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using RecipeManageSystem.Repository;
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
                        // 如果要新建版本且設為啟用，需要先將同一組合的其他版本停用
                        if (dto.IsActive)
                        {
                            conn.Execute(@"
                                    UPDATE RMS.dbo.RecipeHeader 
                                    SET IsActive = 0 
                                    WHERE DeviceId = @deviceId 
                                      AND ProdNo = @prodNo 
                                      AND ISNULL(MaterialNo, '') = @materialNo 
                                      AND ISNULL(MoldNo, '') = @moldNo",
                                new
                                {
                                    deviceId = dto.DeviceId,
                                    prodNo = dto.ProdNo,
                                    materialNo = dto.MaterialNo ?? "",
                                    moldNo = dto.MoldNo ?? ""
                                }, tran);
                        }

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
            // 如果新版本設為啟用，需要先將同一組合的其他版本停用
            if (dto.IsActive)
            {
                conn.Execute(@"
                    UPDATE RMS.dbo.RecipeHeader 
                    SET IsActive = 0 
                    WHERE DeviceId = @deviceId 
                      AND ProdNo = @prodNo 
                      AND ISNULL(MaterialNo, '') = @materialNo 
                      AND ISNULL(MoldNo, '') = @moldNo",
                    new
                    {
                        deviceId = dto.DeviceId,
                        prodNo = dto.ProdNo,
                        materialNo = dto.MaterialNo ?? "",
                        moldNo = dto.MoldNo ?? ""
                    }, tran);
            }

            string sql = @"
                INSERT INTO RMS.dbo.RecipeHeader
                  (ProdNo, DeviceId, MoldNo, MaterialNo, ProdName, Remark, Version, IsActive, CreateBy)
                VALUES
                  (@ProdNo, @DeviceId, @MoldNo, @MaterialNo, @ProdName, @Remark, @Version, @IsActive, @CreateBy);
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
                IsActive = dto.IsActive,  // 加入 IsActive
                CreateBy = user
            }, tran);
        }

        // 更新 Header（不改版次、不停用）
        private void UpdateHeader(RecipeTotalDto dto, string user, IDbConnection conn, IDbTransaction tran)
        {
            // 如果要將此版本設為啟用，需要先將同一組合的其他版本停用
            if (dto.IsActive)
            {
                conn.Execute(@"
            UPDATE RMS.dbo.RecipeHeader 
            SET IsActive = 0 
            WHERE DeviceId = @deviceId 
              AND ProdNo = @prodNo 
              AND ISNULL(MaterialNo, '') = @materialNo 
              AND ISNULL(MoldNo, '') = @moldNo
              AND RecipeId != @recipeId",
                    new
                    {
                        deviceId = dto.DeviceId,
                        prodNo = dto.ProdNo,
                        materialNo = dto.MaterialNo ?? "",
                        moldNo = dto.MoldNo ?? "",
                        recipeId = dto.RecipeId
                    }, tran);
            }

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
                  Remark      = @Remark,
                  IsActive    = @IsActive
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
                IsActive = dto.IsActive,  // 加入 IsActive
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

        public List<RecipeTotalDto> GetRecipes(string prodNo, string deviceName, string moldNo, bool showAllVersions = false)
        {
            using (var conn = new SqlConnection(mesString))
            {
                // 根據 showAllVersions 參數決定是否要過濾 IsActive
                var whereClause = showAllVersions ? "" : "AND h.IsActive = 1";

                var sql = $@"
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
                        WHERE (@prodNo IS NULL OR h.ProdNo LIKE '%'+@prodNo+'%')
                          AND (@deviceName IS NULL OR m.DeviceName LIKE '%'+@deviceName+'%')
                          AND (@moldNo IS NULL OR h.MoldNo LIKE '%'+@moldNo+'%')
                          AND m.Plant in ('1101', '1103')
                          {whereClause}
                        ORDER BY h.CreateDate DESC, h.DeviceId, h.ProdNo, h.MoldNo, h.MaterialNo, h.Version DESC;";

                return conn.Query<RecipeTotalDto>(
                    sql,
                    new
                    {
                        prodNo = string.IsNullOrEmpty(prodNo) ? null : prodNo,
                        deviceName = string.IsNullOrEmpty(deviceName) ? null : deviceName,
                        moldNo = string.IsNullOrEmpty(moldNo) ? null : moldNo
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

        public List<RecipeEditDto> GetParamDetailToEdit(string deviceId, string prodNo, string materialNo, string moldNo)
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

                // 1. 先取得該機台目前應該有的參數定義
                const string sqlMachineParams = @"
            SELECT 
                p.ParamId, p.ParamName, p.Unit, p.SectionCode, p.SequenceNo, p.IsActive
            FROM RMS.dbo.MachineParameter mp
            JOIN RMS.dbo.Parameter p ON mp.ParamId = p.ParamId
            WHERE mp.DeviceId = @deviceId
            ORDER BY p.SectionCode, p.SequenceNo, p.ParamName";

                var currentMachineParams = conn.Query<RecipeDetailDto>(sqlMachineParams, new { deviceId }).ToList();

                // 2. 查詢是否有既有的Recipe版本
                const string sqlVersions = @"
            SELECT 
              h.RecipeId, h.Version, h.ProdNo, h.MaterialNo, h.MoldNo, h.DeviceId, h.Remark,
              h.CreateBy, h.CreateDate, h.IsActive, h.UpdateBy, h.UpdateDate,
              d.ParamId, d.StdValue, d.MaxValue, d.MinValue, d.BiasMethod, d.BiasValue, d.AlarmFlag,
              p.ParamName, p.Unit, p.SectionCode, p.SequenceNo, p.IsActive as ParamIsActive
            FROM RMS.dbo.RecipeHeader h
            LEFT JOIN RMS.dbo.RecipeDetail d ON h.RecipeId = d.RecipeId
            LEFT JOIN RMS.dbo.Parameter p ON d.ParamId = p.ParamId
            WHERE h.DeviceId = @deviceId
              AND h.ProdNo = @prodNo
              AND (@materialNo = '' OR h.MaterialNo = @materialNo)
              AND (@moldNo = '' OR h.MoldNo = @moldNo)
            ORDER BY h.Version DESC";

                var lookup = new Dictionary<int, RecipeVersionDto>();

                // 3. 處理既有版本的資料
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
                            IsActive = hdr.IsActive,
                            Remark = hdr.Remark,
                            UpdateBy = hdr.UpdateBy,
                            UpdateDate = hdr.UpdateDate,
                            CreateBy = hdr.CreateBy,
                            CreateDate = hdr.CreateDate,
                            Params = new List<RecipeDetailDto>()
                        };
                        lookup.Add(hdr.RecipeId, version);
                    }

                    // 只有當 det.ParamId 有值時才加入（避免 LEFT JOIN 產生的空資料）
                    if (det != null && det.ParamId > 0)
                    {
                        version.Params.Add(det);
                    }
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
                .AsQueryable()
                .ToList();

                // 4. 如果有既有版本，處理參數合併邏輯
                if (lookup.Count > 0)
                {
                    foreach (var version in lookup.Values)
                    {
                        // 建立完整的參數清單
                        var completeParams = new List<RecipeDetailDto>();

                        // 首先加入所有已經設定過的參數（保持原有順序和資料）
                        var existingParamIds = new HashSet<int>();
                        foreach (var existingParam in version.Params)
                        {
                            completeParams.Add(existingParam);
                            existingParamIds.Add(existingParam.ParamId);
                        }

                        // 然後檢查機台目前的參數定義，補上新增的參數
                        foreach (var machineParam in currentMachineParams)
                        {
                            // 如果這個參數在既有設定中沒有，就新增一個空的參數項目
                            if (!existingParamIds.Contains(machineParam.ParamId))
                            {
                                completeParams.Add(new RecipeDetailDto
                                {
                                    ParamId = machineParam.ParamId,
                                    ParamName = machineParam.ParamName,
                                    StdValue = string.Empty,
                                    MaxValue = string.Empty,
                                    MinValue = string.Empty,
                                    BiasMethod = "percent",
                                    BiasValue = "10",
                                    AlarmFlag = "N"
                                });
                            }
                        }

                        // 可選：按照 SectionCode, SequenceNo, ParamName 重新排序
                        // 如果你希望保持某種順序的話
                        completeParams = completeParams
                            .OrderBy(p => {
                                var machineParam = currentMachineParams.FirstOrDefault(mp => mp.ParamId == p.ParamId);
                                return machineParam?.ParamName ?? "ZZZ"; // 不在機台定義中的參數排到最後
                            })
                            .ThenBy(p => {
                                var machineParam = currentMachineParams.FirstOrDefault(mp => mp.ParamId == p.ParamId);
                                return machineParam?.ParamId ?? 9999;
                            })
                            .ToList();

                        // 更新為完整的參數清單
                        version.Params = completeParams;
                    }

                    return lookup.Values.ToList();
                }

                // 5. 沒有任何版本，建立新的空版本
                var newVersion = new RecipeVersionDto
                {
                    RecipeId = 0,
                    DeviceId = deviceId,
                    ProdNo = prodNo,
                    MaterialNo = materialNo,
                    MoldNo = moldNo,
                    Version = 0,
                    IsActive = true,
                    Remark = string.Empty,
                    UpdateBy = null,
                    UpdateDate = null,
                    CreateBy = null,
                    CreateDate = DateTime.Now,
                    Params = currentMachineParams.Select(p => new RecipeDetailDto
                    {
                        ParamId = p.ParamId,
                        ParamName = p.ParamName,
                        StdValue = string.Empty,
                        MaxValue = string.Empty,
                        MinValue = string.Empty,
                        BiasMethod = "percent",
                        BiasValue = "10",
                        AlarmFlag = "Y"
                    }).ToList()
                };

                return new List<RecipeVersionDto> { newVersion };
            }
        }

        /// <summary>
        /// 根據機台ID取得該機台所有的料號清單
        /// </summary>
        public List<string> GetProdNosByDevice(string deviceId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    SELECT DISTINCT ProdNo 
                    FROM RMS.dbo.RecipeHeader 
                    WHERE DeviceId = @deviceId 
                      AND ProdNo IS NOT NULL 
                      AND ProdNo != ''
                    ORDER BY ProdNo";

                return conn.Query<string>(sql, new { deviceId }).ToList();
            }
        }

        /// <summary>
        /// 根據機台ID和料號取得模具清單
        /// </summary>
        public List<string> GetMoldNosByDeviceAndProd(string deviceId, string prodNo)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    SELECT DISTINCT MoldNo 
                    FROM RMS.dbo.RecipeHeader 
                    WHERE DeviceId = @deviceId 
                      AND ProdNo = @prodNo
                      AND MoldNo IS NOT NULL 
                      AND MoldNo != ''
                    ORDER BY MoldNo";

                return conn.Query<string>(sql, new { deviceId, prodNo }).ToList();
            }
        }

        /// <summary>
        /// 根據機台ID和料號取得原料清單
        /// </summary>
        public List<string> GetMaterialNosByDeviceAndProd(string deviceId, string prodNo)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    SELECT DISTINCT MaterialNo 
                    FROM RMS.dbo.RecipeHeader 
                    WHERE DeviceId = @deviceId 
                      AND ProdNo = @prodNo
                      AND MaterialNo IS NOT NULL 
                      AND MaterialNo != ''
                    ORDER BY MaterialNo";

                return conn.Query<string>(sql, new { deviceId, prodNo }).ToList();
            }
        }


        // 在 RecipeManageRepository.cs 中新增此方法
        /// <summary>
        /// 取得指定機台的參數定義
        /// </summary>
        public List<RecipeDetailDto> GetMachineParameterDefinitions(string deviceId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
            SELECT 
                p.ParamId, 
                p.ParamName, 
                p.Unit, 
                p.SectionCode, 
                p.SequenceNo, 
                p.IsActive
            FROM RMS.dbo.MachineParameter mp
            JOIN RMS.dbo.Parameter p ON mp.ParamId = p.ParamId
            WHERE mp.DeviceId = @deviceId
              AND p.IsActive = 1
            ORDER BY p.SectionCode, p.SequenceNo, p.ParamName";

                return conn.Query<RecipeDetailDto>(sql, new { deviceId }).ToList();
            }
        }



        /// <summary>
        /// 刪除指定的Recipe版本
        /// </summary>
        public bool DeleteRecipe(int recipeId, string currentUser)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 刪除 RecipeDetail
                        conn.Execute("DELETE FROM RMS.dbo.RecipeDetail WHERE RecipeId = @recipeId",
                                   new { recipeId }, tran);

                        // 2. 刪除 RecipeHeader
                        conn.Execute("DELETE FROM RMS.dbo.RecipeHeader WHERE RecipeId = @recipeId",
                                   new { recipeId }, tran);

                        // 3. 記錄 Log
                        InsertLog(recipeId, "Delete", currentUser, conn, tran);

                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }


        // 在 RecipeManageRepository.cs 中新增此方法
        /// <summary>
        /// 切換Recipe版本的啟用/停用狀態
        /// </summary>
        public bool ToggleRecipeStatus(int recipeId, bool isActive, string currentUser)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 如果要啟用此版本，需要先取得此Recipe的基本資訊
                        if (isActive)
                        {
                            var recipeInfo = conn.QueryFirstOrDefault(@"
                        SELECT DeviceId, ProdNo, MaterialNo, MoldNo 
                        FROM RMS.dbo.RecipeHeader 
                        WHERE RecipeId = @recipeId",
                                new { recipeId }, tran);

                            if (recipeInfo != null)
                            {
                                // 先將同一組合的其他版本停用
                                conn.Execute(@"
                            UPDATE RMS.dbo.RecipeHeader 
                            SET IsActive = 0 
                            WHERE DeviceId = @deviceId 
                              AND ProdNo = @prodNo 
                              AND ISNULL(MaterialNo, '') = @materialNo 
                              AND ISNULL(MoldNo, '') = @moldNo
                              AND RecipeId != @recipeId",
                                    new
                                    {
                                        deviceId = recipeInfo.DeviceId,
                                        prodNo = recipeInfo.ProdNo,
                                        materialNo = recipeInfo.MaterialNo ?? "",
                                        moldNo = recipeInfo.MoldNo ?? "",
                                        recipeId = recipeId
                                    }, tran);
                            }
                        }

                        // 更新目標Recipe的狀態
                        conn.Execute(@"
                            UPDATE RMS.dbo.RecipeHeader 
                            SET IsActive = @isActive, 
                                UpdateBy = @updateBy, 
                                UpdateDate = GETDATE() 
                            WHERE RecipeId = @recipeId",
                            new
                            {
                                isActive = isActive,
                                updateBy = currentUser,
                                recipeId = recipeId
                            }, tran);

                        // 記錄操作日誌
                        InsertLog(recipeId, isActive ? "Activate" : "Deactivate", currentUser, conn, tran);

                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }

    }
}