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
    /// 防止NPC在过月行动时脱离特定势力（理论上不应影响到太吾要求做的事）
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AIChangeGong")]
    public static class NpcPassTurnCantChangeGangHarmony
    {
        public static bool NeedPacth = false;

        /// <summary>
        /// 防止NPC在过月行动时脱离特定势力
        /// </summary>
        /// <param name="__result">原方法的返回的人物特性列表</param>
        /// <param name="key">人物ID</param>
        /// <returns>该补丁执行完后是否继续执行原方法</returns>
        [HarmonyPrefix]
        private static bool AIChangeGongPrefix(int actorId, int baseGongId, int baseGongLevel, int toGongId, int toGongLevel)
        //原方法的签名（参照用）
        //private void AIChangeGong(int actorId, int baseGongId, int baseGongLevel, int toGongId, int toGongLevel)
        {
            //调试信息
            if (Main.Setting.debugMode.Value)
            {
                Main.SB.AppendFormat("AIChangeGong方法。actorId:{0} 试图从帮派:{1} 品阶:{2} 转投至 帮派:{3} 品阶:{4}。NPC过月行动中:{5} 原势力禁止脱离:{6}", actorId, DateFile.instance.GetGangDate(baseGongId, 0), baseGongLevel, DateFile.instance.GetGangDate(toGongId, 0), toGongLevel, NeedPacth, Settings.CantChangeGangIds.Contains(baseGongId));
                Main.Logger.LogDebug(Main.SB);
                Main.SB.Clear();
            }

            //若 处于过月行动中 且 禁止NPC脱离所属势力的列表中包含想脱离的势力
            if (NeedPacth)
            {
                if (Settings.CantChangeGangIds.Contains(baseGongId))
                {
                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        Main.SB.AppendFormat("已拦截 actorId:{0} 试图从帮派:{1} 品阶:{2} 转投至 帮派:{3} 品阶:{4} 的行为", actorId, DateFile.instance.GetGangDate(baseGongId, 0), baseGongLevel, DateFile.instance.GetGangDate(toGongId, 0), toGongLevel);
                        Main.Logger.LogInfo(Main.SB);
                        Main.SB.Clear();
                    }

                    return false;       //跳过原方法的执行（不进行变更）
                }
                else
                {
                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        Main.Logger.LogDebug("不属于禁止脱离势力，未拦截");
                    }
                }
            }

            return true;            //继续执行原方法（NPC变更所属势力）
        }
	}
}