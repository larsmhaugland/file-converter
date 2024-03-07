/// <summary>
/// Parent class for all converters
/// </summary>
public class Converter
{
	public string? Name { get; set; } // Name of the converter
	public string? Version { get; set; } // Version of the converter
	public Dictionary<string, List<string>>? SupportedConversions { get; set; }
	public List<string> SupportedOperatingSystems { get; set; } = new List<string>();

	public Converter()
	{ }

	public virtual List<string> getSupportedOS() 
	{
		return new List<string>();
	}
	public virtual Dictionary<string, List<string>>? getListOfSupportedConvesions()
	{
		return new Dictionary<string, List<string>>();
	}

	/// <summary>
	/// Checks if the converter supports the conversion of a file from one format to another
	/// </summary>
	/// <param name="originalPronom">The pronom code of the current file format</param>
	/// <param name="targetPronom">The pronom code of the target file format</param>
	/// <returns>True if the converter supports it, otherwise False</returns>
	public bool SupportsConversion(string originalPronom, string targetPronom)
	{
        if (SupportedConversions != null && SupportedConversions.ContainsKey(originalPronom))
		{
			return SupportedConversions[originalPronom].Contains(targetPronom);           
        }
		return false;
    }

	/// <summary>
	/// Wrapper for the ConvertFile method that also handles the timeout
	/// </summary>
	/// <param name="fileinfo"></param>
	public void ConvertFile(FileToConvert fileinfo)
	{
		/*
		if(fileinfo.FilePath.First() != '"')
		{
			fileinfo.FilePath = "\"" + fileinfo.FilePath;
		}
		if(fileinfo.FilePath.Last() != '"')
		{
			fileinfo.FilePath += "\"";
		}*/
		ConvertFile(fileinfo, fileinfo.Route.First());

	}

	/// <summary>
	/// Convert a file to a new format
	/// </summary>
	/// <param name="fileinfo">The file to be converted</param>
	/// <param name="pronom">The file format to convert to</param>
	public virtual void ConvertFile(FileToConvert fileinfo, string pronom)
	{ }
	
	/// <summary>
	/// Combine multiple files into one file
	/// </summary>
	/// <param name="files">List of files that should be combined</param>
	/// <param name="pronom">The file format to convert to</param>
	public virtual void CombineFiles(List<FileInfo> files, string pronom)
	{ }

	/// <summary>
	/// Delete an original file, that has been converted, from the output directory
	/// </summary>
	/// <param name="fileInfo">The specific file to be deleted</param>
	public virtual void deleteOriginalFileFromOutputDirectory(string fileInfo)
	{
		try
		{
			if (File.Exists(fileInfo))
			{
				File.Delete(fileInfo);
			}
		} catch (Exception e)
		{
            Logger.Instance.SetUpRunTimeLogMessage("deleteOriginalFileFromOutputDirectory: " + e.Message, true);
        }
	}
	public virtual void replaceFileInList(string newPath, FileToConvert f)
	{
        f.FilePath = newPath;
        var file = FileManager.Instance.GetFile(f.Id);
		if (file != null)
		{
			file.FilePath = newPath;
			file.FileName = Path.GetFileName(newPath);

		} else
		{
			Logger.Instance.SetUpRunTimeLogMessage("replaceFileInList: File not found in FileManager", true,filename: f.FilePath);
		}
		
	}

	/// <summary>
	/// Check if a file has been converted and update the file list
	/// </summary>
	/// <param name="file">File that has been converted</param>
	/// <param name="newFilepath">Filepath to new file</param>
	/// <param name="newFormat">Target pronom code</param>
	/// <returns>True if the conversion succeeded, otherwise false</returns>
	public bool CheckConversionStatus(string newFilepath, string newFormat, FileToConvert file)
	{
		try
		{
			var result = Siegfried.Instance.IdentifyFile(newFilepath, false);
			if (result != null)
			{
				if (result.matches[0].id == newFormat)
				{
					deleteOriginalFileFromOutputDirectory(file.FilePath);
					replaceFileInList(newFilepath, file);
					return true;
				}
			}
		}
        catch (Exception e)
		{
			Logger.Instance.SetUpRunTimeLogMessage("CheckConversionStatus: " + e.Message, true);
        }
		return false;
	}
}