using System;

public class Converter
{
	public string Name { get; set; } // Name of the converter
	public string Version { get; set; } // Version of the converter

	public Converter()
	{
	}

	public virtual void ConvertFile(FileInfo fileinfo)
	{ }
	public virtual void CombineFiles()
	{ }

	public virtual void updateFileInfo(FileInfo fileinfo) {
	}

	public virtual void deleteOriginalFileFromOutputDirectory(FileInfo fileInfo) {
		string outputDirectory = GlobalVariables.parsedOptions.Output;
		string fileToDelete = Path.Combine(outputDirectory, Path.GetFileName(fileInfo.FileName));
		if (File.Exists(fileToDelete))
		{
            File.Delete(fileToDelete);
        }
	}

	//TODO: Implement base methods
	//TODO: Try again if files fail to convert
}