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
    /// AIGetLove （计算表白者爱上被表白者的几率）的Patch补丁
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AIGetLove")]
    public static class PeopleLifeAI_AIGetLoveP1
    {
        /// <summary>
        /// 对 PeopleLifeAI 下的 AIGetLove （计算表白者爱上被表白者的几率）进行 后置补丁
        /// </summary>
        /// <param name="__result">原方法的返回值（表白者爱上被表白者的几率）</param>
        /// <param name="actorId">表白者</param>
        /// <param name="loverId">被表白者</param>
        /// <returns></returns>
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

                Main.SB.AppendFormat("actorId1:{0},性别:{1} 尝试向 actorId2:{2},性别:{3} 表白。成功几率:{4}", actorId, DateFile.instance.GetActorDate(actorId, 14, false), loverId, DateFile.instance.GetActorDate(loverId, 14, false), __result) ;
                Main.Logger.LogDebug(Main.SB);
                Main.SB.Clear();

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
    /// 
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "AddSocial")]
    public static class GreenSkinDebugRecordP2
    {
        /// <summary>
        /// 
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

                Main.SB.AppendFormat("actorId1:{0},性别:{1} 和 actorId2:{2},性别:{3} 结为两情相悦", actorId1, DateFile.instance.GetActorDate(actorId1, 14, false), actorId2, DateFile.instance.GetActorDate(actorId2, 14, false)); 
                Main.Logger.LogDebug(Main.SB);
                Main.SB.Clear();

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
    /// AISetChildren （NPC怀孕设置）的Patch补丁
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
        /// <returns>补丁方式的返回值类型，Prefix 为 bool，Postfix 为 void</returns>
        [HarmonyPostfix]
        private static void Postfix(bool __result, int fatherId, int motherId, int setFather, int setMother)
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

                Main.SB.AppendFormat("fatherId:{0},性别:{1} 试图让 motherId:{2},性别:{3} 怀孕。是否成功:{4}", fatherId, DateFile.instance.GetActorDate(fatherId, 14, false), motherId, DateFile.instance.GetActorDate(motherId, 14, false), __result);
                Main.Logger.LogDebug(Main.SB);
                Main.SB.Clear();

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (selfAntiGenderObscure)
                {
                    selfAntiGenderObscure = false;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = true;     //在本补丁内暂时禁用性别模糊
                }
            }
        }
    }
}
