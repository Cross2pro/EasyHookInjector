using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace EasyHookInjector
{
    public partial class Form1 : Form
    {

        //Dictionary<string, int> pairs = new Dictionary<string, int>();

        [DllImport("kernel32.dll")]
        static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32.dll")]
        static extern long GetPrivateProfileString(string section, string key, string def, out string retval, int size, string filepath);
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);


        Form2 form2 = new Form2();
        List<string> processname = new List<string>();
        List<int> pid = new List<int>();
        List<DllParameter> dlls = new List<DllParameter>();
        [Obsolete]
        private bool RegGACAssembly()
        {
            var dllName = "EasyHook.dll";
            var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
            if (!System.Runtime.InteropServices.RuntimeEnvironment.FromGlobalAccessCache(Assembly.LoadFrom(dllPath)))
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(dllPath);
                Thread.Sleep(100);
            }

            dllName = "ClassLibrary1.dll";
            dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
            new System.EnterpriseServices.Internal.Publish().GacRemove(dllPath);
            if (!System.Runtime.InteropServices.RuntimeEnvironment.FromGlobalAccessCache(Assembly.LoadFrom(dllPath)))
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(dllPath);
                Thread.Sleep(100);
            }

            return true;
        }
        private bool RegGACAssemblys()
        {
            try
            {
                var dllName = "EasyHook.dll";
                var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
                if(!File.Exists( dllPath))
                {
                    throw new Exception("没有找到Dll文件");
                }
                if (!System.Runtime.InteropServices.RuntimeEnvironment.FromGlobalAccessCache(Assembly.LoadFrom(dllPath)))
                {
                    new System.EnterpriseServices.Internal.Publish().GacInstall(dllPath);
                    Thread.Sleep(100);
                }

                foreach (var item in dlls)
                {
                    dllName = item.dllPath;
                    if (!RuntimeEnvironment.FromGlobalAccessCache(Assembly.LoadFrom(dllPath)))
                    {
                        new System.EnterpriseServices.Internal.Publish().GacInstall(dllPath);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            //dllName = "ClassLibrary1.dll";
            //dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
            //new System.EnterpriseServices.Internal.Publish().GacRemove(dllPath);
            //if (!System.Runtime.InteropServices.RuntimeEnvironment.FromGlobalAccessCache(Assembly.LoadFrom(dllPath)))
            //{
            //    new System.EnterpriseServices.Internal.Publish().GacInstall(dllPath);
            //    Thread.Sleep(100);
            //}

            return true;
        }
        private static bool IsWin64Emulator(int processId)
        {
            var process = Process.GetProcessById(processId);
            if (process == null)
                return false;

            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                bool retVal;

                return !(IsWow64Process(process.Handle, out retVal) && retVal);
            }

            return false; // not on 64-bit Windows Emulator
        }

        private bool ExcludeEmptyList2(DllParameter parameter)
        {
            var finded = dlls.FindAll(a => a.dllPath == parameter.dllPath);
            if(finded!=null)
            {
                return true;
            }
            return false;
        }
        private static bool InstallHookInternal(int processId,string path)
        {
            try
            {
                var parameter = new HookParameter
                {
                    Msg = "已经成功注入目标进程",
                    HostProcessId = RemoteHooking.GetCurrentProcessId()
                };
                
                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    path,
                    path,
                    string.Empty,
                    parameter
                );
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }

            return true;
        }


        public void Refreshlist1()
        {
            listBox1.Items.Clear();
            Process[] processes = Process.GetProcesses();
            foreach (var item in processes)
            {
                processname.Add(item.ProcessName);
                pid.Add(item.Id);
                listBox1.Items.Add($"进程名：{item.ProcessName}\tPID：{item.Id}");
            }

        }
        public void Refreshlist2()
        {
            listBox2.Items.Clear();
            foreach (var item in dlls)
            {
                //if (dlls.Find(a => a.dllName.Equals(item.dllName)) == null)
                if (ExcludeEmptyList2(item))
                    listBox2.Items.Add($"DLL:   {item.dllName}");

            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Refreshlist1();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex==-1)
            {
                MessageBox.Show("请选择要注入的进程");
                return;
            }
            if(listBox2.SelectedIndex==-1)
            {
                MessageBox.Show("请选择用来注入的DLL");
                return;
            }
            try
            {
                RegGACAssemblys();
                if(IsWin64Emulator(pid[listBox1.SelectedIndex]))
                {
                    if (MessageBox.Show("您注入的进程是64位的，请确认操作", "注意：", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        return;
                }
                //if(!FindEntryPoint(dlls[listBox2.SelectedIndex].dllPath))
                //{
                //    MessageBox.Show("这不是一个EasyHook程序，无法注入");
                //    return;
                //}
                if(InstallHookInternal(pid[listBox1.SelectedIndex],dlls[listBox2.SelectedIndex].dllPath))
                {
                    MessageBox.Show("注入成功！");
                }
                else
                {
                    MessageBox.Show("注入失败！");
                }
            }
            catch (Exception ex)
            {
                //if(ex is BadImageFormatException)
                //{
                //    throw new Exception("要注入的dll或许有着附加项，请把附加项一并复制到dll所在目录");
                //}
                throw;
            }
            //MessageBox.Show(pid[listBox1.SelectedIndex].ToString());

        }

        private class HookParameter
        {
            public string Msg { get; internal set; }
            public int HostProcessId { get; internal set; }
        }
        private class DllParameter
        {
            public string dllPath { get;  set; }
            public string dllName { get; set; }
            public List<string> functions { get; set; }
            public List<MethodInfo> methodInfos { get; set; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                openFileDialog1.Title = "选择dll的路径";
                openFileDialog1.Filter = "dll文件(*.dll)|*.dll";
                openFileDialog1.RestoreDirectory = true;
                DllParameter dllParameter;
                string tmp = "";


                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    foreach (var item in openFileDialog1.FileNames)
                    {
                        tmp = Path.GetFileName(item);
                        dllParameter = new DllParameter { dllPath = item, dllName = tmp };
                        if (ExcludeEmptyList2(dllParameter)) dlls.Add(dllParameter);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            Refreshlist2();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //string stringBuilder = "";
            //string tmp = "";
            //DllParameter tmpdll=new DllParameter();
            //GetPrivateProfileString("DLL", "len", "",out stringBuilder, 1024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
            ////StringBuilder s[] = new StringBuilder[int.Parse(stringBuilder.ToString())];
            //for (int i = 0; i < int.Parse(stringBuilder.ToString()); i++)
            //{
            //    GetPrivateProfileString("DLL", $"dll{i}", "", out tmp, 1024, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
            //    tmpdll.dllPath = tmp;
            //    tmpdll.dllName = Path.GetFileName(tmp);
            //    dlls.Add(tmpdll);
            //}
            //Refreshlist2();
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json")))

            {
                StreamReader stream = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
                string json = stream.ReadToEnd();
                dlls = JsonConvert.DeserializeObject<List<DllParameter>>(json);
                stream.Close();
                Refreshlist2();
            }
            //JsonConvert.SerializeObject(dlls);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StreamWriter stream = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
            string json = JsonConvert.SerializeObject(dlls);
            foreach (var item in json)
            {
                stream.Write(item);
            }
            stream.Close();
            //if (dlls.Count == 0) return;
            //WritePrivateProfileString("DLL", "len",dlls.Count.ToString(), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));

            //for (int i = 0; i < dlls.Count; i++)
            //{
            //    WritePrivateProfileString("DLL", $"dll{i}", dlls[i].dllPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
            //}

            //XmlDocument.CreateComment("DATA")
        }
        static bool FindEntryPoint(string AssemblyPath)
        {
            try
            {
                if (String.IsNullOrEmpty(AssemblyPath)) return false;
                    var Assemblyobj = Assembly.Load(AssemblyPath);
                var exportedTypes = Assemblyobj.GetExportedTypes();
                foreach (var item in exportedTypes)
                {
                    if (item?.GetInterface("EasyHook.IEntryPoint") != null)
                    {
                        return true;
                    }

                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void listBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            

        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)//delete
            {
                    dlls.RemoveAt(listBox2.SelectedIndex);
                    listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            }
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string msg = "";
            string title = "";
            var tmp = sender as ListBox;
            if((tmp).SelectedIndex!=-1)
            {
                title = $"详细信息：{dlls[tmp.SelectedIndex].dllName}";

                msg += $"Name ： {dlls[tmp.SelectedIndex].dllName}\n";
                msg += $"Path ： {dlls[tmp.SelectedIndex].dllPath}\n";
                msg += $"Function ： {dlls[tmp.SelectedIndex].functions}\n";

                //msg += $"EntryPoint ： {dlls[tmp.SelectedIndex].methodInfos}\n";
                MessageBox.Show(msg, title);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            form2.Show();
        }
    }
}
