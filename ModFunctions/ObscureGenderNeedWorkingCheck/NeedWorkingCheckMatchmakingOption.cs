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

        /// <summary>
        /// 在设置男媒女妁对话选项的窗口前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="baseEventDate">传进来的EventDate（会变成MainEventData）</param>
        /// <param name="chooseId">所选EventId</param>
        /// <param name="__state">传给后方补丁的记录参数</param>
        [HarmonyPrefix]
        private static void SetMassageWindowPrefix(int[] baseEventDate, int chooseId, out bool[] __state)
        //原方法的签名（参照用）
        //private void SetMassageWindow(int[] baseEventDate, int chooseId)
        {
            //记录在是否有在前置补丁中实际做改变，会传给后置补丁作为判断依据
            __state = new bool[] { false, false };
            bool isOk = true;

            //若 传入的所选EventId为正 且 其对应的链接事件ID为-9006（“对话”中的“亲近”选项），【尝试暂时开启性别模糊】
            if (chooseId > 0 && (isOk = int.TryParse(DateFile.instance.eventDate[chooseId][7] , out int linkEventId)) && linkEventId == -9006)
            {
                //若尝试获取数据成功则继续
                if (int.TryParse(DateFile.instance.eventDate[(baseEventDate[2])][2], out int idNumber))
                {
                    //获取对话人物的ID
                    int targetActorId = (idNumber == 0) ? baseEventDate[1] : ((idNumber == -1) ? DateFile.instance.MianActorID() : idNumber);

                    //没有开启的话，暂时开启
                    if (ObscureGenderHarmony.NeedPacth == false)
                    {
                        ObscureGenderHarmony.NeedPacth = true;      //将需要补丁设为是（性别模糊）
                        __state[0] = true;                          //告知需要在原方法结束后，再把NeedPacth关掉
                    }

                    _recoverPatchActorId = Settings.PatchActorID;   //记录原本的“行为主动方”用于还原
                    Settings.PatchActorID = targetActorId;          //行为主动方：重设为传入的第一个人物为
                    __state[1] = true;                              //记录变更了PatchActorID
                }
                else if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Error, "在SetMassageWindow方法（男媒女妁）的性别修正中，无法获取eventDate[(baseEventDate[2])][2]的有效数据，未做修正。baseEventDate:{0} baseEventDate[2]:{1}", baseEventDate, baseEventDate[2]);
                }
            }
            else if(!isOk && Main.Setting.debugMode.Value)
            {
                QuickLogger.Log(LogLevel.Error, "在SetMassageWindow方法（男媒女妁）的性别修正中，无法获取eventDate[chooseId][7]的有效数据，未做修正。chooseId:{0}", chooseId);
            }
        }

        /// <summary>
        /// 在设置男媒女妁对话选项的窗口后，关闭性别模糊
        /// </summary>
        /// <param name="__state">前方补丁传过来的记录参数</param>
        [HarmonyPostfix]
        private static void SetMassageWindowPostfix(bool[] __state)
        //原方法的签名（参照用）
        //private void SetMassageWindow(int[] baseEventDate, int chooseId)
        {
            //若之前开启了NeedPacth
            if (__state[0])
            {
                ObscureGenderHarmony.NeedPacth = false;                 //将需要补丁设为否（并没有实际卸载补丁）
            }

            //若之前变更了PatchActorID
            if (__state[1])
            {
                Settings.PatchActorID = _recoverPatchActorId;               //行为主动方：还原
            }
        }
    }
}