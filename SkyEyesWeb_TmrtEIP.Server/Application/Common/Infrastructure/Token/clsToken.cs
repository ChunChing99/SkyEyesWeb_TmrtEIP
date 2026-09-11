using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TokenModels
{
    public static class TokenCrypto
    {
        //產生 HMACSHA256 雜湊
        public static string ComputeHMACSHA256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmacSHA = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hash = hmacSHA.ComputeHash(dataBytes, 0, dataBytes.Length);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        //AES 加密
        public static string AESEncrypt(string data, string key, string iv)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var ivBytes = Encoding.UTF8.GetBytes(iv);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using (var aes = new AesManaged())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CFB; // 使用 CFB 模式來模擬 CTR
                aes.Padding = PaddingMode.None; // CTR 模式不需要填充
                var encryptor = aes.CreateEncryptor();
                var encrypt = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                return Convert.ToBase64String(encrypt);
            }
        }

        //AES 解密
        public static string AESDecrypt(string data, string key, string iv)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var ivBytes = Encoding.UTF8.GetBytes(iv);
            var dataBytes = Convert.FromBase64String(data);
            using (var aes = new AesManaged())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CFB; // 使用 CFB 模式來模擬 CTR
                aes.Padding = PaddingMode.None; // CTR 模式不需要填充
                var decryptor = aes.CreateDecryptor();
                var decrypt = decryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                return Encoding.UTF8.GetString(decrypt);
            }
        }

    }

    public class TokenManager
    {
        //金鑰，從設定檔或資料庫取得
        public string key = "B69CCA65B4D0B705E37080676E5F5DBBF4EA0E7A0EBD7C60CCE2024E218A53BB";

        //產生 Token
        public Token Create(User user)
        {
            var exp = user.kenLimit; //過期時間(秒)

            //稍微修改 Payload 將使用者資訊和過期時間分開
            var payload = new Payload
            {
                info = user,
                //Unix 時間戳
                exp = Convert.ToInt32(
                    (DateTime.Now.AddSeconds(exp) -
                     new DateTime(1970, 1, 1)).TotalSeconds)
            };

            var json = JsonConvert.SerializeObject(payload);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            var iv = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

            //使用 AES 加密 Payload
            var encrypt = TokenCrypto
                .AESEncrypt(base64, key.Substring(0, 16), iv);

            //取得簽章
            var signature = TokenCrypto
                .ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64));

            return new Token
            {
                //Token 為 iv + encrypt + signature，並用 . 串聯
                access_token = iv + "." + encrypt + "." + signature,
                //Refresh Token 使用 Guid 產生
                refresh_token = Guid.NewGuid().ToString().Replace("-", ""),
                expires_in = exp,
            };
        }

        //取得使用者資訊
        public User GetUser(string token)
        {
            try
            {
                //var token = HttpContext.Current.Request.Headers["Authorization"];

                var split = token.Split('.');
                var iv = split[0];
                var encrypt = split[1];
                var signature = split[2];

                //檢查簽章是否正確
                if (signature != TokenCrypto
                    .ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64)))
                {
                    return null;
                }

                //使用 AES 解密 Payload
                var base64 = TokenCrypto
                    .AESDecrypt(encrypt, key.Substring(0, 16), iv);
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                var payload = JsonConvert.DeserializeObject<Payload>(json);

                //檢查是否過期
                if (payload.exp < Convert.ToInt32(
                    (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds))
                {
                    return null;
                }

                return payload.info;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class Token
    {
        //Token
        public string access_token { get; set; }
        //Refresh Token
        public string refresh_token { get; set; }
        //幾秒過期
        public int expires_in { get; set; }
    }

    public class Payload
    {
        //使用者資訊
        public User info { get; set; }
        //過期時間
        public int exp { get; set; }
    }

    public class User
    {

        /// <summary>
        /// 系統來源
        /// </summary>
        public string SystemSource { get; set; }

        /// <summary>
        /// 使用者類別
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        public string User_No { get; set; }

        /// <summary>
        /// 使用者名稱
        /// </summary>
        public string User_Name { get; set; }

        /// <summary>
        /// 權限群組
        /// </summary>
        public string UserGroup_id { get; set; }

        /// <summary>
        /// 聯絡電話
        /// </summary>
        public string User_Tel { get; set; }

        /// <summary>
        /// 時間戳記
        /// </summary>
        public string datetime { get; set; }

        /// <summary>
        /// Token 有效時間(分鐘)
        /// </summary>
        public int kenLimit { get; set; }
    
    }


}
