using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using YanLib.ModHelper;
using UnityEngine;
using TaiwuUIKit.GameObjects;
using UnityUIKit.Core.GameObjects;
using UnityUIKit.GameObjects;
using HarmonyLib;

namespace GenderControl
{
    /// <summary>
    ///  Mod 入口
    /// </summary>
    [BepInPlugin(GUID, ModDisplayName, Version)]            //GUID, 在BepInEx中的显示名称, 版本
    [BepInProcess("The Scroll Of Taiwu Alpha V1.0.exe")]    //限制插件所能加载的程序
    [BepInDependency("0.0Yan.Lib")]                         //插件的硬性前置依赖MOD/插件
    public class Main : BaseUnityPlugin
    {
        /// <summary>插件版本</summary>
        public const string Version = "0.1.3";
        /// <summary>插件名字</summary>
        public const string ModDisplayName = "GenderControl/性别操控";
        /// <summary>插件ID</summary>
        public const string GUID = "TaiwuMOD.GenderControl";
        /// <summary>日志</summary> 
        public static new ManualLogSource Logger;   //声明一个ManualLogSource实例类的的静态字段
        /// <summary>MOD设置界面</summary>
        public static ModHelper Mod;                //声明一个ModHelper实例类的的静态字段
        /// <summary>设置</summary>
        public static Settings Setting;             //声明一个Settings实例类的静态字段
        /// <summary>用于处理长字符串</summary>
        public static StringBuilder SB;         //声明一个StringBuilder实例类的静态字段

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Logger = base.Logger;       //将Logger字段赋值为基类（BaseUnityPlugin）的Logger方法？
            Setting = new Settings();   //创建Settings实例类的对象、并赋值给Setting字段
            Setting.Init(Config);       //调用Init方法来进行设置选项的初始化
            SB = new StringBuilder();   //创建StringBuilder实例类的对象、并赋值给SB字段

            Harmony harmony = new Harmony(GUID);

            if (Setting.enable.Value)
            {
                try
                {
                    harmony.PatchAll();
                }
                catch (Exception ex)
                {
                    Logger.LogFatal("尝试加载Harmony补丁时出现异常");
                    Logger.LogFatal(ex);
                }
            }

            Mod = new ModHelper(GUID, ModDisplayName + Version);
            //设置MOD设置界面UI【自适应垂直UI组：MOD设置UI界面】这是MOD设置UI的总框(BOX)
            Mod.SettingUI = new BoxAutoSizeModelGameObject()
            {
                //该组件内的子组件排布设定
                Group =
                {
                    //子组件垂直排布
                    Direction = UnityUIKit.Core.Direction.Vertical,
                    //子组件排布间隔
                    Spacing = 10,
                    //组件边缘填充
                    Padding = { 10,0,0,0 },
                },
                //大小自适应设置
                SizeFitter =
                {
                    //垂直高度自适应
                    VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                },
                Children = 
                //该组件内的子组件
                {
                    //【MOD总开关】
                    new TaiwuToggle()
                    {
                        //组件的Name ID（是字符串，但并非该组件在游戏中的显示名称）
                        Name = "MOD总开关",
                        //组件的显示文本（会显示在UI上）
                        Text = Setting.enable.Value ? "MOD 已开启" : "MOD 已关闭",
                        //开关组件的开关状态
                        isOn = Setting.enable.Value,
                        //当数值改变时的操作
                        onValueChanged = (bool value, Toggle tg) =>
                        {
                            //将变动后的数值 赋值给 Setting.enabled.Value
                            //（bool型数值）“value”，是由这个动作 onValueChanged = (bool 「value」, Toggle tg) 传出来的
                            Setting.enable.Value = value;

                            //开关MOD时直接将会应用/卸载 Patch
                            //（该方式不需要YanCore作为支持，直接绿皮全开全关）
                            if (value)
                            {
                                try
                                {
                                    harmony.PatchAll();
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogFatal("尝试加载Harmony补丁时出现异常");
                                    Logger.LogFatal(ex);
                                }
                            }
                            else
                            {
                                try
                                {
                                    harmony.UnpatchAll();
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogFatal("尝试卸载Harmony补丁时出现异常");
                                    Logger.LogFatal(ex);
                                }
                            }

                            tg.Text = Setting.enable.Value ? "MOD 已开启" : "MOD 已关闭";

                            //遍历本组件（tg）的父组件（Parent）中的所有子组件（Children）【即可以理解为同级组件】
                            foreach (UnityUIKit.Core.ManagedGameObject managedGameObject in tg.Parent.Children)
                            {
                                //改变特定组件的显示、隐藏
                                if (managedGameObject.Name == "全体显示关闭")
                                {
                                    //切换组件的显示/隐藏“SetActive(value)”
                                    managedGameObject.SetActive(value);
                                    break;
                                }
                            }
                        },
                        //设定该元素的首选宽度、首选高度。
                        //边框留白大约12.5，上下合计 或 左右合计 留白大约25。（游戏中MOD设置UI中）单个文字长宽约25。
                        //例比如1个文字，建议设为{ 50, 50 }；6个文字，建议设为{ 175, 50 } (宽度为左右留白25 + 文字宽度25 x 文字字数6)
                        //如果想要 宽度, 高度 自适应，则将对应项设为0即可。除了TaiwuToggle()类，不建议两项皆设为0（可能会导致外边框不显示？我不确定）
                        Element = { PreferredSize = { 0, 60 } },
                        //粗体
                        UseBoldFont = true,
                    },
                    //【自适应垂直UI组：设定全体（装进来，方便开启和关闭显示）】
                    new BoxAutoSizeModelGameObject()
                    {
                        Name = "全体显示关闭",
                        //该组件内的子组件排布设定
                        Group =
                        {
                            //子组件垂直排布
                            Direction = UnityUIKit.Core.Direction.Vertical,
                            //子组件排布间隔
                            Spacing = 10,
                        },
                        //默认按照MOD总开关的值来确定是显示还是关闭
                        //（不加的话：若先将MOD设为关、然后退出重进，能看到虽然MOD启用按钮是灰色的，但下面的菜单却显示出来了）
                        DefaultActive = Setting.enable.Value,
                        //大小自适应设置
                        SizeFitter =
                        {
                            //垂直高度自适应
                            VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                        },
                        Children =
                        {
                            //【标签：选项设置 更多详细指路 分隔用】
                            new Container()
                            {
                                //不建议用设定 Name 的方式来辅助记忆，如果不需要调用操作的话，那组件就不用设定 Name 属性
                                Group =
                                {
                                    //子组件垂直排布
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    //子组件排布间隔
                                    Spacing = 10,
                                },
                                Element =
                                {
                                    //设定首选高度为60。
                                    PreferredSize = { 0 , 60 }
                                },
                                Children =
                                {
                                    //【标签：选项设置 分隔用】
                                    new TaiwuLabel()
                                    {
                                        //标签文本，采用<color=#DDCCAA>较亮</color><color=#998877>柔和</color>两种文本颜色。默认的颜色太暗了
                                        Text = "<color=#DDCCAA>选项设置</color><color=#998877>（更多详细说明请参看BepInEx\\config\\文件夹内的GenderControl.cfg配置文件）</color>",
                                        //设定首选高度为60。
                                        Element = { PreferredSize = { 0, 60 } },
                                        //粗体
                                        UseBoldFont = true,
                                        UseOutline = true,
                                    },
                                }
                            },
                            //【垂直UI容器：主要功能 分隔栏】
                            new Container()
                            {
                                Group =
                                {
                                    //子组件垂直排布
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    //子组件排布间隔
                                    Spacing = 10,
                                },
                                Element =
                                {
                                    //设定首选高度为60。
                                    PreferredSize = { 0 , 60 }
                                },
                                Children =
                                {
                                    //【标签：选项设置 分隔用】
                                    new TaiwuLabel()
                                    {
                                        //标签文本
                                        Text = "<color=#DDCCAA>主要功能</color><color=#998877></color>",
                                        //设定首选高度为60。
                                        Element = { PreferredSize = { 0, 60 } },
                                        //粗体
                                        UseBoldFont = true,
                                        UseOutline = true,
                                    },
                                }
                            },
                            //【水平UI组：模糊性别判定 开关】
                            new Container()
                            {
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 10,
                                },
                                Element =
                                {
                                    PreferredSize = { 0, 50 }
                                },
                                Children =
                                {
                                    //【开关：模糊性别判定】
                                    new TaiwuToggle()
                                    {
                                        Text = "模糊性别判定",
                                        isOn = Setting.obscureGender.Value,
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.obscureGender.Value = value;
                                        },
                                        Element = { PreferredSize = { 200 } }
                                    },
                                    //【标签：模糊性别判定 说明】
                                    new TaiwuLabel()
                                    {
                                        Text = "<color=#CCBB99>同性表白、同性生子、无同性流言蜚语、同性男媒女妁。女性参加比武招亲</color>",
                                        Element = { PreferredSize = { 0, 50 } },
                                        UseOutline = true,
                                    },
                                },
                            },
                            //【水平UI组：门派/身份不限性别 开关】
                            new Container()
                            {
                                Name = "性别解禁UI组",
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 10,
                                },
                                Element =
                                {
                                    PreferredSize = { 0, 50 }
                                },
                                Children =
                                {
                                    //【开关：门派/身份不限性别】
                                    new TaiwuToggle()
                                    {
                                        Name = "性别解禁开关",
                                        Text = "门派/身份不限性别",
                                        isOn = Setting.unlockGangLevelGenderRequire.Value,
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.unlockGangLevelGenderRequire.Value = value;
                                            if (value)
                                            {
                                                UnlockLevelGenderRequire.ResetLevelGender();
                                                //Logger.LogInfo("有没有被执行呀？开启");
                                            }
                                            else
                                            {
                                                UnlockLevelGenderRequire.UndoResetLevelGender();
                                                //Logger.LogInfo("有没有被执行呀？关闭");
                                            }
                                        },
                                        Element = { PreferredSize = { 200 } }
                                    },
                                    //【标签：门派/身份不限性别 说明】
                                    new TaiwuLabel()
                                    {
                                        Text = "<color=#CCBB99>开启性别锁时请同时开启此项！</color><color=#998877>不然因性别不符无法晋升高层，会产生掌门缺失现象。</color>",
                                        Element = { PreferredSize = { 0, 50 } },
                                        UseOutline = true,
                                    },
                                },
                            },
                            //【垂直UI容器：新生人物修正 分隔栏】
                            new Container()
                            {
                                //不建议用设定 Name 的方式来辅助记忆，如果不需要调用操作的话，那组件就不用设定 Name 属性
                                Group =
                                {
                                    //子组件垂直排布
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    //子组件排布间隔
                                    Spacing = 10,
                                    //组件边缘填充
                                    Padding = { 10,0,0,0 },
                                },
                                Element =
                                {
                                    //设定首选高度为60。
                                    PreferredSize = { 0 , 60 }
                                },
                                Children =
                                {
                                    //【标签：新生人物修正 分隔用】
                                    new TaiwuLabel()
                                    {
                                        //标签文本，采用<color=#DDCCAA>较亮</color><color=#998877>柔和</color>两种文本颜色。默认的颜色太暗了
                                        Text = "<color=#DDCCAA>新生人物修正</color><color=#998877>（包括新生婴儿和系统生成的成人。剧情、剑冢人物除外。不会对现存角色产生影响）</color>",
                                        //设定首选高度为60。
                                        Element = { PreferredSize = { 0, 60 } },
                                        //粗体
                                        UseBoldFont = true,
                                        UseOutline = true,
                                    },
                                }
                            },
                             //【水平UI组：新人物性别锁定 选项】
                            new Container()
                            {
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 10,
                                },
                                Element =
                                {
                                    //设定首选高度为50。
                                    PreferredSize = { 0, 50 }
                                },
                                Children =
                                {
                                    //【标签：新人物性别锁定 说明】
                                    new TaiwuLabel()
                                    {
                                        Text = "<color=#DDCCAA>新生人物性别锁定</color>",
                                        Element = { PreferredSize = { 0, 50 } },
                                        UseOutline = true,
                                    },
                                    //【开关组：不锁定/锁定男性/锁定女性】
                                    new ToggleGroup()
                                    {
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Children =
                                        { 
                                            //【开关：不启用锁定】
                                            new TaiwuToggle()
                                            {
                                                Text = "不启用性别锁",
                                                isOn = Setting.newActorGenderLock.Value == 0,
                                                //当数值改变时（开关按钮）
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    Setting.newActorGenderLock.Value = 0;
                                                },
                                                Element = { PreferredSize = { 175 } }
                                            },
                                            //【开关：锁定为男性】
                                            new TaiwuToggle()
                                            {
                                                Text = "锁定为男性",
                                                isOn = Setting.newActorGenderLock.Value == 1,
                                                //当数值改变时（开关按钮）
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    Setting.newActorGenderLock.Value = 1;

                                                    //因为【开启性别锁时，若不同步开启“门派身份性限制解除”会导致无法晋升、缺少掌门的情况】
                                                    //所以需要在开启性别锁的同时与其绑定
                                                    try
                                                    {
                                                        //在本开关组件的上一级（开关组）的上一级（水平UI组）的上一级（自适应垂直UI组）的下一级中找 Name属性 为“性别解禁UI组”的组件“n”
                                                        //在组件“n”的下一级中找 Name属性 为“性别解禁开关”的组件“m”
                                                        //再将组件“m”的类型强制转换为TaiwuToggle，这样就可以将该开关的 isOn属性 设为true
                                                        ((TaiwuToggle)((tg.Parent.Parent.Parent.Children.Find(n => n.Name == "性别解禁UI组")).Children.Find(m => m.Name == "性别解禁开关"))).isOn = true;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Logger.LogError(ex);
                                                    }


                                                },
                                                Element = { PreferredSize = { 175 } }
                                            },
                                            //【开关：锁定为女性】
                                            new TaiwuToggle()
                                            {
                                                Text = "锁定为女性",
                                                isOn = Setting.newActorGenderLock.Value == 2,
                                                //当数值改变时（开关按钮）
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    Setting.newActorGenderLock.Value = 2;

                                                    //因为【开启性别锁时，若不同步开启“门派身份性限制解除”会导致无法晋升、缺少掌门的情况】
                                                    //所以需要在开启性别锁的同时与其绑定
                                                    try
                                                    {
                                                        //在本开关组件的上一级（开关组）的上一级（水平UI组）的上一级（自适应垂直UI组）的下一级中找 Name属性 为“性别解禁UI组”的组件“n”
                                                        //在组件“n”的下一级中找 Name属性 为“性别解禁开关”的组件“m”
                                                        //再将组件“m”的类型强制转换为TaiwuToggle，这样就可以将该开关的 isOn属性 设为true
                                                        ((TaiwuToggle)((tg.Parent.Parent.Parent.Children.Find(n => n.Name == "性别解禁UI组")).Children.Find(m => m.Name == "性别解禁开关"))).isOn = true;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Logger.LogError(ex);
                                                    }
                                                },
                                                Element = { PreferredSize = { 175 } }
                                            },
                                        }
                                    },
                                    #region 大概翻了翻生生世世MOD，发现似乎并不与其冲突，作废
                                    /*
                                    //【开关：不锁定转世】
                                    new TaiwuToggle()
                                    {
                                        Text = "不锁定转世",
                                        isOn = Setting.newActorGenderLockExcludeSamsara.Value,
                                        //当数值改变时（开关按钮）
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.newActorGenderLockExcludeSamsara.Value = value;
                                        },
                                        Element = { PreferredSize = { 175 } }
                                    },
                                    */
                                    #endregion
                                }
                            },
                            //【水平UI组：新人物异性面相、性取向开关】
                            new Container()
                            {
                                //不建议用设定 Name 的方式来辅助记忆，如果不需要调用操作的话，那组件就不用设定 Name 属性
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 25,
                                },
                                Element =
                                {
                                    //设定首选高度为50。
                                    PreferredSize = { 0, 50 }
                                },
                                Children =
                                {
                                    //【开关：新人物皆无异性面相】
                                    new TaiwuToggle()
                                    {
                                        Text = "新生人物皆无 男生女相/女生男相",
                                        isOn = Setting.newActorNoOppositeGenderFace.Value,
                                        //当数值改变时（开关按钮）
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.newActorNoOppositeGenderFace.Value = value;
                                        },
                                        Element = { PreferredSize = { 0 } }
                                    },
                                    //【开关：新人物皆为双性恋者】
                                    new TaiwuToggle()
                                    {
                                        Text = "新生人物皆为双性恋者",
                                        isOn = Setting.newActorAllBisexual.Value,
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.newActorAllBisexual.Value = value;
                                        },
                                        Element = { PreferredSize = { 0 } }
                                    },
                                }
                            },
                            //【自适应垂直UI组：新人物魅力上调 综合】
                            new BoxAutoSizeModelGameObject()
                            {
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    Spacing = 10,
                                },
                                
                                //大小自适应设置
                                SizeFitter =
                                {
                                    //垂直高度自适应
                                    VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize
                                },
                                Children =
                                {
                                    //【水平UI组：魅力上调说明 与 上调系数设定】
                                    new Container()
                                    {
                                        Name = "魅力",
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0, 50 }
                                        },
                                        Children =
                                        {
                                            //【标签：魅力上修说明】
                                            new TaiwuLabel()
                                            {
                                                Text = "<color=#DDCCAA>所选地域的新生人物魅力上修↓</color>",
                                                Element = { PreferredSize = { 0, 50 } },
                                                UseOutline = true,
                                            },
                                            //【按钮：一键全部开启/关闭】
                                            new TaiwuButton()
                                            {
                                                //（没有做成开关，因为若这里设为“总开关”会和后面的“单项开关”容易产生混淆）
                                                //（比如：“总开关”明明亮着，但“单项开关”开关却有的亮着、有的不亮）
                                                Text = Settings.NextTimeAllRegionSetOn ? "一键全开" : "一键全关" ,
                                                FontColor =  Color.white,
                                                OnClick = (Button allRegion) =>
                                                {
                                                    #region 发现下面的代码在设定开关的同时，也会触发开关值变更的事件（OnValueChange）这里就弃用了
                                                    /*
                                                    //以一键记录重设每个开关项
                                                    for (int i = 0; i < Setting.newActorInRegionCharmUp.Value.Length; i++)
                                                    {
                                                        Setting.newActorInRegionCharmUp.Value[i] = Settings.NextTimeAllRegionSetOn;
                                                    }

                                                    //以一键记录重设对应的BaseActorIds列表
                                                    if(Settings.NextTimeAllRegionSetOn)
                                                    {
                                                        Settings.RegionCharmUpBaseActorIds.Clear(); //先清空

                                                        //再重设
                                                        //（本来15个地域，对应男女之后BaseActorID是1～30）
                                                        //（但无量金刚宗特别一点，其额外采用了31、32，所以多循环一次）
                                                        for (int i = 0; i < 16; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.Add(i);
                                                            Settings.RegionCharmUpBaseActorIds.Add(i+1);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.RegionCharmUpBaseActorIds.Clear(); //清空
                                                    }
                                                    */
                                                    #endregion 

                                                    //根据“记录值”更新【地域组】内所有开关的“开启/关闭”状态
                                                    foreach (UnityUIKit.Core.ManagedGameObject item in allRegion.Parent.Parent.Children)
                                                    {
                                                        //
                                                        if (item.Name != "魅力")
                                                        {
                                                            foreach (TaiwuToggle toggle in item.Children)
                                                            {
                                                                toggle.isOn = Settings.NextTimeAllRegionSetOn;
                                                            }
                                                        }
                                                    }

                                                    //将记录值改变，以便下一次的一键重设
                                                    Settings.NextTimeAllRegionSetOn = !Settings.NextTimeAllRegionSetOn;

                                                    //根据更新本按钮的文本
                                                    allRegion.Text = Settings.NextTimeAllRegionSetOn ? "一键全开" : "一键全关";


                                                    //调试
                                                    if (Main.Setting.debugMode.Value)
                                                    {
                                                        SB.AppendLine("一键设置后，魅力上修地域对应的BaseActorID列表：");
                                                        for (int i = 0; i < Settings.RegionCharmUpBaseActorIds.Count; i++)
                                                        {
                                                            SB.Append(Settings.RegionCharmUpBaseActorIds[i]);
                                                            SB.Append(" | ");
                                                        }
                                                        Logger.LogInfo(SB);
                                                        SB.Clear();
                                                    }
                                                },
                                                Element = { PreferredSize = { 150 } }
                                            },
                                            //【标签：上修系数说明】
                                            new TaiwuLabel()
                                            {
                                                Text = "<color=#998877>魅力上修系数</color>",
                                                Element = { PreferredSize = { 200, 50 } },
                                                UseOutline = true,
                                            },
                                            //【输入框：魅力上修系数】
                                            new TaiwuInputField()
                                            {
                                                Text = Setting.newActorCharmUpFactor.Value.ToString(),
                                                //输入栏为空时的提示文本
                                                Placeholder = "输入系数（1～50）",
                                                //限制输入的字符类型为整数数字
                                                ContentType = UnityEngine.UI.InputField.ContentType.IntegerNumber,
                                                //限制最多输入两个字符，配合上一行、即达成可输入范围：-9 ～ 99
                                                CharacterLimit = 2,
                                                OnEndEdit = (string factor, InputField input) =>
                                                {
                                                    //尝试将输入的字符串转换为整数
                                                    int.TryParse(factor, out int result);

                                                    //判定 result 是否1～50，若满足则修改魅力上修系数
                                                    if (result > 0 && result < 51)
                                                    {
                                                        Setting.newActorCharmUpFactor.Value = result;
                                                    }
                                                    //若不满足，则将输入框的显示文本重设回魅力上修系数
                                                    else
                                                    {   input.Text = Setting.newActorCharmUpFactor.Value.ToString();   }
                                                },
                                            },
                                        }
                                    },
                                    //【水平UI组：第01～05地域 开关】
                                    new Container()
                                    {
                                        Name = "地域组1",
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0, 50 }
                                        },
                                        Children =
                                        {
                                            //【开关：京畿「少林」】
                                            new TaiwuToggle()
                                            {
                                                Text = "京畿「少林」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[0],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[0] = value;
                                                    //更新列表（ID：1、2）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(1 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(1 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 1 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：巴蜀「峨眉」】
                                            new TaiwuToggle()
                                            {
                                                Text = "巴蜀「峨眉」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[1],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[1] = value;
                                                    //更新列表（ID：3、4）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(3 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(3 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 3 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：广南「百花」】
                                            new TaiwuToggle()
                                            {
                                                Text = "广南「百花」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[2],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[2] = value;
                                                    //更新列表（ID：5、6）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(5 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(5 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 5 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：荆北「武当」】
                                            new TaiwuToggle()
                                            {
                                                Text = "荆北「武当」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[3],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[3] = value;
                                                    //更新列表（ID：7、8）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(7 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(7 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 7 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：山西「元山」】
                                            new TaiwuToggle()
                                            {
                                                Text = "山西「元山」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[4],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[4] = value;
                                                    //更新列表（ID：9、10）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(9 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(9 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 9 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                        }
                                    },
                                    //【水平UI组：第06～10地域 开关】
                                    new Container()
                                    {
                                        Name = "地域组2",
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0, 50 }
                                        },
                                        Children =
                                        {
                                            //【开关：广东「狮相」】
                                            new TaiwuToggle()
                                            {
                                                Text = "广东「狮相」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[5],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[5] = value;
                                                    //更新列表（ID：11、12）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(11 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(11 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 11 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //
                                            //【开关：山东「然山」】
                                            new TaiwuToggle()
                                            {
                                                Text = "山东「然山」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[6],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[6] = value;
                                                    //更新列表（ID：13、14）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(13 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(13 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 13 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：荆南「璇女」】
                                            new TaiwuToggle()
                                            {
                                                Text = "荆南「璇女」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[7],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[7] = value;
                                                    //更新列表（ID：15、16）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(15 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(15 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 15 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //
                                            //辽东【开关：福建「铸剑」】
                                            new TaiwuToggle()
                                            {
                                                Text = "福建「铸剑」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[8],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[8] = value;
                                                    //更新列表（ID：17、18）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(17 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(17 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 17 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：辽东「空桑」】
                                            new TaiwuToggle()
                                            {
                                                Text = "辽东「空桑」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[9],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[9] = value;
                                                    //更新列表（ID：19、20）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(19 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(19 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 19 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                        }
                                    },
                                    //【水平UI组：第11～15地域 开关】
                                    new Container()
                                    {
                                        Name = "地域组3",
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0, 50 }
                                        },
                                        Children =
                                        {
                                            //【开关：西域「金刚」】
                                            new TaiwuToggle()
                                            {
                                                Text = "西域「金刚」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[10],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[10] = value;
                                                    //更新列表（ID：21、22、31、32）
                                                    //（无量金刚宗 和 其他西域人物 的BaseActorID是不同的，额外采用了31、32）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(21 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(21 + i);
                                                            }

                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(31 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(31 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 21 + i);
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 31 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：云南「五仙」】
                                            new TaiwuToggle()
                                            {
                                                Text = "云南「五仙」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[11],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[1] = value;
                                                    //更新列表（ID：23、24）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(23 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(23 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 23 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：淮南「界青」】
                                            new TaiwuToggle()
                                            {
                                                Text = "淮南「界青」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[12],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[13] = value;
                                                    //更新列表（ID：25、26）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(25 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(25 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 25 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：江南「伏龙」】
                                            new TaiwuToggle()
                                            {
                                                Text = "江南「伏龙」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[13],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[13] = value;
                                                    //更新列表（ID：27、28）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(27 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(27 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 27 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                            //【开关：江北「血犼」】
                                            new TaiwuToggle()
                                            {
                                                Text = "江北「血犼」",
                                                isOn = Setting.newActorInRegionCharmUp.Value[14],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.newActorInRegionCharmUp.Value[14] = value;
                                                    //更新列表（ID：29、30）
                                                    if (value)
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            if (!Settings.RegionCharmUpBaseActorIds.Contains(29 + i))
                                                            {
                                                                Settings.RegionCharmUpBaseActorIds.Add(29 + i);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < 2; i++)
                                                        {
                                                            Settings.RegionCharmUpBaseActorIds.RemoveAll(n => n == 29 + i);
                                                        }
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                        }
                                    },
                                },
                            },
                            //【垂直UI容器：其他附带功能 分隔栏】
                            new Container()
                            {
                                Group =
                                {
                                    //子组件垂直排布
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    //子组件排布间隔
                                    Spacing = 10,
                                    //组件边缘填充
                                    Padding = { 10,0,0,0 },
                                },
                                Element =
                                {
                                    //设定首选高度为60。
                                    PreferredSize = { 0 , 60 }
                                },
                                Children =
                                {
                                    //【标签：其他附带功能 分隔用】
                                    new TaiwuLabel()
                                    {
                                        //标签文本
                                        Text = "<color=#DDCCAA>其他附带功能</color><color=#998877></color>",
                                        //设定首选高度为60。
                                        Element = { PreferredSize = { 0, 60 } },
                                        //粗体
                                        UseBoldFont = true,
                                        UseOutline = true,
                                    },
                                }
                            },
                            
                            //【水平UI组：显示肤色变更 选项】
                            new Container()
                            {
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 10,
                                },
                                Element =
                                {
                                    //设定首选高度为50。
                                    PreferredSize = { 0, 50 }
                                },
                                Children =
                                {
                                    //【标签：显示肤色变更 说明】
                                    new TaiwuLabel()
                                    {
                                        Text = "<color=#DDCCAA>显示肤色变更</color><color=#998877>（不改变存档肤色数据）</color>",
                                        Element = { PreferredSize = { 0, 50 } },
                                        UseOutline = true,
                                    },
                                    new ToggleGroup()
                                    {
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Children =
                                        {
                                            //【开关：游戏默认】
                                            new TaiwuToggle()
                                            {
                                                Text = "游戏默认",
                                                isOn = Setting.displayFaceColors.Value == 0,
                                                //当数值改变时（开关按钮）
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    Setting.displayFaceColors.Value = 0;
                                                    DisplayFaceColor.ChangeDisplayColor(0);
                                                },
                                                Element = { PreferredSize = { 175 } }
                                            },
                                            //【开关：较深肤色】
                                            new TaiwuToggle()
                                            {
                                                Text = "较深肤色",
                                                isOn = Setting.displayFaceColors.Value == 1,
                                                //当数值改变时（开关按钮）
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    Setting.displayFaceColors.Value = 1;
                                                    DisplayFaceColor.ChangeDisplayColor(1);
                                                },
                                                Element = { PreferredSize = { 175 } }
                                            },
                                            //【开关：较浅肤色】
                                            new TaiwuToggle()
                                            {
                                                Text = "较浅肤色",
                                                isOn = Setting.displayFaceColors.Value == 2,
                                                //当数值改变时（开关按钮）
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    Setting.displayFaceColors.Value = 2;
                                                    DisplayFaceColor.ChangeDisplayColor(2);
                                                },
                                                Element = { PreferredSize = { 175 } }
                                            },
                                        },
                                    },
                                }
                            },
                            //【自适应垂直UI组：禁止NPC在过月时脱离指定势力 综合】
                            new BoxAutoSizeModelGameObject()
                            {
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Vertical,
                                    Spacing = 10,
                                },
                                //垂直大小自动适应
                                SizeFitter = { VerticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize },
                                Children =
                                {
                                    //【水平UI组：说明 与 一键开关】
                                    new Container()
                                    {
                                        Name = "门派",
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 25,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0, 50 }
                                        },
                                        Children =
                                        {
                                            //【标签：禁止NPC在过月时脱离所选势力】
                                            new TaiwuLabel()
                                            {
                                                Text = "<color=#DDCCAA>禁止NPC在过月行动时，脱离所选势力↓</color><color=#998877>（不影响玩家行动）</color>",
                                                Element = { PreferredSize = { 0 , 50 } },
                                                UseOutline = true,
                                            },
                                            //【按钮：一键全部开启/关闭】
                                            //（不做成开关，因为总开关回合后面的单独开关容易产生混淆）
                                            //（比如：总开关亮着、有的分开关却因为后续单独设定没亮）
                                            new TaiwuButton()
                                            {
                                                Text = Settings.NextTimeAllGangSetOn ? "一键全开" : "一键全关" ,
                                                FontColor =  Color.white,
                                                OnClick = (Button allGang) =>
                                                {
                                                    #region 发现下面的代码在设定开关的同时，也会触发开关值变更的事件（OnValueChange）这里就弃用了
                                                    /*
                                                    //以一键记录重设每个开关项
                                                    for (int i = 0; i < Setting.npcPassTurnCantChangeGang.Value.Length; i++)
                                                    {
                                                        Setting.npcPassTurnCantChangeGang.Value[i] = Settings.NextTimeAllGangSetOn;
                                                    }

                                                    //以一键记录重设对应的禁止脱离势力列表
                                                    if(Settings.NextTimeAllGangSetOn)
                                                    {
                                                        Settings.CantChangeGangIds.Clear(); //先清空

                                                        //再重设
                                                        //（15个门派势力、外加太吾村共16个。跳过了ID0因为那是“无门无派”的势力ID）
                                                        for (int i = 1; i <= 16; i++)
                                                        {
                                                            Settings.CantChangeGangIds.Add(i);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.Clear(); //清空
                                                    }
                                                    */
                                                    #endregion

                                                    //根据“记录值”更新【门派组】内所有开关的“开启/关闭”状态
                                                    foreach (UnityUIKit.Core.ManagedGameObject item in allGang.Parent.Parent.Children)
                                                    {
                                                        //
                                                        if (item.Name != "门派")
                                                        {
                                                            foreach (TaiwuToggle toggle in item.Children)
                                                            {
                                                                toggle.isOn = Settings.NextTimeAllGangSetOn;
                                                            }
                                                        }
                                                    }

                                                    //将记录值改变，以便下一次的一键重设
                                                    Settings.NextTimeAllGangSetOn = !Settings.NextTimeAllGangSetOn;

                                                    //更新本按钮的文本
                                                    allGang.Text = Settings.NextTimeAllGangSetOn ? "一键全开" : "一键全关";

                                                    //调试
                                                    if (Main.Setting.debugMode.Value)
                                                    {
                                                        SB.AppendLine("一键设置后，禁止脱离势力对应的GangID列表：");
                                                        for (int i = 0; i < Settings.CantChangeGangIds.Count; i++)
                                                        {
                                                            SB.Append(Settings.CantChangeGangIds[i]);
                                                            SB.Append(" | ");
                                                        }
                                                        Logger.LogDebug(SB);
                                                        SB.Clear();
                                                    }
                                                },
                                                Element = { PreferredSize = { 200 } }
                                            },
                                        },
                                    },
                                    //【水平UI组：第01～08势力 开关】
                                    new Container()
                                    {
                                        Name = "势力组一",
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0, 50 }
                                        },
                                        Children =
                                        {
                                            //【开关：「少林」】
                                            new TaiwuToggle()
                                            {
                                                Text = "少林派",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[0],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[0] = value;
                                                    //更新列表（ID：1）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(1))
                                                        {
                                                            Settings.CantChangeGangIds.Add(1);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 1);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「峨眉」】
                                            new TaiwuToggle()
                                            {
                                                Text = "峨眉派",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[1],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[1] = value;
                                                    //更新列表（ID：2）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(2))
                                                        {
                                                            Settings.CantChangeGangIds.Add(2);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 2);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「百花」】
                                            new TaiwuToggle()
                                            {
                                                Text = "百花谷",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[2],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[2] = value;
                                                    //更新列表（ID：3）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(3))
                                                        {
                                                            Settings.CantChangeGangIds.Add(3);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 3);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「武当」】
                                            new TaiwuToggle()
                                            {
                                                Text = "武当派",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[3],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[3] = value;
                                                    //更新列表（ID：4）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(4))
                                                        {
                                                            Settings.CantChangeGangIds.Add(4);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 4);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「元山」】
                                            new TaiwuToggle()
                                            {
                                                Text = "元山派",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[4],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[4] = value;
                                                    //更新列表（ID：5）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(5))
                                                        {
                                                            Settings.CantChangeGangIds.Add(5);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 5);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「狮相」】
                                            new TaiwuToggle()
                                            {
                                                Text = "狮相门",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[5],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[5] = value;
                                                    //更新列表（ID：6）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(6))
                                                        {
                                                            Settings.CantChangeGangIds.Add(6);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 6);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「然山」】
                                            new TaiwuToggle()
                                            {
                                                Text = "然山派",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[6],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[6] = value;
                                                    //更新列表（ID：7）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(7))
                                                        {
                                                            Settings.CantChangeGangIds.Add(7);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 7);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「璇女」】
                                            new TaiwuToggle()
                                            {
                                                Text = "璇女派",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[7],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[7] = value;
                                                    //更新列表（ID：8）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(8))
                                                        {
                                                            Settings.CantChangeGangIds.Add(8);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 8);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                        }
                                    },
                                    //【水平UI组：第09～16势力 开关】
                                    new Container()
                                    {
                                        Name = "势力组二",
                                        Group =
                                        {
                                            //子组件水平排布
                                            Direction = UnityUIKit.Core.Direction.Horizontal,
                                            Spacing = 10,
                                        },
                                        Element =
                                        {
                                            PreferredSize = { 0, 50 }
                                        },
                                        Children =
                                        {
                                            //辽东【开关：「铸剑」】
                                            new TaiwuToggle()
                                            {
                                                Text = "铸剑山庄",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[8],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[8] = value;
                                                    //更新列表（ID：9）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(9))
                                                        {
                                                            Settings.CantChangeGangIds.Add(9);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 9);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「空桑」】
                                            new TaiwuToggle()
                                            {
                                                Text = "空桑派",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[9],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[9] = value;
                                                    //更新列表（ID：10）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(10))
                                                        {
                                                            Settings.CantChangeGangIds.Add(10);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 10);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「金刚」】
                                            new TaiwuToggle()
                                            {
                                                Text = "金刚宗",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[10],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[10] = value;
                                                    //更新列表（ID：11）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(11))
                                                        {
                                                            Settings.CantChangeGangIds.Add(11);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 11);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「五仙」】
                                            new TaiwuToggle()
                                            {
                                                Text = "五仙教",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[11],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[11] = value;
                                                    //更新列表（ID：12）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(12))
                                                        {
                                                            Settings.CantChangeGangIds.Add(12);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 12);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「界青」】
                                            new TaiwuToggle()
                                            {
                                                Text = "界青门",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[12],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[12] = value;
                                                    //更新列表（ID：13）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(13))
                                                        {
                                                            Settings.CantChangeGangIds.Add(13);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 13);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「伏龙」】
                                            new TaiwuToggle()
                                            {
                                                Text = "伏龙坛",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[13],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[13] = value;
                                                    //更新列表（ID：14）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(14))
                                                        {
                                                            Settings.CantChangeGangIds.Add(14);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 14);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「血犼」】
                                            new TaiwuToggle()
                                            {
                                                Text = "血犼教",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[14],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[14] = value;
                                                    //更新列表（ID：15）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(15))
                                                        {
                                                            Settings.CantChangeGangIds.Add(15);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 15);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                            //【开关：「太吾村」】
                                            new TaiwuToggle()
                                            {
                                                Text = "太吾村",
                                                isOn = Setting.npcPassTurnCantChangeGang.Value[15],
                                                onValueChanged = (bool value, Toggle tg) =>
                                                {
                                                    //更新配置设定
                                                    Setting.npcPassTurnCantChangeGang.Value[15] = value;
                                                    //更新列表（ID：16）
                                                    if (value)
                                                    {
                                                        if (!Settings.CantChangeGangIds.Contains(16))
                                                        {
                                                            Settings.CantChangeGangIds.Add(16);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.CantChangeGangIds.RemoveAll(n => n == 16);
                                                    }
                                                },
                                                Element = { PreferredSize = { 125 } }
                                            },
                                        }
                                    },
                                },
                            },
                            //【水平UI组：Debug模式 开关】
                            new Container()
                            {
                                Group =
                                {
                                    //子组件水平排布
                                    Direction = UnityUIKit.Core.Direction.Horizontal,
                                    Spacing = 10,
                                },
                                Element =
                                {
                                    //设定首选高度为50。
                                    PreferredSize = { 0, 50 }
                                },
                                Children =
                                {
                                    //【开关：Debug模式】
                                    new TaiwuToggle()
                                    {
                                        Text = "Debug模式开关",
                                        isOn = Setting.debugMode.Value,
                                        //当数值改变时（开关按钮）
                                        onValueChanged = (bool value, Toggle tg) =>
                                        {
                                            Setting.debugMode.Value = value;
                                        },
                                        Element = { PreferredSize = { 200 } }
                                    },
                                    //【标签：Debug模式 说明】
                                    new TaiwuLabel()
                                    {
                                        Text = "<color=#998877>开启后输出简陋的调试信息，一般不必开启。（开启会拖慢性能）</color>",
                                        Element = { PreferredSize = { 0, 50 } },
                                        UseOutline = true,
                                    },
                                }
                            },
                        }
                    },
                }
            };
        }
    }
}
