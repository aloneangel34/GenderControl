using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;
using HarmonyLib;
using UnityEngine;
using AI;

namespace GenderControl
{
    /// <summary>
    /// 部分HarmonyPatch，何时需要实际运行的判断（玩家部分 —— 比武招亲奇遇的奇遇）
    /// </summary>
    /// 附注：女性NPC参加比武招亲的判断部分，已被包含在NPC过月行动里
    [HarmonyPatch(typeof(StorySystem), "StoryStartEvent")]
    public static class NeedWorkingCheckMarriageCompetitionStory
    {
        /// <summary>
        /// 进入比武招亲调用前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="waitTime">等待时间</param>
        /// <param name="startEventId">开始的EventId</param>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorySystem), "StoryStartEvent")]
        private static void StoryStartEventPrefix(StorySystem __instance, int startEventId, out bool __state)
        //原方法的签名（参照用）
        //private IEnumerator StoryStartEvent(float waitTime, int startEventId)
        {
            //若调用该方法时，startEventId为12101（参加比武招亲，原本太吾若为女性不让参加）
            if (startEventId == 12101)
            {
                ObscureGenderHarmony.ForFemaleTaiwuJoin = true;             //将需要补丁设为是（性别模糊）
                __state = true;                                             //告知需要在原方法结束后，再把NeedPacth关掉
            }
            else
            { __state = false; }
        }

        /// <summary>
        /// 进入比武招亲调用后，关闭性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="waitTime">等待时间</param>
        /// <param name="startEventId">开始的EventId</param>
        [HarmonyPostfix]
        private static void StoryStartEventPostfix(StorySystem __instance, bool __state)
        //原方法的签名（参照用）
        //private IEnumerator StoryStartEvent(float waitTime, int startEventId)
        {
            //若之前开启了NeedPacth
            if (__state)
            {
                ObscureGenderHarmony.ForFemaleTaiwuJoin = false;            //将需要补丁设为否（并没有实际卸载补丁）
            }
        }
    }
}