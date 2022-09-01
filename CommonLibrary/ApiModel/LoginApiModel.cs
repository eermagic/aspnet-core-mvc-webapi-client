using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.ApiModel
{
    public class LoginApiModel
    {
        public class LoginReq : ApiModelBase
        {
            /// <summary>
            /// 登入 ID
            /// </summary>
            public string LoginId { get; set; }

            /// <summary>
            /// 登入密碼
            /// </summary>
            public string LoginPwd { get; set; }
        }

        public class LoginResp : ApiModelBase
        {
            /// <summary>
            /// 使用者名稱
            /// </summary>
            public string UserName { get; set; }
        }
    }
}
