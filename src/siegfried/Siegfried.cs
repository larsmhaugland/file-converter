﻿using Ghostscript.NET;
using iText.IO.Source;
using iText.Kernel.Pdf.Function;
using iText.Kernel.Utils.Objectpathitems;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509.SigI;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public class SiegfriedJSON
{
	[JsonPropertyName("siegfried")]
	public string siegfriedVersion = "";
	[JsonPropertyName("scandate")]
	public string scandate = "";
	[JsonPropertyName("files")]
	public SiegfriedFile[] files = [];

}

public class SiegfriedFile
{
	[JsonPropertyName("filename")]
	public string filename = "";
	[JsonPropertyName("filesize")]
	public long filesize = 0;
	[JsonPropertyName("modified")]
	public string modified = "";
	[JsonPropertyName("errors")]
	public string errors = "";
	public string hash = "";
	[JsonPropertyName("matches")]
	public SiegfriedMatches[] matches = [];
}
public class SiegfriedMatches
{
	[JsonPropertyName("ns")]
	public string ns = "";
	[JsonPropertyName("id")]
	public string id = "";
	[JsonPropertyName("format")]
	public string format = "";
	[JsonPropertyName("version")]
	public string version = "";
	[JsonPropertyName("mime")]
	public string mime = "";
	[JsonPropertyName("class")]
	public string class_ = "";
	[JsonPropertyName("basis")]
	public string basis = "";
	[JsonPropertyName("warning")]
	public string warning = "";
}

public class Siegfried
{
	private static Siegfried? instance;
	public string ?Version = null;
	public string ?ScanDate = null;
	public string OutputFolder = "siegfried/JSONoutput";
	private string ExecutablePath = "src/siegfried/sf.exe";
	private string HomeFolder = "src/siegfried";
    private static readonly object lockObject = new object();
	private List<string> CompressedFolders;
	public ConcurrentBag<FileInfo> Files = new ConcurrentBag<FileInfo>();
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
		Logger logger = Logger.Instance;
		//TODO: Should check Version and ScanDate here
		CompressedFolders = new List<string>();
        //Look for Siegfried files
        var found = Path.Exists(ExecutablePath);
		logger.SetUpRunTimeLogMessage("SF Siegfried executable " + (found ? "" : "not") + "found", !found);
		if (!found)
		{
			Console.WriteLine("Cannot find Siegfried executable");
			throw new FileNotFoundException("Cannot find Siegfried executable");
		}
		found = Path.Exists(HomeFolder + "/pronom64k.sig");
		logger.SetUpRunTimeLogMessage("SF Pronom signature file " + (found ? "" : "not") + "found", !found);
		if (!found)
		{
            Console.WriteLine("Cannot find Pronom signature file");
            throw new FileNotFoundException("Cannot find Pronom signature file");
        }
	}

	public void AskReadFiles()
	{
        //Check if json files exist
        if (Directory.Exists(OutputFolder))
        {
            string? input;
            do
            {
                Console.Write("Siegfried data found, do you want to parse it? (Y/N): ");
                input = Console.ReadLine().ToLower(); //TODO: Why does this give a warning?
            } while (input != "y" && input != "n");
            if (input == "y")
            {
                ReadFromFiles();
            }
        }
    }

	public void ClearOutputFolder()
	{
        if (Directory.Exists(OutputFolder))
		{
			try
			{
				Directory.Delete(OutputFolder, true);
			} catch 
			{
				Logger logger = Logger.Instance;
				logger.SetUpRunTimeLogMessage("SF Could not delete Siegfried output folder", true);
			}
        }
    }

	static string HashEnumToString(HashAlgorithms hash)
	{
		switch (hash)
		{
			case HashAlgorithms.MD5:
				return "md5";
			default:
				return "sha256";
		}
	}

	private void ReadFromFiles()
	{
		//TODO: Compressed files are not handled correctly here
		var paths = Directory.GetFiles(OutputFolder, "*.*", SearchOption.AllDirectories);
		using (ProgressBar progressBar = new ProgressBar(paths.Length))
		{
			//Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, (path, state, index) =>
			for(int i = 0; i < paths.Length; i++)
			{
				progressBar.Report((i+1) / (double)paths.Length,i+1);
				var parsedData = ParseJSONOutput(paths[i], true);
				if (parsedData == null)
					return; //TODO: Check error and possibly continue

				if (instance.Version == null || instance.ScanDate == null)
				{
					instance.Version = parsedData.siegfriedVersion;
					instance.ScanDate = parsedData.scandate;
				}

				foreach (var f in parsedData.files)
				{
					instance.Files.Add(new FileInfo(f));
				}
			}//);
		}
	}

	/// <summary>
	/// Returns the pronom id of a specified file
	/// </summary>
	/// <param name="path">Path to file</param>
	/// <param name="hash">True if file should be hashed</param>
	/// <returns>Parsed SiegfriedFile or null</returns>
	public SiegfriedFile? IdentifyFile(string path, bool hash)
	{
		// Wrap the file path in quotes
		string wrappedPath = "\"" + path + "\"";
		string options;
		if (hash)
		{
			options = $"-home {HomeFolder} -json -hash " + HashEnumToString(GlobalVariables.checksumHash) + " -sig pronom64k.sig ";
		} else
		{
			options = $"-home {HomeFolder} -json -sig pronom64k.sig ";
		}

		// Define the process start info
		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = $"{ExecutablePath}", // or any other command you want to run
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
			Logger.Instance.SetUpRunTimeLogMessage("SF IdentifyFile: " + error, true);
		}
		var parsedData = ParseJSONOutput(output, false);
		if (parsedData == null || parsedData.files == null)
			return null; 
			 
		if (parsedData.files.Length > 0)
		{
			return parsedData.files[0];
		}
		else
		{
			return null;
		}
	}

	/// <summary>
	/// Returns a SiegfriedFile list of a specified file array
	/// </summary>
	/// <param name="paths">Array of file paths to </param>
	/// <returns>Pronom id or null</returns>
	public List<FileInfo>? IdentifyList(string[] paths)
	{
		Logger logger = Logger.Instance;
		var files = new List<FileInfo>();

		if(paths.Length < 1)
		{
			return null;
		}
		string[] tempPaths = new string[paths.Length];
		// Wrap the file paths in quotes
		for (int i = 0; i < paths.Length; i++)
		{
			tempPaths[i] = Path.GetFullPath(paths[i]);
			tempPaths[i] = "\"" + paths[i] + "\"";
		}
		string wrappedPaths = String.Join(" ",tempPaths);
		string options = $"-home {HomeFolder} -multi 64 -json -coe -hash " +  HashEnumToString(GlobalVariables.checksumHash) + " -sig pronom64k.sig ";

		string outputFile = Path.Combine(OutputFolder, Guid.NewGuid().ToString(), ".json");
		string? parentDir = Directory.GetParent(outputFile)?.FullName;

		//Create output file
		try
		{
			if (parentDir != null && !Directory.Exists(parentDir))
			{
				Directory.CreateDirectory(parentDir);
			}
			if (parentDir != null) { 
				File.Create(outputFile).Close();
			} else
			{
				Logger.Instance.SetUpRunTimeLogMessage("SF IdentifyList: parentDir is null " + outputFile, true);
			}
		}
		catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("SF IdentifyList: could not create output file " + e.Message, true);
		}

		// Define the process start info
		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = $"{ExecutablePath}", // or any other command you want to run
			Arguments = options + wrappedPaths,
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
		if (error.Length > 0)
		{
			//Remove \n from error message
			error = error.Replace("\n", " - ");
			Logger.Instance.SetUpRunTimeLogMessage("SF IdentifyList: " + error, true);
		}
		var parsedData = ParseJSONOutput(outputFile, true);
		if (parsedData == null)
			return null; //TODO: Check error and possibly continue
		if (Version == null || ScanDate == null)
		{
			Version = parsedData.siegfriedVersion;
			ScanDate = parsedData.scandate;
		}
		for (int i = 0; i < parsedData.files.Length; i++)
		{
			var file = new FileInfo(parsedData.files[i]);
			file.FilePath = paths[i];
			file.FileName = Path.GetFileName(file.FilePath);
            var pathWithoutInput = file.FilePath.Replace(GlobalVariables.parsedOptions.Input, "");
            file.ShortFilePath = Path.Combine(pathWithoutInput.Replace(GlobalVariables.parsedOptions.Output, ""));
            while (file.ShortFilePath[0] == '\\')
            {
                //Remove leading backslashes
                file.ShortFilePath = file.ShortFilePath.Substring(1);
            }
            files.Add(file);
		}
		return files;
	}

	/// <summary>
	/// Identifies all files in input directory and returns a List of FileInfo objects.
	/// </summary>
	/// <param name="input">Path to root folder for search</param>
	/// <returns>A List of identified files</returns>
	public Task<List<FileInfo>>? IdentifyFilesIndividually(string input)
	{
		Logger logger = Logger.Instance;
		var files = new ConcurrentBag<FileInfo>();
		List<string> filePaths = new List<string>(Directory.GetFiles(input, "*.*", SearchOption.AllDirectories));
		ConcurrentBag<string[]> filePathGroups = new ConcurrentBag<string[]>(GroupPaths(filePaths));

		Parallel.ForEach(filePathGroups, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, filePaths =>
		{
			var output = IdentifyList(filePaths);
			if (output == null)
			{
				logger.SetUpRunTimeLogMessage("SF IdentifyFilesIndividually: could not identify files", true);
				return; //Skip current group
			}
			//TODO: Check if all files were identified

			foreach (var f in output)
			{
				files.Add(f);
			}
		});
	   
		return Task.FromResult(files.ToList());
	}

	List<string[]> GroupPaths(List<string> paths)
	{
		int groupSize = 128; //Number of files to be identified in each group
		int groupCount = paths.Count / groupSize;
		var filePathGroups = new List<string[]>();
		if (paths.Count % groupSize != 0)
		{
			groupCount++;
		}
		for (int i = 0; i < groupCount; i++)
		{
			filePathGroups.Add(paths.GetRange(i * groupSize, Math.Min(groupSize, paths.Count - i * groupSize)).ToArray());
		}
		return filePathGroups;
	}

	public List<FileInfo> IdentifyCompressedFilesJSON(string input)
	{
		Logger logger = Logger.Instance;
		UnpackCompressedFolders();
		var fileBag = new ConcurrentBag<FileInfo>();
		
		//For eaccompressed folder, identify all files
		Parallel.ForEach(CompressedFolders, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, folder =>
		{
			//Identify all file paths in compressed folder and group them
			var pathWithoutExt = folder.Split('.')[0]; //TODO: This might not work for paths with multiple file extensions
			var paths = Directory.GetFiles(pathWithoutExt, "*.*", SearchOption.AllDirectories);
			var filePathGroups = GroupPaths(new List<string>(paths));
			//Identify all files in each group
			Parallel.ForEach(filePathGroups, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, paths =>
			{
				var files = IdentifyList(paths);
				if (files != null)
				{
					foreach (FileInfo file in files)
					{
						fileBag.Add(file);
					}
				}
				else
				{
					logger.SetUpRunTimeLogMessage("SF IdentifyCompressedFilesJSON: " + folder + " could not be identified", true);
				}
			});
		});
		return fileBag.ToList();
	}

	SiegfriedJSON? ParseJSONOutput(string json, bool readFromFile)
	{
		try
		{ 
			SiegfriedJSON siegfriedJson;
			FileStream ?file = null;
			if (readFromFile)
			{
				file = File.OpenRead(json);
			}

			if (readFromFile && file == null)
			{
                Logger.Instance.SetUpRunTimeLogMessage("SF ParseJSON: file not found", true);
                return null;
            }

			using (JsonDocument document = readFromFile ? JsonDocument.Parse(file!) : JsonDocument.Parse(json))
			{
				// Access the root of the JSON document
				JsonElement root = document.RootElement;

				// Deserialize JSON into a SiegfriedJSON object
				siegfriedJson = new SiegfriedJSON
				{
					siegfriedVersion = root.GetProperty("siegfried").GetString() ?? "",
					scandate = root.GetProperty("scandate").GetString() ?? "",
					files = root.GetProperty("files").EnumerateArray()
						.Select(fileElement => ParseSiegfriedFile(fileElement))
						.ToArray()
				};
			}
			if (readFromFile && file != null)
			{
				file.Close();
			}
            return siegfriedJson;
        }
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Logger.Instance.SetUpRunTimeLogMessage("SF ParseJSON: " + e.Message, true);
			return null;
		}
	}

	static SiegfriedFile ParseSiegfriedFile(JsonElement fileElement)
	{
		string hashMethod = HashEnumToString(GlobalVariables.checksumHash);
		JsonElement jsonElement;
		return new SiegfriedFile
		{
			
			filename = fileElement.GetProperty("filename").GetString() ?? "",
			
			hash = fileElement.TryGetProperty(hashMethod,out jsonElement) ? fileElement.GetProperty(hashMethod).GetString() ?? "" : "",
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
	public void CopyFiles(string source, string destination)
	{
		string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
		List<string> retryFiles = new List<string>();
		Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, file =>
			// (string file in files)
		{
			string relativePath = file.Replace(source, "");
			string outputPath = destination + relativePath;
			string outputFolder = outputPath.Substring(0, outputPath.LastIndexOf('\\'));
			//TODO: THIS BEHAVIOUR SHOULD BE DOCUMENTED
			//If file already exists in target destination, skip it
			if (File.Exists(outputPath))
			{
				return;
			}
			if (!Directory.Exists(outputFolder))
			{
				Directory.CreateDirectory(outputFolder);
			}
			try
			{
				File.Copy(file, outputPath, true);
			} catch (IOException ex)
			{
				Console.WriteLine("Could not open file '{0}', it may be used in another process");
				retryFiles.Add(file);
            }
		});
		if (retryFiles.Count > 0)
		{
			Console.WriteLine("Some files could not be copied, close the processes using them and hit enter");
			Console.ReadLine();
			Parallel.ForEach(retryFiles, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, file =>
			{
                string relativePath = file.Replace(source, "");
                string outputPath = destination + relativePath;
                string outputFolder = outputPath.Substring(0, outputPath.LastIndexOf('\\'));

                if (!Directory.Exists(outputFolder))
				{
                    Directory.CreateDirectory(outputFolder);
                }
				try
				{
					File.Copy(file, outputPath, true);
				} catch (Exception e)
				{
					Logger.Instance.SetUpRunTimeLogMessage("SF CopyFiles: " + e.Message, true);
				}
            });
		}
	}

	/// <summary>
	/// Compresses all folders in output directory
	/// </summary>
	public void CompressFolders()
	{
		//In Parallel: Identify original compression formats and compress the previously identified folders
		Parallel.ForEach(CompressedFolders, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, filePath =>
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
	public void UnpackCompressedFolders()
	{
		//Identify all files in output directory
		List<string> compressedFoldersOutput = new List<string>(Directory.GetFiles(GlobalVariables.parsedOptions.Output, "*.*", SearchOption.AllDirectories));
		List<string> compressedFoldersInput = new List<string>(Directory.GetFiles(GlobalVariables.parsedOptions.Input, "*.*", SearchOption.AllDirectories));

		List<string> outputFoldersWithoutRoot = new List<string>();
		List<string> inputFoldersWithoutRoot = new List<string>();

		//Remove root path from all paths
		//TODO: Should not use replace, just remove first occurence
		foreach (string compressedFolder in compressedFoldersOutput)
		{
			string relativePath = compressedFolder.Replace(GlobalVariables.parsedOptions.Output, "");
			outputFoldersWithoutRoot.Add(relativePath);
		}

		foreach (string compressedFolder in compressedFoldersInput)
		{
			string relativePath = compressedFolder.Replace(GlobalVariables.parsedOptions.Input, "");
			inputFoldersWithoutRoot.Add(relativePath);
		}

		//Remove all folders that are not in input directory
		foreach (string folder in outputFoldersWithoutRoot)
		{
			if (!inputFoldersWithoutRoot.Contains(folder))
			{
				compressedFoldersOutput.Remove(GlobalVariables.parsedOptions.Output + folder);
			}
		}
		ConcurrentBag<string> unpackedFolders = new ConcurrentBag<string>();
		//In Parallel: Unpack compressed folders and delete the compressed folder
		Parallel.ForEach(compressedFoldersOutput, new ParallelOptions { MaxDegreeOfParallelism = GlobalVariables.maxThreads }, filePath =>
		{
			var extension = Path.GetExtension(filePath);
			//Switch for different compression formats
			switch (extension)
			{
				case ".zip":
					UnpackFolder(filePath);
					unpackedFolders.Add(filePath);
					break;
				case ".tar":
					UnpackFolder(filePath);
					unpackedFolders.Add(filePath);
					break;
				case ".gz":
					UnpackFolder(filePath);
					unpackedFolders.Add(filePath);
					break;
				case ".rar":
					UnpackFolder(filePath);
					unpackedFolders.Add(filePath);
					break;
				case ".7z":
					UnpackFolder(filePath);
					unpackedFolders.Add(filePath);
					break;
				default:
					//Do nothing
					break;
			}
		});
		foreach (string folder in unpackedFolders)
		{
			CompressedFolders.Add(folder);
		}
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
			Logger.Instance.SetUpRunTimeLogMessage("SF CompressFolder " + e.Message, true);
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
			string pathWithoutExtension = path.LastIndexOf('.') > 0 ? path.Substring(0, path.LastIndexOf('.')) : path;
			// Ensure the extraction directory exists
			if (!Directory.Exists(pathWithoutExtension))
			{
				Directory.CreateDirectory(pathWithoutExtension);
			}
			try
			{
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
			} catch (CryptographicException)
			{
				Logger.Instance.SetUpRunTimeLogMessage("SF UnpackFolder " + path + " is encrypted", true);
			} catch (Exception e)
			{
                Logger.Instance.SetUpRunTimeLogMessage("SF UnpackFolder " + e.Message, true);
            }
			//TODO: Delete the compressed folder

		} catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("SF UnpackFolder " + e.Message, true);
		}
	}
	/*
    public string PronomToFullName(string pronom)
    {
        string output;
        switch (pronom)
        {
            #region PDF
            case "fmt/14": output = "Acrobat PDF 1.0"; break;
            case "fmt/15": output = "Acrobat PDF 1.1"; break;
            case "fmt/16": output = "Acrobat PDF 1.2"; break;
            case "fmt/17": output = "Acrobat PDF 1.3"; break;
            case "fmt/18": output = "Acrobat PDF 1.4"; break;
            case "fmt/19": output = "Acrobat PDF 1.5"; break;
            case "fmt/20": output = "Acrobat PDF 1.6"; break;
            case "fmt/276": output = "Acrobat PDF 1.7"; break;
            case "fmt/1129": output = "Acrobat PDF 2.0"; break;
            case "fmt/95": output = "Acrobat PDF/A - 1A"; break;
            case "fmt/354": output = "Acrobat PDF/A - 1B"; break;
            case "fmt/476": output = "Acrobat PDF/A - 2A"; break;
            case "fmt/477": output = "Acrobat PDF/A - 2B"; break;
            case "fmt/478": output = "Acrobat PDF/A - 2U"; break;
            case "fmt/479": output = "Acrobat PDF/A - 3A"; break;
            case "fmt/480": output = "Acrobat PDF/A - 3B"; break;
            case "fmt/481": output = "Acrobat PDF/A - 3U"; break;
            case "fmt/1910": output = "Acrobat PDF/A - 4"; break;
            case "fmt/1911": output = "Acrobat PDF/A - 4E"; break;
            case "fmt/1912": output = "Acrobat PDF/A - 4F"; break;
            //TODO: PDF/X?
            #endregion
            #region Word
            case "fmt/37": output = "Microsoft Word 1.0"; break;
            case "fmt/38": output = "Microsoft Word 2.0"; break;
            case "fmt/39": output = "Microsoft Word 6.0/95"; break;
            case "fmt/40": output = "Microsoft Word 97-2003"; break;
            case "fmt/412": output = "Microsoft Word 2007 onwards"; break;
            case "fmt/523": output = "Microsoft Word Macro enabled 2007 onwards"; break;
            #endregion
            #region Excel
            case "fmt/55": output = "Microsoft Excel 2.x"; break;
            case "fmt/56": output = "Microsoft Excel 3.0"; break;
            case "fmt/57": output = "Microsoft Excel 4.0"; break;
            case "fmt/59": output = "Microsoft Excel 5.0/95"; break;
            case "fmt/61": output = "Microsoft Excel 97"; break;
            case "fmt/62": output = "Microsoft Excel 2000-2003"; break;
            case "fmt/214": output = "Microsoft Excel 2007 onwards"; break;
            case "fmt/445": output = "Microsoft Excel Macro enabled 2007"; break;
			#endregion
			#region PowerPoint

			#endregion
			#region PNG
			case "fmt/11": output = "PNG 1.0"; break;
			case "fmt/12": output = "PNG 1.1"; break;
			case "fmt/13": output = "PNG 1.2"; break;
			#endregion
			#region JPG
			case "fmt/41": output = "RAW JPEG"; break;
			case "fmt/42": output = "JPEG 1.00"; break;
            case "fmt/43": output = "JPEG 1.01"; break;
            case "fmt/44": output = "JPEG 1.02"; break;
			//TODO: Hva er Exchagable Image File Format?
			#endregion
			#region GIF
			case "fmt/3": output = "GIF 87a"; break;
			case "fmt/4": output = "GIF 89a"; break;
			#endregion
			#region TIF
			case "fmt/": output = "TIFF 6.0"; break;
            #endregion
            #region Open Document Standard

            #endregion
            #region RTF

            #endregion
            #region Email

            #endregion
            #region HTML
            case "fmt/96":	output = "HTML 1.0"; break;
            case "fmt/97":	output = "HTML 2.0"; break;
            case "fmt/98":	output = "HTML 3.2"; break;
            case "fmt/99":	output = "HTML 4.0"; break;
            case "fmt/100":	output = "HTML 4.01"; break;
			case "fmt/471":	output = "HTML 5.0"; break;
			case "fmt/102": output = "Extenxible HTML 1.0"; break;
			case "fmt/103": output = "Extenxible HTML 1.1"; break;
            #endregion
            #region PostScript
            case "x-fmt/91":	output = "PostScript 1.0"; break;
            case "x-fmt/406":	output = "PostScript 2.0"; break;
            case "x-fmt/407":	output = "PostScript 2.1"; break;
            case "x-fmt/408":	output = "PostScript 3.0"; break;
            case "fmt/501":		output = "PostScript 3.1"; break;
            #endregion
            case "fmt/494": output = "Microsoft Office Encrypted Document"; break;
            default: output = pronom; break;
        }
        return output;
    }*/
}