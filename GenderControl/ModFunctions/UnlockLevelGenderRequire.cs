using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace GenderControl
{
    /// <summary>
    /// 功能：解除身份对性别要求的限制（重写/还原presetGangGroupDateValue[101]数据）
    /// </summary>
    public static class UnlockLevelGenderRequire
    {
        /// <summary>
        /// 存放记录下来的“身份GangGroupValueId”（键）和其对应的原“身份要求性别101”（值）
        /// </summary>
        public static Dictionary<int, string> RecordLevelGenderRequir = new Dictionary<int, string>();

        /// <summary>
        /// 手动重设“身份要求性别101”的值（若记录为空，记录元数据以便还原）
        /// </summary>
        public static void ResetLevelGender()
        {
            if (DateFile.instance != null)
            {
                //调试信息
                if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Info, "解禁身份性别：重设presetGangGroupDateValue[101] 还原记录条目数:{0} 实际条目数:{1}", RecordLevelGenderRequir.Count, DateFile.instance.presetGangGroupDateValue.Count);
                }

                //若记录为空，【先保存记录】
                if (RecordLevelGenderRequir.Count == 0)
                {
                    foreach (var item in DateFile.instance.presetGangGroupDateValue)
                    {
                        RecordLevelGenderRequir[item.Key] = item.Value[101];    //将每一个“身份GangGroupValueId”的“身份要求性别101”项记录下来
                    }

                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        QuickLogger.Log(LogLevel.Debug, "已备份原“身份要求性别”数据 记录条目数:{0} 实际条目数:{1}", RecordLevelGenderRequir.Count, DateFile.instance.presetGangGroupDateValue.Count);
                    }
                }

                //开始覆盖
                foreach (var item in DateFile.instance.presetGangGroupDateValue)
                {
                    item.Value[101] = "0";      //然后把字符串“0”（表示无性别要求）重新写入“身份要求性别101”项
                }
            }
            else if(Main.Setting.debugMode.Value)
            {
                Main.Logger.LogError("解禁身份性别：尝试重设presetGangGroupDateValue[101]时失败，DateFile实例不存在");
            }
        }

        /// <summary>
        /// 尝试以记录里的元数据，还原“身份要求性别101”的值
        /// </summary>
        public static void UndoResetLevelGender()
        {
            if (DateFile.instance != null)
            {

                //调试信息
                if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Info, "解禁身份性别：尝试还原presetGangGroupDateValue[101]。还原用记录的条目数：{0},实际presetGangGroupDateValue条目数：{0}", RecordLevelGenderRequir.Count, DateFile.instance.presetGangGroupDateValue.Count);
                }

                //如果记录不为空，以记录来还原数据（若虽然字典条目不为空，但某个条目的值为空，则以字符串“0”代替）
                if (RecordLevelGenderRequir.Count > 0)
                {
                    foreach (var item in DateFile.instance.presetGangGroupDateValue)
                    {
                        item.Value[101] = (RecordLevelGenderRequir[item.Key] != null) ? RecordLevelGenderRequir[item.Key] : "0";
                    }
                }
            }
            else if (Main.Setting.debugMode.Value)
            {
                Main.Logger.LogError("解禁身份性别：尝试还原presetGangGroupDateValue[101]时失败，DateFile实例不存在");
            }
        }

    }
}