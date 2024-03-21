using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Drawing;

public enum HashAlgorithms
{
	MD5,
	SHA256
}

public class FileInfo
{
	public string FilePath { get; set; } = "";					// Filepath relative to input directory
	public string FileName { get; set; } = "";					// Filename with extension
	public string ShortFilePath { get; set; } = "";				// Filepath without input/output directory
	public string OriginalPronom { get; set; } = "";            // Original Pronom ID
	public string NewPronom { get; set; } = "";					// New Pronom ID
	public string OriginalMime { get; set; } = "";				// Original Mime Type
	public string NewMime { get; set; } = "";					// New Mime Type
	public string OriginalFormatName { get; set; } = "";		// Original Format Name
	public string NewFormatName { get; set; } = "";				// New Format Name
	public string OriginalChecksum { get; set; } = "";			// Original Checksum
	public string NewChecksum { get; set; } = "";				// New Checksum
	public List<string> ConversionTools { get; set; } = new List<string>();   // List of conversion tools used
	public long OriginalSize { get; set; } = 0;					// Original file size (bytes)
	public long NewSize { get; set; } = 0;						// New file size (bytes)
	public bool IsConverted { get; set; } = false;				// True if file is converted
	public bool IsModified { get; set; } = false;				// True if file is modified
	public List<string> Route { get; set; } = new List<string>();   // List of modification tools used
	public string TargetPronom { get; set; } = "";				// The pronom the file should be converted to
	public Guid Id { get; set; }								// Unique identifier for the file
	public bool ShouldMerge { get; set; } = false;				// True if file should be merged
	public bool IsMerged { get; set; } = false;					// True if file is merged
	public bool NotSupported { get; set; } = false;				// True if file is not supported
	public bool OutputNotSet { get; set; } = false;				// True if file didn't have a specified format
	public string MergedTo { get; set; } = "";					// The file the file is merged to

	public FileInfo()
	{
	}

	/// <summary>
	/// Constructor for FileInfo
	/// </summary>
	/// <param name="output"> The raw output string from Siegfried</param>
	/// <param name="path"> The relative path for file</param>
	public FileInfo(string output, string path)
	{
		FilePath = path;
		ParseOutput(output);
		//TODO: Hashing algorithm should be set in settings
		//Get checksum
		switch (GlobalVariables.checksumHash)
		{
			case HashAlgorithms.MD5:
				OriginalChecksum = CalculateFileChecksum(MD5.Create());
				break;
			default:
				OriginalChecksum = CalculateFileChecksum(SHA256.Create());
				break;

		}
	}

	public FileInfo(SiegfriedFile siegfriedFile)
	{
		OriginalSize = siegfriedFile.filesize;
		FileName = Path.GetFileName(siegfriedFile.filename);
		if (siegfriedFile.matches.Length > 0) { 
			OriginalPronom = siegfriedFile.matches[0].id;
			OriginalFormatName = siegfriedFile.matches[0].format;
			OriginalMime = siegfriedFile.matches[0].mime;
		}
		FilePath = siegfriedFile.filename;
		OriginalChecksum = siegfriedFile.hash;
	}

	public FileInfo(FileToConvert f)
	{
		var result = Siegfried.Instance.IdentifyFile(f.FilePath, true);
		if (result != null)
		{
			OriginalChecksum = NewChecksum = result.hash;
			OriginalSize = NewSize = result.filesize;
			FileName = Path.GetFileName(f.FilePath);
			if (result.matches.Length > 0)
			{
                OriginalPronom = NewChecksum = result.matches[0].id;
                OriginalFormatName = NewFormatName = result.matches[0].format;
                OriginalMime = OriginalMime = result.matches[0].mime;
            }
		}
        FilePath = f.FilePath;
    }

	/// <summary>
	/// Update the properties of the FileInfo object based on a FileInfo object
	/// </summary>
	/// <param name="f">FileInfo that has new data in it</param>
	public void UpdateSelf(FileInfo f)
	{
		//Set new values based on the input FileInfo
		NewPronom = f.OriginalPronom;
		NewFormatName = f.OriginalFormatName;
		NewMime = f.OriginalMime;
		NewSize = f.OriginalSize;
		NewChecksum = f.OriginalChecksum;
	}

	public void RenameFile(string newName)
	{
		try 
		{ 
			File.Move(FilePath, newName);
			FilePath = newName;
			FileName = Path.GetFileName(newName);
		} catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("RenameFile: " + e.Message, true);
		}
    }

	public void AddConversionTool(string tool)
	{
		ConversionTools.Add(tool);
	}

	/// <summary>
	/// Parses the output from Siegfried and sets the properties of the FileInfo object
	/// </summary>
	/// <param name="output"> The raw output string from Siegfried</param>
	void ParseOutput(string output)
	{
		if(FilePath != null)
			FileName = FilePath.Split('\\').Last();
		else
			FileName = "N/A";

		Regex fileSizeRegex = new Regex(@"filesize : (\d+)");
		Match fileSizeMatch = fileSizeRegex.Match(output);
		if (fileSizeMatch.Success)
		{
			OriginalSize = long.Parse(fileSizeMatch.Groups[1].Value);
		}
		else
		{

		}

		Regex idRegex = new Regex(@"id\s+:\s+'([^']+)'");
		Match idMatch = idRegex.Match(output);
		if (idMatch.Success)
		{
			OriginalPronom = idMatch.Groups[1].Value;
		}
		else
		{

		}

		Regex formatRegex = new Regex(@"format\s+:\s+'([^']+)'");
		Match formatMatch = formatRegex.Match(output);
		if (formatMatch.Success)
		{
			OriginalFormatName = formatMatch.Groups[1].Value;
		}
		else
		{

		}

		Regex mimeRegex = new Regex(@"mime\s+:\s+'([^']+)'");
		Match mimeMatch = mimeRegex.Match(output);
		if (mimeMatch.Success)
		{
			OriginalMime = mimeMatch.Groups[1].Value;
		}
		else
		{

		}
	}

	/// <summary>
	/// Calculates checksum of file
	/// </summary>
	/// <param name="algorithm">What algorithm should be used for hashing</param>
	/// <returns></returns>
	string CalculateFileChecksum(HashAlgorithm algorithm)
	{
		using (var conversionMethod = algorithm)
		{
			try
			{
				using (var stream = File.OpenRead(FileName))
				{
					return BitConverter.ToString(conversionMethod.ComputeHash(stream)).Replace("-", "").ToLower();
				}
			} catch { return "Not found"; }
		}
	}
}
