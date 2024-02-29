using static System.Runtime.InteropServices.JavaScript.JSType;

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
  
    public async void IdentifyFiles()
    {
		identifyingFiles = true;
		Siegfried sf = Siegfried.Instance;
		Logger logger = Logger.Instance;
		//TODO: Can maybe run both individually and compressed files at the same time
		//Identifying all uncompressed files
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

	/// <summary>
	/// Prints out a grouped list of all identified input file formats and target file formats with pronom codes and full name. <br></br>
	/// Also gives a count of how many files are in each group.
	/// </summary>
	public void DisplayFileList()
	{
		//Get converters supported formats
		var converters = AddConverters.Instance.GetConverters();

		string notSupportedString = " NS"; //Needs to have a space in front to extract the pronom code from the string
		Dictionary<KeyValuePair<string, string>, int> fileCount = new Dictionary<KeyValuePair<string, string>, int>();
		foreach (FileInfo file in Files)
		{
			string currentPronom = file.NewPronom != "" ? file.NewPronom : file.OriginalPronom;
			string? targetPronom = Settings.GetTargetPronom(file);
			bool supported = false;

            //Check if the conversion is supported by any of the converters
            if (targetPronom != null) { 
				converters.ForEach(c =>
				{
					if (c.SupportsConversion(currentPronom, targetPronom))
					{
						supported = true;
					}
				});
			}
			//If no supported format is found, set the overrideFormat to "Not set"
			if (targetPronom == null)
			{
				targetPronom = "Not set";
			}
			else if (!supported)
			{
                targetPronom = targetPronom + notSupportedString;
            }

			KeyValuePair<string, string> key = new KeyValuePair<string, string>(currentPronom, targetPronom);
			if (fileCount.ContainsKey(key))
			{
				fileCount[key]++;
			}
			else
			{
				fileCount[key] = 1;
			}
		}

		//Find the longest format name for current and target formats
		int currentMax = 0;
		int targetMax = 0;
		foreach (KeyValuePair<KeyValuePair<string, string>, int> entry in fileCount)
		{
			var currentFormat = PronomHelper.PronomToFullName(entry.Key.Key);
			var targetFormat = PronomHelper.PronomToFullName(entry.Key.Value);
            if (entry.Key.Value.Contains(notSupportedString))
			{
				targetFormat = PronomHelper.PronomToFullName(entry.Key.Value.Split(" ")[0]);
			}
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
			Console.ForegroundColor = oldColor;
			var currentPronom = entry.Key.Key;
			var targetPronom = entry.Key.Value;
			var count = entry.Value;
            //Get full name of filetype with pronom code
            var currentFormat = PronomHelper.PronomToFullName(currentPronom);
            var targetFormat = PronomHelper.PronomToFullName(targetPronom);
            if (targetPronom.Contains(notSupportedString))
			{
				var split = targetPronom.Split(" ")[0];
                targetFormat = PronomHelper.PronomToFullName(split);
				Console.ForegroundColor = ConsoleColor.Red;
            } else if (targetPronom == "Not set")
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
			} else if (targetPronom == currentPronom)
			{
				Console.ForegroundColor = ConsoleColor.Green;
			}

            Console.WriteLine("{0,13} - {1,-" + currentMax + "} | {2,13} - {3,-" + targetMax + "} | {4,6}", currentPronom, currentFormat, targetPronom, targetFormat, count);
		}
		
		//Sum total from all entries in fileCount where key. is not "Not set" or "Not supported"
		int total = fileCount.Where(x => x.Key.Value != "Not set" && !x.Key.Value.Contains(notSupportedString)).Sum(x => x.Value);
		//Sum total from all entries in fileCount where the input pronom is the same as the output pronom
        int totalFinished = fileCount.Where(x => x.Key.Key == x.Key.Value).Sum(x => x.Value);
        //Print totals to user
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Number of files: {0,-10}", Files.Count);
		Console.WriteLine("Number of files with output specified: {0,-10}", total);
		Console.WriteLine("Number of files not at target pronom: {0,-10}", total-totalFinished);
        Console.ForegroundColor = oldColor;
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
