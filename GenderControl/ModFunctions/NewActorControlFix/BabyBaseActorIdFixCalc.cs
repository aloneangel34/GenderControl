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
    /// 计算婴儿的BaseActorId修正值。用于修复因“传入的父母不对应原方法所设计的性别，导致BaseActorId计算出错”的问题
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeNewChildren")]
    public static class BabyBaseActorIdFixCalc
    {
        /// <summary>
        /// 婴儿BaseActorId的修正值（0 表示不修正）
        /// </summary>
        static int _babyBaseActorIdFixValue = 0;

        /// <summary>
        /// （属性）婴儿BaseActorId的修正值（0 表示不修正）
        /// </summary>
        public static int BabyBaseActorIdFixValue
        {
            get { return _babyBaseActorIdFixValue; }
            private set { _babyBaseActorIdFixValue = value; }
        }

        /// <summary>
        /// 方法调用前，计算修正值
        /// </summary>
        /// <param name="__instance">原方法的当前实例</param>
        /// <param name="__result">原方法返回的新生儿列表</param>
        /// <param name="fatherId">父方人物ID</param>
        /// <param name="motherId">母方人物ID</param>
        /// <param name="setFather">是否添加父方关系</param>
        /// <param name="setMother">是否添加母方关系</param>
        /// <param name="gongId">所属门派ID</param>
        /// <param name="gongLevel">所属门派阶级</param>
        /// <param name="baseActorId">基础人物ID，无父无母需要</param>
        /// <param name="childrenValue">孩子精纯数据列表</param>
        [HarmonyPrefix]
        private static void MakeNewChildrenPrefix(DateFile __instance, int fatherId, int motherId, bool setFather, bool setMother)
        //原方法的签名（参照用）
        //public List<int> MakeNewChildren(int fatherId, int motherId, bool setFather, bool setMother, int gongId = 0, int gongLevel = 0, int baseActorId = 0, List<int> childrenValue = null)
        {
            bool selfAntiGenderObscure = false;             //用于记录的参数

            //若调用该补丁时，本MOD的性别模糊正处于实际启用
            if (ObscureGenderHarmony.NeedPacth)
            {
                selfAntiGenderObscure = true;               //记录在开头已经改变了
                ObscureGenderHarmony.NeedPacth = false;     //在本补丁内暂时禁用性别模糊
            }

            //若有父方，且设置父方为true，【优先按照父方的BaseActorID设置】
            if (setFather && fatherId > 0)
            {
                //按照父方设置时：
                //若父方的BaseActorID为偶数/双数（通常为女性），【需要修正】
                if (int.Parse(__instance.GetActorDate(fatherId, 997, false)) % 2 == 0)
                {
                    BabyBaseActorIdFixValue = -1;           //原方法计算出的baseActorId 需要 -1

                    if (Main.Setting.debugMode.Value)
                    {
                        QuickLogger.Log(LogLevel.Info, "决定婴儿BaseActorID的父方（ID:{0}）的BaseActorID为{1}（是偶数/双数）、其性别为{2}性，婴儿BaseActorID的修正值设为{3}。", fatherId, __instance.GetActorDate(fatherId, 997, false), (int.Parse(__instance.GetActorDate(fatherId, 14, false)) == 1) ? "1男" : "2女", BabyBaseActorIdFixValue);
                    }
                }
                //父方的BaseActorID为奇数/单数（通常为男性）
                else
                {
                    BabyBaseActorIdFixValue = 0;            //不用修正
                }
            }
            //若有母方，且设置母方为true，【再次按照母方的BaseActorID设置】
            else if (setMother && motherId > 0)
            {
                //按照母方设置时：
                //若母方的BaseActorID为奇数/单数（通常为男性），【需要修正】
                if (int.Parse(__instance.GetActorDate(motherId, 997, false)) % 2 == 1)
                {
                    BabyBaseActorIdFixValue = 1;            //原方法计算出的baseActorId 需要 +1

                    if (Main.Setting.debugMode.Value)
                    {
                        QuickLogger.Log(LogLevel.Info, "决定婴儿BaseActorID的母方（ID:{0}）的BaseActorID为{1}（是奇数/单数）、其性别为{2}性，婴儿BaseActorID的修正值设为{3}。", motherId, __instance.GetActorDate(motherId, 997, false), (int.Parse(__instance.GetActorDate(motherId, 14, false)) == 1) ? "1男" : "2女", BabyBaseActorIdFixValue); 
                    }
                }
                //母方的BaseActorID为偶数/双数（通常为女性）
                else
                {
                    BabyBaseActorIdFixValue = 0;            //不用修正
                }
            }
            //没有可设定的双亲
            else
            {
                BabyBaseActorIdFixValue = 0;                //不用修正
            }

            //若在补丁开始时暂时禁用了性别模糊
            if (selfAntiGenderObscure)
            {
                ObscureGenderHarmony.NeedPacth = true;      //在结束时重新启用性别模糊
            }
        }


        /// <summary>
        /// 方法调用后，重置修正值
        /// </summary>
        [HarmonyPostfix]
        private static void MakeNewChildrenPostfix(List<int> __result, int fatherId, int motherId, bool setFather, bool setMother)
        //原方法的签名（参照用）
        //public List<int> MakeNewChildren(int fatherId, int motherId, bool setFather, bool setMother, int gongId = 0, int gongLevel = 0, int baseActorId = 0, List<int> childrenValue = null)
        {
            if (Main.Setting.debugMode.Value && BabyBaseActorIdFixValue != 0 && ((setFather && fatherId > 0) || (setMother && motherId > 0)))
            {
                QuickLogger.Log(LogLevel.Info, "父方ID:{0} 和 母方ID:{1} 共生了{2}个孩子，孩子的BaseActorId已修正。", fatherId, motherId, __result.Count);
            }

            BabyBaseActorIdFixValue = 0;                    //方法调用后、不论如何修正值重置为0
        }
    }
}
