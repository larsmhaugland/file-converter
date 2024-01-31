using System;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Collections.Generic;

public class ConversionManager
{
    List<FileInfo> Files;
    Dictionary<KeyValuePair<string, string>, List<string>> ConversionMap = new Dictionary<KeyValuePair<string, string>, List<string>>();
    List<string> WordPronoms = [
        "x-fmt/329", "fmt/609", "fmt/39", "x-fmt/274",
        "x-fmt/275", "x-fmt/276", "fmt/1688", "fmt/37",
        "fmt/38", "fmt/1282", "fmt/1283", "x-fmt/131",
        "x-fmt/42", "x-fmt/43", "fmt/473", "fmt/40",
        "x-fmt/44", "fmt/523", "fmt/1827", "fmt/412",
        "fmt/754", "x-fmt/393", "x-fmt/394", "fmt/892",
        "fmt/494"
        ];
    List<string> ImagePronoms = [
        "fmt/3", "fmt/4", "fmt/11", "fmt/12",
        "fmt/13", "fmt/935", "fmt/41", "fmt/42",
        "fmt/43", "fmt/44", "x-fmt/398", "x-fmt/390",
        "x-fmt/391", "fmt/645", "fmt/1507", "fmt/112",
        "fmt/367", "fmt/1917", "x-fmt/399", "x-fmt/388",
        "x-fmt/387", "fmt/155", "fmt/353", "fmt/154",
        "fmt/153", "fmt/156", "x-fmt/270", "fmt/115",
        "fmt/118", "fmt/119", "fmt/114", "fmt/116",
        "fmt/117"
    ];
    List<string> HTMLPronoms = [
        "fmt/103", "fmt/96", "fmt/97", "fmt/98",
        "fmt/99", "fmt/100", "fmt/471", "fmt/1132",
        "fmt/102", "fmt/583"
    ];
    List<string> PDFPronoms = [
        "fmt/559", "fmt/560", "fmt/561", "fmt/562",
        "fmt/563", "fmt/564", "fmt/565", "fmt/558",
        "fmt/14", "fmt/15", "fmt/16", "fmt/17",
        "fmt/18", "fmt/19", "fmt/20", "fmt/276",
        "fmt/95", "fmt/354", "fmt/476", "fmt/477",
        "fmt/478", "fmt/479", "fmt/480", "fmt/481",
        "fmt/1910", "fmt/1911", "fmt/1912", "fmt/493",
        "fmt/144", "fmt/145", "fmt/157", "fmt/146",
        "fmt/147", "fmt/158", "fmt/148", "fmt/488",
        "fmt/489", "fmt/490", "fmt/492", "fmt/491",
        "fmt/1129", "fmt/1451"
    ];
    List<string> ExcelPronoms = [
        "fmt/55", "fmt/56", "fmt/57", "fmt/61",
        "fmt/595", "fmt/445", "fmt/214", "fmt/1828",
        "fmt/494", "fmt/62", "fmt/59", "fmt/598"
    ];
    List<string> PPTPronoms = [
        "fmt/1537", "fmt/1866", "fmt/181", "fmt/1867",
        "fmt/179", "fmt/1747", "fmt/1748", "x-fmt/88",
        "fmt/125", "fmt/126", "fmt/487", "fmt/215",
        "fmt/1829", "fmt/494", "fmt/631"
    ];
    List<string> OpenDocPronoms = [
        "fmt/140", "fmt/135", "fmt/136", "fmt/137",
        "fmt/138", "fmt/139", "x-fmt/3", "fmt/1756",
        "fmt/290", "fmt/291", "fmt/1755", "fmt/294",
        "fmt/295", "fmt/1754", "fmt/292", "fmt/293"
    ];

    /// <summary>
    /// initializes the map for how to reach each format
    /// </summary>
    private void initMap()
    {
        foreach (string pronom in WordPronoms)
        {
            foreach (string otherpronom in WordPronoms)
            {
                if (pronom != otherpronom)
                {
                    ConversionMap.Add(new KeyValuePair<string, string>(pronom, otherpronom), [pronom, "fmt/147", otherpronom]); // word to pdf other word
                }
            }
        }
        // TODO: Add more routes
    }
    public ConversionManager()
    {
        initMap();
    }

    void discoverFiles()
    {

    }
    /// <summary>
    /// Sends files to the correct converter
    /// </summary>
    /// <param name="fileInfo"> info about the file </param>
    /// <param name="pronomFrom"> pronom before converting </param>
    /// <param name="pronomTo"> pronom the file wants to be converted to </param>
	public void ConvertFiles(FileInfo fileInfo, string pronomFrom, string pronomTo)
    {
        //Converter converter = new Converter();
        Logger logger = Logger.Instance;

        logger.SetUpRunTimeLogMessage("No converters found for this path", true, pronom: "");
    }
    bool allIsConverted()
    {
        foreach (FileInfo file in Files)
        {
            if (!file.IsConverted)
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    public void ConvertFiles()
    {
        List<FileInfo> WorkingSet = Files;
        do
        {
            List<FileInfo> modWorkingSet = WorkingSet;

            // iText7
            foreach (FileInfo file in modWorkingSet)
            {
                //TODO: Get supported input/ouput pronoms from Conversion tool
            }

            // Remove all files that are converted from WorkingSet
            foreach (FileInfo file in WorkingSet)
            {
                if (file.IsConverted)
                {
                    WorkingSet.Remove(file);
                }
            }
        } while (!allIsConverted());
    }
}
/*
	public void ConvertFiles_Old(FileInfo fileinfo, string pronom)
	{
		Converter converter = new Converter();
		Logger logger = Logger.Instance;
        switch (pronom)
		{
            
            #region imageToPDF
            // GIF
            case "fmt/3":
			case "fmt/4":
            // PNG
            case "fmt/11":
			case "fmt/12":
			case "fmt/13":
			case "fmt/935":
            // JPG/JPEG
            case "fmt/41":
            case "fmt/42":
            case "fmt/43":
            case "fmt/44":
            case "x-fmt/398":
            case "x-fmt/390":
            case "x-fmt/391":
            case "fmt/645":
            case "fmt/1507":
            case "fmt/112":
            case "fmt/367":
            // TIF
            case "fmt/1917":
            case "x-fmt/399":
            case "x-fmt/388":
            case "x-fmt/387":
            case "fmt/155":
            case "fmt/353":
            case "fmt/154":
            case "fmt/153":
            case "fmt/156":
            // BMP
            case "x-fmt/270":
            case "fmt/115":
            case "fmt/118":
            case "fmt/119":
            case "fmt/114":
            case "fmt/116":
            case "fmt/117":
                // TODO: Put image converter here
                break;
                // TODO: Add convertername to fileinfo list
                break;
            #endregion
            #region HTML
            case "fmt/103":
			case "fmt/96":
			case "fmt/97":
			case "fmt/98":
			case "fmt/99":
			case "fmt/100":
			case "fmt/471":
			case "fmt/1132":
			case "fmt/102":
			case "fmt/583":
                // TODO: Put HTML converter here
                // TODO: Add convertername to fileinfo list
                break;
            #endregion
            #region PDF
            case "fmt/559":
            case "fmt/560":
            case "fmt/561":
            case "fmt/562":
            case "fmt/563":
            case "fmt/564":
            case "fmt/565":
            case "fmt/558":
            case "fmt/14":
            case "fmt/15":
            case "fmt/16":
            case "fmt/17":
            case "fmt/18":
            case "fmt/19":
            case "fmt/20":
            case "fmt/276":
            case "fmt/95":
            case "fmt/354":
            case "fmt/476":
            case "fmt/477":
            case "fmt/478":
            case "fmt/479":
            case "fmt/480":
            case "fmt/481":
            case "fmt/1910":
            case "fmt/1911":
            case "fmt/1912":
            case "fmt/493":
            case "fmt/144":
            case "fmt/145":
            case "fmt/157":
            case "fmt/146":
            case "fmt/147":
            case "fmt/158":
            case "fmt/148":
            case "fmt/488":
            case "fmt/489":
            case "fmt/490":
            case "fmt/492":
            case "fmt/491":
            case "fmt/1129":
            case "fmt/1451":
                // converter.PDFConverter
                // TODO: Add convertername to fileinfo list
                break;
            #endregion
            #region Word
            // DOC
            case "x-fmt/329":
            case "fmt/609":
            case "fmt/39":
            case "x-fmt/274":
            case "x-fmt/275":
            case "x-fmt/276":
            case "fmt/1688":
            case "fmt/37":
            case "fmt/38":
            case "fmt/1282":
            case "fmt/1283":
            case "x-fmt/131":
            case "x-fmt/42":
            case "x-fmt/43":
            case "fmt/473":
            case "fmt/40":
            case "x-fmt/44":
            case "fmt/523":
            case "fmt/1827":
            case "fmt/412":
            case "fmt/754":
            case "x-fmt/393":
            case "x-fmt/394":
            case "fmt/892":
            case "fmt/494":
            // DOCX
            */
            /* 
            case "fmt/473":
            case "fmt/1827":
            case "fmt/412":
            case "fmt/494":    
            */
                    // DOCM
                    // case "fmt/523":
                    // DOTX
                    /*
                    case "fmt/597":
                        // TODO: Add word converter here
                        // TODO: Add convertername to fileinfo list
                        break;
                    #endregion
                    #region Excel
                    // XLS
                    case "fmt/55":
                    case "fmt/56":
                    case "fmt/57":
                    case "fmt/61":
                    case "fmt/595":
                    case "fmt/445":
                    case "fmt/214":
                    case "fmt/1828":
                    // case "fmt/494":
                    case "fmt/62":
                    case "fmt/59":

                    // XLSX
                    /*
                    case "fmt/214":
                    case "fmt/1828":
                    case "fmt/494":
                    */
                    // XLSM
                    //case "fmt/445":

                    // XLTX
                    /*
                    case "fmt/598":
                        // TODO: add excel converter here
                        // TODO: Add convertername to fileinfo list
                        break;
                    #endregion
                    #region PowerPoint
                    // PPT
                    case "fmt/1537":
                    case "fmt/1866":
                    case "fmt/181":
                    case "fmt/1867":
                    case "fmt/179":
                    case "fmt/1747":
                    case "fmt/1748":
                    case "x-fmt/88":
                    case "fmt/125":
                    case "fmt/126":
                    case "fmt/487":
                    case "fmt/215":
                    case "fmt/1829":
                    //case "fmt/494":

                    // PPTX
                    /*
                    case "fmt/215":
                    case "fmt/1829":
                    case "fmt/494":
                    */
                    /*
                    // PPTM
                    // case "fmt/487":
                    // POTX
                    case "fmt/631":
                        // TODO: add powerpoint converter here
                        break;
                    #endregion
                    #region Open Document
                    //ODF
                    case "fmt/140":
                    case "fmt/135":
                    case "fmt/136":
                    case "fmt/137":
                    case "fmt/138":
                    case "fmt/139":
                    // ODT
                    case "x-fmt/3":
                    case "fmt/1756":
                    //case "fmt/136":
                    case "fmt/290":
                    case "fmt/291":
                    // ODS
                    case "fmt/1755":
                    //case "fmt/137":
                    case "fmt/294":
                    case "fmt/295":
                    // ODP
                    case "fmt/1754":
                    //case "fmt/138":
                    case "fmt/292":
                    case "fmt/293":
                        // TODO: Add open document converter here
                        break;
                    #endregion
                    #region Rich Text Format
                    case "fmt/969":
                    case "fmt/45":
                    case "fmt/50":
                    case "fmt/52":
                    case "fmt/53":
                    case "fmt/355":
                        // TODO: add RTF converter here
                        break;
                    #endregion
                    #region E-Mail
                    // PST
                    case "x-fmt/248":
                    case "x-fmt/249":
                    // MSG
                    case "x-fmt/430":
                    case "fmt/1144":
                    // EML
                    case "fmt/278":
                    case "fmt/950":
                    // OLM
                    // OLM Region (Not Found)
                        //TODO: Add Email converter here
                    #endregion
                    #region Compressed folder
                    // ZIP 
                    case "x-fmt/263":

                    // TAR 
                    case "x-fmt/265":

                    // 7ZIP
                    case "fmt/484":

                    // GZ
                    case "fmt/266":

                    // RAR
                    case "x-fmt/264":
                    case "fmt/411":
                    case "fmt/613":
                        // Do Nothing
                    #endregion

                    default:
                        logger.SetUpRunTimeLogMessage("Cant convert from that format",true,filetype:pronomFrom); 	
                        break;
                }

            }
}
            */
