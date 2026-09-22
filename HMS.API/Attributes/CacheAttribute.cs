using HMS.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace HMS.API.Attributes
{
    public class CacheAttribute : ActionFilterAttribute
    {
        private readonly double _durationInHours;

        public CacheAttribute(double durationInHours)
        {
            _durationInHours = durationInHours;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

            var cacheKey = CreateCacheKey(context.HttpContext.Request);

            var cacheValue = await cacheService.GetCacheValueAsync(cacheKey);

            if (!string.IsNullOrWhiteSpace(cacheValue))
            {
                var result = new ContentResult()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Content = cacheValue,
                    ContentType = "application/json"
                };
                context.Result = result;
                return;
            }
            else
            {
                var actionExecuted = await next.Invoke();
                if (actionExecuted.Result is OkObjectResult okResult)
                {
                    await cacheService.SetCacheValueAsync(
                        cacheKey,
                        okResult.Value!,
                        TimeSpan.FromHours(_durationInHours)
                        );
                }
            }

        }

        private string CreateCacheKey(HttpRequest request)
        {
            var cacheKey = new StringBuilder();

            cacheKey.Append(request.Path);

            foreach (var query in request.Query.OrderBy(q => q.Key))
                cacheKey.Append($"|{query.Key}-{query.Value}");

            return cacheKey.ToString();
        }
    }
}
