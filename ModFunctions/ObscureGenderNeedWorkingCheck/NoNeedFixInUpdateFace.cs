using System;
using HarmonyLib;
using BepInEx.Logging;

namespace GenderControl
{
    /// <summary>
    /// 性别模糊不适宜启用、因此需要修正的场合：更新立绘
    /// </summary>
    //因为目标方法有重载，所以需要输入目标方法的参数类型的数组、来帮助Harmony识别具体要Patch的方法
    [HarmonyPatch(typeof(ActorFace), "UpdateFace", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int[]), typeof(int[]), typeof(int), typeof(bool), typeof(bool) })]
    public static class NeedFixInUpdateFace
    {
        /// <summary>
        /// 更新立绘调用前，尝试修正因“开启性别模糊”而造成的传入性别有误
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="actorId">人物ID</param>
        /// <param name="age">年龄</param>
        /// <param name="gender">性别</param>
        /// <param name="actorGenderChange">是否要改变立绘性别（通常传入的是人物的“异性生相17”属性）</param>
        /// <param name="faceDate">面容相貌数据</param>
        /// <param name="faceColor">面容颜色数据</param>
        /// <param name="clotheIndex">衣着序号</param>
        /// <param name="life">（不确定：是否存活？）</param>
        /// <param name="isMainMenu">（不确定：是否处于主菜单？）</param>
        [HarmonyPrefix]
        private static void UpdateFacePrefix(int actorId, ref int gender)
        //原方法的签名（参照用）
        //public void UpdateFace(int actorId, int age, int gender, int actorGenderChange, int[] faceDate, int[] faceColor, int clotheIndex, bool life = false, bool isMainMenu = false)
        {
            //若调用该方法时，性别模糊已开启 且 该人物ID不为 -1 （即传入人物的性别有可能有误、且并非是设置界面在调用该方法时）
            if (ObscureGenderHarmony.NeedPacth == true && actorId != -1)
            {
                ObscureGenderHarmony.NeedPacth = false;         //修正性别前，暂时关闭性别模糊

                //若获得了有效的人物性别，【将传入的性别修正为人物有效的性别】【无效则不处理】
                if (int.TryParse(DateFile.instance.GetActorDate(actorId, 14, false), out int n) && (n == 1 || n == 2))
                {
                    gender = n;     //由于本补丁的签名中，在gender参数加上了ref关键字，所以这里改动后，传给原方法的就是改动后的值
                }
                //调试信息
                else if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Error, "在UpdateFace方法的性别修正中，无法获取actorId:{0} 的有效性别，未做修正", actorId);
                }

                ObscureGenderHarmony.NeedPacth = true;          //修正性别后，恢复启用性别模糊
            }
        }
    }
}