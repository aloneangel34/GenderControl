using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Random = UnityEngine.Random;
using BepInEx.Logging;

namespace GenderControl
{
    /// <summary>
    /// （仅在 模糊性别判定功能 开关开启时生效）按设置指定怀孕方（太吾参与、异性NPC间）。同性NPC固定设为50%随机
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
    public static class SpecifyPregnantSide
    {
        static bool _isPrefixRun = false;
        static bool _noNeedDisableAgain = false;
        static int _recoverPatchActorId;
        static int _taiwuActorId;

        /// <summary>
        /// 怀孕判定调用前，尝试按设置指定怀孕方（同时暂时开启性别模糊的NeedPatch）
        /// </summary>
        /// <param name="__result">原方法的返回值（是否成功怀孕）</param>
        /// <param name="fatherId">父方人物ID</param>
        /// <param name="motherId">母方人物ID</param>
        /// <param name="setFather">是否在孩子关系中显示父方</param>
        /// <param name="setMother">是否在孩子关系中显示母方</param>
        [HarmonyPrefix]
        private static void AISetChildrenPrefix(ref int fatherId, ref int motherId)
        //public bool AISetChildren(int fatherId, int motherId, int setFather, int setMother)
        {
            //若 模糊性别判定功能 开关开启（一般所指的已启用是指NeedPatch，抱歉造成歧义），则追加指定怀孕方
            if (Main.Setting.obscureGender.Value)
            {
                _isPrefixRun = true;                                //记录，告知后置补丁、本前置补丁未被其他前置补丁跳过

                bool isNeedPacthDisabledAtFront = false;            //用于记录的参数

                //若性别模糊已启用，【暂时关闭性别模糊】
                if (ObscureGenderHarmony.NeedPacth == true)
                {
                    isNeedPacthDisabledAtFront = true;              //记录在开头已经暂时关闭了
                    ObscureGenderHarmony.NeedPacth = false;         //在本补丁内暂时禁用性别模糊
                }

                _taiwuActorId = DateFile.instance.MianActorID();

                #region 按照设定指定受孕方（母方）

                //太吾参与怀孕判定的场合
                if (fatherId == _taiwuActorId || motherId == _taiwuActorId)
                {
                    bool isTaiwuAsFarther = (fatherId == _taiwuActorId);

                    //根据
                    switch (Main.Setting.specifyPregnantForTaiwu.Value)
                    {
                        //按照太吾性别指定（默认）
                        case 0:
                            //若太吾是男性，且传进来的太吾不是父方，【则将母方设为对象】
                            if (DateFile.instance.GetActorDate(_taiwuActorId, 14, false) == "1" && !isTaiwuAsFarther)
                            {
                                _taiwuActorId = motherId;   //保存太吾ID（原母方）
                                motherId = fatherId;        //将母方设为对方ID（原父方）
                                fatherId = _taiwuActorId;   //将父方设为保存的太吾ID
                            }
                            //若太吾是女性，且传进来的太吾不是母方，【则将母方设为太吾】
                            else if (DateFile.instance.GetActorDate(_taiwuActorId, 14, false) == "2" && isTaiwuAsFarther)
                            {
                                _taiwuActorId = motherId;   //保存对方ID（原母方）
                                motherId = fatherId;        //将母方设为太吾ID（原父方）
                                fatherId = _taiwuActorId;   //将父方设为保存的对方ID
                            }
                            break;
                        //指定太吾
                        case 1:
                            //若母方不为太吾（父方为太吾），【则将母方设为太吾】
                            if (isTaiwuAsFarther)
                            {
                                _taiwuActorId = motherId;   //保存对方ID（原母方）
                                motherId = fatherId;        //将母方设为太吾ID（原父方）
                                fatherId = _taiwuActorId;   //将父方设为保存的对方ID
                            }
                            break;
                        //指定对方
                        case 2:
                            //若父方不为太吾（母方为太吾），【则将母方设为对象】
                            if (!isTaiwuAsFarther)
                            {
                                _taiwuActorId = motherId;   //保存太吾ID（原母方）
                                motherId = fatherId;        //将母方设为对方ID（原父方）
                                fatherId = _taiwuActorId;   //将父方设为保存的太吾ID
                            }
                            break;
                        //随机指定
                        case 3:
                            //50%几率，【调换双方】
                            if (Random.Range(0, 100) < 50)
                            {
                                _taiwuActorId = fatherId;
                                fatherId = motherId;
                                motherId = _taiwuActorId;
                            }
                            break;
                    }
                }
                //没有太吾参与，NPC之间的怀孕判定场合
                else
                {
                    int fatherGender;
                    int motherGender;

                    //如果获取到了父母双方的有效性别值
                    if (int.TryParse(DateFile.instance.GetActorDate(fatherId, 14, false), out fatherGender) && int.TryParse(DateFile.instance.GetActorDate(motherId, 14, false), out motherGender) && (fatherGender != 1 || fatherGender != 2) && (motherGender != 1 || motherGender != 2))
                    {
                        //父方、母方为异性的场合
                        if (fatherGender != motherGender)
                        {
                            switch (Main.Setting.specifyPregnantForOppsiteSex.Value)
                            {
                                //指定女性（默认）
                                case 0:
                                    //若母方不为女性（母方为男性），【则将双方互换】
                                    if (motherGender != 2)
                                    {
                                        _taiwuActorId = motherId;
                                        motherId = fatherId;
                                        fatherId = _taiwuActorId;
                                    }
                                    break;
                                //指定男性
                                case 1:
                                    //若母方不为男性（母方为女性），【则将双方互换】
                                    if (motherGender == 2)
                                    {
                                        _taiwuActorId = motherId;
                                        motherId = fatherId;
                                        fatherId = _taiwuActorId;
                                    }
                                    break;
                                //随机指定
                                case 2:
                                    //50%几率，【调换双方】
                                    if (Random.Range(0, 100) < 50)
                                    {
                                        _taiwuActorId = fatherId;
                                        fatherId = motherId;
                                        motherId = _taiwuActorId;
                                    }
                                    break;
                            }
                        }
                        //父方、母方为同性的场合
                        else
                        {
                            //50%几率，【调换双方】
                            if (Random.Range(0, 100) < 50)
                            {
                                _taiwuActorId = fatherId;
                                fatherId = motherId;
                                motherId = _taiwuActorId;
                            }
                        }
                    }
                    else if (Main.Setting.debugMode.Value)
                    {
                        QuickLogger.Log(LogLevel.Error, "尝试获取怀孕判定的父母双方性别时失败 父方ID:{0} 母方ID:{1}", fatherId, motherId);
                    }
                }
                #endregion

                _recoverPatchActorId = Settings.PatchActorID;       //记录原本的“行为主动方”用于还原
                Settings.PatchActorID = fatherId;                   //行为主动方：重设为传入的第一个人物为父方（这样保证母方是女性）

                ObscureGenderHarmony.NeedPacth = true;              //之前必定为false，为了保证调换后能怀孕，需要再开启

                //如果在开头暂时关闭了性别模糊，则告知给后置补丁，不需要关闭了
                if (isNeedPacthDisabledAtFront)
                {
                    _noNeedDisableAgain = true;                     //告知不需要重新关闭
                }
            }
        }

        /// <summary>
        /// 怀孕判定调用前，还原性别模糊的NeedPatch
        /// </summary>
        [HarmonyPostfix]
        private static void AISetChildrenPostfix(int fatherId, int motherId)
        //public bool AISetChildren(int fatherId, int motherId, int setFather, int setMother)
        {
            if (_isPrefixRun)
            {
                //若不需要重新关闭性别模糊
                if (_noNeedDisableAgain)
                {
                    _noNeedDisableAgain = false;                //重置记录
                }
                //需要重新关闭性别模糊
                else
                {
                    ObscureGenderHarmony.NeedPacth = false;     //将需要补丁设为否（并没有实际卸载补丁）
                }

                Settings.PatchActorID = _recoverPatchActorId;   //行为主动方：还原
                _isPrefixRun = false;                           //重置记录
            }
            //在非预期被跳过“指定怀孕方”补丁时，【汇报原方法的HarmonyPatch列表】
            else if (!NoBothPregnant.IsSkipped && Main.Setting.debugMode.Value && Main.Setting.obscureGender.Value)
            {
                QuickLogger.Log(LogLevel.Warning, "用于指定怀孕方的前置补丁已被跳过 fatherId:{0} motherId{1}", fatherId, motherId);
                var patch = Harmony.GetPatchInfo(typeof(PeopleLifeAI).GetMethod("AISetChildren"));

                //对AISetChildren方法进行补丁的前置补丁列表
                Main.Logger.LogDebug("PeopleLifeAI.AISetChildren 的所有前置补丁列表:");
                for (int i = 0; i < patch.Prefixes.Count; i++)
                {
                    Main.Logger.LogDebug("补丁序号: " + patch.Prefixes[i].index);
                    Main.Logger.LogDebug("所属HarmonyID: " + patch.Prefixes[i].owner);
                    Main.Logger.LogDebug("优先级: " + patch.Prefixes[i].priority);
                    Main.Logger.LogDebug("指定 before 个数: " + patch.Prefixes[i].before.Length);
                    for (int j = 0; j < patch.Prefixes[i].before.Length; j++)
                    {
                        Main.Logger.LogDebug(patch.Prefixes[i].before[j]);
                    }
                    Main.Logger.LogDebug("指定 after 个数: " + patch.Prefixes[i].after.Length);
                    for (int j = 0; j < patch.Prefixes[i].after.Length; j++)
                    {
                        Main.Logger.LogDebug(patch.Prefixes[i].after[j]);
                    }
                    Main.Logger.LogDebug("--------");
                }
            }

            //单独重置记录参数
            NoBothPregnant.IsSkipped = false;
        }
    }
}
