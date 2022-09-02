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

                req.LoginId = txtLoginId.Text; //�b��
                req.LoginPwd = txtLoginPwd.Text; //�K�X

                // Api �K�_ (��ĳ�s�b�]�w�ɤ�)
                string EncyptKey = "123456";

                // Api ���ҽX
                string nowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                string token = EncryptAES256(nowTime, EncyptKey, null);
                req.Token = token;

                // Api Url
                string url = "https://localhost:7151/Api/Login";

                // �I�s Web Api �n�J�ˬd
                LoginResp resp = CallApi<LoginResp>(url, req);

                // ErrMsg ���Ȫ�ܵn�J����
                if (!string.IsNullOrEmpty(resp.ErrMsg))
                {
                    // �n�J���~
                    MessageBox.Show(resp.ErrMsg);
                }
                else
                {
                    // �n�J���\
                    MessageBox.Show("�n�J���\");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// �I�s Api
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
        /// �ϥ� AES 256 �[�K
        /// </summary>
        /// <param name="source">����</param>
        /// <param name="key">�K�_</param>
        /// <param name="iv">IV</param>
        /// <returns></returns>
        public static string EncryptAES256(string source, string key, string? iv)
        {
            // IV �� 16 �ӭ^��μƦr
            if (string.IsNullOrEmpty(iv))
            {
                iv = "1234567890abcdef";
            }

            // ���� MD5 32 �r��K�_
            string md5Key = "";
            using (MD5 md5Serv = MD5.Create())
            {
                byte[] keyArray = md5Serv.ComputeHash(Encoding.UTF8.GetBytes(key));
                md5Key = BitConverter.ToString(keyArray).Replace("-", "").ToUpper();
            }

            // �ϥ� AES �[�K
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