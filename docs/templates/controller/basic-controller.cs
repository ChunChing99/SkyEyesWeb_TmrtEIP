using Microsoft.AspNetCore.Mvc;
using TokenModels;

namespace SkyEyesWeb_TmrtEIP.Server.Controllers.Sample;

[ApiController]
[Route("api/[controller]/[action]")]
public class BasicController : clsApiBase
{

    /// <summary>
    /// Web Controller 範例 - 查詢資料
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<clsResult> GetData(clsPostData PostData)
    {

        clsResult Res = base.CmpSignStatus(WsMethodName, FunctionType.Search);

        try
        {
            if (!(bool)Res["Result"]) throw new Exception(Res["ReturnMsg"].ToString());

            System.Collections.Hashtable HT = PostData.ToHashtable();
            int PageRowCount = HT["PageRowCount"] != null ? Convert.ToInt32(HT["PageRowCount"].ToString()) : 10;
            int NowPageNum = HT["NowPageNum"] != null ? Convert.ToInt32(HT["NowPageNum"].ToString()) : 1;
            string keyword = HT["keyword"] != null ? HT["keyword"].ToString() : "";

            string strSql = @"
                Select *
                From YourTable
                Where 1=1
                And (@keyword='' OR YourField like @keyword)
            ";

            clsPostgreDB.PaginationInfo PagInfo = clsPostgreDB.getListPagination(
                strSql,
                new
                {
                    keyword = keyword == "" ? "" : "%" + keyword + "%",
                },
                PageRowCount,
                NowPageNum,
                "YourSortField",
                clsPostgreDB.enumOrder.DESC
            );

            Res["Data"] = PagInfo.Data;
            Res["DataCount"] = PagInfo.DataCount;
            Res["PageCount"] = PagInfo.PageCount;

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
    /// Web Controller 範例 - 新增資料
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<clsResult> DoInsert(clsPostData PostData)
    {

        clsResult Res = base.CmpSignStatus(WsMethodName, FunctionType.Insert);

        try
        {
            if (!(bool)Res["Result"]) throw new Exception(Res["ReturnMsg"].ToString());

            User user = GetUser;

            System.Collections.Hashtable HT = PostData.ToHashtable();
            string Id = HT["Id"].ToString();
            string Name = HT["Name"].ToString();
            bool Enabled = HT["Enabled"] != null ? Convert.ToBoolean(HT["Enabled"]) : true;

            var dt = clsPostgreDB.getList(
                @"
                    Select *
                    From YourTable
                    Where Id=@Id;
                ",
                new
                {
                    Id = Id
                }
            );

            if (dt.Count > 0)
            {
                throw new Exception("資料已存在");
            }

            clsPostgreDB.execNonQuery(
                @"
                    Insert Into YourTable
                    (Id, Name, Enabled, insert_user)
                    Values
                    (@Id, @Name, @Enabled, @insert_user);
                ",
                new
                {
                    Id = Id,
                    Name = Name,
                    Enabled = Enabled,
                    insert_user = user.User_Name,
                }
            );

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
    /// Web Controller 範例 - 更新資料
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<clsResult> DoUpdate(clsPostData PostData)
    {

        clsResult Res = base.CmpSignStatus(WsMethodName, FunctionType.Update);

        try
        {
            if (!(bool)Res["Result"]) throw new Exception(Res["ReturnMsg"].ToString());

            User user = GetUser;

            System.Collections.Hashtable HT = PostData.ToHashtable();
            string Id = HT["Id"].ToString();
            string Name = HT["Name"].ToString();
            bool Enabled = HT["Enabled"] != null ? Convert.ToBoolean(HT["Enabled"]) : true;

            clsPostgreDB.execNonQuery(
                @"
                    Update YourTable
                    Set Name=@Name
                    , Enabled=@Enabled
                    , update_dt=now()
                    , update_user=@update_user
                    Where Id=@Id;
                ",
                new
                {
                    Id = Id,
                    Name = Name,
                    Enabled = Enabled,
                    update_user = user.User_Name,
                }
            );

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
    /// App Controller 範例 - 需 App Token
    /// </summary>
    /// <param name="PostData"></param>
    /// <returns></returns>
    [HttpGet]
    [HttpPost]
    public async Task<clsResult> AppSample(clsPostData PostData)
    {

        clsResult Res = CmpAppSignStatus();

        try
        {
            if (!(bool)Res["Result"]) throw new Exception(Res["ReturnMsg"].ToString());

            System.Collections.Hashtable HT = PostData.ToHashtable();
            string DemoValue = HT["DemoValue"] != null ? HT["DemoValue"].ToString() : "";

            Res["data"] = new
            {
                DemoValue = DemoValue,
                Message = "App controller template"
            };

        }
        catch (Exception ex)
        {
            if ((bool)Res["Result"]) Res.SetMsgNumber = clsResult.MsgNumber.N999;
            Res["ReturnMsg"] = ex.Message.ToString() != "" ? ex.Message.ToString() : ex.ToString();
        }

        _ = DoAppRecLog(WsMethodName, PostData.ToJson(), Res);
        return Res;
    }

}
