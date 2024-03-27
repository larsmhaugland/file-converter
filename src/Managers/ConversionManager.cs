using SharpCompress;
using System.Collections.Concurrent;

public class FileToConvert
{
	public string FilePath { get; set; }            //From FileInfo
	public string CurrentPronom { get; set; }       //From FileInfo
	public string TargetPronom { get; set; }        //From Dictionary
	public List<string> Route { get; set; }         //From Dictionary
	public bool IsModified { get; set; } = false;   //True if file has been worked on
	public bool Failed { get; set; } = false;       //True if the file has failed conversion without throwing an exception
	public CancellationToken ct { get; set; }        //CancellationToken for the conversion
	public Guid Id { get; set; }   //Unique identifier for the file

	public FileToConvert(FileInfo file)
	{
		FilePath = file.FilePath;
		CurrentPronom = file.OriginalPronom;
		TargetPronom = Settings.GetTargetPronom(file) ?? CurrentPronom;
		Route = new List<string>();
		Id = file.Id;
	}
	public FileToConvert(string path, Guid id)
	{
		FilePath = path;
		CurrentPronom = "";
		TargetPronom = "";
		Route = new List<string>();
		IsModified = false;
		Id = id;
	}
}

public class ConversionManager
{
	ConcurrentDictionary<KeyValuePair<string, string>, List<string>> ConversionMap = new ConcurrentDictionary<KeyValuePair<string, string>, List<string>>();
	public ConcurrentDictionary<Guid, FileInfo> FileInfoMap = new ConcurrentDictionary<Guid, FileInfo>();
	public Dictionary<string, string> WorkingSetMap = new Dictionary<string, string>();
	private static ConversionManager? instance;
	private static readonly object lockObject = new object();

	List<Converter> Converters;
	List<string> WordPronoms = [
		"x-fmt/329", "fmt/609", "fmt/39", "x-fmt/274",
		"x-fmt/275", "x-fmt/276", "fmt/1688", "fmt/37",
		"fmt/38", "fmt/1282", "fmt/1283", "x-fmt/131",
		"x-fmt/42", "x-fmt/43", "fmt/473", "fmt/40",
		"x-fmt/44", "fmt/523", "fmt/1827", "fmt/412",
		"fmt/754", "x-fmt/393", "x-fmt/394", "fmt/892",
		"fmt/494"
	];
	List<string> ImagePronoms = [
		"fmt/3", "fmt/4", "fmt/11", "fmt/12",
		"fmt/13", "fmt/935", "fmt/41", "fmt/42",
		"fmt/43", "fmt/44", "x-fmt/398", "x-fmt/390",
		"x-fmt/391", "fmt/645", "fmt/1507", "fmt/112",
		"fmt/367", "fmt/1917", "x-fmt/399", "x-fmt/388",
		"x-fmt/387", "fmt/155", "fmt/353", "fmt/154",
		"fmt/153", "fmt/156", "x-fmt/270", "fmt/115",
		"fmt/118", "fmt/119", "fmt/114", "fmt/116",
		"fmt/117"
	];
	List<string> HTMLPronoms = [
		"fmt/103", "fmt/96", "fmt/97", "fmt/98",
		"fmt/99", "fmt/100", "fmt/471", "fmt/1132",
		"fmt/102", "fmt/583"
	];
	List<string> PDFPronoms = [
		"fmt/559", "fmt/560", "fmt/561", "fmt/562",
		"fmt/563", "fmt/564", "fmt/565", "fmt/558",
		"fmt/14", "fmt/15", "fmt/16", "fmt/17",
		"fmt/18", "fmt/19", "fmt/20", "fmt/276",
		"fmt/95", "fmt/354", "fmt/476", "fmt/477",
		"fmt/478", "fmt/479", "fmt/480", "fmt/481",
		"fmt/1910", "fmt/1911", "fmt/1912", "fmt/493",
		"fmt/144", "fmt/145", "fmt/157", "fmt/146",
		"fmt/147", "fmt/158", "fmt/148", "fmt/488",
		"fmt/489", "fmt/490", "fmt/492", "fmt/491",
		"fmt/1129", "fmt/1451"
	];
	List<string> ExcelPronoms = [
		"fmt/55", "fmt/56", "fmt/57", "fmt/61",
		"fmt/595", "fmt/445", "fmt/214", "fmt/1828",
		"fmt/494", "fmt/62", "fmt/59", "fmt/598"
	];
	List<string> PPTPronoms = [
		"fmt/1537", "fmt/1866", "fmt/181", "fmt/1867",
		"fmt/179", "fmt/1747", "fmt/1748", "x-fmt/88",
		"fmt/125", "fmt/126", "fmt/487", "fmt/215",
		"fmt/1829", "fmt/494", "fmt/631"
	];
	List<string> OpenDocPronoms = [
		"fmt/140", "fmt/135", "fmt/136", "fmt/137",
		"fmt/138", "fmt/139", "x-fmt/3", "fmt/1756",
		"fmt/290", "fmt/291", "fmt/1755", "fmt/294",
		"fmt/295", "fmt/1754", "fmt/292", "fmt/293"
	];
	List<string> RichTextPronoms = [
			"fmt/969", "fmt/45", "fmt/50", "fmt/52",
		"fmt/53", "fmt/355"
	];
	List<string> EmailPronoms = [
			"x-fmt/248", "x-fmt/249", "x-fmt/430", "fmt/1144",
		"fmt/278", "fmt/950"
	];
	List<string> CompressedFolderPronoms = [
			"x-fmt/263", "x-fmt/265", "fmt/484", "fmt/266",
		"x-fmt/264", "fmt/411", "fmt/613"
	];

	/// <summary>
	/// initializes the map for how to reach each format
	/// </summary>
	private void initMap()
	{
		/*
		LibreOfficeConverter converter = new LibreOfficeConverter();
		List<string> supportedConversionsLibreOffice = new List<string>(converter.SupportedConversions?.Keys);
		string pdfA = "fmt/477";
		string pdfPronom = OperatingSystem.IsLinux() ? "fmt/20" : "fmt/276";
		foreach(FileInfo file in FileManager.Instance.Files.Values)
		{
			if (Settings.GetTargetPronom(file) == pdfA && supportedConversionsLibreOffice.Contains(file.OriginalPronom))
			{
				ConversionMap.TryAdd(new KeyValuePair<string, string>(file.OriginalPronom, pdfA), [pdfPronom, pdfA]);
			}
		}
		*/
	}

	private void initFileMap()
	{
		foreach (FileInfo file in FileManager.Instance.Files.Values)
		{
			FileInfoMap.TryAdd(file.Id, file);
		}
	}

	/// <summary>
	/// Removes the entries in the ConversionMap where a part of the route is not supported by any converter present.
	/// </summary>
	void FilterConversionMap()
	{
		var toDelete = new List<KeyValuePair<string,string>>();
		Parallel.ForEach(ConversionMap, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, entry =>
		{
			bool supported = false;
			var route = entry.Value;
			var prev = entry.Key.Key;
			foreach(var pronom in route)
			{
				supported = false;
				foreach(Converter c in Converters)
				{
					if (c.SupportsConversion(prev, pronom))
					{
						supported = true;
					}
				}
				if (!supported)
				{
					toDelete.Add(entry.Key);
					return;
				}
				prev = pronom;
			}
			
		});

		//Remove all entries that are not supported by any converter
		foreach (var key in toDelete)
		{
			ConversionMap.TryRemove(key, out _);
		}

	}

	/// <summary>
	/// 
	/// </summary>
	public ConversionManager()
	{
		//Initialize conversion map
		initMap();

		//Initialize converters
		Converters = AddConverters.Instance.GetConverters();

		//Initialize FileMap
		initFileMap();
		FilterConversionMap();
	}

	public static ConversionManager Instance
	{
		get
		{
			lock (lockObject)
			{
				if (instance == null)
				{
					instance = new ConversionManager();
				}
				return instance;
			}
		}
	}

	

	public bool SupportsConversion(string currentPronom, string targetPronom)
	{
		return ConversionMap.ContainsKey(new KeyValuePair<string, string>(currentPronom, targetPronom));
	}

	/// <summary>
	/// Updates the FileInfo list with new data after conversion
	/// </summary>
	/// <summary>
	/// Updates the FileInfo list with new data after conversion
	/// </summary>
	void CheckConversion()
	{
		var files = FileManager.Instance.Files.Values.ToList();
		//Run siegfried on all files
		var f = Siegfried.Instance.IdentifyFilesIndividually(files)?.Result;

		//If siegfried fails, log error message and return
		if (f == null)
		{
			Console.WriteLine("Could not identify files after conversion");
			Logger.Instance.SetUpRunTimeLogMessage("CM CheckConversion: Could not identify files", true);
			return;
		}
		Dictionary<Guid, FileInfo> fDict = f.ToDictionary(x => x.Id, x => x);

		//Update FileInfoMap with new data
		//Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, file =>
		//DEBUG ONLY        Console.WriteLine("{0,40} | {1,10} | {2,10} | {3,10}", "File Path", "Actual", "Target", "Original");
		files.ForEach(file =>
		{
			if (fDict.ContainsKey(file.Id))
			{
				file.UpdateSelf(fDict[file.Id]);
				file.IsConverted = Settings.GetTargetPronom(file) == file.NewPronom;
			}
			else
			{
				Console.BackgroundColor = Console.BackgroundColor;
			}
			if (!file.IsConverted && !file.NotSupported && !file.OutputNotSet)
			{
				//DEBUG ONLY    Console.WriteLine("{0,40} | {1,10} | {2,10} | {3,10}",Path.GetFileName(file.FilePath),file.NewPronom,Settings.GetTargetPronom(file), file.OriginalPronom);
			}
		});
	}

	/// <summary>
	/// Responsible for managing the convertion and combining of all files
	/// </summary>
	public void ConvertFiles()
	{
		int maxThreads = GlobalVariables.maxThreads;
		Dictionary<string, List<FileInfo>> mergingFiles = new Dictionary<string, List<FileInfo>>();
		ConcurrentDictionary<Guid, FileToConvert> WorkingSet = new ConcurrentDictionary<Guid, FileToConvert>();

		//Initialize working set
		SetupWorkingSet(WorkingSet, mergingFiles);  //Initialize working set

		Task combineTask = Task.Run(() => SendToCombineFiles(mergingFiles));           //Combine files 

		//Set max threads for the thread pool based on global variables
		ThreadPool.SetMaxThreads(maxThreads, maxThreads);

		//Repeat until all files have been converted/checked or there was no change during last run
		while (WorkingSet.Count > 0)
		{
			ConcurrentDictionary<string, CountdownEvent> countdownEvents = new ConcurrentDictionary<string, CountdownEvent>();
			//Reset the working set map for the next run
			WorkingSetMap.Clear();
			var totalQueued = 0;
			//Loop through working set
			WorkingSet.Values.ForEach(file =>
			{
				//Loop through converters
				foreach (Converter converter in Converters)
				{
					//Check if the converter supports the format of the file
					if (file.Route.Count < 1 || !converter.SupportsConversion(file.CurrentPronom, file.Route.First()))
					{
						continue;
					}
					totalQueued++;
					//Send file to converter and check if the conversion was successful
					SendToConverter(file, converter, countdownEvents);
					return;

				}
			});

			//Wait for all threads to finish
			AwaitConversion(countdownEvents, totalQueued);
			try
			{
				//Remove files that are finished on and update the rest
				UpdateWorkingSet(WorkingSet);
			}
			catch (Exception e)
			{
				Logger.Instance.SetUpRunTimeLogMessage("CM ConvertFiles: " + e.Message, true);
			}
		}
		//Wait for the combine task to finish
		if (!combineTask.IsCompleted)
		{
			Console.WriteLine("Waiting for combine task to finish...");
			combineTask.Wait();
		}

		Console.WriteLine("Checking conversion status...");

		//TODO: Maybe look into checking file when they are removed from WorkingSet, not just all files at the end
		//Update FileInfo list with new data after conversion
		CheckConversion();
	}

	/// <summary>
	/// Initialises the working set with files to be converted based on settings
	/// </summary>
	/// <param name="ws">Working set to add files to</param>
	/// <param name="mf">Files that should be combined</param>
	void SetupWorkingSet(ConcurrentDictionary<Guid, FileToConvert> ws, Dictionary<string, List<FileInfo>> mf)
	{
		//TODO: Should we parallelize this?
		foreach (FileInfo file in FileManager.Instance.Files.Values)
		{
			//Create a new FileToConvert object
			var newFile = new FileToConvert(file);

			//Check if the file should be overridden by a folder override
			bool addToWorkingSet = CheckInOverride(file, newFile, mf);

			//Use current and target pronom to create a key for the conversion map
			var key = new KeyValuePair<string, string>(newFile.CurrentPronom, newFile.TargetPronom);

			//If the conversion map contains the key, set the route to the value of the key
			if (ConversionMap.ContainsKey(key))
			{
				newFile.Route = new List<string>(ConversionMap[key]);
			}
			//If the conversion map does not contain the key, set the route to the target pronom
			else if (newFile.CurrentPronom != newFile.TargetPronom)
			{
				newFile.Route.Add(newFile.TargetPronom);
			}
			else
			{
				continue;
			}
			//TODO: Does this serve a purpose?
			file.Route = newFile.Route;
			//Add the file to the working set if it was not set to be merged
			if (addToWorkingSet)
			{
				//Try to add the file to the working set
				bool added = ws.TryAdd(file.Id, newFile);
				if (!added)
				{
					Logger.Instance.SetUpRunTimeLogMessage("CM ConvertFiles: Could not add file to working set: " + file.FilePath, true);
				}
			} else
			{
				Console.Write("");		//DEBUG ONLY
			}
		}
	}


	/// <summary>
	/// Updates the data in the working set and removes files that are done or failed conversion after 3 attempts
	/// </summary>
	/// <param name="ws">Workingset to be updated</param>
	void UpdateWorkingSet(ConcurrentDictionary<Guid, FileToConvert> ws)
	{
		ConcurrentBag<Guid> itemsToRemove = new ConcurrentBag<Guid>();

		Parallel.ForEach(ws.Values, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, item =>
		{
			//If the file was not modified or failed the conversion, remove it from the WorkingSet
			if (!item.IsModified || item.Failed)
			{
				itemsToRemove.Add(item.Id);
				return;
			}
			//Reset the IsModified flag
			item.IsModified = false;

			//Update the current pronom to the pronom it was converted to
			//This assumes that the Converter has correctly identified if the file was converted correctly or not
			item.CurrentPronom = item.Route.First();
			//Remove the first step in the route
			item.Route.RemoveAt(0);

			// Remove if there are no more steps in route
			if (item.Route.Count == 0)
			{
				itemsToRemove.Add(item.Id);
			}
		});

		//Try to remove all items that were marked in the loop above
		foreach (var item in itemsToRemove)
		{
			ws.TryRemove(item, out _); // Try to remove the item from ConcurrentBag
		}
	}

	/// <summary>
	/// Sends a file to a converter and adds a CountdownEvent to a countdownEvents dictionary to be waited for later
	/// </summary>
	/// <param name="f">File that should convert</param>
	/// <param name="c">Converter that will do the conversion</param>
	/// <param name="threads">Where the ThreadInfo will be added</param>
	/// <returns>True if the conversion succeeded, False if not</returns>
	void SendToConverter(FileToConvert f, Converter c, ConcurrentDictionary<string, CountdownEvent> countdownEvents)
	{
		bool success = false;
		//Save return value of QueueUserWorkItem
		bool queued = ThreadPool.QueueUserWorkItem(state =>
		{
			var countdownEvent = new CountdownEvent(1);
			//Try to add a new CountdownEvent to the dictionary with the file path as key
			bool added = countdownEvents.TryAdd(f.FilePath, countdownEvent);
			if (!added)
			{
				Logger.Instance.SetUpRunTimeLogMessage("CM SendToConverter: Could not add countdown event: " + f.FilePath, true);
			}
			try
			{
				//Send file to converter
				c.ConvertFile(f);
                f.IsModified = true;
                //Add the name of the converter to the file if the previous entry is not the same converter for documentation
                if (c.NameAndVersion != null &&
					(FileInfoMap[f.Id].ConversionTools.Count == 0 || FileInfoMap[f.Id].ConversionTools.Last() != c.NameAndVersion))
				{
					FileInfoMap[f.Id].ConversionTools.Add(c.NameAndVersion);
				}
			}
			catch (Exception e)
			{
				//Set success to false and log the error message if an exception was thrown
				Logger.Instance.SetUpRunTimeLogMessage("CM SendToConverter: Error when converting file: " + e.Message, true);
			}
			finally
			{
				//Signal the CountdownEvent to indicate that the conversion is done (either succeeded or failed)
				countdownEvent.Signal();
			}
		});
	}

	public bool ExecuteWithTimeout(Action action, TimeSpan timeout)
	{
		// Create a task to execute the action
		var task = Task.Run(action);

		// Wait for the task with a timeout
		if (!task.Wait(timeout))
		{
			// Handle timeout (task did not complete within the specified time)
			Logger.Instance.SetUpRunTimeLogMessage("Operation timed out", true);

			return false;
		}
		return true;
	}

	/// <summary>
	/// Waits for all CountdownEvents in a dictionary to be signaled
	/// </summary>
	/// <param name="threads">The dictionary of ThreadInfos that should be waited for</param>
	/// <param name="total">The total number of thread jobs queued</param>
	void AwaitConversion(ConcurrentDictionary<string, CountdownEvent> countdownEvents, int total)
	{
		using (ProgressBar pb = new ProgressBar(total))
		{
			int numFinished = 0;
			while (numFinished < total)
			{
				numFinished = countdownEvents.Values.Count(c => c.IsSet);

				pb.Report((float)(numFinished) / (float)total, numFinished);
				Thread.Sleep(200);
			}
		}
	}


	/// <summary>
	/// Sends files to be combined
	/// </summary>
	/// <param name="mergingFiles">Dictionary with a List of all files that should be combined</param>
	Task SendToCombineFiles(Dictionary<string, List<FileInfo>> mergingFiles)
	{
		try
		{
			Parallel.ForEach(mergingFiles, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, entry =>
			{
				var converter = new iText7();
				var outputPronom = GlobalVariables.FolderOverride[entry.Key].DefaultType;
				converter.CombineFiles(entry.Value, outputPronom);
				foreach (FileInfo file in entry.Value)
				{
					file.ConversionTools.Add(converter.NameAndVersion ?? "Not found");
				}
			});
		}
		catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("CM SendToCombineFiles: " + e.Message, true);
		}
		return Task.CompletedTask;
	}

	/// <summary>
	/// Checks if a file should be overridden by a folder override
	/// </summary>
	/// <param name="parentDirName"></param>
	/// <param name="file"></param>
	/// <param name="newFile"></param>
	/// <param name="mergingFiles"></param>
	/// <returns>True if the file should be converted, False if it should be merged</returns>
	bool CheckInOverride(FileInfo file, FileToConvert newFile, Dictionary<string, List<FileInfo>> mergingFiles)
	{
		//Check if the file is in a folder that should be overridden
		string ?parentDirName  = Path.GetDirectoryName(Path.GetRelativePath(GlobalVariables.parsedOptions.Output, file.FilePath));
        if (parentDirName == null || !GlobalVariables.FolderOverride.ContainsKey(parentDirName))
		{
			return true;
		}

		foreach (string pronom in GlobalVariables.FolderOverride[parentDirName].PronomsList)
		{
			if (file.OriginalPronom != pronom)
			{
				continue;
			}
			if (!GlobalVariables.FolderOverride[parentDirName].Merge)
			{
				newFile.TargetPronom = GlobalVariables.FolderOverride[parentDirName].DefaultType;
			}
			else
			{
				// Check if the key exists in the dictionary
				if (!mergingFiles.ContainsKey(parentDirName))
				{
					// If the key does not exist, add it along with a new list
					mergingFiles[parentDirName] = new List<FileInfo>();
				}
				file.ShouldMerge = true;
				// Add the file to the list associated with the key
				mergingFiles[parentDirName].Add(file);
				return false;
			}
		}
		return true;
	}
}