using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
public class SettingsData
{
    public List<string> PronomsList { get; set; } = new List<string>();
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
    /// <summary>
    /// Reads settings from file
    /// </summary>
    /// <param name="pathToSettings"> the path to the settings file from working directory </param>
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
                            DefaultType = defaultType
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
        }
        catch (Exception ex)
        {
            logger.SetUpRunTimeLogMessage(ex.Message, true);
        }
    }
    /// <summary>
    /// Sets up the FolderOverride Dictionary
    /// </summary>
    /// <param name="pathToSettings"> relative path to settings file from working directory </param>
    public void SetUpFolderOverride(string pathToSettings)
    {
        Logger logger = Logger.Instance;
        try
        {
            // Load the XML document from a file
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathToSettings);

            if (xmlDoc.DocumentElement == null) { logger.SetUpRunTimeLogMessage("Could not find settings file", true, filename: pathToSettings); return; }


            XmlNodeList? folderOverrideNodes = xmlDoc.SelectNodes("/root/FolderOverride");
            if (folderOverrideNodes != null)
            {
                foreach (XmlNode folderOverrideNode in folderOverrideNodes)
                {
                    string? folderPath = folderOverrideNode.SelectSingleNode("FolderPath")?.InnerText;
                    string? pronoms = folderOverrideNode.SelectSingleNode("Pronoms")?.InnerText;

                    List<string> pronomsList = new List<string>();
                    if (!string.IsNullOrEmpty(pronoms))
                    {
                        pronomsList.AddRange(pronoms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(pronom => pronom.Trim()));
                    }

                    SettingsData settings = new SettingsData
                    {
                        PronomsList = pronomsList,
                        DefaultType = folderOverrideNode.SelectSingleNode("ConvertTo")?.InnerText
                    };

                    bool folderPathEmpty = String.IsNullOrEmpty(folderPath);
                    bool pronomsEmpty = String.IsNullOrEmpty(pronoms);
                    bool convertToEmpty = String.IsNullOrEmpty(settings.DefaultType);

                    if (folderPathEmpty && pronomsEmpty && convertToEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("empty folderOverride in settings", true);
                    }
                    else if (folderPathEmpty && !pronomsEmpty && !convertToEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("folderpath is empty for " + settings.PronomsList + "-" + settings.DefaultType + " in settings", true);
                    }
                    else if (pronomsEmpty && !convertToEmpty && !folderPathEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("pronomlist is empty for " + folderPath + " in settings", true);
                    }
                    else if (convertToEmpty && !pronomsEmpty && !folderPathEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("convertTo is empty for " + folderPath + " in settings", true);
                    }
                    else
                    {
                        //string outputPlusfolderPath = GlobalVariables.parsedOptions.Output + "/" + folderPath;
                        GlobalVariables.FolderOverride[folderPath] = settings;
                        List<string> subfolders = GetSubfolderPaths(GlobalVariables.parsedOptions.Output, folderPath);
                        if (subfolders.Count > 0)
                        {
                            foreach (string subfolder in subfolders)
                            {
                                if (!GlobalVariables.FolderOverride.ContainsKey(subfolder))
                                {
                                    GlobalVariables.FolderOverride[subfolder] = settings;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.SetUpRunTimeLogMessage(ex.Message, true);
        }
    }

    private static List<string> GetSubfolderPaths(string outputPath, string folderName)
    {
        List<string> subfolders = new List<string>();

        try
        {
            string targetFolderPath = Path.Combine(outputPath, folderName);
            string relativePath = Path.GetRelativePath(outputPath, targetFolderPath); // Calculate the relative path for the current folder

            if (Directory.Exists(targetFolderPath))
            {
                // Add current folder to subfolders list
                subfolders.Add(relativePath);

                // Add immediate subfolders
                foreach (string subfolder in Directory.GetDirectories(targetFolderPath))
                {
                    // Recursively get subfolders of each subfolder
                    subfolders.AddRange(GetSubfolderPaths(outputPath, Path.Combine(folderName, Path.GetFileName(subfolder))));
                }
            }
            else
            {
                Console.WriteLine($"Folder '{folderName}' does not exist under '{outputPath}'");
            }
        }
        catch (UnauthorizedAccessException)
        {
            Logger.Instance.SetUpRunTimeLogMessage("You do not have permission to access this folder", true, filename: outputPath);
        }

        return subfolders;
    }
}

