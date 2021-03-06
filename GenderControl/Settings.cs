using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;

namespace GenderControl
{
    /// <summary>MOD设置选项</summary>
    public class Settings
    {
        /// <summary>总开关：MOD启用与否</summary>
        public ConfigEntry<bool> ModEnable;
        /// <summary>开关：Debug模式开关</summary>
        public ConfigEntry<bool> debugMode;
        /// <summary>字符串：许可声明</summary>
        public ConfigEntry<string> modClaimInfo;

        /// <summary>开关：模糊性别（可同性生子、可同性表白、不会因同性而产生的流言蜚语、女性可参加比武招亲）</summary>
        public ConfigEntry<bool> obscureGender;
        /// <summary>开关：解除门派/身份对性别要求的限制</summary>
        public ConfigEntry<bool> unlockGangLevelGenderRequire;
        /// <summary>整数：新人物性别锁定</summary>
        public ConfigEntry<int> newActorGenderLock;
        /// <summary>整数：指定怀孕对象（太吾）</summary>
        public ConfigEntry<int> specifyPregnantForTaiwu;
        /// <summary>整数：指定怀孕对象（异性NPC）</summary>
        public ConfigEntry<int> specifyPregnantForOppsiteSex;
        /// <summary>开关：新人物不出现男生女相/女生男相</summary>
        public ConfigEntry<bool> newActorNoOppositeGenderFace;
        /// <summary>开关：新人物皆设为双性恋</summary>
        public ConfigEntry<bool> newActorAllBisexual;
        /// <summary>开关组：指定地区的新人物魅力上调</summary>（基础魅力越低、上升越多，700以上不调整。有父亲或母亲的不调整——因为继承相貌、不好调整）
        public ConfigEntry<bool[]> newActorInRegionCharmUp;
        /// <summary>整数：魅力上调系数</summary>
        public ConfigEntry<int> newActorCharmUpFactor;
        /// <summary>整数：游戏显示肤色变更（不实际修改人物肤色ID，只是让游戏中显示出来的颜色有变化）</summary>
        public ConfigEntry<int> displayFaceColors;
        /// <summary>开关组：禁止NPC在过月时脱离指定势力（理论上太吾的行动不会受影响）</summary>
        public ConfigEntry<bool[]> npcPassTurnCantChangeGang;

        //声明静态字段变相作为全局变量，不用保存进配置文件
        /// <summary>在需要被补丁的行动中，行为主动方的人物ID（该角色被视为男性，其他角色被视为女性）</summary>
        public static int PatchActorID = -1;
        /// <summary>魅力上调指定地区的一键开关记录</summary>
        public static bool NextTimeAllRegionSetOn = true;
        /// <summary>禁止脱离指定势力的一键开关记录</summary>
        public static bool NextTimeAllGangSetOn = true;
        /// <summary>魅力上修的指定地区所对应的BaseActorID列表</summary>
        public static List<int> RegionCharmUpBaseActorIds = new List<int>();
        /// <summary>NPC在过月时禁止脱离的势力列表</summary>
        public static List<int> CantChangeGangIds = new List<int>();

        /// <summary>Config的属性（公开可读，私有可写）</summary>
        public ConfigFile Config { get; private set; }

        /// <summary>设置选项初始化</summary>
        public void Init(ConfigFile Config)
        {
            //将选项参数接口与配置文件绑定，并填入缺省值
            ModEnable = Config.Bind(Main.ModDisplayName, "enable", true, "【MOD开关】");
            //将选项参数接口与配置文件绑定，并填入缺省值
            debugMode = Config.Bind(Main.ModDisplayName, "debugMode", false, "【Debug模式开关】一般无需开启（开启后会拖慢性能）\n若有遭遇BUG，请尽量“先开启此项、再重试一遍遭遇BUG的场景，然后把输出的信息日志LOG提交给作者”");
            //许可协议声明
            modClaimInfo = Config.Bind(Main.ModDisplayName, "许可协议声明", "", "作者：aloneangel34\n许可协议声明：MIT许可\nMOD适配游戏版本：v0.2.8.4");

            //将选项参数接口与配置文件绑定，并填入缺省值
            //因为BepInEx的Mod没有Info.json，所以在这里记录（如果因MOD更新而改动了“描述/第四项参数”的话，“描述”的文字段会自动更新。包括“描述”被用户手动改掉、初始化后也会按这里的来更新。）

            //将选项参数接口与配置文件绑定，并填入缺省值（如果因MOD更新而改动了相应的描述的话，描述类的文字段这些会自动更新）
            //（小括号内的四个参数分别是【选项参数在配置文件里所属标签分类】，【选项参数在配置文件里的Key名称】，【选项参数的缺省值】，【选项参数的描述（如下所示，可以用\n等转义符）】）
            obscureGender = Config.Bind("主要功能", "obscureGender", true, "【模糊性别判定】\n变相实现可同性生子、可同性表白、可同性男媒女妁，且不会因同性行为产生流言蜚语。\n女性可参加比武招亲（包括玩家和NPC）\n本功能开启后，可以指定怀孕方\n【注意】该功能并不会禁止异性间的行为");
            unlockGangLevelGenderRequire = Config.Bind("主要功能", "unlockGangGenderRequire", true, "【取消门派、身份对性别的限制】\n门派与身份不再限定性别\n【注意】若开启了新生人物性别锁，请务必同时开启此项！\n（特别是开启新游戏时）\n不然因性别不符、NPC无法晋升高层，而新生人物性别始终只有一种，导致掌门缺失的情况】");
            specifyPregnantForTaiwu = Config.Bind("怀孕方指定", "specifyPregnantForTaiwu", 0, "【指定怀孕对象（太吾参与的场合）】\n指定有太吾参与时的怀孕方（开启“模糊性别判定”后生效）。\n0 按照太吾性别自动判定（太吾为男性，对象怀孕/太吾为女性，太吾怀孕）\n1 指定为太吾怀孕方\n2 指定对象为怀孕方\n3 50%随机指定怀孕方");
            specifyPregnantForOppsiteSex = Config.Bind("怀孕方指定", "specifyPregnantForOppsiteSex", 0, "【指定怀孕对象（异性NPC之间的场合）】\n指定异性NPC中的怀孕方（开启“模糊性别判定”后生效）。\n0 指定女性为怀孕方（游戏默认）\n1 指定男性为怀孕方\n2 50%随机指定怀孕方\n（同性NPC之间怀孕时，50%随机指定，不受该功能影响）");
            newActorGenderLock = Config.Bind("新生人物设定项", "newActorGenderLock", 0, "【新生人物性别锁定】\n仅对新生人物产生影响（剧情、剑冢人物除外）。\n不会对现有人物产生影响，所以若想要全世界单一性别，新在开启性别锁时开新档。\n0 不锁定，1 锁定男性，2 锁定女性\n理论上不会影响太吾人物");
            newActorNoOppositeGenderFace = Config.Bind("新生人物设定项", "newActorNoOppositeGenderFace", false, "【新人物不出现男生女相/女生男相】\n游戏原本有3%的几率产生，开启后新生人物绝对不会出现异性生相（剧情、剑冢人物除外）");
            newActorAllBisexual = Config.Bind("新生人物设定项", "newActorAllBisexual", false, "【新人物皆设为双性恋】\n游戏原本有20%的几率产生，开启后新生人物都会是双性恋（剧情、剑冢人物除外）\n若开启了本MOD的【模糊性别】功能，则此处开不开都无所谓\n游戏本身不包含纯同性恋取向");
            newActorInRegionCharmUp = Config.Bind("新生人物设定项", "newActorInRegionCharmUp", new bool[15], "【指定地域的新人物魅力上修】\n指定地域（省份）的新生人物，会根据人物的基础魅力进行魅力上修（剧情、剑冢人物除外）。\n基础魅力越低、则上修越多，700以上不调整。\n同时无父无母的新生人物会根据上修后的魅力重新生成面容\n（有父/母方的新人物则不会重新生成，不然就打破相貌继承了）");
            newActorCharmUpFactor = Config.Bind("新生人物设定项", "newActorCharmUpFactor", 20, "【魅力上调系数】\n设置范围为1～50，超限会变回默认值。\n一般建议设为20，变动不会太夸张。");
            displayFaceColors = Config.Bind("附带功能", "displayFaceColorIndex", 0, "【游戏显示肤色变更】\n0 显示游戏原本肤色，1 显示较深肤色，2 显示较浅肤色\n不会实际修改人物的肤色ID，只是在游戏中变更肤色ID所对应的显示颜色。\n即该功能不会改动存档数据\n可以理解为：“人物肤色无变化，只不过外界光照变暗/变亮了”");
            npcPassTurnCantChangeGang = Config.Bind("附带功能", "npcWontLeaveGangWhenTurnPass", new bool[16], "【禁止NPC在过月时脱离指定势力】\n理论上只在NPC过月行动时禁止，不会影响玩家的行动\n可以用来：比如留住村民、不会被娶走");

            //防止读取到非法数值（理论上应该由属性去控制的……但Config好像是一整个，不知道怎么下手去单独为某一项设限）
            if (specifyPregnantForTaiwu.Value > 3 || specifyPregnantForTaiwu.Value < 0) newActorGenderLock.Value = 0;
            if (specifyPregnantForOppsiteSex.Value > 2 || specifyPregnantForOppsiteSex.Value < 0) newActorGenderLock.Value = 0;
            if (newActorGenderLock.Value > 2 || newActorGenderLock.Value < 0) newActorGenderLock.Value = 0;
            if (newActorCharmUpFactor.Value < 1 || newActorCharmUpFactor.Value > 50) newActorCharmUpFactor.Value = 20;
            if (displayFaceColors.Value > 2 || displayFaceColors.Value < 0) displayFaceColors.Value = 0;
            if (newActorInRegionCharmUp.Value.Length != 15) newActorInRegionCharmUp.Value = new bool[15];
            if (npcPassTurnCantChangeGang.Value.Length != 16) npcPassTurnCantChangeGang.Value = new bool[16];

            //以配置数据设定 魅力上修的指定地区 所对应的BaseActorID列表
            for (int i = 0; i < newActorInRegionCharmUp.Value.Length; i++)
            {
                if (newActorInRegionCharmUp.Value[i])
                {
                    RegionCharmUpBaseActorIds.Add(i * 2 + 1);
                    RegionCharmUpBaseActorIds.Add(i * 2 + 2);
                }
            }
            //西域的「无量金刚宗」需要额外添加BaseActorID
            if (newActorInRegionCharmUp.Value[10])
            {
                RegionCharmUpBaseActorIds.Add(31);
                RegionCharmUpBaseActorIds.Add(32);
            }

            //以配置数据设定NPC在过月时禁止脱离的势力列表
            for (int i = 0; i < npcPassTurnCantChangeGang.Value.Length; i++)
            {
                if (npcPassTurnCantChangeGang.Value[i])
                {
                    CantChangeGangIds.Add(i + 1);
                }
            }
        }
    }
}
