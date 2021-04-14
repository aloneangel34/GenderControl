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

        /// <summary>
        /// 在获取男媒女妁的适任人选前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="__state">传给后方补丁的记录参数</param>
        [HarmonyPrefix]
        private static void GetActorPrefix(ui_MessageWindow __instance, out bool[] __state)
        //原方法签名
        //private void GetActor()
        {
            //记录在是否有在前置补丁中实际做改变，会传给后置补丁作为判断依据
            __state = new bool[] { false, false };

            //若 调用本方法时，EventId为900600012（“男媒女妁”选项），【尝试暂时开启性别模糊】
            if (__instance.massageItemTyp == 900600012)
            {
                    //没有开启的话，暂时开启
                    if (ObscureGenderHarmony.NeedPacth == false)
                    {
                        ObscureGenderHarmony.NeedPacth = true;                              //将需要补丁设为是（性别模糊）
                        __state[0] = true;                                                  //告知需要在原方法结束后，再把NeedPacth关掉
                    }

                    _recoverPatchActorId = Settings.PatchActorID;                           //记录原本的“行为主动方”用于还原
                    Settings.PatchActorID = MessageEventManager.Instance.MainEventData[1];  //行为主动方：重设为EVENT对话者
                    __state[1] = true;                                                      //记录变更了PatchActorID
            }
        }

        /// <summary>
        /// 在获取男媒女妁的适任人选后，关闭性别模糊
        /// </summary>
        /// <param name="__state">前方补丁传过来的记录参数</param>
        [HarmonyPostfix]
        private static void GetActorPostfix(bool[] __state)
        //原方法签名
        //private void GetActor()
        {
            //若之前开启了NeedPacth
            if (__state[0])
            {
                ObscureGenderHarmony.NeedPacth = false;             //将需要补丁设为否（并没有实际卸载补丁）
            }

            //若之前变更了PatchActorID
            if (__state[1])
            {
                Settings.PatchActorID = _recoverPatchActorId;       //行为主动方：还原
            }
        }
    }
}