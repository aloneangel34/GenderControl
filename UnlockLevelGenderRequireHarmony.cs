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
    /// 功能：解除身份对性别要求的限制（HarmonyPatch）
    /// </summary>
    [HarmonyPatch(typeof(GetSprites), "GetDate")]
    public static class UnlockLevelGenderRequireHarmony
    {
        /// <summary>
        /// 游戏读取了GangGroupValue的数据后，将DateFile实例存放数据相应字段中的“身份要求性别101”项统一设为“0”（表示该身份无性别要求）
        /// </summary>
        /// <param name="dateName">数据名称</param>
        /// <param name="dateList">数据列表</param>
        /// <param name="passDateIndex"></param>
        [HarmonyPostfix]
        public static void GetDatePostfix(string dateName)
        //public void GetDate(string dateName, Dictionary<int, Dictionary<int, string>> dateList, int passDateIndex = -1)
        {
            //若 已启用解除门派身份的性别限制 且 读取的数据名为“GangGroupValue_Date”
            if (Main.Setting.unlockGangLevelGenderRequire.Value && dateName == "GangGroupValue_Date")
            {
                //调试信息
                if (Main.Setting.debugMode.Value)
                {
                    Main.Logger.LogInfo("解禁身份性别：游戏调用GetDate方法，尝试记录元数据并重写");
                }

                //清空原先的记录
                UnlockLevelGenderRequire.RecordLevelGenderRequir.Clear();
                //调用重写（若记录为空会尝试记录元数据）
                UnlockLevelGenderRequire.ResetLevelGender();
            }
        }
    }
}