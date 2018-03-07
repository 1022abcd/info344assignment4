using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class RecentUrlEntity : TableEntity
    {
        public RecentUrlEntity()
        {
        }

        public RecentUrlEntity(string link)
        {
            this.PartitionKey = "recentTen";
            this.RowKey = new DateTime().ToString();
            this.GetLink = link;
        }
        
        public string GetLink { get; set; }


    }
}

