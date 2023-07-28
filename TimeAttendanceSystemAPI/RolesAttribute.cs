using Microsoft.AspNetCore.Authorization;

namespace TimeAttendanceSystemAPI
{
    public class RolesAttribute : AuthorizeAttribute
    {
        public RolesAttribute(params string[] roles) 
        { 
            Roles = String.Join(",", roles);
        }
    }
}
