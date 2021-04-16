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
    /// 性别模糊启用时机补充：判定男媒女妁选项是否显现时
    /// </summary>
    [HarmonyPatch(typeof(ui_MessageWindow), "SetMassageWindow")]
    public static class NeedWorkingCheckMatchmakingOption
    {
        static int _recoverPatchActorId = 0;
        static bool[] _needRecover = { false, false };

        /// <summary>
        /// 在设置男媒女妁对话选项的窗口前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="baseEventDate">传进来的EventDate（会变成MainEventData）</param>
        /// <param name="chooseId">所选EventId</param>
        [HarmonyPrefix]
        private static void SetMassageWindowPrefix(int[] baseEventDate, int chooseId)
        //原方法的签名（参照用）
        //private void SetMassageWindow(int[] baseEventDate, int chooseId)
        {
            //若 传入的所选EventId为正 且 eventDate有对应的数据 且 数据中对应的链接事件ID为-9006（“对话”中的“亲近”选项），【尝试暂时开启性别模糊】
            if (chooseId > 0 && DateFile.instance.eventDate.ContainsKey(chooseId) && int.TryParse(DateFile.instance.eventDate[chooseId][7] , out int linkEventId) && linkEventId == -9006)
            {
                //若 eventDate有对应的数据 且 尝试获取数据成功则继续
                if (DateFile.instance.eventDate.ContainsKey(baseEventDate[2]) && int.TryParse(DateFile.instance.eventDate[(baseEventDate[2])][2], out int idNumber))
                {
                    //获取对话人物的ID
                    int targetActorId = (idNumber == 0) ? baseEventDate[1] : ((idNumber == -1) ? DateFile.instance.MianActorID() : idNumber);

                    //没有开启的话，暂时开启
                    if (ObscureGenderHarmony.NeedPacth == false)
                    {
                        ObscureGenderHarmony.NeedPacth = true;      //将需要补丁设为是（性别模糊）
                        _needRecover[0] = true;                     //告知需要在原方法结束后，再把NeedPacth关掉
                    }

                    _recoverPatchActorId = Settings.PatchActorID;   //记录原本的“行为主动方”用于还原
                    Settings.PatchActorID = targetActorId;          //行为主动方：重设为传入的第一个人物为
                    _needRecover[1] = true;                         //记录变更了PatchActorID
                }
                else if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Error, "在SetMassageWindow方法（男媒女妁）的性别修正中，无法获取eventDate[(baseEventDate[2])][2]的有效数据，未做修正。baseEventDate:{0} baseEventDate[2]:{1}", baseEventDate, baseEventDate[2]);
                }
            }
        }

        /// <summary>
        /// 在设置男媒女妁对话选项的窗口后，关闭性别模糊
        /// </summary>
        [HarmonyPostfix]
        private static void SetMassageWindowPostfix()
        //原方法的签名（参照用）
        //private void SetMassageWindow(int[] baseEventDate, int chooseId)
        {
            //若之前开启了NeedPacth
            if (_needRecover[0])
            {
                ObscureGenderHarmony.NeedPacth = false;                 //将需要补丁设为否（并没有实际卸载补丁）
                _needRecover[0] = false;
            }

            //若之前变更了PatchActorID
            if (_needRecover[1])
            {
                Settings.PatchActorID = _recoverPatchActorId;               //行为主动方：还原
                _needRecover[1] = false;
            }
        }
    }
}