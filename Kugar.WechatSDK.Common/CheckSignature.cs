using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Kugar.WechatSDK.Common
{
    /// <summary>
    /// 签名验证类
    /// </summary>
    public static class CheckSignature
    {
        /// <summary>
        /// 检查签名是否正确
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool Check(string signature, string timestamp, string nonce, string token = null)
        {
            return signature == GetSignature(timestamp, nonce, token);
        }


        /// <summary>
        /// 返回正确的签名
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string GetSignature(string timestamp, string nonce, string token = null)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }
            //token = token ?? Token;
            var arr = new[] { token, timestamp, nonce }.OrderBy(z => z).ToArray();
            var arrString = string.Join("", arr);
            //var enText = FormsAuthentication.HashPasswordForStoringInConfigFile(arrString, "SHA1");//使用System.Web.Security程序集
            using (var sha1 = SHA1.Create())
            {
                var sha1Arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
                StringBuilder enText = new StringBuilder();
                foreach (var b in sha1Arr)
                {
                    enText.AppendFormat("{0:x2}", b);
                }

                return enText.ToString();
            }


        }
    }
}