using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.ApiModel
{
    public class ApiModelBase
    {
        /// <summary>
        /// 驗證碼
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrMsg { get; set; }
    }
}
