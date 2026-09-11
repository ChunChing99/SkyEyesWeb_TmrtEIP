using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;



public class clsExcel
{

    /// <summary>
    /// 將資料清單轉成Excel
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    public System.IO.MemoryStream DoDataListToExcel(clsDtToExcelModel[] models)
    {
        try
        {
            NPOI.XSSF.UserModel.XSSFWorkbook workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();


            #region 樣式
            NPOI.SS.UserModel.IFont Header_Font = workbook.CreateFont();
            Header_Font.FontName = "微軟正黑體";
            Header_Font.FontHeightInPoints = 14;
            Header_Font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;

            NPOI.SS.UserModel.IFont Row_Font = workbook.CreateFont();
            Row_Font.FontName = "微軟正黑體";
            Row_Font.FontHeightInPoints = 12;

            NPOI.SS.UserModel.ICellStyle Header_Style = workbook.CreateCellStyle();
            Header_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Header_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            Header_Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            Header_Style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
            Header_Style.SetFont(Header_Font);

            NPOI.SS.UserModel.ICellStyle Row_Style = workbook.CreateCellStyle();
            Row_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Row_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            Row_Style.SetFont(Row_Font);

            NPOI.SS.UserModel.ICellStyle Node_Style = workbook.CreateCellStyle();
            Node_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Node_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            Node_Style.SetFont(Row_Font);
            #endregion

            foreach (clsDtToExcelModel m in models)
            {

                if (m.arrWidth == null && m.ColKey.Count > 0)
                {
                    m.arrWidth = new ArrayList();
                    foreach (object item in m.ColKey)
                    {
                        m.arrWidth.Add(25);
                    }
                }

                List<Dictionary<string, object>> _dataList = m.dataList;
                ArrayList ColKey = m.ColKey;
                ArrayList ColName = m.ColName;
                ArrayList Note = m.Note;
                ArrayList arrWidth = m.arrWidth;
                ArrayList Merged = m.Merged;
                short _Heigh = m.Height;

                NPOI.SS.UserModel.ISheet sheet;
                if (m.SheetName != null)
                {
                    sheet = workbook.CreateSheet(m.SheetName);
                }
                else
                {
                    sheet = workbook.CreateSheet();
                }

                int NoteCnt = 0;
                if (Note != null)
                {
                    NoteCnt = Note.Count;
                    for (int i = 0; i < Note.Count; i++)
                    {
                        NPOI.SS.UserModel.IRow DataRow = sheet.CreateRow(i);
                        NPOI.SS.UserModel.ICell DataCell = DataRow.CreateCell(0);
                        DataCell.SetCellValue(Note[i].ToString());
                        DataCell.CellStyle = Node_Style;
                    }
                }

                for (int i = -1; i < _dataList.Count; i++)
                {
                    NPOI.SS.UserModel.IRow DataRow = sheet.CreateRow(i + 1 + NoteCnt);
                    DataRow.Height = _Heigh;

                    for (int j = 0; j < ColKey.Count; j++)
                    {
                        NPOI.SS.UserModel.ICell DataCell = DataRow.CreateCell(j);

                        if (i == -1)
                        {
                            DataCell.SetCellValue(ColName[j].ToString());
                            DataCell.CellStyle = Header_Style;
                            if (arrWidth != null)
                            {
                                if (arrWidth[j] != null)
                                {
                                    sheet.SetColumnWidth(j, Convert.ToInt16(arrWidth[j]) * 256);
                                }
                            }
                        }
                        else
                        {
                            object cellValue = _dataList[i][ColKey[j].ToString()];
                            if (cellValue == null)
                            {
                                DataCell.SetCellValue("");
                            }
                            else
                            {
                                string _type = cellValue.GetType().ToString();
                                switch (_type)
                                {
                                    case "System.DateTime":
                                        DataCell.SetCellValue(Convert.ToDateTime(cellValue).ToString("yyyy-MM-dd HH:mm:ss"));
                                        break;

                                    case "System.Int16":
                                    case "System.Int32":
                                    case "System.Int64":
                                    case "System.Single":
                                    case "System.Double":
                                        DataCell.SetCellValue(Convert.ToDouble(cellValue));
                                        break;

                                    default:
                                        DataCell.SetCellValue(cellValue.ToString());
                                        break;
                                }
                            }
                            DataCell.CellStyle = Row_Style;
                        }
                    }
                }

                if (Merged != null)
                {
                    foreach (string item in Merged)
                    {
                        if (item.Split(',').Length == 4)
                        {
                            string[] xy = item.Split(",");
                            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(
                                Convert.ToInt32(xy[0]),
                                Convert.ToInt32(xy[1]),
                                Convert.ToInt32(xy[2]),
                                Convert.ToInt32(xy[3]))
                                );
                        }
                    }
                }
                sheet = null;
            }



            //System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //workbook.Write(ms);
            //ms.Seek(0, SeekOrigin.Begin);
            //ms.Flush();

            string tmpFile = "tmp_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
            FileStream fileStream = new FileStream(Directory.GetCurrentDirectory() + "\\temp\\" + tmpFile, FileMode.Create, FileAccess.Write);
            workbook.Write(fileStream);//調用這個後會關於文件流，在HSSFWorkbook不會關閉所以在處理時應注意
            FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\temp\\" + tmpFile, FileMode.Open, FileAccess.Read);
            long fileSize = fs.Length;
            byte[] fileBuffer = new byte[fileSize];

            fs.Read(fileBuffer, 0, (int)fileSize);
            //HttpContext.Current.Response.BinaryWrite(fileBuffer);

            System.IO.MemoryStream ms = new System.IO.MemoryStream(fileBuffer);
            //ms.Seek(0, SeekOrigin.Begin);
            //ms.Flush();

            fileStream.Close();
            fs.Close();

            if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "\\temp\\/" + tmpFile)) System.IO.File.Delete(Directory.GetCurrentDirectory() + "\\temp\\" + tmpFile);

            return ms;

            //ms.Seek(0, SeekOrigin.Begin);
            //ms.Flush();
            //Context.Response.Clear();
            //Context.Response.AddHeader("Content-Disposition", String.Format("attachment;filename=" + Context.Server.UrlEncode(DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls")));
            //Context.Response.ContentType = "application/vnd.ms-excel";
            //Context.Response.BinaryWrite(ms.GetBuffer());
            //ms.Close();
            //ms.Dispose();
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    /// <summary>
    /// 將資料清單轉成Excel檔案
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    public String DoDataListToFile(clsDtToExcelModel[] models , string setFileName = "")
    {
        try
        {
            NPOI.XSSF.UserModel.XSSFWorkbook workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();


            #region 樣式
            NPOI.SS.UserModel.IFont Header_Font = workbook.CreateFont();
            Header_Font.FontName = "微軟正黑體";
            Header_Font.FontHeightInPoints = 14;
            Header_Font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;

            NPOI.SS.UserModel.IFont Row_Font = workbook.CreateFont();
            Row_Font.FontName = "微軟正黑體";
            Row_Font.FontHeightInPoints = 12;

            NPOI.SS.UserModel.ICellStyle Header_Style = workbook.CreateCellStyle();
            Header_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Header_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            Header_Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            Header_Style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
            Header_Style.SetFont(Header_Font);

            NPOI.SS.UserModel.ICellStyle Row_Style = workbook.CreateCellStyle();
            Row_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Row_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            Row_Style.SetFont(Row_Font);

            NPOI.SS.UserModel.ICellStyle Node_Style = workbook.CreateCellStyle();
            Node_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Node_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            Node_Style.SetFont(Row_Font);

            #endregion

            foreach (clsDtToExcelModel m in models)
            {

                if (m.arrWidth == null && m.ColKey.Count > 0)
                {
                    m.arrWidth = new ArrayList();
                    foreach (object item in m.ColKey)
                    {
                        m.arrWidth.Add(25);
                    }
                }

                List<Dictionary<string, object>> _dataList = m.dataList;
                ArrayList ColKey = m.ColKey;
                ArrayList ColName = m.ColName;
                ArrayList Note = m.Note;
                ArrayList arrWidth = m.arrWidth;
                ArrayList Merged = m.Merged;
                short _Heigh = m.Height;

                NPOI.SS.UserModel.ISheet sheet;
                if (m.SheetName != null)
                {
                    sheet = workbook.CreateSheet(m.SheetName);
                }
                else
                {
                    sheet = workbook.CreateSheet();
                }

                int NoteCnt = 0;
                if (Note != null)
                {
                    NoteCnt = Note.Count;
                    for (int i = 0; i < Note.Count; i++)
                    {
                        NPOI.SS.UserModel.IRow DataRow = sheet.CreateRow(i);
                        NPOI.SS.UserModel.ICell DataCell = DataRow.CreateCell(0);
                        DataCell.SetCellValue(Note[i].ToString());
                        DataCell.CellStyle = Node_Style;
                    }
                }

                for (int i = -1; i < _dataList.Count; i++)
                {
                    NPOI.SS.UserModel.IRow DataRow = sheet.CreateRow(i + 1 + NoteCnt);
                    DataRow.Height = _Heigh;

                    for (int j = 0; j < ColKey.Count; j++)
                    {
                        NPOI.SS.UserModel.ICell DataCell = DataRow.CreateCell(j);

                        if (i == -1)
                        {
                            DataCell.SetCellValue(ColName[j].ToString());
                            DataCell.CellStyle = Header_Style;
                            if (arrWidth != null)
                            {
                                if (arrWidth[j] != null)
                                {
                                    sheet.SetColumnWidth(j, Convert.ToInt16(arrWidth[j]) * 256);
                                }
                            }
                        }
                        else
                        {
                            object cellValue = _dataList[i][ColKey[j].ToString()];
                            if (cellValue == null)
                            {
                                DataCell.SetCellValue("");
                            }
                            else
                            {
                                string _type = cellValue.GetType().ToString();
                                switch (_type)
                                {
                                    case "System.DateTime":
                                        DataCell.SetCellValue(Convert.ToDateTime(cellValue).ToString("yyyy-MM-dd HH:mm:ss"));
                                        break;

                                    case "System.Int16":
                                    case "System.Int32":
                                    case "System.Int64":
                                    case "System.Single":
                                    case "System.Double":
                                        DataCell.SetCellValue(Convert.ToDouble(cellValue));
                                        break;

                                    default:
                                        DataCell.SetCellValue(cellValue.ToString());
                                        break;
                                }
                            }
                            DataCell.CellStyle = Row_Style;
                        }
                    }
                }

                if (Merged != null)
                {
                    foreach (string item in Merged)
                    {
                        if (item.Split(',').Length == 4)
                        {
                            string[] xy = item.Split(",");
                            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(
                                Convert.ToInt32(xy[0]),
                                Convert.ToInt32(xy[1]),
                                Convert.ToInt32(xy[2]),
                                Convert.ToInt32(xy[3]))
                                );
                        }
                    }
                }
                sheet = null;
            }



            //System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //workbook.Write(ms);
            //ms.Seek(0, SeekOrigin.Begin);
            //ms.Flush();

            string tmpFile = "tmp_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
            if(setFileName != "") tmpFile = setFileName + ".xlsx";

            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\temp\\") == false) Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\temp\\");
            FileStream fileStream = new FileStream(Directory.GetCurrentDirectory() + "\\temp\\" + tmpFile, FileMode.Create, FileAccess.Write);
            workbook.Write(fileStream);//調用這個後會關於文件流，在HSSFWorkbook不會關閉所以在處理時應注意
            FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\temp\\" + tmpFile, FileMode.Open, FileAccess.Read);
            long fileSize = fs.Length;
            byte[] fileBuffer = new byte[fileSize];

            fs.Read(fileBuffer, 0, (int)fileSize);
            //HttpContext.Current.Response.BinaryWrite(fileBuffer);

            System.IO.MemoryStream ms = new System.IO.MemoryStream(fileBuffer);
            //ms.Seek(0, SeekOrigin.Begin);
            //ms.Flush();

            fileStream.Close();
            fs.Close();

            if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "\\temp\\" + tmpFile))
            {
                return (Directory.GetCurrentDirectory() + "\\temp\\" + tmpFile);
            }
            else
            {
                return "";
            }

            //ms.Seek(0, SeekOrigin.Begin);
            //ms.Flush();
            //Context.Response.Clear();
            //Context.Response.AddHeader("Content-Disposition", String.Format("attachment;filename=" + Context.Server.UrlEncode(DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls")));
            //Context.Response.ContentType = "application/vnd.ms-excel";
            //Context.Response.BinaryWrite(ms.GetBuffer());
            //ms.Close();
            //ms.Dispose();
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    /// <summary>
    /// 將資料清單轉成Excel
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    public string DoDataListToBase64(clsDtToExcelModel[] models)
    {
        try
        {
            NPOI.XSSF.UserModel.XSSFWorkbook workbook = new NPOI.XSSF.UserModel.XSSFWorkbook();


            #region 樣式
            NPOI.SS.UserModel.IFont Header_Font = workbook.CreateFont();
            Header_Font.FontName = "微軟正黑體";
            Header_Font.FontHeightInPoints = 14;
            Header_Font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;

            NPOI.SS.UserModel.IFont Row_Font = workbook.CreateFont();
            Row_Font.FontName = "微軟正黑體";
            Row_Font.FontHeightInPoints = 12;

            NPOI.SS.UserModel.ICellStyle Header_Style = workbook.CreateCellStyle();
            Header_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Header_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            Header_Style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            Header_Style.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
            Header_Style.SetFont(Header_Font);

            NPOI.SS.UserModel.ICellStyle Row_Style = workbook.CreateCellStyle();
            Row_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Row_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            Row_Style.SetFont(Row_Font);

            NPOI.SS.UserModel.ICellStyle Node_Style = workbook.CreateCellStyle();
            Node_Style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            Node_Style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            Node_Style.SetFont(Row_Font);
            #endregion

            foreach (clsDtToExcelModel m in models)
            {

                if (m.arrWidth == null && m.ColKey.Count > 0)
                {
                    m.arrWidth = new ArrayList();
                    foreach (object item in m.ColKey)
                    {
                        m.arrWidth.Add(25);
                    }
                }

                List<Dictionary<string, object>> _dataList = m.dataList;
                ArrayList ColKey = m.ColKey;
                ArrayList ColName = m.ColName;
                ArrayList Note = m.Note;
                ArrayList arrWidth = m.arrWidth;
                ArrayList Merged = m.Merged;
                short _Heigh = m.Height;

                NPOI.SS.UserModel.ISheet sheet;
                if (m.SheetName != null)
                {
                    sheet = workbook.CreateSheet(m.SheetName);
                }
                else
                {
                    sheet = workbook.CreateSheet();
                }

                int NoteCnt = 0;
                if (Note != null)
                {
                    NoteCnt = Note.Count;
                    for (int i = 0; i < Note.Count; i++)
                    {
                        NPOI.SS.UserModel.IRow DataRow = sheet.CreateRow(i);
                        NPOI.SS.UserModel.ICell DataCell = DataRow.CreateCell(0);
                        DataCell.SetCellValue(Note[i].ToString());
                        DataCell.CellStyle = Node_Style;
                    }
                }

                for (int i = -1; i < _dataList.Count; i++)
                {
                    NPOI.SS.UserModel.IRow DataRow = sheet.CreateRow(i + 1 + NoteCnt);
                    DataRow.Height = _Heigh;

                    for (int j = 0; j < ColKey.Count; j++)
                    {
                        NPOI.SS.UserModel.ICell DataCell = DataRow.CreateCell(j);

                        if (i == -1)
                        {
                            DataCell.SetCellValue(ColName[j].ToString());
                            DataCell.CellStyle = Header_Style;
                            if (arrWidth != null)
                            {
                                if (arrWidth[j] != null)
                                {
                                    sheet.SetColumnWidth(j, Convert.ToInt16(arrWidth[j]) * 256);
                                }
                            }
                        }
                        else
                        {
                            object cellValue = _dataList[i][ColKey[j].ToString()];
                            if (cellValue == null)
                            {
                                DataCell.SetCellValue("");
                            }
                            else
                            {
                                string _type = cellValue.GetType().ToString();
                                switch (_type)
                                {
                                    case "System.DateTime":
                                        DataCell.SetCellValue(Convert.ToDateTime(cellValue).ToString("yyyy-MM-dd HH:mm:ss"));
                                        break;

                                    case "System.Int16":
                                    case "System.Int32":
                                    case "System.Int64":
                                    case "System.Single":
                                    case "System.Double":
                                        DataCell.SetCellValue(Convert.ToDouble(cellValue));
                                        break;

                                    default:
                                        DataCell.SetCellValue(cellValue.ToString());
                                        break;
                                }
                            }
                            DataCell.CellStyle = Row_Style;
                        }
                    }
                }

                if (Merged != null)
                {
                    foreach (string item in Merged)
                    {
                        if (item.Split(',').Length == 4)
                        {
                            string[] xy = item.Split(",");
                            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(
                                Convert.ToInt32(xy[0]),
                                Convert.ToInt32(xy[1]),
                                Convert.ToInt32(xy[2]),
                                Convert.ToInt32(xy[3]))
                                );
                        }
                    }
                }
                sheet = null;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(bytes);
                return base64String;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    public class clsDtToExcelModel
    {
        public List<Dictionary<string, object>> dataList = null;
        public ArrayList ColKey = null;
        public ArrayList ColName = null;
        public ArrayList Note = null;
        public ArrayList arrWidth = null;
        public ArrayList Merged = null;
        public short Height;
        public string SheetName = null;

        public clsDtToExcelModel()
        {
            dataList = new List<Dictionary<string, object>>();
            Height = 15 * 20;
        }

    }


}
