using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TokenModels;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;


public partial class clsApiBase : Microsoft.AspNetCore.Mvc.ControllerBase
{

    #region Basic

    /// <summary>
    /// 目前使用Method名稱
    /// </summary>
    public static string WsMethodName
    {
        get
        {
            System.Diagnostics.StackTrace ss = new System.Diagnostics.StackTrace(true);
            System.Reflection.MethodBase mb = ss.GetFrame(1).GetMethod();
            string strRes = mb.ReflectedType.ToString() + "." + mb.Name;

            // 判斷並轉換字串格式
            if (strRes.Contains("+") )
            {
                string[] s = strRes.Split("+");

                var match = System.Text.RegularExpressions.Regex.Match(s[1], @"\<(.+?)\>");
                if (match.Success)
                {
                    string methodName = match.Groups[1].Value;
                    s[1] = methodName;
                }

                strRes = s[0] + "." + s[1];
            }

            return strRes;
        }
    }

    #endregion

    #region Do

    /// <summary>
    /// 寫入Log
    /// </summary>
    /// <param name="api">api名稱</param>
    /// <param name="User_No">使用者</param>
    /// <param name="PostData">表單值</param>
    /// <param name="Res">結果</param>
    [NonAction]
    public async Task DoAppRecLog(string api, string PostData, System.Collections.Hashtable Res, bool isLogFile = true)
    {

        try
        {
            if (isLogFile)
            {

                string LogPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "log",
                DateTime.Now.ToString("yyyyMMdd"),
                api
                );
                if (Directory.Exists(LogPath) == false) Directory.CreateDirectory(LogPath);
                string filename = LogPath + "/" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".log";
                using (StreamWriter sw = new StreamWriter(filename, true))
                {
                    sw.WriteLine(string.Format("[{0}]   {1}"
                        , "PostData"
                        , PostData)
                    );
                    sw.WriteLine(string.Format("[{0}]   {1}"
                        , "Result"
                        , JsonConvert.SerializeObject(Res))
                    );
                    sw.Flush();
                    sw.Dispose();
                    sw.Close();
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        try
        {

            string Token = "";

            if (Request.Headers["Authorization"].ToString() != null && Request.Headers["Authorization"].ToString() != "")
            {
                Token = Request.Headers["Authorization"];
            }

            string User_No = "";
            User user = GetUser;
            if (user != null) User_No = user.User_No;

            string WebPage = Request.Headers["Page"].ToString();

            clsPostgreDB.execNonQuery(
                @"
                Insert Into SYS_ttLog (User_No,WebPage,Api,PostData,Token,ReturnNumber,ReturnMsg)
                Values (@User_No,@WebPage,@Api,@PostData,@Token,@ReturnNumber,@ReturnMsg);
                ",
                new
                {
                    User_No,
                    WebPage,
                    Api = api,
                    PostData = PostData != null ? PostData : "",
                    Res = JsonConvert.SerializeObject(Res),
                    Token,
                    ReturnNumber = Res["ReturnNumber"].ToString(),
                    ReturnMsg = Res["ReturnMsg"].ToString()
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    #endregion

    #region Get

    /// <summary>
    /// 取得密碼驗證規則
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> GetPasswordValidationRules(clsPostData PostData)
    {
        clsResult Res = new clsResult();

        try
        {
            User user = GetUser;

            if (user != null)
            {

                //token是否被登出失效
                var dt = clsPostgreDB.getList(
                        @"
                            Select *
                            From SYS_tbPasswordValidationRules
                            Where IsActive=true
                            LIMIT 1;
                        ",
                        new
                        {
                        }
                );

                Res["data"] = dt;

            }
            else
            {

                Res.SetMsgNumber = clsResult.MsgNumber.N003;

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

    /// <summary>
    /// API 接收的 PostData 物件，會自動將傳入的 JSON 字串轉換成 Hashtable
    /// </summary>
    public class clsPostData
    {
        public object PostData { get; set; }
        private  string strJson { get; set; }

        public System.Collections.Hashtable ToHashtable()
        {
            System.Collections.Hashtable res = null;

            try
            {

                string jsonString = PostData.ToString();

                if (PostData.GetType() == typeof(System.Text.Json.JsonElement))
                {

                    if (res == null && (jsonString.ToString().IndexOf("%7B") > -1 || jsonString.ToString().IndexOf("{") > -1))
                    {
                        strJson = Uri.UnescapeDataString(jsonString.ToString());
                        res = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Hashtable>(strJson);
                    }

                    if (res == null)
                    {
                        strJson = Uri.UnescapeDataString(System.Text.Encoding.Default.GetString(Convert.FromBase64String(jsonString.ToString())));
                        res = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Hashtable>(strJson);

                    }

                }

            }
            catch
            {
            }

            return res;

        }

        public string ToJson()
        {
            return strJson;
        }

    }

}

