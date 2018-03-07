using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Xml;


namespace WebRole1
{
    /// <summary>
    /// Summary description for PA2
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class PA2 : System.Web.Services.WebService
    {
        static TrieTree trie = new TrieTree();

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DownloadBlob()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("wiki");
            if (File.Exists(System.IO.Path.GetTempPath() + "/temp.txt"))
            {
                File.Delete(System.IO.Path.GetTempPath() + "/temp.txt");
            }
            if (container.Exists())
            {
                // Loop over items within the container and output the length and URI.
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        var filePath = System.IO.Path.GetTempPath() + "/temp.txt";
                        blob.DownloadToFile(filePath, FileMode.Create);
                    }
                }
            }
            return System.IO.Path.GetTempPath() + "/temp.txt";
        }

        [WebMethod]
        public string BuildTrie()
        {
            trie = new TrieTree();
            Regex regex = new Regex(@"^[a-zA-Z_]+$");
            PerformanceCounter theMemCounter = new PerformanceCounter("Memory", "Available MBytes");
            try
            {
                var filePath = System.IO.Path.GetTempPath() + "/temp.txt";
                using (StreamReader sr = new StreamReader(filePath))
                {
                    Double availableRam = theMemCounter.NextValue();
                    while ((!sr.EndOfStream) && (availableRam > 40))
                    {
                        String line = sr.ReadLine().Trim();
                        if (regex.IsMatch(line) && line.ToLower()[0] <= 'c')
                        {

                            trie.Insert(line.Replace("_", " ").ToLower());
                        }
                    }
                    Debug.WriteLine("Tree has maxed out its RAM capacity...");
                    return "Done";
                }
            }
            catch (IOException)
            {
                Debug.WriteLine("File could be used by another process");
                return "Failed...";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SearchTrie(string word)
        {
            return new JavaScriptSerializer().Serialize(trie.Search(word.ToLower()));
        }
    }
}