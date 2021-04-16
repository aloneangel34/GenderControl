using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GenderControl
{
    /// <summary>
    /// 性别模糊不适宜启用、因此需要修正的场合：人物对话跳转相关（EventID:10000 + (性别-1）*1000 + 处世立场*100 + 好感等级）
    /// </summary>
    [HarmonyPatch(typeof(MessageEventManager), "SetActorMassage")]
    public static class NoNeedFixInSetActorMassage
    {
        static bool _needRecover = false;

        /// <summary>
        /// 调用前 关闭性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="actorId">人物ID</param>
        [HarmonyPrefix]
        private static void SetActorMassagePrefix()
        //原方法签名
        //public void SetActorMassage(int actorId)
        {
            //有开启的话，暂时关闭
            if (ObscureGenderHarmony.NeedPacth == true)
            {
                ObscureGenderHarmony.NeedPacth = false;            //暂时关闭性别模糊
                _needRecover = true;                               //告知需要在原方法结束后，恢复开启
            }
        }

        /// <summary>
        /// 调用后 还原性别模糊
        /// </summary>
        [HarmonyPostfix]
        private static void SetActorMassagePostfix()
        //原方法签名
        //public void SetActorMassage(int actorId)
        {
            if (_needRecover)
            {
                ObscureGenderHarmony.NeedPacth = true;             //恢复性别模糊
                _needRecover = false;
            }
        }
    }
}
