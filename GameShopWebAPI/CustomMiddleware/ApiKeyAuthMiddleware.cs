namespace GameShopWebAPI.CustomMiddleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKey = "ApiKey";

        public ApiKeyAuthMiddleware(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(ApiKey, out var extractedKey)) 
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API key not given in the request");
                return;
            }

            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var key = appSettings.GetValue<string>(ApiKey);
            if (!key.Equals(extractedKey)) 
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API key is not valid");
                return;
            }

            await _next(context);
        }
    }
}
