using Microsoft.AspNetCore.Mvc;
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
                string tokenData = Decode3DES(req.Token, encyptKey);

                // 檢查時間有效內
                TimeSpan TS = new System.TimeSpan(Convert.ToDateTime(tokenData).Ticks - DateTime.Now.Ticks);
                double timeDiff =  Convert.ToDouble(TS.TotalMinutes); //分鐘差異
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
        /// 使用 3DES 解碼
        /// </summary>
        /// <param name="strEncryptData"></param>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public static string Decode3DES(string strEncryptData, string strKey)
        {
            byte[] inputArray = Convert.FromBase64String(strEncryptData);
            var tripleDES = TripleDES.Create();
            var md5Serv = MD5.Create();
            byte[] keyArray = md5Serv.ComputeHash(Encoding.UTF8.GetBytes(strKey));
            md5Serv.Dispose();
            byte[] allKey = new byte[24];
            Buffer.BlockCopy(keyArray, 0, allKey, 0, 16);
            Buffer.BlockCopy(keyArray, 0, allKey, 16, 8);
            tripleDES.Key = allKey;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            string result = Encoding.UTF8.GetString(resultArray);
            return result;
        }
    }
}
