using iText.Barcodes.Dmcode;
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
	//NOTE: Since this function is async, it may not be finished before the conversion starts, what should we do?
	public async void IdentifyFiles()
	{
		Siegfried sf = Siegfried.Instance;
		Logger logger = Logger.Instance;


		//Identifying all uncompressed files
		//TODO: Why do I get a warning here (without '!')?
		List<FileInfo> ?files = await sf.IdentifyFilesIndividually(GlobalVariables.parsedOptions.Input)!; //Search for files in output folder since they are copied there from input folder
		
		if(files != null)
		{
			//Change path from input to output directory
			foreach(FileInfo file in files)
			{
				//Replace first occurence of input path with output path
				file.FilePath = file.FilePath.Replace(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
			}
			Files.AddRange(files);
		} else
		{
			logger.SetUpRunTimeLogMessage("Error when discovering files / No files found", true);
		}

		//Identifying all compressed files
		List<FileInfo> ?compressedFiles = await sf.IdentifyCompressedFilesJSON(GlobalVariables.parsedOptions.Input)!;

		if(compressedFiles != null)
		{
			Files.AddRange(compressedFiles);
		}

	}

	public void DisplayFileList()
	{
		//Count the number of files per pronom code
		Dictionary<KeyValuePair<string,string>, int> fileCount = new Dictionary<KeyValuePair<string,string>, int>();
		foreach(FileInfo file in Files)
		{
			string folder = Path.GetDirectoryName(Path.GetRelativePath(GlobalVariables.parsedOptions.Output, file.FilePath));
			string overrideFormat = "";
			/*
            //Check folder override and output format
            if (folder != null && FolderOverride.ContainsKey(folder))
			{
				overrideFormat = FolderOverride[folder].Key;
			}
            if(fileCount.ContainsKey(file.OriginalPronom))
			{
				fileCount[file.OriginalPronom]++;
            } else
			{
				fileCount[file.OriginalPronom] = 1;
            }*/
        }

		//Print the number of files per pronom code
		Console.WriteLine("Pronom code: Number of files");
		foreach(KeyValuePair<KeyValuePair<string,string>, int> entry in fileCount)
		{
            Console.WriteLine(entry.Key + ": " + entry.Value + " Output format: ");
        }
	}

	public List<FileInfo> GetFiles()
	{
		if(Files.Count == 0)
		{
			//TODO: Should maybe wait?
			IdentifyFiles();
		}
		return Files;
	}

	public void DocumentFiles()
	{
		Logger logger = Logger.Instance;
		logger.SetUpDocumentation(Files);
	} 
}
