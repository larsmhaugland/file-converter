




public class FileManager
{
    private static FileManager? instance;
    private static readonly object lockObject = new object();
	public List<FileInfo> Files;	// List of files to be converted
	private bool identifyingFiles = false; // True if files are being identified
    private FileManager()
    {
        Files = new List<FileInfo>();
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
    //NOTE: Since this function is async, it may not be finished before the conversion starts, what should we do?
    public async void IdentifyFiles()
    {
		identifyingFiles = true;
		Siegfried sf = Siegfried.Instance;
		Logger logger = Logger.Instance;


		//Identifying all uncompressed files
		//TODO: Why do I get a warning here (without '!')?
		List<FileInfo>? files = await sf.IdentifyFilesIndividually(GlobalVariables.parsedOptions.Input)!; //Search for files in output folder since they are copied there from input folder

		if (files != null)
		{
			//Change path from input to output directory
			foreach (FileInfo file in files)
			{
				//Replace first occurence of input path with output path
				file.FilePath = file.FilePath.Replace(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
			}
			Files.AddRange(files);
		}
		else
		{
			logger.SetUpRunTimeLogMessage("Error when discovering files / No files found", true);
		}

		//Identifying all compressed files
		List<FileInfo>? compressedFiles = sf.IdentifyCompressedFilesJSON(GlobalVariables.parsedOptions.Input)!;

		if (compressedFiles != null)
		{
			Files.AddRange(compressedFiles);
		}
        Files = Files.GroupBy(file => file.FilePath)
                     .Select(group => group.First())
                     .ToList();
        identifyingFiles = false;
    }

    public void ImportFiles(List<FileInfo> files)
    {	
		try
		{
            //Remove files that no longer exist
            files.RemoveAll(file => !File.Exists(file.FilePath));
            //Remove files that are not in the input directory
            files.RemoveAll(file => !FileExistsInDirectory(GlobalVariables.parsedOptions.Input, file.ShortFilePath));

            //Copy files to output directory
            Parallel.ForEach(Files, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, file =>
            {
				if (!FileExistsInDirectory(GlobalVariables.parsedOptions.Output, file.ShortFilePath))
				{
					var newPath = Path.Combine(GlobalVariables.parsedOptions.Output, file.ShortFilePath);
					File.Copy(file.FilePath,newPath);
				}
			});
		} catch (Exception e)
		{
            Logger logger = Logger.Instance;
            logger.SetUpRunTimeLogMessage("Error when copying files: " + e.Message, true);
        }
		Files.AddRange(files);
    }

	public void ImportCompressedFiles(List<FileInfo> files)
	{
        try
        {
            //Remove files that no longer exist
            files.RemoveAll(file => !File.Exists(file.FilePath));
        }
        catch (Exception e)
        {
            Logger logger = Logger.Instance;
            logger.SetUpRunTimeLogMessage("Error when copying files: " + e.Message, true);
        }
        Files.AddRange(files);
    }

	public void DisplayFileList()
	{
		//Get converters supported formats
		var converters = AddConverters.Instance.GetConverters();
		List<Dictionary<string,List<string>>> supportedConversions = new List<Dictionary<string,List<string>>>();
		foreach (var converter in converters)
		{
			var formats = converter.getListOfSupportedConvesions();
			if (formats != null)
			{
				supportedConversions.Add(formats);
			}
		}
		
		//Count the number of files per pronom code
		Dictionary<KeyValuePair<string, string>, int> fileCount = new Dictionary<KeyValuePair<string, string>, int>();
		foreach (FileInfo file in Files)
		{
			string folder = Path.GetDirectoryName(Path.GetRelativePath(GlobalVariables.parsedOptions.Output, file.FilePath));
			string? overrideFormat;
			//Fetch the standard format for the file type
			GlobalVariables.FileSettings.TryGetValue(file.OriginalPronom, out overrideFormat);
			//Check if the file is in a folder with an override
			if (folder != null && GlobalVariables.FolderOverride.ContainsKey(folder))
			{
				overrideFormat = GlobalVariables.FolderOverride[folder].DefaultType;
			}
			bool supported = false;
			//Check if the override format is supported by any of the converters
			foreach (var supportedConversion in supportedConversions)
			{
				if (supportedConversion.ContainsKey(file.OriginalPronom))
				{
					foreach (var outputFormat in supportedConversion[file.OriginalPronom])
					{
						if (outputFormat == overrideFormat)
						{
							supported = true;
							break;
						}
					}
					if (supported) break;
				}
			}
			//If no supported format is found, set the overrideFormat to "Not set"
			if (overrideFormat == null)
			{
				overrideFormat = "Not set";
			}
			if (!supported && overrideFormat != null)
			{
                overrideFormat = "Not supported";
            }

			KeyValuePair<string, string> key = new KeyValuePair<string, string>(file.OriginalPronom, overrideFormat);
			if (fileCount.ContainsKey(key))
			{
				fileCount[key]++;
			}
			else
			{
				fileCount[key] = 1;
			}
		}
		Siegfried sf = Siegfried.Instance;


		//Find the longest format name for current and target formats
		int currentMax = 0;
		int targetMax = 0;
		foreach (KeyValuePair<KeyValuePair<string, string>, int> entry in fileCount)
		{
			var currentFormat = PronomHelper.PronomToFullName(entry.Key.Key);
			var targetFormat = PronomHelper.PronomToFullName(entry.Key.Value);
			if (currentFormat.Length > currentMax)
			{
				currentMax = currentFormat.Length;
			}
			if (targetFormat.Length > targetMax)
			{
				targetMax = targetFormat.Length;
			}
		}

		//Sort the fileCount dictionary by the number of files
		fileCount = fileCount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

		//Print the number of files per pronom code
		var oldColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.WriteLine("\n\n{0,13} - {1,-" + currentMax + "} | {2,13} - {3,-" + targetMax + "} | {4,6}", "Input pronom", "Full name", "Output pronom", "Full name", "Count");
		Console.ForegroundColor = oldColor;

		foreach (KeyValuePair<KeyValuePair<string, string>, int> entry in fileCount)
		{
			var currentPronom = entry.Key.Key;
			var targetPronom = entry.Key.Value;
			var count = entry.Value;
			//Get full name of filetype with pronom code
			var currentFormat = PronomHelper.PronomToFullName(currentPronom);
			var targetFormat = PronomHelper.PronomToFullName(targetPronom);
			Console.WriteLine("{0,13} - {1,-" + currentMax + "} | {2,13} - {3,-" + targetMax + "} | {4,6}", currentPronom, currentFormat, targetPronom, targetFormat, count);
		}
		//Sum total from all entries in fileCount where key. is not "Not set"
		int total = fileCount.Where(x => x.Key.Value != "Not set" && x.Key.Value != "Not supported").Sum(x => x.Value);
		Console.WriteLine("Number of files: {0,-10}\nNumber of files with output specified: {1,-10}", Files.Count, total);
		
	}

	public List<FileInfo> GetFiles()
	{
		while (identifyingFiles)
		{
            Thread.Sleep(100);
        }
		return Files;
	}

	public void DocumentFiles()
	{
		Logger logger = Logger.Instance;
		logger.SetUpDocumentation(Files);
	}

    private static bool FileExistsInDirectory(string directoryPath, string fileName)
    {
		try
		{
			// Check if the directory exists
			if (Directory.Exists(directoryPath))
			{
				// Check if parent directory exists
				if (!Directory.Exists(Directory.GetParent(fileName).FullName))
				{
					return false;
				}
				// Search for the file with the specified pattern in the directory and its subdirectories
				string[] files = Directory.GetFiles(directoryPath, fileName, SearchOption.AllDirectories);

				// If any matching file is found, return true
				return files.Length > 0;
			}
		} 
		catch 
		{
			return false; 
		}
        // If the directory doesn't exist, return false
        return false;

    }
}
