using System;
using System.Collections.Generic;
using System.Text;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.WechatSDK.MP.Entities
{
    [JsonConverter(typeof(MenuJsonConverter))]
    public class MPMenuBase
    {
        /// <summary>
        /// 菜单标题，不超过16个字节，子菜单不超过60个字节
        /// </summary>
        public string Name { set; get; }
    }

    /// <summary>
    /// 主菜单按钮
    /// </summary>
    public class MPMainMenu : MPMenuBase
    {
        /// <summary>
        /// 二级菜单数组，个数应为1~5个
        /// </summary>
        public IReadOnlyList<MPFunctionBase> SubMenus { set; get; }
    }

    [JsonConverter(typeof(MenuJsonConverter))]
    public abstract class MPFunctionBase : MPMenuBase
    {
        /// <summary>
        /// 按钮类型
        /// </summary>
        public abstract string Type { get; }

    }

    public abstract class MPFunctionWithKeyButton : MPFunctionBase
    {
        /// <summary>
        /// 菜单KEY值，用于消息接口推送，不超过128字节
        /// </summary>
        public virtual string Key { set; get; }
    }

    public class UrlViewButton : MPFunctionBase
    {
        /// <summary>
        /// 网页 链接，用户点击菜单可打开链接，不超过1024字节
        /// </summary>
        public string Url { set; get; }

        public override string Type => "view";
    }

    public class MiniProgramButton : MPFunctionBase
    {
        public override string Type => "miniprogram";

        /// <summary>
        /// 不支持小程序的老版本客户端将打开本url。
        /// </summary>
        public string Url { set; get; }

        /// <summary>
        /// 小程序的appid（仅认证公众号可配置）
        /// </summary>
        public string AppID { set; get; }

        /// <summary>
        /// 小程序的页面路径
        /// </summary>
        public string PagePath { set; get; }

    }

    public class ClickButton : MPFunctionWithKeyButton
    {
        public override string Type => "click";

    }

    /// <summary>
    /// 扫码带提示,,扫码推事件且弹出“消息接收中”提示框用户点击按钮后，微信客户端将调起扫一扫工具，完成扫码操作后，将扫码的结果传给开发者，同时收起扫一扫工具，然后弹出“消息接收中”提示框，随后可能会收到开发者下发的消息。
    /// </summary>
    public class ScanCodeAndWaitMsgButton : MPFunctionWithKeyButton
    {
        public override string Type => "scancode_waitmsg";
    }

    /// <summary>
    /// 扫码推事件,,扫码推事件用户点击按钮后，微信客户端将调起扫一扫工具，完成扫码操作后显示扫描结果（如果是URL，将进入URL），且会将扫码的结果传给开发者，开发者可以下发消息
    /// </summary>
    public class ScanCodeAndPushButton : MPFunctionWithKeyButton
    {
        public override string Type => "scancode_push";
    }

    /// <summary>
    /// 系统拍照发图,弹出系统拍照发图用户点击按钮后，微信客户端将调起系统相机，完成拍照操作后，会将拍摄的相片发送给开发者，并推送事件给开发者，同时收起系统相机，随后可能会收到开发者下发的消息。
    /// </summary>
    public class PicSysPhotoButton : MPFunctionWithKeyButton
    {
        public override string Type => "pic_sysphoto";
    }

    /// <summary>
    /// 拍照或者相册发图,弹出拍照或者相册发图用户点击按钮后，微信客户端将弹出选择器供用户选择“拍照”或者“从手机相册选择”。用户选择后即走其他两种流程。
    /// </summary>
    public class PhotoOrAlbumButton : MPFunctionWithKeyButton
    {
        public override string Type => "pic_photo_or_album";
    }

    /// <summary>
    /// 微信相册发图,,弹出微信相册发图器用户点击按钮后，微信客户端将调起微信相册，完成选择操作后，将选择的相片发送给开发者的服务器，并推送事件给开发者，同时收起相册
    /// </summary>
    public class WeixinPicButton : MPFunctionWithKeyButton
    {
        public override string Type => "pic_weixin";
    }

    /// <summary>
    /// 发送位置,,弹出地理位置选择器用户点击按钮后，微信客户端将调起地理位置选择工具，完成选择操作后，将选择的地理位置发送给开发者的服务器，同时收起位置选择工具
    /// </summary>
    public class LocationSelectButton : MPFunctionWithKeyButton
    {
        public override string Type => "location_select";
    }

    /// <summary>
    /// 图片,专门给第三方平台旗下未微信认证（具体而言，是资质认证未通过）的订阅号准备的事件类型，它们是没有事件推送的，能力相对受限，其他类型的公众号不必使用
    /// </summary>
    public class MediaButton : MPFunctionBase
    {
        public override string Type => "media_id";

        /// <summary>
        /// 调用新增永久素材接口返回的合法media_id
        /// </summary>
        public string MediaID { set; get; }
    }

    /// <summary>
    /// 图文消息,门给第三方平台旗下未微信认证（具体而言，是资质认证未通过）的订阅号准备的事件类型，它们是没有事件推送的，能力相对受限，其他类型的公众号不必使用
    /// </summary>
    public class ViewLimitedButton : MPFunctionBase
    {
        public override string Type => "view_limited";

        /// <summary>
        /// 调用新增永久素材接口返回的合法media_id
        /// </summary>
        public string MediaID { set; get; }
    }

    public class MenuJsonConverter : JsonConverter<MPMenuBase>
    {
        public override void WriteJson(JsonWriter writer, MPMenuBase value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WriteProperty("name", value.Name);

            if (value is MPFunctionBase functionBtn)
            {
                writer.WriteProperty("type", functionBtn.Type);

                if (value is MPFunctionWithKeyButton keyButton)
                {
                    writer.WriteProperty("key", keyButton.Key);
                }

                if (value is UrlViewButton urlView)
                {
                    writer.WriteProperty("url", urlView.Url);
                }
                else if (value is MiniProgramButton miniProgram)
                {
                    writer.WriteProperty("url", miniProgram.Url)
                        .WriteProperty("appid", miniProgram.AppID)
                        .WriteProperty("pagepath", miniProgram.PagePath);
                }
                else if (value is MediaButton media)
                {
                    writer.WriteProperty("media_id", media.MediaID);
                }
                else if (value is ViewLimitedButton viewLimited)
                {
                    writer.WriteProperty("media_id", viewLimited.MediaID);
                }
            }
            else if (value is MPMainMenu mainMenu)
            {
                writer.WritePropertyName("sub_button");

                writer.WriteStartArray();
                foreach (var item in mainMenu.SubMenus)
                {
                    serializer.Serialize(writer, item);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        public override MPMenuBase ReadJson(JsonReader reader, Type objectType, MPMenuBase existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var json = (JObject)JObject.ReadFrom(reader);
            
            var type = json.GetString("type");
            var name = json.GetString("name");
            var key = json.GetString("key");

            MPMenuBase menu = null;
            
            if (json.ContainsKey("sub_button"))
            {
                var subMenuJson=json.GetJObjectArray("sub_button");

                var subMenus = new List<MPFunctionBase>();

                foreach (var item in subMenuJson)
                {
                    var m = item.ToObject<MPFunctionBase>();

                    subMenus.Add(m);
                }

                menu = new MPMainMenu()
                {
                    SubMenus = subMenus
                };
            }
            else
            {
                if (!string.IsNullOrEmpty(type))
                {
                    switch (type)
                    {
                        case "click":
                            {
                                menu = new ClickButton()
                                {
                                    Key = key
                                };
                                break;
                            }
                        case "view":
                            {
                                menu = new UrlViewButton()
                                {
                                    Url = json.GetString("url")
                                };
                                break;
                            }
                        case "miniprogram":
                            {
                                menu = new MiniProgramButton()
                                {
                                    AppID = json.GetString("appid"),
                                    PagePath = json.GetString("pagepath"),
                                    Url = json.GetString("url")
                                };
                                break;
                            }
                        case "scancode_waitmsg":
                            {
                                menu = new ScanCodeAndWaitMsgButton()
                                {
                                    Key = key
                                };
                                break;
                            }
                        case "scancode_push":
                            {
                                menu = new ScanCodeAndPushButton()
                                {
                                    Key = key
                                };
                                break;
                            }
                        case "pic_sysphoto":
                            {
                                menu = new PicSysPhotoButton()
                                {
                                    Key = key
                                };
                                break;
                            }
                        case "pic_photo_or_album":
                            {
                                menu = new PhotoOrAlbumButton()
                                {
                                    Key = key
                                };
                                break;
                            }
                        case "pic_weixin":
                            {
                                menu = new WeixinPicButton()
                                {
                                    Key = key
                                };
                                break;
                            }
                        case "location_select":
                            {
                                menu = new LocationSelectButton()
                                {
                                    Key = key
                                };
                                break;
                            }
                        case "media_id":
                            {
                                menu = new MediaButton()
                                {
                                    MediaID = json.GetString("media_id")
                                };
                                break;
                            }
                        case "view_limited":
                            {
                                menu = new ViewLimitedButton()
                                {
                                    MediaID = json.GetString("media_id")
                                };
                                break;
                            }
                    }
                }
            }

            if (menu!=null)
            {
                menu.Name = name;
            }
            
            return menu;
        }
    }


}
