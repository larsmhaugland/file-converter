using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Security.Cryptography.X509Certificates;

public enum HashAlgorithms
{
    MD5,
    SHA256
}

public class FileInfo
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; }                    // Relative path

    [JsonPropertyName("filename")]
    public string FileName { get; set; } 				// Filename with extention

    [JsonPropertyName("")]
    public string OriginalPronom { get; set; }			// Original Pronom ID

    [JsonPropertyName("")]
    public string NewPronom { get; set; }				// New Pronom ID

    [JsonPropertyName("")]
    public string OriginalMime { get; set; }			// Original Mime Type

    [JsonPropertyName("")]
    public string NewMime { get; set; }                 // New Mime Type

    [JsonPropertyName("")]
    public string OriginalFormatName { get; set; }      // Original Format Name

    [JsonPropertyName("")]
    public string NewFormatName { get; set; }           // New Format Name

    [JsonPropertyName("")]
    public string OriginalChecksum { get; set; }        // Original Checksum

    [JsonPropertyName("")]
    public string NewChecksum { get; set; }             // New Checksum

    [JsonPropertyName("")]
    public List<string> ConversionTools { get; set; }   // List of conversion tools used

    [JsonPropertyName("")]
    public long OriginalSize { get; set; }              // Original file size (bytes)

    [JsonPropertyName("")]
    public long NewSize { get; set; }                   // New file size (bytes)

    [JsonPropertyName("")]
    public bool IsConverted { get; set; }				// True if file is converted

    [JsonPropertyName("")]
    public bool SupportsConversion { get; set; }        // True if file supports conversion 

    [JsonPropertyName("hashingAlgorithm")]
    private HashAlgorithms HashingAlgorithm;
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
                using (var stream = File.OpenRead(FilePath))
                {
                    return BitConverter.ToString(conversionMethod.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            } catch { return "Not found"; }
        }
    }
}
