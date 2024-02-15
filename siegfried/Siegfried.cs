using Ghostscript.NET;
using iText.IO.Source;
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
		//TODO: Should check Version and ScanDate here
		CompressedFolders = new List<string>();
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
			options = $"-home siegfried -json -hash " + HashEnumToString(GlobalVariables.checksumHash) + " -sig pronom64k.sig ";
		} else
		{
			options = $"-home siegfried -json -sig pronom64k.sig ";
		}

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
			Logger.Instance.SetUpRunTimeLogMessage("SF IdentifyFile " + error, true);
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
		string options = $"-home siegfried -multi 64 -json -coe -hash " +  HashEnumToString(GlobalVariables.checksumHash) + " -sig pronom64k.sig ";

		string outputFolder = "siegfried/JSONoutput/";
		string dir = Path.Combine(outputFolder);
		string outputFile = dir + Guid.NewGuid().ToString() + ".json";
		string? parentDir = Directory.GetParent(outputFile)?.FullName;

		//Create output file
		try
		{
			if (parentDir != null && !Directory.Exists(parentDir))
			{
				Directory.CreateDirectory(parentDir);
			}
			else if (parentDir == null)
			{
				logger.SetUpRunTimeLogMessage("SF IdentifyList could not create output directory " + outputFile, true);
				throw new Exception("SF IdentifyList could not create output directory " + outputFile);
			}
			File.Create(outputFile).Close();
		}
		catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("SF IdentifyList could not create output file " + e.Message, true);
			throw new Exception("SF IdentifyList could not create output file " + e.Message);
		}

		// Define the process start info
		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = @"siegfried/sf.exe", // or any other command you want to run
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
			Logger.Instance.SetUpRunTimeLogMessage("SF " + error, true);
			//return; 
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
			if (file != null)
			{
				file.OriginalChecksum = parsedData.files[i].hash;
				if (paths.Length - 1 >= i)
				{
					file.FilePath = paths[i];
					file.FileName = Path.GetFileName(file.FileName);
				}
				files.Add(file);
			}
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

		Parallel.ForEach(filePathGroups, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePaths =>
		{
			var output = IdentifyList(filePaths);
			if (output == null)
			{
				logger.SetUpRunTimeLogMessage("SF IdentifyFilesIndividually could not identify files", true);
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

	public Task<List<FileInfo>>? IdentifyCompressedFilesJSON(string input)
	{
		Logger logger = Logger.Instance;
		UnpackCompressedFolders();
		var fileBag = new ConcurrentBag<FileInfo>();
		
		//For eaccompressed folder, identify all files
		Parallel.ForEach(CompressedFolders, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, folder =>
		{
			//Identify all file paths in compressed folder and group them
			var pathWithoutExt = folder.Split('.')[0]; //TODO: This might not work for paths with multiple file extensions
			var paths = Directory.GetFiles(pathWithoutExt, "*.*", SearchOption.AllDirectories);
			var filePathGroups = GroupPaths(new List<string>(paths));
			//Identify all files in each group
			Parallel.ForEach(filePathGroups, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, paths =>
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
		return Task.FromResult(fileBag.ToList());
	}

	/// <summary>
	/// Identifies all files in input directory and returns a list of FileInfo objects. 
	/// Siegfried output is put in a JSON file.
	/// </summary>
	/// <param name="inputFolder">Path to root folder for files to be identified</param>
	/// <returns>List of all identified files or null</returns>
	public Task<List<FileInfo>>? IdentifyFilesJSON(string inputFolder)
	{
		Logger logger = Logger.Instance;
		var files = new List<FileInfo>();
		// Wrap the file path in quotes
		string wrappedPath = "\"" + inputFolder + "\"";
		string options = $"-home siegfried -multi 64 -hash sha256 -json -sig pronom64k.sig ";
		string outputFolder = "siegfried/JSONoutput/";
		string dir = Path.Combine(outputFolder,inputFolder);
		string outputFile = dir + ".json";
		string ?parentDir = Directory.GetParent(outputFile)?.FullName;
		
		//Create output file
		try
		{
			if (parentDir != null && !Directory.Exists(parentDir))
			{
				Directory.CreateDirectory(parentDir);
			} else if(parentDir == null)
			{
				logger.SetUpRunTimeLogMessage("SF IdentifyFilesJSON could not create output file/directory " + outputFile, true);
				throw new Exception("SF IdentifyFilesJSON could not create output file/directory " + outputFile);
			}
			File.Create(outputFile).Close();
		}
		catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("SF IdentifyFilesJSON could not create output file " + e.Message, true);
			throw new Exception("SF IdentifyFilesJSON could not create output file " + e.Message);
		}
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
			}
		}
		return Task.FromResult(files);
	}

	SiegfriedJSON? ParseJSONOutput(string json, bool file)
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
				using (JsonDocument document = JsonDocument.Parse(json))
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
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Logger.Instance.SetUpRunTimeLogMessage("SF ParseJSON " + e.Message, true);
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
		Parallel.ForEach(compressedFoldersOutput, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
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
			//TODO: There may be a problem with this method of getting the path
			// Get path to folder without extention
			string pathWithoutExtension = path.Split('.')[0];
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
}