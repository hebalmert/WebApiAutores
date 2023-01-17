using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebApiAutores
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(option => 
            option.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //services.AddControllersWithViews();
            //Se agrega el JsonSerializerOption para evitar cualquier consulta Ciclica.
            services.AddControllers()
                .AddJsonOptions(x=> x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
            
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            //Inplementacion de AutoMapper
            services.AddAutoMapper(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Lineas para mostrar un Mensaje y parar la ejecucion completa de la aplicacion
            //sirve mientras se hace algun cambio
            app.Map("/Ruta1", app =>  //asi, si piden esta ruta /Ruta1 hacer enviar el mensaje la Tuberia
            {
                app.Run(async contexto =>  //asi solo capturaa cualquier peticion y muestra el mensaje
                {
                    await contexto.Response.WriteAsync("Estoy Interceptando la Tuberia");
                });
            });

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
