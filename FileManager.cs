using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

public class FileManager
{
    string InputFolder;             // Path to input folder
    string OutputFolder;            // Path to output folder
    public List<FileInfo> Files;	// List of files to be converted
    public Dictionary<string, SettingsData> FileSettings;

    private FileManager()
    {
    }

    public FileManager(string input, string output)
    {
        Files = new List<FileInfo>();
        FileSettings = new Dictionary<string, SettingsData>();
        InputFolder = input;
        OutputFolder = output;
    }
    public class SettingsData
    {
        public string Pronom { get; set; }
        public string ConvertTo { get; set; }
        public string DefaultType { get; set; }
    }
    public void DocumentFiles()
    {
        Logger logger = Logger.Instance;
        logger.SetUpDocumentation(Files);
    }

    public void IdentifyFiles()
    {
        //Identify all files in input directory
        string[] filePaths = Directory.GetFiles(InputFolder, "*.*", SearchOption.AllDirectories);

        //In Parallel: Run SF and parse output into FileInfo constructor
        Parallel.ForEach(filePaths, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
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
                        Pronom = fileTypeNode.SelectSingleNode("Pronom")?.InnerText,
                        ConvertTo = fileTypeNode.SelectSingleNode("ConvertTo")?.InnerText,
                        DefaultType = fileTypeNode.SelectSingleNode("Default")?.InnerText
                    };

                    FileSettings[extension] = settings;
                }
            }
        }
        catch (Exception ex)
        {
            
            string exceptionMessage = ex.Message.ToString();
            logger.SetUpRunTimeLogMessage(exceptionMessage, true);
        }
    }
}
