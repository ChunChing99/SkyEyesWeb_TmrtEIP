using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// clsResponseRes 的摘要描述
/// </summary>
public class clsResult : System.Collections.Hashtable
{
    public clsResult()
    {
        this.SetMsgNumber = MsgNumber.N000;
        //this.Add("Result", true);
        //this.Add("ReturnMsg", "");
        //this.Add("ReturnNumber", MsgNumber.N000 );
    }

    public MsgNumber SetMsgNumber
    {
        set
        {
            this["ReturnNumber"] = ((MsgNumber)value).ToString();
            this["Result"] = false;
            switch (value)
            {
                case MsgNumber.N999:
                    break;
                case MsgNumber.N000:
                    this["Result"] = true;
                    this["ReturnMsg"] = "正常";
                    break;
                case MsgNumber.N001:
                    this["ReturnMsg"] = "驗證碼錯誤";
                    break;
                case MsgNumber.N002:
                    this["ReturnMsg"] = "登入失敗";
                    break;
                case MsgNumber.N003:
                    this["ReturnMsg"] = "未登入或登入逾時";
                    break;
                case MsgNumber.N004:
                    this["ReturnMsg"] = "無權限進行操作";
                    break;
                case MsgNumber.N005:
                    this["ReturnMsg"] = "無權限進行此頁";
                    break;
                case MsgNumber.N006:
                    this["ReturnMsg"] = "已在出勤狀態，無法重覆出勤";
                    break;
                case MsgNumber.N007:
                    this["ReturnMsg"] = "尚未出勤，無法結束出勤";
                    break;
                case MsgNumber.N099:
                    this["ReturnMsg"] = "無效或逾時Token";
                    break;
            }
        }
    }

    public void ThrowError(MsgNumber SetMsgNumber) {
        this.SetMsgNumber = SetMsgNumber;
        throw new Exception(this["ReturnMsg"].ToString());
    }

    public enum MsgNumber
    {
        /// <summary>
        /// 其他
        /// </summary>
        N999,
        /// <summary>
        /// 正常
        /// </summary>
        N000,
        /// <summary>
        /// 驗證碼錯誤
        /// </summary>
        N001,
        /// <summary>
        /// 登入失敗
        /// </summary>
        N002,
        /// <summary>
        /// 未登入或登入逾時
        /// </summary>
        N003,
        /// <summary>
        /// 無權限進行操作
        /// </summary>
        N004,
        /// <summary>
        /// 無權限進行此頁
        /// </summary>
        N005,
        /// <summary>
        /// 已在出勤狀態，無法重覆出勤
        /// </summary>
        N006,
        /// <summary>
        /// 尚未出勤，無法結束出勤
        /// </summary>
        N007,
        /// <summary>
        /// 無效或逾時Token
        /// </summary>
        N099
    }
}