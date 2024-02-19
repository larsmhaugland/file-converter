

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
		List<FileInfo>? compressedFiles = await sf.IdentifyCompressedFilesJSON(GlobalVariables.parsedOptions.Input)!;

		if (compressedFiles != null)
		{
			Files.AddRange(compressedFiles);
		}
        Files = Files.GroupBy(file => file.FilePath)
                     .Select(group => group.First())
                     .ToList();
        identifyingFiles = false;
    }

	public void DisplayFileList()
	{
		//Count the number of files per pronom code
		Dictionary<KeyValuePair<string,string>, int> fileCount = new Dictionary<KeyValuePair<string,string>, int>();
		foreach(FileInfo file in Files)
		{
			string folder = Path.GetDirectoryName(Path.GetRelativePath(GlobalVariables.parsedOptions.Output, file.FilePath));
			string? overrideFormat;
            GlobalVariables.FileSettings.TryGetValue(file.OriginalPronom, out overrideFormat);
            if (folder != null && GlobalVariables.FolderOverride.ContainsKey(folder))
            {
                overrideFormat = GlobalVariables.FolderOverride[folder].DefaultType;
            }

			if (overrideFormat == null)
			{
                overrideFormat = "Not set";
            }

			KeyValuePair<string,string> key = new KeyValuePair<string,string>(file.OriginalPronom, overrideFormat);
			if (fileCount.ContainsKey(key))
			{
				fileCount[key]++;
			} else
			{
				fileCount[key] = 1;
			}
        }

		//Print the number of files per pronom code
		Console.WriteLine("Input format | Output format | Count");
		foreach(KeyValuePair<KeyValuePair<string,string>, int> entry in fileCount)
		{
            Console.WriteLine("{0,10} | {1,10} | {2, 6}",entry.Key.Key,entry.Key.Value,entry.Value);
        }
		//Sum total from all entries in fileCount where key. is not "Not set"
		int total = fileCount.Where(x => x.Key.Value != "Not set").Sum(x => x.Value);
		Console.WriteLine("Number of files: {0,-10}\nNumber of files with output specified: {1,-10}", Files.Count,total);
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
}
