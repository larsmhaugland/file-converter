using iText.Forms.Form.Element;
using System;

/// <summary>
/// Parent class for all converters
/// </summary>
public class Converter
{
	public string? Name { get; set; } // Name of the converter
	public string? Version { get; set; } // Version of the converter
	public Dictionary<string, List<string>>? SupportedConversions { get; set; }

	private List<FileInfo> files = new List<FileInfo>(FileManager.Instance.Files);
	public Converter()
	{ }



	public virtual Dictionary<string, List<string>>? listOfSupportedConversions()
	{
		return new Dictionary<string, List<string>>();
	}

	/// <summary>
	/// Convert a file to a new format
	/// </summary>
	/// <param name="fileinfo">The file to be converted</param>
	/// <param name="pronom">The file format to convert to</param>
	public virtual void ConvertFile(string fileinfo, string pronom)
	{ }
	
	/// <summary>
	/// Combine multiple files into one file
	/// </summary>
	/// <param name="files">Array of files that should be combined</param>
	/// <param name="pronom">The file format to convert to</param>
	public virtual void CombineFiles(string []files, string pronom)
	{ }

	/// <summary>
	/// Delete an original file, that has been converted, from the output directory
	/// </summary>
	/// <param name="fileInfo">The specific file to be deleted</param>
	public virtual void deleteOriginalFileFromOutputDirectory(string fileInfo)
	{
		if (File.Exists(fileInfo))
		{
			File.Delete(fileInfo);
		}
	}
	public virtual void replaceFileInList(string filepathBefore, string filepathAfter)
	{
		foreach (var file in files) 
		{
			if (filepathBefore.Equals(file.FilePath)) 
			{
				file.FilePath = filepathAfter;
			}
		}
	}

	/// <summary>
	/// Check if a file has been converted and update the file list
	/// </summary>
	/// <param name="oldFilepath">Filepath to original file</param>
	/// <param name="newFilepath">Filepath to new file</param>
	/// <param name="newFormat">Target pronom code</param>
	/// <returns></returns>
	public bool CheckConversionStatus(string oldFilepath, string newFilepath, string newFormat)
	{
		Siegfried sf = Siegfried.Instance;
		var file = sf.IdentifyFile(newFilepath, false);
		if (file != null)
		{
			if (file.matches[0].id == newFormat)
			{
				replaceFileInList(oldFilepath, newFilepath);
				deleteOriginalFileFromOutputDirectory(oldFilepath);
				return true;
			}
		}
		else
		{
			Console.WriteLine("File not found");
		}
		return false;
	}
}