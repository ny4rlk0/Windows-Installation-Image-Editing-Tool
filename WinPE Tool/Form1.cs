using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinPE_Tool
{
    public partial class Form1 : Form
    {
        string startup_path = Application.StartupPath + "\\";
        //string sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System)+"\\CMD.exe /C";
        string sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\CMD.exe";
        string WimFolderPath, ImageFilePath;
        string doNotRepeat=" ";
        List<string> TimeZones = new List<string> { "Afghanistan Standard Time", "Alaskan Standard Time", "Aleutian Standard Time", "Altai Standard Time", "Arab Standard Time", "Arabian Standard Time", "Arabic Standard Time", "Argentina Standard Time", "Astrakhan Standard Time", "Atlantic Standard Time", "AUS Central Standard Time", "Aus Central W. Standard Time", "AUS Eastern Standard Time", "Azerbaijan Standard Time", "Azores Standard Time", "Bahia Standard Time", "Bangladesh Standard Time", "Belarus Standard Time", "Bougainville Standard Time", "Canada Central Standard Time", "Cape Verde Standard Time", "Caucasus Standard Time", "Cen. Australia Standard Time", "Central America Standard Time", "Central Asia Standard Time", "Central Brazilian Standard Time", "Central Europe Standard Time", "Central European Standard Time", "Central Pacific Standard Time", "Central Standard Time", "Central Standard Time (Mexico)", "Chatham Islands Standard Time", "China Standard Time", "Cuba Standard Time", "Dateline Standard Time", "E. Africa Standard Time", "E. Australia Standard Time", "E. Europe Standard Time", "E. South America Standard Time", "Easter Island Standard Time", "Eastern Standard Time", "Eastern Standard Time (Mexico)", "Egypt Standard Time", "Ekaterinburg Standard Time", "Fiji Standard Time", "FLE Standard Time", "Georgian Standard Time", "GMT Standard Time", "Greenland Standard Time", "Greenwich Standard Time", "GTB Standard Time", "Haiti Standard Time", "Hawaiian Standard Time", "India Standard Time", "Iran Standard Time", "Israel Standard Time", "Jordan Standard Time", "Kaliningrad Standard Time", "Kamchatka Standard Time", "Korea Standard Time", "Libya Standard Time", "Line Islands Standard Time", "Lord Howe Standard Time", "Magadan Standard Time", "Magallanes Standard Time", "Marquesas Standard Time", "Mauritius Standard Time", "Mid-Atlantic Standard Time", "Middle East Standard Time", "Montevideo Standard Time", "Morocco Standard Time", "Mountain Standard Time", "Mountain Standard Time (Mexico)", "Myanmar Standard Time", "N. Central Asia Standard Time", "Namibia Standard Time", "Nepal Standard Time", "New Zealand Standard Time", "Newfoundland Standard Time", "Norfolk Standard Time", "North Asia East Standard Time", "North Asia Standard Time", "North Korea Standard Time", "Omsk Standard Time", "Pacific SA Standard Time", "Pacific Standard Time", "Pacific Standard Time (Mexico)", "Pakistan Standard Time", "Paraguay Standard Time", "Qyzylorda Standard Time", "Romance Standard Time", "Russia Time Zone 10", "Russia Time Zone 11", "Russia Time Zone 3", "Russian Standard Time", "SA Eastern Standard Time", "SA Pacific Standard Time", "SA Western Standard Time", "Saint Pierre Standard Time", "Sakhalin Standard Time", "Samoa Standard Time", "Sao Tome Standard Time", "Saratov Standard Time", "SE Asia Standard Time", "Singapore Standard Time", "South Africa Standard Time", "South Sudan Standard Time", "Sri Lanka Standard Time", "Sudan Standard Time", "Syria Standard Time", "Taipei Standard Time", "Tasmania Standard Time", "Tocantins Standard Time", "Tokyo Standard Time", "Tomsk Standard Time", "Tonga Standard Time", "Transbaikal Standard Time", "Turkey Standard Time", "Turks And Caicos Standard Time", "Ulaanbaatar Standard Time", "US Eastern Standard Time", "US Mountain Standard Time", "UTC", "UTC+12", "UTC+13", "UTC-02", "UTC-08", "UTC-09", "UTC-11", "Venezuela Standard Time", "Vladivostok Standard Time", "Volgograd Standard Time", "W. Australia Standard Time", "W. Central Africa Standard Time", "W. Europe Standard Time", "W. Mongolia Standard Time", "West Asia Standard Time", "West Bank Standard Time", "West Pacific Standard Time", "Yakutsk Standard Time", "Yukon Standard Time" };
        bool lang = true;
        /// Fix to drag form without titlebar///////////////////////////
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
                message.Result = (IntPtr)HTCAPTION;
        }
        ////////////////////////////////////////////////////////////////
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ///Hide Title BAR///////////////////////////
            this.Text = string.Empty;
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            ////////////////////////////////////////////
            checkAdminAccess();//Will exit if admin access is denied!
            foreach(string tz in TimeZones)
                comboBox1.Items.Add(tz);
            try{comboBox1.SelectedIndex = comboBox1.FindString("Turkey Standard Time");}
            catch (Exception) { }
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    //MessageBox.Show("Dosya Sayısı: " + files.Length.ToString(), "Message");
                    //MessageBox.Show("Klasör Yolu: " + fbd.SelectedPath.ToString(), "Message");
                    textBox1.Text = fbd.SelectedPath.ToString();
                    WimFolderPath = fbd.SelectedPath.ToString();
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new OpenFileDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    //MessageBox.Show("Klasör Yolu: " + fbd.FileName.ToString(), "Message");
                    textBox2.Text = fbd.FileName.ToString();
                    ImageFilePath = fbd.FileName.ToString();
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null)
                CMD("/C dism /mount-image /index:" + numericUpDown1.Value.ToString() + " /imagefile:\"" + ImageFilePath.ToString() + "\" /MountDir:\"" + WimFolderPath.ToString() + "\"");

        }
        private void button4_Click(object sender, EventArgs e)
        {
            string commitdiscard = null;
            if (radioButton1.Checked)
                commitdiscard = " /discard";
            else if (radioButton2.Checked)
                commitdiscard = " /commit";
            if (ImageFilePath != null && WimFolderPath != null)
            { 
                CMD("/C dism /unmount-image /MountDir:\"" + WimFolderPath.ToString() + "\"" + commitdiscard);
                textBox1.Clear();
                textBox2.Clear();
                WimFolderPath = null;
                ImageFilePath = null;
            }
                
        }
        private void button5_Click(object sender, EventArgs e)
        { CMD("/C dism /get-mountedwiminfo"); }
        private void button7_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null)
                CMD("/C dism /get-imageinfo /imagefile:\"" + ImageFilePath + "\"");
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null && !checkBox2.Checked)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Get-Packages /format:List");
            else if (checkBox2.Checked)
                CMD("/C dism /Online /Get-Packages /format:List");
        }
        private void button8_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (ImageFilePath != null && WimFolderPath != null)
                    {
                        if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.SelectedPath.ToString() + "\"");
                        else if (checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.SelectedPath.ToString() + "\" /IgnoreCheck");
                        else if (!checkBox1.Checked && !checkBox2.Checked && checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.SelectedPath.ToString() + "\" /PreventPending");
                        else if (checkBox1.Checked && !checkBox2.Checked && checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.SelectedPath.ToString() + "\" /IgnoreCheck /PreventPending");
                        else if (!checkBox1.Checked && checkBox2.Checked && !checkBox3.Checked)
                            CMD("/C dism /Online /Add-Package /PackagePath:\"" + fbd.SelectedPath.ToString() + "\"");
                    }
                }
            }
        }
        private void button50_Click(object sender, EventArgs e)
        {
            using (var fbd = new OpenFileDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    if (ImageFilePath != null && WimFolderPath != null)
                    {
                        if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.FileName.ToString() + "\"");
                        else if (checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.FileName.ToString() + "\" /IgnoreCheck");
                        else if (!checkBox1.Checked && !checkBox2.Checked && checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.FileName.ToString() + "\" /PreventPending");
                        else if (checkBox1.Checked && !checkBox2.Checked && checkBox3.Checked)
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Package /PackagePath:\"" + fbd.FileName.ToString() + "\" /IgnoreCheck /PreventPending");
                        else if (!checkBox1.Checked && checkBox2.Checked && !checkBox3.Checked)
                            CMD("/C dism /Online /Add-Package /PackagePath:\"" + fbd.FileName.ToString() + "\"");
                    }
                }
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null && textBox3.Text.ToString() != null && !checkBox2.Checked)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Remove-Package /PackageName:\"" + textBox3.Text.ToString() + "\"");
            else if (textBox3.Text.ToString() != null && checkBox2.Checked)
                CMD("/C dism /Online /Remove-Package /PackageName:\"" + textBox3.Text.ToString() + "\"");
        }
        private void button10_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null && !checkBox2.Checked)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Get-Features /Format:List");
            else if (checkBox2.Checked)
                CMD("/C dism /Online /Get-Features /Format:List");
        }
        private void button11_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null && textBox4.Text.ToString() != null && !checkBox2.Checked)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Get-FeatureInfo /FeatureName:\"" + textBox4.Text.ToString() + "\"");
            else if (textBox4.Text.ToString() != null && checkBox2.Checked)
                CMD("/C dism /Online /Get-FeatureInfo /FeatureName:\"" + textBox4.Text.ToString() + "\"");
        }
        private void button12_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null && textBox4.Text.ToString() != null && !checkBox2.Checked)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Enable-Feature /FeatureName:\"" + textBox4.Text.ToString() + "\"");
            else if (textBox4.Text.ToString() != null && checkBox2.Checked)
                CMD("/C dism /Online /Enable-Feature /FeatureName:\"" + textBox4.Text.ToString() + "\"");
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null && textBox4.Text.ToString() != null && !checkBox2.Checked)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Disable-Feature /FeatureName:\"" + textBox4.Text.ToString() + "\"");
            else if (textBox4.Text.ToString() != null && checkBox2.Checked)
                CMD("/C dism /Online /Disable-Feature /FeatureName:\"" + textBox4.Text.ToString() + "\"");
        }
        private void button14_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    CMD("/C dism /Online /Get-PackageInfo /PackagePath:\"" + fbd.SelectedPath.ToString() + "\"");
            }
        }
        private void button15_Click(object sender, EventArgs e)
        {
            using (var fbd = new OpenFileDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    //MessageBox.Show("Klasör Yolu: " + fbd.FileName.ToString(), "Message");
                    CMD("/C dism /Online /Get-PackageInfo /PackagePath:\"" + fbd.FileName.ToString() + "\"");
                }
            }
        }
        private void button16_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Online /Cleanup-Image /ScanHealth");
        }
        private void button17_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Online /Cleanup-Image /RestoreHealth");
        }
        private void button18_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                    {
                        //MessageBox.Show("Klasör Yolu: " + fbd.FileName.ToString(), "Message");
                        CMD("/C dism /Online /Cleanup-Image /RestoreHealth /Source:\"" + fbd.FileName.ToString() + ":" + numericUpDown2.Value.ToString() + "\" /LimitAccess");
                    }
                }
                /*
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        CMD("/C dism /Online /Cleanup-Image /RestoreHealth /Source:\"" + fbd.SelectedPath.ToString() + "\" /LimitAccess");
                }*/
            }

        }
        private void button19_Click(object sender, EventArgs e)
        {
            using (var fbd = new OpenFileDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    //MessageBox.Show("Klasör Yolu: " + fbd.FileName.ToString(), "Message");
                    CMD("/C dism /Get-WimInfo /WimFile:\"" + fbd.FileName.ToString() + "\"");
                }
            }
        }
        private void button_20_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Online /Cleanup-Image /StartComponentCleanup");
        }
        private void button_21_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Online /Cleanup-Image /AnalyzeComponentStore");
        }
        private void button_22_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Online /Cleanup-Image /CheckHealth");
        }
        private void button_23_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Cleanup-Mountpoints");
        }
        private void button_24_Click(object sender, EventArgs e)
        {
            if (lang)
                MessageBox.Show("Onarılacak imajı seç!");
            else if (!lang)
                MessageBox.Show("Select image to repair!");
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (lang)
                        MessageBox.Show("Onarımda kullanılacak Wim dosyasını seç!");
                    else if (!lang)
                        MessageBox.Show("Select a wim file to use in repairing image!");
                    using (var fbd2 = new OpenFileDialog())
                    {
                        DialogResult result2 = fbd.ShowDialog();
                        if (result2 == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd2.FileName))
                            CMD("/C dism /Image:\"" + fbd.SelectedPath.ToString() + "\" /Cleanup-Image /RestoreHealth /Source:\"" + fbd2.FileName.ToString() + "\"");
                    }
                }
            }
        }
        private void button25_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Online /Get-Drivers /Format:List");
            else if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null)
                CMD("/C dism /Image:\"" + WimFolderPath + " \" /Get-Drivers /Format:List");
        }

        private void button26_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                CMD("/C dism /Online /Get-Drivers /all /Format:List");
            else if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null)
                CMD("/C dism /Image:\"" + WimFolderPath + " \" /Get-Drivers /all /Format:List");
        }
        private void button27_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked && !checkBox4.Checked)
                CMD("/C dism /Online /Get-DriverInfo /Driver:\"" + textBox5.Text.ToString() + "\"");
            else if (checkBox2.Checked && checkBox4.Checked)
            {
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                        CMD("/C dism /Online /Get-DriverInfo /Driver:\"" + fbd.FileName.ToString() + "\"");
                }
            }
            else if (!checkBox4.Checked && !checkBox2.Checked && ImageFilePath != null && WimFolderPath != null)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Get-DriverInfo /driver:\"" + textBox5.Text.ToString() + "\"");
            else if (checkBox4.Checked && !checkBox2.Checked && ImageFilePath != null && WimFolderPath != null)
            {
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                        CMD("/C dism /Image:\"" + WimFolderPath + "\" /Get-DriverInfo /driver:\"" + fbd.FileName.ToString() + "\"");
                }
            }
        }
        private void button28_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked)
            {
                //MULTIPLE INF FOLDER SELECTED
                if (!checkBox7.Checked && !checkBox5.Checked && !checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.SelectedPath.ToString() + "\"");
                    }
                }//2
                else if (!checkBox7.Checked && checkBox5.Checked && !checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.SelectedPath.ToString() + "\" /recurse");
                    }
                }//3
                else if (!checkBox7.Checked && !checkBox5.Checked && checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.SelectedPath.ToString() + "\" /ForceUnsigned");
                    }
                }//4
                else if (!checkBox7.Checked && checkBox5.Checked && checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.SelectedPath.ToString() + "\" /recurse /ForceUnsigned");
                    }
                }
                // SINGLE INF SELECTED
                if (checkBox7.Checked && !checkBox5.Checked && !checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new OpenFileDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.FileName.ToString() + "\"");
                    }
                }//2
                else if (checkBox7.Checked && checkBox5.Checked && !checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new OpenFileDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.FileName.ToString() + "\" /recurse");
                    }
                }//3
                else if (checkBox7.Checked && !checkBox5.Checked && checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new OpenFileDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.FileName.ToString() + "\" /ForceUnsigned");
                    }
                }//4
                else if (checkBox7.Checked && checkBox5.Checked && checkBox6.Checked && ImageFilePath != null && WimFolderPath != null)
                {
                    using (var fbd = new OpenFileDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                            CMD("/C dism /Image:\"" + WimFolderPath + "\" /Add-Driver /driver:\"" + fbd.FileName.ToString() + "\" /recurse /ForceUnsigned");
                    }
                }
            }
        }
        private void button29_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\""+WimFolderPath+"\" /Remove-Driver /driver:\""+textBox6.Text.ToString()+"\"");
        }
        private void button30_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        CMD("/C dism /Image:\"" + WimFolderPath + "\" /Export-Driver /Destination:\"" + fbd.SelectedPath.ToString() + "\"");
                }
            }
            else if (checkBox2.Checked) // <!> Perform Actions On Running Windows Installation <!>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        CMD("/C dism /Online /Export-Driver /Destination:\"" + fbd.SelectedPath.ToString() + "\"");
                }
            }
        }
        private void button31_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
            {
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                        CMD("/C dism /Image:\""+WimFolderPath+"\" /Apply-Unattend:\""+fbd.FileName.ToString()+"\"");
                }
            }
            else if (checkBox2.Checked) // <!> Perform Actions On Running Windows Installation <!>
            {
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                        CMD("/C dism /Online /Apply-Unattend:\"" + fbd.FileName.ToString() + "\"");
                }
            }
        }
        private void button33_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-AllIntl:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button53_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-InputLocale:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button55_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Get-Intl");
            else if(checkBox2.Checked)
                CMD("/C dism /Online /Get-Intl");
        }
        private void button56_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-UILangFallBack:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button57_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-SysLocale:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button58_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-UserLocale:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button54_Click(object sender, EventArgs e){
            string locale = "\nLanguage/Region\tPrimary input profile (language and keyboard pair)\tSecondary input profile\r\nAfrikaans\taf-ZA: US keyboard (0436:00000409)\t\r\nAfrikaans (Namibia)\ten-US: US keyboard (0409:00000409)\t\r\nAlbanian\tsq-AL: Albanian keyboard (041C:0000041C)\t\r\nAlbanian (Kosovo)\ten-US: US keyboard (0409:00000409)\t\r\nAlsatian (France)\tgsw-FR: French keyboard (0484:0000040C)\t\r\nAmharic\tam-ET: Amharic Input Method 2 (045E:{7C472071-36A7-4709-88CC-859513E583A9}{9A4E8FC7-76BF-4A63-980D-FADDADF7E987})\ten-US: US keyboard (0409:00000409)\r\nArabic\tar-SA: Arabic (101) keyboard (0401:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Algeria)\tar-DZ: Arabic (102) AZERTY keyboard (1401:00020401)\tfr-FR: French keyboard (040C:0000040C)\r\nArabic (Bahrain)\tar-BH: Arabic (101) keyboard (3C01:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Egypt)\tar-EG: Arabic (101) keyboard (0C01:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Iraq)\tar-IQ: Arabic (101) keyboard (0801:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Jordan)\tar-JO: Arabic (101) keyboard (2C01:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Kuwait)\tar-KW: Arabic (101) keyboard (3401:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Lebanon)\tar-LB: Arabic (101) keyboard (3001:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Libya)\tar-LY: Arabic (101) keyboard (1001:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Morocco)\tar-MA: Arabic (102) AZERTY keyboard (1801:00020401)\tfr-FR: French keyboard (040C:0000040C)\r\nArabic (Oman)\tar-OM: Arabic (101) keyboard (2001:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Qatar)\tar-QA: Arabic (101) keyboard (4001:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Syria)\tar-SY: Arabic (101) keyboard (2801:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (Tunisia)\tar-TN: Arabic (102) AZERTY keyboard (1C01:00020401)\tfr-FR: French keyboard (040C:0000040C)\r\nArabic (United Arab Emirates)\tar-AE: Arabic (101) keyboard (3801:00000401)\ten-US: US keyboard (0409:00000409)\r\nArabic (World)\ten-US: US keyboard (0409:00000409)\t\r\nArabic (Yemen)\tar-YE: Arabic (101) keyboard (2401:00000401)\ten-US: US keyboard (0409:00000409)\r\nArmenian\thy-AM: Armenian Phonetic keyboard (042B:0002042B)\thy-AM: Armenian Typewriter keyboard (042B:0003042B)\r\nru-RU: Russian keyboard (0419:00000419)\r\nAssamese\tas-IN: Assamese - INSCRIPT keyboard (044D:0000044D)\ten-US: US keyboard (0409:00000409)\r\nAsturian\tes-ES_tradnl: Spanish keyboard (040A:0000040A)\t\r\nAzerbaijani\taz-Latn-AZ: Azerbaijani Latin keyboard (042C:0000042C)\taz-Cyrl-AZ: Azerbaijani Cyrillic keyboard (082C:0000082C)\r\nen-US: US keyboard (0409:00000409)\r\nAzerbaijani (Cyrillic)\taz-Cyrl-AZ: Azerbaijani Cyrillic keyboard (082C:0000082C)\taz-Latn-AZ: Azerbaijani Latin keyboard (042C:0000042C)\r\nen-US: US keyboard (0409:00000409)\r\nBangla\tbn-BD: Bangla keyboard (0845:00000445)\ten-US: US keyboard (0409:00000409)\r\nBashkir\tba-RU: Bashkir keyboard (046D:0000046D)\tru-RU: Russian keyboard (0419:00000419)\r\nen-US: US keyboard (0409:00000409)\r\nBasque\teu-ES: Spanish keyboard (042D:0000040A)\t\r\nBelarusian\tbe-BY: Belarusian keyboard (0423:00000423)\ten-US: US keyboard (0409:00000409)\r\nru-RU: Russian keyboard (0419:00000419)\r\nBengali (India)\tbn-IN: Bangla - INSCRIPT keyboard (0445:00020445)\ten-US: US keyboard (0409:00000409)\r\nBodo\thi-IN: Devanagari - INSCRIPT keyboard (0439:00000439)\ten-US: US keyboard (0409:00000409)\r\nBosnian\tbs-Latn-BA: Standard keyboard (141A:0000041A)\t\r\nBosnian (Cyrillic)\tbs-Cyrl-BA: Bosnian (Cyrillic) keyboard (201A:0000201A)\tbs-Latn-BA: Standard keyboard (141A:0000041A)\r\nBreton\tbr-FR: French keyboard (047E:0000040C)\t\r\nBulgarian\tbg-BG: Bulgarian keyboard (0402:00030402)\ten-US: United States-International keyboard (0409:00020409)\r\nBurmese\tmy-MM: Myanmar (Visual order) keyboard (0455:00130C00)\ten-US: US keyboard (0409:00000409)\r\nCatalan\tca-ES: Spanish keyboard (0403:0000040A)\t\r\nCatalan (Andorra)\tfr-FR: French keyboard (040C:0000040C)\t\r\nCatalan (France)\tfr-FR: French keyboard (040C:0000040C)\t\r\nCatalan (Italy)\tit-IT: Italian keyboard (0410:00000410)\t\r\nCentral Atlas Tamazight\ttzm-Latn-DZ: Central Atlas Tamazight keyboard (085F:0000085F)\t\r\nCentral Atlas Tamazight (Arabic)\tar-MA: Arabic (102) AZERTY keyboard (1801:00020401)\tfr-FR: French keyboard (040C:0000040C)\r\nCentral Atlas Tamazight (Tifinagh)\ttzm-Tfng-MA: Tifinagh (Basic) keyboard (105F:0000105F)\tfr-FR: French keyboard (040C:0000040C)\r\nCentral Kurdish\tku-Arab-IQ: Central Kurdish keyboard (0492:00000492)\ten-US: US keyboard (0409:00000409)\r\nChechen\tru-RU: Russian keyboard (0419:00000419)\ten-US: US keyboard (0409:00000409)\r\nCherokee\tchr-Cher-US: Cherokee Nation keyboard (045C:0000045C)\tchr-Cher-US: Cherokee Phonetic keyboard (045C:0001045C)\r\nen-US: US keyboard (0409:00000409)\r\nChinese\tzh-CN: Microsoft Pinyin (0804:{81D4E9C9-1D3B-41BC-9E6C-4B40BF79E35E}{FA550B04-5AD7-411F-A5AC-CA038EC515D7})\t\r\nChinese (Simplified, Hong Kong SAR)\ten-US: US keyboard (0409:00000409)\t\r\nChinese (Simplified, Macao SAR)\ten-US: US keyboard (0409:00000409)\t\r\nChinese (Traditional)\tzh-TW: Microsoft Quick (0404:{531FDEBF-9B4C-4A43-A2AA-960E8FCDC732}{6024B45F-5C54-11D4-B921-0080C882687E})\t\r\nChinese (Traditional, Taiwan)\tzh-TW: Microsoft Bopomofo (0404:{B115690A-EA02-48D5-A231-E3578D2FDF80}{B2F9C502-1742-11D4-9790-0080C882687E})\t\r\nChurch Slavic\tru-RU: Russian keyboard (0419:00000419)\ten-US: US keyboard (0409:00000409)\r\nColognian\tde-DE: German keyboard (0407:00000407)\t\r\nCornish\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nCorsican\tco-FR: French keyboard (0483:0000040C)\t\r\nCroatian\thr-HR: Standard keyboard (041A:0000041A)\t\r\nCroatian (Bosnia & Herzegovina)\thr-BA: Standard keyboard (101A:0000041A)\t\r\nCzech\tcs-CZ: Czech keyboard (0405:00000405)\t\r\nDanish\tda-DK: Danish keyboard (0406:00000406)\ten-US: Danish keyboard (0409:00000406)\r\nDanish (Greenland)\tda-DK: Danish keyboard (0406:00000406)\t\r\nDivehi\tdv-MV: Divehi Phonetic keyboard (0465:00000465)\ten-US: US keyboard (0409:00000409)\r\nDutch\tnl-NL: United States-International keyboard (0413:00020409)\t\r\nDutch (Aruba)\ten-US: United States-International keyboard (0409:00020409)\t\r\nDutch (Belgium)\tnl-BE: Belgian (Period) keyboard (0813:00000813)\t\r\nDutch (Bonaire, Sint Eustatius and Saba)\ten-US: United States-International keyboard (0409:00020409)\t\r\nDutch (Curaçao)\ten-US: United States-International keyboard (0409:00020409)\t\r\nDutch (Sint Maarten)\ten-US: United States-International keyboard (0409:00020409)\t\r\nDutch (Suriname)\ten-US: United States-International keyboard (0409:00020409)\t\r\nDzongkha\tdz-BT: Dzongkha keyboard (0C51:00000C51)\ten-US: US keyboard (0409:00000409)\r\nEnglish\ten-US: US keyboard (0409:00000409)\t\r\nEnglish (Australia)\ten-AU: US keyboard (0C09:00000409)\t\r\nEnglish (Austria)\ten-GB: German keyboard (0809:00000407)\t\r\nEnglish (Belgium)\tfr-BE: Belgian French keyboard (080C:0000080C)\t\r\nEnglish (Belize)\ten-BZ: US keyboard (2809:00000409)\t\r\nEnglish (British Virgin Islands)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Burundi)\ten-GB: French keyboard (0809:0000040C)\t\r\nEnglish (Canada)\ten-CA: US keyboard (1009:00000409)\t\r\nEnglish (Caribbean)\ten-029: US keyboard (2409:00000409)\t\r\nEnglish (Denmark)\ten-US: Danish keyboard (0409:00000406)\t\r\nEnglish (Falkland Islands)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Finland)\ten-GB: Finnish keyboard (0809:0000040B)\t\r\nEnglish (Germany)\ten-GB: German keyboard (0809:00000407)\t\r\nEnglish (Gibraltar)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Guernsey)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Hong Kong SAR)\ten-HK: US keyboard (3C09:00000409)\t\r\nEnglish (India)\ten-IN: English (India) keyboard (4009:00004009)\t\r\nEnglish (Ireland)\ten-IE: Irish keyboard (1809:00001809)\t\r\nEnglish (Isle of Man)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Israel)\ten-US: US keyboard (0409:00000409)\the-IL: Hebrew (Standard) keyboard (040D:0002040D)\r\nEnglish (Jamaica)\ten-JM: US keyboard (2009:00000409)\t\r\nEnglish (Jersey)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Malaysia)\ten-MY: US keyboard (4409:00000409)\t\r\nEnglish (Malta)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Netherlands)\ten-GB: United States-International keyboard (0809:00020409)\t\r\nEnglish (New Zealand)\ten-NZ: NZ Aotearoa keyboard (1409:00001409)\t\r\nEnglish (Philippines)\ten-PH: US keyboard (3409:00000409)\t\r\nEnglish (Singapore)\ten-SG: US keyboard (4809:00000409)\t\r\nEnglish (Slovenia)\ten-GB: Slovenian keyboard (0809:00000424)\t\r\nEnglish (South Africa)\ten-ZA: US keyboard (1C09:00000409)\t\r\nEnglish (Sweden)\ten-GB: Swedish keyboard (0809:0000041D)\t\r\nEnglish (Switzerland)\ten-GB: German keyboard (0809:00000407)\t\r\nEnglish (Trinidad & Tobago)\ten-TT: US keyboard (2C09:00000409)\t\r\nEnglish (United Kingdom)\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nEnglish (Zimbabwe)\ten-ZW: US keyboard (3009:00000409)\t\r\nEstonian\tet-EE: Estonian keyboard (0425:00000425)\t\r\nFaroese\tfo-FO: Danish keyboard (0438:00000406)\t\r\nFilipino\tfil-PH: US keyboard (0464:00000409)\t\r\nFinnish\tfi-FI: Finnish keyboard (040B:0000040B)\t\r\nFrench\tfr-FR: French keyboard (040C:0000040C)\t\r\nFrench (Belgium)\tfr-BE: Belgian French keyboard (080C:0000080C)\t\r\nFrench (Cameroon)\tfr-CM: French keyboard (2C0C:0000040C)\t\r\nFrench (Canada)\tfr-CA: Canadian French keyboard (0C0C:00001009)\ten-CA: Canadian French keyboard (1009:00001009)\r\nFrench (Côte d’Ivoire)\tfr-CI: French keyboard (300C:0000040C)\t\r\nFrench (Haiti)\tfr-HT: French keyboard (3C0C:0000040C)\t\r\nFrench (Luxembourg)\tfr-LU: Swiss French keyboard (140C:0000100C)\tfr-LU: French keyboard (140C:0000040C)\r\nFrench (Mali)\tfr-ML: French keyboard (340C:0000040C)\t\r\nFrench (Monaco)\tfr-MC: French keyboard (180C:0000040C)\t\r\nFrench (Morocco)\tfr-MA: French keyboard (380C:0000040C)\t\r\nFrench (Réunion)\tfr-RE: French keyboard (200C:0000040C)\t\r\nFrench (Senegal)\tfr-SN: French keyboard (280C:0000040C)\t\r\nFrench (Switzerland)\tfr-CH: Swiss French keyboard (100C:0000100C)\tde-CH: Swiss German keyboard (0807:00000807)\r\nFrench Congo (DRC)\tfr-CD: French keyboard (240C:0000040C)\t\r\nFriulian\tit-IT: Italian keyboard (0410:00000410)\t\r\nFulah\tff-Latn-SN: Wolof keyboard (0867:00000488)\t\r\nFulah (Adlam)\ten-US: US keyboard (0409:00000409)\t\r\nFulah (Latin, Burkina Faso)\ten-US: US keyboard (0409:00000409)\t\r\nFulah (Latin, Cameroon)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Gambia)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Ghana)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Guinea)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Guinea-Bissau)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Liberia)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Mauritania)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Niger)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Nigeria)\two-SN: Wolof keyboard (0488:00000488)\t\r\nFulah (Latin, Sierra Leone)\two-SN: Wolof keyboard (0488:00000488)\t\r\nGalician\tgl-ES: Spanish keyboard (0456:0000040A)\t\r\nGeorgian\tka-GE: Georgian (QWERTY) keyboard (0437:00010437)\ten-US: US keyboard (0409:00000409)\r\nGerman\tde-DE: German keyboard (0407:00000407)\t\r\nGerman (Austria)\tde-AT: German keyboard (0C07:00000407)\t\r\nGerman (Belgium)\tfr-BE: Belgian French keyboard (080C:0000080C)\t\r\nGerman (Italy)\ten-US: US keyboard (0409:00000409)\t\r\nGerman (Liechtenstein)\tde-LI: Swiss German keyboard (1407:00000807)\t\r\nGerman (Luxembourg)\tde-LU: German keyboard (1007:00000407)\t\r\nGerman (Switzerland)\tde-CH: Swiss German keyboard (0807:00000807)\tfr-CH: Swiss French keyboard (100C:0000100C)\r\nGreek\tel-GR: Greek keyboard (0408:00000408)\ten-US: US keyboard (0409:00000409)\r\nGuarani\tgn-PY: Guarani keyboard (0474:00000474)\t\r\nGujarati\tgu-IN: Gujarati keyboard (0447:00000447)\ten-US: US keyboard (0409:00000409)\r\nHausa\tha-Latn-NG: Hausa keyboard (0468:00000468)\t\r\nHawaiian\thaw-US: Hawaiian keyboard (0475:00000475)\ten-US: US keyboard (0409:00000409)\r\nHebrew\the-IL: Hebrew (Standard) keyboard (040D:0002040D)\ten-US: US keyboard (0409:00000409)\r\nHindi\thi-IN: Hindi Traditional keyboard (0439:00010439)\ten-US: US keyboard (0409:00000409)\r\nHungarian\thu-HU: Hungarian keyboard (040E:0000040E)\t\r\nIcelandic\tis-IS: Icelandic keyboard (040F:0000040F)\t\r\nIgbo\tig-NG: Igbo keyboard (0470:00000470)\t\r\nIndonesian\tid-ID: US keyboard (0421:00000409)\t\r\nInterlingua\tfr-FR: French keyboard (040C:0000040C)\t\r\nInuktitut\tiu-Latn-CA: Inuktitut - Latin keyboard (085D:0000085D)\ten-CA: US keyboard (1009:00000409)\r\nInuktitut (Syllabics)\tiu-Cans-CA: Inuktitut - Naqittaut keyboard (045D:0001045D)\ten-CA: US keyboard (1009:00000409)\r\nIrish\tga-IE: Irish keyboard (083C:00001809)\t\r\nIrish (United Kingdom)\ten-US: US keyboard (0409:00000409)\t\r\nItalian\tit-IT: Italian keyboard (0410:00000410)\t\r\nItalian (Switzerland)\tit-CH: Swiss French keyboard (0810:0000100C)\tit-CH: Italian keyboard (0810:00000410)\r\nItalian (Vatican City)\ten-US: US keyboard (0409:00000409)\t\r\nJapanese\tja-JP: Microsoft IME (0411:{03B5835F-F03C-411B-9CE2-AA23E1171E36}{A76C93D9-5523-4E90-AAFA-4DB112F9AC76})\t\r\nJavanese\tjv: US keyboard (0C00:00000409)\t\r\nJavanese (Javanese)\tjv-Java: Javanese keyboard (0C00:00110C00)\ten-US: US keyboard (0409:00000409)\r\nKalaallisut\tkl-GL: Danish keyboard (046F:00000406)\t\r\nKannada\tkn-IN: Kannada keyboard (044B:0000044B)\ten-US: US keyboard (0409:00000409)\r\nKashmiri\tur-PK: Urdu keyboard (0420:00000420)\ten-US: US keyboard (0409:00000409)\r\nKashmiri (Devanagari)\thi-IN: Hindi Traditional keyboard (0439:00010439)\ten-US: US keyboard (0409:00000409)\r\nKazakh\tkk-KZ: Kazakh keyboard (043F:0000043F)\ten-US: US keyboard (0409:00000409)\r\nKhmer\tkm-KH: Khmer keyboard (0453:00000453)\ten-US: US keyboard (0409:00000409)\r\nKinyarwanda\trw-RW: US keyboard (0487:00000409)\t\r\nKiswahili\tsw-KE: US keyboard (0441:00000409)\t\r\nKiswahili (Congo DRC)\ten-US: US keyboard (0409:00000409)\t\r\nKiswahili (Tanzania)\ten-US: US keyboard (0409:00000409)\t\r\nKiswahili (Uganda)\ten-US: US keyboard (0409:00000409)\t\r\nKonkani\tkok-IN: Devanagari - INSCRIPT keyboard (0457:00000439)\ten-US: US keyboard (0409:00000409)\r\nKorean\tko-KR: Microsoft IME (0412:{A028AE76-01B1-46C2-99C4-ACD9858AE02F}{B5FE1F02-D5F2-4445-9C03-C568F23C99A1})\t\r\nKorean (North Korea)\ten-US: US keyboard (0409:00000409)\t\r\nKyrgyz\tky-KG: Kyrgyz Cyrillic keyboard (0440:00000440)\ten-US: US keyboard (0409:00000409)\r\nKʼicheʼ\tquc-Latn-GT: Latin American keyboard (0486:0000080A)\t\r\nLao\tlo-LA: Lao keyboard (0454:00000454)\ten-US: US keyboard (0409:00000409)\r\nLatvian\tlv-LV: Latvian (Standard) keyboard (0426:00020426)\t\r\nLithuanian\tlt-LT: Lithuanian keyboard (0427:00010427)\t\r\nLower Sorbian\tdsb-DE: Sorbian Standard keyboard (082E:0002042E)\t\r\nLuxembourgish\tlb-LU: Luxembourgish keyboard (046E:0000046E)\t\r\nMacedonian\tmk-MK: Macedonian - Standard keyboard (042F:0001042F)\ten-US: US keyboard (0409:00000409)\r\nMalagasy\tmg: French keyboard (0C00:0000040C)\t\r\nMalay\tms-MY: US keyboard (043E:00000409)\t\r\nMalay (Brunei)\tms-BN: US keyboard (083E:00000409)\t\r\nMalay (Indonesia)\ten-US: US keyboard (0409:00000409)\t\r\nMalay (Singapore)\ten-US: US keyboard (0409:00000409)\t\r\nMalayalam\tml-IN: Malayalam keyboard (044C:0000044C)\ten-US: US keyboard (0409:00000409)\r\nMaltese\tmt-MT: Maltese 47-Key keyboard (043A:0000043A)\t\r\nManipuri\ten-IN: English (India) keyboard (4009:00004009)\t\r\nManx\ten-GB: United Kingdom keyboard (0809:00000809)\t\r\nMaori\tmi-NZ: Maori keyboard (0481:00000481)\ten-NZ: Maori keyboard (1409:00000481)\r\nMapuche\tarn-CL: Latin American keyboard (047A:0000080A)\t\r\nMarathi\tmr-IN: Marathi keyboard (044E:0000044E)\ten-US: US keyboard (0409:00000409)\r\nMazanderani\tfa-IR: Persian keyboard (0429:00000429)\tfa-IR: Persian (Standard) keyboard (0429:00050429)\r\nen-US: US keyboard (0409:00000409)\r\nMohawk\tmoh-CA: US keyboard (047C:00000409)\t\r\nMongolian\tmn-MN: Mongolian Cyrillic keyboard (0450:00000450)\ten-US: US keyboard (0409:00000409)\r\nMongolian (Traditional Mongolian)\tmn-Mong-CN: Traditional Mongolian (Standard) keyboard (0850:00010850)\ten-US: US keyboard (0409:00000409)\r\nMongolian (Traditional Mongolian, Mongolia)\tmn-Mong-MN: Traditional Mongolian (Standard) keyboard (0C50:00010850)\ten-US: US keyboard (0409:00000409)\r\nN'ko\tnqo: N’Ko keyboard (0C00:00090C00)\ten-US: US keyboard (0409:00000409)\r\nNepali\tne-NP: Nepali keyboard (0461:00000461)\ten-US: US keyboard (0409:00000409)\r\nNepali (India)\tne-IN: Nepali keyboard (0861:00000461)\t\r\nNorthern Luri\tar-IQ: Arabic (101) keyboard (0801:00000401)\ten-US: US keyboard (0409:00000409)\r\nNorthern Sami\tse-NO: Norwegian with Sami keyboard (043B:0000043B)\t\r\nNorwegian\tnb-NO: Norwegian keyboard (0414:00000414)\t\r\nNorwegian Nynorsk\tnn-NO: Norwegian keyboard (0814:00000414)\t\r\nOccitan\toc-FR: French keyboard (0482:0000040C)\t\r\nOdia\tor-IN: Odia keyboard (0448:00000448)\ten-US: US keyboard (0409:00000409)\r\nOromo\tom-ET: US keyboard (0472:00000409)\t\r\nOromo (Kenya)\ten-US: US keyboard (0409:00000409)\t\r\nOssetic\tru-RU: Russian keyboard (0419:00000419)\ten-US: US keyboard (0409:00000409)\r\nPapiamento\tpap-029: US keyboard (0479:00000409)\t\r\nPashto\tps-AF: Pashto (Afghanistan) keyboard (0463:00000463)\ten-US: US keyboard (0409:00000409)\r\nPashto (Pakistan)\ten-US: US keyboard (0409:00000409)\t\r\nPersian\tfa-IR: Persian keyboard (0429:00000429)\tfa-IR: Persian (Standard) keyboard (0429:00050429)\r\nen-US: US keyboard (0409:00000409)\r\nPersian (Afghanistan)\tfa-AF: Persian (Standard) keyboard (048C:00050429)\ten-US: US keyboard (0409:00000409)\r\nPolish\tpl-PL: Polish (Programmers) keyboard (0415:00000415)\t\r\nPortuguese\tpt-BR: Portuguese (Brazil ABNT) keyboard (0416:00000416)\t\r\nPortuguese (Angola)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPortuguese (Cabo Verde)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPortuguese (Equatorial Guinea)\ten-US: US keyboard (0409:00000409)\t\r\nPortuguese (Guinea-Bissau)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPortuguese (Luxembourg)\ten-US: US keyboard (0409:00000409)\t\r\nPortuguese (Macao SAR)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPortuguese (Mozambique)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPortuguese (Portugal)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPortuguese (Switzerland)\ten-US: US keyboard (0409:00000409)\t\r\nPortuguese (São Tomé & Príncipe)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPortuguese (Timor-Leste)\tpt-PT: Portuguese keyboard (0816:00000816)\t\r\nPrussian\tde-DE: German keyboard (0407:00000407)\t\r\nPunjabi\tpa-IN: Punjabi keyboard (0446:00000446)\ten-US: US keyboard (0409:00000409)\r\nPunjabi\tpa-Arab-PK: Urdu keyboard (0846:00000420)\ten-US: US keyboard (0409:00000409)\r\nQuechua\tquz-BO: Latin American keyboard (046B:0000080A)\t\r\nQuechua (Ecuador)\tquz-EC: Latin American keyboard (086B:0000080A)\t\r\nQuechua (Peru)\tquz-PE: Latin American keyboard (0C6B:0000080A)\t\r\nRomanian\tro-RO: Romanian (Standard) keyboard (0418:00010418)\t\r\nRomanian (Moldova)\tro-MD: Romanian (Standard) keyboard (0818:00010418)\t\r\nRomansh\trm-CH: Swiss German keyboard (0417:00000807)\t\r\nRussian\tru-RU: Russian keyboard (0419:00000419)\ten-US: US keyboard (0409:00000409)\r\nRussian (Moldova)\ten-US: US keyboard (0409:00000409)\t\r\nSakha\tsah-RU: Sakha keyboard (0485:00000485)\tru-RU: Russian keyboard (0419:00000419)\r\nen-US: US keyboard (0409:00000409)\r\nSami (Inari)\tsmn-FI: Finnish with Sami keyboard (243B:0001083B)\t\r\nSami (Lule)\tsmj-SE: Swedish with Sami keyboard (143B:0000083B)\t\r\nSami (Skolt)\tsms-FI: Finnish with Sami keyboard (203B:0001083B)\t\r\nSami (Southern)\tsma-SE: Swedish with Sami keyboard (1C3B:0000083B)\t\r\nSami, Lule (Norway)\tsmj-NO: Norwegian with Sami keyboard (103B:0000043B)\t\r\nSami, Northern (Finland)\tse-FI: Finnish with Sami keyboard (0C3B:0001083B)\t\r\nSami, Northern (Sweden)\tse-SE: Swedish with Sami keyboard (083B:0000083B)\t\r\nSami, Southern (Norway)\tsma-NO: Norwegian with Sami keyboard (183B:0000043B)\t\r\nSanskrit\tsa-IN: Devanagari - INSCRIPT keyboard (044F:00000439)\ten-US: US keyboard (0409:00000409)\r\nScottish Gaelic\tgd-GB: Scottish Gaelic keyboard (0491:00011809)\t\r\nSerbian\tsr-Latn-RS: Serbian (Latin) keyboard (241A:0000081A)\t\r\nSerbian (Cyrillic)\tsr-Cyrl-RS: Serbian (Cyrillic) keyboard (281A:00000C1A)\ten-US: United States-International keyboard (0409:00020409)\r\nSerbian (Cyrillic, Bosnia and Herzegovina)\tsr-Cyrl-BA: Serbian (Cyrillic) keyboard (1C1A:00000C1A)\ten-US: US keyboard (0409:00000409)\r\nSerbian (Cyrillic, Kosovo)\ten-US: US keyboard (0409:00000409)\t\r\nSerbian (Cyrillic, Montenegro)\tsr-Cyrl-ME: Serbian (Cyrillic) keyboard (301A:00000C1A)\ten-US: United States-International keyboard (0409:00020409)\r\nSerbian (Latin, Bosnia & Herzegovina)\tsr-Latn-BA: Serbian (Latin) keyboard (181A:0000081A)\t\r\nSerbian (Latin, Kosovo)\ten-US: US keyboard (0409:00000409)\t\r\nSerbian (Latin, Montenegro)\tsr-Latn-ME: Serbian (Latin) keyboard (2C1A:0000081A)\t\r\nSesotho\tst-ZA: US keyboard (0430:00000409)\t\r\nSesotho (Lesotho)\ten-US: US keyboard (0409:00000409)\t\r\nSesotho sa Leboa\tnso-ZA: Sesotho sa Leboa keyboard (046C:0000046C)\t\r\nSetswana\ttn-ZA: Setswana keyboard (0432:00000432)\t\r\nSetswana (Botswana)\ttn-BW: Setswana keyboard (0832:00000432)\t\r\nShona\tjv: US keyboard (0C00:00000409)\t\r\nSindhi\tsd-Arab-PK: Urdu keyboard (0859:00000420)\ten-US: US keyboard (0409:00000409)\r\nSindhi (Devanagari)\thi-IN: Hindi Traditional keyboard (0439:00010439)\ten-IN: English (India) keyboard (4009:00004009)\r\nSinhala\tsi-LK: Sinhala keyboard (045B:0000045B)\ten-US: US keyboard (0409:00000409)\r\nSlovak\tsk-SK: Slovak keyboard (041B:0000041B)\t\r\nSlovenian\tsl-SI: Slovenian keyboard (0424:00000424)\t\r\nSomali\tso-SO: US keyboard (0477:00000409)\t\r\nSomali (Djibouti)\ten-US: US keyboard (0409:00000409)\t\r\nSomali (Ethiopia)\ten-US: US keyboard (0409:00000409)\t\r\nSomali (Kenya)\ten-US: US keyboard (0409:00000409)\t\r\nSpanish\tes-ES: Spanish keyboard (0C0A:0000040A)\t\r\nSpanish (Argentina)\tes-AR: Latin American keyboard (2C0A:0000080A)\t\r\nSpanish (Belize)\ten-US: US keyboard (0409:00000409)\t\r\nSpanish (Bolivia)\tes-BO: Latin American keyboard (400A:0000080A)\t\r\nSpanish (Brazil)\ten-US: US keyboard (0409:00000409)\t\r\nSpanish (Chile)\tes-CL: Latin American keyboard (340A:0000080A)\t\r\nSpanish (Colombia)\tes-CO: Latin American keyboard (240A:0000080A)\t\r\nSpanish (Costa Rica)\tes-CR: Latin American keyboard (140A:0000080A)\t\r\nSpanish (Cuba)\tes-MX: Latin American keyboard (080A:0000080A)\t\r\nSpanish (Dominican Republic)\tes-DO: Latin American keyboard (1C0A:0000080A)\t\r\nSpanish (Ecuador)\tes-EC: Latin American keyboard (300A:0000080A)\t\r\nSpanish (El Salvador)\tes-SV: Latin American keyboard (440A:0000080A)\t\r\nSpanish (Equatorial Guinea)\tes-MX: Latin American keyboard (080A:0000080A)\t\r\nSpanish (Guatemala)\tes-GT: Latin American keyboard (100A:0000080A)\t\r\nSpanish (Honduras)\tes-HN: Latin American keyboard (480A:0000080A)\t\r\nSpanish (Latin America)\tes-419: Latin American keyboard (580A:0000080A)\t\r\nSpanish (Mexico)\tes-MX: Latin American keyboard (080A:0000080A)\t\r\nSpanish (Nicaragua)\tes-NI: Latin American keyboard (4C0A:0000080A)\t\r\nSpanish (Panama)\tes-PA: Latin American keyboard (180A:0000080A)\t\r\nSpanish (Paraguay)\tes-PY: Latin American keyboard (3C0A:0000080A)\t\r\nSpanish (Peru)\tes-PE: Latin American keyboard (280A:0000080A)\t\r\nSpanish (Philippines)\tes-MX: Latin American keyboard (080A:0000080A)\t\r\nSpanish (Puerto Rico)\tes-PR: Latin American keyboard (500A:0000080A)\t\r\nSpanish (United States)\tes-US: Latin American keyboard (540A:0000080A)\ten-US: US keyboard (0409:00000409)\r\nSpanish (Uruguay)\tes-UY: Latin American keyboard (380A:0000080A)\t\r\nSpanish (Venezuela)\tes-VE: Latin American keyboard (200A:0000080A)\t\r\nStandard Moroccan Tamazight\tzgh: Tifinagh (Basic) keyboard (0C00:0000105F)\tfr-FR: French keyboard (040C:0000040C)\r\nSwedish\tsv-SE: Swedish keyboard (041D:0000041D)\t\r\nSwedish (Finland)\tsv-FI: Swedish keyboard (081D:0000041D)\t\r\nSwiss German\tde-CH: Swiss German keyboard (0807:00000807)\t\r\nSyriac\tsyr-SY: Syriac keyboard (045A:0000045A)\ten-US: US keyboard (0409:00000409)\r\nTachelhit\ttzm-Tfng-MA: Tifinagh (Basic) keyboard (105F:0000105F)\ten-US: US keyboard (0409:00000409)\r\nTachelhit (Latin)\ttzm-Latn-DZ: Central Atlas Tamazight keyboard (085F:0000085F)\t\r\nTajik\ttg-Cyrl-TJ: Tajik keyboard (0428:00000428)\ten-US: US keyboard (0409:00000409)\r\nTamil\tta-IN: Tamil 99 keyboard (0449:00020449)\ten-IN: English (India) keyboard (4009:00004009)\r\nTamil (Malaysia)\tta-IN: Tamil 99 keyboard (0449:00020449)\tms-MY: US keyboard (043E:00000409)\r\nTamil (Singapore)\tta-IN: Tamil 99 keyboard (0449:00020449)\ten-SG: US keyboard (4809:00000409)\r\nTamil (Sri Lanka)\tta-LK: Tamil 99 keyboard (0849:00020449)\ten-US: US keyboard (0409:00000409)\r\nTatar\ttt-RU: Tatar keyboard (0444:00010444)\ten-US: US keyboard (0409:00000409)\r\nru-RU: Russian keyboard (0419:00000419)\r\nTelugu\tte-IN: Telugu keyboard (044A:0000044A)\ten-US: US keyboard (0409:00000409)\r\nThai\tth-TH: Thai Kedmanee keyboard (041E:0000041E)\ten-US: US keyboard (0409:00000409)\r\nTibetan\tbo-CN: Tibetan (PRC) - Updated keyboard (0451:00010451)\ten-US: US keyboard (0409:00000409)\r\nTibetan (India)\tbo-CN: Tibetan (PRC) keyboard (0451:00000451)\ten-US: US keyboard (0409:00000409)\r\nTigrinya\tti-ET: Tigrinya Input Method (0473:{E429B25A-E5D3-4D1F-9BE3-0C608477E3A1}{3CAB88B7-CC3E-46A6-9765-B772AD7761FF})\ten-US: US keyboard (0409:00000409)\r\nTurkish\ttr-TR: Turkish Q keyboard (041F:0000041F)\t\r\nTurkmen\ttk-TM: Turkmen keyboard (0442:00000442)\ten-US: US keyboard (0409:00000409)\r\nUkrainian\tuk-UA: Ukrainian (Enhanced) keyboard (0422:00020422)\ten-US: US keyboard (0409:00000409)\r\nUpper Sorbian\thsb-DE: Sorbian Standard keyboard (042E:0002042E)\t\r\nUrdu\tur-PK: Urdu keyboard (0420:00000420)\ten-US: US keyboard (0409:00000409)\r\nUrdu (India)\tur-IN: Urdu keyboard (0820:00000420)\ten-US: US keyboard (0409:00000409)\r\nUyghur\tug-CN: Uyghur keyboard (0480:00010480)\ten-US: US keyboard (0409:00000409)\r\nUzbek\tuz-Latn-UZ: US keyboard (0443:00000409)\t\r\nUzbek (Arabic)\tps-AF: Pashto (Afghanistan) keyboard (0463:00000463)\ten-US: US keyboard (0409:00000409)\r\nUzbek (Cyrillic)\tuz-Cyrl-UZ: Uzbek Cyrillic keyboard (0843:00000843)\tuz-Latn-UZ: US keyboard (0443:00000409)\r\nValencian (Spain)\tca-ES-valencia: Spanish keyboard (0803:0000040A)\t\r\nVietnamese\tvi-VN: Vietnamese Telex (042A:{C2CB2CF0-AF47-413E-9780-8BC3A3C16068}{5FB02EC5-0A77-4684-B4FA-DEF8A2195628})\tvi-VN: Vietnamese Number Key-Based (042A:{C2CB2CF0-AF47-413E-9780-8BC3A3C16068}{591AE943-56BE-48F6-8966-06B43915CC5A})\r\nvi-VN: Vietnamese keyboard (042A:0000042A)\r\nen-US: US keyboard (0409:00000409)\r\nWalser\tde-CH: Swiss German keyboard (0807:00000807)\t\r\nWelsh\tcy-GB: United Kingdom Extended keyboard (0452:00000452)\ten-GB: United Kingdom keyboard (0809:00000809)\r\nWestern Frisian\tfy-NL: United States-International keyboard (0462:00020409)\t\r\nWolof\two-SN: Wolof keyboard (0488:00000488)\t\r\nXitsonga\tts-ZA: US keyboard (0431:00000409)\t\r\nYi\tii-CN: Yi Input Method (0478:{E429B25A-E5D3-4D1F-9BE3-0C608477E3A1}{409C8376-007B-4357-AE8E-26316EE3FB0D})\tzh-CN: Microsoft Pinyin (0804:{81D4E9C9-1D3B-41BC-9E6C-4B40BF79E35E}{FA550B04-5AD7-411F-A5AC-CA038EC515D7})\r\nYoruba\tyo-NG: Yoruba keyboard (046A:0000046A)\t\r\nisiXhosa\txh-ZA: US keyboard (0434:00000409)\t\r\nisiZulu\tzu-ZA: US keyboard (0435:00000409)\t";
            richTextBox1.Text=richTextBox1.Text+locale;
        }
        private void button38_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-UILang:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button39_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-SysUILang:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button40_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-SetupUILang:\"" + textBox7.Text.ToString() + "\" /distribution:\"" + ImageFilePath + "\"");
        }
        private void button32_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-TimeZone:\"" + comboBox1.SelectedText.ToString() + "\"");
        }
        private void button34_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-SKUIntlDefaults:\"" + textBox7.Text.ToString() + "\"");
        }
        private void button35_Click(object sender, EventArgs e)
        {
            string info = "The possible values for these settings are 1, 2, 3, 4, 5, 6:\r\n\r\n1 Specifies the PC/AT Enhanced Keyboard (101/102-Key).\r\n2 Specifies the Korean PC/AT 101-Key Compatible Keyboard/MS Natural Keyboard (Type 1).\r\n3 Specifies the Korean PC/AT 101-Key Compatible Keyboard/MS Natural Keyboard (Type 2).\r\n4 Specifies the Korean PC/AT 101-Key Compatible Keyboard/MS Natural Keyboard (Type 3).\r\n5 Specifies the Korean Keyboard (103/106 Key).\r\n6 Specifies the Japanese Keyboard (106/109 Key). ";
            richTextBox1.Text = richTextBox1.Text + "\n\n" + info;
        }
        private void button36_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) 
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-LayeredDriver:\"" + numericUpDown3.Value.ToString() + "\"");
        }
        private void button37_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) 
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Gen-LangINI /distribution:\"" + ImageFilePath + "\"");
        }
        private void button42_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) 
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Get-ProvisionedAppxPackages");
            else if (checkBox2.Checked) // <!> Perform Actions On Running Windows Installation <!>
                CMD("/C dism /Online /Get-ProvisionedAppxPackages");
        }
        private void button43_Click(object sender, EventArgs e)
        {
            //Entire Appx Package / Folder Installing Logic. Even empty space has meaning. Do Not Delete it!
            string baseCMD = "/C dism";
            if (!checkBox8.Checked &&!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null)//Yüklemek İçin Paket Dosyası Seçilecek
            {
                baseCMD = " /Image:\"" + WimFolderPath + " /Add-ProvisionedAppxPackage";
                if (lang)
                    MessageBox.Show("Paket dosyasını seçin. (Dosya!)");
                else if (!lang)
                    MessageBox.Show("Select package file. (File!)");
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                        baseCMD = baseCMD+" /PackagePath:\""+fbd.FileName.ToString()+"\"";
                }
            }
            else if (checkBox8.Checked && checkBox2.Checked)//Yüklemek İçin Paketi İçeren Klasör Seçilecek // <!> Perform Actions On Running Windows Installation <!>
            {
                baseCMD = " /Online /Add-ProvisionedAppxPackage";
                if (lang)
                    MessageBox.Show("Paket klasörünü seçin. (Klasör!)");
                else if (!lang)
                    MessageBox.Show("Select package folder. (Folder!)");
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        baseCMD = baseCMD + " /FolderPath:\"" + fbd.SelectedPath.ToString() + "\"";
                }
            }
            if(textBox9.Text!=null &&textBox9.Text!=""&&checkBox14.Checked)
                baseCMD = baseCMD + " /Region:\"" + textBox9.Text.ToString() + "\"";
            if (checkBox12.Checked&&!checkBox13.Checked)
                baseCMD = baseCMD + " /stubpackageoption:installstub";
            else if (!checkBox12.Checked&&checkBox13.Checked)
                baseCMD = baseCMD + " /stubpackageoption:installfull";

            if (checkBox10.Checked) //Özelleştirilmiş veri dosyası ekleyin
            {
                if (lang)
                    MessageBox.Show("Özelleştirilmiş veri dosyasını seçin. (Dosya!)");
                else if (!lang)
                    MessageBox.Show("Select a customized data file. (File!)");
                using (var fbd2 = new OpenFileDialog())
                {
                    DialogResult result2 = fbd2.ShowDialog();
                    if (result2 == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd2.FileName))
                        baseCMD = baseCMD + " /CustomDataPath:\"" + fbd2.FileName.ToString() + "\"";
                }
            }
            if (checkBox11.Checked)//Paket Gereksinimlerini Ekleyin
            {
                for (int i = 1; i <= Convert.ToInt32(numericUpDown4.Value.ToString()); i++)
                {
                    if (lang)
                        MessageBox.Show("Paket Gereksinimlerini Seçin. (Dosya!)");
                    else if (!lang)
                        MessageBox.Show("Select package dependencies. (File!)");
                    using (var fbd3 = new OpenFileDialog())
                    {
                        DialogResult result3 = fbd3.ShowDialog();
                        if (result3 == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd3.FileName))
                            baseCMD = baseCMD + " /DependencyPackagePath:\"" + fbd3.FileName.ToString() + "\"";
                    }
                }
            }
            if (checkBox9.Checked)//Lisans kontrolünü atla
                baseCMD = baseCMD + " /SkipLicense";
            else if (!checkBox9.Checked)
            {
                if (lang)
                    MessageBox.Show("Paket Lisans Dosyasını Seçin. (Dosya!)");
                else if (!lang)
                    MessageBox.Show("Select License File to Add Package (File!)");
                using (var fbd3 = new OpenFileDialog())
                {
                    DialogResult result3 = fbd3.ShowDialog();
                    if (result3 == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd3.FileName))
                        baseCMD = baseCMD + " /LicensePath:\"" + fbd3.FileName.ToString() + "\"";
                }
            }
            CMD(baseCMD); //Kodu Hazırladın Artık Çalıştır!
        }
        private void button44_Click(object sender, EventArgs e)
        {
            string info = "\nUse /Region to specify what regions an app package (.appx or .appxbundle) must be provisioned in.\nThe region argument can either be:\r\n\r\nall, indicating that the app should be provisioned for all regions, or\r\nA semi-colon delimited list of regions. The regions will be in the form of ISO 3166-1 Alpha-2 or ISO 3166-1 Alpha-3 codes.\r\nFor example, the United States can be specified as either \"US\" or \"USA\" (case-insensitive).\r\nWhen a list of regions is not specified, the package will be provisioned only if it is pinned to start layout.\r\nLang Options: https://en.wikipedia.org/wiki/ISO_3166-1";
            richTextBox1.Text = richTextBox1.Text + "\n\n" + info;
        }

        private void button45_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Remove-ProvisionedAppxPackage /PackageName:\"" + textBox10.Text.ToString() + "\"");
        }
        private void button46_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null)
                CMD("/C dism /Image:\"" + WimFolderPath + "\" /Optimize-ProvisionedAppxPackages");
        }
        private void button47_Click(object sender, EventArgs e)
        {//DISM.exe /Image:C:\test\offline /Set-ProvisionedAppxDataFile /CustomDataPath:c:\Test\Apps\Custom.dat /PackageName:microsoft.appx.app1_1.0.0.0_neutral_ac4zc6fex2zjp
            if (!checkBox2.Checked && ImageFilePath != null && WimFolderPath != null) //If it is a offline image not an running windows installation
            {
                if (lang)
                    MessageBox.Show(textBox10.Text.ToString()+" adlı pakete eklemek için dosya seçin.");
                else if (!lang)
                    MessageBox.Show(textBox10.Text.ToString() + " select a file to add this package.");
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                        CMD("/C dism /Image:\"" + WimFolderPath + "\" /Set-ProvisionedAppxDataFile /CustomDataPath:\"" + fbd.FileName.ToString()+ "\" /PackageName:\"" + textBox10.Text.ToString() + "\"");
                }
            }

        }
        private void button48_Click(object sender, EventArgs e)
        { //Subroutine that saved me deleting work of 2 hours 
            List<string> TimeZones = new List<string>();
            try
            {
                using (var fbd = new OpenFileDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                    {
                        using (var fileStream = File.OpenRead(fbd.FileName.ToString()))
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                        {
                            String line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                if (line.Contains("[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\"))
                                {
                                    line = line.Replace("[HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\", "");
                                    line = line.Replace("]","");
                                    line = line.Replace("\\Dynamic DST", "");
                                    TimeZones.Add(line);
                                    //richTextBox1.Text = richTextBox1.Text +"\n" +line;
                                    //Console.WriteLine(line);
                                }
                            }
                        }
                    }
                }
                IEnumerable<string> TZ = TimeZones.Distinct().ToList();
                foreach (string tz in TZ) {
                    richTextBox1.Text = richTextBox1.Text + ",\"" + tz+"\"";
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void button49_Click(object sender, EventArgs e)
        {
            lang = false;
            label1.Text = "Sysinternals Windows Image Tool";
            tabPage1.Text = "Wim Mount / Unmount";
            tabPage2.Text = "Packages and Features";
            tabPage3.Text = "Image Cleaning";
            tabPage4.Text = "Driver";
            tabPage5.Text = "Lang and Time";
            tabPage6.Text = "Appx";
            label5.Text = "Index";
            label6.Text = "Wim Folder Path";
            label7.Text = "Image Path";
            groupBox1.Text = "Unmount Image File";
            radioButton1.Text = "/discard (Delete changes)";
            radioButton2.Text = "/commit (Save changes)";
            button1.Text = "Select Wim Folder";
            button2.Text ="Select Image File";
            checkBox2.Text = "/Online (Make changes or querys on this computers Windows Installation!)";
            label12.Text = "(Not checked: all changes will performed on wim folder, additionally \r\nyou need to select Wim Folder and Image File.)";
            groupBox7.Text = "Image Settings";
            button3.Text = "Mount Image";
            button4.Text = "Unmount Image";
            button5.Text = "List Mounted Images";
            button7.Text = "Image Details";
            button19.Text = "/Get-WimInfo";
            richTextBox1.Text = "Download dependencies (ADK ve PE Addon)\r\nDownloaded version and editing Image version must be same!\r\n\r\nhttps://learn.microsoft.com/en-us/windows-hardware/get-started/adk-install\r\n\r\nThe expected way to copy data from this field is to select the relevant data and press\r\nCTRL + C on keyboard.\r\n\r\nDeveloper: https://github.com/ny4rlk0\r\nhttps://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/default-input-locales-for-windows-language-packs?view=windows-11";
            button8.Text = "Add Package From Folder (.cab .msu etc)";
            button48.Text = "Add Package From File (.cab .msu etc)";
            checkBox1.Text = "/IgnoreCheck (Offline Only)";
            checkBox3.Text = "/PreventPending (Offline Only)";
            label8.Text = "Package Name";
            button9.Text = "Remove Package";
            button6.Text = "List Packages";
            groupBox5.Text = "Query Features";
            label9.Text = "Feature Name";
            button18.Text = "/RestoreHealth (Pick Windows ISO/Source/install.wim or install.esd)";
            button24.Text = "Repair an Offline Image";
            button25.Text = "List 3th Party Drivers";
            button26.Text = "List All Drivers";
            groupBox3.Text = "Query Drivers";
            button28.Text = "Add Driver (Offline Image)";
            checkBox5.Text = "/Recurse (All subfolders will be scan for drivers.)";
            checkBox6.Text = "/ForceUnsigned (Install driver without signature!)";
            checkBox7.Text = "Only pick .inf file";
            groupBox2.Text = "Add Driver";
            groupBox4.Text = "Remove Drivers";
            button29.Text = "Remove Driver (Offline Image)";
            groupBox6.Text = "Backup Driver";
            button30.Text = "Backup Driver";
            groupBox8.Text = "System Lang Settings (Image)";
            textBox7.Text = "en-US";
            label14.Text = "Lang Code";
            groupBox9.Text = "Other Settings";
            try { comboBox1.SelectedIndex = comboBox1.FindString("US Eastern Standard Time"); }
            catch (Exception) { }
            label15.Text = "Time Code";
            button32.Text = "Set System Time Settings (Image)";
            groupBox10.Text = "Set Keyboard Settings";
            label17.Text = "Keyboard Code";
            button36.Text = "/Set-LayeredDriver (Image)";
            button37.Text = "More info";
            groupBox11.Text = "Create Lang File";
            button42.Text = "List App Packages";
            button45.Text = "Reduce Total File Size /Optimize-ProvisionedAppxPackages";
            groupBox13.Text = "Install Appx Package";
            label18.Text = "Region Code";
            checkBox14.Text = "Use region code";
            button43.Text = "More Info";
            checkBox8.Text = "Folder (Tick if you gonna select folder instead of file!)";
            checkBox9.Text = "Skip license (Use only on apps that doesnt require license! /SkipLicense)";
            checkBox10.Text = "Add Customized Data File (/CustomDataPath)";
            checkBox11.Text = "Add Package Dependencies (/DependencyPackagePath)";
            label19.Text = "Required Dependent Package Count";
            groupBox14.Text = "Uninstall App Package and Other Settings";
            label20.Text = "Package Name";
            button44.Text = "Remove Package From Image";
            button46.Text = "Add file to package (/Set-ProvisionedAppxDataFile)";
            checkBox4.Text = "Select Driver name instead of path while querying driver";
            label11.Text = "Driver Name";
            label13.Text = "Driver Name";
            button34.Text = "/Set-SKUIntlDefaults: (Image)";
            button49.Text = "Open";
            button50.Text = "Save";
            button58.Text = "Open";
            button57.Text = "Save";
            button59.Text = "Create ISO";

        }
        private void button51_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null)
            {
                if (File.Exists(WimFolderPath+"//windows//system32//startnet.cmd"))
                {
                    try{
                        richTextBox2.Text = File.ReadAllText(WimFolderPath + "//windows//system32//startnet.cmd");
                    }
                    catch (Exception ex){ MessageBox.Show(ex.ToString());
                    }
                }
            }
        }
        private void button52_Click(object sender, EventArgs e)
        {
            if (ImageFilePath != null && WimFolderPath != null)
            {
                try{
                    File.WriteAllText(WimFolderPath + "//windows//system32//startnet.cmd",richTextBox2.Text.ToString(), Encoding.UTF8);
                }
                catch (Exception ex){ MessageBox.Show(ex.ToString()); }
            }
        }
        private void button59_Click(object sender, EventArgs e)
        {
            if (File.Exists(startup_path + "bootOrder.txt"))
            {
                try
                {
                    richTextBox3.Text = File.ReadAllText(startup_path + "bootOrder.txt");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        private void button60_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(startup_path + "bootOrder.txt", richTextBox3.Text.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void button61_Click(object sender, EventArgs e)
        { //Create ISO
            try
            {
                if (true)//(ImageFilePath != null && WimFolderPath != null)
                {
                    string programfiles_path = null;
                    string efi_pathx64 = null,efi_pathArm64=null,efi_pathx86=null;
                    string oscdimgx64=null,oscdimgArm64=null,oscdimgx86=null;
                    string isoFolder = null;

                    try { programfiles_path = upupdowndownleftrightleftrightbastart_ProgramFilesx86();}
                    catch (Exception) { }
                    if (programfiles_path == null || programfiles_path=="")
                    {
                        MessageBox.Show("ProgramFiles (x86)");
                        using (var fbd = new FolderBrowserDialog())
                        {
                            DialogResult result = fbd.ShowDialog();
                            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                                programfiles_path = fbd.SelectedPath.ToString();
                        }
                    }
                    if (lang)
                        MessageBox.Show("Wim dosyanızı paketleyip Source klasörüne atmanız ve Wim dosyanızı silmeden Windows ISO dosyasının içeriğini aynı klasöre kopyalamanız gerekiyordu.\nSizin wim dosyanızı ve Source klasörünü içeren Windows ISO dizini nerede? (Klasör!)");
                    else if (!lang)
                        MessageBox.Show("You had to package your Wim file and put it in the Source package and save the Windows ISO files in the same folder without deleting your Wim file.\nWhere is the Windows ISO directory containing your wim file and the Source folder? (Folder!)");
                    using (var fbd = new FolderBrowserDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            isoFolder = fbd.SelectedPath.ToString();
                    }
                    if ((efi_pathx64==null || efi_pathx64=="") || (efi_pathArm64 == null || efi_pathArm64 == "") || (efi_pathx86 == null || efi_pathx86 == "") )
                    {
                        efi_pathx64 = programfiles_path + "\\Windows Kits\\10\\Assessment and Deployment Kit\\Deployment Tools\\amd64\\Oscdimg\\efisys.bin";
                        efi_pathArm64 = programfiles_path + "\\Windows Kits\\10\\Assessment and Deployment Kit\\Deployment Tools\\arm64\\Oscdimg\\efisys.bin";
                        efi_pathx86 = programfiles_path + "\\Windows Kits\\10\\Assessment and Deployment Kit\\Deployment Tools\\x86\\Oscdimg\\efisys.bin";
                    }
                    /*using (var fbd = new OpenFileDialog())
                    {
                        DialogResult result = fbd.ShowDialog();
                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                           efi_path = fbd.FileName.ToString();
                    }*/
                    //Oscdimg -bC:\winpe_amd64\Efisys.bin -pEF -u1 -udfver102 C:\winpe_amd64\media C:\winpe_amd64\winpeamd64.iso
                    if (programfiles_path!=null||programfiles_path!="")
                    {
                        oscdimgx64 = programfiles_path + "\\Windows Kits\\10\\Assessment and Deployment Kit\\Deployment Tools\\amd64\\Oscdimg\\oscdimg.exe";
                        oscdimgArm64 = programfiles_path + "\\Windows Kits\\10\\Assessment and Deployment Kit\\Deployment Tools\\arm64\\Oscdimg\\oscdimg.exe";
                        oscdimgx86 = programfiles_path + "\\Windows Kits\\10\\Assessment and Deployment Kit\\Deployment Tools\\x86\\Oscdimg\\oscdimg.exe";
                    }
                    if (isoFolder!=null&& isoFolder != ""&&radioButton3.Checked && efi_pathx64 != null && programfiles_path != null && programfiles_path != ""&&oscdimgx64!=null)//x64
                        oscdimg("-b\"" + efi_pathx64 + "\" -pEF -u1 -udfver102 \""+ isoFolder + "\" \""+startup_path+"Created.ISO\"",oscdimgx64);
                    else if (isoFolder != null && isoFolder != "" && radioButton4.Checked && efi_pathArm64 != null && programfiles_path != null && programfiles_path != ""&&oscdimgArm64!=null)//Arm64
                        oscdimg("-b\"" + efi_pathArm64 + "\" -pEF -u1 -udfver102 \"" + isoFolder + "\" \"" + startup_path + "Created.ISO\"",oscdimgArm64);
                    else if (isoFolder != null && isoFolder != "" && radioButton5.Checked && efi_pathx86 != null && programfiles_path != null && programfiles_path != ""&&oscdimgx86!=null)//x86
                        oscdimg("-b\"" + efi_pathx86 + "\" -pEF -u1 -udfver102 \"" + isoFolder + "\" \"" + startup_path + "Created.ISO\"",oscdimgx86);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        //For exiting app
        private void label2_Click(object sender, EventArgs e) { Application.Exit(); }
        //For minimizing window
        private void label3_Click(object sender, EventArgs e)
        {this.WindowState = FormWindowState.Minimized;}
        private void label4_Click(object sender, EventArgs e)
        {this.WindowState = FormWindowState.Maximized;}
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///                                                                                                                                          ///
        ///                                                                                                                                          ///
        ///                  N Y 4 R L K 0                                                                                                           ///
        ///                                                                                                                                          ///
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /*
        ⠋⠌⠁⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠠⠀⠄⠤⠐⡀⠄⠠⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⣁⠠⢀⢀⠀⢀⠠⠀⠀⠠⠀⠀⡀⠠⠀⠀⠠⠀⢀⠀⠀⠀⠀⡀⠀⠄⠠⠀⠀⠤⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⡠⠀⠀⡠⠀⡠⠐⢌⠠⠄⡔⠠⢔⠠⠤⠡⠆⢠⠀⠄⠀⠀⠀⡀⠀⠀⡀⠠⠀⠀⢀⠀⢀⠠⠀⠀⠄⠠⠀⡀⠀⠠⠀⠀⠀⠀⠀⠠⠀⢀⠀⠀⡀⠀⠠⠀⠀⠠⠀⠀⡀⢀⠀⠄⠀⠀⠄⠀⡀⢀⠀⠠⠀⢀⠠⠀⠀⡀⢀⠀⡀⠠⠀⠀⠄
        ⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⣀⡀⠀⠀⢂⠠⠈⡀⠠⠀⠀⠀⠀⠀⠠⠐⠀⠂⠠⠐⠀⠄⠀⠀⠠⠀⢀⢂⠀⢊⠀⠄⢀⠑⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠀⠀⠀⡀⠐⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠤⠀⠀⠂⠀⡁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠈⠀⠀⣀⣤⣀⠀⠀⠀⠀⣔⣿⣿⡿⠿⣶⣄⡀⠅⡐⢀⡀⠀⠀⡀⠀⠁⢌⠈⡄⢁⠂⠂⢌⠠⡉⢀⠁⠤⡈⡈⢀⠂⠀⠀⠀⠀⢀⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠈⡀⠀⠈⠀⠌⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠢⠐⠀⢀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠀⠀⠠⠀⠁⠠⠀⠄⠀⠀⠁⢼⡿⠉⠙⠱⠀⠀⢰⣿⢿⡁⠀⠀⠀⠈⠻⣆⠐⠀⠄⡁⢂⠀⠀⠀⠀⠠⢀⠂⠠⠌⢠⠐⠄⠂⠍⡠⠐⡀⠀⠀⠀⠀⠐⠀⠀⠀⠄⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠠⠐⠀⠠⢀⠡⠉⠌⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡑⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠀⠀⠀⢀⣤⠀⠈⢀⢀⠘⣿⣄⠀⠀⠀⡀⠹⣿⣀⠀⢀⠀⠀⠀⠀⢻⣇⠈⡀⠠⠀⠂⡀⠂⠀⠁⠂⠌⡐⠂⠄⠂⡌⠑⠠⠀⢀⠠⠀⠀⠀⠀⠀⠀⡐⠈⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠐⠀⢂⠔⠁⠂⠄⠁⠄⡈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢌⠀⠐⠀⠀⡁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠀⠀⠐⠀⠀⠈⢷⣄⠘⣿⡉⠀⠈⠻⣷⣄⡀⢀⣤⣤⣬⣤⣄⣀⣀⠀⠀⠈⣿⡆⠀⠀⠁⠔⠀⠀⠀⠠⠁⢂⠀⠂⠌⠑⣀⢉⠐⣁⠠⢀⠀⠂⠀⢀⠀⡀⢠⠂⠁⡀⠀⠂⠀⠀⠀⠀⠀⠀⠀⠀⡀⠠⠀⠋⣀⠂⢉⠂⠐⡀⠁⠀⠈⠀⠀⠉⠐⠠⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠢⠀⠀⠈⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣰⡋⠁⠀⠀⠉⠓⠮⢿⣦⣤⣤⣌⣛⣿⣿⣥⣄⣀⣀⣀⡈⠉⠻⣦⣤⣿⣯⠀⠄⠀⠀⠈⢀⠀⠄⡁⠂⢀⠀⠌⢠⠀⠄⡁⠠⠐⠄⡂⠡⢀⠄⢂⠀⢂⠌⢢⠠⠑⠠⢄⡈⠀⠀⢀⠠⠐⢀⠀⠠⠒⡐⢠⠉⠠⠀⠂⠠⠀⠀⠀⠀⠀⠐⠈⠀⠀⠀⠁⠈⠀⡀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡑⠀⠀⠁⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣆⠙⢷⣷⣤⣀⠀⠀⠐⠂⠀⣉⣛⣻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣶⣄⢹⣿⣯⠀⠀⠀⠀⠀⠂⠐⠠⠀⠂⠀⠄⠈⠄⠂⠐⠀⠐⠈⠄⠐⡐⠀⢂⠂⠌⠄⠂⠡⠄⠁⠂⡀⠈⠢⢉⠄⠤⠐⠠⠤⣋⠐⢣⡀⠐⠠⠀⡁⠀⠐⠀⢀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⣌⠀⠀⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⢳⣶⣮⣽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣻⣿⣷⣿⣿⣿⣿⡇⠈⠀⠀⠀⠐⡀⠁⢀⠐⠀⠂⠈⡀⠈⠀⡈⠀⠀⠈⢀⠀⢣⡄⢡⢈⠐⡉⠂⡌⢑⡀⠀⠐⡀⢀⢣⠀⡌⠀⠀⠀⠙⠂⣤⠁⢒⢠⠈⢀⠀⡄⠀⠀⠀⠀⠀⠀⠀⠀⠈⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢮⠀⠀⠆⠀⡃⠀⠀⠀⠀⠀⠀⠀⠀⣀⣴⣶⣷⣶⣤⣄⣀⠉⢻⣿⣯⣍⣉⣽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣃⠀⠀⠀⠀⠀⠁⠃⢨⠀⠀⠃⠰⡁⢣⠀⢁⢠⠰⠀⢘⡜⠀⢠⠰⠈⡀⠁⠀⠘⡌⡁⠰⠀⠁⠘⠀⡛⠀⠀⠀⡄⠀⠰⡈⡞⣬⡳⠶⢨⠃⢠⠀⡄⠀⠀⠀⠀⠀⠀⠀⠀⠁⠀⠃⡘⠀⢠⠀⢠⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠣⠀⠀⢀⠀⡁⠀⠀⠀⠀⠀⠀⠀⣘⣿⣿⣧⣄⡉⠙⠻⣿⣿⣧⣽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⡀⠀⠀⠀⠀⠄⠀⠠⠀⠀⠀⠁⡀⢀⠈⠠⠀⠛⠈⠀⠀⠀⠀⠀⡁⠀⠠⠀⠀⡁⠀⠀⠀⠀⠀⠁⠀⠀⠀⠀⠠⠀⣅⠣⠄⡅⢃⠏⢀⡠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠁⠀⠀⠀⠀⠀⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠱⠀⠈⠀⠀⠄⠀⠀⠀⠀⠀⠀⠸⠟⢟⡿⢿⣿⣿⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⢷⣄⠐⠀⠀⠀⠀⠠⠀⠀⠀⠀⣠⣤⡀⠂⠄⠀⠀⠀⠀⠀⠀⢀⠀⠈⠐⠂⠄⡄⠄⠂⠄⠠⠀⠀⠀⠀⠠⠀⠐⠠⢀⠐⠠⠄⠂⠠⠀⠄⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡑⠀⠀⠐⠀⠂⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠛⠿⠟⠻⠻⠿⢿⣷⣤⣤⣥⣽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⣿⣿⣿⡄⢻⡄⠀⠀⣀⣶⣴⠀⢁⡈⣀⣿⣿⣧⠀⣁⣁⡀⣀⣀⣤⣤⣤⣤⡄⠂⠈⠀⠀⠈⠑⠂⠁⠂⢀⠀⠀⠐⠀⢁⡀⠠⠀⠔⠀⠊⠀⠀⠀⠐⠀⠀⠐⡀⠂⠠⠀⠐⠀⠂⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢌⠀⠐⠀⠀⡁⠀⠀⠀⠀⠀⠀⠀⠀⠀⢤⣶⡾⠷⠶⢶⣶⣤⣤⣌⣽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣥⣿⢿⣿⣿⢹⢇⡼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣿⡇⠀⣀⠀⠀⠀⠀⠀⠀⡀⠀⠀⠀⢀⠈⠀⠠⠁⠈⠀⠀⠁⠀⠀⠀⠀⠀⠀⠀⠀⠠⠁⠀⢁⠀⡀⠀⠀⠀⠄⠁⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠢⠀⢀⠠⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⠳⠀⠀⠀⠸⣤⣴⡾⢿⣛⣛⣻⣿⢶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⣉⡽⢃⣨⣿⣗⠞⠉⠀⠉⠉⢉⣽⣿⣿⠀⣻⣿⣏⡉⠫⠉⠀⣿⣿⡇⠀⢈⣍⣍⣍⢉⣁⣀⣀⣀⣀⣀⣠⣀⣀⣀⣀⣤⣁⣀⣀⣀⣄⣀⣀⣀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡑⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠀⠀⠀⠀⣾⡿⣻⠿⠛⢋⣹⡿⠿⠛⠻⢟⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣶⣴⣿⣿⣿⣿⣶⣶⣶⣶⣾⣿⣿⣶⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣤⣤⣤⣦⣤⣦⣶⣴⣦⣶⣴⣦⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣶⣿⡿⣿⣿⢿⣷⣶⡀⠂⠀⠀⠀⠀⠀⠀⠀⠀
        ⢌⠀⠀⠂⠀⡁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣿⠀⠻⠄⣤⡟⠉⠀⠀⠀⣰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠛⠛⢛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠻⠿⠿⠿⠿⠿⠿⠛⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠢⠀⠀⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠀⠀⣸⣿⣀⡀⠐⠀⣰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣟⣿⡟⢻⣿⣿⣿⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⡄⠀⠀⠀⠀⠀⠀⠀⠉⠈⠈⠁⠈⠁⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡑⠀⠠⠀⠀⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠀⠀⠀⠀⠻⣿⠾⠛⠀⢀⣿⣿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣾⣷⣼⣿⣿⣿⣧⠈⠹⡻⠿⣿⣟⡻⣟⣛⣻⣟⠿⣿⣿⣿⣿⣿⣿⣿⣷⣿⣿⣿⣿⣿⢿⡞⠹⢿⠿⣿⠿⠿⠿⠿⠿⠻⠟⠛⠛⠛⠙⠿⠟⠁⠙⠆⠁⠀⠀⠀⠀⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠐⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢌⠀⠀⠠⠀⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠈⠀⡀⠀⠈⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣟⣿⣿⣿⣿⢹⢿⣇⡴⢷⣾⣻⣥⣿⡝⣻⣟⣻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣟⠀⠀⠀⣴⠏⠀⡀⠀⠀⢀⠀⠠⢈⠌⡁⠉⠤⠀⠁⠀⠀⠀⠀⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⠊⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢀⠀⡀⠀⠀⡁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⠀⢺⣿⡆⠀⠀⣀⣰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣞⣽⣿⣿⣿⣯⠿⣿⣹⣿⣼⣿⢷⣿⢯⣭⠷⣿⣿⠿⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠃⠀⢠⣾⠏⠀⠀⣀⣔⠈⠤⢡⡍⢠⠐⠀⠢⠄⡁⢐⠄⡀⢀⡀⠠⢀⠀⢀⠀⠀⠀⠀⡂⢈⠤⠀⠆⡒⠂⠄⠀⠠⠀⠀⢀⠠⡐⢈⠡⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⠀⡀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠠⠀⠹⠿⣷⠀⠀⠀⠛⠃⣤⠀⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣻⣿⡿⠟⠻⣿⣿⣿⡿⠋⠔⠠⠄⠄⠠⣿⡿⠿⠿⠟⠛⠋⢋⠉⡉⢀⣰⡿⠁⠄⢐⣶⣿⠟⠁⠁⠄⠐⠢⠈⠌⣁⠐⡀⢂⠌⠄⠀⠀⣀⠖⠊⠀⠀⠀⠀⠀⠖⠁⠀⠒⠠⠈⠑⠬⢁⡂⡐⠌⠄⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⠀⢐⡠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡜⠻⣥⡀⠀⠀⠀⠀⠤⣬⣦⢠⣬⣿⣝⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⡟⠁⠀⠒⠀⠀⠉⣰⡀⠁⢈⣀⠌⠘⡀⣀⠐⠂⢉⠠⡈⠀⠂⠐⢀⣼⠟⠀⢀⣾⣿⠟⠁⠀⠀⠀⠀⠀⠀⠁⠐⠀⠠⠀⠀⠀⠀⠠⠁⠀⣀⠀⡀⠂⢁⠉⠀⠀⠈⠀⠀⠐⠀⠀⠀⠀⠀⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⠈⠁⠀⠀⠄⠈⠀⠀⠀⠀⠀⠘⠓⠀⠛⠀⠀⠀⠀⣿⡇⢈⠄⡈⠉⢿⠰⢿⣿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡯⢍⣿⣷⣶⣾⣿⣾⣶⣶⣆⠿⠇⠀⠄⣀⠔⠂⠔⡠⠉⠠⢀⡀⠤⠀⣖⣤⡿⠃⡀⠰⡿⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⠀⠀⠀⠀⠀⡀⠐⠀⣠⠁⢄⠨⠀⠆⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⠀⠀⠀⠀⠄⠀⠀⠀⠀⠀⠳⣄⡀⠀⠀⠀⠀⠀⣠⠏⢡⢀⠒⠄⠢⠀⠀⢈⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⠀⠒⠰⢀⠊⡉⠤⠀⠺⢶⡀⠅⢂⣵⡾⠋⢀⠤⢠⣐⠢⣀⡠⠔⠒⠒⠢⣀⠠⠀⠄⠐⠂⠀⠈⠀⠀⠀⡀⠀⠀⠀⠀⠆⡨⠄⠃⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡀⠀⠀⠀⠀⠂⠀⠀⠀⠀⠀⠀⠀⠁⠤⢀⡤⠚⡍⠀⠨⠄⣀⣤⣶⣶⣶⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⢭⡁⣄⠈⠻⣿⠇⣠⠘⡀⠀⠐⠈⡀⠂⣀⣤⣾⡿⡋⢀⢳⠌⣀⢃⠰⠌⣩⠐⠂⡉⠉⠒⡀⠑⠀⠈⠀⠐⠀⠀⠀⠄⡠⠀⠄⠀⡁⠔⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢌⠀⠐⠀⠀⡁⠀⠀⠀⠀⠀⠀⠀⠀⠐⠚⠀⠆⠐⠂⠐⠀⢻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢺⡥⣓⠄⠢⡅⠹⡻⡟⠠⠄⠅⠒⣠⣴⢾⣯⠗⠋⣀⠉⢠⠞⣌⠢⡨⠗⢘⡁⢈⢁⡠⢁⠀⠀⠁⠀⠀⢀⡀⠈⠁⠈⣀⠑⠈⡌⠁⠆⣈⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠢⠀⠀⠀⠀⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠰⣶⣶⣶⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡷⣯⣷⣔⣈⡄⣑⣭⡶⣤⣾⣻⠽⠞⠉⠀⠉⢀⢀⠰⢉⠶⠌⡖⠁⠤⠈⣤⠆⠁⡀⠳⠈⠀⡁⠠⠈⠠⢀⠀⠔⠀⠀⠀⠀⢀⠉⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡑⠀⠀⠄⠀⡀⠐⠀⠀⠀⠸⣶⡆⠀⠀⢀⣤⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢏⡿⠶⣝⣳⣿⣷⡟⠉⢀⠘⢯⠀⠐⠀⠀⠀⡃⢁⠘⢀⣾⠐⡪⠈⢒⠄⠄⠢⣈⡘⠁⠂⠌⡀⠀⠂⠴⠀⢠⠒⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠌⠀⠀⠀⠀⠄⠀⠄⠀⠉⠀⠀⠃⠐⠀⠀⠘⣿⣿⣿⣿⣿⣿⣿⡿⣹⢿⣿⣿⣿⣿⣿⣿⣷⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣾⣿⣾⡿⢻⡏⢩⢛⣷⡀⠈⠒⣻⡀⠁⣀⠀⠀⡂⡁⠀⠘⡟⠁⠡⢩⢀⠋⢀⡁⡈⠘⠌⠇⠀⠐⠀⠐⠈⢂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⠀⠈⠀⠀⠀⠆⠀⠐⠀⠀⠀⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠨⠀⢀⠀⠈⠀⠂⠀⠀⠂⠀⠠⠁⠀⠠⡈⠀⠘⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢏⣽⣿⣿⣿⡿⠂⠙⢦⡘⢻⣽⣶⣿⣿⣷⡀⠠⠀⢂⠈⢁⣠⣌⢱⡄⢐⠣⡈⠄⣵⡀⡈⠔⠐⠄⠁⠀⠄⠠⠁⠂⠀⠀⠀⢀⠀⠁⠀⠀⠀⠀⠀⠀⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⡳⠀⢀⠆⠘⡄⠀⠀⡄⢀⠘⠀⠀⠀⠆⢠⣴⣤⠸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⡇⡄⠀⠀⠃⢷⣾⣿⣿⣿⣿⣿⣿⣄⠀⠆⠀⣰⢟⣀⠶⠀⣤⡀⢇⣤⢿⡆⡀⣸⡄⠀⡄⠀⢀⠰⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡄⠀⠀⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠱⠀⢨⠀⠀⠆⠀⠀⠆⢨⠀⠘⠀⠆⡴⠸⣿⣵⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠟⠃⢠⡅⣤⠃⠰⠀⠙⣿⣿⣿⣿⣿⣿⣿⡆⠘⠸⠃⠛⠟⠀⡟⢻⣽⣿⠟⠈⣤⢡⡟⠃⠀⡜⠰⠸⢧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⠶⠀⠘⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠆⠀⠀⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠈⢀⠠⠀⠈⠀⠒⠀⠀⠀⠀⠂⡃⠈⠔⠄⡩⢁⡙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠉⣉⣲⠐⠄⠀⡄⢘⢈⠒⡨⢀⡘⠂⡀⢁⠀⠹⣿⣿⣿⣿⣿⣷⡁⠰⣆⠀⠀⠩⢉⢁⢘⠛⠊⡁⠈⠈⠅⢈⢃⠀⠐⠰⠸⡓⢲⡀⠀⠀⠀⠀⠀⢀⠐⠁⠈⡀⢂⠀⡠⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠈⠀⡀⠐⠀⢁⣀⠀⡀⢈⠀⠀⠤⠉⠄⠣⠜⢨⡐⢀⢻⣿⣿⣻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠛⠁⠰⠸⠂⠡⣨⢃⠐⠆⣂⢡⠐⡄⠩⠜⡰⢀⠌⡅⢈⠻⣿⣿⣿⣿⣷⡀⠤⠤⠀⢂⠠⠄⡀⢡⠈⣀⣾⠃⠀⣟⢈⠀⢡⢈⡁⢣⠀⢇⠀⠀⠀⠀⢀⠀⡀⢈⡐⠀⠄⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⠀⢀⠔⢉⠔⢀⠒⠤⢀⠡⠄⢃⠪⠌⡡⢚⢠⣈⠄⣾⣿⣿⢻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡏⠄⠐⢀⠑⡄⢊⡀⢓⡶⢯⠐⠒⠀⠤⠠⣌⠉⠦⠰⡀⢦⡈⠔⢢⠘⢻⣿⣿⣿⣷⡀⠐⠐⠂⠄⢂⠀⠴⣶⠋⣁⢈⠀⣸⢎⠠⣿⡖⢼⡅⢳⡀⢀⠀⠀⠀⡈⠀⠠⠜⠇⠈⠠⠀⡀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⡀⠤⠘⢠⠊⢀⠊⡐⠂⠙⠄⠦⡁⠒⠰⠀⠄⡑⢸⣿⣿⣷⣤⡙⠼⢭⢩⠵⣍⠟⣭⣻⣿⣿⡿⣠⠐⡈⢄⣧⢹⡀⠂⣈⡙⠀⡨⠑⡍⡒⠡⢄⠙⡇⡑⢢⠐⠡⡘⢈⠢⢁⠹⣿⣿⣿⣷⣄⡀⠀⠁⠀⠄⠀⢏⣉⢻⣹⡆⣬⣷⣦⢏⢻⡌⠾⣾⡇⡀⢀⠀⠁⠀⠀⢁⠄⢀⡂⠁⣀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠉⠀⢄⠉⡂⠌⢢⠀⢂⠌⠁⡌⠠⢐⠉⠠⠉⢡⠀⠀⢿⣿⣿⣿⣿⣿⣶⣧⣾⣴⣯⣾⣿⣿⠏⣠⣿⢷⣈⡼⠿⡁⢣⢁⣬⡁⡖⢁⠲⠇⡐⢃⣼⡇⢰⠠⠡⢌⡑⢠⠈⠩⠑⢄⠈⣿⣿⣿⣿⣦⡢⢀⠂⠉⠷⣽⣿⣰⣮⡹⣦⠘⢷⣯⣰⣿⡄⢻⢷⢻⠀⣧⠀⢀⠀⠀⡀⠄⠀⠄⡈⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⠈⡄⠠⢉⠔⡠⠜⠠⢌⠂⠤⢁⠄⠂⡁⠡⠀⠀⠀⠀⡇⣬⢛⠻⠿⣿⣿⣿⡿⣿⣷⡿⡟⣶⣹⢏⣼⣿⢃⡒⡒⢯⢠⢑⡀⠜⡤⣨⣁⠆⢟⡁⢺⠻⡀⠇⠤⠐⠄⠂⠅⠣⠐⠆⡈⢿⣿⣿⣿⣿⡂⠠⣀⢴⣖⢹⠽⠋⢿⣇⣷⡞⢿⣳⠌⠹⠆⡈⢸⡜⢙⠀⠈⠀⠀⠙⢁⠀⣤⠀⢌⠢⣀⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⠀⠂⡄⢁⠢⠁⠴⠈⠖⠢⠌⠴⠠⠐⠦⠐⠀⠒⠀⠂⢸⢃⠣⣌⡃⡒⠐⣠⣿⣿⣿⣟⣻⣯⣭⢊⣻⣯⣦⠒⢇⡑⢺⡤⢦⣅⡎⣜⡳⡉⡷⢷⡿⢃⣒⣃⣒⣒⣁⣊⣑⢂⡒⣈⢄⡠⢙⣿⣿⣿⣿⣷⣜⢿⠿⣿⠆⣀⢊⢡⣿⣾⣿⢜⣫⠉⣼⣮⣿⡈⠹⢾⡄⢁⢀⡀⣀⠀⣶⣧⣧⡈⠐⠀⠇⢀⠀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⣀⣀⣀⣀⣈⣉⣈⣩⣥⣦⣤⢦⢭⣤⢭⣉⣏⡭⣭⣡⣬⣧⣓⡌⡃⢌⣰⣿⡿⠿⢿⣺⣿⣯⣿⣧⣼⡿⣝⡟⣈⣤⢋⠗⣠⣤⡴⣼⠷⣯⢿⠧⡞⣧⢥⢦⡴⣌⣰⢤⡬⠄⣁⡠⠂⠂⢉⣿⣿⣿⣿⣿⣿⣆⡯⡽⠛⣁⠤⢤⢽⠾⣿⣯⣿⣿⣿⣿⡛⡏⠷⣽⠃⡒⠀⠀⡀⠰⡼⠿⠿⠿⠿⠶⣦⠤⠀⢀⠠⠈⠀⠁⠄⠠⠀⠄⠀⠀⠀⠄⠀⠀⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        ⢉⢐⣢⣭⣫⣛⣛⣭⣭⣽⡼⡭⠾⠤⠷⠼⣴⣎⣳⢛⡼⠮⠿⢟⡛⢟⡳⠶⠿⣿⡿⠿⠻⠿⠷⡿⢻⡿⣧⠾⠿⣐⠭⠧⠷⢆⡓⠮⡝⣬⣩⣚⠱⣌⣋⡭⣖⡥⣞⣤⣭⡽⣤⢦⡵⣶⣮⣿⣿⣿⣿⣿⣿⣿⠾⠝⠛⠃⢋⣉⣩⣿⣿⣿⣿⣿⣿⡏⣿⣥⣶⣡⣷⢶⣶⣤⣤⣤⣤⣤⣤⣤⣤⣤⣤⣤⣤⣤⣤⣀⣈⡀⣀⣀⣀⣀⣀⣀⣀⡀⣀⠀⣀⢀⡀⣁⢀⡀⢈⡀⣁⡀⣀⡀⠐⠀⠂⠀⠂⠀⠀⠀
        ⠸⠉⢵⣶⣽⠄⢃⠢⢴⣄⣐⣢⣡⣉⣜⣠⣆⠰⠌⠥⢲⣉⣱⢨⡐⠡⠦⠙⠒⠠⠦⠭⠭⠴⠣⠼⠔⣒⠒⢓⣒⠓⡨⢉⣍⣆⣜⠭⠵⠦⠧⠭⠿⠐⠮⣉⣰⢈⠩⡡⢍⡱⠭⢭⠓⡳⠮⠽⠯⠘⡙⢉⣉⣀⣀⣌⡙⢫⠝⣻⣻⢟⡿⣛⣿⡻⢯⣽⣯⣷⣚⣻⣷⣚⣶⣾⠶⠷⢬⣍⠡⠦⠖⠒⢈⣀⣉⣉⣉⣉⣉⣉⣉⡉⢉⠉⡁⠀⠀⠉⠀⠉⠈⠀⠉⠀⠁⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠂⠈⠀
         */
        private void checkAdminAccess()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);
            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                // It is not possible to launch a ClickOnce app as administrator directly,
                // so instead we launch the app as administrator in a new process.
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);
                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";
                // Start the new process
                try { Process.Start(processInfo); }
                // The user did not allow the application to run as administrator
                catch (Exception) { MessageBox.Show("I need to run as Administrator.\nNow exiting."); }
                // Shut down the current process
                Application.Exit();
            }
        }
        private void CMD(string Command)
        {
            //MessageBox.Show(Command);
            //richTextBox1.Text = richTextBox1.Text +"\n"+Command;
            
            Thread cmdBackground = new Thread(() => CMDelagate(Command));
            cmdBackground.IsBackground = true;
            cmdBackground.Start();
        }
        private void CMDelagate(string Command)
        {
            try{
                richTextBox1.Text = richTextBox1.Text + "\nCOMMAND: CMD " + Command;
                Process p = new Process();
                p.StartInfo.FileName = "CMD.EXE";
                p.StartInfo.Arguments = Command;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.OutputDataReceived += HandleExeOutput;
                p.BeginOutputReadLine();
                p.WaitForExit();
                /* string output = p.StandardOutput.ReadToEnd();
                 richTextBox1.Text=richTextBox1.Text+"\n"+output.ToString();
                 string err= p.StandardError.ReadToEnd();
                 richTextBox1.Text = richTextBox1.Text + "\n" + err.ToString();
                 p.WaitForExit();*/
                //Process.Start(sysFolder+Command);
            }
            catch (Exception er){ MessageBox.Show(er.ToString()); }
        }
        private void oscdimg(string Command ,string Path)
        {
            Thread oscdimgDelagateBackground = new Thread(() => oscdimgDelagate(Command,Path));
            oscdimgDelagateBackground.IsBackground = true;
            oscdimgDelagateBackground.Start();
        }
        private void oscdimgDelagate(string Command, string Path)
        {
            try
            {
                richTextBox1.Text = richTextBox1.Text + "\n"+Path+"\nOSCDIMG: " + Command;
                Process p = new Process();
                p.StartInfo.FileName = Path;
                p.StartInfo.Arguments = Command;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.OutputDataReceived += HandleExeOutput;
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
            catch (Exception er) { MessageBox.Show(er.ToString()); }
        }
        static string upupdowndownleftrightleftrightbastart_ProgramFilesx86()//Old Magic
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
        private void HandleExeOutput(object sender, DataReceivedEventArgs e)
        {
            try
            {
                string output = e.Data;
                if (output!=null&&output!=doNotRepeat)
                    richTextBox1.Text = richTextBox1.Text + "\n" + output.ToString();
                doNotRepeat = output;
            }
            catch (Exception er)
            {

                MessageBox.Show(er.ToString());
            }

        }


        private void button57_Click_1(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
                // set the current caret position to the end
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                // scroll it automatically
                richTextBox1.ScrollToCaret();
        }
    }
}
