using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ExampleCodeGenApp.Unity
{
    public class DataFileHelper
    {
        public static string SaveFile(string Filter = "(*.xml)|*.xml", string nameHead = "", bool appendNameTime = true, string InitialDirectory = null)
        {
            SaveFileDialog saveDg = new SaveFileDialog();
            saveDg.Filter = Filter;
            if (InitialDirectory != null)
                saveDg.InitialDirectory = InitialDirectory;
            else
                saveDg.RestoreDirectory = true;
            if (appendNameTime)
                nameHead += DateTime.Now.ToString("-yyyy_MMdd__HHmm_ssff");
            saveDg.FileName = nameHead;
            saveDg.AddExtension = true;
            if (saveDg.ShowDialog() == true)
            {
                return saveDg.FileName;
            }
            else
            {
                return null;
            }
        }
        public static string OpenFile(string Filter = "(*.xml)|*.xml", string name = "", string InitialDirectory = null)
        {
            OpenFileDialog saveDg = new OpenFileDialog();
            saveDg.Filter = Filter;
            if (name != "")
                saveDg.FileName = name;
            saveDg.AddExtension = true;
            saveDg.RestoreDirectory = true;
            if (!string.IsNullOrWhiteSpace(InitialDirectory))
                saveDg.InitialDirectory = InitialDirectory;
            if (saveDg.ShowDialog() == true)
            {
                return saveDg.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}
