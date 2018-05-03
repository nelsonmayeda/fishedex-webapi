using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using FishEDexWebAPI.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace FishEDexWebAPI.Controllers
{
    public class BaseController: ApiController
    {
        protected FishesDbEntities FishDb = new FishesDbEntities();
        protected static CloudBlobContainer imagesBlobContainer;

        public BaseController()
        {
            InitializeDb();
            InitializeStorage();
        }

        private void InitializeDb()
        {
            string sstring;
#if(DEBUG)
            sstring = Properties.Settings.Default.DebugDbConnectionString;
#else
            sstring = Properties.Settings.Default.ReleaseDbConnectionString;
#endif
            FishDb = new FishesDbEntities(sstring);
        }
        private void InitializeStorage()
        {
            string sstring;
#if(DEBUG)
            sstring = Properties.Settings.Default.DebugStorageConnectionString;
#else
            sstring = Properties.Settings.Default.ReleaseStorageConnectionString;
#endif
            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse(sstring);

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            imagesBlobContainer = blobClient.GetContainerReference("images");

            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FishDb.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}