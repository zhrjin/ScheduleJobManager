﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using log4net.Config;
using Quartz;
using Utility;

namespace JobHostSite
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class WebApiApplication : System.Web.HttpApplication
	{
		IScheduler scheduler;
		protected void Application_Start(object sender, EventArgs e) {
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			XmlConfigurator.Configure();

			//初始化quart.net 
			NameValueCollection config = (NameValueCollection)ConfigurationManager.GetSection("quartz");
			scheduler = QuartzNetHelper.Initialize(config);
			scheduler.Start();

			Log4NetHelper.WriteInfo("JobHostSite is started.");
		}

		protected void Application_Error(object sender, EventArgs e) {
			Log4NetHelper.WriteExcepetion(Server.GetLastError());
		}

		protected void Application_End(object sender, EventArgs e) {
			if ( scheduler != null ) {
				scheduler.Shutdown(true);
			}
			Log4NetHelper.WriteInfo("网站关闭，系统资源回收。");
			// 防止IIS资源回收导致服务终止
			try {
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["JobHostSite"]);
				HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
				string desc = rsp.StatusDescription;
				Log4NetHelper.WriteInfo(desc);
			}
			catch ( Exception ex ) {
				Log4NetHelper.WriteExcepetion(ex);
			}
		}
	}
}