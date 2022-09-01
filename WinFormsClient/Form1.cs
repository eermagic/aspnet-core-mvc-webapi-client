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
                string token = Encode3DES(nowTime, EncyptKey);
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
        /// �ϥ� 3DES �s�X
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public static string Encode3DES(string strSource, string strKey)
        {
            byte[] inputArray = Encoding.UTF8.GetBytes(strSource);
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
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            string result = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            return result;
        }
    }
}