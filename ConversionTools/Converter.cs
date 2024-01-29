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

	//TODO: Implement base methods
	//TODO: UpdateFileInfo
	//TODO: Try again if files fail to convert
}