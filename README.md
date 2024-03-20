# file-converter
>NOTE: This README is currently being updated, information found here might not reflect the current application.

![Static Badge](https://img.shields.io/badge/.net-8.0-blue)
![dotnet-badge](https://github.com/larsmhaugland/file-converter/actions/workflows/dotnet.yml/badge.svg?event=push)
[![standard-readme compliant](https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square)](https://github.com/RichardLitt/standard-readme)

A module-based .NET application that converts files and generates documentation for archiving.

This application provides a framework for different conversion libraries/software to work together. It aims to promote a comprehensive open-source solution for file conversion, as opposed to the many paid options, which allows for multi-step conversion between different external libraries. 

# Table of Contents
- [Background](#background)
- [Install](#install)
  - [Dependencies](#dependencies)
  - [Installation for Windows](#installation-for-windows)
  - [Installation for Linux](#installation-for-linux)
- [Usage](#usage)
  - [Beta notes](#beta)	 
  - [GUI](#gui)
  - [CLI](#cli)
  - [Settings](#settings)
  - [Currently supported file formats](#currently-supported-file-formats)
  - [Documentation and logging](#documentation-and-logging)
  - [Adding a new converter](#adding-a-new-converter)
  - [Adding a new conversion path (Multistep conversion)](#adding-a-new-conversion-path-multistep-conversion)
- [Use cases](#use-cases)
- [Acknowledgments](#acknowledgments)
- [Contributing](#contributing)
- [Licensing](#licensing)


## Background 
This project is part of a collaboration with [Innlandet County Archive](https://www.visarkiv.no/) and is a Bachelor's thesis project for a [Bachelor's in Programming](https://www.ntnu.edu/studies/bprog) at the [Norwegian University of Technology and Science (NTNU)](https://www.ntnu.edu/).

In Norway, the act of archiving is regulated by the Archives Act, which states that public bodies have a duty to register and preserve documents that are created as part of their activity [^1]. As society is becoming more digitized so is information, and the documents that were previously physical and stored physically are now digital and stored digitally. Innlandet County Archive is an inter-municipal archive cooperation, that registers and preserves documents from 48 municipalities. However, not all file types they receive are suitable for archiving as they run a risk of becoming obsolete. (For further reading see: [Obsolescence: File Formats and Software](https://dpworkshop.org/dpm-eng/oldmedia/obsolescence1.html)) Innlandet County Archive wished to streamline its conversion process into one application that could deal with a vast array of file formats. Furthermore, archiving is based on the principles of accessibility, accountability and integrity, which is why this application also provides documentation of all changes made to files.

Much like programmers and software developers, archivists believe in an open source world. Therefore it would only be right for this program to be open source. 

[^1]: Kultur- og likestillingsdepartementet. *Lov om arkiv [arkivlova].* URL: https://lovdata.no/dokument/NL/lov/1992-12-04-126?q=arkivloven (visited on 17th Jan. 2024).

## Install
To download the application source code simply run:
 ```sh
  git clone --recursive https://github.com/larsmhaugland/file-converter.git
```

Then open it in your .NET IDE of choice (We are using Microsoft Visual Studios) and build it. 
<br>Alternatively, you can build it from the command line using:
```sh
dotnet build
```

>NOTE: Cloning **with** the Git submodules is required for the application to work.
>If you did not clone the repository recursively or do not see the git submodules in your local repository we would suggest:
> ```sh
>   git submodule init
>   git submodule update
>```

### Dependencies
> NOTE: For dependencies for specific Linux distributions please see [Installation for Linux](#installation-for-linux)

|OS| Dependencies | Needed for? |
|---|---|---|
|Linux| [dotnet version 8.0](https://dotnet.microsoft.com/en-us/download) | Needed to run program. |
| Windows | [dotnet version 8.0](https://dotnet.microsoft.com/en-us/download) | Needed to run program. |

#### External libraries/software used
**Libraries**
- [iText7](https://github.com/itext/itext-dotnet) under the GNU Affero General Public License v3.0.
- [BouncyCastle.NetCore](https://github.com/chrishaly/bc-csharp) under the MIT License.
- [iText7 Bouncycastle Adapter](https://www.nuget.org/packages/itext7.bouncy-castle-adapter/8.0.2) under the GNU Affero General Public License v3.0.
- [CommandLineParser](https://www.nuget.org/packages/CommandLineParser/) under the MIT License.
- [SharpCompress](https://github.com/adamhathcock/sharpcompress) under the MIT License.

**Software**
- [GhostScript](https://www.ghostscript.com/index.html) under the GNU Affero General Public License v3.0.
- [LibreOffice](https://www.libreoffice.org/) under the Mozilla Public License 2.0.
- [Siegfried](https://www.itforarchivists.com/siegfried/) under the Apache License 2.0.

### Installation for Windows
### Installation for Linux

**Downloading dependencies for Linux distributions**
| Distro | Dependency |
|---|---|
| Ubuntu/Debian | curl |
| Arch Linux | curl <br> brew |
| Fedora/Red Hat | brew [^2] |

> NOTE: Fedora/Red hat has not been tested yet! We're working on it

[^2]:*Homebrew on Linux* URL: https://docs.brew.sh/Homebrew-on-Linux (visited on 3rd Mar. 2024)

## Usage
Common usage (code block)

### Beta
Since the program is still in beta, there are some limitations to the software. This section will be updated throughout the development process.
- Multi-threading
	- The program gets varied results if multi-threading (```<MaxThreads>``` is over 1) is enabled. For consistent results, use ```<MaxThreads>1</MaxThreads>```
- Parsing siegfried data from incomplete run
  	- The current version of the program cannot successfully recover siegfried data from an incomplete run


### GUI
Common usage GUI

### CLI
Cover options and common usage
```
$ cd C:\PathToFolder\bin\Debug\net8.0
$ .\file-converter-prog2900.exe 
```

**Options**
```
$ .\example -i "C:\Users\user\Downloads
$ .\example -input "C:\Users\user\Downloads

$ .\example -o "C:\Users\user\Downloads
$ .\example -output "C:\Users\user\Downloads
```


### Settings
Settings can be manually set in the ```settings.xml``` file.

#### Setting run time arguments
```xml  
    <Requester></Requester>                    <!-- Name of person requesting the conversion -->
    <Converter></Converter>                    <!-- Name of person doing the conversion -->
	<ChecksumHashing></ChecksumHashing>          <!-- SHA256 (standard) or MD5 -->
	<InputFolder></InputFolder>                  <!-- Specify input folder, default is "input" -->
	<OutputFolder></OutputFolder>                <!-- Specify output folder, default is "output" -->
	<MaxThreads></MaxThreads>	                   <!-- Write a number, deafult is cores*2 -->
	<Timeout></Timeout>			                     <!-- Timeout in minutes, default is 30min -->
```

The first part of the XML file concerns arguments needed to run the program. The second part allows you to set up two things:
1. Global settings stating that file format ```x``` should be converted to file format ```y```.
2. Folder settings stating that file format ```x``` should be converted to file format ```y``` in the specific folder ```folder```.

#### Global settings
```xml
<FileClass>
	<ClassName>pdf</ClassName>
	<Default>fmt/477</Default>        <!-- The target PRONOM code the class should be converted to -->
	<FileTypes>
		<Filename>pdf</Filename>
    <Pronoms>                             <!-- List of all PRONOMs that should be converted to the target PRONOM -->
			fmt/95,fmt/354,fmt/476,fmt/477 ,fmt/478 ,fmt/479 ,fmt/480
		</Pronoms>
		<Default></Default>
	</FileTypes>
</FileClass>
```

#### Folder settings
```xml
	<FolderOverride>
		<FolderPath>apekatter</FolderPath>      <!-- Path after input folder example: /documents -->
		<Pronoms>fmt/41, fmt/42, fmt/43, fmt/44, x-fmt/398</Pronoms>
		<ConvertTo>fmt/14</ConvertTo>
		<MergeImages></MergeImages>             <!-- Yes, No -->
	</FolderOverride>
```

### Currently supported file formats
The following table shows supported file formats one can convert *from.*
| File format| External converter | Linux | Windows |
|-------------|-----------|--------------------|-------|
| JPG | iText7 | Yes | Yes|
| PNG | iText7 | Yes | Yes|
| TIFF | iText7 | Yes | Yes|
| BMP | iText7 | Yes | Yes|
| HTML | iText7 | Yes | Yes |
| PDF | itext7 | Yes | Yes |
| PostScript | GhostScript | No | Yes |
| DOC | LibreOffice | Yes | Yes |
| DOCX | LibreOffice | Yes | Yes |
| XLS | LibreOffice | Yes | Yes |
| XLSX | LibreOffice | Yes | Yes |
| PPT | LibreOffice | Yes | Yes |
| PPTX | LibreOffice | Yes | Yes |

The following table shows supported file formats one can convert *to.*
| File format| External converter | Linux | Windows |
|-------------|-----------|--------------------|-------|
| JPG | GhostScript | No | Yes|
| PNG | GhostScript | No | Yes|
| TIFF | GhostScript | No | Yes|
| BMP | GhostScript | No | Yes|
| PDF | itext7 | Yes | Yes |
| PDF-A | itext7 | Yes | Yes |
| DOC | LibreOffice | Yes | Yes |
| DOCX | LibreOffice | Yes | Yes |
| XLS | LibreOffice | Yes | Yes |
| XLSX | LibreOffice | Yes | Yes |
| PPT | LibreOffice | Yes | Yes |
| PPTX | LibreOffice | Yes | Yes |

### Documentation and logging
The ```.txt```log files use the following convention and is automatically generated each time the program is run:
```
Type	| (Error) Message | Format | Filetype | Filename
```
All log files can be found in the ```logs``` folder.

Additionally, a ```documentation.json``` file is created which lists all files and their respective data.
```json
{"Metadata": {
    "requester": "Name",
    "converter": "Name"
  },
  "Files": [
    {
      "Filename": "output\\filename.pdf",
      "OriginalPronom": "fmt/14",
      "OriginalChecksum": "6c6458545d3a41967a5ef2f12b1b03ad6a6409641670f823635cfb766181f636",
      "OriginalSize": 513631,
      "TargetPronom": "fmt/477",
      "NewPronom": "fmt/477",
      "NewChecksum": "b462a8261d26ece8707fac7f6921cc0ddfb352165cb608a38fed92ed044a6a05",
      "NewSize": 519283,
      "Converter": [
	"iText7"
	],
      "IsConverted": true
    }]}
```

### Adding a new converter
All source code for external converters is based on the same parent ```Converter``` class, located in ```\ConversionTools\Converter.cs```.

**Converter class**
```csharp
	public string? Name { get; set; } // Name of the converter
	public string? Version { get; set; } // Version of the converter
	public Dictionary<string, List<string>>? SupportedConversions { get; set; }
	public List<string> SupportedOperatingSystems { get; set; } = new List<string>();

	private List<FileInfo> files = new List<FileInfo>(FileManager.Instance.Files);

	public virtual void ConvertFile(string fileinfo, string pronom){ }
	public virtual void CombineFiles(string []files, string pronom){ }
```

All fields shown in the code block above must be included in the subclass for the new external converter to work properly. If you are adding a library-based converter we would suggest having a look at ```iText7.cs``` for examples on how to structure the subclass.
For external converters where you want to parse arguments and use an executable in CLI we would suggest looking at ```GhostScript.cs```.

>NOTE: If you are adding an **executable** file that you want to use it needs to be included in the ```.csproj``` file as such to be loaded properly at runtime:
>```xml
><ItemGroup>
>	<None Update="PathToExecutableFile">
>	   <CopyToOutputDirectory>Always</CopyToOutputDirectory>
>	</None>
></ItemGroup>
>```
>This will make the executable file available at the path ```file-converter-prog2900\bin\Debug\net8.0\PathToExecutableFile```.

All subclasses of ```Converter``` follow the same commenting scheme for consistency and ease when maintaining/debugging the application. It should state that it is a subclass of the ```Converter```class and which conversions it supports. Other functionalities of the converter, such as combining images, can be added after.

**Commenting scheme**
```csharp
/// <summary>
/// iText7 is a subclass of the Converter class.                                                     <br></br>
///                                                                                                  <br></br>
/// iText7 supports the following conversions:                                                       <br></br>
/// - Image (jpg, png, gif, tiff, bmp) to PDF 1.0-2.0                                                <br></br>
/// - Image (jpg, png, gif, tiff, bmp) to PDF-A 1A-3B                                                <br></br>
/// - HTML to PDF 1.0-2.0                                                                            <br></br>
/// - PDF 1.0-2.0 to PDF-A 1A-3B                                                                     <br></br>                                                                          
///                                                                                                  <br></br>
/// iText7 can also combine the following file formats into one PDF (1.0-2.0) or PDF-A (1A-3B):      <br></br>
/// - Image (jpg, png, gif, tiff, bmp)                                                               <br></br>
///                                                                                                  <br></br>
/// </summary>
```

To add the converter to the list of converters, add the line ```converters.Add(new NameOfConverter());``` in the ```AddConverter``` class. Assuming that the source code written for the converter is correct, and the settings are set correctly, the application should now use the new converter for the conversions it supports. 
```csharp
    public List<Converter> GetConverters()
    {
        List<Converter> converters = new List<Converter>();
        converters.Add(new iText7());
        converters.Add(new GhostscriptConverter());
        converters.Add(new LibreOfficeConverter());
	/*Add a new converter here!*/
        var currentOS = Environment.OSVersion.Platform.ToString();
        converters.RemoveAll(c => c.SupportedOperatingSystems == null ||
                                  !c.SupportedOperatingSystems.Contains(currentOS));
        return converters;
    }
```
### Adding a new conversion path (Multistep conversion)
Multistep conversion means that one can combine the functionality of several converters to convert a file to a file type that would not have been possible if you were using only one of the converters. For example, LibreOffice can convert Word documents to PDF and iText7 can convert PDF documents to PDF-A. Multistep conversion means that the functionalities can be combined so that a Word document can be converted to a PDF-A document. 

To add a new multistep conversion you need to add a route in the ```initMap``` function in ```ConversionManager.cs``` following this convention:

```csharp
foreach (string pronom in ListOfPronoms)
{
	foreach (string otherpronom in ListOfPronoms)
	{
		if (pronom != otherpronom){
			ConversionMap.Add(new KeyValuePair<string, string>(pronom, otherpronom), [pronom, helppronom , otherpronom]); 
	}}
```
```pronom``` is the pronom you want to convert from, while ```otherpronom``` is the pronom you want to convert to. ```ConversionMap``` works as a route so any ```helppronom``` is a stepping stone in that route from ```pronom``` to ```otherpronom```. You can add as many stepping stones as you want but they have to be added in the correct order from left to right.

## Use cases
Here are the use cases, please reflect and write something down for each use case about how understandable the README/source code was and how manageable it was to do the task.

Use case tasks:
+ Add a new conversion path route that converts a document from Word to PDF to PDF-A.
+ Change the settings, using the ```settings.xml``` so that Word documents get converted to PDF-A.
+ Change the settings, using the GUI, so that Word documents get converted to PDF-A.
+ Run the program in CLI with the proper options to specify the input and output directory.
+ Try to combine a set of images into one PDF.

Questions regarding use cases:
+ Were there any tasks you weren't able to complete? Was it because of a lack of understanding or due to a bug in the program?
+ Were there any tasks that you feel could be simplified?

Questions regarding README:
+ Was it clear how to download, install and build the program?
+ Did you encounter any instructions in the README that weren't correct?
+ Were there any sections you found vague/unhelpful?
+ Are there some sections you would have liked that weren't here?

## Acknowledgments
Acknowledge all externals. Acknowledge supervisor + archive.

## Contributing

### Contributors
This project exists thanks to these wonderful people:

<a href="https://github.com/larsmhaugland/file-converter/graphs/contributors)https://github.com/larsmhaugland/file-converter/graphs/contributors"><img src="https://contrib.rocks/image?repo=larsmhaugland/file-converter"/></a>

## Licensing
as listed on https://spdx.org/licenses/ 
