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
    /// 性别模糊启用时机补充：获取男媒女妁的适任人选时
    /// </summary>
    [HarmonyPatch(typeof(ui_MessageWindow), "GetActor")]
    public static class NeedWorkingCheckMatchmakingActorWindows
    {
        static int _recoverPatchActorId = 0;
        static bool[] _needRecover = { false, false };

        /// <summary>
        /// 在获取男媒女妁的适任人选前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        [HarmonyPrefix]
        private static void GetActorPrefix(ui_MessageWindow __instance)
        //原方法签名
        //private void GetActor()
        {
            //若 调用本方法时，EventId为900600012（“男媒女妁”选项），【尝试暂时开启性别模糊】
            if (__instance.massageItemTyp == 900600012)
            {
                //没有开启的话，暂时开启
                if (ObscureGenderHarmony.NeedPacth == false)
                {
                    ObscureGenderHarmony.NeedPacth = true;                              //将需要补丁设为是（性别模糊）
                    _needRecover[0] = true;                                             //告知需要在原方法结束后，再把NeedPacth关掉
                }

                _recoverPatchActorId = Settings.PatchActorID;                           //记录原本的“行为主动方”用于还原
                Settings.PatchActorID = MessageEventManager.Instance.MainEventData[1];  //行为主动方：重设为EVENT对话者
                _needRecover[1] = true;                                                 //记录变更了PatchActorID
            }
        }

        /// <summary>
        /// 在获取男媒女妁的适任人选后，关闭性别模糊
        /// </summary>
        [HarmonyPostfix]
        private static void GetActorPostfix()
        //原方法签名
        //private void GetActor()
        {
            //若之前开启了NeedPacth
            if (_needRecover[0])
            {
                ObscureGenderHarmony.NeedPacth = false;             //将需要补丁设为否（并没有实际卸载补丁）
                _needRecover[0] = false;
            }

            //若之前变更了PatchActorID
            if (_needRecover[1])
            {
                Settings.PatchActorID = _recoverPatchActorId;       //行为主动方：还原
                _needRecover[1] = false;
            }
        }
    }
}