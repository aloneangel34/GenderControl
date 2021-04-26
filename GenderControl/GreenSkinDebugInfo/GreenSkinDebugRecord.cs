using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx.Logging;


namespace GenderControl
{
    //早期用来查看各项功能是否生效用的。不然心里比较没底
    #if DEBUG

    /// <summary>
    /// LogInfo输出同性NPC之间，表白成功几率
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AIGetLove")]
    public static class GreenSkinDebugRecordP1
    {
        /// <summary>
        /// LogInfo输出同性NPC之间，表白成功几率
        /// </summary>
        /// <param name="__result">原方法的返回值（表白者爱上被表白者的几率）</param>
        /// <param name="actorId">表白者</param>
        /// <param name="loverId">被表白者</param>
        [HarmonyPostfix]
        private static void Postfix(int __result, int actorId, int loverId)
        //private int AIGetLove(int actorId, int loverId)
        {
            if (Main.Setting.debugMode.Value)
            {
                bool selfAntiGenderObscure = false;             //用于记录的参数

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (ObscureGenderHarmony.NeedPacth)
                {
                    selfAntiGenderObscure = true;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = false;     //在本补丁内暂时禁用性别模糊
                }

                QuickLogger.Log(LogLevel.Info, "actorId1:{0},性别:{1}尝试向 actorId2:{2},性别:{3}表白。成功几率:{4} NeedPacth:{5}", actorId, DateFile.instance.GetActorDate(actorId, 14, false), loverId, DateFile.instance.GetActorDate(loverId, 14, false), __result, selfAntiGenderObscure) ;

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (selfAntiGenderObscure)
                {
                    selfAntiGenderObscure = false;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = true;     //在本补丁内暂时禁用性别模糊
                }
            }
        }
    }

    /// <summary>
    /// LogInfo输出同性NPC之间是否有结为两情相悦
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "AddSocial")]
    public static class GreenSkinDebugRecordP2
    {
        /// <summary>
        /// LogInfo输出同性NPC之间是否有结为两情相悦
        /// </summary>
        /// <param name="__result">原方法的返回值（返回的具体是啥没去看，太累了，大约和是否添加成功有关？）</param>
        /// <param name="actorId1">第一个人物ID</param>
        /// <param name="actorId2">第二个人物ID</param>
        /// <param name="scoialTyp">要添加的关系类型</param>
        [HarmonyPostfix]
        private static void Postfix(int __result, int actorId1, int actorId2, int scoialTyp)
        //public int AddSocial(int actorId1, int actorId2, int scoialTyp)
        {
            if (scoialTyp == 306 && Main.Setting.debugMode.Value)
            {
                bool selfAntiGenderObscure = false;             //用于记录的参数

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (ObscureGenderHarmony.NeedPacth)
                {
                    selfAntiGenderObscure = true;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = false;     //在本补丁内暂时禁用性别模糊
                }

                QuickLogger.Log(LogLevel.Info, "actorId1:{0},性别:{1} 和 actorId2:{2},性别:{3} 结为两情相悦", actorId1, DateFile.instance.GetActorDate(actorId1, 14, false), actorId2, DateFile.instance.GetActorDate(actorId2, 14, false)); 

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (selfAntiGenderObscure)
                {
                    selfAntiGenderObscure = false;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = true;     //在本补丁内暂时禁用性别模糊
                }
            }
        }
    }

    /// <summary>
    /// LogInfo输出同性NPC之间是否怀孕成功
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
    public static class GreenSkinDebugRecordP3
    {
        /// <summary>
        /// 对 PeopleLifeAI 下的 AISetChildren （NPC怀孕设置）进行 后置补丁
        /// </summary>
        /// <param name="__result">原方法的返回值（是否成功怀孕）</param>
        /// <param name="fatherId">父方人物ID</param>
        /// <param name="motherId">母方人物ID</param>
        /// <param name="setFather">是否在孩子关系中显示父方</param>
        /// <param name="setMother">是否在孩子关系中显示母方</param>
        [HarmonyPostfix]
        private static void AISetChildrenPostfix(bool __result, int fatherId, int motherId)
        //public bool AISetChildren(int fatherId, int motherId, int setFather, int setMother)
        {
            if (Main.Setting.debugMode.Value)
            {
                bool selfAntiGenderObscure = false;             //用于记录的参数

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (ObscureGenderHarmony.NeedPacth)
                {
                    selfAntiGenderObscure = true;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = false;     //在本补丁内暂时禁用性别模糊
                }

                QuickLogger.Log(LogLevel.Info, "fatherId:{0},性别:{1}试图让 motherId:{2},性别:{3}怀孕。成功？:{4} NeedPacth:{5}", fatherId, DateFile.instance.GetActorDate(fatherId, 14, false), motherId, DateFile.instance.GetActorDate(motherId, 14, false), __result, selfAntiGenderObscure);

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (selfAntiGenderObscure)
                {
                    selfAntiGenderObscure = false;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = true;     //在本补丁内暂时禁用性别模糊
                }
            }
        }
    }

    #endif
}
