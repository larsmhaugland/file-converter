using System;

public class ConversionManager
{
	List<FileInfo> Files;
	//TODO: ConversionMap

	public ConversionManager()
	{

	}

	void discoverFiles()
	{

	}

	void ConvertFiles(string pronom)
	{
		Converter converter = new Converter();
		Logger logger = Logger.Instance;
        switch (pronom)
		{
            #region image
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
                // converter.HTMLConverter();
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
            /* 
            case "fmt/473":
            case "fmt/1827":
            case "fmt/412":
            case "fmt/494":    
            */
            // DOCM
            // case "fmt/523":
            // DOTX
            case "fmt/597":
                // TODO: Add word converter here
                break;
            #endregion
            #region Excel
            #region XLS
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
            #endregion
            /*
            #region XLSX
            case "fmt/214":
            case "fmt/1828":
            case "fmt/494":
            #endregion
            #region XLSM
            case "fmt/445":
            #endregion
            */
            // XLTX Region
            #region XLTX
            case "fmt/598":
                // TODO: add excel converter here
                break;
            #endregion
            #endregion
            #region PowerPoint
            #region PPT
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
            #endregion
            /*
            #region PPTX
            case "fmt/215":
            case "fmt/1829":
            case "fmt/494":
            #endregion
            
            #region PPTM
            case "fmt/487":
            #endregion
            */
            #region POTX
            case "fmt/631":
                // TODO: add powerpoint converter here
                break;
            #endregion
            #endregion
            #region Open Document
            #region ODF
            case "fmt/140":
            case "fmt/135":
            case "fmt/136":
            case "fmt/137":
            case "fmt/138":
            case "fmt/139":
            #endregion
            #region ODT
            case "x-fmt/3":
            case "fmt/1756":
            //case "fmt/136":
            case "fmt/290":
            case "fmt/291":
            #endregion
            #region ODS
            case "fmt/1755":
            //case "fmt/137":
            case "fmt/294":
            case "fmt/295":
            #endregion
            #region ODP
            case "fmt/1754":
            //case "fmt/138":
            case "fmt/292":
            case "fmt/293":
                // TODO: Add open document converter here
                break;
            #endregion
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
            #region PST
            case "x-fmt/248":
            case "x-fmt/249":
            #endregion
            #region MSG
            case "x-fmt/430":
            case "fmt/1144":
            #endregion
            #region EML
            case "fmt/278":
            case "fmt/950":
            #endregion
            #region OLM
            // OLM Region (Not Found)
            #endregion
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
				logger.SetUpRunTimeLogMessage("Cant convert from that format",true,filetype:pronom); 			
				break;
		}
	}
}