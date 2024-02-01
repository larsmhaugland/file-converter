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
        Thread thread = new Thread(() => IdentifyFiles());
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
        //TODO: Why do I get a warning here (without '!')?
        List<FileInfo> ?files = await sf.IdentifyFilesJSON(GlobalVariables.parsedOptions.Output)!; //Search for files in output folder since they are copied there from input folder
        
        if(files != null)
        {
            Files = files;
        } else
        {
            Logger logger = Logger.Instance;
            logger.SetUpRunTimeLogMessage("Error when discovering files / No files found", true);
        }
    }

    public List<FileInfo> GetFiles()
    {
        if(Files.Count == 0)
        {
            //Should maybe wait?
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
