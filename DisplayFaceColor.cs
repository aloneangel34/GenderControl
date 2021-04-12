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
    /// 功能：游戏显示肤色变更（主要）
    /// </summary>
    public static class DisplayFaceColor
    {

        /// <summary>
        /// 游戏原版的显示肤色
        /// </summary>
        public static Color[] _defalutColors = new Color[]
        {
            new Color(255f / 255f, 220f / 255f, 200f / 255f),
            new Color(250f / 255f, 200f / 255f, 175f / 255f),
            new Color(245f / 255f, 185f / 255f, 155f / 255f),
            new Color(240f / 255f, 180f / 255f, 140f / 255f),
            new Color(235f / 255f, 170f / 255f, 135f / 255f),
            new Color(230f / 255f, 160f / 255f, 115f / 255f),
            new Color(225f / 255f, 150f / 255f, 110f / 255f),
            new Color(190f / 255f, 115f / 255f, 75f / 255f),
            new Color(160f / 255f, 100f / 255f, 65f / 255f),
            new Color(130f / 255f, 80f / 255f, 55f / 255f)
        };

        /// <summary>
        /// 较深的显示肤色
        /// </summary>
        public static Color[] _deeperColors = new Color[]
        {
            new Color(200f / 255f, 148f / 255f, 123f / 255f),
            new Color(194f / 255f, 138f / 255f, 112f / 255f),
            new Color(189f / 255f, 129f / 255f, 100f / 255f),
            new Color(184f / 255f, 119f / 255f, 88f / 255f),
            new Color(180f / 255f, 109f / 255f, 75f / 255f),
            new Color(167f / 255f, 102f / 255f, 71f / 255f),
            new Color(155f / 255f, 95f / 255f, 66f / 255f),
            new Color(143f / 255f, 87f / 255f, 61f / 255f),
            new Color(132f / 255f, 80f / 255f, 55f / 255f),
            new Color(119f / 255f, 73f / 255f, 51f / 255f)
        };

        /// <summary>
        /// 较浅的显示肤色
        /// </summary>
        public static Color[] _lighterColors = new Color[]
        {
            new Color(255f / 255f, 220f / 255f, 200f / 255f),
            new Color(250f / 255f, 213f / 255f, 187f / 255f),
            new Color(245f / 255f, 200f / 255f, 166f / 255f),
            new Color(240f / 255f, 193f / 255f, 149f / 255f),
            new Color(235f / 255f, 183f / 255f, 132f / 255f),
            new Color(230f / 255f, 172f / 255f, 119f / 255f),
            new Color(225f / 255f, 161f / 255f, 114f / 255f),
            new Color(220f / 255f, 151f / 255f, 101f / 255f),
            new Color(215f / 255f, 140f / 255f, 96f / 255f),
            new Color(210f / 255f, 129f / 255f, 91f / 255f)
        };

        /// <summary>
        /// 改变DateFile中保存的faceColor[0]的赋值
        /// </summary>
        /// <param name="colorsId">色号 0为原版，1为较浅，2为较深，其他无效</param>
        public static void ChangeDisplayColor(int colorsId)
        {
            if (DateFile.instance != null)
            {
                switch (colorsId)
                {
                    //变回原版
                    case 0:
                        DateFile.instance.faceColor[0] = _defalutColors;
                        break;
                    //变为较深
                    case 1:
                        DateFile.instance.faceColor[0] = _deeperColors;
                        break;
                    //变为较浅
                    case 2:
                        DateFile.instance.faceColor[0] = _lighterColors;
                        break;
                    default:
                        //调试信息
                        if (Main.Setting.debugMode.Value)
                        {
                            Main.SB.AppendFormat("显示肤色设定值{0}无效（0原版、1较深、2较浅），变更失败", colorsId);
                            Main.Logger.LogError(Main.SB.ToString());
                            Main.SB.Clear();
                        }
                        break;
                }
            }
            else
            {
                //调试信息
                if (Main.Setting.debugMode.Value)
                {
                    Main.Logger.LogError("DateFile实例不存在，显示肤色变更失败");
                }
            }
        }


        ///// <summary>
        ///// 较浅的显示肤色
        ///// </summary>
        //public static Color[] LighterColors = new Color[]
        //{
        //    new Color(255f / 255f, 220f / 255f, 200f / 255f),
        //    new Color(250f / 255f, 213f / 255f, 187f / 255f),
        //    new Color(245f / 255f, 200f / 255f, 166f / 255f),
        //    new Color(240f / 255f, 189f / 255f, 149f / 255f),
        //    new Color(235f / 255f, 179f / 255f, 132f / 255f),
        //    new Color(230f / 255f, 168f / 255f, 115f / 255f),
        //    new Color(225f / 255f, 157f / 255f, 106f / 255f),
        //    new Color(220f / 255f, 147f / 255f, 89f / 255f),
        //    new Color(215f / 255f, 136f / 255f, 80f / 255f),
        //    new Color(210f / 255f, 125f / 255f, 71f / 255f)
        //};

    }
}