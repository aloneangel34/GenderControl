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
    /// 模糊人物性别，以便同性NPC能够实行异性之间才能做的行为（不会实际影响人物数据）
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetActorDate")]
    public static class ObscureGenderHarmony
    {
        public static bool NeedPacth = false;
        public static bool ForFemaleTaiwuJoin = false;

        /// <summary>
        /// （在特定场合）将主动行动者视为男性，将被其互动者视为女性。以便简单粗暴地实现同性可以进行异性行为的效果（不会实际影响人物数据）
        /// </summary>
        /// <param name="__result">原方法的人物指定属性的值（字符串）</param>
        /// <param name="actorId">人物ID</param>
        /// <param name="key">属性ID</param>
        /// <returns>该补丁执行完后是否继续执行原方法</returns>
        [HarmonyPrefix]
        private static bool GetActorDatePrefix(ref string __result, int actorId, int key)
        //原方法的签名（参照用）
        //public string GetActorDate(int actorId, int key, bool applyBonus = true)
        {
            //若尝试获取的人物属性为“人物性别14”
            if (key == 14)
            {
                //调试信息
                //if (Main.Setting.debugMode.Value)
                //{
                //    Main.SB.AppendFormat("性别模糊GetActorDate方法补丁 实际生效:{0} actorId:{1}，当前行动主动方ID:{2}。", NeedPacth, actorId, Settings.PatchActorID);
                //    Main.Logger.LogDebug(Main.SB);
                //    Main.SB.Clear();
                //}

                //若处于NPC过月行动的方法循环中，且要求获取的人物属性为“性别14”
                if (NeedPacth)
                {
                    //如果人物ID为“正在设置过月行动的NPC”
                    if (Settings.PatchActorID == actorId)
                    {
                        //调试信息
                        //if (Main.Setting.debugMode.Value)
                        //{
                        //    Main.SB.AppendFormat("actorId:{0} 模糊为男性 当前行动主动方ID:{1}。", actorId, Settings.PatchActorID);
                        //    Main.Logger.LogDebug(Main.SB);
                        //    Main.SB.Clear();
                        //}

                        __result = "1";     //性别模糊为男性（不然女性NPC行动时，不会选择参加比武招亲）
                        return false;       //跳过原方法
                    }
                    else
                    {
                        //调试信息
                        //if (Main.Setting.debugMode.Value)
                        //{
                        //    Main.SB.AppendFormat("actorId:{0} 模糊为女性 当前行动主动方ID:{1}。", actorId, Settings.PatchActorID);
                        //    Main.Logger.LogDebug(Main.SB);
                        //    Main.SB.Clear();
                        //}

                        __result = "2";     //性别模糊为女性（这样“行动者”与“被互动方”互为异性。双方真实性别为同性也可以正常怀孕）
                        return false;       //跳过原方法
                    }
                }

                //若处于太吾参加比武招亲的环节，且请求检查性别的人物为太吾
                if (ForFemaleTaiwuJoin && actorId == DateFile.instance.MianActorID())
                {
                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        Main.Logger.LogInfo("参加比武招亲中，尝试为太吾模糊性别");
                    }

                    __result = "1";     //性别模糊为男性（不然女性无法参加比武招亲时，不会选择参加比武招亲）
                    return false;       //跳过原方法
                }
            }
            //继续执行原方法
            return true;
        }
    }
}