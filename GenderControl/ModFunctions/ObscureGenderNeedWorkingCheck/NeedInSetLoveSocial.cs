using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;
using HarmonyLib;

namespace GenderControl
{
    /// <summary>
    /// 性别模糊启用时机补充：SetLoveSocial（仅用于处理流言蜚语）
    /// </summary>
    [HarmonyPatch(typeof(MessageEventManager), "SetLoveSocial")]
    public static class NeedInSetLoveSocial
    {
        static int _recoverPatchActorId = 0;
        static bool _needRecover = false;

        /// <summary>
        /// SetLoveSocial调用前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="actorId1">第1个人物ID</param>
        /// <param name="actorId2">第2个人物ID</param>
        /// <param name="changeEventId">转向事件ID</param>
        [HarmonyPrefix]
        public static void SetLoveSocialPrefix(int actorId1, int actorId2)
        //原方法的签名（参照用）
        //public void SetLoveSocial(int actorId1, int actorId2, int changeEventId)
        {
            //若性别模糊未启用，且 传入的两个人物的性别相同时，【暂时开启性别模糊】
            if (ObscureGenderHarmony.NeedPacth == false && DateFile.instance.GetActorDate(actorId1, 14, false) == DateFile.instance.GetActorDate(actorId2, 14, false))
            {
                ObscureGenderHarmony.NeedPacth = true;                  //将需要补丁设为是（性别模糊）
                _needRecover = true;                                    //告知需要在原方法结束后，再把NeedPacth关掉
            }

            _recoverPatchActorId = Settings.PatchActorID;               //记录原本的“行为主动方”用于还原
            Settings.PatchActorID = actorId1;                           //行为主动方：重设为传入的第一个人物为
        }

        /// <summary>
        /// SetLoveSocial调用后，关闭性别模糊
        /// </summary>
        [HarmonyPostfix]
        public static void SetLoveSocialPostfix()
        //原方法的签名（参照用）
        //public void SetLoveSocial(int actorId1, int actorId2, int changeEventId)
        {
            //若之前开启了NeedPacth
            if (_needRecover)
            {
                ObscureGenderHarmony.NeedPacth = false;                 //将需要补丁设为否（并没有实际卸载补丁）
                _needRecover = false;
            }
            
            Settings.PatchActorID = _recoverPatchActorId;               //行为主动方：还原
        }
    }
}