using System.Xml;


public class FileManager
{
    private static FileManager? instance;
    private static readonly object lockObject = new object();
    public Dictionary<string, KeyValuePair<string,string>> FolderOverride;
	public List<FileInfo> Files;	// List of files to be converted

    private FileManager()
    {
        Files = new List<FileInfo>();
        FolderOverride = new Dictionary<string, KeyValuePair<string,string>>(); 
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
        var files = sf.IdentifyFilesJSON(GlobalVariables.parsedOptions.Output); //Search for files in output folder since they are copied there from input folder
        if(files != null)
        {
            Files = files;
        }
    }

    public class SettingsData
    {
        public List<string>? PronomsList { get; set; }
        public string ConvertTo { get; set; }
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

            // Access elements and attributes
            XmlNode classNode = xmlDoc.SelectSingleNode("FileClass");
            string className = classNode?.SelectSingleNode("ClassName")?.InnerText;
            string defaultType = classNode?.SelectSingleNode("Default")?.InnerText;
            XmlNodeList fileTypeNodes = classNode.SelectNodes("FileTypes");
            if (fileTypeNodes != null)
            {
                foreach (XmlNode fileTypeNode in fileTypeNodes)
                {
                    string extension = fileTypeNode.SelectSingleNode("Filename")?.InnerText;
                    string pronoms = fileTypeNode.SelectSingleNode("Pronoms")?.InnerText;
                    string innerDefault = fileTypeNode.SelectSingleNode("Default")?.InnerText;
                    if (!String.IsNullOrEmpty(innerDefault))
                    {
                        defaultType = innerDefault;
                    }

                    // Remove whitespace and split pronoms string by commas into a list of strings
                    List<string> pronomsList = new List<string>();
                    if (!string.IsNullOrEmpty(pronoms))
                    {
                        pronomsList.AddRange(pronoms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(pronom => pronom.Trim()));
                    }
                    SettingsData settings = new SettingsData
                    {
                        PronomsList = pronomsList,
                        DefaultType = defaultType
                    };
                    if (settings.PronomsList.Count > 0)
                    {
                        foreach (string pronom in settings.PronomsList)
                        {
                            GlobalVariables.FileSettings[pronom] = defaultType;
                        }
                    }                       
                    else
                    {
                        logger.SetUpRunTimeLogMessage("Could not find any pronoms to convert to " + extension, true);
                    }  
                }
            }
            // ============ DO NOT DELETE ===============
            /*
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
            }*/
        }
        catch (Exception ex)
        {

            string exceptionMessage = ex.Message.ToString();
            logger.SetUpRunTimeLogMessage(exceptionMessage, true);
        }
    }
}
