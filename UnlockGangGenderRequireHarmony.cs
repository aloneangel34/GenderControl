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
    /// 功能：解除门派对性别要求的限制（HarmonyPatch）
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetGangDate")]
    public static class UnlockGangGenderRequireHarmony
    {
        /// <summary>
        /// 对 DateFile 下的 GetGangDate （获取势力数据）进行 前置拦截补丁（用于模糊门派性别要求）
        /// </summary>
        /// <param name="__result">原方法的指定势力数据的值（字符串）</param>
        /// <param name="id">势力ID</param>
        /// <param name="index">势力数据索引ID</param>
        /// <returns>该补丁执行完后是否继续执行原方法</returns>
        [HarmonyPrefix]
        public static bool GetGangDatePrefix(ref string __result, int id, int index)
        //public string GetGangDate(int id, int index)  //原方法的声明，用于对照
        {
            //若要求获取的势力数据为“门派性别要求5”
            if (index == 5)
            {
                //调试信息
                //if (Main.Setting.debugMode.Value)
                //{
                //    Main.SB.AppendFormat("模糊门派性别限制GetGangDate方法 启用:{0} 门派ID:{1}", Main.Setting.unlockGangLevelGenderRequire.Value, id);
                //    Main.Logger.LogDebug(Main.SB);
                //    Main.SB.Clear();
                //}

                //代替原方法返回数值，让游戏认为该门派对性别无限制，
                if (Main.Setting.unlockGangLevelGenderRequire.Value)
                {
                    __result = "0";     //将返回值（门派性别要求）模糊为“无要求0”
                    return false;       //跳过原方法
                }
            }
            //继续执行原方法
            return true;
        }
    }
}