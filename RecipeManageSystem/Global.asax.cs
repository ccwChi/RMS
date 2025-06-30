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
            
            if (defaultLogin == "1") // �u�b�������ұҥ�
            {
                // �p�G�|�����ҡA�N��ʫإߵn�J����
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

                    // �ߧY��ʸѪR���� �� �����n�J���A
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

                    // 3. �A�s�� RMS DB�A��l�� RoleId + PermissionIds
                    customUser.InitializePermissions();

                    // 4. �� HttpContext.Current.User �����ڭ̪� customUser
                    HttpContext.Current.User = customUser;
                }
            }
        }
    }
}
