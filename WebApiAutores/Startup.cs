using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]  // para no tener que hacer la convencion por controlador
                                                              //asi no apareceran No Document en las respuesta de los EndPoint
namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //Con esto evitamos el Mapeo automatico de los Claims, para poderlos llamar como 
            //nosotros lo personalizamos cuando creamos la lista de Claims.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigurationServices(IServiceCollection Services)
        {
            //Se agrega para eliminar Las Consultas Ciclicas en los Include de los API
            //Este es sin el filtro Global de FiltroDeExepcion para capturar los Errores en tiempo de ejecucion
            //Services.AddControllers().AddJsonOptions(x=> x.JsonSerializerOptions
            //.ReferenceHandler = ReferenceHandler.IgnoreCycles);
            Services.AddControllers(option =>
            {
                option.Filters.Add(typeof(FiltroDeExepcion));
                //para hacer el agrupamiento de la carpeta Utilidad/SwaggerAgrupaPorVersiones
                option.Conventions.Add(new SwaggerAgrupaPorVersion());

            }).AddJsonOptions(x => x.JsonSerializerOptions
            .ReferenceHandler = ReferenceHandler.IgnoreCycles) //queda con el filtro Global para capturar los errores en el Logger
            .AddNewtonsoftJson(); //Para poder usar el HTTP PATCH

            //Conexion a la base de datos con EF
            Services.AddDbContext<ApplicationDbContext>(option => option
            .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //Para Comenzar a Usar Token de Sweguridad Bearer
            Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            //Para Comenzar a usar el Identity de NetCore
            Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            Services.AddEndpointsApiExplorer();
            Services.AddSwaggerGen(c =>
            {
                //Este es para manejar Versiones de los API y consumirlos Ordenado en el Swagger
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "WebAPIAutores", 
                    Version = "v1",
                    Description = "Este es un Web API para Trabajar Autores",
                    Contact = new OpenApiContact
                    { 
                        Email = "merchanhebert@gmil.com",  
                        Name = "Hebert Merchan",
                        Url = new Uri("https://spi.nexxtplanet.net")
                    },
                    License = new OpenApiLicense
                    { 
                        Name = "License",
                    }

                });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPIAutores", Version = "v2" });

                //Para manejar los comentarios a los API en Swagger
                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
                c.IncludeXmlComments(rutaXML);


                c.OperationFilter<AgregarParametrosHATEOAS>();
                //Para hacer el filtro por Attribute para desde Swagger
                c.OperationFilter<AgregarParametroXVersion>();

                //Configuracion de Swagger para enviar Token de Seguridad
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{ }
                    }
                });
            });

            //Para Activar el servicio del sistema de AutoMapper
            //Se implementara el uso en la Carpeta Utilidades
            Services.AddAutoMapper(typeof(Startup));

            //terminado
            //AddTransient = No importa que sea en lo mismo contexto Http, se va generar una nueva instancia
            //AddScoped = dentor un mismo contexto se asigna la misma instancia.  
            //AddSingleton =  es la misma instancia para x cantidad de contextos o peticiones

            //Vamos a crear Accesos basado en Claims, parecido a usar Roles en MVC
            Services.AddAuthorization(opcion =>
            {
                opcion.AddPolicy("EsAdmin", po => po.RequireClaim("EsAdmin"));
                opcion.AddPolicy("EsVendedor", po => po.RequireClaim("EsVendedor"));
            });


            Services.AddTransient<GenerardorEnlaces>();
            //Services.AddTransient<HATEOASAutorFilterAttribute>();
            //Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


            //Implementacion de CORS para peticiones Cruzadas de navegadores a nuestra API
            //O de Blazor, Angular, React a nuestro API
            Services.AddCors(c =>
            {
                c.AddDefaultPolicy(builder =>
                {
                    //WithOrigins("https://www.apirequest.io") se puede especificar las URL que podran acceder al CORS
                    builder.WithOrigins()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(new string[] { "cantidadTotalRegistros"});
                });
            });

            //Para crear Sistemas de Encriptacion
            Services.AddDataProtection();

            //Para implementar el sistema de HASH
            Services.AddTransient<HashService>();


            Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Para usar un Middleware que Configuramos en la Carpeta Middlewares
            //app.UseMiddleware<LoguerarRespuestaHTTPMiddleware>();
            app.UseLoguearRespuestaHTTP();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebAPIAutores v2");
                });
            }

            app.UseHttpsRedirection();

            //Se adiciona
            app.UseRouting();

            //Complemento para Usea CORS
            app.UseCors();


            app.UseAuthorization();

            //se adiciona
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
