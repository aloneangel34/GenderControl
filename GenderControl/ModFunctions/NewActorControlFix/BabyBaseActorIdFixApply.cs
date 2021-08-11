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
    /// 对婴儿的BaseActorId应用修正值。用于修复因“传入的父母不对应原方法所设计的性别，导致BaseActorId计算出错”的问题
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeGangActor")]
    public static class BabyBaseActorIdFixApply
    {
        /// <summary>
        /// 在方法调用前，对传入的baseActorId应用修正值
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="__result">原方法的返回值（新生儿实际人物ID）</param>
        /// <param name="baseActorId">新人物基础ID</param>
        /// <param name="gangId">新人物所属势力ID</param>
        /// <param name="level">新人物身份阶级</param>
        /// <param name="age">新人物的年龄</param>
        /// <param name="partId">新人物出现地点大地图位置</param>
        /// <param name="placeId">新人物出现地点小地图位置</param>
        /// <param name="fatherId">父方人物ID</param>
        /// <param name="motherId">母方人物ID</param>
        /// <param name="surnameId">新人物姓氏ID</param>
        /// <param name="surname">新人物自定义姓氏</param>
        /// <param name="baseCharm">新人物基础魅力</param>
        /// <param name="faceDate">新人物面容相貌数据</param>
        /// <param name="faceColorDate">新人物面容颜色数据</param>
        /// <param name="solarId">新人物出生时节</param>
        /// <param name="fatherBloodId">父系血统</param>
        /// <param name="motherBloodId">母系血统</param>
        [HarmonyPrefix]
        private static void MakeGangActorPrefix(ref int baseActorId, int fatherId, int motherId)
        //public int MakeGangActor(int baseActorId, int gangId, int level, int age, int partId, int placeId, int fatherId, int motherId, int surnameId = 0, string surname = "", int baseCharm = -1, string[] faceDate = null, string[] faceColorDate = null, int solarId = -1, int fatherBloodId = 0, int motherBloodId = 0)
        {
            //若修正值不为0，【应用修正】
            //（理论上来说，若不是由MakeNewChildren来调用此方法），则修正值必定为0（也就是没有修正）
            if (BabyBaseActorIdFixCalc.BabyBaseActorIdFixValue != 0)
            {
                if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Info, "婴儿原baseActorId:{0} 修正值:{1} 婴儿父方ID:{2} 婴儿母方ID:{3}", baseActorId, BabyBaseActorIdFixCalc.BabyBaseActorIdFixValue, fatherId, motherId);
                }

                //对婴儿的BaseActorId应用修正值
                baseActorId += BabyBaseActorIdFixCalc.BabyBaseActorIdFixValue;
            }

            //若（修正后的）婴儿的BaseActorId不在 1～32 的范围内，【报错】
            //（理论上来说，修正后的BaseActorId应该在 1～32 的范围内。因为只有婴儿才需要修正，而1～32是婴儿才会采用的BaseActorId）
            if (baseActorId < 1 || baseActorId > 32)
            {
                bool selfAntiGenderObscure = false;             //用于记录的参数

                //若开始时，本MOD的性别模糊正处于实际启用
                if (ObscureGenderHarmony.NeedPacth)
                {
                    selfAntiGenderObscure = true;               //记录在开头已经改变了
                    ObscureGenderHarmony.NeedPacth = false;     //在本补丁内暂时禁用性别模糊
                }

                QuickLogger.Log(LogLevel.Warning, "婴儿（修正后）的baseActorId:{0} 不在1～32的范围内，将在1～32中随机设置", baseActorId);
                QuickLogger.Log(LogLevel.Warning, "修正值:{0} 父方ID:{1} 父方BaseActorId:{2} 父方性别:{3} 母方ID:{4}  母方BaseActorId:{5} 母方性别:{6}", BabyBaseActorIdFixCalc.BabyBaseActorIdFixValue, fatherId, DateFile.instance.GetActorDate(fatherId, 997, false), DateFile.instance.GetActorDate(fatherId, 14, false), motherId, DateFile.instance.GetActorDate(motherId, 997, false), DateFile.instance.GetActorDate(motherId, 14, false));

                baseActorId = UnityEngine.Random.Range(1, 33);  //在1～32中随机（左闭右开，可以取到1、无法取到33）

                QuickLogger.Log(LogLevel.Info, "随机设置之后、婴儿的baseActorId为:{0}", baseActorId);

                //若在开始时暂时禁用了性别模糊
                if (selfAntiGenderObscure)
                {
                    ObscureGenderHarmony.NeedPacth = true;      //在结束时重新启用性别模糊
                }
            }
        }
    }
}
