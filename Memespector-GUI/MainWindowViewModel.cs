using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using MessageBox.Avalonia;
using System.Reactive;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MessageBox.Avalonia.Models;
using Google.Cloud.Vision.V1;
using CVClient;
using Newtonsoft.Json;
using ServiceStack.Text;
using Memespector_GUI.APIViewModels;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Memespector_GUI
{
    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel()
        {

            BrowseFile = ReactiveCommand.Create<string>(doBrowseFile);
            OpenExternal = ReactiveCommand.Create<string>(doOpenExternal);
            ShowMessageBox = ReactiveCommand.Create<string>(doShowMessageBox);
            InvokeTargetAPI = ReactiveCommand.Create<string>(doInvokeTargetAPI);

            OutputJsonFileLocation = Path.Join(defaultDataPath, sessionBaseFilename + ".json");
            OutputCsvFileLocation = Path.Join(defaultDataPath, sessionBaseFilename + ".csv");

            this.WhenAnyValue(x => x.IsGoogleVisionEnabled, x => x.IsMicrosoftAuzreEnabled, x => x.IsClarifaiEnabled, x => x.IsOpenSourceEnabled).Subscribe(_ => saveUIStateToConfig());
            this.WhenAnyValue(x => x.GoogleVisionSettings.CredentialFileLocation, x => x.MicrosoftAzureSettings.Endpoint, x => x.MicrosoftAzureSettings.SubscriptionKey, x => x.ClarifaiSettings.APIKey, x => x.OpenSourceSettings.Endpoint).Subscribe(_ => saveUIStateToConfig());
        }

        private CVClientManager gvClient = new CVClientManager();
        public Window Parent { get; set; } = new Window(); // shall be set to the true parent (Window) of the view model.  it is necessnary to pass the parent window to a message dialog box.

        public GoogleVisionSettingsViewModel GoogleVisionSettings { get; set; } = new GoogleVisionSettingsViewModel();
        public MicrosoftAzureSettingsViewModel MicrosoftAzureSettings { get; set; } = new MicrosoftAzureSettingsViewModel();
        public ClarifaiSettingsViewModel ClarifaiSettings { get; set; } = new ClarifaiSettingsViewModel();
        public OpenSourceSettingsViewModel OpenSourceSettings { get; set; } = new OpenSourceSettingsViewModel();

        public ReactiveCommand<string, Unit> BrowseFile { get; }
        public ReactiveCommand<string, Unit> OpenExternal { get; }
        public ReactiveCommand<string, Unit> ShowMessageBox { get; }
        public ReactiveCommand<string, Unit> InvokeTargetAPI { get; }

        private string defaultDataPath = Utilities.GetApplicationPath();
        private string sessionBaseFilename = string.Format("cv-apis-{0:yyyyMMdd_HHmmss}", DateTime.Now);

        private bool isGoogleVisionEnabled = Utilities.MemespectorConfig.SelectedGoogleVision;
        public bool IsGoogleVisionEnabled { get => isGoogleVisionEnabled; set => this.RaiseAndSetIfChanged(ref isGoogleVisionEnabled, value); }

        private bool isMicrosoftAuzreEnabled = Utilities.MemespectorConfig.SelectedMicrosoftAzure;
        public bool IsMicrosoftAuzreEnabled { get => isMicrosoftAuzreEnabled; set => this.RaiseAndSetIfChanged(ref isMicrosoftAuzreEnabled, value); }

        private bool isClarifaiEnabled = Utilities.MemespectorConfig.SelectedClarifai;
        public bool IsClarifaiEnabled { get => isClarifaiEnabled; set => this.RaiseAndSetIfChanged(ref isClarifaiEnabled, value); }

        private bool isOpenSourceEnabled = Utilities.MemespectorConfig.SelectedOpenSource;
        public bool IsOpenSourceEnabled { get => isOpenSourceEnabled; set => this.RaiseAndSetIfChanged(ref isOpenSourceEnabled, value); }

        private int imageSourcesCount = 0;
        private IEnumerable<string> imageSources = new List<string>();
        private string imageSourcesText = string.Empty;
        public string ImageSourcesText {
            get {
                if (imageSourcesCount > (Utilities.MemespectorConfig.MaxUIImageSourceLinesToDisplay) && Utilities.MemespectorConfig.MaxUIImageSourceLinesToDisplay > 0)
                    return string.Join(Environment.NewLine, imageSources.Take(Utilities.MemespectorConfig.MaxUIImageSourceLinesToDisplay)) + $"{Environment.NewLine}... [truncated for display purpose only - {imageSourcesCount} lines in total - all will be processed]";
                else
                    return imageSourcesText;
            } set {
                imageSources = value.Split(Environment.NewLine).Select(l => l.Trim()).Distinct().Where(l => (Utilities.IsFilePath(l) || Utilities.IsFolderPath(l) || Utilities.IsUrl(l)) && !string.IsNullOrEmpty(l));
                imageSourcesCount = imageSources.Count();
                this.RaiseAndSetIfChanged(ref imageSourcesText, string.Join(Environment.NewLine, imageSources));
            }
        }

        private string outputJsonFileLocation = string.Empty;
        public string OutputJsonFileLocation { get => outputJsonFileLocation; set => this.RaiseAndSetIfChanged(ref outputJsonFileLocation, value); }
        private string outputCsvFileLocation = string.Empty;
        public string OutputCsvFileLocation { get => outputCsvFileLocation; set => this.RaiseAndSetIfChanged(ref outputCsvFileLocation, value); }

        private int progressValue = 0;
        public int ProgressValue { get => progressValue; set => this.RaiseAndSetIfChanged(ref progressValue, value); }

        private string progressMessage = string.Empty;
        public string ProgressMessage { get => progressMessage; set => this.RaiseAndSetIfChanged(ref progressMessage, value); }

        private bool isInvocationInProgress = false;
        public bool IsInvocationInProgress { get => isInvocationInProgress; set => this.RaiseAndSetIfChanged(ref isInvocationInProgress, value); }

        private bool isInputEnabled = true;
        public bool IsInputEnabled { get => isInputEnabled; set => this.RaiseAndSetIfChanged(ref isInputEnabled, value); }

        private object outputFileIO = new object();

        private void saveUIStateToConfig()
        {
            var config = Utilities.MemespectorConfig;

            config.GoogleCloudCredentialFileLocation = GoogleVisionSettings.CredentialFileLocation;
            config.MicrosoftAzureEndpoint = MicrosoftAzureSettings.Endpoint;
            config.MicrosoftAzureSubscriptionKey = MicrosoftAzureSettings.SubscriptionKey;
            config.ClarifaiAPIKey = ClarifaiSettings.APIKey;
            config.OpenSourceEndpoint = OpenSourceSettings.Endpoint;

            config.SelectedGoogleVision = isGoogleVisionEnabled;
            config.SelectedMicrosoftAzure = isMicrosoftAuzreEnabled;
            config.SelectedClarifai = isClarifaiEnabled;
            config.SelectedOpenSource = isOpenSourceEnabled;

            Utilities.WriteConfig(config);
        }

        private void doOpenExternal(string parameter)
        {
            if (parameter == "Github")
            {
                Utilities.OpenExternalUrl("https://github.com/jason-chao/memespector-gui");
            }
        }

        private async void doShowMessageBox(string parameter)
        {
            if (parameter == "WebImages")
            {
                Utilities.ShowInfoDialog("Add images from the web", $"If you are going to add hundreds or even thousands of images from the web, please copy the URLs of the images and paste them into a text file (.txt).  Then, add the text file into the box using the 'a text file containing image locations' button.{Environment.NewLine + Environment.NewLine}If you are going to add tens of or just a few images from the web, please paste the URLs directly into the box.  Put one URL per line.  You may press ENTER to create a new line.", Parent);
            }
            else if (parameter == "About")
            {
                var messageBox = MessageBoxManager.GetMessageBoxHyperlinkWindow(new MessageBox.Avalonia.DTO.MessageBoxHyperlinkParams
                {
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    ContentTitle = "About Memespector GUI",
                    HyperlinkContentProvider = new[] {
                    new HyperlinkContent { Alias = "GUI client for computer vision APIs by " },
                    new HyperlinkContent { Alias = "Jason CHAO", Url="https://jasontc.net/" }, new HyperlinkContent { Alias = ". " },
                    new HyperlinkContent { Alias = "Inspired by the original command-line memespector projects of " },
                    new HyperlinkContent { Alias = "bernorieder ", Url = "https://github.com/bernorieder/memespector" },
                    new HyperlinkContent { Alias = "and " },
                    new HyperlinkContent { Alias = "amintz", Url = "https://github.com/amintz/memespector-python" } },
                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Style = MessageBox.Avalonia.Enums.Style.Windows
                });
                await messageBox.ShowDialog(Parent);
            }
        }

        private List<VisualFeatureTypes?> getSelectedMicrosoftAzureFeatures()
        {
            var list = new List<VisualFeatureTypes?>();

            if (MicrosoftAzureSettings.Detection_Adult)
                list.Add(VisualFeatureTypes.Adult);

            if (MicrosoftAzureSettings.Detection_Brands)
                list.Add(VisualFeatureTypes.Brands);

            if (MicrosoftAzureSettings.Detection_Categories)
                list.Add(VisualFeatureTypes.Categories);

            if (MicrosoftAzureSettings.Detection_Description)
                list.Add(VisualFeatureTypes.Description);

            if (MicrosoftAzureSettings.Detection_Faces)
                list.Add(VisualFeatureTypes.Faces);

            if (MicrosoftAzureSettings.Detection_Objects)
                list.Add(VisualFeatureTypes.Objects);

            if (MicrosoftAzureSettings.Detection_Tags)
                list.Add(VisualFeatureTypes.Tags);

            return list;
        }

        private List<Feature.Types.Type> getSelectedGoogleVisionFeatures()
        {
            var list = new List<Feature.Types.Type>();

            if (GoogleVisionSettings.Detection_Safety)
                list.Add(Feature.Types.Type.SafeSearchDetection);

            if (GoogleVisionSettings.Detection_Face)
                list.Add(Feature.Types.Type.FaceDetection);

            if (GoogleVisionSettings.Detection_Label)
                list.Add(Feature.Types.Type.LabelDetection);

            if (GoogleVisionSettings.Detection_Web)
                list.Add(Feature.Types.Type.WebDetection);

            if (GoogleVisionSettings.Detection_Text)
                list.Add(Feature.Types.Type.TextDetection);

            if (GoogleVisionSettings.Detection_Landmark)
                list.Add(Feature.Types.Type.LandmarkDetection);

            if (GoogleVisionSettings.Detection_Logo)
                list.Add(Feature.Types.Type.LogoDetection);

            return list;
        }

        private async void doInvokeTargetAPI(string parameter)
        {
            var settings = GoogleVisionSettings;

            if (parameter == "AllAPIs")
            {
                IsInputEnabled = false;

                if ((new bool[] { isGoogleVisionEnabled, isMicrosoftAuzreEnabled, isClarifaiEnabled, isOpenSourceEnabled }).Where(apiEnabled => apiEnabled).Count() <= 0)
                {
                    Utilities.ShowErrorDialog("API settings", "No API is selected.  Please select at least one API.", Parent);
                    IsInputEnabled = true;
                    return;
                }

                if (isGoogleVisionEnabled)
                {
                    if (!File.Exists(GoogleVisionSettings.CredentialFileLocation))
                    {
                        Utilities.ShowErrorDialog("API settings", "The Google Cloud credential file has not been chosen or does not exist.  Please select a credential file.", Parent);
                        IsInputEnabled = true;
                        return;
                    }

                    if (!CVClientCommon.Google_MayBeAValidCredentialFile(GoogleVisionSettings.CredentialFileLocation))
                    {
                        Utilities.ShowErrorDialog("API settings", "The Google Cloud credential file is invalid.  Please select a valid credential file.", Parent);
                        IsInputEnabled = true;
                        return;
                    }

                    gvClient.GoogleCloudCredentialsFile = GoogleVisionSettings.CredentialFileLocation;
                }

                if (isMicrosoftAuzreEnabled)
                {
                    if (string.IsNullOrEmpty(MicrosoftAzureSettings.SubscriptionKey))
                    {
                        Utilities.ShowErrorDialog("API settings", "The Microsoft Azure subscription key is missing.  Please provide a subscription key.", Parent);
                        IsInputEnabled = true;
                        return;
                    }

                    gvClient.MicrosoftAzureSubscriptionKey = MicrosoftAzureSettings.SubscriptionKey;

                    if (!Uri.IsWellFormedUriString(MicrosoftAzureSettings.Endpoint, UriKind.Absolute))
                    {
                        Utilities.ShowErrorDialog("API settings", "The Microsoft Azure Cognitive Services endpoint is invalid.  Please provide a valid endpoint.", Parent);
                        IsInputEnabled = true;
                        return;
                    }

                    gvClient.MicrosoftAzureEndpoint = MicrosoftAzureSettings.Endpoint;
                }

                if (isClarifaiEnabled)
                {
                    if (string.IsNullOrEmpty(ClarifaiSettings.APIKey))
                    {
                        Utilities.ShowErrorDialog("API settings", "The Clarifai API key is missing.  Please provide an API key.", Parent);
                        IsInputEnabled = true;
                        return;
                    }

                    gvClient.ClarifaiApiKey = ClarifaiSettings.APIKey;
                }

                if (isOpenSourceEnabled)
                {
                    if (!Uri.IsWellFormedUriString(OpenSourceSettings.Endpoint, UriKind.Absolute))
                    {
                        Utilities.ShowErrorDialog("API settings", "The open source API endpoint is missing.  Please provide a valid endpoint.", Parent);
                        IsInputEnabled = true;
                        return;
                    }

                    gvClient.OpenSourceApiEndpoint = OpenSourceSettings.Endpoint;
                }

                if (File.Exists(outputJsonFileLocation) || File.Exists(outputCsvFileLocation))
                {
                    var dialogResult = await Utilities.ShowOkCancelDialog("Exisiting output file", "The name for the ouput JSON or CSV file already exists.  The file will be overwritten.  Are you sure you want to continue?", Parent);

                    if (dialogResult == MessageBox.Avalonia.Enums.ButtonResult.Cancel)
                    {
                        IsInputEnabled = true;
                        return;
                    }
                }

                var effecitveImageLocations = getEffectiveImageLocations(this.imageSources, true);

                if (!effecitveImageLocations.Any())
                {
                    Utilities.ShowErrorDialog("Image locations", "Please provide valid locations of image files on this computer or on the web.", Parent);
                    IsInputEnabled = true;
                    return;
                }

                var googleVisionFeatureList = getSelectedGoogleVisionFeatures();
                var googleVisionMaxResults = Utilities.GetConfigGoogleVisionMaxResults();
                var googleVisionMinScores = Utilities.GetConfigGoogleVisionFlatteningMinScores();

                var microsoftAuzreFeatureList = getSelectedMicrosoftAzureFeatures();
                var microsoftAzureMinScores = Utilities.GetConfigMicrosoftAzureFlatteningMinScores();

                var memespecotrConfig = Utilities.MemespectorConfig;

                gvClient.ParallelCVImageTasksToRun = memespecotrConfig.MaxConcurrentImageTasks;

                var gvTaskList = new List<CVImageTask>();

                foreach (var fileLocation in effecitveImageLocations)
                {
                    var gvTask = new CVImageTask() { ImageLocation = fileLocation };

                    if (isGoogleVisionEnabled)
                    {
                        gvTask.GoogleInvocation = new InvocationGoogle() { DetectionFeatureTypes = googleVisionFeatureList, DetectionMaxResults = googleVisionMaxResults, FlatteningMinScores = googleVisionMinScores };
                    }

                    if (isMicrosoftAuzreEnabled)
                    {
                        gvTask.AzureInvocation = new InvocationAzure() { DetectionFeatureTypes = microsoftAuzreFeatureList, FlatteningMinScores = microsoftAzureMinScores };
                    }

                    if (isClarifaiEnabled)
                    {
                        gvTask.ClarifaiInvocation = new InvocationClarifai() { Model = ClarifaiSettings.Model, FlatteningMinScore = memespecotrConfig.ClarifaiMinScore };
                    }

                    if (isOpenSourceEnabled)
                    {
                        gvTask.OpenSourceInvocation = new InvocationOpenSource() { Model = OpenSourceSettings.Model, DetectionMaxResults = memespecotrConfig.OpenSourceMaxResults, FlatteningMinScore = memespecotrConfig.OpenSourceMinScore };
                    }

                    if (Utilities.IsFilePath(gvTask.ImageLocation))
                        gvTask.ImageLocationType = CVClientCommon.LocationType.Local;
                    else if (Utilities.IsUrl(gvTask.ImageLocation))
                        gvTask.ImageLocationType = CVClientCommon.LocationType.Uri;

                    gvTaskList.Add(gvTask);
                }

                IsInvocationInProgress = true;

                var checkProgressTask = Task.Run(() =>
                {
                    int total = gvTaskList.Count;
                    int processed = 0;
                    int written = 0;
                    DateTime lastWritten = DateTime.Now;
                    do
                    {
                        processed = gvTaskList.Count(t => t.Completed.HasValue);
                        if (processed > written)
                        {
                            if ((DateTime.Now - lastWritten).TotalSeconds > memespecotrConfig.MinWriteResultsIntervalInSeconds)
                            {
                                writeResultsToOutputFiles(gvTaskList);
                                written = processed;
                                lastWritten = DateTime.Now;
                            }
                        }
                        ProgressValue = Convert.ToInt32(Convert.ToDouble(processed) / Convert.ToDouble(total) * 100);
                        ProgressMessage = $"Processed {processed.ToString()} of {total.ToString()}";
                        System.Threading.Thread.Sleep(memespecotrConfig.MinUIProgressUpdateIntervalInMilliseconds);
                    } while (!((processed >= total) || !IsInvocationInProgress));
                });

                bool completed = false;
                Exception? runTaskException = null;

                try
                {
                    await gvClient.RunTasks(gvTaskList);
                    completed = true;
                }
                catch (Exception ex) { runTaskException = ex; }
                finally { writeResultsToOutputFiles(gvTaskList); }

                IsInputEnabled = true;
                IsInvocationInProgress = false;

                if (completed)
                    Utilities.ShowInfoDialog("Completion", "All images have been processed by the APIs.  Open the result files to see the details.", Parent);
                else
                {
                    string exceptionMessage = (runTaskException != null) ? Environment.NewLine + Environment.NewLine + "Technical message: " + runTaskException.Message + $" ({runTaskException.Source})" : string.Empty;
                    Utilities.ShowInfoDialog("Completion", $"Some images may NOT have been processed.  Open the result files to see the details.{exceptionMessage}", Parent);
                }
            }

        }

        private void writeResultsToOutputFiles(IEnumerable<CVImageTask> cvImageTasks)
        {
            lock (outputFileIO)
            {
                File.WriteAllText(outputJsonFileLocation, JsonConvert.SerializeObject(cvImageTasks, Formatting.Indented));
                File.WriteAllText(OutputCsvFileLocation, CsvSerializer.SerializeToCsv(cvImageTasks.Select(t => t.FlatResults)));
            }
        }

        private IEnumerable<string> getEffectiveImageLocations(IEnumerable<string> locations, bool parseTxt = false)
        {
            var effecitveLocationList = new List<string>();
            var imageExtensions = CVClientCommon.SupportedFormats;

            foreach (var location in locations)
            {
                if (string.IsNullOrEmpty(location))
                    continue;

                if (Utilities.IsFolderPath(location))
                {
                    if (!Directory.Exists(location))
                        continue;

                    var locationsInFolder = getEffectiveImageLocations(Directory.GetFiles(location, "*.*", SearchOption.AllDirectories).ToArray(), false);
                    effecitveLocationList.AddRange(locationsInFolder);
                }
                else if (Utilities.IsFilePath(location))
                {
                    if (!File.Exists(location))
                        continue;

                    if (parseTxt && location.ToLower().EndsWith(".txt"))
                    {
                        var locationsInFile = getEffectiveImageLocations(File.ReadAllLines(location), false);
                        effecitveLocationList.AddRange(locationsInFile);
                    }
                    else if (imageExtensions.Any(ext => location.ToLower().EndsWith("." + ext)))
                    {
                        effecitveLocationList.Add(location);
                    }
                }
                else if (Utilities.IsUrl(location))
                    effecitveLocationList.Add(location); // IsURL must be the last case to be tested.  A file or folder path is also considered an absolute path.
            }

            return effecitveLocationList.Distinct();
        }


        private async void doBrowseFile(string forProperty)
        {
            if (forProperty == "GCSCredentialFileLocation")
            {
                var openFileDialog = new OpenFileDialog() { Title = "Open Google Cloud credential file", AllowMultiple = false, Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "JSON", Extensions = { "json" } } }, Directory = defaultDataPath };
                var files = await openFileDialog.ShowAsync(Parent);
                if (files.Any())
                {
                    string filename = files.First();
                    GoogleVisionSettings.CredentialFileLocation = filename;
                }
            }
            else if (forProperty == "OutputJsonFileLocation")
            {
                var saveFileDialog = new SaveFileDialog() { Title = "Save detailed results as JSON", InitialFileName = Path.GetFileName(OutputJsonFileLocation), Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "JSON", Extensions = { "json" } } }, Directory = defaultDataPath };
                var file = await saveFileDialog.ShowAsync(Parent);
                if (!string.IsNullOrEmpty(file))
                    OutputJsonFileLocation = file;
            }
            else if (forProperty == "OutputCsvFileLocation")
            {
                var saveFileDialog = new SaveFileDialog() { Title = "Save simplified results as CSV", InitialFileName = Path.GetFileName(OutputCsvFileLocation), Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "CSV", Extensions = { "csv" } } }, Directory = defaultDataPath };
                var file = await saveFileDialog.ShowAsync(Parent);
                if (!string.IsNullOrEmpty(file))
                    OutputCsvFileLocation = file;
            }
            else if (forProperty.StartsWith("ImageFileLocations_"))
            {
                var imageExtensions = CVClientCommon.SupportedFormats;
                string[] localPaths = new string[] { };

                if (forProperty.EndsWith("_Files"))
                {
                    var openFileDialog = new OpenFileDialog() { Title = "Open image files", AllowMultiple = true, Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "Image", Extensions = imageExtensions } }, Directory = defaultDataPath };
                    localPaths = await openFileDialog.ShowAsync(Parent);
                }
                else if (forProperty.EndsWith("_Txt"))
                {
                    var openFileDialog = new OpenFileDialog() { Title = "Open a text file containing image locations", AllowMultiple = false, Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "Text", Extensions = new List<string>() { "txt" } } }, Directory = defaultDataPath };
                    localPaths = await openFileDialog.ShowAsync(Parent);
                }
                else if (forProperty.EndsWith("_Folder"))
                {
                    var openFolderDialog = new OpenFolderDialog() { Title = "Open a folder containing images", Directory = defaultDataPath };
                    localPaths = new string[] { await openFolderDialog.ShowAsync(Parent) };
                }

                if (localPaths.Any())
                {
                    ImageSourcesText = string.Join(Environment.NewLine, localPaths) + Environment.NewLine + imageSourcesText;
                }
            }
        }
    }
}
