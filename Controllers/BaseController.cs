using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace AllocationSystem.WebApi.Controllers;

public class BaseController : Controller
{
    private readonly IActionContextAccessor _accessor;

    public BaseController(IActionContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected string IpAddress
    {
        get
        {
            return _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }

    protected string Email
    {
        get
        {
            if (HttpContext.Request.Headers.ContainsKey("Email"))
                return HttpContext.Request.Headers["Email"].ToString();

            return default;
        }
    }

    protected long UserId
    {
        get
        {
            //if (HttpContext.Request.Headers.ContainsKey("UserId"))
            //    return long.Parse(HttpContext.Request.Headers["UserId"].ToString());

            //if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value == null)            
            // return Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return Convert.ToInt64(HttpContext.User.SubjectId());//.FindFirst("sub")?.Value);//HttpContext.User.GetLoggedInUserId<long>();
            //return -1;
        }
    }
    //public string GetUserIdentity()
    //{
    //    return _accessor.ActionContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
    //}
}

