using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CVClient
{
    public class CVClientManager
    {
        public CVClientManager()
        {
        }

        public int parallelCVImageTasksToRun = Environment.ProcessorCount;
        public int ParallelCVImageTasksToRun { get => parallelCVImageTasksToRun; set { if (value > 0) parallelCVImageTasksToRun = value;  } }

        private string googleCloudCredentialsFilePath = string.Empty;
        private string microsoftAzureEndpoint = string.Empty;
        private string microsoftAzureSubscriptionKey = string.Empty;
        private string clarifaiApiKey = string.Empty;
        private string openSourceApiEndpoint = string.Empty;

        public string GoogleCloudCredentialsFile
        {
            get => googleCloudCredentialsFilePath;
            set { googleCloudCredentialsFilePath = value; Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", value); }
        }

        public string MicrosoftAzureEndpoint
        {
            get => microsoftAzureEndpoint;
            set { microsoftAzureEndpoint = value; Environment.SetEnvironmentVariable("MICROSOFT_AZURE_CV_ENDPOINT", value); }
        }

        public string MicrosoftAzureSubscriptionKey
        {
            get => microsoftAzureSubscriptionKey;
            set { microsoftAzureSubscriptionKey = value; Environment.SetEnvironmentVariable("MICROSOFT_AZURE_CV_KEY", value); }
        }

        public string ClarifaiApiKey
        {
            get => clarifaiApiKey;
            set { clarifaiApiKey = value; Environment.SetEnvironmentVariable("CLARIFAI_API_KEY", value); }
        }

        public string OpenSourceApiEndpoint
        {
            get => openSourceApiEndpoint;
            set { openSourceApiEndpoint = value; Environment.SetEnvironmentVariable("OPEN_SOURCE_CV_ENDPOINT", value); }
        }


        public bool HasGoogleCloudCredentialsFile()
        {
            if (string.IsNullOrEmpty(googleCloudCredentialsFilePath))
                return false;
            if (!File.Exists(googleCloudCredentialsFilePath))
                return false;
            return true;
        }

        public async Task RunTasks(IEnumerable<CVImageTask> cvImageTasks)
        {
            var queue = new ConcurrentQueue<int>(Enumerable.Range(0, cvImageTasks.Count()));

            List<Task> tasks = new List<Task>();

            for (int n = 0; n < parallelCVImageTasksToRun; n++)
            {
                tasks.Add(Task.Run(() =>
                {
                    while (queue.TryDequeue(out int gvTaskIndex))
                    {
                        Task.Run(cvImageTasks.Skip(gvTaskIndex).First().AllAPIsAction).Wait();
                    }
                }));
            }

            Task concurrentTasks = Task.WhenAll(tasks.ToArray());

            try
            {
                await concurrentTasks;
            }
            catch { }
        }

    }
}
