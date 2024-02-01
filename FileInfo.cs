using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

public enum HashAlgorithms
{
	MD5,
	SHA256
}

public class FileInfo
{
	[JsonPropertyName("filePath")]
	public string FilePath { get; set; } = "";                // TODO: Remove?

	[JsonPropertyName("fileName")]
	public string FileName { get; set; } = "";              // Filename with extention

	[JsonPropertyName("originalPronom")]
	public string OriginalPronom { get; set; } = "";            // Original Pronom ID

	[JsonPropertyName("newPronom")]
	public string NewPronom { get; set; } = "";             // New Pronom ID

	[JsonPropertyName("originalMime")]
	public string OriginalMime { get; set; } = "";          // Original Mime Type

	[JsonPropertyName("newMime")]
	public string NewMime { get; set; } = "";                 // New Mime Type

	[JsonPropertyName("originalFormatName")]
	public string OriginalFormatName { get; set; } = "";      // Original Format Name

	[JsonPropertyName("newFormatName")]
	public string NewFormatName { get; set; } = "";           // New Format Name

	[JsonPropertyName("originalChecksum")]
	public string OriginalChecksum { get; set; } = "";        // Original Checksum

	[JsonPropertyName("newChecksum")]
	public string NewChecksum { get; set; } = "";             // New Checksum

	[JsonPropertyName("conversionTools")]
	public List<string> ConversionTools { get; set; } = new List<string>();   // List of conversion tools used

	[JsonPropertyName("originalSize")]
	public long OriginalSize { get; set; } = 0;              // Original file size (bytes)

	[JsonPropertyName("newSize")]
	public long NewSize { get; set; } = 0;                   // New file size (bytes)

	[JsonPropertyName("isConverted")]
	public bool IsConverted { get; set; } = false;				// True if file is converted

	[JsonPropertyName("hashingAlgorithm")]
	private HashAlgorithms HashingAlgorithm = HashAlgorithms.SHA256;
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
		//HashingAlgorithm = GlobalVariables.HashingAlgorithm;
		//Get checksum
		switch (HashingAlgorithm)
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
		FileName = siegfriedFile.filename;
		OriginalPronom = siegfriedFile.matches[0].id;
		OriginalFormatName = siegfriedFile.matches[0].format;
		OriginalMime = siegfriedFile.matches[0].mime;
		FilePath = siegfriedFile.filename;


		switch(HashingAlgorithm)
		{
			case HashAlgorithms.MD5:
				OriginalChecksum = CalculateFileChecksum(MD5.Create());
				break;
			default:
				OriginalChecksum = CalculateFileChecksum(SHA256.Create());
				break;
		}
	}

	public bool CheckIfConverted()
	{
        //Get new pronom
        var newInfo = Siegfried.Instance.IdentifyFile(FileName);
        if (newInfo != null && newInfo.matches[0].id == GlobalVariables.FileSettings[OriginalPronom])
        {
            NewPronom = newInfo.matches[0].id;
            NewFormatName = newInfo.matches[0].format;
            NewMime = newInfo.matches[0].mime;
            NewSize = newInfo.filesize;

            //Get checksum
            switch (HashingAlgorithm)
            {
                case HashAlgorithms.MD5:
                    NewChecksum = CalculateFileChecksum(MD5.Create());
                    break;
                default:
                    NewChecksum = CalculateFileChecksum(SHA256.Create());
                    break;
            }
			IsConverted = true;
			return true;
        }
		return false;
    }


	/// <summary>
	/// 
	/// </summary>
	public void SetNewFields()
	{
		//Get checksum
		switch (HashingAlgorithm)
		{
			case HashAlgorithms.MD5:
				NewChecksum = CalculateFileChecksum(MD5.Create());
				break;
			default:
				NewChecksum = CalculateFileChecksum(SHA256.Create());
				break;
		}

		//Get new pronom
		var newInfo = Siegfried.Instance.IdentifyFile(FileName);
		if(newInfo != null)
		{
			NewPronom = newInfo.matches[0].id;
			NewFormatName = newInfo.matches[0].format;
			NewMime = newInfo.matches[0].mime;
			NewSize = newInfo.filesize;
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
