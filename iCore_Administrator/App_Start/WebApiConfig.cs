﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace iCore_Administrator
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //    );

            config.Routes.MapHttpRoute(
                name: "Request",
                routeTemplate: "api/Request/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            config.Routes.MapHttpRoute(
                name: "Result",
                routeTemplate: "api/Result/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            config.Routes.MapHttpRoute(
                name: "DocBuilder",
                routeTemplate: "api/DocBuilder/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );
        }
    }
}
