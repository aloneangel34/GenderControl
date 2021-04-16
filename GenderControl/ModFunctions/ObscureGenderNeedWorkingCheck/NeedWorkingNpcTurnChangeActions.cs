using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GenderControl
{
    /// <summary>
    /// 性别模糊的主要启用时机：NPC过月【亦为“禁止NPC过月脱离势力”的启用时机】
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
    public static class NeedWorkingNpcTurnChangeActions
    {
        /// <summary>
        /// NPC过月行动调用前，开启【性别模糊】与【禁止NPC脱离指定势力】的实际运行
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="__result">人物死于天灾的几率（原方法的返回值）</param>
        /// <param name="actorId">人物ID</param>
        /// <param name="mapId">大地图位置</param>
        /// <param name="tileId">小地图位置</param>
        /// <param name="mainActorId">太吾的人物ID</param>
        /// <param name="isTaiwuAtThisTile">人物是否和太吾处于同一格</param>
        /// <param name="worldId">世界地图ID（哪个省）</param>
        /// <param name="mainActorItems"></param>
        /// <param name="aliveChars">人物所在地格中，活着的人物列表</param>
        /// <param name="deadChars">人物所在地格中，死去的人物列表</param>
        [HarmonyPrefix]
        private static void DoTrunAIChangePrefix(int actorId)
        //private static void Prefix(int actorId, int mapId, int tileId, int[] aliveChars)
        //原方法的签名（参照用）
        //private int DoTrunAIChange(int actorId, int mapId, int tileId, int mainActorId, bool isTaiwuAtThisTile, int worldId, Dictionary<int, List<int>> mainActorItems, int[] aliveChars, int[] deadChars)    //原方法的声明，用于对照
        {
            Settings.PatchActorID = actorId;                      //记录本次调用中的行为主动方的人物ID

            ObscureGenderHarmony.NeedPacth = true;                //将需要补丁设为是（性别模糊）
            NpcPassTurnCantChangeGangHarmony.NeedPacth = true;    //将需要补丁设为是（势力变更拦截）
        }

        /// <summary>
        /// NPC过月行动调用后，关闭【性别模糊】与【禁止NPC脱离指定势力】的实际运行
        /// </summary>
        [HarmonyPostfix]
        private static void DoTrunAIChangePostfix()
        //private static void Postfix(PeopleLifeAI __instance, int actorId, int mainActorId, int[] aliveChars)
        //原方法的签名（参照用）
        //private int DoTrunAIChange(int actorId, int mapId, int tileId, int mainActorId, bool isTaiwuAtThisTile, int worldId, Dictionary<int, List<int>> mainActorItems, int[] aliveChars, int[] deadChars)
        {
            ObscureGenderHarmony.NeedPacth = false;               //将需要补丁设为否（并没有实际卸载补丁）
            NpcPassTurnCantChangeGangHarmony.NeedPacth = false;   //将需要补丁设为否（并没有实际卸载补丁）
        }
    }
}