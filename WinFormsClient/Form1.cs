using CommonLibrary.ApiModel;
using Newtonsoft.Json;
using System.Collections;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using static CommonLibrary.ApiModel.LoginApiModel;

namespace WinFormsClient
{
    public partial class Form1 : Form
    {
        public static readonly HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                LoginReq req = new LoginReq();

                req.LoginId = txtLoginId.Text; //帳號
                req.LoginPwd = txtLoginPwd.Text; //密碼

                // Api 密鑰 (建議存在設定檔內)
                string EncyptKey = "123456";

                // Api 驗證碼
                string nowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                string token = EncryptAES256(nowTime, EncyptKey, null);
                req.Token = token;

                // Api Url
                string url = "https://localhost:7151/Api/Login";

                // 呼叫 Web Api 登入檢查
                LoginResp resp = CallApi<LoginResp>(url, req);

                // ErrMsg 有值表示登入失敗
                if (!string.IsNullOrEmpty(resp.ErrMsg))
                {
                    // 登入錯誤
                    MessageBox.Show(resp.ErrMsg);
                }
                else
                {
                    // 登入成功
                    MessageBox.Show("登入成功");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 呼叫 Api
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T CallApi<T>(string apiUrl, ApiModelBase req)
        {
            string json = JsonConvert.SerializeObject(req);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var responseTask = httpClient.PostAsync(apiUrl, data);

            responseTask.Wait();
            var result = responseTask.Result;
            var readTask = result.Content.ReadAsStringAsync();
            readTask.Wait();

            if (result.IsSuccessStatusCode)
            {
                json = readTask.Result;
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                throw new Exception(result.ReasonPhrase + "\n" + readTask.Result);
            }
        }

        /// <summary>
        /// 使用 AES 256 加密
        /// </summary>
        /// <param name="source">本文</param>
        /// <param name="key">密鑰</param>
        /// <param name="iv">IV</param>
        /// <returns></returns>
        public static string EncryptAES256(string source, string key, string? iv)
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

            // 使用 AES 加密
            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
            string encryptStr = "";
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(md5Key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = aes.CreateEncryptor();
                encryptStr = Convert.ToBase64String(transform.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length));
            }
            return encryptStr;
        }
    }
}