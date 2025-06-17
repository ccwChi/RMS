using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace RecipeManageSystem.Generic
{
    public class CustomPrincipal : IPrincipal
    {
        //private readonly CustomIdentity identity;
        private readonly IPrincipal _systemPrincipal;

        public CustomPrincipal(IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            _systemPrincipal = principal;
        }
        public IIdentity Identity
        {
            get { return _systemPrincipal.Identity; }
        }

        public bool IsInRole(string role)
        {
            return _systemPrincipal.IsInRole(role);
        }

        public string UserNo { get; set; }
        public string UserName { get; set; }
        public string DeptNo { get; set; }
        public string DeptName { get; set; }
        public string Title { get; set; }
    }
}