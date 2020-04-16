using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using myChatRoomZ.Data;
using myChatRoomZ.SignalRHub;
using Microsoft.AspNetCore.Identity;
using myChatRoomZ.Data.Models;
using Microsoft.IdentityModel.Tokens;
using myChatRoomZ.Services;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNet.Identity;

namespace myChatRoomZ
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }

        public IConfiguration Configuration { get; }

        //***********************************************************************************************************************
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //------------------------------------------------------------------------------------------------------------------
            services.AddDbContext<ChatRoomZContext>(cfg =>
            {
                cfg.UseSqlServer(_config.GetConnectionString("ChatRoomZConnectionString"));
            });

            //-----------------------------------------------------------------------------------------------------------------
            services.AddTransient<ChatRoomZSeeder>();
            services.AddScoped<IChatRoomZRepository, ChatRoomZRepository>();
            //------------------------------------------------------------------------------------------------------------------
            services.AddIdentity<ChatUser, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<ChatRoomZContext>()
            .AddDefaultTokenProviders();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            //------------------------------------------------------------------------------------------------------------------
            services.AddSignalR();
            services.AddSingleton<IChatGroupService, ChatGroupService>();
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            //------------------------------------------------------------------------------------------------------------------
            services.AddAuthentication()
                .AddCookie()
                .AddJwtBearer(cfg =>
                {
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = _config["Tokens:Issuer"],
                        ValidAudience = _config["Tokens:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Tokens:Key"]))
                    };
                });

            //--------------------------------------------------------------------------------------------------------
            //To avoid the MultiPartBodyLength error,
            services.Configure<FormOptions>(o => {
             o.ValueLengthLimit = int.MaxValue;
             o.MultipartBodyLengthLimit = int.MaxValue;
             o.MemoryBufferThreshold = int.MaxValue;
             });
            //--------------------------------------------------------------------------------------------------------
        }
        //***********************************************************************************************************************
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
                RequestPath = new PathString("/Resources")
            });


            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                //Route Configuration for SignalR Hub
                endpoints.MapHub<ChatHub>("/chatHub");
               
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });



        }
    }
}
