using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.HealthChecks;
using Newtonsoft.Json;

namespace Microsoft.AspNet.HealthChecks
{
    public class HealthCheckHandler : HttpTaskAsyncHandler
    {
        public override bool IsReusable => true;

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var result = await GlobalHealthChecks.Service.CheckHealthAsync();
            var status = result.CheckStatus;

            if (status != CheckStatus.Healthy)
                context.Response.StatusCode = 503;

            context.Response.Headers.Add("content-type", "application/json");
            context.Response.Write(JsonConvert.SerializeObject(new { status = status.ToString() }));
        }
    }
}
