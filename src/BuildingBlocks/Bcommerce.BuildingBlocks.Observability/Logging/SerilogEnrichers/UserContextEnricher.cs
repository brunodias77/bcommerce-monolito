using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Bcommerce.BuildingBlocks.Observability.Logging.SerilogEnrichers;

public class UserContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextEnricher() : this(new HttpContextAccessor())
    {
    }
    
    public UserContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
         _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
             logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
        }
        
        var userName = user.Identity.Name;
        if (!string.IsNullOrEmpty(userName))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userName));
        }
    }
}
