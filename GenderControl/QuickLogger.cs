using System.Text;
using BepInEx.Logging;

namespace GenderControl
{
    /// <summary>
    /// 自己的绿皮报错信息工具
    /// </summary>
    public static class QuickLogger
    {
        /// <summary>
        /// 用Main类中，创建的继承自BepInEx的Logging，按指定信息等级输出复合格式字符串信息
        /// </summary>
        /// <param name="level">输出信息的等级</param>
        /// <param name="formatString">复合格式字符串（例如"Year{0} Month{1}"这种）</param>
        /// <param name="stringArgs">要设置字符串格式的参数数组</param>
        public static void Log(LogLevel level, string formatString, params object[] stringArgs)
        {
            if (Main.SB == null)
            { Main.SB = new StringBuilder(); }

            Main.SB.Clear();                                    //用前清空（虽然感觉没必要，但以防万一吧）
            Main.SB.AppendFormat(formatString, stringArgs);     //调用StringBuilder处理复合格式字符串
            Main.Logger.Log(level, Main.SB.ToString());         //输出
            Main.SB.Clear();                                    //用后清空
        }
    }
}
