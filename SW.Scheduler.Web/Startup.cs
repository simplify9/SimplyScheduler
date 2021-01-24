using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using SW.Bus;
using SW.PrimitiveTypes;

namespace SW.Scheduler.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            
            
            services.AddSingleton<SchedulePublisher>();
            var schedulerOptions = new SchedulerOptions();
            Configuration.GetSection(SchedulerOptions.ConfigurationSection).Bind(schedulerOptions);
            services.AddSingleton(schedulerOptions);

            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));

            //var quartzOptions = new QuartzOptions();
            
            //services.AddSingleton(quartzOptions);
            //
            var connectionString = Configuration.GetConnectionString("SchedulerDb");


            services.AddQuartz(q =>
            {
                // we could leave DI configuration intact and then jobs need to have public no-arg constructor
                // the MS DI is expected to produce transient job instances

                // q.UseMicrosoftDependencyInjectionScopedJobFactory(options =>
                // {
                //     // if we don't have the job in DI, allow fallback to configure via default constructor
                //     options.AllowDefaultConstructor = true;
                //
                //     // set to true if you want to inject scoped services like Entity Framework's DbContext
                //     options.CreateScope = false;
                // });

                // these are the defaults
                q.UseSimpleTypeLoader();

                q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = schedulerOptions.MaxConcurrency; });

                // example of persistent job store using JSON serializer as an example

                q.UsePersistentStore(s =>
                {
                    s.UseProperties = true;
                    s.RetryInterval = TimeSpan.FromSeconds(15);
                    
                    switch (schedulerOptions.DatabaseType)
                    {
                        case SchedulerOptions.DatabaseTypeSqlite:
                            s.UseSQLite(sqlite => { sqlite.ConnectionString = connectionString; });
                            break;
                        case SchedulerOptions.DatabaseTypeMsSql:
                            s.UseSqlServer(sqlServer => { sqlServer.ConnectionString = connectionString; });
                            break;
                        case SchedulerOptions.DatabaseTypePgSql:
                            s.UsePostgres(postgres =>
                            {
                                postgres.ConnectionString = connectionString;
                                
                            });
                            break;
                        case SchedulerOptions.DatabaseTypeMySql:
                            s.UseMySql(mysql => { mysql.ConnectionString = connectionString; });
                            break;
                        default:
                            throw new Exception("Invalid database type in configuration");
                    }

                    s.UseJsonSerializer();
                });
            });
            
            // services.AddQuartzServer(options =>
            // {
            //     
            //     // when shutting down we want jobs to complete gracefully
            //     options.WaitForJobsToComplete = true;
            // });
            //
            
            services.AddHostedService<QuartzHostedService>();

            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            
            var identity = new GenericIdentity("scheduler");
            var principal = new ClaimsPrincipal(identity);
            var requestContext = new RequestContext();
            requestContext.Set(principal);

            services.AddSingleton(requestContext);
            services.AddBus();
            services.AddBusPublish();
            services.AddBusConsume();


        }

        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
            });
            
            
            app.UseCrystalQuartz(
                () => StdSchedulerFactory.GetDefaultScheduler().Result,
                new CrystalQuartzOptions
                {
                    Path = "/scheduler"
                });
        }
    }
}