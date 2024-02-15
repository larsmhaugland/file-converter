using System.Collections.Concurrent;
using Org.BouncyCastle.Asn1;
using System.IO;
using System.Threading;
using iText.Layout.Splitting;
using System.Threading.Tasks.Sources;
using Org.BouncyCastle.Asn1.Crmf;
using System.Diagnostics;

class FileToConvert
{
	public string FilePath { get; set; }            //From FileInfo
	public string CurrentPronom { get; set; }       //From FileInfo
	public string TargetPronom { get; set; }        //From Dictionary
	public List<string> Route { get; set; }         //From Dictionary
	public bool IsModified { get; set; } = false;   //True if file has been worked on

    public FileToConvert(FileInfo file)
	{
		FilePath = file.FilePath;
		CurrentPronom = file.OriginalPronom;

		if (GlobalVariables.FileSettings.ContainsKey((CurrentPronom)))
		{
			TargetPronom = GlobalVariables.FileSettings[CurrentPronom];
		}
		else
		{
			TargetPronom = CurrentPronom;
		}

		Route = new List<string>();
	}
}

public class ConversionManager
{
	List<FileInfo> Files;
	Dictionary<KeyValuePair<string, string>, List<string>> ConversionMap = new Dictionary<KeyValuePair<string, string>, List<string>>();
	Dictionary<string,FileInfo> FileInfoMap = new Dictionary<string,FileInfo>();
	public Dictionary<string,string> WorkingSetMap = new Dictionary<string,string>();
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
		foreach (string pronom in WordPronoms)
		{
			foreach (string otherpronom in WordPronoms)
			{
				if (pronom != otherpronom)
				{
					ConversionMap.Add(new KeyValuePair<string, string>(pronom, otherpronom), [pronom, "fmt/276", otherpronom]); // word to pdf 1.7 other word
				}
			}
		}
		foreach (string pronom in ImagePronoms)
		{
			foreach(string otherpronom in ImagePronoms)
			{
				if (pronom != otherpronom)
				{
					ConversionMap.Add(new KeyValuePair<string, string>(pronom, otherpronom), [pronom, "fmt/18", otherpronom]); // Image to pdf 1.4 other Image
				}
			}
		}
		// TODO: Add more routes
	}
	private void initFileMap()
	{
		foreach(FileInfo file in Files)
		{
			FileInfoMap.TryAdd(file.FilePath, file);
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

		/*Converters = new List<Converter>();
		Converters.Add(new iText7());
		Converters.Add(new GhostscriptConverter());
		Converters.Add(new CogniddoxConverter());*/

		Converters = AddConverters.Instance.GetConverters();
		//Get files from FileManager
		Files = FileManager.Instance.GetFiles();
		//Initialize FileMap
		initFileMap();
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
	
	/// <summary>
	/// 
	/// </summary>
	void checkConversion()
	{
		var sf = Siegfried.Instance;
		foreach (FileInfo file in Files)
		{
			file.CheckIfConverted();
		}
	}

    /// <summary>
    /// Responsible for converting all files
    /// </summary>
    public async Task ConvertFiles()
    {
		int maxThreads = GlobalVariables.maxThreads;
        Dictionary<string, List<FileInfo>> mergingFiles = new Dictionary<string, List<FileInfo>>();
		ConcurrentDictionary<string,FileToConvert> WorkingSet = new ConcurrentDictionary<string,FileToConvert>();
        //Initialize working set
        Siegfried sf = Siegfried.Instance;
        Logger logger = Logger.Instance;
        foreach (FileInfo file in Files)
        {
            var newFile = new FileToConvert(file);			
            string? parentDirName = Path.GetDirectoryName(Path.GetRelativePath(GlobalVariables.parsedOptions.Output,file.FilePath));
			bool addToWorkingSet = CheckInOverride(parentDirName,file,newFile,mergingFiles);
            
            //Use current and target pronom to create a key for the conversion map
            var key = new KeyValuePair<string, string>(newFile.CurrentPronom, newFile.TargetPronom);

			//If the conversion map contains the key, set the route to the value of the key
			if (ConversionMap.ContainsKey(key))
			{
				newFile.Route = ConversionMap[key];
			}
			//If the conversion map does not contain the key, set the route to the target pronom
			else if (newFile.CurrentPronom != newFile.TargetPronom)
			{
				newFile.Route.Add(newFile.TargetPronom);
			} else
			{
				continue;
			}
			file.Route = newFile.Route;
			if (addToWorkingSet)
			{
				bool added = WorkingSet.TryAdd(file.FilePath, newFile);
				if (!added)
				{
					logger.SetUpRunTimeLogMessage("CM ConvertFiles Could not add file to working set: " + file.FilePath, true);
				}
            }
		}
		SendToCombineFiles(mergingFiles);
		List<Task> tasks = new List<Task>();
		ThreadPool.SetMaxThreads(maxThreads, maxThreads);
		int totalFiles = WorkingSet.Count;

		//Repeat until all files have been converted/checked
		while (WorkingSet.Count > 0)
		{
			ConcurrentDictionary<string, CountdownEvent> countdownEvents = new ConcurrentDictionary<string, CountdownEvent>();
			WorkingSetMap.Clear();
			//Loop through working set
			Parallel.ForEach(WorkingSet.Values, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
			{
				
				//If file is already worked on, skip it
				if (file.IsModified || file.Route.Count == 0)
				{
					return;
				}
				//Loop through converters
				foreach (Converter converter in Converters)
				{
					if (file.IsModified)
					{
						return;
					}
					var dict = converter.SupportedConversions;

					//If the converter supports the current pronom, check if it can convert to the next pronom in the route
					if (dict == null || !dict.ContainsKey(file.CurrentPronom))
					{
						break;
					}
					foreach (string outputFormat in dict[file.CurrentPronom])
					{
						//Check if the converter can convert to the next pronom in the route
						if (file.Route.First() != outputFormat)
						{
							continue;
						}
                        //Create a countdown event for the current file
                        file.IsModified = true;
						//Try to queue converting file using virtual function
						if (ThreadPool.QueueUserWorkItem(state =>
							{
								try
								{
									converter.ConvertFile(file.FilePath, outputFormat);
									if (converter.Name != null &&
										(FileInfoMap[file.FilePath].ConversionTools.Count == 0 || FileInfoMap[file.FilePath].ConversionTools.Last() != converter.Name))
									{
										FileInfoMap[file.FilePath].ConversionTools.Add(converter.Name);
									}
								}
								catch (Exception e)
								{
									logger.SetUpRunTimeLogMessage("Error when converting file: " + e.Message, true);
									file.IsModified = false;
								}
								finally
								{
									if (countdownEvents.ContainsKey(file.FilePath))
									{
										countdownEvents[file.FilePath].Signal();
									}
								}
							}))
						{
							//To be run if the Thread was successfully queued
							var added = countdownEvents.TryAdd(file.FilePath, new CountdownEvent(1));
							if(!added)
							{
                                logger.SetUpRunTimeLogMessage("CM ConvertFiles Could not add countdown event: " + file.FilePath, true);
                            }
							return;
						}
					}
				}
			});

			//Wait for all threads to finish
			var total = countdownEvents.Count;
			var current = 0;
			using (ProgressBar progressBar = new ProgressBar("Awaiting threads", countdownEvents.Count))
			{
				await Task.Run(() =>
				{
					bool allDone = false;
					Stopwatch sw = new Stopwatch();
					int timeout = 1;
					sw.Start();
					while (!allDone && sw.Elapsed.Seconds < timeout) {
						Thread.Sleep(200);
						allDone = true;
						foreach (var countdownEvent in countdownEvents)
						{
							if (countdownEvent.Value.IsSet)
							{
								progressBar.Report((float)++current / (float)total, current);
								countdownEvent.Value.Dispose(); // Dispose after completion
							}
							else
							{
								allDone = false;
							}
						}
					}
					sw.Stop();
					if (!allDone)
					{
						Console.WriteLine("[DEBUG] Threads should time out!");
						countdownEvents.Values.AsParallel().ForAll(c => c.Wait());
					}
					countdownEvents.Clear();
				});
                progressBar.Report(1, current);
				Thread.Sleep(200);
            }
			//Remove files that have been worked on from the working set and update for the rest
			var itemsToRemove = new List<FileToConvert>();
			foreach (var entry in WorkingSetMap)
			{
				var oldFile = WorkingSet[entry.Key];
				WorkingSet.Remove(oldFile.FilePath, out _);
				oldFile.FilePath = entry.Value;
				//WorkingSet[entry.Value] = oldFile;
				var worked = WorkingSet.TryAdd(entry.Value,oldFile);
				if (!worked)
				{
					Console.WriteLine("[DEBUG] Could not add new entry to WorkingSet");
				}
			}
			foreach (var item in WorkingSet.Values)
			{
				if (!item.IsModified)
				{
					itemsToRemove.Add(item);
					continue;
				}

				item.IsModified = false;

				// Check if file was converted correctly
				var file = sf.IdentifyFile(item.FilePath, false);

				if (file == null)
				{
					logger.SetUpRunTimeLogMessage("CM ConvertFiles Could not identify file: " + item.FilePath, true);
					continue;
				}

				item.CurrentPronom = file.matches[0].id;

				if (item.CurrentPronom == item.Route.First())
				{
					item.Route.RemoveAt(0);
				}

				// Remove if no more steps in route
				if (item.Route.Count == 0)
				{
					itemsToRemove.Add(item);
					continue;
				}
				Console.Write("");
			}
			foreach (var itemToRemove in itemsToRemove)
			{
				WorkingSet.TryRemove(itemToRemove.FilePath, out _); // TryTake removes the item from ConcurrentBag
			}
		}
		
		//Update FileInfo list with new data
		Console.WriteLine("Checking conversion status...");
		checkConversion();
	}
    void SendToCombineFiles(Dictionary<string, List<FileInfo>> mergingFiles)
	{
		try
		{
            foreach (var entry in mergingFiles)
            {
                var converter = new iText7();
                var outputPronom = GlobalVariables.FolderOverride[entry.Key].DefaultType;
                List<string> filepaths = new List<string>();
                foreach (var file in entry.Value)
                {
                    filepaths.Add(file.FilePath);
                }
                converter.CombineFiles(filepaths.ToArray(), outputPronom);
            }
        }
		catch(Exception e)
		{
            Logger.Instance.SetUpRunTimeLogMessage(e.Message,true);
        }
    }
	
	bool CheckInOverride(string? parentDirName,FileInfo file, FileToConvert newFile, Dictionary<string, List<FileInfo>> mergingFiles)
	{
        //check if there is a folderoverride on the folder this file is in  
        if (parentDirName != null && GlobalVariables.FolderOverride.ContainsKey(parentDirName))
        {
            foreach (string pronom in GlobalVariables.FolderOverride[parentDirName].PronomsList)
            {
                if (file.OriginalPronom == pronom)
                {
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

                        // Add the file to the list associated with the key
                        mergingFiles[parentDirName].Add(file);
                        return false;
                    }
                }
            }
        }
        return true;
	}
}