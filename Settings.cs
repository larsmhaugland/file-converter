using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
public class SettingsData
{
    public List<string>? PronomsList { get; set; }
    public string ConvertTo { get; set; } = "";
    public string DefaultType { get; set; } = "";
}
class Settings
{
    private static Settings? instance;
    private static readonly object lockObject = new object();
    public static Settings Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new Settings();
                    }
                }
            }
            return instance;
        }
    }
    public void ReadSettings(string pathToSettings)
    {
        Logger logger = Logger.Instance;
        try
        {
            // Load the XML document from a file
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathToSettings);

            if (xmlDoc.DocumentElement == null) { logger.SetUpRunTimeLogMessage("Could not find settings file", true, filename: pathToSettings); return; }

            // Access the root element
            XmlNode? root = xmlDoc.SelectSingleNode("/root");
            if (root == null) { logger.SetUpRunTimeLogMessage("Could not find root", true, filename: pathToSettings); return; }
            // Access the Requester and Converter elements
            XmlNode? requesterNode = root?.SelectSingleNode("Requester");
            XmlNode? converterNode = root?.SelectSingleNode("Converter");

            Logger.JsonRoot.requester = requesterNode?.InnerText.Trim(); 
            Logger.JsonRoot.converter = converterNode?.InnerText.Trim();

            string? checksumHashing = root?.SelectSingleNode("ChecksumHashing")?.InnerText;
            if (checksumHashing != null)
            {
                checksumHashing = checksumHashing.ToUpper().Trim();
                switch (checksumHashing)
                {
                    case "MD5": GlobalVariables.checksumHash = HashAlgorithms.MD5; break;
                    default: GlobalVariables.checksumHash = HashAlgorithms.SHA256; break;
                }
            }

            // Access elements and attributes
            XmlNodeList? classNodes = root?.SelectNodes("FileClass");
            if (classNodes == null) { logger.SetUpRunTimeLogMessage("Could not find any classNodes", true, filename: pathToSettings); return; }
            foreach (XmlNode classNode in classNodes)
            {
                string? className = classNode?.SelectSingleNode("ClassName")?.InnerText;
                string? defaultType = classNode?.SelectSingleNode("Default")?.InnerText;

                if (defaultType == null)
                {
                    //TODO: This should not be thrown, but rather ask the user for a default type
                    throw new Exception("No default type found in settings");
                }
                XmlNodeList? fileTypeNodes = classNode?.SelectNodes("FileTypes");
                if (fileTypeNodes != null)
                {
                    foreach (XmlNode fileTypeNode in fileTypeNodes)
                    {
                        string? extension = fileTypeNode.SelectSingleNode("Filename")?.InnerText;
                        string? pronoms = fileTypeNode.SelectSingleNode("Pronoms")?.InnerText;
                        string? innerDefault = fileTypeNode.SelectSingleNode("Default")?.InnerText;
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
                            DefaultType = defaultType //TODO: Why is defaultType possibly null reference?
                        };
                        if (settings.PronomsList.Count > 0 && !String.IsNullOrEmpty(defaultType))
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
            logger.SetUpRunTimeLogMessage(ex.Message, true);
        }
    }
}

