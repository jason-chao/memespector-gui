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
using ResearchGVClient;
using Newtonsoft.Json;
using ServiceStack.Text;

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

            OutputJsonFileLocation = Path.Join(userDataPath, sessionBaseFilename + ".json");
            OutputCsvFileLocation = Path.Join(userDataPath, sessionBaseFilename + ".csv");
        }

        private GVClientManager gvClient = new GVClientManager();
        public Window Parent { get; set; } = new Window(); // shall be set to the true parent (Window) of the view model.  it is necessnary to pass the parent window to a message dialog box.

        public ReactiveCommand<string, Unit> BrowseFile { get; }
        public ReactiveCommand<string, Unit> OpenExternal { get; }
        public ReactiveCommand<string, Unit> ShowMessageBox { get; }
        public ReactiveCommand<string, Unit> InvokeTargetAPI { get; }

        private string userDataPath = Utilities.GetApplicationPath();
        private string sessionBaseFilename = string.Format("gcv-api-{0:yyyyMMdd_HHmmss}", DateTime.Now);

        private string gcsCredentialFileLocation = Utilities.ReadConfig().GCSCredentialFileLocation;
        public string GCSCredentialFileLocation { get => gcsCredentialFileLocation; set => this.RaiseAndSetIfChanged(ref gcsCredentialFileLocation, value); }

        private bool detection_Safety = true;
        public bool Detection_Safety { get => detection_Safety; set => this.RaiseAndSetIfChanged(ref detection_Safety, value); }
        private bool detection_Face = true;
        public bool Detection_Face { get => detection_Face; set => this.RaiseAndSetIfChanged(ref detection_Face, value); }
        private bool detection_Label = true;
        public bool Detection_Label { get => detection_Label; set => this.RaiseAndSetIfChanged(ref detection_Label, value); }
        private bool detection_Web = true;
        public bool Detection_Web { get => detection_Web; set => this.RaiseAndSetIfChanged(ref detection_Web, value); }
        private bool detection_Text = true;
        public bool Detection_Text { get => detection_Text; set => this.RaiseAndSetIfChanged(ref detection_Text, value); }
        private bool detection_Landmark = true;
        public bool Detection_Landmark { get => detection_Landmark; set => this.RaiseAndSetIfChanged(ref detection_Landmark, value); }
        private bool detection_Logo = true;
        public bool Detection_Logo { get => detection_Logo; set => this.RaiseAndSetIfChanged(ref detection_Logo, value); }

        private string imageFileLocations = string.Empty;
        public string ImageFileLocations { get => imageFileLocations; set => this.RaiseAndSetIfChanged(ref imageFileLocations, value); }
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

        private void cleanimageFileLocationList()
        {
            ImageFileLocations = string.Join(Environment.NewLine, imageFileLocations.Split(Environment.NewLine).Select(l => l.Trim()).Distinct().Where(l => (Utilities.IsFilePath(l) || Utilities.IsUrl(l)) && !string.IsNullOrEmpty(l)));
        }

        private void doOpenExternal(string parameter)
        {
            if (parameter == "Github")
            {
                Utilities.OpenExternalUrl("https://www.github.com/jason-chao/");
            }
        }


        private async void doShowMessageBox(string parameter)
        {
            if (parameter == "WebImages")
            {
                Utilities.ShowInfoDialog("Add images from the web", $"Please copy the URLs of the images and paste them into the box.{Environment.NewLine + Environment.NewLine}Please put one URL per line.  You may press ENTER to create a new line.", Parent);
            }
            else if (parameter == "About")
            {
                var messageBox = MessageBoxManager.GetMessageBoxHyperlinkWindow(new MessageBox.Avalonia.DTO.MessageBoxHyperlinkParams
                {
                    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    ContentTitle = "About Memespector GUI",
                    HyperlinkContentProvider = new[] {
                    new HyperlinkContent { Alias = "GUI client for " },
                    new HyperlinkContent { Alias = "Google Cloud Vision API", Url="https://cloud.google.com/vision/docs" },
                    new HyperlinkContent { Alias = " for research purposes by " },
                    new HyperlinkContent { Alias = "Jason CHAO", Url="https://jasontc.net/" }, new HyperlinkContent { Alias = ". " },
                    new HyperlinkContent { Alias = "Inspired by the original command-line memespector projects of " },
                    new HyperlinkContent { Alias = "bernorieder ", Url = "https://github.com/bernorieder/memespector" },
                    new HyperlinkContent { Alias = $"and " },
                    new HyperlinkContent { Alias = "amintz", Url = "https://github.com/amintz/memespector-python" } },
                    Icon = MessageBox.Avalonia.Enums.Icon.Info,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Style = MessageBox.Avalonia.Enums.Style.Windows
                });
                await messageBox.ShowDialog(Parent);
            }
        }

        private List<Feature.Types.Type> getSelectedFeatures()
        {
            var list = new List<Feature.Types.Type>();

            if (detection_Safety)
                list.Add(Feature.Types.Type.SafeSearchDetection);

            if (detection_Face)
                list.Add(Feature.Types.Type.FaceDetection);

            if (detection_Label)
                list.Add(Feature.Types.Type.LabelDetection);

            if (detection_Web)
                list.Add(Feature.Types.Type.WebDetection);

            if (detection_Text)
                list.Add(Feature.Types.Type.TextDetection);

            if (detection_Landmark)
                list.Add(Feature.Types.Type.LandmarkDetection);

            if (detection_Logo)
                list.Add(Feature.Types.Type.LogoDetection);

            return list;
        }

        private async void doInvokeTargetAPI(string parameter)
        {
            if (parameter == "GoogleCloudVisionAPIV1")
            {
                IsInputEnabled = false;

                if (!File.Exists(gcsCredentialFileLocation))
                {
                    Utilities.ShowErrorDialog("Credential file", "The Google Cloud credential file has not been chosen or does not exist.  Please select a Google Cloud credential file.", Parent);
                    IsInputEnabled = true;
                    return;
                }

                if (!GVCommon.MayBeAValidGCSCredentialFile(gcsCredentialFileLocation))
                {
                    Utilities.ShowErrorDialog("Credential file", "The Google Cloud credential file is invalid.  Please select a valid Google Cloud credential file.", Parent);
                    IsInputEnabled = true;
                    return;
                }

                gvClient.GoogleCloudCredentialsFile = gcsCredentialFileLocation;
                gvClient.MaxThreads = Utilities.ReadConfig().MaxConcurrentAPICalls;

                if (File.Exists(outputJsonFileLocation) || File.Exists(outputCsvFileLocation))
                {
                    var dialogResult = await Utilities.ShowOkCancelDialog("Exisiting output file", "The name for the ouput JSON or CSV file already exists.  The file will be overwritten.  Are you sure you want to continue?", Parent);

                    if (dialogResult == MessageBox.Avalonia.Enums.ButtonResult.Cancel)
                    {
                        IsInputEnabled = true;
                        return;
                    }
                }

                cleanimageFileLocationList();

                var imageFileLocations = this.imageFileLocations.Split(Environment.NewLine).Where(l => !string.IsNullOrEmpty(l));

                if (!imageFileLocations.Any())
                {
                    Utilities.ShowErrorDialog("Image locations", "Please provide valid locations of image files on this computer or on the web.", Parent);
                    IsInputEnabled = true;
                    return;
                }

                var featureDetecitonList = getSelectedFeatures();
                var featureMaxResults = Utilities.GetConfigFeaturesMaxResults();
                var flatteningMinScores = Utilities.GetConfigFlatteningMinScores();

                var gvTaskList = new List<GVTask>();

                foreach (var fileLocation in imageFileLocations)
                {
                    var gvTask = new GVTask() { ImageLocation = fileLocation, DetectionFeatureTypes = featureDetecitonList, FeatureMaxResults = featureMaxResults, FlatteningMinScores = flatteningMinScores };
                    if (Utilities.IsFilePath(gvTask.ImageLocation))
                        gvTask.ImageLocationType = GVTask.LocationType.Local;
                    else if (Utilities.IsUrl(gvTask.ImageLocation))
                        gvTask.ImageLocationType = GVTask.LocationType.Uri;

                    gvTaskList.Add(gvTask);
                }

                IsInvocationInProgress = true;

                var checkProgressTask = Task.Run(() =>
                {
                    int total = gvTaskList.Count;
                    int processed = 0;
                    do
                    {
                        processed = gvTaskList.Count(t => t.Processed.HasValue);
                        ProgressValue = Convert.ToInt32(Convert.ToDouble(processed) / Convert.ToDouble(total) * 100);
                        ProgressMessage = $"Processed {processed.ToString()} of {total.ToString()}";
                        System.Threading.Thread.Sleep(200);
                    } while (!((processed >= total) || !IsInvocationInProgress));
                });

                var gvTaskResults = await gvClient.AnnotateImages(gvTaskList);

                File.WriteAllText(outputJsonFileLocation, JsonConvert.SerializeObject(gvTaskResults, Formatting.Indented));
                File.WriteAllText(OutputCsvFileLocation, CsvSerializer.SerializeToCsv(gvTaskResults.Select(t => t.FlatAnnotationResult)));

                IsInputEnabled = true;
                IsInvocationInProgress = false;

                Utilities.ShowInfoDialog("Completion", "All images have been processed by the API.  Open the result files to see the details.", Parent);
            }

        }

        private async void doBrowseFile(string forProperty)
        {

            if (forProperty == "GCSCredentialFileLocation")
            {
                var openFileDialog = new OpenFileDialog() { Title = "Open Google Cloud credential file", AllowMultiple = false, Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "JSON", Extensions = { "json" } } }, Directory = userDataPath };
                var files = await openFileDialog.ShowAsync(Parent);
                if (files.Any())
                {
                    string filename = files.First();
                    GCSCredentialFileLocation = filename;

                    var config = Utilities.ReadConfig();
                    config.GCSCredentialFileLocation = filename;
                    Utilities.WriteConfig(config);
                }
            }
            else if (forProperty == "OutputJsonFileLocation")
            {
                var saveFileDialog = new SaveFileDialog() { Title = "Save detailed results as JSON", InitialFileName = Path.GetFileName(OutputJsonFileLocation), Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "JSON", Extensions = { "json" } } }, Directory = userDataPath };
                var file = await saveFileDialog.ShowAsync(Parent);
                if (!string.IsNullOrEmpty(file))
                    OutputJsonFileLocation = file;
            }
            else if (forProperty == "OutputCsvFileLocation")
            {
                var saveFileDialog = new SaveFileDialog() { Title = "Save simplified results as CSV", InitialFileName = Path.GetFileName(OutputCsvFileLocation), Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "CSV", Extensions = { "csv" } } }, Directory = userDataPath };
                var file = await saveFileDialog.ShowAsync(Parent);
                if (!string.IsNullOrEmpty(file))
                    OutputCsvFileLocation = file;
            }
            else if (forProperty.StartsWith("ImageFileLocations_"))
            {
                var imageExtensions = GVCommon.GVSupportedFormats;
                string[] files = new string[] { };

                if (forProperty.EndsWith("_Files"))
                {
                    var openFileDialog = new OpenFileDialog() { Title = "Open image files", AllowMultiple = true, Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "Image", Extensions = imageExtensions } }, Directory = userDataPath };
                    files = await openFileDialog.ShowAsync(Parent);
                }
                else if (forProperty.EndsWith("_Folder"))
                {
                    var openFolderDialog = new OpenFolderDialog() { Title = "Open a folder containing images", Directory = userDataPath };
                    var folderPath = await openFolderDialog.ShowAsync(Parent);
                    files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).Where(f => imageExtensions.Any(ext => f.ToLower().EndsWith("." + ext))).ToArray();
                }

                if (files.Any())
                {
                    ImageFileLocations = string.Join(Environment.NewLine, files) + Environment.NewLine + ImageFileLocations;
                    cleanimageFileLocationList();
                }
            }
        }
    }
}
