using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.WechatSDK.Common;

namespace Kugar.WechatSDK.MP
{
    public class MenuService:MPBaseService
    {
        public MenuService(CommonApi api) : base(api)
        {
        }

        public async Task<ResultReturn> CreateMenu(string appID)
        {
            return SuccessResultReturn.Default;
        }
    }
}
