using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ConsoleWriter : TextWriter
    {
        delegate void WriteFunc(string value);
        WriteFunc write;
        WriteFunc writeLine;
        Action<string> writerAction;
        public static void SetConsole(Action<string> action)
        {
            Console.SetOut(new ConsoleWriter(action));
        }
        public ConsoleWriter(Action<string> action)
        {
            write = Write;
            writeLine = WriteLine;
            writerAction = action;
        }

        /// <summary>
        /// 编码转换-UTF8
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
            //get { return Encoding.Unicode; }
        }

        /// <summary>
        /// 最低限度需要重写的方法
        /// </summary>
        public override void Write(string value)
        {
            //if (textBox.InvokeRequired)
            //{
            //    textBox.BeginInvoke(write, value);
            //}
            //else
            //{
            //    textBox.AppendText(value);
            //}
            writerAction(value);
        }

        /// <summary>
        /// 为提高效率直接处理一行的输出
        /// </summary>
        public override void WriteLine(string value)
        {
            //if (textBox.InvokeRequired)
            //{
            //    textBox.BeginInvoke(writeLine, value);
            //}
            //else
            //{
            //    textBox.AppendText(value);
            //    textBox.AppendText(this.NewLine);
            //}

            writerAction(value + "\r\n");
        }
    }
}
