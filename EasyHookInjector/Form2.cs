using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
namespace EasyHookInjector
{
    public partial class Form2 : Form
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        List<para> parameters = new List<para>();
        List<TextBox> textBoxes = new List<TextBox>();
        List<ComboBox> comboBoxes = new List<ComboBox>();
        struct para
        {
            public Type t;
            public string name;
        }
        public Form2()
        {
            InitializeComponent();
            textBoxes.Add(textBox1);
            comboBoxes.Add(comboBox1);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var path = Path.Combine(basePath, "Parameters.json");
            if (File.Exists(path))
            {
                StreamReader streamReader = new StreamReader(path);
                var off = streamReader.ReadToEnd();
                parameters = (List<para>)JsonConvert.DeserializeObject(off);
            }
            if(parameters!=null)
            {
                foreach (var item in parameters)
                {
                    var NewCombo = new ComboBox();
                    var Textbox = new TextBox();
                    Textbox.Location = new Point(98, textBoxes[textBoxes.Count - 1].Location.Y + 26);
                    Textbox.Name = $"textBox{textBoxes.Count + 1}";
                    Textbox.Size = new Size(100, 21);
                    Textbox.TabIndex = 2 + textBoxes.Count;
                    Textbox.Text = item.name;

                    NewCombo.Location = new Point(6, comboBoxes[comboBoxes.Count - 1].Location.Y + 26);
                    NewCombo.Name = $"comboBox{comboBoxes.Count + 1}";
                    NewCombo.Size = comboBox1.Size;

                    if(item.t==typeof(int))
                    {
                        NewCombo.SelectedIndex = 0;
                    }
                    else if (item.t==typeof(string))
                    {
                        NewCombo.SelectedIndex = 1;
                    }
                    else if (item.t==typeof(float))
                    {
                        NewCombo.SelectedIndex = 2;
                    }
                    groupBox1.Controls.Add(Textbox);
                    groupBox1.Controls.Add(NewCombo);

                    comboBoxes.Add(NewCombo);
                    textBoxes.Add(Textbox);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var NewCombo = new ComboBox();
            var Textbox = new TextBox();
            Textbox.Location = new Point(98, textBoxes[textBoxes.Count - 1].Location.Y + 26);
            Textbox.Name = $"textBox{textBoxes.Count + 1}";
            Textbox.Size = new Size(100, 21);
            Textbox.TabIndex = 2 + textBoxes.Count;


            NewCombo.Location = new Point(6, comboBoxes[comboBoxes.Count - 1].Location.Y + 26);
            NewCombo.Name = $"comboBox{comboBoxes.Count + 1}";
            NewCombo.Size = comboBox1.Size;
            foreach (var item in comboBox1.Items)
            {
                NewCombo.Items.Add(item);
            }
            //this.groupBox1.Controls.Add(this.comboBox1);
            groupBox1.Controls.Add(Textbox);
            groupBox1.Controls.Add(NewCombo);

            comboBoxes.Add(NewCombo);
            textBoxes.Add(Textbox);
        }
        void WriteJson()
        {
            var path = Path.Combine(basePath, "Parameters.json");
            StreamWriter streamWriter = new StreamWriter(path);
            var str= JsonConvert.SerializeObject(parameters);
            if (str != "") streamWriter.Write(str);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            para pp = new para();
            for (int i = 0; i < textBoxes.Count; i++)
            {
                if (textBoxes[i].Text != "")
                {
                    switch (comboBoxes[i].SelectedIndex)
                    {
                        case 1:
                            pp.t = typeof(string);
                            break;
                        case 2:
                            pp.t = typeof(float);
                            break;
                        default:
                            pp.t = typeof(int);
                            break;
                    }
                    pp.name = textBoxes[i].Text.Trim();
                }
                parameters.Add(pp);
            }
            WriteJson();
            this.Close();
        }
    }
}
