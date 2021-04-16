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
        static bool[] _needRecover = { false, false };

        /// <summary>
        /// 太吾表白事件调用前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        [HarmonyPrefix]
        private static void EndEvent90011Prefix(MessageEventManager __instance)
        //原方法的签名（参照用）
        //private void EndEvent9001_1()
        {
            //若调用该事件时，选项为6（太吾尝试表白）
            if (__instance.EventValue[1] == 6)
            {
                //若性别模糊处于未开启的状态
                if (ObscureGenderHarmony.NeedPacth == false)
                {
                    ObscureGenderHarmony.NeedPacth = true;                  //性别模糊设为实际启用
                    _needRecover[0] = true;                                 //告知需要在原方法结束后，再把NeedPacth关掉
                }

                _recoverPatchActorId = Settings.PatchActorID;               //记录原本的“行为主动方”用于还原
                Settings.PatchActorID = DateFile.instance.MianActorID();    //行为主动方：重设为太吾
                _needRecover[1] = true;
            }
        }

        /// <summary>
        /// 太吾表白事件调用后，关闭性别模糊
        /// </summary>
        [HarmonyPostfix]
        private static void EndEvent90011Postfix()
        //原方法的签名（参照用）
        //private void EndEvent9001_1()
        {
            //若之前开启了NeedPacth
            if (_needRecover[0])
            {
                ObscureGenderHarmony.NeedPacth = false;                     //性别模糊设为不再启用（并没有实际卸载补丁）
                _needRecover[0] = false;
            }

            if (_needRecover[1])
            {
                Settings.PatchActorID = _recoverPatchActorId;               //行为主动方：还原
                _needRecover[1] = false;
            }
        }
    }
}