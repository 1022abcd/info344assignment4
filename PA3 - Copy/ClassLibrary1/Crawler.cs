using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClassLibrary1;
using HtmlAgilityPack;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using RobotsTxt;

namespace ClassLibrary1
{
    public class Crawler
    {

        public static string state;
        private static ConcurrentBag<string> disallowedUrl;
        private static int CpuUsage;
        private static int RamAvailable;
        private static HashSet<string> HtmlSet;
        private static Dictionary<Uri, Robots> robotsdisc;
        private static Queue<string> recentTen;

        public Crawler()
        {
            disallowedUrl = new ConcurrentBag<string>();
            robotsdisc = new Dictionary<Uri, Robots>();
            state = "Idle";
            HtmlSet = new HashSet<string>();
            recentTen = new Queue<string>();
        }
        public void CrawlUrl()
        {
            new Task(GetPerfCounters).Start();
            Thread.Sleep(100);
            CloudQueueMessage linkMessage = StorageManager.LinkQueue().GetMessage();
            if (linkMessage != null)
            {
                string stringifiedLink = linkMessage.AsString;

                // if the message is robots.txt
                if (stringifiedLink.EndsWith("robots.txt"))
                {
                    HandleRobotstxt(stringifiedLink);
                    stringifiedLink = "";
                    StorageManager.LinkQueue().DeleteMessage(linkMessage);
                }
                // if the message is url.xml
                else
                {
                    // if the link contains more xml links
                    if (stringifiedLink.Contains("-index"))
                    {
                        CrawlSiteMapIndex(stringifiedLink);
                    }
                    // if the link contains html links
                    else
                    {
                        CrawlSiteMap(stringifiedLink);
                    }
                    StorageManager.LinkQueue().DeleteMessage(linkMessage);

                    if (StorageManager.LinkQueue().GetMessage() == null)
                    {
                        state = "Crawling";
                    }
                }
            }

        }

        public void Start()
        {
            state = "Loading";
        }

        public void Stop()
        {
            state = "Idle";
        }

        public string GetState()
        {
            return state;
        }

        private void HandleRobotstxt(string message)
        {
            using (WebClient client = new WebClient())
            {
                StreamReader reader = new StreamReader(client.OpenRead(message));
                Uri mainUri = new Uri(message.Replace("/robots.txt", ""));
                while (!reader.EndOfStream)
                {
                    String line = reader.ReadLine();
                    if (line.Contains("Sitemap"))
                    {
                        if (line.Contains("cnn.com") || line.Contains("/nba"))
                        {
                            line = line.Replace("Sitemap: ", "");
                            StorageManager.LinkQueue().AddMessageAsync(new CloudQueueMessage(line));
                        }
                    }
                    // if the line starts with "Disallow"
                    else if (line.Contains("Disallow"))
                    {
                        line = line.Replace("Disallow: ", "");
                        string disallowedLink = mainUri.OriginalString + line;
                        disallowedUrl.Add(disallowedLink);
                    }
                }
            }
        }

        private void CrawlSiteMapIndex(string link)
        {
            string cnn = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement sitemap = XElement.Load(link);
            XName sitemaps = XName.Get("sitemap", cnn);
            XName loc = XName.Get("loc", cnn);
            XName time = XName.Get("lastmod", cnn);
            DateTime givendate = new DateTime(2017, 12, 1);
            DateTime publishDate;

            foreach (var sitemapElement in sitemap.Elements(sitemaps))
            {
                string locLink = sitemapElement.Element(loc).Value;
                publishDate = DateTime.Parse(sitemapElement.Element(time).Value);

                if (publishDate > givendate)
                {
                    StorageManager.LinkQueue().AddMessage(new CloudQueueMessage(locLink));
                }
            }
        }

        private void CrawlSiteMap(string link)
        {
            XElement whole = XElement.Load(link);
            XName url;
            XName loc;
            string selectLink = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XName lastmod = XName.Get("lastmod", "http://www.sitemaps.org/schemas/sitemap/0.9");
            XName news = XName.Get("news", "http://www.google.com/schemas/sitemaps-news/0.9");
            XName newsPublicationDate = XName.Get("publication_date", "http://www.google.com/schemas/sitemaps-news/0.9");
            XName video = XName.Get("video", "http://www.google.com/schemas/sitemap-video/1.1");
            XName videoPublicationDate = XName.Get("publication_date", "http://www.google.com/schemas/sitemap-video/1.1");
            DateTime givendate = new DateTime(2017, 12, 1);
            DateTime publishDate = new DateTime(1000, 01, 01);
            if (link.Contains("bleacherreport.com"))
            {
                selectLink = "http://www.google.com/schemas/sitemap/0.9";
                publishDate = DateTime.Today;
            }

            url = XName.Get("url", selectLink);
            loc = XName.Get("loc", selectLink);


            try
            {
                foreach (var urlElement in whole.Elements(url))
                {
                    string locElement = urlElement.Element(loc).Value;
                    if (urlElement.Element(news) != null)
                    {
                        publishDate = DateTime.Parse(urlElement.Element(news).Element(newsPublicationDate).Value);
                    }
                    else if (urlElement.Element(video) != null)
                    {
                        publishDate = DateTime.Parse(urlElement.Element(video).Element(videoPublicationDate).Value);
                    }
                    else if (urlElement.Element(lastmod) != null)
                    {
                        publishDate = DateTime.Parse(urlElement.Element(lastmod).Value);
                    }

                    if (publishDate != null)
                    {
                        if (publishDate > givendate)
                        {
                            if (!HtmlSet.Contains(locElement))
                            {
                                HtmlSet.Add(locElement);
                                StorageManager.HTMLQueue().AddMessage(new CloudQueueMessage(locElement));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation(ex.Message);
            }

        }

        public void GetHTMLData()
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument webpage = new HtmlDocument();
            new Task(GetPerfCounters).Start();
            Thread.Sleep(100);
            CloudQueueMessage htmllink = StorageManager.HTMLQueue().GetMessage();
            string url = htmllink.AsString;
            if (!IsDisallow(url) && htmllink != null)
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        webpage.LoadHtml(client.DownloadString(url));
                        DateTime pubdlication;
                        Uri currentUri = new Uri(url);
                        HtmlNode pubdateHTML = webpage.DocumentNode.SelectSingleNode("//head/meta[@name='lastmod']");
                        if (pubdateHTML != null)
                        {
                            pubdlication = DateTime.Parse(pubdateHTML.Attributes["content"].Value);
                        }
                        else
                        {
                            pubdlication = DateTime.Today;
                        }
                        UpdateRecentUrl(url);
                        string title = webpage.DocumentNode.SelectSingleNode("//head/title").InnerText ?? "";
                        foreach(string word in CleanUpTitle(title))
                        {
                                TableOperation insertOp = TableOperation.InsertOrReplace(new PageEntity(word, title, url, pubdlication));
                                StorageManager.GetTable().Execute(insertOp);
                        }
                        HtmlNodeCollection href = webpage.DocumentNode.SelectNodes("//a[@href]");
                        if (href != null)
                        {
                            foreach (HtmlNode linkNode in href)
                            {
                                string templink = linkNode.Attributes["href"].Value;
                                if (templink.StartsWith("//"))
                                {
                                    templink = "http:" + templink;
                                }
                                else if (templink.StartsWith("/"))
                                {
                                    templink = "http://" + currentUri.Host + templink;
                                }
                                if (!HtmlSet.Contains(templink))
                                {
                                    HtmlSet.Add(templink);
                                    StorageManager.HTMLQueue().AddMessageAsync(new CloudQueueMessage(templink));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!HtmlSet.Contains(htmllink.AsString))
                        {
                            HtmlSet.Add(htmllink.AsString);
                            TableOperation insertOp = TableOperation.InsertOrReplace(new ErrorMessage(htmllink.AsString, e.Message));
                            StorageManager.ErrorTable().Execute(insertOp);
                        }
                    }
                }
            }
            StorageManager.HTMLQueue().DeleteMessage(htmllink);
        }

        private HashSet<string> CleanUpTitle(string title)
        {
            string returnTitle = "";
            string cnn = " - CNN";
            string travel = " | CNN Travel";
            string style = " - CNN Style";
            string video = " - CNN Video";
            string politics = " - CNNPolitics";
            string bleacher = " | Bleacher Report";
            string specialreports = " - Special Reports from CNN.com";
            if (title.EndsWith("CNN"))
            {
                if (title.Contains(travel))
                {
                    returnTitle = title.Replace(travel, "");
                }
                else if (title.Contains(style))
                {
                    returnTitle = title.Replace(style, "");
                }
                else if (title.Contains(video))
                {
                    returnTitle = title.Replace(video, "");
                }
                else if (title.Contains(politics))
                {
                    returnTitle = title.Replace(politics, "");
                }
                else if (title.Contains(bleacher))
                {
                    returnTitle = title.Replace(bleacher, "");
                }
                else if (title.Contains(specialreports))
                {
                    returnTitle = title.Replace(specialreports, "");
                }
                else
                {
                    returnTitle = title.Replace(cnn, "");
                }
            } else
            {
                returnTitle = title;
            }
            HashSet<string> set = new HashSet<string>(returnTitle.ToLower().Split
                (new char[] {'"', '+', ' ', '&', '.', ':', '-', ',', '\'', '?', '\r', '\n', '\t', '!', '#', '$', '%', '^', '(', ')', '|'}, StringSplitOptions.RemoveEmptyEntries));
            return set;
        }
        public void GetPerfCounters()
        {
            PerformanceCounter mem = new PerformanceCounter("Memory", "Available MBytes", null);
            PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            float perfCounterValue = cpu.NextValue();

            System.Threading.Thread.Sleep(1000);
            CpuUsage = (int)cpu.NextValue();
            RamAvailable = (int)mem.NextValue();

            PerformanceCounterEntity counter = new PerformanceCounterEntity(CpuUsage, RamAvailable, state);
            TableOperation insertOp = TableOperation.InsertOrReplace(counter);
            StorageManager.PerformanceCounterTable().Execute(insertOp);
        }

        private void UpdateRecentUrl(string url)
        {
            TableOperation recentInsert = TableOperation.Insert(new RecentUrlEntity(url));
            StorageManager.RecentTenTable().Execute(recentInsert);
        }

        public bool IsDisallow(string url)
        {
            bool result = false;
            foreach (string noEntering in disallowedUrl)
            {
                if (url.Contains(noEntering))
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
