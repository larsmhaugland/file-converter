using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class FileManager
{
	string InputFolder;		// Path to input folder
	string OutputFolder;    // Path to output folder
	List<FileInfo> Files;	// List of files to be converted

	public FileManager()
	{
	}

	public FileManager(string input, string output)
	{
		InputFolder = input;
		OutputFolder = output;
	}

    /**
	 * 
	 */
    void IdentifyFiles()
    {
		string[] filePaths = Directory.GetFiles(InputFolder, "*.*", SearchOption.AllDirectories);
        Parallel.ForEach(filePaths, filePath =>
        {
            Files.Add(GetFileInfo(filePath));
        });
    }

    static FileInfo GetFileInfo(string filePath)
    {
        // Wrap the file path in quotes
        filePath = "\"" + filePath + "\"";
        // Define the process start info
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = @"", // or any other command you want to run
            Arguments = filePath,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        string output = "";
        // Create the process
        using (Process process = new Process { StartInfo = psi })
        {
            // Start the process
            process.Start();

            // Read the output
            output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            // Wait for the process to exit
            process.WaitForExit();

            // Display the output and error
            //Console.WriteLine("Output:\n" + output);
            if (error.Length > 0)
                Console.WriteLine("Error:\n" + error);
        }
        return ParseFileInformation(output);
    }

    static FileInfo ParseFileInformation(string output)
    {
        FileInfo fileInfo = new FileInfo();

        // Use regular expressions to extract the relevant information
        Regex fileNameRegex = new Regex(@"filename : '(.+)'");
        Match fileNameMatch = fileNameRegex.Match(output);
        if (fileNameMatch.Success)
        {
            fileInfo.FileName = fileNameMatch.Groups[1].Value;
        }
        //Get only file name
        fileInfo.FileName = fileInfo.FileName.Split('\\').Last();

        Regex fileSizeRegex = new Regex(@"filesize : (\d+)");
        Match fileSizeMatch = fileSizeRegex.Match(output);
        if (fileSizeMatch.Success)
        {
            fileInfo.OriginalSize = long.Parse(fileSizeMatch.Groups[1].Value);
        }

        Regex idRegex = new Regex(@"id\s+:\s+'([^']+)'");
        Match idMatch = idRegex.Match(output);
        if (idMatch.Success)
        {
            fileInfo.OriginalPronom = idMatch.Groups[1].Value;
        }

        Regex formatRegex = new Regex(@"format\s+:\s+'([^']+)'");
        Match formatMatch = formatRegex.Match(output);
        if (formatMatch.Success)
        {
            fileInfo.OriginalFormatName = formatMatch.Groups[1].Value;
        }

        return fileInfo;
    }
}
