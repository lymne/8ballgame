﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ConsoleApplication.Helper
{
    /// <summary>
    /// Web帮助类
    /// </summary>
    public class WebHelper
    {
        //浏览器列表
        private static string[] _browserlist = new string[] { "ie", "chrome", "mozilla", "netscape", "firefox", "opera", "konqueror" };
        //搜索引擎列表
        private static string[] _searchenginelist = new string[] { "baidu", "google", "360", "sogou", "bing", "msn", "sohu", "soso", "sina", "163", "yahoo", "jikeu" };
        //meta正则表达式
        private static Regex _metaregex = new Regex("<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        #region 编码

        /// <summary>
        /// HTML解码
        /// </summary>
        /// <returns></returns>
        public static string HtmlDecode(string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        /// <summary>
        /// HTML编码
        /// </summary>
        /// <returns></returns>
        public static string HtmlEncode(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        /// <summary>
        /// URL解码
        /// </summary>
        /// <returns></returns>
        public static string UrlDecode(string s)
        {
            return HttpUtility.UrlDecode(s);
        }

        /// <summary>
        /// URL编码
        /// </summary>
        /// <returns></returns>
        public static string UrlEncode(string s)
        {
            return HttpUtility.UrlEncode(s);
        }

        #endregion



        #region 客户端信息

        /// <summary>
        /// 是否是get请求
        /// </summary>
        /// <returns></returns>
        public static bool IsGet()
        {
            return HttpContext.Current.Request.HttpMethod == "GET";
        }

        /// <summary>
        /// 是否是post请求
        /// </summary>
        /// <returns></returns>
        public static bool IsPost()
        {
            return HttpContext.Current.Request.HttpMethod == "POST";
        }

        /// <summary>
        /// 是否是Ajax请求
        /// </summary>
        /// <returns></returns>
        public static bool IsAjax()
        {
            return HttpContext.Current.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// 是否请求的类型是xml
        /// </summary>
        /// <returns></returns>
        public static bool IsAcceptXml()
        {
            return HttpContext.Current.Request.Headers["Accept"] == "application/xml";
        }

        /// <summary>
        /// 是否请求的类型是json
        /// </summary>
        /// <returns></returns>
        public static bool IsAcceptJson()
        {
            return HttpContext.Current.Request.Headers["Accept"] == "application/json";
        }

        /// <summary>
        /// 获得上次请求的url
        /// </summary>
        /// <returns></returns>
        public static string GetUrlReferrer()
        {
            Uri uri = HttpContext.Current.Request.UrlReferrer;
            if (uri == null)
                return string.Empty;

            return uri.ToString();
        }

        /// <summary>
        /// 获得请求的主机部分
        /// </summary>
        /// <returns></returns>
        public static string GetHost()
        {
            return HttpContext.Current.Request.Url.Host;
        }

        /// <summary>
        /// 获得请求的url
        /// </summary>
        /// <returns></returns>
        public static string GetUrl()
        {
            return HttpContext.Current.Request.Url.ToString();
        }



        /// <summary>
        /// 获得请求的原始url
        /// </summary>
        /// <returns></returns>
        public static string GetRawUrl()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        /// <summary>
        /// 获得请求的ip
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            string ip = string.Empty;
            if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            else
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();

            //if (string.IsNullOrEmpty(ip) || !ValidateHelper.IsIP(ip))
            //    ip = "127.0.0.1";
            return ip;
        }

        /// <summary>
        /// 获得请求的浏览器类型
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserType()
        {
            string type = HttpContext.Current.Request.Browser.Type;
            if (string.IsNullOrEmpty(type) || type == "unknown")
                return "未知";

            return type.ToLower();
        }

        /// <summary>
        /// 获得请求的浏览器名称
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserName()
        {
            string name = HttpContext.Current.Request.Browser.Browser;
            if (string.IsNullOrEmpty(name) || name == "unknown")
                return "未知";

            return name.ToLower();
        }

        /// <summary>
        /// 获得请求的浏览器版本
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserVersion()
        {
            string version = HttpContext.Current.Request.Browser.Version;
            if (string.IsNullOrEmpty(version) || version == "unknown")
                return "未知";

            return version;
        }

        /// <summary>
        /// 获得请求客户端的操作系统类型
        /// </summary>
        /// <returns></returns>
        public static string GetOSType()
        {
            string userAgent = HttpContext.Current.Request.UserAgent;
            if (userAgent == null)
                return "未知";

            string type = null;
            if (userAgent.Contains("NT 6.1"))
                type = "Windows 7";
            else if (userAgent.Contains("NT 5.1"))
                type = "Windows XP";
            else if (userAgent.Contains("NT 6.2"))
                type = "Windows 8";
            else if (userAgent.Contains("android"))
                type = "Android";
            else if (userAgent.Contains("iphone"))
                type = "IPhone";
            else if (userAgent.Contains("Mac"))
                type = "Mac";
            else if (userAgent.Contains("NT 6.0"))
                type = "Windows Vista";
            else if (userAgent.Contains("NT 5.2"))
                type = "Windows 2003";
            else if (userAgent.Contains("NT 5.0"))
                type = "Windows 2000";
            else if (userAgent.Contains("98"))
                type = "Windows 98";
            else if (userAgent.Contains("95"))
                type = "Windows 95";
            else if (userAgent.Contains("Me"))
                type = "Windows Me";
            else if (userAgent.Contains("NT 4"))
                type = "Windows NT4";
            else if (userAgent.Contains("Unix"))
                type = "UNIX";
            else if (userAgent.Contains("Linux"))
                type = "Linux";
            else if (userAgent.Contains("SunOS"))
                type = "SunOS";
            else
                type = "未知";

            return type;
        }

        /// <summary>
        /// 获得请求客户端的操作系统名称
        /// </summary>
        /// <returns></returns>
        public static string GetOSName()
        {
            string name = HttpContext.Current.Request.Browser.Platform;
            if (string.IsNullOrEmpty(name))
                return "未知";

            return name;
        }

        /// <summary>
        /// 判断是否是浏览器请求
        /// </summary>
        /// <returns></returns>
        public static bool IsBrowser()
        {
            string name = GetBrowserName();
            foreach (string item in _browserlist)
            {
                if (name.Contains(item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 是否是移动设备请求
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile()
        {
            if (HttpContext.Current.Request.Browser.IsMobileDevice)
                return true;

            bool isTablet = false;
            if (bool.TryParse(HttpContext.Current.Request.Browser["IsTablet"], out isTablet) && isTablet)
                return true;

            return false;
        }

        /// <summary>
        /// 判断是否是搜索引擎爬虫请求
        /// </summary>
        /// <returns></returns>
        public static bool IsCrawler()
        {
            bool result = HttpContext.Current.Request.Browser.Crawler;
            if (!result)
            {
                string referrer = GetUrlReferrer();
                if (referrer.Length > 0)
                {
                    foreach (string item in _searchenginelist)
                    {
                        if (referrer.Contains(item))
                            return true;
                    }
                }
            }
            return result;
        }

        #endregion

        #region Http

        /// <summary>
        /// 获得参数列表
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static NameValueCollection GetParmList(string data)
        {
            NameValueCollection parmList = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(data))
            {
                int length = data.Length;
                for (int i = 0; i < length; i++)
                {
                    int startIndex = i;
                    int endIndex = -1;
                    while (i < length)
                    {
                        char c = data[i];
                        if (c == '=')
                        {
                            if (endIndex < 0)
                                endIndex = i;
                        }
                        else if (c == '&')
                        {
                            break;
                        }
                        i++;
                    }
                    string key;
                    string value;
                    if (endIndex >= 0)
                    {
                        key = data.Substring(startIndex, endIndex - startIndex);
                        value = data.Substring(endIndex + 1, (i - endIndex) - 1);
                    }
                    else
                    {
                        key = data.Substring(startIndex, i - startIndex);
                        value = string.Empty;
                    }
                    parmList[key] = value;
                    if ((i == (length - 1)) && (data[i] == '&'))
                        parmList[key] = string.Empty;
                }
            }
            return parmList;
        }

        /// <summary>
        /// 获得http请求数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">发送数据</param>
        /// <returns></returns>
        public static string GetRequestData(string url, string postData)
        {
            return GetRequestData(url, "post", postData);
        }

        /// <summary>
        /// 获得http请求数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="method">请求方式</param>
        /// <param name="postData">发送数据</param>
        /// <returns></returns>
        public static string GetRequestData(string url, string method, string postData)
        {
            return GetRequestData(url, method, postData, Encoding.UTF8);
        }

        /// <summary>
        /// 获得http请求数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="method">请求方式</param>
        /// <param name="postData">发送数据</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetRequestData(string url, string method, string postData, Encoding encoding)
        {
            return GetRequestData(url, method, postData, encoding, 20000);
        }

        /// <summary>
        /// 获得http请求数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="method">请求方式</param>
        /// <param name="postData">发送数据</param>
        /// <param name="encoding">编码</param>
        /// <param name="timeout">超时值</param>
        /// <returns></returns>
        public static string GetRequestData(string url, string method, string postData, Encoding encoding, int timeout)
        {
            if (!(url.Contains("http://") || url.Contains("https://")))
                url = "http://" + url;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.Trim().ToLower();
            request.Timeout = timeout;
            request.AllowAutoRedirect = true;
            request.ContentType = "text/html";
            request.Accept = "text/html, application/xhtml+xml, */*,zh-CN";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

            try
            {
                if (!string.IsNullOrEmpty(postData) && request.Method == "post")
                {
                    byte[] buffer = encoding.GetBytes(postData);
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (encoding == null)
                    {
                        MemoryStream stream = new MemoryStream();
                        if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                            new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(stream, 10240);
                        else
                            response.GetResponseStream().CopyTo(stream, 10240);

                        byte[] RawResponse = stream.ToArray();
                        string temp = Encoding.Default.GetString(RawResponse, 0, RawResponse.Length);
                        Match meta = _metaregex.Match(temp);
                        string charter = (meta.Groups.Count > 2) ? meta.Groups[2].Value : string.Empty;
                        charter = charter.Replace("\"", string.Empty).Replace("'", string.Empty).Replace(";", string.Empty);
                        if (charter.Length > 0)
                        {
                            charter = charter.ToLower().Replace("iso-8859-1", "gbk");
                            encoding = Encoding.GetEncoding(charter);
                        }
                        else
                        {
                            if (response.CharacterSet.ToLower().Trim() == "iso-8859-1")
                            {
                                encoding = Encoding.GetEncoding("gbk");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(response.CharacterSet.Trim()))
                                {
                                    encoding = Encoding.UTF8;
                                }
                                else
                                {
                                    encoding = Encoding.GetEncoding(response.CharacterSet);
                                }
                            }
                        }
                        return encoding.GetString(RawResponse);
                    }
                    else
                    {
                        StreamReader reader = null;
                        if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress), encoding))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                        else
                        {
                            using (reader = new StreamReader(response.GetResponseStream(), encoding))
                            {
                                try
                                {
                                    return reader.ReadToEnd();
                                }
                                catch (Exception ex)
                                {
                                    return "close";
                                }

                            }
                        }
                    }
                }

            }
            catch (WebException ex)
            {
                return "error";
            }
        }

        #endregion

        #region .NET

        /// <summary>
        /// 获得当前应用程序的信任级别
        /// </summary>
        /// <returns></returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            AspNetHostingPermissionLevel trustLevel = AspNetHostingPermissionLevel.None;
            //权限列表
            AspNetHostingPermissionLevel[] levelList = new AspNetHostingPermissionLevel[] {
                                                                                            AspNetHostingPermissionLevel.Unrestricted,
                                                                                            AspNetHostingPermissionLevel.High,
                                                                                            AspNetHostingPermissionLevel.Medium,
                                                                                            AspNetHostingPermissionLevel.Low,
                                                                                            AspNetHostingPermissionLevel.Minimal 
                                                                                            };

            foreach (AspNetHostingPermissionLevel level in levelList)
            {
                try
                {
                    //通过执行Demand方法检测是否抛出SecurityException异常来设置当前应用程序的信任级别
                    new AspNetHostingPermission(level).Demand();
                    trustLevel = level;
                    break;
                }
                catch (SecurityException ex)
                {
                    continue;
                }
            }
            return trustLevel;
        }


        /// <summary>
        /// 重启应用程序
        /// </summary>
        //public static void RestartAppDomain()
        //{
        //    if (GetTrustLevel() > AspNetHostingPermissionLevel.Medium)//如果当前信任级别大于Medium，则通过卸载应用程序域的方式重启
        //    {
        //        HttpRuntime.UnloadAppDomain();
        //        TryWriteGlobalAsax();
        //    }
        //    else//通过修改web.config方式重启应用程序
        //    {
        //        bool success = TryWriteWebConfig();
        //        if (!success)
        //        {
        //            throw new Exception("修改web.config文件重启应用程序");
        //        }

        //        success = TryWriteGlobalAsax();
        //        if (!success)
        //        {
        //            throw new Exception("修改global.asax文件重启应用程序");
        //        }
        //    }

        //}

        #endregion
    }
}
