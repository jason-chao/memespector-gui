using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;

namespace ResearchGVClient
{
    public class GVClientManager
    {
        public GVClientManager()
        {
        }

        private string googleCloudCredentialsFilePath = string.Empty;

        public int MaxThreads { get; set; } = 5;

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
            var queue = new ConcurrentQueue<int>(Enumerable.Range(0, gvTasks.Count()));

            List<Task> tasks = new List<Task>();

            for (int n = 0; n < MaxThreads; n++)
            {
                tasks.Add(Task.Run(() =>
                {
                    while (queue.TryDequeue(out int gvTaskIndex)) {
                        Task.Run(gvTasks.Skip(gvTaskIndex).First().GVAction).Wait();
                    }
                }));
            }

            Task concurrentTasks = Task.WhenAll(tasks.ToArray());

            try
            {
                await concurrentTasks;
            }
            catch { }

            return gvTasks;
        }

    }
}
