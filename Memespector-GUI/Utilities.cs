using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Google.Cloud.Vision.V1;
using MessageBox.Avalonia;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Memespector_GUI
{
    static public class Utilities
    {
        static private MemespectorConfig? memespectorConfig = null;
        static public MemespectorConfig MemespectorConfig
        {
            get
            {
                if (memespectorConfig == null)
                {
                    try
                    {
                        var configFilePath = Path.Join(GetApplicationPath(), configBaseFilename);
                        memespectorConfig = JsonConvert.DeserializeObject<MemespectorConfig>(File.ReadAllText(configFilePath));
                    }
                    catch
                    {
                        memespectorConfig = new MemespectorConfig();
                        WriteConfig(memespectorConfig);
                    }
                }
                return memespectorConfig;
            }
        }

        static public Dictionary<VisualFeatureTypes, float> GetConfigMicrosoftAzureFlatteningMinScores()
        {
            var config = MemespectorConfig;
            return new Dictionary<VisualFeatureTypes, float> {
                { VisualFeatureTypes.Categories, config.MicrosoftAzure_CSV_MinScores.Categories },
                { VisualFeatureTypes.Description, config.MicrosoftAzure_CSV_MinScores.Description },
                { VisualFeatureTypes.Tags, config.MicrosoftAzure_CSV_MinScores.Tags },
                { VisualFeatureTypes.Brands, config.MicrosoftAzure_CSV_MinScores.Brands },
                { VisualFeatureTypes.Objects, config.MicrosoftAzure_CSV_MinScores.Objects }
            };
        }

        static public Dictionary<Feature.Types.Type, int> GetConfigGoogleVisionMaxResults()
        {
            var config = MemespectorConfig;
            return new Dictionary<Feature.Types.Type, int> {
                { Feature.Types.Type.FaceDetection, config.GoogleVision_MaxResults.Face },
                { Feature.Types.Type.LabelDetection, config.GoogleVision_MaxResults.Label },
                { Feature.Types.Type.WebDetection, config.GoogleVision_MaxResults.Web },
                { Feature.Types.Type.LandmarkDetection, config.GoogleVision_MaxResults.Landmark },
                { Feature.Types.Type.LogoDetection, config.GoogleVision_MaxResults.Logo }
            };
        }

        static public Dictionary<Feature.Types.Type, float> GetConfigGoogleVisionFlatteningMinScores()
        {
            var config = MemespectorConfig;
            return new Dictionary<Feature.Types.Type, float> {
                { Feature.Types.Type.LabelDetection, config.GoogleVision_CSV_MinScores.Label },
                { Feature.Types.Type.WebDetection, config.GoogleVision_CSV_MinScores.Web },
                { Feature.Types.Type.LandmarkDetection, config.GoogleVision_CSV_MinScores.Landmark },
                { Feature.Types.Type.LogoDetection, config.GoogleVision_CSV_MinScores.Logo },
                { Feature.Types.Type.FaceDetection, config.GoogleVision_CSV_MinScores.Face }
            };
        }

        static public void WriteConfig(MemespectorConfig config)
        {
            var configFilePath = Path.Join(GetApplicationPath(), configBaseFilename);
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
            memespectorConfig = config;
        }

        static public bool IsFolderPath(string filename)
        {
            return Directory.Exists(filename);
        }

        static public bool IsFilePath(string filename)
        {
            return File.Exists(filename);
        }

        static public bool IsUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        static public bool OpenExternalUrl(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
                return true;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
                return true;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
                return true;
            }
            return false;
        }


        static public async void ShowErrorDialog(string title, string message, Window parent)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Icon = MessageBox.Avalonia.Enums.Icon.Error,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Style = MessageBox.Avalonia.Enums.Style.Windows
            });
            await messageBox.ShowDialog(parent);
        }

        static public async void ShowInfoDialog(string title, string message, Window parent)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Icon = MessageBox.Avalonia.Enums.Icon.Info,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Style = MessageBox.Avalonia.Enums.Style.Windows
            });
            await messageBox.ShowDialog(parent);
        }

        static public async Task<MessageBox.Avalonia.Enums.ButtonResult> ShowOkCancelDialog(string title, string message, Window parent)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            {
                ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.OkCancel,
                ContentTitle = title,
                ContentMessage = message,
                Icon = MessageBox.Avalonia.Enums.Icon.Warning,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Style = MessageBox.Avalonia.Enums.Style.Windows
            });
            return await messageBox.ShowDialog(parent);
        }

        static public string GetApplicationPath()
        {
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8603 // Possible null reference return.
        }

        static private string configBaseFilename = "config-memespector-gui.json";
    }
}
