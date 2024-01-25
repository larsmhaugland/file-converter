using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

public class SiegfriedJSON
{
    [JsonPropertyName("siegfried")]
    public string siegfriedVersion;
    [JsonPropertyName("scandate")]
    public string scandate;
    [JsonPropertyName("files")]
    public SiegfriedFile[] files;

}

public struct SiegfriedFile
{
    [JsonPropertyName("filename")]
    public string filename;
    [JsonPropertyName("filesize")]
    public long filesize;
    [JsonPropertyName("modified")]
    public string modified;
    [JsonPropertyName("errors")]
    public string errors;
    [JsonPropertyName("matches")]
    public SiegfriedMatches[] matches;
}
public struct SiegfriedMatches
{
    [JsonPropertyName("ns")]
    public string ns;
    [JsonPropertyName("id")]
    public string id;
    [JsonPropertyName("format")]
    public string format;
    [JsonPropertyName("version")]
    public string version;
    [JsonPropertyName("mime")]
    public string mime;
    [JsonPropertyName("class")]
    public string class_;
    [JsonPropertyName("basis")]
    public string basis;
    [JsonPropertyName("warning")]
    public string warning;
}


public class FileManager
{
    private static FileManager? instance;
    private static readonly object lockObject = new object();
    public Dictionary<string, SettingsData> FileSettings;
    public Dictionary<string, SettingsData> FolderOverride;
    string SiegfriedVersion;
    string ScanDate;
	string InputFolder;		        // Path to input folder
	string OutputFolder;            // Path to output folder
	public List<FileInfo> Files;	// List of files to be converted

    private FileManager()
    {
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
    //Test for gitlab runner
    public FileManager(string input, string output)
    {
        Files = new List<FileInfo>();
        FileSettings = new Dictionary<string, SettingsData>();
        FolderOverride = new Dictionary<string, SettingsData>();
        InputFolder = input;
        OutputFolder = output;
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


    public void IdentifyFilesJSON()
    {
        // Wrap the file path in quotes
        string wrappedPath = "\"" + InputFolder + "\"";
        string options = $"-home ConversionTools -multi 64 -json -sig pronom64k.sig ";
        string outputFile = OutputFolder + "/siegfried.json";
        //Create output file
        File.Create(outputFile).Close();
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
            // Create the StreamWriter to write to the file
            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                // Set the output stream for the process
                process.OutputDataReceived += (sender, e) => { if (e.Data != null) sw.WriteLine(e.Data); };

                // Start the process
                process.Start();

                // Begin asynchronous read operations for output and error streams
                process.BeginOutputReadLine();
                error = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();
            }
        }
        /*
        if (error.Length > 0)
        {
            Logger.Instance.SetUpRunTimeLogMessage("FileManager SF " + error, true);
            return; //TODO: Check error and possibly continue
        }*/
        var parsedData = ParseJSONOutput(outputFile);
        if(parsedData == null)            
            return; //TODO: Check error and possibly continue

        SiegfriedVersion = parsedData.siegfriedVersion;
        ScanDate = parsedData.scandate;
        for(int i = 0; i < parsedData.files.Length; i++)
        {
            var file = new FileInfo(parsedData.files[i]);
            if(file != null)
                Files.Add(file);
        }

    }

    SiegfriedJSON ParseJSONOutput(string jsonFilePath)
    {
        try
        {
            using (JsonDocument document = JsonDocument.Parse(File.OpenRead(jsonFilePath)))
            {
                // Access the root of the JSON document
                JsonElement root = document.RootElement;

                // Deserialize JSON into a SiegfriedJSON object
                SiegfriedJSON siegfriedJson = new SiegfriedJSON
                {
                    siegfriedVersion = root.GetProperty("siegfried").GetString(),
                    scandate = root.GetProperty("scandate").GetString(),
                    files = root.GetProperty("files").EnumerateArray()
                        .Select(fileElement => ParseSiegfriedFile(fileElement))
                        .ToArray()
                };
                return siegfriedJson;
            }
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Logger.Instance.SetUpRunTimeLogMessage("FileManager JSON " + e.Message, true);
            return null;
        }
    }

    static SiegfriedFile ParseSiegfriedFile(JsonElement fileElement)
    {
        return new SiegfriedFile
        {
            filename = fileElement.GetProperty("filename").GetString(),
            filesize = fileElement.GetProperty("filesize").GetInt64(),
            modified = fileElement.GetProperty("modified").GetString(),
            errors = fileElement.GetProperty("errors").GetString(),
            matches = fileElement.GetProperty("matches").EnumerateArray()
                .Select(matchElement => ParseSiegfriedMatches(matchElement))
                .ToArray()
        };
    }

    static SiegfriedMatches ParseSiegfriedMatches(JsonElement matchElement)
    {
        return new SiegfriedMatches
        {
            ns = matchElement.GetProperty("ns").GetString(),
            id = matchElement.GetProperty("id").GetString(),
            format = matchElement.GetProperty("format").GetString(),
            version = matchElement.GetProperty("version").GetString(),
            mime = matchElement.GetProperty("mime").GetString(),
            class_ = matchElement.GetProperty("class").GetString(),
            basis = matchElement.GetProperty("basis").GetString(),
            warning = matchElement.GetProperty("warning").GetString()
        };
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
                    if (String.IsNullOrEmpty(settings.ConvertTo)) { settings.ConvertTo = null; }
                    if (String.IsNullOrEmpty(settings.ConvertFrom)) {
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
