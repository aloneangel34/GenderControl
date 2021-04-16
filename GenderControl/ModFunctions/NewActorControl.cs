using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;

namespace GenderControl
{
    /// <summary>
    /// 对新生人物做修正（性别锁定、异性生相、性取向、魅力上修）
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "DoActorMake")]
    public static class NewActorControl
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
        [HarmonyPostfix]
        [HarmonyAfter("DoActorMakePostfixReport")]
        private static void DoActorMakePostfix(DateFile __instance, int baseActorId, int actorId, int fatherId, int motherId, int baseCharm)
        //原方法的签名（参照用）
        //private void DoActorMake(int baseActorId, int actorId, bool makeNewFeatures, int fatherId, int motherId, int age = -1, string[] attrValue = null, string[] skillValue = null, string[] gongFaValue = null, string[] resourceValue = null, int baseCharm = -1, string[] faceDate = null, string[] faceColorDate = null, int randObbs = 20, int fatherBloodId = 0, int motherBloodId = 0)
        {
            bool selfAntiGenderObscure = false;             //用于记录的参数

            //若调用该补丁时，本MOD的性别模糊正处于实际启用
            if (ObscureGenderHarmony.NeedPacth)
            {
                selfAntiGenderObscure = true;               //记录在开头已经改变了
                ObscureGenderHarmony.NeedPacth = false;     //在本补丁内暂时禁用性别模糊
            }

            //获取baseActorId对应的enemyRandId
            int.TryParse(__instance.presetActorDate[baseActorId][8], out int enemyRandId);

            //若不为特殊人物（0、3、4） 且 人物不为初代太吾（actorId 固定为10001），【则按设定进行修正】
            if (enemyRandId != 0 && enemyRandId != 3 && enemyRandId != 4 && actorId != 10001)
            {
                //调试信息
                //if (Main.Setting.debugMode.Value)
                //{
                //    QuickLogger.Log(LogLevel.Info, "actorId:{0} baseActorId:{1} 魅力:{2} 加成魅力:{3} 性别:{4} 出家:{5}", actorId, baseActorId, __instance.GetActorDate(actorId, 15, false), __instance.GetActorDate(actorId, 15, true), __instance.GetActorDate(actorId, 14, false), __instance.GetActorDate(actorId, 2, false));
                //}

                //若 性别锁已启用（且有合法值）
                if (Main.Setting.newActorGenderLock.Value == 1 || Main.Setting.newActorGenderLock.Value == 2)
                {
                    //若人物性别可以转为整数，且为有效数值
                    if (int.TryParse(__instance.GetActorDate(actorId, 14, false), out int gender) && (gender == 1 || gender == 2))
                    {
                        //若有效性别和MOD锁定值不同，【实际进行相关变更】
                        if (Main.Setting.newActorGenderLock.Value != gender)
                        {
                            //性别按设定值重设
                            Characters.SetCharProperty(actorId, 14, Main.Setting.newActorGenderLock.Value.ToString());

                            //若baseActorId为1～32，【性别变更后、需要调整人物的baseActorId（人物属性997项）】
                            if (baseActorId >= 1 && baseActorId <= 32)
                            {
                                //依照原性别，判定怎么调整
                                if (gender == 1)
                                { baseActorId++; }
                                else
                                { baseActorId--; }

                                Characters.SetCharProperty(actorId, 997, baseActorId.ToString());   //重设调整后的baseActorId（997项）

                                //调试信息
                                //if (Main.Setting.debugMode.Value)
                                //{
                                //    QuickLogger.Log(LogLevel.Info, "actorId:{0} baseActorId变为:{1} 原性别:{2}", actorId, baseActorId, gender);
                                //}
                            }

                            //若人物有无法生育的人物特性，性别更改后需要对调。
                            //“无根之人1001”（男）、“石芯玉女1002”（女）
                            if (DateFile.instance.GetActorFeature(actorId, false).Contains(gender == 1 ? 1001 : 1002))
                            {
                                //调试信息
                                //if (Main.Setting.debugMode.Value)
                                //{
                                //    QuickLogger.Log(LogLevel.Info, "actorId:{0} 原性别:{1} 原有无根之人:{2} 原有石芯玉女:{3} 特性将按照新性别修正", actorId, gender, __instance.GetActorFeature(actorId, false).Contains(1001), __instance.GetActorFeature(actorId, false).Contains(1002));
                                //}

                                DateFile.instance.ChangeActorFeature(actorId, (gender == 1 ? 1001 : 1002), (gender == 1 ? 1002 : 1001));
                            }
                        }
                    }
                    //人物无有效性别
                    else
                    {
                        //性别直接按设定值重设
                        Characters.SetCharProperty(actorId, 14, Main.Setting.newActorGenderLock.Value.ToString());

                        //调试信息
                        //if (Main.Setting.debugMode.Value)
                        //{
                        //    QuickLogger.Log(LogLevel.Warning, "actorId:{0} 未获取到有效性别，按MOD设置设为:{1}", actorId, Main.Setting.newActorGenderLock.Value);
                        //}
                    }
                }

                //若开启 新人物不出现男生女相/女生男相，【统一重设】
                if (Main.Setting.newActorNoOppositeGenderFace.Value)
                {
                    Characters.SetCharProperty(actorId, 17, "0");       //重设新人物的“是否为异性面相”（17项）
                }

                //若开启 新人物皆设为双性恋，【统一重设】
                if (Main.Setting.newActorAllBisexual.Value)
                {
                    Characters.SetCharProperty(actorId, 21, "1");       //重设新人物的“性取向”（17项）
                }

                //若 baseActorId在指定颜值UP列表中，【新人物魅力上调（依照人物原有魅力区间、魅力上调系数）】
                if (Settings.RegionCharmUpBaseActorIds.Contains(baseActorId))
                {
                    //获取新人物的基础魅力
                    int.TryParse(__instance.GetActorDate(actorId, 15, false), out int charm);

                    //若人物基础魅力已达到700，不再上调
                    if (charm < 700)
                    {
                        //依照（人物原有魅力区间、魅力上调系数）计算上调数值
                        int charmUpValue = (9 - charm / 100) * Main.Setting.newActorCharmUpFactor.Value + (10 - charm / 100) * 2;
                        //再随机调整一下
                        charmUpValue = __instance.RandomAndSetNewSeed(charmUpValue - (charm / 100) * 4, charmUpValue + (7 - charm / 100) * 2, actorId);


                        //调试信息
                        //if (Main.Setting.debugMode.Value)
                        //{
                        //    QuickLogger.Log(LogLevel.Info, "actorId:{0} 原魅力:{1} 魅力上升值:{2} （终值最高900） ", actorId, charm, charmUpValue);
                        //}

                        //魅力上调（上限为900，下限为原数值。防止出错）
                        charm = Mathf.Clamp((charm + charmUpValue), charm, 900);
                        //重新设定魅力数值
                        Characters.SetCharProperty(actorId, 15, charm.ToString());

                        //若原方法传入的baseCharm参数为不为正或省略（有父母的新生儿、其baseCharm必定为正，此时为了不破坏相貌继承。需要排除出去）
                        if (baseCharm < 0)
                        {
                            //以新魅力来重新随机设定人物面容
                            __instance.RandActorFace(actorId, charm, -1);

                            //若新人物的出家属性不为0，要在面容重设后，再把发型变回出家专用发型
                            if (int.TryParse(__instance.presetActorDate[baseActorId][2], out int n) && n != 0)
                            {
                                //读取新人物面容数据
                                string[] faceData = __instance.GetActorDate(actorId, 995, false).Split(new char[] { '|' });

                                if (faceData.Length >= 8)
                                {
                                    //发型设为 "15"（出家专用发型）
                                    faceData[7] = "15";
                                    //重新设定人物相貌
                                    //Characters.SetCharProperty(actorId, key, string.Format(format, args));
                                    Characters.SetCharProperty(actorId, 995, string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", faceData));
                                }
                                else
                                {
                                    QuickLogger.Log(LogLevel.Error, "在重设出家人发型时异常：actorId:{0} baseActorId:{1} 面容相貌数组长度:{2}", actorId, baseActorId, faceData.Length);
                                }
                            }
                        }

                    }
                }
            }

            //若在补丁开始时暂时禁用了性别模糊
            if (selfAntiGenderObscure)
            {
                ObscureGenderHarmony.NeedPacth = true;  //在结束时重新启用性别模糊
            }
        }
    }
}
