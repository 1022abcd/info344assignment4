using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class PageEntity : TableEntity
    {

        public PageEntity()
        {

        }

        public PageEntity(string title, string fullTitle, string url, DateTime time)
        {
            this.PartitionKey = title;
            this.RowKey = new HashUrl(url).encoded;
            this.FullTitle = fullTitle;
            this.Url = url;
            this.Pubdate = time;
        }
        public string FullTitle { get; set; }
        public string Url { get; set; }

        public DateTime Pubdate { get; set; }



    }
}
