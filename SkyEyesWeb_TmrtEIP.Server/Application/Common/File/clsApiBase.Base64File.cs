using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TokenModels;

public partial class clsApiBase
{

    #region Basic

    /// <summary>
    /// Base64轉存檔案
    /// </summary>
    /// <param name="strBase64">Base64 字串</param>
    /// <param name="SaveDirectory">存檔資料夾</param>
    /// <param name="exFileName">副檔名</param>
    /// <param name="setFileName">自訂檔名</param>
    /// <returns>檔案名稱  </returns>
    public static string DoSaveBase64ToFile(string strBase64, string exFileName, string SaveDirectory = "", string setFileName = null)
    {

        try
        {
            string SavePath = Directory.GetCurrentDirectory() + "/Upload/";
            if (SaveDirectory != "")
            {
                SavePath += SaveDirectory + "/";
                if (Directory.Exists(SavePath) == false) Directory.CreateDirectory(SavePath);
            }

            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "." + exFileName;
            if (setFileName != null) FileName = setFileName + "." + exFileName;
            string FilePath = SavePath + FileName;
            byte[] buffer = Convert.FromBase64String(strBase64);
            MemoryStream ms = new MemoryStream(buffer);
            FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            ms.WriteTo(file);
            file.Close();
            ms.Close();
            return FileName;
        }
        catch (Exception ex)
        {
            return "";
        }


    }

    #endregion

    #region Do

    /// <summary>
    /// 上傳頭像
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> DoUploadHd(clsPostData PostData)
    {

        clsResult Res = CmpSignStatus(WsMethodName, FunctionType.Null);

        try
        {
            User user = GetUser;

            System.Collections.Hashtable HT = PostData.ToHashtable();
            string base64Hd = HT["base64Hd"].ToString();

            string fileType = base64Hd.Split(',')[0];
            string fileBase64 = base64Hd.Split(',')[1];

            // 驗證 MIME 類型是否為合法的圖檔類型
            string extension = fileType switch
            {
                "data:image/jpeg;base64" => "jpg",
                "data:image/png;base64" => "png",
                "data:image/gif;base64" => "gif",
                "data:image/bmp;base64" => "bmp",
                "data:image/webp;base64" => "webp",
                _ => throw new Exception("不支援的圖檔格式")
            };

            string strFileName = DoSaveBase64ToFile(fileBase64, "jpg", "/HeadImg", user.User_No);
            if (strFileName != "")
            {
                Res.Add("HeadImg", GetUserHeadimg(user.User_No));
            }
            else
            {
                throw new Exception("上傳失敗");
            }
        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
        return Res;
    }

    #endregion

    #region Get

    [NonAction]
    public string GetUserHeadimg(string User_No)
    {
        string res = "";

        try
        {
            string SavePath = Directory.GetCurrentDirectory() + $"/Upload/HeadImg/{User_No}.jpg";

            if (System.IO.File.Exists(SavePath))
            {
                res = GetFileBase64(SavePath);
            }
            else
            {

                string str = User_No.Substring(0, 1).ToUpper();

                // 將數字字串轉換為圖像
                Bitmap bmp = new Bitmap(64, 64); // 設定圖像大小
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // 設定背景顏色
                    g.Clear(Color.FromArgb(255, 255, 255));

                    // 設定文字樣式和間隔
                    using (System.Drawing.Font font = new System.Drawing.Font("Arial", 48, FontStyle.Bold))
                    {
                        g.DrawString(str.ToString(), font, new SolidBrush(Color.FromArgb(51, 51, 51)), new PointF(3, 3));
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    byte[] imageBytes = ms.ToArray();
                    string strBase = Convert.ToBase64String(imageBytes);
                    DoSaveBase64ToFile(strBase, "jpg", "/HeadImg", User_No);
                    res = strBase;
                }


            }

        }
        catch (Exception ex)
        {
            res = "";
        }

        return res;
    }

    /// <summary>
    /// 將檔案轉成Base64
    /// </summary>
    /// <param name="FilePath"></param>
    /// <returns></returns>
    public static string GetFileBase64(string FilePath)
    {
        string res = "";
        try
        {
            if (FilePath.IndexOf(Directory.GetCurrentDirectory()) == -1) FilePath = Directory.GetCurrentDirectory() + "/wwwroot/" + FilePath;

            if (System.IO.File.Exists(FilePath))
            {
                FileStream fs = new FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                long fileSize = fs.Length;
                byte[] fileBuffer = new byte[fileSize];

                fs.Read(fileBuffer, 0, (int)fileSize);
                System.IO.MemoryStream ms = new System.IO.MemoryStream(fileBuffer);
                ms.Seek(0, SeekOrigin.Begin);
                Byte[] FileData = new byte[ms.Length];
                ms.Read(FileData, 0, FileData.Length);
                string strJson = System.Convert.ToBase64String(FileData);
                res = strJson;
                fs.Close();
            }

        }
        catch (Exception ex)
        {
        }
        return res;
    }

    #endregion

}
