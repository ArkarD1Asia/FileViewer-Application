using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LogMonitor
{
    public partial class FileViewer : Form
    {
        private readonly string _logFolderPath = @"F:/GrgBanking/eCAT/Log/";
        private readonly string _imgFolderPath = @"F:/CameraImage/Image/";
        private readonly string _outputFolderPath = @"F:/FileViewer/Output/";
        private string getTerminalId = "";
        public FileViewer()
        {
            InitializeComponent();
            LoadATMNumber();
        }

        private void btn_Generate_Click(object sender, EventArgs e)
        {
            btn_Generate.Enabled = false;
            if (string.IsNullOrEmpty(getTerminalId))
            {
                btn_Generate.Enabled = true;
                MessageBox.Show("Please enter the terminal No.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            try
            {
                string outputFileName = $"{getTerminalId.Trim()}_{DateTime.Now:yyyyMMdd}_FileViewer.txt";
                if (!Directory.Exists(_outputFolderPath))
                {
                    Directory.CreateDirectory(_outputFolderPath);
                }
                string outputFilePath = Path.Combine(_outputFolderPath, outputFileName);

                DateTime oneYearCutoff = DateTime.Today.AddYears(-1);
                DateTime threeMonthsCutoff = DateTime.Today.AddMonths(-3);

                using(StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    var logFiles = Directory.GetFiles(_logFolderPath, "*.*", SearchOption.AllDirectories)
                                   .Where(f => !File.GetAttributes(f).HasFlag(FileAttributes.Directory))
                                   .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                                               f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                    foreach(string file in logFiles)
                    {                        
                        DateTime creationDate = File.GetCreationTime(file);
                        DateTime lastWriteDate = File.GetLastWriteTime(file);

                        bool olderOneYear = creationDate > oneYearCutoff || lastWriteDate > oneYearCutoff;

                        if (olderOneYear)
                        {
                            writer.WriteLine(file);
                            writer.WriteLine();
                        }
                    }

                    var imgFiles = Directory.GetDirectories(_imgFolderPath)
                                    .Where(folder =>
                                    {
                                        string folderName = Path.GetFileName(folder);
                                        return folderName.Length == 8 && folderName.All(char.IsDigit);
                                    });

                    foreach (string folder in imgFiles) 
                    {
                        string folderName = Path.GetFileName(folder);
                        if (DateTime.TryParseExact(folderName, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime folderDate))
                        {
                            bool olderThreeMonth = folderDate > threeMonthsCutoff;

                            if (olderThreeMonth)
                            {
                                var zipFiles = Directory.GetFiles(folder, "*.zip");
                                foreach (string zipFile in zipFiles)
                                {
                                    writer.WriteLine($"{zipFile.Replace("\\","/")}");
                                    writer.WriteLine();
                                }                                   
                            }
                        }

                        
                    }
                    lblStatus.Text= "Report generated successfully";
                }

            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }

        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #region "GetTerminalInfo"
        private void LoadATMNumber()
        {
            try
            {
                string filePath = @"D:\GrgBanking\eCAT\Config\TerminalConfig.xml";

                XDocument xmlDoc = XDocument.Load(filePath);

                string atmNumber = xmlDoc.Root.Element("Terminal")?.Attribute("ATMNumber")?.Value;
                txt_Terminal.Text = atmNumber ?? "ATMNumber not found";             
                 getTerminalId=atmNumber;
    }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading XML: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
