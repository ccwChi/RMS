using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using System.Web.Routing;

using RecipeManageSystem.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Principal;
using Dapper;

namespace RecipeManageSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_AuthenticateRequest()
        {
            string defaultLogin = ConfigurationManager.AppSettings["DefaultLogin"];
            
            if (defaultLogin == "1") // 只在測試環境啟用
            {
                // 如果尚未驗證，就手動建立登入票證
                if (HttpContext.Current.User == null)
                {
                    string testUserNo = "G02828";
                    string role = "Admin";

                    var ticket = new FormsAuthenticationTicket(
                        1,
                        testUserNo,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30),
                        false,
                        role,
                        FormsAuthentication.FormsCookiePath
                    );

                    string encTicket = FormsAuthentication.Encrypt(ticket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
                    {
                        HttpOnly = true
                    };
                    HttpContext.Current.Response.Cookies.Add(authCookie);

                    // 立即手動解析票證 → 模擬登入狀態
                    var identity = new FormsIdentity(ticket);
                    var principal = new GenericPrincipal(identity, new[] { role });
                    HttpContext.Current.User = principal;
                }
            }
        }


        protected void Application_PostAuthenticateRequest()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var systemPrincipal = User;
                var customUser = new CustomPrincipal(systemPrincipal);

                var flag = ConfigurationManager.AppSettings["EnvFlag"];
                var mesConnKey = flag == "1" ? "MESConnection" : "MES_DEVConnection";
                var mesConnStr = ConfigurationManager.ConnectionStrings[mesConnKey].ConnectionString;

                using (var mesConn = new SqlConnection(mesConnStr))
                {
                    const string sql = @"
                    SELECT TOP 1
                        UserNo,
                        UserName,
                        DepartmentNo AS DeptNo,
                        DepartmentName AS DeptName,
                        TitleName      AS Title
                    FROM MES_USERS
                    WHERE (UserNo = @name OR LdapAccount = @name)
                      AND ExpirationDate IS NULL";

                    var mesInfo = mesConn.QuerySingleOrDefault<(string UserNo, string UserName, string DeptNo, string DeptName, string Title)>(
                        sql, new { name = User.Identity.Name });

                    if (mesInfo == default)
                        return;

                    customUser.UserNo = mesInfo.UserNo;
                    customUser.UserName = mesInfo.UserName;
                    customUser.DeptNo = mesInfo.DeptNo;
                    customUser.DeptName = mesInfo.DeptName;
                    customUser.Title = mesInfo.Title;

                    // 3. 再連到 RMS DB，初始化 RoleId + PermissionIds
                    customUser.InitializePermissions();

                    // 4. 把 HttpContext.Current.User 換成我們的 customUser
                    HttpContext.Current.User = customUser;
                }
            }
        }
    }
}
