## Taiwu_mods_BIE / 太吾绘卷游戏Mod BepInEx

for "The Scroll Of Taiwu" v0.2.8.4 / 对应游戏版本 v0.2.8.4

## GenderControl / 性别操控

## Functions / 功能：
1. 模糊性别判定（可指定怀孕方）
1. 解除门派/身份对性别的要求
2. 新生人物性别锁定
2. 新人物不出现 男生女相/女生男相
2. 新人物皆设为双性恋
2. 指定地域的新生人物魅力上修
3. 游戏显示肤色变更
3. 禁止NPC在过月时脱离指定势力

## Release Site / 发布地址:
https://bbs.nga.cn/read.php?tid=26301617

## Dependency Mod / 前置MOD:
* YanCore 1.5.1.1

## UpdataLog / 更新日志:

### v0.2.4
* **修复父/母方的BaseActorID与其自身的性别不符，导致婴儿的BaseActorID计算异常的问题**
（产生不符的原因可能有：使用了修改过的PresetActor_Date.txt文件）

### v0.2.3
* 修正“模糊性别判定”功能，在女太吾参加比武招亲奇遇时、未能正确生效的问题

### v0.2.2
* 解决“模糊性别判定时，导致人物特性（无根之人/石芯玉女互换）缓存不正确”的问题

### v0.2.1
* 解决“开启模糊性别判定后，由于春宵双方可以同时怀孕，导致NPC的孩子数量过多”的问题

### v0.2.0 重要更新！（之前的所有 v0.1.x 版本都有严重BUG，请停止使用）
* 增加一处“模糊性别判定”的作用领域
* **修复婴儿的BaseActorID计算异常的问题**
* 修复一处与YanLib对话事件的兼容问题(比如多彩生活MOD)
* 部分代码清理

### v0.1.4
* 添加辅助功能：“怀孕方指定” 可以指定 有太吾参与时的怀孕方 和 异性NPC之间的怀孕方(仅在开启“模糊性别判定”功能后可用)
* 修复“模糊性别判定”功能在关闭后依然生效的问题
* UI界面调整

### v0.1.3
* 增加一处“模糊性别判定”的作用领域

### v0.1.2
* “模糊性别判定”功能 添加同性可“男媒女妁”的效果
* Logger调整

### v0.1.1
* 关闭部分LoggerDebug级记录
* 针对报告的“婴儿诞生时、新生人物修正补丁出异常”BUG做了一个特化的LoggerInfo级记录报告

### v0.1.0
* 最初发布版本