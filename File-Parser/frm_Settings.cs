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

namespace File_Parser
{
    public partial class frm_Settings : Form
    {
        private readonly string FolderPath = @"C:\File Parser";
        private readonly string LookupFormatPath;
        private readonly string ReplaceFormatPath;
        private readonly string ReportFormatPath;
        private readonly string ReportSourcePath;

        private frm_Main _mainForm;

        private List<string> filePaths;
        List<string> FindColValues { get; set; }
        List<string> ColReplaceFormat { get; set; }
        List<string> ColReportFormat { get; set; }
        List<string> ColReportSource { get; set; }


        public frm_Settings()
        {
            InitializeComponent();
            filePaths = new List<string>();

            LookupFormatPath = Path.Combine(FolderPath, "ColumnLookup.txt");
            ReplaceFormatPath = Path.Combine(FolderPath, "ColumnReplace.txt");
            ReportFormatPath = Path.Combine(FolderPath, "ColumnFormat.txt");
            ReportSourcePath = Path.Combine(FolderPath, "SourceFormat.txt");

        }

        private void frm_Settings_Load(object sender, EventArgs e)
        {
            GetTextFiles();
            LoadTextData();
        }

        public void GetTextFiles()
        {
            try
            {
                FindColValues       = new List<string>();
                ColReplaceFormat    = new List<string>();
                ColReportFormat     = new List<string>();
                ColReportSource     = new List<string>();

                // Load text from NameLookup.txt into findColumnsValues
                if (File.Exists(LookupFormatPath))
                {
                    FindColValues = File.ReadAllLines(LookupFormatPath).ToList();
                }

                // Load text from NameReplace.txt into colFormatValues
                if (File.Exists(ReplaceFormatPath))
                {
                    ColReplaceFormat = File.ReadAllLines(ReplaceFormatPath).ToList();
                }

                // Load text from NameReplace.txt into colFormatValues
                if (File.Exists(ReportFormatPath))
                {
                    ColReportFormat = File.ReadAllLines(ReportFormatPath).ToList();
                }

                // Load text from NameReplace.txt into colFormatValues
                if (File.Exists(ReportSourcePath))
                {
                    ColReportSource = File.ReadAllLines(ReportSourcePath).ToList();
                }


                // Debug.
                /*foreach (var value in ColReportSource)
                {
                    Console.WriteLine($"Report Value: {value}");
                }*/

                // MessageBox.Show("Files loaded successfully!", "Load Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadTextData()
        {
            // Clear the RichTextBox contents before adding new text
            rtx_LookUpColumns.Clear();
            rtx_RenameColumn.Clear();
            rtx_ReportSource.Clear();
            rtx_ReportFormat.Clear();

            // Append values from findColumnsValues to rtx_FindColumns
            foreach (var value in FindColValues)
            {
                rtx_LookUpColumns.AppendText(value + Environment.NewLine);
            }

            // Append values from colFormatValues to rtx_ColFormat
            foreach (var value in ColReplaceFormat)
            {
                rtx_RenameColumn.AppendText(value + Environment.NewLine);
            }

            // Append values from colFormatValues to rtx_ColFormat
            foreach (var value in ColReportFormat)
            {
                rtx_ReportFormat.AppendText(value + Environment.NewLine);
            }

            foreach (var value in ColReportSource)
            {
                rtx_ReportSource.AppendText(value + Environment.NewLine);
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            saveToTextFile();
        }

        public void saveToTextFile()
        {
            try
            {
                // Ensure the folder exists
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                // Save text from rtb_FindColumns to NameLookup.txt
                File.WriteAllText(LookupFormatPath, rtx_LookUpColumns.Text);
                File.WriteAllText(ReplaceFormatPath, rtx_RenameColumn.Text);
                File.WriteAllText(ReportFormatPath, rtx_ReportFormat.Text);
                File.WriteAllText(ReportSourcePath, rtx_ReportSource.Text);

                MessageBox.Show($"Saved to: \n{ReplaceFormatPath}\n{LookupFormatPath}\n{ReportFormatPath}\n{ReportSourcePath}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
