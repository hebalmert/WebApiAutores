using Microsoft.AspNetCore.Builder;

namespace WebApiAutores.Middlewares
{
    public static class LoguearRespuestasHHTPMiddlwareExtensions
    {
        public static IApplicationBuilder UseLoguearRespuestaHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoguerarRespuestaHTTPMiddleware>();
        }
    }


    public class LoguerarRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate siguinte;
        private readonly ILogger<LoguerarRespuestaHTTPMiddleware> logger;

        public LoguerarRespuestaHTTPMiddleware(RequestDelegate siguinte, ILogger<LoguerarRespuestaHTTPMiddleware> logger)
        {
            this.siguinte = siguinte;
            this.logger = logger;
        }

        //Invoke o InvokeAsync  para que pueda trabajar como un Middleware
        public async Task InvokeAsync(HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                var cuerpoOriginalRespuesta = contexto.Response.Body;
                contexto.Response.Body = ms;

                await siguinte(contexto);

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginalRespuesta);
                contexto.Response.Body = cuerpoOriginalRespuesta;

                logger.LogInformation(respuesta);
            }
        }
    }
}
