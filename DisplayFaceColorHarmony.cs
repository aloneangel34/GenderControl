using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;
using HarmonyLib;
using UnityEngine;

namespace GenderControl
{
    /// <summary>
    /// 功能：游戏显示肤色变更（HarmonyPatch）
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "GetFaceColor")]
    public static class DisplayFaceColorHarmony
    {
        /// <summary>
        /// 确保在游戏开始时，自动根据MOD设置改变相符肤色颜色（0原版则不变）
        /// </summary>
        /// 理论上原方法只会在游戏启动、读取游戏数据时被调用一次
        [HarmonyPostfix]
        public static void GetFaceColorPostfix()
        //原方法的签名（参照用）
        //public void GetFaceColor()
        {
            //若有有效设定，则自动补丁
            if (Main.Setting.displayFaceColors.Value == 1 || Main.Setting.displayFaceColors.Value == 2)
            {
                DisplayFaceColor.ChangeDisplayColor(Main.Setting.displayFaceColors.Value);  //改变肤色

                //调试信息
                if (Main.Setting.debugMode.Value)
                {
                    Main.SB.AppendFormat("游戏调用GetFaceColor方法。已尝试自动变更肤色显示为{0}（0原版、1较深、2较浅）", Main.Setting.displayFaceColors.Value);
                    Main.Logger.LogInfo(Main.SB.ToString());
                    Main.SB.Clear();
                }
            }
        }
    }
}