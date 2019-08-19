using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace GetVersionFromGit
{
    class Program
    {
        static void Main( string[] args )
        {
            string git_path = "git";
            string repository_path = null;
            string temp = null;

            if( args.Length > 0 )
            {
                repository_path = args[0];
            }

            if( args.Length > 1 )
            {
                git_path = args[1];
            }
            string mingling = null;
            if( git_path.Any( p => p == 32 ) )
            {
                mingling = string.Format( "\"{0}\" log", git_path );
            }
            else
            {
                mingling = string.Format( "{0} log", git_path );
            }
            var output = ExecBat( repository_path, "cmd.exe", mingling, ref temp );

            StringBuilder sb = new StringBuilder();
            sb.AppendLine( output );
            Console.WriteLine( Handle( sb.ToString() ) );
        }

        static string Handle( string str )
        {
            Regex reg = new Regex( @"(?<=commit\s*)[a-f0-9]+" );
            var m = reg.Match( str );
            int v1, v2;

            try
            {
                if( !m.Success )
                {
                    return "检测版本失败";
                }

                v1 = Convert.ToUInt16( m.Value.Substring( 0, 4 ), 16 );
                v2 = Convert.ToUInt16( m.Value.Substring( 4, 4 ), 16 );
            }
            catch( Exception e )
            {
                return e.ToString();
            }



            reg = new Regex( @"(?<=Date:\s*).+" );
            m = reg.Match( str );
            if( !m.Success )
            {
                return "匹配日期失败";
            }

            string[] ss = m.Value.Trim().Split( ' ' );
            //Thu Mar 21 13:26:04 2019 +0800
            //Thu, 21 Mar 2019 13:26:04 GMT
            DateTime dt = DateTime.Now;
            try
            {
                dt = DateTime.ParseExact( string.Format( "{0}, {1} {2} {3} {4} GMT", ss[0], ss[2].PadLeft( 2, '0' ), ss[1], ss[4], ss[3] ), "r", System.Globalization.CultureInfo.CurrentCulture );
            }
            catch( Exception e )
            {
                return e.ToString();
            }

            return string.Format( "{0}.{1}.{2}.{3}", dt.ToString( "yyMM" ), dt.ToString( "ddHH" ), v1, v2 );

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="batPath"></param>
        /// <param name="mingling"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static string ExecBat( string Path, string batPath, string mingling, ref string errMsg )
        {
            string outPutString = string.Empty;
            using( Process pro = new Process() )
            {
                FileInfo file = new FileInfo( batPath );
                pro.StartInfo.WorkingDirectory = Path;
                pro.StartInfo.FileName = batPath;
                pro.StartInfo.UseShellExecute = false;   //是否使用操作系统shell启动 
                pro.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 (不显示程序窗口)
                pro.StartInfo.RedirectStandardInput = true;  // 接受来自调用程序的输入信息 
                pro.StartInfo.RedirectStandardOutput = true;  // 由调用程序获取输出信息
                pro.StartInfo.RedirectStandardError = true;  //重定向标准错误输出
                pro.Start();                         // 启动程序
                pro.StandardInput.WriteLine( mingling ); //向cmd窗口发送输入信息
                pro.StandardInput.AutoFlush = true;
                // 前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
                pro.StandardInput.WriteLine( "exit" );

                outPutString = pro.StandardOutput.ReadToEnd();
                errMsg = pro.StandardError.ReadToEnd();

                pro.WaitForExit();


            }
            return outPutString;
        }
    }
}
