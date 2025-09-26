using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Middleware
{
    //maps InvalidOperationException, KeyNotFoundException, etc. to 400/404)

    public sealed class ExceptionMappingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMappingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            try { await _next(ctx); }
            catch (KeyNotFoundException ex)
            {
                await WriteProblem(ctx, ex.Message, (int)HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ex)
            {
                await WriteProblem(ctx, ex.Message, (int)HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await WriteProblem(ctx, "Unexpected error.", (int)HttpStatusCode.InternalServerError, ex);
            }
        }

        private static async Task WriteProblem(HttpContext ctx, string title, int status, Exception? ex = null)
        {
            var problem = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = ex?.Message,
                Instance = ctx.Request.Path
            };
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(problem);
        }
    }
}
