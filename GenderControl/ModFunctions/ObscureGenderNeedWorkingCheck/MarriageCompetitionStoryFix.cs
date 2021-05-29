using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GenderControl
{
    /// <summary>
    /// 性别模糊启用时机补充：太吾参加比武招亲奇遇
    /// </summary>
    /// 附注：女性NPC参加比武招亲的判断部分，已被包含在NPC过月行动里
    [HarmonyPatch(typeof(DateFile), "SetEvent")]
    public static class MarriageCompetitionStoryFix
    {
        /// <summary>
        /// 进入比武招亲调用前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        [HarmonyPrefix]
        private static void SetEventPrefix(DateFile __instance, ref int[] eventDate)
        //原方法的签名（参照用）
        //public void SetEvent(int[] eventDate, bool addToFirst = false, bool only = true)
        {
            //若要设置的事件ID为12110“比武招亲性别不符”，且性别模糊功能为开启状态
            if (eventDate[2] == 12110 && Main.Setting.obscureGender.Value)
            {
                //要设置的事件ID重设为12101“开始比武招亲”
                eventDate[2] = 12101;
            }
        }
    }
}
