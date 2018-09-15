using Flurl.Http;
using Hangfire.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hangfire.Job
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
            services.AddHangfire(x =>
            {
                //x.UseMongoStorage("mongodb://118.24.27.231:27017", "hangfire");
                x.UseRedisStorage("118.24.27.231:6379,password=sun940622", new RedisStorageOptions
                {
                    Db = 1
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            // /addjob?name=test1&method=get&url=https%3a%2f%2fwww.baidu.com%2f&assert=html&cron=1 1 1 * *
            app.Map("/addjob", r => r.Run(c =>
            {
                var name = c.Request.Query["name"].ToString();
                var url = c.Request.Query["url"].ToString();
                var method = c.Request.Query["method"].ToString().ToUpper();
                var body = c.Request.Query["body"].ToString();
                var assert = c.Request.Query["assert"].ToString();
                var cron = c.Request.Query["cron"].ToString();
                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(name))
                {
                    return c.Response.WriteAsync("��������");
                }
                if (string.IsNullOrWhiteSpace(body) && method == "POST")
                {
                    return c.Response.WriteAsync("method����");
                }
                if (!new[] { "POST", "GET" }.Contains(method))
                {
                    return c.Response.WriteAsync("method����");
                }
                var job = Common.Job.FromExpression<Jobs>(j => j.Send(name, url, method, body, assert));
                var manager = new RecurringJobManager(JobStorage.Current);
                try
                {
                    manager.AddOrUpdate(name, job, cron);
                }
                catch (Exception ex)
                {
                    return c.Response.WriteAsync($"          �쳣��{ ex.Message}\r\n          ��ջ��{ ex.StackTrace}");
                }
                return c.Response.WriteAsync("ok");
            }));

        }
    }

    class Jobs
    {
        public void Send(string name, string url, string method, string body, string assert)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(method))
            {
                new LogHelper().Write("warn", "��������");
                return;
            }
            if (string.IsNullOrWhiteSpace(body) && method == "POST")
            {
                new LogHelper().Write("warn", "method����");
                return;
            }

            try
            {
                var result = string.Empty;
                if (method == "POST")
                {
                    result = url
                        .PostUrlEncodedAsync(body)
                        .ReceiveString().Result;
                }
                else if (method == "GET")
                {
                    result = url
                        .GetStringAsync().Result;
                }

                if (result.Contains(assert))
                {
                    new LogHelper().Write("ok", result);
                    return;
                }
                new LogHelper().Write("error", $"assert��{assert}�쳣��{result}");
            }
            catch (Exception ex)
            {
                new LogHelper().Write("error", $"url��{assert}��body��{body}\r\n          �쳣��{ex.Message}\r\n          ��ջ��{ex.StackTrace}");
            }

        }
    }

    public class LogHelper
    {
        readonly string path = Environment.CurrentDirectory + "/" + DateTime.Now.ToString("yyyyMMdd") + ".log";
        public void Write(string title, string msg)
        {
            var content = $@"


========================{DateTime.Now.ToLongTimeString()}==========================
title : {title}
msg   : {msg}
========================END==========================";

            File.AppendAllLines(path, new List<string> { content });
        }
    }
}
