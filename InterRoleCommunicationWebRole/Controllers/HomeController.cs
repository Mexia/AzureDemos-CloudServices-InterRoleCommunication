using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InterRoleCommunicationWebRole.Controllers
{
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;

    using InterRoleCommunicationWebRole.Models;

    using Microsoft.WindowsAzure.ServiceRuntime;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Index(SendMessageModel model)
        {
            ViewBag.Message = "Send message page";

            try
            {
                var targetWorkerRoleHostName = RoleEnvironment.GetConfigurationSettingValue("TargetWorkerRoleHostName");

                var request = WebRequest.Create(targetWorkerRoleHostName);
                request.Method = "POST";

                string postData = model.MessageContent;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                var dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                var response = request.GetResponse();
                var statusDescription = ((HttpWebResponse)response).StatusDescription;
                dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);

                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();

                ViewBag.Message = statusDescription;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            return View("SubmittedMessage");
        }
    }
}
