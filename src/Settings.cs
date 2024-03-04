using System.Xml;
public class SettingsData
{
	// List of input pronom codes
    public List<string> PronomsList { get; set; } = new List<string>();
    //public string ConvertTo { get; set; } = "";
	// Default file type to convert to
    public string DefaultType { get; set; } = "";
	// Whether to merge images or not
	public bool Merge { get; set; } = false;
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
            XmlNode? inputNode = root?.SelectSingleNode("InputFolder");
            XmlNode? outputNode = root?.SelectSingleNode("OutputFolder");
            XmlNode? maxThreadsNode = root?.SelectSingleNode("MaxThreads");
			XmlNode? timeoutNode = root?.SelectSingleNode("Timeout");

			string? requester = requesterNode?.InnerText.Trim();
            string? converter = converterNode?.InnerText.Trim();
            if (!String.IsNullOrEmpty(requester))
			{
                Logger.JsonRoot.requester = requester;
            } 
			if (!String.IsNullOrEmpty(converter))
			{
                Logger.JsonRoot.converter = converter;
            }
			string? input = inputNode?.InnerText.Trim();
			string? output = outputNode?.InnerText.Trim();
			if (!String.IsNullOrEmpty(input))
			{
                GlobalVariables.parsedOptions.Input = input;
            }
			if(!String.IsNullOrEmpty(output))
			{
                GlobalVariables.parsedOptions.Output = output;
            }
            string? inputMaxThreads = maxThreadsNode?.InnerText;
            if (!String.IsNullOrEmpty(inputMaxThreads) && int.TryParse(inputMaxThreads, out int maxThreads))
            {
                GlobalVariables.maxThreads = maxThreads;
            }
			string? inputTimeout = timeoutNode?.InnerText;
			if (!String.IsNullOrEmpty(inputTimeout) && int.TryParse(inputTimeout, out int timeout))
			{
				GlobalVariables.timeout = timeout;
			}
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
						if (String.IsNullOrEmpty(innerDefault))
						{
                            innerDefault = defaultType;
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
							DefaultType = innerDefault
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
			string outputFolder = GlobalVariables.parsedOptions.Output;
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
					string? merge = folderOverrideNode.SelectSingleNode("MergeImages")?.InnerText.ToUpper().Trim();

                    List<string> pronomsList = new List<string>();
                    if (!string.IsNullOrEmpty(pronoms))
                    {
                        pronomsList.AddRange(pronoms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(pronom => pronom.Trim()));
                    }

					SettingsData settings = new SettingsData
					{
						PronomsList = pronomsList,
						DefaultType = folderOverrideNode.SelectSingleNode("ConvertTo")?.InnerText,
						Merge = merge == "YES",
					};

                    bool folderPathEmpty = String.IsNullOrEmpty(folderPath);
                    bool pronomsEmpty = String.IsNullOrEmpty(pronoms);
                    bool convertToEmpty = String.IsNullOrEmpty(settings.DefaultType);

                    if (folderPathEmpty || pronomsEmpty || convertToEmpty)
                    {
                        logger.SetUpRunTimeLogMessage("something wrong with a folderOverride in settings", true);
                    }
                    else
                    {
						if (Directory.Exists(outputFolder + "/" + folderPath)) 
						{ 
							GlobalVariables.FolderOverride[folderPath] = settings;
						
							List<string> subfolders = GetSubfolderPaths(folderPath);
							if (subfolders.Count > 0)
							{
								foreach (string subfolder in subfolders)
								{
									// Check if the subfolder is already in the FolderOverride Map
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
        }
        catch (Exception ex)
        {
            logger.SetUpRunTimeLogMessage(ex.Message, true);
        }
    }
    /// <summary>
    /// Recursively retrieves all subfolders of a given parent folder.
    /// </summary>
    /// <param name="folderName">the name of the parent folder</param>
    /// <returns>list with paths to all subfolders</returns>
    private static List<string> GetSubfolderPaths(string folderName)
    {
        string outputPath = GlobalVariables.parsedOptions.Output;
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
                    subfolders.AddRange(GetSubfolderPaths(Path.Combine(folderName, Path.GetFileName(subfolder))));
                }
            }
            else
            {
                Console.WriteLine($"Folder '{folderName}' does not exist in '{outputPath}'");
                Logger.Instance.SetUpRunTimeLogMessage($"Folder '{folderName}' does not exist in '{outputPath}'", true, filename: folderName);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Logger.Instance.SetUpRunTimeLogMessage("You do not have permission to access this folder", true, filename: outputPath);
        }

		return subfolders;
	}

	public static string? GetTargetPronom(FileInfo f)
	{
        //Get the parent directory of the file
        var parentDir = Path.GetDirectoryName(Path.GetRelativePath(GlobalVariables.parsedOptions.Output, f.FilePath));
        //If the file is in a folder that has a folder override, check if the file is at the correct output format for that folder
        if (parentDir != null && GlobalVariables.FolderOverride.ContainsKey(parentDir))
        {
			if (GlobalVariables.FolderOverride[parentDir].PronomsList.Contains(f.OriginalPronom))
			{
				return GlobalVariables.FolderOverride[parentDir].DefaultType;
			}
        }
        //Otherwise, check if the new type matches the global settings for the input format
        if (GlobalVariables.FileSettings.ContainsKey(f.OriginalPronom))
        {
			return GlobalVariables.FileSettings[f.OriginalPronom];
        }
		return null;
    }
}

