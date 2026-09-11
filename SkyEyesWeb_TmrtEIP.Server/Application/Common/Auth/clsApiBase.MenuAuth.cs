using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TokenModels;
using static clsApiBase.clsMenu;

public partial class clsApiBase
{

    #region Basic

    /// <summary>
    /// 取得使用者資料
    /// </summary>
    public User GetUser
    {
        get {
            TokenManager TM = new TokenManager();
            User user = TM.GetUser(Request.Headers["Authorization"].ToString());
            return user;
        }
    }

    /// <summary>
    /// 取得 Menu
    /// </summary>
    public List<Dictionary<string, object>> Menu
    {
        get {
            TokenManager TM = new TokenManager();
            User user = TM.GetUser(Request.Headers["Authorization"]);
            if (user == null) return null;

            var dt = clsPostgreDB.getList(
                @"
                    Select *
                    From SYS_tbMenu
                ",
                new { }
                );

            return dt;
        }
    }

    #endregion

    #region Do

    /// <summary>
    /// 登出
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> DoSignOut(clsPostData PostData)
    {
        clsResult Res = new clsResult();

        try
        {
            Res.SetMsgNumber = clsResult.MsgNumber.N000;

            if (Request.Headers["Authorization"].ToString() != "")
            {

                User user = GetUser;

                if (user != null)
                {

                    clsPostgreDB.execNonQuery(
                        @"
                        Update SYS_ttToken Set SignOut=now() Where TokenTime=@TokenTime;
                        ",
                        new { TokenTime = user.datetime }
                    );

                }

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

    /// <summary>
    /// 取得路由資料
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> GetRouterData(clsPostData PostData)
    {
        clsResult Res = new clsResult();

        try
        {

            TokenManager TM = new TokenManager();
            List<clsMenu.MenuItem> oMenu = new List<clsMenu.MenuItem> { };

            var dt = clsPostgreDB.getList(
                @"
                    Select *
                    From SYS_tbMenu
                ",
                new { }
            );


            foreach (var row in dt)
            {
                if (row["ParentId"].ToString() == "0")
                {
                    oMenu.Add(new clsMenu.MenuItem()
                    {
                        ParentId = row["ParentId"].ToString(),
                        Id = row["Id"].ToString(),
                        Name = row["Name"].ToString(),
                        Path = row["Path"] != null ? row["Path"].ToString() : null,
                        Type = row["Type"] != null ? row["Type"].ToString() : null,
                        target = row["NewWindow"].ToString() == "Y" ? "_blank" : "_self",
                        Icon = row["Icon"].ToString(),
                        Enabled = Convert.ToBoolean(row["Enabled"]),
                        subItems = new List<clsMenu.SubItem> { }
                    });
                }

                if (row["Type"].ToString() == "app" && row["ParentId"].ToString() != "0" )
                {
                    var menuItem = oMenu.FirstOrDefault(item => item.Id == row["ParentId"].ToString());
                    if (menuItem != null)
                    {

                        menuItem.subItems.Add(
                            new SubItem
                            {
                                Name = row["Name"].ToString(),
                                Path = row["Path"] != null ? row["Path"].ToString() : null,
                                target = row["NewWindow"].ToString() == "Y" ? "_blank" : "_self",
                                Icon = row["Icon"].ToString(),
                                Enabled = Convert.ToBoolean(row["Enabled"]),
                                Global = row["Global"].ToString() == "Y" ? true : false,
                                Auth = row["Auth"].ToString() == "Y" ? true : false,
                            }
                        );

                    }
                }

            }

            foreach (var folder in oMenu) {
                if (folder.Enabled && folder.ParentId != "0") {
                    bool enable = false;
                    foreach (var app in folder.subItems)
                    {
                        if (app.Enabled)
                        {
                            enable = true;
                            break;
                        }
                    }
                    folder.Enabled = enable;
                }
            }

            Res.Add("Menu", oMenu);

        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
        return Res;
    }

    /// <summary>
    /// 取得選單資料
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> GetMenuData(clsPostData PostData)
    {
        clsResult Res = new clsResult();

        try
        {

            TokenManager TM = new TokenManager();
            List<clsMenu.MenuItem> oMenu = new List<clsMenu.MenuItem> { };

            var dt = Menu;

            foreach (var row in dt)
            {
                if (row["ParentId"].ToString() == "0")
                {
                    oMenu.Add(new clsMenu.MenuItem()
                    {
                        ParentId = row["ParentId"].ToString(),
                        Id = row["Id"].ToString(),
                        Name = row["Name"].ToString(),
                        Path = row["Path"] != null ? row["Path"].ToString() : null,
                        Type = row["Type"] != null ? row["Type"].ToString() : null,
                        target = row["NewWindow"].ToString() == "Y" ? "_blank" : "_self",
                        Icon = row["Icon"].ToString(),
                        Enabled = Convert.ToBoolean(row["Enabled"]),
                        subItems = new List<clsMenu.SubItem> { }
                    });
                }

                if (row["Type"].ToString() == "app" && row["ParentId"].ToString() != "0" && row["Visible"].ToString() == "Y")
                {
                    var menuItem = oMenu.FirstOrDefault(item => item.Id == row["ParentId"].ToString());
                    if (menuItem != null)
                    {

                        menuItem.subItems.Add(
                            new SubItem
                            {
                                Name = row["Name"].ToString(),
                                Path = row["Path"] != null ? row["Path"].ToString() : null,
                                target = row["NewWindow"].ToString() == "Y" ? "_blank" : "_self",
                                Icon = row["Icon"].ToString(),
                                Enabled = Convert.ToBoolean(row["Enabled"]),
                                Visible = row["Visible"].ToString() == "Y" ? true : false,
                                Global = row["Global"].ToString() == "Y" ? true : false,
                                Auth = row["Auth"].ToString() == "Y" ? true : false,
                            }
                        );

                    }
                }

            }

            foreach (var folder in oMenu) {
                if (folder.Type== "folder") {
                    bool enable = false;
                    foreach (var app in folder.subItems)
                    {
                        if (app.Enabled && app.Visible)
                        {
                            enable = true;
                            break;
                        }
                    }
                    folder.Enabled = enable;
                }
            }

            Res.Add("Menu", oMenu);

        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
        return Res;
    }

    /// <summary>
    /// 取得首頁
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    [NonAction]
    public List<Dictionary<string, object>> GetHomePage(List<Dictionary<string, object>> menu = null)
    {


        try
        {

            TokenManager TM = new TokenManager();
            User user = TM.GetUser(Request.Headers["Authorization"]);
            if (user == null) return null;

            var dt = menu != null ? menu : Menu;

            // 直接取得第一筆 Type == "app" 的資料
            var firstApp = dt?.FirstOrDefault(item => item["Type"]?.ToString() == "app" && Convert.ToBoolean(item["Enabled"]) == true);

            return firstApp != null ? new List<Dictionary<string, object>> { firstApp } : new List<Dictionary<string, object>>();

        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    /// <summary>
    /// 取得首頁
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> GetHome(clsPostData PostData)
    {
        clsResult Res = new clsResult();

        try
        {

            System.Collections.Hashtable HT = PostData.ToHashtable();
            string LastPath = HT["LastPath"] != null ? HT["LastPath"].ToString() : "";

            #region 檢查LastPath是否存在且有權限
            #endregion

            User user = GetUser;
            if (user == null) Res.ThrowError(clsResult.MsgNumber.N003);

            var home = GetHomePage();

            Res.Add("Home", home);

        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
        return Res;
    }

    /// <summary>
    /// 取得新的Token
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> GetToken(clsPostData PostData)
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
                            From SYS_ttToken
                            Where SignOut is not null
                            And TokenTime=@TokenTime;
                        ",
                        new
                        {
                            TokenTime = user.datetime
                        }
                );

                if (dt.Count > 0)
                {
                    Res.SetMsgNumber = clsResult.MsgNumber.N003;
                    _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
                    return Res;
                }


                //註銷Token
                clsPostgreDB.execNonQuery(
                    @"
                        Update SYS_ttToken
                        Set SignOut=now()
                        Where TokenTime=@TokenTime;
                    ",
                    new
                    {
                        TokenTime = user.datetime
                    }
                );

                user.datetime = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                TokenManager TM = new TokenManager();
                var token = TM.Create(user);
                Res.Add("Token", token.access_token);

                //記錄Token
                clsPostgreDB.execNonQuery(
                    @"
                        Insert Into SYS_ttToken (TokenTime, Token)
                        values (@TokenTime, @Token);
                    ",
                    new
                    {
                        TokenTime = user.datetime,
                        Token = token.access_token
                    }
                );

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

    #region Cmp

    /// <summary>
    /// 驗證登入狀態與權限
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> CmpSignStatus(clsPostData PostData)
    {
        clsResult Res = new clsResult();

        try
        {
            Res.SetMsgNumber = clsResult.MsgNumber.N000;

            User user = GetUser;

            if (user == null)
            {
                Res.ThrowError(clsResult.MsgNumber.N003);
            }

            #region token是否被登出失效
            var dt = clsPostgreDB.getList(
                    @"
                        Select *
                        From SYS_ttToken
                        Where SignOut is not null And TokenTime=@TokenTime;
                    ",
                    new
                    {
                        TokenTime = user.datetime
                    }
            );
            if (dt.Count > 0)
            {
                Res.ThrowError(clsResult.MsgNumber.N003);
            }
            #endregion


            #region 頁面權限
            #endregion

            Res.Add("user", user);
            Res.Add("HeadImg", GetUserHeadimg(user.User_No));
            Res.Add("Token_Time", user.datetime);
        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
        return Res;

    }

    /// <summary>
    /// 驗證登入狀態與權限
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<clsResult> CmpAuth(clsPostData PostData)
    {
        clsResult Res = new clsResult();

        try
        {
            Res.SetMsgNumber = clsResult.MsgNumber.N000;

            System.Collections.Hashtable HT = PostData.ToHashtable();
            string path = HT["path"].ToString();

            User user = GetUser;

            if (user == null)
            {
                Res.ThrowError(clsResult.MsgNumber.N003);
            }

            #region token是否被登出失效
            var dt = clsPostgreDB.getList(
                    @"
                        Select *
                        From SYS_ttToken
                        Where SignOut is not null And TokenTime=@TokenTime;
                    ",
                    new
                    {
                        TokenTime = user.datetime
                    }
            );
            if (dt.Count > 0)
            {
                Res.ThrowError(clsResult.MsgNumber.N003);
            }
            #endregion

            #region 頁面權限
            #endregion

            Res.Add("user", user);
            Res.Add("HeadImg", GetUserHeadimg(user.User_No));
            Res.Add("Token_Time", user.datetime);
            Res.Add("TokenLimit", clsConfig.TokenLimit);
        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
        return Res;

    }

    /// <summary>
    /// 檢查登入狀態和頁面權限
    /// </summary>
    /// <param name="api"></param>
    /// <param name="FunctionType"></param>
    /// <returns></returns>
    [NonAction]
    public clsResult CmpSignStatus(string api, FunctionType FunctionType = FunctionType.Null)
    {
        clsResult Res = new clsResult();

        User user = GetUser;

        try
        {
            if (user == null)
            {
                Res.SetMsgNumber = clsResult.MsgNumber.N003;
                return Res;
            }

            string WebPage = Request.Headers["Page"].ToString();
            string[] arrApi = api.Split('.');
            string ApiPath = "/" + arrApi[arrApi.Length - 2].Replace("Controller", "").Replace("_", "/");
            
            #region 權限檢查
            #endregion

        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        return Res;
    }

    /// <summary>
    /// 檢查APP登入狀態
    /// </summary>
    /// <param name="api"></param>
    /// <param name="FunctionType"></param>
    /// <returns></returns>
    [NonAction]
    public clsResult CmpAppSignStatus()
    {
        clsResult Res = new clsResult();

        User user = GetUser;

        try
        {
            if (user == null || user.SystemSource != "App")
            {
                Res.SetMsgNumber = clsResult.MsgNumber.N003;
                return Res;
            }

        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        return Res;
    }

    #endregion

    /// <summary>
    /// 權限類型
    /// </summary>
    public enum FunctionType
    {
        /// <summary>
        /// Null
        /// </summary>
        Null,
        /// <summary>
        /// 搜尋
        /// </summary>
        Search,
        /// <summary>
        /// 新增
        /// </summary>
        Insert,
        /// <summary>
        /// 修改
        /// </summary>
        Update,
        /// <summary>
        /// 刪除
        /// </summary>
        Delete,
        /// <summary>
        /// 下載
        /// </summary>
        Download
    }

    public class clsMenu
    {
        public class SubItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string target { get; set; }
            public string Icon { get; set; }
            public bool Enabled { get; set; }
            public bool Visible { get; set; }
            public bool Global { get; set; }
            public bool Auth { get; set; }
        }

        public class MenuItem
        {
            public string ParentId { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public string Type { get; set; }
            public string target { get; set; }
            public string Icon { get; set; }
            public bool Enabled { get; set; }
            public List<SubItem> subItems { get; set; }
        }

    }

}
