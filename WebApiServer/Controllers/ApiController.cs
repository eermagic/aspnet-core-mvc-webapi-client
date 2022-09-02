using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using static CommonLibrary.ApiModel.LoginApiModel;

namespace WebApiServer.Controllers
{
    public class ApiController : Controller
    {
        /// <summary>
        /// 登入檢查
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public IActionResult Login([FromBody] LoginReq req)
        {
            LoginResp resp = new LoginResp();

            //檢查欄位必填
            if (ModelState.IsValid == false)
            {
                string ErrMsg = string.Join("\n", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                resp.ErrMsg = ErrMsg;
                return Json(resp);
            }

            // 檢查驗證碼
            try
            {
                // Api 密鑰
                IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
                string encyptKey = Config.GetSection("EncyptKey").Value;
                string tokenData = DecryptAES256(req.Token, encyptKey, null);

                // 檢查時間有效內
                TimeSpan TS = new System.TimeSpan(Convert.ToDateTime(tokenData).Ticks - DateTime.Now.Ticks);
                double timeDiff = Convert.ToDouble(TS.TotalMinutes); //分鐘差異
                if (timeDiff > 5 || timeDiff < -5)
                {
                    resp.ErrMsg = "驗證碼已過期";
                    return Json(resp);
                }
            }
            catch
            {
                resp.ErrMsg = "驗證碼格式錯誤";
                return Json(resp);
            }

            // 檢查資料庫帳密正確
            string loginId = req.LoginId;
            string loginPwd = req.LoginPwd;

            bool loginCheck = true; //資料庫檢查結果

            //省略資料庫查詢動作...


            //直接回傳帳密正確的結果
            if (loginCheck)
            {
                resp.UserName = "王小明";
            }
            else
            {
                resp.ErrMsg = "帳號密碼錯誤";
            }
            return Json(resp);
        }

        /// <summary>
        /// 使用 AES 256 解密
        /// </summary>
        /// <param name="encryptData">密文</param>
        /// <param name="key">密鑰</param>
        /// <param name="iv">IV</param>
        /// <returns></returns>
        public static string DecryptAES256(string encryptData, string key, string? iv)
        {
            // IV 為 16 個英文或數字
            if (string.IsNullOrEmpty(iv))
            {
                iv = "1234567890abcdef";
            }

            // 產生 MD5 32 字串密鑰
            string md5Key = "";
            using (MD5 md5Serv = MD5.Create())
            {
                byte[] keyArray = md5Serv.ComputeHash(Encoding.UTF8.GetBytes(key));
                md5Key = BitConverter.ToString(keyArray).Replace("-", "").ToUpper();
            }

            // 使用 AES 解密
            var encryptBytes = Convert.FromBase64String(encryptData);
            string decryptStr = "";
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(md5Key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = aes.CreateDecryptor();
                decryptStr = Encoding.UTF8.GetString(transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length));
            }
            return decryptStr;
        }
    }
}
