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
    /// 用于修复因“生成人物性别相同，导致游戏在生成夫妻、并接着生成双方的孩子时，传入的父母双方ID为同一人”的问题
    /// </summary>
    [HarmonyPatch(typeof(DateFile), "MakeChildren")]
    public static class MakeChildrenParentsFix
    {
        /// <summary>
        /// 用于修复因“生成人物性别相同，导致游戏在生成夫妻、并接着生成双方的孩子时，传入的父母双方ID为同一人”的问题
        /// </summary>
        /// <param name="__instance">原方法所属的实例</param>
        /// <param name="gangTyp">势力类型（1非门派/2门派）</param>
        /// <param name="childrenLevel">孩子的阶级</param>
        /// <param name="motherAge">母亲年龄</param>
        /// <param name="baseGangActorId">基础baseActorId</param>
        /// <param name="gangId">势力ID</param>
        /// <param name="fatherId">父方人物ID</param>
        /// <param name="motherId">母方人物ID</param>
        /// <param name="socialTyp">关系种类</param>
        /// <param name="partId">地区ID</param>
        /// <param name="placeId">地格ID</param>
        [HarmonyPrefix]
        private static void MakeChildrenPrefix(DateFile __instance, int fatherId, ref int motherId, int socialTyp, int partId, int placeId)
        //原方法的签名（参照用）
        //public void MakeChildren(int gangTyp, int childrenLevel, int motherAge, int baseGangActorId, int gangId, int fatherId, int motherId, int socialTyp, int partId, int placeId)
        {

            //若父方ID和母方ID相同，【尝试进行修正】
            //（比如当传入数据时，因理论父母双方性别相同、而造成无法正确识别父母双方，导致传入的皆为同一人）
            if (fatherId == motherId)
            {
                //获取父方人物传入的关系种类所对应的人物ID列表
                List<int> maybeMotherIds = __instance.GetActorSocial(fatherId, socialTyp, false, false);

                //调试信息
                if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Info, "MakeChildren方法、检测到传入父母双方ID相同:{0} 关系:{1} 关系对应人数:{2} 地区ID:{3} 地格ID:{4}", fatherId, socialTyp, maybeMotherIds.Count, partId, placeId);
                }

                //遍历该列表，将其中不在传入地点的人物排除
                for (int i = 0; i < maybeMotherIds.Count; i++)
                {
                    List<int> position = __instance.GetActorAtPlace(maybeMotherIds[i]); //获取遍历到的人物的所在地点

                    //判断该所在地点是否和传入的地点一致
                    if (position[0] != partId || position[1] != placeId)
                    {
                        maybeMotherIds.RemoveAt(i);     //将该人物从列表中移除
                        i--;                            //由于列表少了一个，所以需要将 i 减一
                    }
                }

                //若列表不为空，【尝试将母方修正为正确的对象】
                //（若此时列表中还有多个对象的话，暂时想不到什么好的办法去进一步识别）（若列表为空，找不到合适的。也许是MOD造成、那就不处理）
                if (maybeMotherIds.Count > 0)
                {
                    //重新设定母方人物ID（理论上原本游戏中，该方法只被两处所调用。且关系都是309结发夫妻。那时候不会出错，但可能会因MOD而产生变数）
                    motherId = maybeMotherIds[__instance.Rand(0, maybeMotherIds.Count)];

                    //调试信息
                    if (Main.Setting.debugMode.Value)
                    {
                        QuickLogger.Log(LogLevel.Info, "修复成功，指定actorId:{0}为母方 符合条件的总人数:{1}", motherId, maybeMotherIds.Count);
                    }
                }
                //尝试修正的可选列表为空，调试信息
                else if (Main.Setting.debugMode.Value)
                {
                    QuickLogger.Log(LogLevel.Error, "为MakeChildren方法修复“父母双方ID相同”失败，未能为actorId:{0} 在关系:{1}、地区:{2}、地格:{3} 找到可能的另一方 ", fatherId, socialTyp, partId, placeId);
                }
            }
        }
    }
}
