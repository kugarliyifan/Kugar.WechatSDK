using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.WechatSDK.Common;
using Kugar.WechatSDK.Common.Gateway;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MiniProgram
{
    /// <summary>
    /// 小程序吗/二维码功能
    /// </summary>
    public interface IQrCode
    {
        /// <summary>
        /// 创建二维码,有数量限制:100,000个
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="path">跳转路径</param>
        /// <param name="width">二维码尺寸:280-1280px之间的尺寸</param>
        /// <returns></returns>
        Task<ResultReturn<Stream>> CreateLimitQrCode(string appID,string path,int width);

        /// <summary>
        /// 获取小程序码，适用于需要的码数量较少的业务场景。通过该接口生成的小程序码，永久有效，有数量限制
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="path">扫码进入的小程序页面路径，最大长度 128 字节，不能为空；对于小游戏，可以只传入 query 部分，来实现传参效果，如：传入 "?foo=bar"</param>
        /// <param name="width">二维码的宽度，单位 px。最小 280px，最大 1280px</param>
        /// <param name="auto_color">自动配置线条颜色，如果颜色依然是黑色，则说明不建议配置主色调</param>
        /// <param name="line_color">auto_color 为 false 时生效，使用 rgb 设置颜色</param>
        /// <param name="is_hyaline">是否需要透明底色，为 true 时，生成透明底色的小程序码</param>
        /// <returns></returns>
        Task<ResultReturn<Stream>> CreateLimitMiniProgramCode(string appID, 
            string path, 
            int width,
            bool auto_color=true,
            Color line_color=default,
            bool is_hyaline=false
        );

        /// <summary>
        /// 创建不限定数量的小程序码
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="scene">扫码后跳转到小程序页面时,传递的参数,最大不能超过32个字符</param>
        /// <param name="page">必须是已经发布的小程序存在的页面（否则报错）,（参数请放在scene字段里），如果不填写这个字段，默认跳主页面</param>
        /// <param name="width">二维码的宽度，单位 px。最小 280px，最大 1280px</param>
        /// <param name="auto_color">自动配置线条颜色，如果颜色依然是黑色，则说明不建议配置主色调</param>
        /// <param name="line_color">auto_color 为 false 时生效，使用 rgb 设置颜色</param>
        /// <param name="is_hyaline">是否需要透明底色，为 true 时，生成透明底色的小程序码</param>
        /// <returns></returns>
        Task<ResultReturn<Stream>> CreateUnlimitMiniProgramCode(string appID, 
            string scene,
            string page, 
            int width,
            bool auto_color=true,
            Color line_color=default,
            bool is_hyaline=false);
    }

    /// <summary>
    /// 小程序吗/二维码功能
    /// </summary>
    public class QrCode:BaseService, IQrCode
    {
        public QrCode(ICommonApi api):base(api)
        {
        }

        /// <summary>
        /// 创建二维码,有数量限制:100,000个
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="path">跳转路径</param>
        /// <param name="width">二维码尺寸:280-1280px之间的尺寸</param>
        /// <returns></returns>
        public async Task<ResultReturn<Stream>> CreateLimitQrCode(string appID,string path,int width)
        {
            if (width<280)
            {
                width = 280;
            }

            if (width>1280)
            {
                width = 1280;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentOutOfRangeException(nameof(path), "跳转路径不能为空");
            }

            if (path.Length>128)
            {
                throw new ArgumentOutOfRangeException(nameof(path), "跳转路径长度不能超过128个字节");
            }

            var data=await CommonApi.PostRaw(appID, "/cgi-bin/wxaapp/createwxaqrcode?access_token=ACCESS_TOKEN", new JObject()
            {
                ["path"] = path,
                ["width"] = width
            });

            if (data.data.Length>100)
            {
                return new SuccessResultReturn<Stream>(data.data);
            }
            else
            {
                var jsonStr = data.data.ReadToEnd(Encoding.UTF8);

                var json = JObject.Parse(jsonStr);

                return new FailResultReturn<Stream>(json.GetString("errmsg"), json.GetInt("errcode"));
            }
        }

        /// <summary>
        /// 获取小程序码，适用于需要的码数量较少的业务场景。通过该接口生成的小程序码，永久有效，有数量限制
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="path">扫码进入的小程序页面路径，最大长度 128 字节，不能为空；对于小游戏，可以只传入 query 部分，来实现传参效果，如：传入 "?foo=bar"</param>
        /// <param name="width">二维码的宽度，单位 px。最小 280px，最大 1280px</param>
        /// <param name="auto_color">自动配置线条颜色，如果颜色依然是黑色，则说明不建议配置主色调</param>
        /// <param name="line_color">auto_color 为 false 时生效，使用 rgb 设置颜色</param>
        /// <param name="is_hyaline">是否需要透明底色，为 true 时，生成透明底色的小程序码</param>
        /// <returns></returns>
        public async Task<ResultReturn<Stream>> CreateLimitMiniProgramCode(string appID, 
            string path, 
            int width,
            bool auto_color=true,
            Color line_color=default,
            bool is_hyaline=false
            )
        {
            if (width<280)
            {
                width = 280;
            }

            if (width>1280)
            {
                width = 1280;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentOutOfRangeException(nameof(path), "跳转路径不能为空");
            }

            if (path.Length>128)
            {
                throw new ArgumentOutOfRangeException(nameof(path), "跳转路径长度不能超过128个字节");
            }

            var data=await CommonApi.PostRaw(appID, "/wxa/getwxacode?access_token=ACCESS_TOKEN", new JObject()
            {
                ["path"] = path,
                ["width"] = width,
                ["auto_color"] = auto_color,
                ["line_color"] = new JObject()
                {
                    ["r"]=line_color.R.ToString("X2"),
                    ["g"]=line_color.G.ToString("X2"),
                    ["b"]=line_color.B.ToString("X2")
                },
                ["is_hyaline"] = is_hyaline
            });

            if (data.data.Length>100)
            {
                return new SuccessResultReturn<Stream>(data.data);
            }
            else
            {
                var jsonStr = data.data.ReadToEnd(Encoding.UTF8);

                var json = JObject.Parse(jsonStr);

                return new FailResultReturn<Stream>(json.GetString("errmsg"), json.GetInt("errcode"));
            }
        }

        /// <summary>
        /// 创建不限定数量的小程序码
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="scene">扫码后跳转到小程序页面时,传递的参数,最大不能超过32个字符</param>
        /// <param name="page">必须是已经发布的小程序存在的页面（否则报错）,（参数请放在scene字段里），如果不填写这个字段，默认跳主页面</param>
        /// <param name="width">二维码的宽度，单位 px。最小 280px，最大 1280px</param>
        /// <param name="auto_color">自动配置线条颜色，如果颜色依然是黑色，则说明不建议配置主色调</param>
        /// <param name="line_color">auto_color 为 false 时生效，使用 rgb 设置颜色</param>
        /// <param name="is_hyaline">是否需要透明底色，为 true 时，生成透明底色的小程序码</param>
        /// <returns></returns>
        public async Task<ResultReturn<Stream>> CreateUnlimitMiniProgramCode(string appID, 
            string scene,
            string page, 
            int width,
            bool auto_color=true,
            Color line_color=default,
            bool is_hyaline=false)
        {
            if (width<280)
            {
                width = 280;
            }

            if (width>1280)
            {
                width = 1280;
            }

            if (!string.IsNullOrWhiteSpace(scene) && scene.Length>32)
            {
                throw new ArgumentOutOfRangeException(nameof(scene), "最大长度不能超过32个字符");
            }

            if (!string.IsNullOrWhiteSpace(page) && page[0]=='/')
            {
                page = page.Substring(1);
            }

            var data=await CommonApi.PostRaw(appID, "/wxa/getwxacode?access_token=ACCESS_TOKEN", new JObject()
            {
                ["scene"]=scene,
                ["page"] = page,
                ["width"] = width,
                ["auto_color"] = auto_color,
                ["line_color"] = new JObject()
                {
                    ["r"]=line_color.R.ToString("X2"),
                    ["g"]=line_color.G.ToString("X2"),
                    ["b"]=line_color.B.ToString("X2")
                },
                ["is_hyaline"] = is_hyaline
            });

            if (data.data.Length>100)
            {
                return new SuccessResultReturn<Stream>(data.data);
            }
            else
            {
                var jsonStr = data.data.ReadToEnd(Encoding.UTF8);

                var json = JObject.Parse(jsonStr);

                return new FailResultReturn<Stream>(json.GetString("errmsg"), json.GetInt("errcode"));
            }
        }
    }
}
