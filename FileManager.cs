using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;


public class FileManager
{
    private static FileManager? instance;
    private static readonly object lockObject = new object();
    public Dictionary<string, SettingsData> FileSettings;
    public Dictionary<string, SettingsData> FolderOverride;
	public List<FileInfo> Files;	// List of files to be converted

    private FileManager()
    {
        Files = new List<FileInfo>();
        FileSettings = new Dictionary<string, SettingsData>();
        FolderOverride = new Dictionary<string, SettingsData>(); 
    }
    public static FileManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new FileManager();
                    }
                }
            }
            return instance;
        }
    }

    public async void IdentifyFiles()
    {
        Siegfried sf = Siegfried.Instance;
        Files = sf.IdentifyFilesJSON(GlobalVariables.parsedOptions.Output); //Search for files in output folder since they are copied there from input folder
        if(Files == null)
        {
            Files = new List<FileInfo>();
        }
    }

    public class SettingsData
    {
        public string ConvertFrom { get; set; }
        public string? ConvertTo { get; set; }
        public string DefaultType { get; set; }
    }
    public void DocumentFiles()
    {
        Logger logger = Logger.Instance;
        logger.SetUpDocumentation(Files);
    }

    public void ReadSettings(string pathToSettings)
    {
        Logger logger = Logger.Instance;
        try
        {


            // Load the XML document from a file
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathToSettings);
            // Uncomment the line below to simulate an error
            //xmlDoc.Load("nonexistent_file.xml");

            // Access elements and attributes
            XmlNode requesterNode = xmlDoc.SelectSingleNode("/root/Requester");
            string requester = requesterNode?.InnerText;
            Logger.JsonRoot.requester = requester;

            XmlNode converterNode = xmlDoc.SelectSingleNode("/root/Converter");
            string converter = converterNode?.InnerText;
            Logger.JsonRoot.converter = converter;

            // Access elements and attributes
            XmlNodeList fileTypeNodes = xmlDoc.SelectNodes("/root/FileTypes");
            if (fileTypeNodes != null)
            {
                foreach (XmlNode fileTypeNode in fileTypeNodes)
                {
                    string extension = fileTypeNode.SelectSingleNode("Filename")?.InnerText;
                    SettingsData settings = new SettingsData
                    {
                        ConvertFrom = fileTypeNode.SelectSingleNode("ConvertFrom")?.InnerText,
                        ConvertTo = fileTypeNode.SelectSingleNode("ConvertTo")?.InnerText,
                        DefaultType = fileTypeNode.SelectSingleNode("Default")?.InnerText
                    };
                    if (!String.IsNullOrEmpty(settings.ConvertFrom)) {
                        FileSettings[settings.ConvertFrom] = settings;
                        logger.SetUpRunTimeLogMessage("No convertTo sepcified at " + settings.ConvertFrom,true);
                    }
                    
                }
            }
            XmlNodeList folderOverrideNodes = xmlDoc.SelectNodes("/root/FolderOverride");
            if(folderOverrideNodes != null)
            {
                foreach(XmlNode folderOverrideNode in folderOverrideNodes)
                {
                    string folderPath = folderOverrideNode.SelectSingleNode("FolderPath")?.InnerText;
                    SettingsData settings = new SettingsData
                    {
                        ConvertFrom = folderOverrideNode.SelectSingleNode("ConvertFrom")?.InnerText,
                        ConvertTo = folderOverrideNode.SelectSingleNode("ConvertTo")?.InnerText,
                    };
                    bool folderPathEmpty = String.IsNullOrEmpty(folderPath);
                    bool convertFromEmpty = String.IsNullOrEmpty(settings.ConvertFrom);
                    bool convertToEmpty = String.IsNullOrEmpty(settings.ConvertTo);

                    if (folderPathEmpty && convertFromEmpty && convertToEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("empty folderOverride in seettings", true);
                    }
                    else if (folderPathEmpty && !convertFromEmpty && !convertToEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("folderpath is empty for " + settings.ConvertFrom + "-" + settings.ConvertTo + " in settings", true);
                    }
                    else if (convertFromEmpty && !convertToEmpty && !folderPathEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("convertFrom is empty for " + folderPath + " in settings", true);
                    }
                    else if (convertToEmpty && !convertFromEmpty && !folderPathEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("convertTo is empty for " + folderPath + " in settings", true);
                    }
                    else
                    { 
                        FolderOverride[folderPath] = settings;
                    }
                }
            }
        }
        catch (Exception ex)
        {

            string exceptionMessage = ex.Message.ToString();
            logger.SetUpRunTimeLogMessage(exceptionMessage, true);
        }
    }
    /*
        public void IdentifyFiles()
        {
            //Identify all files in input directory
            string[] filePaths = Directory.GetFiles(InputFolder, "*.*", SearchOption.AllDirectories);

            //In Parallel: Run SF and parse output into FileInfo constructor
            Parallel.ForEach(filePaths, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount}, filePath =>
            {
                var extention = Path.GetExtension(filePath);
                //Switch for different compression formats
                switch (extention)
                {
                    case ".zip":
                        //TODO: Unzip
                        break;
                    case ".tar":
                        //TODO: Untar
                        break;
                    case ".gz":
                        //TODO: Unzip
                        break;
                    case ".rar":
                        //TODO: Unrar
                        break;
                    case ".7z":
                        //TODO: Un7z
                        break;
                    default:
                        //Do nothing
                        break;
                }

                FileInfo file = GetFileInfo(filePath);
                if (file != null)
                    Files.Add(file);
            });
        }

        static FileInfo GetFileInfo(string filePath)
        {
            // Wrap the file path in quotes
            string wrappedPath = "\"" + filePath + "\"";
            string options = $"-home ConversionTools ";
            // Define the process start info
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = @"ConversionTools/sf.exe", // or any other command you want to run
                Arguments = options + wrappedPath,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            string output = "";
            string error = "";
            // Create the process
            using (Process process = new Process { StartInfo = psi })
            {
                // Start the process
                process.Start();

                // Read the output
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();                     
            }
            if (error.Length > 0)
            {
                Logger.Instance.SetUpRunTimeLogMessage("FileManager SF " + error, true);
                return null;
            }
            return new FileInfo(output, filePath);
        }



    */
}
