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
            richTextBox1.Text = "Download dependencies (ADK ve PE Addon)\r\nDownloaded version and editing Image version must be same!\r\n\r\nhttps://learn.microsoft.com/en-us/windows-hardware/get-started/adk-install\r\n\r\nThe expected way to copy data from this field is to select the relevant data and press\r\nCTRL + C on keyboard.\r\n\r\nDeveloper: https://github.com/ny4rlk0";
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

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
                // set the current caret position to the end
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                // scroll it automatically
                richTextBox1.ScrollToCaret();
        }
    }
}
