using SharpCompress;
using System.Collections.Concurrent;

public class FileManager
{
    private static FileManager? instance;
    private static readonly object lockObject = new object();
	public ConcurrentDictionary<Guid,FileInfo> Files;	// List of files to be converted
	private static readonly object identifyingFiles = new object(); // True if files are being identified
    private FileManager()
    {
        Files = new ConcurrentDictionary<Guid, FileInfo>();
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
  
    public void IdentifyFiles()
    {
		lock (identifyingFiles)
		{
			Siegfried sf = Siegfried.Instance;
			Logger logger = Logger.Instance;
			//TODO: Can maybe run both individually and compressed files at the same time
			//Identifying all uncompressed files
			List<FileInfo>? files = sf.IdentifyFilesIndividually(GlobalVariables.parsedOptions.Input)!.Result; //Search for files in output folder since they are copied there from input folder
			if (files != null)
			{
				//TODO: Should be more robust
				//Change path from input to output directory
				foreach (FileInfo file in files)
				{
					//Replace first occurence of input path with output path
					file.FilePath = file.FilePath.Replace(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
					Guid id = Guid.NewGuid();
					file.Id = id;
					Files.TryAdd(id, file);
				}

			}
			else
			{
				logger.SetUpRunTimeLogMessage("Error when discovering files / No files found", true);
			}

			//Identifying all compressed files
			List<FileInfo>? compressedFiles = sf.IdentifyCompressedFilesJSON(GlobalVariables.parsedOptions.Input)!;

			foreach (var file in compressedFiles)
			{
				Guid id = Guid.NewGuid();
				file.Id = id;
				Files.TryAdd(id, file);
			}
		}
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
            Parallel.ForEach(Files.Values, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, file =>
            {
				if (!FileExistsInDirectory(GlobalVariables.parsedOptions.Output, file.ShortFilePath))
				{
					var newPath = Path.Combine(GlobalVariables.parsedOptions.Output, file.ShortFilePath);
					File.Copy(file.FilePath,newPath);
				}
				Guid id = Guid.NewGuid();
				file.Id = id;
				Files.TryAdd(id, file);
			});
		} catch (Exception e)
		{
            Logger logger = Logger.Instance;
            logger.SetUpRunTimeLogMessage("Error when copying files: " + e.Message, true);
        }
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
        files.ForEach(file =>
		{
            Guid id = Guid.NewGuid();
            file.Id = id;
            Files.TryAdd(id, file);
        });
    }

	/// <summary>
	/// Prints out a grouped list of all identified input file formats and target file formats with pronom codes and full name. <br></br>
	/// Also gives a count of how many files are in each group.
	/// </summary>
	public void DisplayFileList()
	{
		//Get converters supported formats
		var converters = AddConverters.Instance.GetConverters();
		bool result = false;

		var totalIsConverted = Files.Values.Count(x => x.IsConverted == true);

		string notSupportedString = " (Not supported)"; //Needs to have a space in front to extract the pronom code from the string
		Dictionary<KeyValuePair<string, string>, int> fileCount = new Dictionary<KeyValuePair<string, string>, int>();
		foreach (FileInfo file in Files.Values)
		{
			//Skip files that should be merged
			if (file.ShouldMerge)
			{
				continue;
			}
			//If NewPronom is set, the conversion is done and result should be printed out	
            if (file.NewPronom != "")
            {
                result = true;
            }
            string currentPronom = (file.NewPronom != "") ? file.NewPronom : file.OriginalPronom;
			string? targetPronom = Settings.GetTargetPronom(file);
			var col = Console.ForegroundColor;

            if (file.NewPronom == "" && !file.ShouldMerge)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("New pronom not set for file: {0}", file.FilePath);
			}
			if(targetPronom != currentPronom && !file.ShouldMerge)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				//Console.WriteLine("Target: {0} | Current: {1} | Filename: {2}", targetPronom, currentPronom, file.FilePath);
			}
			Console.ForegroundColor = col;
			bool supported = false;
			
            //Check if the conversion is supported by any of the converters
            if (targetPronom != null) { 
				converters.ForEach(c =>
				{
					//Check if the conversion is directly supported by the converter or if it is supported by the ConversionManager through different converters
					if (c.SupportsConversion(currentPronom, targetPronom) || ConversionManager.Instance.SupportsConversion(currentPronom,targetPronom))
					{
						supported = true;
						return;
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
			//Add new entry in dictionary or add to count if entry already exists
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
				targetFormat = PronomHelper.PronomToFullName(entry.Key.Value.Split(" ")[0]) + notSupportedString;
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
		//Adjust length to be at least as big as the column name
		currentMax = Math.Max(currentMax, "Full name".Length);
        targetMax  = Math.Max(targetMax, "Full name".Length);

        //Sort the fileCount dictionary by the number of files
        fileCount = fileCount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);


		var firstName = result ? "Actual pronom" : "Input pronom";
		var secondName = result ? "Target pronom" : "Output pronom";

		//Print the number of files per pronom code
		var oldColor = Console.ForegroundColor;
		Console.ForegroundColor = GlobalVariables.INFO_COL;
		Console.WriteLine("\n\n{0,13} - {1,-" + currentMax + "} | {2,13} - {3,-" + targetMax + "} | {4,6}", firstName, "Full name", secondName, "Full name", "Count");

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
                targetFormat = PronomHelper.PronomToFullName(split) + notSupportedString;
				targetPronom = split;
				Console.ForegroundColor = GlobalVariables.ERROR_COL;
            } else if (targetPronom == "Not set")
			{
				Console.ForegroundColor = GlobalVariables.WARNING_COL;
			} else if (targetPronom == currentPronom)
			{
				Console.ForegroundColor = GlobalVariables.SUCCESS_COL;
			}

            Console.WriteLine("{0,13} - {1,-" + currentMax + "} | {2,13} - {3,-" + targetMax + "} | {4,6}", currentPronom, currentFormat, targetPronom, targetFormat, count);
		}
		
		//Sum total from all entries in fileCount where key. is not "Not set" or "Not supported"
		int total = fileCount.Where(x => x.Key.Value != "Not set" && !x.Key.Value.Contains(notSupportedString)).Sum(x => x.Value);
		//Sum total from all entries in fileCount where the input pronom is the same as the output pronom
        int totalFinished = fileCount.Where(x => x.Key.Key == x.Key.Value).Sum(x => x.Value);
        //Print totals to user
        Console.ForegroundColor = GlobalVariables.INFO_COL;
        Console.WriteLine("Number of files: {0,-10}", Files.Count);
		Console.WriteLine("Number of files with output specified: {0,-10}", total);
		Console.WriteLine("Number of files not at target pronom: {0,-10}", total-totalFinished);
		//Get a list of all directories that will be merged
		List<string> mergedirs = new List<string>();
		foreach (var entry in GlobalVariables.FolderOverride)
		{
			if (entry.Value.Merge)
			{
				mergedirs.Add(entry.Key);
			}
		}
		//Print out the directories that will be or have been merged
		if (mergedirs.Count > 0)
		{
			//Print plan for merge
			if (!result)
			{
				Console.WriteLine("Some folders will be merged:");
				foreach (var dir in mergedirs)
				{
					Console.WriteLine("\t{0}", dir);
				}
			}
			//Check result of merge
			else
			{
                List<string> mergedDirs = new List<string>();
                foreach (var file in Files.Values)
                {
                    var parent = Path.GetRelativePath(GlobalVariables.parsedOptions.Output, Directory.GetParent(file.FilePath)?.ToString() ?? "");
					//Check if file was merged, only add the parent directory once
                    if (!mergedDirs.Contains(parent) && mergedirs.Contains(parent) && file.IsMerged)
                    {
                        mergedDirs.Add(parent);
                    }
                }
				//Get the directories that were not merged
                var notMerged = mergedirs.Except(mergedDirs);
				//Print out the result of the merge
				Console.WriteLine("{0}/{1} folders were merged:", mergedDirs.Count, mergedirs.Count);
				Console.ForegroundColor = GlobalVariables.SUCCESS_COL;
				foreach (var dir in mergedDirs)
				{
					Console.WriteLine("\t{0}", dir);
				}
				Console.ForegroundColor = GlobalVariables.ERROR_COL;
				foreach (var dir in notMerged)
				{
					Console.WriteLine("\t{0}", dir);
				}
            }
		}

        Console.ForegroundColor = oldColor;

    }

	public List<FileInfo> GetFiles()
	{
		lock (identifyingFiles)
		{
			return Files.Values.ToList();
		}
	}

	public FileInfo? GetFile(Guid id)
	{
		if (!Files.ContainsKey(id)) return null;
		return Files[id];
	}

	public void DocumentFiles()
	{
		Logger logger = Logger.Instance;
		List<FileInfo> files = new List<FileInfo>();
		foreach (var file in Files)
		{
            files.Add(file.Value);
        }
		logger.SetUpDocumentation(files);
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
