using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kugar.WechatSDK.Common.Helpers
{
    public static class EncryptHelper
    {
        /// <summary>采用SHA-1算法加密字符串（小写）</summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <returns></returns>
        public static string GetSha1(string encypStr)
        {
            byte[] hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(encypStr));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte num in hash)
                stringBuilder.AppendFormat("{0:x2}", (object)num);
            return stringBuilder.ToString();
        }

        /// <summary>HMAC SHA256 加密</summary>
        /// <param name="message">加密消息原文。当为小程序SessionKey签名提供服务时，其中message为本次POST请求的数据包（通常为JSON）。特别地，对于GET请求，message等于长度为0的字符串。</param>
        /// <param name="secret">秘钥（如小程序的SessionKey）</param>
        /// <returns></returns>
        public static string GetHmacSha256(string message, string secret)
        {
            message = message ?? "";
            secret = secret ?? "";
            byte[] bytes1 = Encoding.UTF8.GetBytes(secret);
            byte[] bytes2 = Encoding.UTF8.GetBytes(message);
            using (HMACSHA256 hmacshA256 = new HMACSHA256(bytes1))
            {
                byte[] hash = hmacshA256.ComputeHash(bytes2);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte num in hash)
                    stringBuilder.AppendFormat("{0:x2}", (object)num);
                return stringBuilder.ToString();
            }
        }

        /// <summary>获取大写的MD5签名结果</summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetMD5(string encypStr, Encoding encoding)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes;
            try
            {
                bytes = encoding.GetBytes(encypStr);
            }
            catch
            {
                bytes = Encoding.GetEncoding("utf-8").GetBytes(encypStr);
            }
            return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToUpper();
        }

        /// <summary>获取大写的MD5签名结果</summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="charset">编码</param>
        /// <returns></returns>
        public static string GetMD5(string encypStr, string charset = "utf-8")
        {
            charset = charset ?? "utf-8";
            try
            {
                return EncryptHelper.GetMD5(encypStr, Encoding.GetEncoding(charset));
            }
            catch
            {
                return EncryptHelper.GetMD5("utf-8", Encoding.GetEncoding(charset));
            }
        }

        /// <summary>获取小写的MD5签名结果</summary>
        /// <param name="encypStr">需要加密的字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string GetLowerMD5(string encypStr, Encoding encoding) => EncryptHelper.GetMD5(encypStr, encoding).ToLower();

        /// <summary>AES加密（默认为CBC模式）</summary>
        /// <param name="inputdata">输入的数据</param>
        /// <param name="iv">向量</param>
        /// <param name="strKey">加密密钥</param>
        /// <returns></returns>
        public static byte[] AESEncrypt(byte[] inputdata, byte[] iv, string strKey)
        {
            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm)Aes.Create();
            byte[] buffer = inputdata;
            symmetricAlgorithm.Key = Encoding.UTF8.GetBytes(strKey.PadRight(32));
            symmetricAlgorithm.IV = iv;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(buffer, 0, buffer.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>AES解密（默认为CBC模式）</summary>
        /// <param name="inputdata">输入的数据</param>
        /// <param name="iv">向量</param>
        /// <param name="strKey">key</param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] inputdata, byte[] iv, string strKey)
        {
            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm)Aes.Create();
            symmetricAlgorithm.Key = Encoding.UTF8.GetBytes(strKey.PadRight(32));
            symmetricAlgorithm.IV = iv;
            using (MemoryStream memoryStream1 = new MemoryStream(inputdata))
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream1, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (MemoryStream memoryStream2 = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        int count;
                        while ((count = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                            memoryStream2.Write(buffer, 0, count);
                        return memoryStream2.ToArray();
                    }
                }
            }
        }

        /// <summary>AES 加密（无向量，CEB模式，秘钥长度=128）</summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AESEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str))
                return (string)null;
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            byte[] inArray = rijndaelManaged.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(inArray, 0, inArray.Length);
        }

        /// <summary>AES 解密（无向量，CEB模式，秘钥长度=128）</summary>
        /// <param name="data">被加密的明文（注意：为Base64编码）</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string AESDecrypt(string data, string key)
        {
            byte[] buffer1 = Convert.FromBase64String(data);
            byte[] numArray1 = new byte[32];
            Array.Copy((Array)Encoding.UTF8.GetBytes(key.PadRight(numArray1.Length)), (Array)numArray1, numArray1.Length);
            MemoryStream memoryStream = new MemoryStream(buffer1);
            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm)Aes.Create();
            symmetricAlgorithm.Mode = CipherMode.ECB;
            symmetricAlgorithm.Padding = PaddingMode.PKCS7;
            symmetricAlgorithm.KeySize = 128;
            symmetricAlgorithm.Key = numArray1;
            ICryptoTransform decryptor = symmetricAlgorithm.CreateDecryptor();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] numArray2 = new byte[buffer1.Length + 32];
            byte[] buffer2 = numArray2;
            int count = buffer1.Length + 32;
            int length = cryptoStream.Read(buffer2, 0, count);
            byte[] bytes = new byte[length];
            Array.Copy((Array)numArray2, 0, (Array)bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
