using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Extensions
{
    public static class FileHostingHelpers
    {
        public static readonly string ServerUrl = ConfigurationManager.AppSettings["FileHosting.ServerUrl"].ToString();
        public static readonly string ImagePath = ConfigurationManager.AppSettings["SiteName"].ToString() + "/" + ConfigurationManager.AppSettings["FileHosting.ImagePath"].ToString();
        public static readonly string Mp3Path = ConfigurationManager.AppSettings["SiteName"].ToString() + "/" + ConfigurationManager.AppSettings["FileHosting.Mp3Path"].ToString();
        public static readonly string TabPath = ConfigurationManager.AppSettings["SiteName"].ToString() + "/" + ConfigurationManager.AppSettings["FileHosting.TabPath"].ToString();
        public static readonly string LyricPath = ConfigurationManager.AppSettings["SiteName"].ToString() + "/" + ConfigurationManager.AppSettings["FileHosting.LyricPath"].ToString();

        public static MvcHtmlString FileHostingImagePath(this HtmlHelper html)
        {
            string path = ServerUrl + ImagePath;

            return new MvcHtmlString(path.ToString());
        }

        public static MvcHtmlString FileHostingImagePath(this HtmlHelper html, string fileName)
        {
            string path = ServerUrl + ImagePath + Path.DirectorySeparatorChar + fileName;

            return new MvcHtmlString(path.ToString());
        }

        public static MvcHtmlString FileHostingMp3Path(this HtmlHelper html)
        {
            string path = ServerUrl + Mp3Path;

            return new MvcHtmlString(path.ToString());
        }

        public static MvcHtmlString FileHostingMp3Path(this HtmlHelper html, string fileName)
        {
            string path = ServerUrl + Mp3Path + "/" + fileName;

            return new MvcHtmlString(path.ToString());
        }

        public static MvcHtmlString FileHostingRootPath(this HtmlHelper html, string path)
        {
            path = ServerUrl + path;

            return new MvcHtmlString(path.ToString());
        }

        public static MvcHtmlString FileHostingRootPath(this HtmlHelper html, string path, string fileName)
        {
            path = ServerUrl + path + fileName;

            return new MvcHtmlString(path.ToString());
        }

    }
}