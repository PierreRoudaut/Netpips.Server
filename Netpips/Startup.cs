using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using Coravel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Netpips.Core;
using Netpips.Core.Extensions;
using Netpips.Core.Model;
using Netpips.Core.Service;
using Netpips.Core.Settings;
using Netpips.Download.Authorization;
using Netpips.Download.DownloadMethod;
using Netpips.Download.DownloadMethod.DirectDownload;
using Netpips.Download.DownloadMethod.PeerToPeer;
using Netpips.Download.Event;
using Netpips.Download.Job;
using Netpips.Download.Model;
using Netpips.Download.Service;
using Netpips.Identity.Authorization;
using Netpips.Identity.Model;
using Netpips.Identity.Service;
using Netpips.Media.Model;
using Netpips.Media.Service;
using Netpips.Search.Service;
using Netpips.Subscriptions.Job;
using Netpips.Subscriptions.Model;
using Netpips.Subscriptions.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

namespace Netpips
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private void SetupLogger(string logFolderPath)
        {
            const string LogFilename = "{Date}.netpips.log";
            var logPath = Path.Combine(logFolderPath, LogFilename);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.RollingFile(
                    pathFormat: logPath,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .CreateLogger();


            AppDomain.CurrentDomain.FirstChanceException +=
                (source, e) =>
                {
                    var msg = $"[{source}] [{AppDomain.CurrentDomain.FriendlyName}] [FirstChanceException]: {e.Exception.Message}";
                    Log.Logger.Error(msg);
                };
        }

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", reloadOnChange: true, optional: false)
                .AddEnvironmentVariables("NETPIPS_");
            Configuration = builder.Build();

            var netpipsAppSettings = Configuration.GetSection("Netpips").Get<NetpipsSettings>();

            AppAsserter.AssertCliDependencies();
            AppAsserter.AssertSettings(netpipsAppSettings);

            SetupLogger(netpipsAppSettings.LogsPath);

            // removes default claim mapping
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            services.AddCors();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration.GetSection("Auth")["JwtIssuer"],
                        ValidAudience = Configuration.GetSection("Auth")["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("Auth")["JwtKey"]))
                    };
                });


            // sql server db
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            // memory cache
            services.AddMemoryCache();

            // Mvc
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(
                options =>
                    {
                        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                        options.SerializerSettings.Converters = new List<JsonConverter> { new StringEnumConverter() };
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    });


            // policies
            services.AddAuthorization(options =>
                {
                    // cancel
                    options.AddPolicy(DownloadItemPolicies.CancelPolicy,
                        policy => policy.AddRequirements(
                            new ItemOwnershipRequirement(),
                            new ItemDownloadingRequirement()));

                    // archive
                    options.AddPolicy(
                        DownloadItemPolicies.ArchivePolicy,
                        policy => policy.AddRequirements(
                            new ItemOwnershipRequirement(),
                            new ItemCanceledOrCompletedRequirement()));

                    // torrent done
                    options.AddPolicy(
                        DownloadItemPolicies.TorrentDonePolicy,
                        policy => policy.AddRequirements(
                            new ItemDownloadingRequirement()));

                    // admin or higher
                    options.AddPolicy(IdentityPolicies.AdminOrHigherPolicy,
                        policy =>
                            {
                                policy.RequireRole(Enum.GetValues(typeof(Role))
                                    .Cast<Role>()
                                    .Where(r => r > Role.User)
                                    .Select(r => r.ToString()));
                            });
                });

            // swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Netpips",
                    Version = "v1"
                });
            });

            // scheduler
            services.AddScheduler();

            // events
            services.AddEvents();


            // services
            services.AddScoped<IDownloadItemService, DownloadItemService>();
            services.AddScoped<IMediaLibraryService, MediaLibraryService>();
            services.AddTransient<IDownloadItemRepository, DownloadItemRepository>();
            services.AddScoped<IDownloadMethod, DirectDownloadMethod>();
            services.AddScoped<IDownloadMethod, P2PDownloadMethod>();
            services.AddTransient<IMediaLibraryMover, MediaLibraryMover>();
            services.AddTransient<IFilebotService, FilebotService>();
            services.AddTransient<IMediaInfoService, MediaInfoService>();

            services.AddTransient<ITorrentDaemonService, TransmissionRemoteDaemonService>();
            services.AddTransient<IAria2CService, Aria2CService>();
            services.AddScoped<IControllerHelperService, ControllerHelperService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IArchiveExtractorService, ArchiveExtractorService>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IMediaItemRepository, MediaItemRepository>();
            services.AddScoped<ISmtpService, GmailSmtpClient>();
            services.AddScoped<IUserAdministrationService, UserAdministrationService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IShowRssGlobalSubscriptionService, ShowRssGlobalSubscriptionService>();

            services.AddScoped<IShowRssItemRepository, ShowRssItemRepository>();
            services.AddScoped<ITvShowSubscriptionRepository, TvShowSubscriptionRepository>();

            //events listeners
            services.AddTransient<NotifyUsersItemStarted>();
            services.AddTransient<ProcessDownloadItem>();
            services.AddTransient<SendItemCompletedEmail>();

            //jobs
            services.AddScoped<ShowRssFeedConsumerJob>();
            services.AddScoped<ShowRssFeedSyncJob>();
            services.AddScoped<ArchiveDownloadItemsJob>();

            //scrappers
            services.AddScoped<ITorrentSearchScrapper, _1337xScrapper>();
            services.AddScoped<ITorrentDetailScrapper, _1337xScrapper>();
            services.AddScoped<ITorrentSearchScrapper, BitTorrentAmScrapper>();
            services.AddScoped<ITorrentDetailScrapper, BitTorrentAmScrapper>();

            //policies
            services.AddSingleton<IAuthorizationHandler, ItemOwnershipAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ArchiveItemAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ItemDownloadingAuthorizationHandler>();

            //appSettings
            services.Configure<NetpipsSettings>(Configuration.GetSection("Netpips"));
            services.Configure<ShowRssSettings>(Configuration.GetSection("ShowRss"));
            services.Configure<AuthSettings>(Configuration.GetSection("Auth"));
            services.Configure<GmailMailerAccountSettings>(Configuration.GetSection("GmailMailerAccount"));
            services.Configure<DirectDownloadSettings>(Configuration.GetSection("DirectDownload"));
            services.Configure<TransmissionSettings>(Configuration.GetSection("Transmission"));
        }

        // To configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, IOptions<NetpipsSettings> settings)
        {
            // Serve medialibrary folder as static
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(settings.Value.MediaLibraryPath),
                RequestPath = "/api/file",
                OnPrepareResponse = ctx =>
                    {
                        Log.Logger.Information("[SERVING] " + ctx.File.Name);
                        ctx.Context.Response.Headers.Append("Content-Disposition", "attachment; filename=" + ctx.File.Name.RemoveDiacritics().Quoted());
                    }
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = httpReq.Host.Value);
                c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api/swagger";
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Netpips API v1");
            });

            // CORS
            app.UseCors(builder => builder
                .WithOrigins(Configuration.GetValue<string>("Netpips:Domain"))
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseAuthentication();
            app.UseRequestLocalization(builder => { builder.DefaultRequestCulture = new RequestCulture(Program.EnUsCulture); });
            app.UseMvc();

            // coravel scheduler
            var provider = app.ApplicationServices;
            provider.UseScheduler(
                scheduler =>
                    {
                        if (!env.IsDevelopment())
                        {
                            // Start download from synced feed
                            scheduler
                                .Schedule<ShowRssFeedConsumerJob>()
                                .EveryThirtyMinutes()
                                .PreventOverlapping(nameof(ShowRssFeedConsumerJob));

                            // Sync items from feed
                            scheduler
                                .Schedule<ShowRssFeedSyncJob>()
                                .Hourly()
                                .PreventOverlapping(nameof(ShowRssFeedSyncJob));

                            // Archive passed download items
                            scheduler
                                .Schedule<ArchiveDownloadItemsJob>()
                                .DailyAt(16, 0)
                                .PreventOverlapping(nameof(ArchiveDownloadItemsJob));

                            // Download missing subtitles for subscription items
                            scheduler
                                .Schedule<GetMissingSubtitlesJob>()
                                .Hourly()
                                .PreventOverlapping(nameof(GetMissingSubtitlesJob));
                        }

                    });

            // coravel events
            var registration = provider.ConfigureEvents();
            registration
                .Register<ItemStarted>()
                .Subscribe<NotifyUsersItemStarted>();
            registration
                .Register<ItemDownloaded>()
                .Subscribe<ProcessDownloadItem>()
                .Subscribe<SendItemCompletedEmail>();
        }
    }
}