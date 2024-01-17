using System;

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
		//Basically just the code from FileManager.cs
	}
}
