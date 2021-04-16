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
    [HarmonyPatch(typeof(StorySystem), "StoryStartEvent")]
    public static class NeedWorkingCheckMarriageCompetitionStory
    {
        static bool _needRecover = false;

        /// <summary>
        /// 进入比武招亲调用前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="waitTime">等待时间</param>
        /// <param name="startEventId">开始的EventId</param>
        [HarmonyPrefix]
        private static void StoryStartEventPrefix(int startEventId)
        //原方法的签名（参照用）
        //private IEnumerator StoryStartEvent(float waitTime, int startEventId)
        {
            //若调用该方法时，startEventId为12101（参加比武招亲，原本太吾若为女性不让参加）
            if (startEventId == 12101)
            {
                ObscureGenderHarmony.ForFemaleTaiwuJoin = true;             //将需要补丁设为是（性别模糊）
                _needRecover = true;                                        //告知需要在原方法结束后，再把NeedPacth关掉
            }
        }

        /// <summary>
        /// 进入比武招亲调用后，关闭性别模糊
        /// </summary>
        [HarmonyPostfix]
        private static void StoryStartEventPostfix()
        //原方法的签名（参照用）
        //private IEnumerator StoryStartEvent(float waitTime, int startEventId)
        {
            //若之前开启了NeedPacth
            if (_needRecover)
            {
                ObscureGenderHarmony.ForFemaleTaiwuJoin = false;            //将需要补丁设为否（并没有实际卸载补丁）
                _needRecover = false;
            }
        }
    }
}