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
            string env = ConfigurationManager.AppSettings["EnvFlag"];
            
            if (env != "1") // 只在測試環境啟用
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
                var newUser = new CustomPrincipal(systemPrincipal);

                var flag = ConfigurationManager.AppSettings["EnvFlag"];
                var connName = (flag == "1") ? "MESConnection" : "MES_DEVConnection";
                var connStr = ConfigurationManager.ConnectionStrings[connName].ConnectionString;

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                                    SELECT TOP 1 UserNo, UserName, DepartmentNo, TitleName , DepartmentName
                                    FROM MES_USERS 
                                    WHERE (UserNo = @name OR LdapAccount = @name) AND ExpirationDate IS NULL", conn);
                    cmd.Parameters.AddWithValue("@name", User.Identity.Name);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            newUser.UserNo = reader["UserNo"].ToString();
                            newUser.UserName = reader["UserName"].ToString();
                            newUser.DeptNo = reader["DepartmentNo"].ToString();
                            newUser.Title = reader["TitleName"].ToString();
                            newUser.DeptName = reader["DepartmentName"].ToString(); 
                            HttpContext.Current.User = newUser;
                        }
                    }
                }
            }
        }
    }
}
