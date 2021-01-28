using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;

namespace ResearchGVClient
{
    public class GVClient
    {
        public GVClient()
        {
        }

        private string googleCloudCredentialsFilePath = string.Empty;
        public string GoogleCloudCredentialsFile
        {
            get => googleCloudCredentialsFilePath;
            set { googleCloudCredentialsFilePath = value; Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", value); }
        }

        public bool HasCredentialsFile()
        {
            if (string.IsNullOrEmpty(googleCloudCredentialsFilePath))
                return false;
            if (!File.Exists(googleCloudCredentialsFilePath))
                return false;
            return true;
        }

        public async Task<IEnumerable<GVTask>> AnnotateImages(IEnumerable<GVTask> gvTasks, ICollection<Google.Cloud.Vision.V1.Feature.Types.Type> featureDetectionTypes)
        {
            foreach (var gvTask in gvTasks)
            {
                gvTask.DetectionFeatureTypes = featureDetectionTypes;
            }

            return await AnnotateImages(gvTasks);
        }


        public async Task<IEnumerable<GVTask>> AnnotateImages(IEnumerable<GVTask> gvTasks)
        {
            List<Task> tasks = new List<Task>();

            foreach(var gvTask in gvTasks)
            {
                tasks.Add(Task.Run(gvTask.GVAction));
            }

            Task executionOfAllTasks = Task.WhenAll(tasks.ToArray());

            try
            {
                await executionOfAllTasks;
            }
            catch { }

            return gvTasks;
        }

    }
}
