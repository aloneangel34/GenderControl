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
    /// 性别模糊的主要启用时机：NPC过月【亦为“禁止NPC过月脱离势力”的启用时机】
    /// </summary>
    [HarmonyPatch(typeof(PeopleLifeAI), "DoTrunAIChange")]
    public static class NeedWorkingNpcTurnChangeActions
    {
        /// <summary>
        /// 地格中的NPC是否全都行动过了（即本次调用为新地格）
        /// </summary>
        //static bool _isAllCharsInOneTileAlreadyActed = true;
        /// <summary>
        /// 当前地格未行动NPC列表
        /// </summary>
        //static List<int> _unActChars = new List<int>();

        /// <summary>
        /// NPC过月行动调用前，开启部分HarmonyPatch的实际运行
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
        /// <returns>是否执行原方法（本补丁为false，即不再执行）</returns>
        [HarmonyPrefix]
        private static void Prefix(PeopleLifeAI __instance, int actorId, int mapId, int tileId, int mainActorId, int[] aliveChars)
        //原方法的签名（参照用）
        //private int DoTrunAIChange(int actorId, int mapId, int tileId, int mainActorId, bool isTaiwuAtThisTile, int worldId, Dictionary<int, List<int>> mainActorItems, int[] aliveChars, int[] deadChars)    //原方法的声明，用于对照
        {
            Settings.PatchActorID = actorId;                    //记录本次调用中的行为主动方的人物ID

            ObscureGenderHarmony.NeedPacth = true;                //将需要补丁设为是（性别模糊）
            NpcPassTurnCantChangeGangHarmony.NeedPacth = true;    //将需要补丁设为是（势力变更拦截）


            #region 本来加判断是为了减轻运行负担的（但由于并没有实际采用最初设想的加载/卸载Patch，而只是改了Patch运行中的参数、Patch一直加载着。所以可以省略了）

            ////调试信息
            //if (Main.Setting.debugMode.Value)
            //{
            //    Main.Logger.LogInfo(string.Format("地点ID：{0}，地格ID：{1}。当前行动人物ID：{2}。当前地格未行动NPC数量：{3}，当前地格存活NPC总数：{4}。", mapId, tileId, actorId, _unActChars.Count, aliveChars.Length));
            //}

            //Settings.PatchActorID = actorId;                    //记录本次调用中的行为主动方的人物ID

            ////若地格中的NPC是否全都行动过了（即轮到执行另一块地格上的NPC过月时）
            //if (_isAllCharsInOneTileAlreadyActed)
            //{
            //    _unActChars.Clear();                            //理论上是不用清空的，能进来就是表示已经是空的，但为防意外，求个安心
            //    _unActChars.AddRange(aliveChars);               //将当前地格的存活NPC加入未行动者列表

            //    _isAllCharsInOneTileAlreadyActed = false;       //设为否，表示开始在这块地格的人物全部行动完之前，不用再进来

            //    ObscureGenderHarmony.NeedPacth = true;                //将需要补丁设为是（性别模糊）
            //    NpcPassTurnCantChangeGangHarmony.NeedPacth = true;    //将需要补丁设为是（势力变更拦截）
            //} 
            #endregion
        }

        /// <summary>
        /// NPC过月行动调用后，关闭部分HarmonyPatch的实际运行
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
        /// <returns>是否执行原方法（本补丁为false，即不再执行）</returns>
        [HarmonyPostfix]
        private static void Postfix(PeopleLifeAI __instance, int actorId, int mainActorId, int[] aliveChars)
        //原方法的签名（参照用）
        //private int DoTrunAIChange(int actorId, int mapId, int tileId, int mainActorId, bool isTaiwuAtThisTile, int worldId, Dictionary<int, List<int>> mainActorItems, int[] aliveChars, int[] deadChars)
        {
            ObscureGenderHarmony.NeedPacth = false;               //将需要补丁设为否（并没有实际卸载补丁）
            NpcPassTurnCantChangeGangHarmony.NeedPacth = false;   //将需要补丁设为否（并没有实际卸载补丁）

            #region 本来加判断是为了减轻运行负担的（但由于并没有实际采用最初设想的加载/卸载Patch，而只是改了Patch运行中的参数、Patch一直加载着。所以可以省略了）

            //_unActChars.Remove(actorId);                        //从未行动NPC列表中移除当前行动角色

            ////若未行动NPC列表为空，
            //if (_unActChars.Count == 0)
            //{
            //    _isAllCharsInOneTileAlreadyActed = true;        //地格中的NPC已全部结束行动

            //    ObscureGenderHarmony.NeedPacth = false;               //将需要补丁设为否（并没有实际卸载补丁）
            //    NpcPassTurnCantChangeGangHarmony.NeedPacth = false;   //将需要补丁设为否（并没有实际卸载补丁）
            //}
            #endregion
        }
    }
}