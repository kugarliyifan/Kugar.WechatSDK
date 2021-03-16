using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Kugar.WechatSDK.MiniProgram.Results;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MiniProgram
{
    /// <summary>
    /// 账号相关功能
    /// </summary>
    public interface ISNS
    {
        /// <summary>
        /// 小程序wx.login返回的jsCode换取openid
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="jsCode">小程序通过wx.login获取的code</param>
        /// <returns></returns>
        Task<ResultReturn<(string openid, string session_key, string unionid)>> Code2session(string appID, string jsCode);

        /// <summary>
        /// 解密用户数据
        /// </summary>
        /// <param name="session_key">登录获取的session_key</param>
        /// <param name="iv">加密算法的初始向量</param>
        /// <param name="encryptedData">包括敏感数据在内的完整用户信息的加密数据</param>
        /// <returns></returns>
        Task<ResultReturn<DecryptUserData_Result>> DecryptUserData(string session_key,string iv, string encryptedData);

        /// <summary>
        /// 解密手机号数据
        /// </summary>
        /// <param name="session_key">登录获取的session_key</param>
        /// <param name="iv">加密算法的初始向量</param>
        /// <param name="encryptedData">包括敏感数据在内的完整用户信息的加密数据</param>
        /// <returns></returns>
        Task<ResultReturn<DecryptPhoneData_Result>> DecryptPhoneData(string session_key,string iv, string encryptedData);
    }

    /// <summary>
    /// 账号相关功能
    /// </summary>
    public class SNS :BaseService, ISNS
    {
        //private ICommonApi _api = null;
        private IWechatGateway _gateway = null;

        public SNS(ICommonApi api, IWechatGateway gateway):base(api)
        {
            _gateway = gateway;
        }


        /// <summary>
        /// 小程序wx.login返回的jsCode换取openid
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="jsCode">小程序通过wx.login获取的code</param>
        /// <returns></returns>
        public async Task<ResultReturn<(string openid, string session_key, string unionid)>> Code2session(string appID, string jsCode)
        {
            var appSerect = _gateway.Get(appID);

            var ret = await CommonApi.Get(appID,
                $"/sns/jscode2session?appid={appID}&secret={appSerect.AppSerect}&js_code={jsCode}&grant_type=authorization_code");

            if (ret.IsSuccess)
            {
                return ret.Cast(
                    (ret.ReturnData.GetString("openid"), ret.ReturnData.GetString("session_key"),
                        ret.ReturnData.GetString("unionid")), default);
            }
            else
            {
                return ret.Cast<(string openid, string session_key, string unionid)>(default);
            }
            

        }

        /// <summary>
        /// 解密用户数据
        /// </summary>
        /// <param name="session_key">登录获取的session_key</param>
        /// <param name="iv">加密算法的初始向量</param>
        /// <param name="encryptedData">包括敏感数据在内的完整用户信息的加密数据</param>
        /// <returns></returns>
        public async Task<ResultReturn<DecryptUserData_Result>> DecryptUserData(string session_key,string iv, string encryptedData)
        {
            Convert.FromBase64String(encryptedData);
            byte[] Key = Convert.FromBase64String(session_key);
            byte[] Iv = Convert.FromBase64String(iv);

            var jsonStr= AES_Decrypt(encryptedData, Iv, Key);

            var userJson = JObject.Parse(jsonStr);

            var result = new DecryptUserData_Result()
            {
                OpenId = userJson.GetString("openId"),
                NickName = userJson.GetString("nickName"),
                Gender = userJson.GetString("gender"),
                City = userJson.GetString("city"),
                Province = userJson.GetString("province"),
                Country = userJson.GetString("country"),
                AvatarUrl = userJson.GetString("avatarUrl"),
                UnionId = userJson.GetString("unionId")
            };

            return new SuccessResultReturn<DecryptUserData_Result>(result);
        }

        /// <summary>
        /// 解密手机号数据
        /// </summary>
        /// <param name="session_key">登录获取的session_key</param>
        /// <param name="iv">加密算法的初始向量</param>
        /// <param name="encryptedData">包括敏感数据在内的完整用户信息的加密数据</param>
        /// <returns></returns>
        public async Task<ResultReturn<DecryptPhoneData_Result>> DecryptPhoneData(string session_key,string iv, string encryptedData)
        {
            Convert.FromBase64String(encryptedData);
            byte[] Key = Convert.FromBase64String(session_key);
            byte[] Iv = Convert.FromBase64String(iv);

            var jsonStr= AES_Decrypt(encryptedData, Iv, Key);

            var userJson = JObject.Parse(jsonStr);

            var result = new DecryptPhoneData_Result()
            {
                PhoneNumber = userJson.GetString("phoneNumber"),
                PurePhoneNumber = userJson.GetString("purePhoneNumber"),
                CountryCode = userJson.GetString("countryCode")
            };

            return new SuccessResultReturn<DecryptPhoneData_Result>(result);
        }


        private string  AES_Decrypt(string Input, byte[] Iv, byte[] Key)
        {
            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm) Aes.Create();
            symmetricAlgorithm.KeySize = 128;
            symmetricAlgorithm.BlockSize = 128;
            symmetricAlgorithm.Mode = CipherMode.CBC;
            symmetricAlgorithm.Padding = PaddingMode.PKCS7;
            symmetricAlgorithm.Key = Key;
            symmetricAlgorithm.IV = Iv;
            ICryptoTransform decryptor = symmetricAlgorithm.CreateDecryptor(symmetricAlgorithm.Key, symmetricAlgorithm.IV);
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Write))
                    {
                        byte[] buffer = Convert.FromBase64String(Input);
                        byte[] numArray = new byte[buffer.Length + 32 - buffer.Length % 32];
                        Array.Copy((Array) buffer, (Array) numArray, buffer.Length);
                        cryptoStream.Write(buffer, 0, buffer.Length);
                    }
                    return Encoding.UTF8.GetString(decode2(memoryStream.ToArray()));
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("===== CryptographicException =====");
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Write);
                    byte[] numArray1 = Convert.FromBase64String(Input);
                    byte[] numArray2 = new byte[numArray1.Length + 32 - numArray1.Length % 32];
                    Array.Copy((Array) numArray1, (Array) numArray2, numArray1.Length);
                    byte[] buffer = numArray1;
                    int length = numArray1.Length;
                    cryptoStream.Write(buffer, 0, length);
                    return Encoding.UTF8.GetString(decode2(memoryStream.ToArray()));
                }
            }
        }

        private byte[] decode2(byte[] decrypted)
        {
            int num = (int) decrypted[decrypted.Length - 1];
            if (num < 1 || num > 32)
                num = 0;
            byte[] numArray = new byte[decrypted.Length - num];
            Array.Copy((Array) decrypted, 0, (Array) numArray, 0, decrypted.Length - num);
            return numArray;
        }
    }
}
