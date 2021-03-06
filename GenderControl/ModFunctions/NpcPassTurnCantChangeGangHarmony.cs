using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx.Logging;

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
        /// <param name="actorId">人物ID</param>
        /// <param name="baseGongId">原势力ID</param>
        /// <param name="baseGongLevel">原身份品阶</param>
        /// <param name="toGongId">新势力ID</param>
        /// <param name="toGongLevel">新身份品阶</param>
        /// <returns>该补丁执行完后是否继续执行原方法</returns>
        [HarmonyPrefix]
        private static bool AIChangeGongPrefix(int actorId, int baseGongId, int baseGongLevel, int toGongId, int toGongLevel)
        //原方法的签名（参照用）
        //private void AIChangeGong(int actorId, int baseGongId, int baseGongLevel, int toGongId, int toGongLevel)
        {
            //调试信息
            //if (Main.Setting.debugMode.Value)
            //{
            //    QuickLogger.Log(LogLevel.Info, "AIChangeGong方法。actorId:{0} 试图从帮派:{1} 品阶:{2} 转投至 帮派:{3} 品阶:{4}。NPC过月行动中:{5} 原势力禁止脱离:{6}", actorId, DateFile.instance.GetGangDate(baseGongId, 0), baseGongLevel, DateFile.instance.GetGangDate(toGongId, 0), toGongLevel, NeedPacth, Settings.CantChangeGangIds.Contains(baseGongId));
            //}

            //若 处于过月行动中 且 禁止NPC脱离所属势力的列表中包含想脱离的势力
            if (NeedPacth)
            {
                if (Settings.CantChangeGangIds.Contains(baseGongId))
                {
                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        QuickLogger.Log(LogLevel.Info, "已拦截 actorId:{0} 试图从帮派:{1} 品阶:{2} 转投至 帮派:{3} 品阶:{4} 的行为", actorId, DateFile.instance.GetGangDate(baseGongId, 0), baseGongLevel, DateFile.instance.GetGangDate(toGongId, 0), toGongLevel);
                    }

                    return false;       //跳过原方法的执行（不进行变更）
                }
                //else
                //{
                //    //调试信息
                //    if (Main.Setting.debugMode.Value)
                //    {
                //        Main.Logger.LogInfo("不属于禁止脱离势力，未拦截");
                //    }
                //}
            }

            return true;            //继续执行原方法（NPC变更所属势力）
        }
    }
}