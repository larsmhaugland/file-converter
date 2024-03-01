# file-converter
>NOTE: This README is currently being updated, information found here might not reflect the current application.

INSERT WORKFLOW BADGE HERE AT SOME POINT
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
  - [GUI](#gui)
  - [CLI](#cli)
  - [Settings](#settings)
  - [Currently supported file formats](#currently-supported-file-formats)
  - [Adding a new converter](#adding-a-new-converter)
- [Acknowledgments](#acknowledgments)
- [Contributing](#contributing)
- [Licensing](#licensing)


## Background 
This project is part of a collaboration with [Innlandet County Archive](https://www.visarkiv.no/) and is a Bachelor's thesis project for a [Bachelor's in Programming](https://www.ntnu.edu/studies/bprog) at the [Norwegian University of Technology and Science (NTNU)](https://www.ntnu.edu/).

In Norway, the act of archiving is regulated by the Archives Act, which states that public bodies have a duty to register and preserve documents that are created as part of their activity [^1]. As society is becoming more digitized so is information, and the documents that were previously physical and stored physically are now digital and stored digitally. Innlandet County Archive is an inter-municipal archive cooperation, that registers and preserves documents from 48 municipalities. However, not all file types they receive are suitable for archiving as they run a risk of becoming obsolete. (For further reading see: [Obsolescence: File Formats and Software](https://dpworkshop.org/dpm-eng/oldmedia/obsolescence1.html)) Innlandet County Archive wished to streamline its conversion process into one application that could deal with a vast array of file formats. Furthermore, archiving is based on the principles of accessibility, accountability and integrity, which is why this application also provides documentation of all changes made to files.

Much like programmers and software developers, archivists believe in an open source world. Therefore it would only be right for this program to be open source. 

[^1]: Kultur- og likestillingsdepartementet. *Lov om arkiv [arkivlova].* URL: https://lovdata.no/dokument/NL/lov/1992-12-04-126?q=arkivloven (visited on 17th Jan. 2024).

## Install
To download the application source code simply:
 ```sh
  git clone --recursive https://github.com/larsmhaugland/file-converter.git
```


>NOTE: Cloning **with** the Git submodules is required for the application to work.
>If you did not clone the repository recursively or do not see the git submodules in your local repo we would suggest:
> ```sh
>   git submodule init
>   git submodule update
>```

### Dependencies
#### External libraries/software used
**Libraries**
- [iText7](https://github.com/itext/itext-dotnet)
- [BouncyCastle](https://www.bouncycastle.org/)

**Software**
- [GhostScript](https://www.ghostscript.com/index.html)
- [LibreOffice](https://www.libreoffice.org/)

### Installation for Windows
### Installation for Linux

## Usage
Common usage (code block)

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
1. Global settings stating that file format ```x```should be converted to file format ```y```.
2. Folder settings stating that file format ```x```should be converted to file format ```y``` in the specific folder ```folder```.

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

### Adding a new converter

## Acknowledgments
Acknowledge all externals. Acknowledge supervisor + archive.

## Contributing

### Contributors
<a href="https://github.com/larsmhaugland/file-converter/graphs/contributors)https://github.com/larsmhaugland/file-converter/graphs/contributors"><img src="https://contrib.rocks/image?repo=larsmhaugland/file-converter"/></a>

## Licensing
as listed on https://spdx.org/licenses/ 
