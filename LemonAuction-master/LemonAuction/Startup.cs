using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using LemonAuction.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LemonAuction
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
            services.Configure<CookiePolicyOptions>(options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
                });

            // var connection = "Data Source=LemonContext.db";
            // services.AddDbContext<Models.LemonContext>(options => options.UseSqlite(connection));
            // 
            services.AddDbContext<Models.LemonContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("LemonAuctionDB") + Environment.UserName);

            }).AddEntityFrameworkNpgsql()
               .BuildServiceProvider();
            services.AddIdentity<Identity.LemonUser, IdentityRole>()
             .AddEntityFrameworkStores<Models.LemonContext>()
             .AddDefaultTokenProviders();
            // services.AddDefaultIdentity<Identity.LemonUser>()
            //      .AddEntityFrameworkStores<Models.LemonContext>();
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;



            });

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Guid"]));



            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        LifetimeValidator = (before, expires, token, param) =>
                        {
                            return expires > DateTime.UtcNow;
                        },
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateActor = false,
                        ValidateLifetime = false,
                        IssuerSigningKey = securityKey
                    };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        //If for bidding hub
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs/bidding")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                });
            //         services.ConfigureApplicationCookie(options =>
            // {
            //     // Cookie settings
            //     options.Cookie.HttpOnly = true;
            //     options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

            //     options.LoginPath = "/Identity/Account/Login";
            //     options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            //     options.SlidingExpiration = true;
            // });

            services.AddTransient<IBiddingService, BiddingService>();
            services.AddMvc()
              .AddJsonOptions(
                    options =>  {
                        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                        options.SerializerSettings.Converters =  new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } };
                        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    }
                    
              )
              .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
            {
                options.SuppressConsumesConstraintForFormFileParameters = true;
                options.SuppressInferBindingSourcesForParameters = true;
                options.SuppressModelStateInvalidFilter = true;
            });
            services.AddSignalR()
                .AddHubOptions<Hubs.BiddingHub>(options =>
                {
                    options.EnableDetailedErrors = true;
                });

            //services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                // {
                //     HotModuleReplacement = true
                // });
            }
            else
            {
                // app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            // TODO: Fix genric stuff with hub and strongly typed stuff
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseSignalR(route =>
            {
                route.MapHub<LemonAuction.Hubs.BiddingHub>("/hubs/bidding");
            });
            // app.Use(async (context, next) =>
            // {
            //     var hubContext = context.RequestServices
            //         .GetRequiredService<IHubContext<Hubs.BiddingHub, Hubs.IBiddingClient>>();
            //     ObjectFactory
            // });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                // routes.MapRoute("")
                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });

            });
        }
    }
}
