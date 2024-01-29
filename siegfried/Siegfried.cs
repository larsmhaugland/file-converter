using SharpCompress.Archives;
using SharpCompress.Common;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

public class SiegfriedJSON
{
    [JsonPropertyName("siegfried")]
    public string siegfriedVersion;
    [JsonPropertyName("scandate")]
    public string scandate;
    [JsonPropertyName("files")]
    public SiegfriedFile[] files;

}

public struct SiegfriedFile
{
    [JsonPropertyName("filename")]
    public string filename;
    [JsonPropertyName("filesize")]
    public long filesize;
    [JsonPropertyName("modified")]
    public string modified;
    [JsonPropertyName("errors")]
    public string errors;
    [JsonPropertyName("matches")]
    public SiegfriedMatches[] matches;
}
public struct SiegfriedMatches
{
    [JsonPropertyName("ns")]
    public string ns;
    [JsonPropertyName("id")]
    public string id;
    [JsonPropertyName("format")]
    public string format;
    [JsonPropertyName("version")]
    public string version;
    [JsonPropertyName("mime")]
    public string mime;
    [JsonPropertyName("class")]
    public string class_;
    [JsonPropertyName("basis")]
    public string basis;
    [JsonPropertyName("warning")]
    public string warning;
}

public class Siegfried
{
    private static Siegfried? instance;
    public string Version;
    public string ScanDate;
    private static readonly object lockObject = new object();
    private List<string> CompressedFolders;
    private List<string> SupportedCompressionExtensions = new List<string>{ ".zip", ".tar", ".gz", ".rar", ".7z" };

    public static Siegfried Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new Siegfried();
                    }
                }
            }
            return instance;
        }
    }

    private Siegfried()
    {
        Version = "Not Found";
        ScanDate = "Not Found";
        CompressedFolders = new List<string>();
        try 
        { 
            CopyFiles(GlobalVariables.parsedOptions.Input, GlobalVariables.parsedOptions.Output);
            UnpackCompressedFolders();
        } catch (System.Exception e) 
        {
            Logger.Instance.SetUpRunTimeLogMessage("FileManager CopyFiles " + e.Message, true); 
        }
    }

    /// <summary>
    /// Returns the pronom id of a specified file
    /// </summary>
    /// <param name="path">Path to file</param>
    /// <returns>Pronom id or null</returns>
    public string IdentifyPronom(string path)
    {
        // Wrap the file path in quotes
        string wrappedPath = "\"" + path + "\"";
        string options = $"-home siegfried -multi 64 -json -sig pronom64k.sig ";

        // Define the process start info
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = @"siegfried/sf.exe", // or any other command you want to run
            Arguments = options + wrappedPath,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        string error = "";
        string output = "";
        // Create the process
        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();

            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();

            process.WaitForExit();
        }
        //TODO: Check error and possibly continue
        
        if (error.Length > 0)
        {
            Logger.Instance.SetUpRunTimeLogMessage("FileManager SF " + error, true); 
        }
        var parsedData = ParseJSONOutput(output, false);
        if (parsedData == null)
            return null; //TODO: Check error and possibly continue
        //Return pronom id
        if (parsedData.files.Length > 0)
        {
            return parsedData.files[0].matches[0].id;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Identifies all files in input directory and returns a list of FileInfo objects. 
    /// Siegfried output is put in a JSON file.
    /// </summary>
    /// <param name="inputFolder">Path to root folder for files to be identified</param>
    /// <returns>List of all identified files or null</returns>
    public List<FileInfo> IdentifyFilesJSON(string inputFolder)
    {
        var files = new List<FileInfo>();
        // Wrap the file path in quotes
        string wrappedPath = "\"" + inputFolder + "\"";
        string options = $"-home siegfried -multi 64 -json -sig pronom64k.sig ";
        string outputFile = "siegfried/siegfried.json";

        //Create output file
        File.Create(outputFile).Close();
        // Define the process start info
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = @"siegfried/sf.exe", // or any other command you want to run
            Arguments = options + wrappedPath,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        string error = "";
        // Create the process
        using (Process process = new Process { StartInfo = psi })
        {
            // Create the StreamWriter to write to the file
            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                // Set the output stream for the process
                process.OutputDataReceived += (sender, e) => { if (e.Data != null) sw.WriteLine(e.Data); };

                // Start the process
                process.Start();

                // Begin asynchronous read operations for output and error streams
                process.BeginOutputReadLine();
                error = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();
            }
        }
        //TODO: Check error and possibly continue
        /*
        if (error.Length > 0)
        {
            Logger.Instance.SetUpRunTimeLogMessage("FileManager SF " + error, true);
            return; 
        }*/
        var parsedData = ParseJSONOutput(outputFile, true);
        if (parsedData == null)
            return null; //TODO: Check error and possibly continue

        Version = parsedData.siegfriedVersion;
        ScanDate = parsedData.scandate;
        for (int i = 0; i < parsedData.files.Length; i++)
        {
            var file = new FileInfo(parsedData.files[i]);
            if (file != null) 
            {
                files.Add(file);
                SupportedCompressionExtensions.ForEach(ext =>
                {
                    if (Path.GetExtension(file.FileName) == ext)
                    {
                        CompressedFolders.Add(file.FilePath);
                    }
                });
            }
        }
        return files;
    }

    SiegfriedJSON ParseJSONOutput(string json, bool file)
    {

        try
        {
            if (file)
            {
                using (JsonDocument document = JsonDocument.Parse(File.OpenRead(json)))
                {
                    // Access the root of the JSON document
                    JsonElement root = document.RootElement;

                    // Deserialize JSON into a SiegfriedJSON object
                    SiegfriedJSON siegfriedJson = new SiegfriedJSON
                    {
                        siegfriedVersion = root.GetProperty("siegfried").GetString() ?? "",
                        scandate = root.GetProperty("scandate").GetString() ?? "",
                        files = root.GetProperty("files").EnumerateArray()
                            .Select(fileElement => ParseSiegfriedFile(fileElement))
                            .ToArray()
                    };
                    return siegfriedJson;
                }
            }
            else
            {
                return JsonSerializer.Deserialize<SiegfriedJSON>(json);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Logger.Instance.SetUpRunTimeLogMessage("FileManager JSON " + e.Message, true);
            return null;
        }
    }

    static SiegfriedFile ParseSiegfriedFile(JsonElement fileElement)
    {
        return new SiegfriedFile
        {
            filename = fileElement.GetProperty("filename").GetString() ?? "",
            //Explicitly check if filesize exists and is a number, otherwise set to 0
            filesize = fileElement.GetProperty("filesize").GetInt64(),
            modified = fileElement.GetProperty("modified").GetString() ?? "",
            errors = fileElement.GetProperty("errors").GetString() ?? "",
            matches = fileElement.GetProperty("matches").EnumerateArray()
                .Select(matchElement => ParseSiegfriedMatches(matchElement))
                .ToArray()
        };
    }

    static SiegfriedMatches ParseSiegfriedMatches(JsonElement matchElement)
    {
        return new SiegfriedMatches
        {
            ns = matchElement.GetProperty("ns").GetString() ?? "",
            id = matchElement.GetProperty("id").GetString() ?? "",
            format = matchElement.GetProperty("format").GetString() ?? "",
            version = matchElement.GetProperty("version").GetString() ?? "",
            mime = matchElement.GetProperty("mime").GetString() ?? "",
            class_ = matchElement.GetProperty("class").GetString() ?? "",
            basis = matchElement.GetProperty("basis").GetString() ?? "",
            warning = matchElement.GetProperty("warning").GetString() ?? ""
        };
    }
    
    /// <summary>
    /// Copies all files (while retaining file structure) from a source directory to a destination directory
    /// </summary>
    /// <param name="source">source directory</param>
    /// <param name="destination">destination directory</param>
    private void CopyFiles(string source, string destination)
    {
        string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string relativePath = file.Replace(source, "");
            string outputPath = destination + relativePath;
            string outputFolder = outputPath.Substring(0, outputPath.LastIndexOf('\\'));
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            File.Copy(file, outputPath, true);
        }
    }

    /// <summary>
    /// Compresses all folders in output directory
    /// </summary>
    public void CompressFolders()
    {
        //In Parallel: Identify original compression formats and compress the previously identified folders
        Parallel.ForEach(CompressedFolders, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
        {
            var extention = Path.GetExtension(filePath);
            //Switch for different compression formats
            switch (extention)
            {
                case ".zip":
                    CompressFolder(filePath, ArchiveType.Zip);
                    break;
                case ".tar":
                    CompressFolder(filePath, ArchiveType.Tar);
                    break;
                case ".gz":
                    CompressFolder(filePath, ArchiveType.GZip);
                    break;
                case ".rar":
                    CompressFolder(filePath, ArchiveType.Rar);
                    break;
                case ".7z":
                    CompressFolder(filePath, ArchiveType.SevenZip);
                    break;
                default:
                    //Do nothing
                    break;
            }
        });
    }

    /// <summary>
    /// Unpacks all compressed folders in output directory
    /// </summary>
    private void UnpackCompressedFolders()
    {
        //Identify all files in input directory
        string[] filePaths = Directory.GetFiles(GlobalVariables.parsedOptions.Output, "*.*", SearchOption.AllDirectories);

        //In Parallel: Run SF and parse output into FileInfo constructor
        Parallel.ForEach(filePaths, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
        {
            var extention = Path.GetExtension(filePath);
            //Switch for different compression formats
            switch (extention)
            {
                case ".zip":
                    UnpackFolder(filePath);
                    break;
                case ".tar":
                    UnpackFolder(filePath);
                    break;
                case ".gz":
                    UnpackFolder(filePath);
                    break;
                case ".rar":
                    UnpackFolder(filePath);
                    break;
                case ".7z":
                    UnpackFolder(filePath);
                    break;
                default:
                    //Do nothing
                    break;
            }
        });
    }

    /// <summary>
    /// Compresses a folder to a specified format and deletes the unpacked folder
    /// </summary>
    /// <param name="archiveType">Format for compression</param>
    /// <param name="path">Path to folder to be compressed</param>
    private void CompressFolder(string path, ArchiveType archiveType)
    {
        try
        {
            string fileExtension = Path.GetExtension(path);
            string pathWithoutExtension = path.Replace(fileExtension, ""); //TODO: This might not work for paths with multiple file extensions
            using (var archive = ArchiveFactory.Create(archiveType))
            {
                archive.AddAllFromDirectory(pathWithoutExtension);
                archive.SaveTo(path, CompressionType.None);
            }
            // Delete the unpacked folder
            Directory.Delete(pathWithoutExtension, true);
        } catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("FileManager CompressFolder " + e.Message, true);
        }
    }

    /// <summary>
    /// Unpacks a compressed folder regardless of format
    /// </summary>
    /// <param name="path">Path to compressed folder</param>
    private void UnpackFolder(string path)
    {
        try
        {
            // Get path to folder without extention
            string pathWithoutExtension = path.Split('.')[0];
            // Ensure the extraction directory exists
            if (!Directory.Exists(pathWithoutExtension))
            {
                Directory.CreateDirectory(pathWithoutExtension);
            }

            // Extract the contents of the compressed file
            using (var archive = ArchiveFactory.Open(path))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(pathWithoutExtension, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                    }
                }
            }
        } catch (Exception e)
        {
            Logger.Instance.SetUpRunTimeLogMessage("FileManager UnpackFolder " + e.Message, true);
        }
    }
}