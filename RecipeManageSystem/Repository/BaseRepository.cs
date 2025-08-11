using System.Configuration;

namespace RecipeManageSystem.Repository
{
    public class BaseRepository
    {
        protected readonly string mesString;
        protected readonly string rmsString;
        protected readonly string qcBasString;
        protected readonly string EnvFlag;

        public BaseRepository()
        {
            // 讀取環境設定，沒有就預設為開發環境
            EnvFlag = ConfigurationManager.AppSettings["EnvFlag"] ?? "0";

            // 取得 RMS 連線字串（必要）
            var rmsConnectionName = (EnvFlag == "1") ? "RMSConnection" : "RMS_DEVConnection";
            var rmsConfig = ConfigurationManager.ConnectionStrings[rmsConnectionName];

            // 如果找不到指定的，嘗試另一個
            if (rmsConfig == null)
            {
                rmsConfig = ConfigurationManager.ConnectionStrings["RMSConnection"] ??
                           ConfigurationManager.ConnectionStrings["RMS_DEVConnection"];
            }

            rmsString = rmsConfig?.ConnectionString ?? "";

            // 取得 MES 連線字串（選用）
            var mesConnectionName = (EnvFlag == "1") ? "MESConnection" : "MES_DEVConnection";
            var mesConfig = ConfigurationManager.ConnectionStrings[mesConnectionName];

            if (mesConfig == null)
            {
                mesConfig = ConfigurationManager.ConnectionStrings["MESConnection"] ??
                           ConfigurationManager.ConnectionStrings["MES_DEVConnection"];
            }

            mesString = mesConfig?.ConnectionString ?? "";

            // 取得 QC 連線字串（選用）
            var qcConfig = ConfigurationManager.ConnectionStrings["6129Connection"];
            qcBasString = qcConfig?.ConnectionString ?? "";
        }
    }
}