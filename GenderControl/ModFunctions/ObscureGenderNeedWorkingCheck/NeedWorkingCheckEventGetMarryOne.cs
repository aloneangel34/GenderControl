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
    /// 性别模糊启用时机补充：事件设置共结连理
    /// </summary>
    /// <param name="__instance">原方法所属的实例</param>
    /// <param name="__state">传给后置补丁的参数（告知是否有将NeedPacth设为true）</param>
    /// <summary>
    /// 解读 原方法名
    /// </summary>
    [HarmonyPatch(typeof(MessageEventManager), "SetSpouseSocial")]
    public static class NeedWorkingCheckEventGetMarryOne
    {
        static int _recoverPatchActorId = 0;

        /// <summary>
        /// 事件设置共结连理调用前，开启性别模糊
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="actorId1">第1个人物ID</param>
        /// <param name="actorId2">第2个人物ID</param>
        /// <param name="changeEventId">转向事件ID</param>
        /// <param name="__state">传给后置补丁的参数（告知是否有将NeedPacth设为true）</param>
        [HarmonyPrefix]
        public static void SetSpouseSocialPrefix(int actorId1, int actorId2, out bool __state)
        //原方法的签名（参照用）
        //public void SetSpouseSocial(int actorId1, int actorId2, int changeEventId)
        {
            __state = false;

            //若性别模糊未启用，且 传入的两个人物的性别相同时，【暂时开启性别模糊】
            if (ObscureGenderHarmony.NeedPacth == false && DateFile.instance.GetActorDate(actorId1, 14, false) == DateFile.instance.GetActorDate(actorId2, 14, false))
            {
                ObscureGenderHarmony.NeedPacth = true;                  //将需要补丁设为是（性别模糊）
                __state = true;                                         //告知需要在原方法结束后，再把NeedPacth关掉
            }

            _recoverPatchActorId = Settings.PatchActorID;               //记录原本的“行为主动方”用于还原
            Settings.PatchActorID = actorId1;                           //行为主动方：重设为传入的第一个人物为
        }

        /// <summary>
        /// 事件设置共结连理调用后，关闭性别模糊
        /// </summary>
        /// <param name="__state">前置补丁传过来的参数（告知是否有将NeedPacth设为true）</param>
        [HarmonyPostfix]
        public static void SetSpouseSocialPostfix(bool __state)
        //原方法的签名（参照用）
        //public void SetSpouseSocial(int actorId1, int actorId2, int changeEventId)
        {
            //若之前开启了NeedPacth
            if (__state)
            {
                ObscureGenderHarmony.NeedPacth = false;                 //将需要补丁设为否（并没有实际卸载补丁）
            }
            
            Settings.PatchActorID = _recoverPatchActorId;               //行为主动方：还原
        }
    }
}