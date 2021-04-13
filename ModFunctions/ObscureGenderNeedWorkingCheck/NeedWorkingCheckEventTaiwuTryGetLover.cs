using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GenderControl
{
    /// <summary>
    /// 性别模糊启用时机补充：太吾主动表白判定时
    /// </summary>
    [HarmonyPatch(typeof(MessageEventManager), "EndEvent9001_1")]
    public static class NeedWorkingCheckTaiwuTryGetLover
    {
        static int _recoverPatchActorId = 0;

        /// <summary>
        /// 太吾表白事件调用前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="__state">传给后置补丁的记录参数</param>
        [HarmonyPrefix]
        private static void EndEvent90011Prefix(MessageEventManager __instance, out bool[] __state)
        //原方法的签名（参照用）
        //private void EndEvent9001_1()
        {
            __state = new bool[] { false, false };

            //若调用该事件时，选项为6（太吾尝试表白）
            if (__instance.EventValue[1] == 6)
            {
                //若性别模糊处于未开启的状态
                if (ObscureGenderHarmony.NeedPacth == false)
                {
                    ObscureGenderHarmony.NeedPacth = true;                      //性别模糊设为实际启用
                    __state[0] = true;                                             //告知需要在原方法结束后，再把NeedPacth关掉
                }

                _recoverPatchActorId = Settings.PatchActorID;               //记录原本的“行为主动方”用于还原
                Settings.PatchActorID = DateFile.instance.MianActorID();    //行为主动方：重设为太吾
                __state[1] = true;
            }
        }

        /// <summary>
        /// 太吾表白事件调用后，关闭性别模糊
        /// </summary>
        /// <param name="__state">前置补丁传过来的参数</param>
        [HarmonyPostfix]
        private static void EndEvent90011Postfix(bool[] __state)
        //原方法的签名（参照用）
        //private void EndEvent9001_1()
        {
            //若之前开启了NeedPacth
            if (__state[0])
            {
                ObscureGenderHarmony.NeedPacth = false;                //性别模糊设为不再启用（并没有实际卸载补丁）
            }

            if (__state[1])
            {
                Settings.PatchActorID = _recoverPatchActorId;               //行为主动方：还原
            }
        }
    }
}