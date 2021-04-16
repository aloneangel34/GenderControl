using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;
using HarmonyLib;
using BepInEx.Logging;

#if DEBUG
namespace GenderControl
{
    //版本v0.2.0前，由于父母双方性别不一定正好为父方->男、母方->女，导致在原方法中婴儿的BaseActorId的计算异常
    //（婴儿的BaseActorId不一定和父母方所在地域一致、且有可能导致数据范围超限的问题。婴儿BaseActorId的合法范围为1～32。而BUG时会出现0、33的情况）
    //版本v0.2.0已修正。

    /// <summary>
    /// DeBug用：传出婴儿降生时的父母双方数据
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeNewChildren")]
    public static class ReportSwitchOnMakeNewChildren
    {
        public static bool IsSwitchOn = false;

        /// <summary>
        /// DateFile 实例类中 MakeNewChildren 方法的 Prefix 前置补丁（不执行原方法）
        /// </summary>
        /// <param name="__instance">原方法的当前实例</param>
        /// <param name="__result">替代原方法返回的新生儿列表</param>
        /// <param name="fatherId">父亲人物ID</param>
        /// <param name="motherId">母亲人物ID</param>
        /// <param name="setFather">是否添加父亲关系</param>
        /// <param name="setMother">是否添加母亲关系</param>
        /// <param name="gongId">所属门派ID</param>
        /// <param name="gongLevel">所属门派阶级</param>
        /// <param name="baseActorId">基础人物ID，无父无母需要</param>
        /// <param name="childrenValue">孩子精纯数据列表</param>
        [HarmonyPostfix]
        private static void MakeNewChildrenPrefixReport(int fatherId, int motherId)
        //原方法的声明
        //public List<int> MakeNewChildren(int fatherId, int motherId, bool setFather, bool setMother, int gongId = 0, int gongLevel = 0, int baseActorId = 0, List<int> childrenValue = null)
        {
            IsSwitchOn = true;

            if (Main.Setting.debugMode.Value)
            {
                QuickLogger.Log(LogLevel.Debug, "【婴儿诞生开始记录】： fatherId:{0} motherId:{1}", fatherId, motherId);
            }

        }

        [HarmonyPostfix]
        private static void MakeNewChildrenPostfixReport()
        //原方法的声明
        //public List<int> MakeNewChildren(int fatherId, int motherId, bool setFather, bool setMother, int gongId = 0, int gongLevel = 0, int baseActorId = 0, List<int> childrenValue = null)
        {
            IsSwitchOn = false;

            if (Main.Setting.debugMode.Value)
            {
                Main.Logger.LogDebug("婴儿诞生记录完成");
            }

        }
    }

    /// <summary>
    /// DeBug用：传出婴儿降生时的父母双方数据
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "DoActorMake")]
    public static class ReportedOnDoActorMake
    {
        /// <summary>
        /// 对新生人物做修正（性别锁定、异性生相、性取向、魅力上修）
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="baseActorId">新人物的基础人物ID</param>
        /// <param name="actorId">新人物的实际人物ID</param>
        /// <param name="makeNewFeatures">是否新建人物特性</param>
        /// <param name="fatherId">父方人物ID</param>
        /// <param name="motherId">母方人物ID</param>
        /// <param name="age">年龄</param>
        /// <param name="attrValue">身体资质</param>
        /// <param name="skillValue">技艺资质</param>
        /// <param name="gongFaValue">功法资质</param>
        /// <param name="resourceValue">基础资源</param>
        /// <param name="baseCharm">基础魅力</param>
        /// <param name="faceDate">面容相貌数据</param>
        /// <param name="faceColorDate">面容颜色数据</param>
        /// <param name="randObbs">随机数</param>
        /// <param name="fatherBloodId">父系血统</param>
        /// <param name="motherBloodId">母系血统</param>
        [HarmonyPrefix]
        private static void DoActorMakePrefixReport(DateFile __instance, int baseActorId, int actorId, int baseCharm)
        //原方法的签名（参照用）
        //private void DoActorMake(int baseActorId, int actorId, bool makeNewFeatures, int fatherId, int motherId, int age = -1, string[] attrValue = null, string[] skillValue = null, string[] gongFaValue = null, string[] resourceValue = null, int baseCharm = -1, string[] faceDate = null, string[] faceColorDate = null, int randObbs = 20, int fatherBloodId = 0, int motherBloodId = 0)
        {
            if (ReportSwitchOnMakeNewChildren.IsSwitchOn == true && Main.Setting.debugMode.Value)
            {
                QuickLogger.Log(LogLevel.Debug, "【婴儿数据建立前】： actorId:{0} baseActorId:{1} baseCharm:{2}", actorId, baseActorId, baseCharm);
            }
        }

        [HarmonyPostfix]
        [HarmonyBefore("DoActorMakePostfix")]
        private static void DoActorMakePostfixReport(DateFile __instance, int baseActorId, int actorId)
        //原方法的声明
        //private void DoActorMake(int baseActorId, int actorId, bool makeNewFeatures, int fatherId, int motherId, int age = -1, string[] attrValue = null, string[] skillValue = null, string[] gongFaValue = null, string[] resourceValue = null, int baseCharm = -1, string[] faceDate = null, string[] faceColorDate = null, int randObbs = 20, int fatherBloodId = 0, int motherBloodId = 0)
        {
            if (ReportSwitchOnMakeNewChildren.IsSwitchOn == true && Main.Setting.debugMode.Value)
            {
                bool selfAntiGenderObscure = false;             //用于记录的参数

                //若调用该补丁时，本MOD的性别模糊正处于实际启用
                if (ObscureGenderHarmony.NeedPacth)
                {
                    selfAntiGenderObscure = true;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = false;     //在本补丁内暂时禁用性别模糊
                }

                Main.Logger.LogDebug("【婴儿数据建立后，MOD修正前】尝试读取婴儿相关的数据：");

                Main.Logger.LogDebug("presetActorDate是否为空:");
                Main.Logger.LogDebug(__instance.presetActorDate == null);
                Main.Logger.LogDebug("presetActorDate[baseActorId]是否为空:");
                Main.Logger.LogDebug(__instance.presetActorDate[baseActorId] == null);
                Main.Logger.LogDebug("presetActorDate[baseActorId][8]是否为空:");
                Main.Logger.LogDebug(__instance.presetActorDate[baseActorId][8] == null);
                Main.Logger.LogDebug("presetActorDate[baseActorId][8]数据:");
                Main.Logger.LogDebug(__instance.presetActorDate[baseActorId][8]);
                Main.Logger.LogDebug("presetActorDate[baseActorId][2]是否为空:");
                Main.Logger.LogDebug(__instance.presetActorDate[baseActorId][2] == null);
                Main.Logger.LogDebug("presetActorDate[baseActorId][2]数据:");
                Main.Logger.LogDebug(__instance.presetActorDate[baseActorId][2]);
                Main.Logger.LogDebug("actorId魅力值（无修正）");
                Main.Logger.LogDebug(__instance.GetActorDate(actorId, 15, false));
                Main.Logger.LogDebug("actorId魅力值（有修正）");
                Main.Logger.LogDebug(__instance.GetActorDate(actorId, 15, true));
                Main.Logger.LogDebug("actorId性别");
                Main.Logger.LogDebug(__instance.GetActorDate(actorId, 14, false));
                Main.Logger.LogDebug("actorId出家");
                Main.Logger.LogDebug(__instance.GetActorDate(actorId, 2, false));

                Main.Logger.LogDebug("要写入的项");
                Main.Logger.LogDebug("CharProperty14数据:");
                Main.Logger.LogDebug(Characters.GetCharProperty(actorId, 14));
                Main.Logger.LogDebug("CharProperty997数据:");
                Main.Logger.LogDebug(Characters.GetCharProperty(actorId, 997));
                Main.Logger.LogDebug("CharProperty17数据:");
                Main.Logger.LogDebug(Characters.GetCharProperty(actorId, 17));
                Main.Logger.LogDebug("CharProperty21数据:");
                Main.Logger.LogDebug(Characters.GetCharProperty(actorId, 21));
                Main.Logger.LogDebug("CharProperty15数据:");
                Main.Logger.LogDebug(Characters.GetCharProperty(actorId, 15));
                Main.Logger.LogDebug("CharProperty995数据:");
                Main.Logger.LogDebug(Characters.GetCharProperty(actorId, 995));

                Main.Logger.LogDebug("输出完成，进入实际补丁");

                //若在补丁开始时暂时禁用了性别模糊
                if (selfAntiGenderObscure)
                {
                    ObscureGenderHarmony.NeedPacth = true;  //在结束时重新启用性别模糊
                }
            }
        }
    }
}
#endif