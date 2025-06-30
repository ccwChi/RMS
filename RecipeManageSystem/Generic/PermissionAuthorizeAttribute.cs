using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecipeManageSystem.Generic
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>允許的權限清單；留空代表只驗證是否登入</summary>
        public int[] PermissionIds { get; set; } = new int[0];

        public PermissionAuthorizeAttribute(params int[] permissionIds)
        {
            PermissionIds = permissionIds ?? new int[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext)) return false; // 未登入

            var user = httpContext.User as CustomPrincipal;
            if (user == null) return false;

            if (PermissionIds == null || PermissionIds.Length == 0)
                return true; // 未指定權限，代表只要登入就好

            // 只要有其中一個權限即可
            return user.PermissionIds.Intersect(PermissionIds).Any();
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // Ajax 請求 → 回傳 JSON 給前端處理
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = "您沒有權限執行此操作。可以在使用者清單查看自己的設定身分後，前往權限管理確認。" },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                // 一般頁面導向 → 轉跳至無權限頁面
                filterContext.Result = new RedirectResult("~/Home/NoAuth");
            }
        }

    }
}
