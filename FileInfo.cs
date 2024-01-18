using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public enum HashAlgorithms
{
    MD5,
    SHA256
}

public class FileInfo
{
	public string FileName { get; set; } 				// Relative path to file with root of Input folder
	public string OriginalPronom { get; set; }			// Original Pronom ID
	public string NewPronom { get; set; }				// New Pronom ID
	public string OriginalMime { get; set; }			// Original Mime Type
	public string NewMime { get; set; }                 // New Mime Type
    public string OriginalFormatName { get; set; }      // Original Format Name
    public string NewFormatName { get; set; }           // New Format Name
    public string OriginalChecksum { get; set; }        // Original Checksum
    public string NewChecksum { get; set; }             // New Checksum
	public bool IsConverted { get; set; }				// True if file is converted
	public bool SupportsConversion { get; set; }        // True if file supports conversion
    public long OriginalSize { get; set; }              // Original file size
    public long NewSize { get; set; }                   // New file size

    private HashAlgorithms HashingAlgorithm;
    
    Logger logger = Logger.Instance;
    public FileInfo()
	{
	}

	/** Constructor taking output from SiegFried as input.
	 * 
	 * @param output
	 *     
	 */
	public FileInfo(string output)
	{

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

    void ParseOutput(string output)
    {
        // Use regular expressions to extract the relevant information
        Regex fileNameRegex = new Regex(@"filename : '(.+)'");
        Match fileNameMatch = fileNameRegex.Match(output);
        if (fileNameMatch.Success)
        {
            string path = fileNameMatch.Groups[1].Value;
            //Get only relative path from Output dir
            FileName = path.Split('\\').Last();
        }
        else
        {

        }

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
    }
    string CalculateFileChecksum(HashAlgorithm algorithm)
    {
        using (var conversionMethod = algorithm)
        {
            using (var stream = File.OpenRead(FileName))
            {
                return BitConverter.ToString(conversionMethod.ComputeHash(stream)).Replace("-", "").ToLower();
            }
        }
    }

    
}
