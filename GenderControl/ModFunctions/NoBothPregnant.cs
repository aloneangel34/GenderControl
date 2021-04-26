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
    /// 用于解决“开启性别模糊后，由于可以双方同时怀孕，导致NPC生孩子数量过多的问题”
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
    public static class NoBothPregnant
    {
        public static bool IsSkipped = false;
        /// <summary>
        /// （高优先）当怀孕判定的双方之中，已有怀孕者时（包括一方处于怀孕冷却期），强制不再怀孕
        /// </summary>
        /// <param name="__result">原方法的返回值（是否成功怀孕）</param>
        /// <param name="fatherId">父方人物ID</param>
        /// <param name="motherId">母方人物ID</param>
        /// <returns>是否继续执行原方法</returns>
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]    //将本补丁的优先级调高
        private static bool AISetChildrenPrefix(ref bool __result, int fatherId, int motherId)
        //public bool AISetChildren(int fatherId, int motherId, int setFather, int setMother)
        {
            //若 模糊性别判定功能 开关开启（此时母方不一定为女性，父方也不一定为男性），则进行修正
            if (Main.Setting.obscureGender.Value)
            {
                //如果参与怀孕判定的双方之中，已有人怀有身孕（包括一方处于怀孕冷却期），【强制判定为不怀孕，跳过原方法】
                if (DateFile.instance.HaveLifeDate(fatherId, 901) || DateFile.instance.HaveLifeDate(motherId, 901))
                {
                    //记录（用于告知SpecifyPregnantSide.AISetChildrenPostfix：“由于本补丁跳过『指定怀孕方的前置补丁』不算是非预期错误”）
                    IsSkipped = true;

                    #region 由于原方法会被跳过，补上原方法中“就算怀孕判定失败也必定会被执行的”代码段
                    //双方变为毁阴杂阳
                    DateFile.instance.ChangeActorFeature(fatherId, 4001, 4002);
                    DateFile.instance.ChangeActorFeature(motherId, 4001, 4002);

                    //春宵一刻事件？
                    GEvent.OnEvent(eEvents.Copulate, new object[] { fatherId, motherId });
                    #endregion

                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        QuickLogger.Log(LogLevel.Info, "一方已有身孕、怀孕判定强制失败 父方ID:{0} 身孕:{2} | 母方ID:{1} 身孕:{3}", fatherId, motherId, DateFile.instance.HaveLifeDate(fatherId, 901), DateFile.instance.HaveLifeDate(motherId, 901));
                    }

                    //将原方法的返回值设为false（即没有怀孕）
                    __result = false;
                    //不再执行原方法（也会同时跳过，对AISetChildren的前置补丁中、所有优先级低于本前置补丁的其余前置补丁）
                    return false;
                }
                //双方都未怀孕，则继续执行原方法
            }

            //继续执行原方法
            return true;
        }
    }
}
