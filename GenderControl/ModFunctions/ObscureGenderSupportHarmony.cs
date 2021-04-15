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
    /// 启用性别模糊时，对尝试获取人物无法生育特性的辅助修正
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetActorFeature")]
    public static class ObscureGenderSupportHarmony
    {
        /// <summary>
        /// 性别模糊的辅助，由于游戏会按被模糊后的性别去查找“无根之人1001”或“石芯玉女1002”特性，需要做对应的修正（不会实际影响人物数据）
        /// </summary>
        /// <param name="__result">原方法的返回的人物特性列表</param>
        /// <param name="key">人物ID</param>
        [HarmonyPostfix]
        private static void GetActorFeaturePostfix(ref List<int> __result, int key)
        //原方法的签名（参照用）
        //public List<int> GetActorFeature(int key, bool getAll = false)
        {
            //性别模糊实际生效时，对原方法返回的特性列表中的【无法生育特性】做修正
            if (Main.Setting.obscureGender.Value && ObscureGenderHarmony.NeedPacth)
            {
                //如果人物ID为“正在设置过月行动的NPC” 且人物有“石芯玉女1002”特性
                if (Settings.PatchActorID == key && __result.Contains(1002))
                {
                    #region 补丁应该没问题，有需要再启用调试
                    //调试信息
                    //if (Main.Setting.debugMode.Value)
                    //{
                    //    QuickLogger.Log(LogLevel.Info, "特性读取修正 actorId:{0} 主行动者ID:{1} 原列表含：无根之人{2}、石芯玉女{3}", key, Settings.PatchActorID, __result.Contains(1001), __result.Contains(1002));
                    //}
                    #endregion

                    __result.Remove(1002);  //返回的列表中移除“石芯玉女1002”
                    __result.Add(1001);     //返回的列表中添加“无根之人1001”

                    //因为“正在设置过月行动的NPC”性别被模糊为男性，需要对“NPC情难自已”的判断处理一下
                }
                //如果人物ID属于“正在设置过月行动NPC”的“同格人物” 且人物有“无根之人1001”特性
                else if (Settings.PatchActorID != key && __result.Contains(1001))
                {
                    #region 补丁应该没问题，有需要再启用调试
                    //调试信息
                    //if (Main.Setting.debugMode.Value)
                    //{
                    //    QuickLogger.Log(LogLevel.Info, "特性读取修正 actorId:{0} 主行动者ID:{1} 原列表含：无根之人{2}、石芯玉女{3}", key, Settings.PatchActorID, __result.Contains(1001), __result.Contains(1002));
                    //}
                    #endregion

                    __result.Remove(1001);  //返回的列表中移除“无根之人1001”
                    __result.Add(1002);     //返回的列表中添加“石芯玉女1002”
                }
                //若尝试获取其他人物的特性则不对返回的列表作处理
            }
        }
    }
}