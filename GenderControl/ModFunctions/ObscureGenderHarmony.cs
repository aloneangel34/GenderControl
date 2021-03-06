using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;
using HarmonyLib;
using BepInEx.Logging;

namespace GenderControl
{
    /// <summary>
    /// 模糊人物性别，以便同性NPC能够实行异性之间才能做的行为（不会实际影响人物数据）
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetActorDate")]
    public static class ObscureGenderHarmony
    {
        public static bool NeedPacth = false;
#if false
        public static bool ForFemaleTaiwuJoin = false;
#endif

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
            if (key == 14 && Main.Setting.obscureGender.Value)
            {
#region 补丁应该没问题，有需要再启用调试
                //调试信息
                //if (Main.Setting.debugMode.Value)
                //{
                //    QuickLogger.Log(LogLevel.Info, "性别模糊GetActorDate方法补丁 实际生效:{0} actorId:{1}，当前行动主动方ID:{2}。", NeedPacth, actorId, Settings.PatchActorID);
                //}
#endregion

                //若处于NPC过月行动的方法循环中，且要求获取的人物属性为“性别14”
                if (NeedPacth)
                {
                    //如果人物ID为“正在设置过月行动的NPC”
                    if (Settings.PatchActorID == actorId)
                    {
#region 补丁应该没问题，有需要再启用调试
                        //调试信息
                        //if (Main.Setting.debugMode.Value)
                        //{
                        //    QuickLogger.Log(LogLevel.Info, "actorId:{0} 模糊为男性 当前行动主动方ID:{1}。", actorId, Settings.PatchActorID);
                        //}
#endregion

                        __result = "1";     //性别模糊为男性（不然女性NPC行动时，不会选择参加比武招亲）
                        return false;       //跳过原方法
                    }
                    else
                    {
#region 补丁应该没问题，有需要再启用调试
                        //调试信息
                        //if (Main.Setting.debugMode.Value)
                        //{
                        //    QuickLogger.Log(LogLevel.Info, "actorId:{0} 模糊为女性 当前行动主动方ID:{1}。", actorId, Settings.PatchActorID);
                        //}
#endregion

                        __result = "2";     //性别模糊为女性（这样“行动者”与“被互动方”互为异性。双方真实性别为同性也可以正常怀孕）
                        return false;       //跳过原方法
                    }
                }
#if false
                //若处于太吾参加比武招亲的环节，且请求检查性别的人物为太吾
                if (ForFemaleTaiwuJoin && actorId == DateFile.instance.MianActorID())
                {
                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        Main.Logger.LogInfo("太吾参加比武招亲中，为太吾模糊性别");
                    }

                    __result = "1";     //性别模糊为男性（不然女性太吾参加比武招亲时，不会选择参加比武招亲）
                    return false;       //跳过原方法
                }
#endif
            }
            //继续执行原方法
            return true;
        }
    }
}