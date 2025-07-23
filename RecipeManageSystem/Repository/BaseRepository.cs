using System.Configuration;


namespace MeasrueVendor.Repository
{
    public class BaseRepository
    {
        protected readonly string mesString;
        protected readonly string rmsString;
        protected readonly string WmsDbConnectionString;
        protected readonly string EnvFlag = ConfigurationManager.AppSettings["EnvFlag"];

        //protected readonly string SpcLoaderString = ConfigurationManager.ConnectionStrings["SPC_LOADER"].ConnectionString;

        protected readonly string qcBasString = ConfigurationManager.ConnectionStrings["6129Connection"].ConnectionString;



        public BaseRepository()
        {
            mesString = ConfigurationManager.ConnectionStrings[
                (EnvFlag == "1") ? "MESConnection" : "MES_DEVConnection"
            ].ConnectionString;

            rmsString = ConfigurationManager.ConnectionStrings[
                (EnvFlag == "1") ? "RMSConnection" : "RMS_DEVConnection"
            ].ConnectionString;
        }
    }
}