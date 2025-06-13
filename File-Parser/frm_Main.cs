using OfficeOpenXml;
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
    public partial class frm_Main : Form
    {
        private DataTable DTProcess { get; set; }
        private DataTable DTExport { get; set; }
        private DataTable DTFinal { get; set; }
        private DataTable MenuData { get; set; }


        private  string FolderPath = @"C:\File Parser";
        private  string LookupFormatPath;
        private  string ReplaceFormatPath;
        private  string ReportFormatPath;
        private  string ReportSourcePath;

        List<string> FindColValues { get; set; }
        List<string> ColReplaceFormat { get; set; }
        List<string> ColReportFormat { get; set; }
        List<string> ColReportSource { get; set; }

        private List<string> filePaths;

        public frm_Main()
        {
            InitializeComponent();
            LookupPath();
        }

        private void LookupPath()
        {
            filePaths = new List<string>();
            LookupFormatPath = Path.Combine(FolderPath, "ColumnLookup.txt");
            ReplaceFormatPath = Path.Combine(FolderPath, "ColumnReplace.txt");
            ReportFormatPath = Path.Combine(FolderPath, "ColumnFormat.txt");
            ReportSourcePath = Path.Combine(FolderPath, "SourceFormat.txt");
            CheckFilesAndFolder();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            EnableDoubleBuffering(dgv_File);
        }

        private void EnableDoubleBuffering(DataGridView dgv)
        {
            // Access the underlying control class and set double buffering
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(dgv, true, null);
        }

        private void LoadData()
        {
            getTextDetails();
            MenuData = getMenuData();
        }

        private void getTextDetails()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            GetTextFiles();
            //SetRequiredText();
        }

        public void GetTextFiles()
        {
            try
            {
                FindColValues = new List<string>();
                ColReplaceFormat   = new List<string>();

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
                if (File.Exists(ReplaceFormatPath))
                {
                    ColReportFormat = File.ReadAllLines(ReportFormatPath).ToList();
                }

                // Load text from NameReplace.txt into colFormatValues
                if (File.Exists(ReportSourcePath))
                {
                    ColReportSource = File.ReadAllLines(ReportSourcePath).ToList();
                }

                // Debug.
                // foreach (var value in findColumnsValues)
                // {
                //     Console.WriteLine(value);
                // }
                // foreach (var value in colFormatValues)
                // {
                //     Console.WriteLine(value);
                // }

                // MessageBox.Show("Files loaded successfully!", "Load Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*
        public void SetRequiredText()
        {
            // Clear the RichTextBox contents before adding new text
            rtx_ColFormat.Clear();
            rtx_FindColumns.Clear();
            rtx_ReportFormat.Clear();

            // Append values from findColumnsValues to rtx_FindColumns
            foreach (var value in FindColValues)
            {
                rtx_FindColumns.AppendText(value + Environment.NewLine);
            }

            // Append values from colFormatValues to rtx_ColFormat
            foreach (var value in ColReplaceFormat)
            {
                rtx_ColFormat.AppendText(value + Environment.NewLine);
            }
            // Append values from colFormatValues to rtx_ColFormat
            foreach (var value in ColReportFormat)
            {
                rtx_ReportFormat.AppendText(value + Environment.NewLine);
            }
        }*/

        public DataTable getMenuData()
        {
            // Define the path to the Excel file
            string folderPath = @"C:\File Parser\Menu.xlsx";

            // Initialize a DataTable to hold the data
            DataTable dataTable = new DataTable();

            try
            {
                // Ensure the file exists before attempting to read it
                if (File.Exists(folderPath))
                {
                    // Open the Excel package using EPPlus
                    using (var package = new ExcelPackage(new FileInfo(folderPath)))
                    {
                        // Get the first worksheet
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                        // Loop through the columns in the first row (header row)
                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            // Add columns to DataTable based on the header in the first row
                            dataTable.Columns.Add(worksheet.Cells[1, col].Text);
                        }

                        // Loop through all rows starting from the second row (to skip the header)
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            // Create a new row for each row in the Excel file
                            DataRow dataRow = dataTable.NewRow();

                            // Loop through each column and add the data to the DataRow
                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                dataRow[col - 1] = worksheet.Cells[row, col].Text;
                            }

                            // Add the row to the DataTable
                            dataTable.Rows.Add(dataRow);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException("The specified file does not exist.");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., file not found, reading errors)
                Console.WriteLine($"Error reading Excel file: {ex.Message}");
            }

            return dataTable;
        }

        private void UpdateRowCountLabel()
        {
            // Get the total number of rows in the DataGridView
            int rowCount = dgv_File.Rows.Count;

            // Format the row count with commas for thousands and above
            string formattedRowCount = rowCount.ToString("N0");

            // Display the formatted row count in the label
            lbl_GridCount.Text = $"{formattedRowCount}";
        }


        public void RenameColumnsInDataTable(DataTable dataTable, List<string> oldColumnNames, List<string> newColumnNames)
        {
            // Ensure both lists have the same number of entries
            if (oldColumnNames.Count != newColumnNames.Count)
            {
                MessageBox.Show("The number of old and new column names do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create a mapping of old column names to new column names
            Dictionary<string, string> columnMapping = oldColumnNames.Zip(newColumnNames, (oldName, newName) => new { oldName, newName })
                .ToDictionary(pair => pair.oldName, pair => pair.newName);

            // Rename the columns in the DataTable
            foreach (var mapping in columnMapping)
            {
                if (dataTable.Columns.Contains(mapping.Key))
                {
                    dataTable.Columns[mapping.Key].ColumnName = mapping.Value;
                }
                else
                {
                    Console.WriteLine($"Column '{mapping.Key}' not found in the DataTable.");
                }
            }
        }

        public DataTable MapRecordsData(DataTable sourceTable)
        {
            // Define the structure of the new DataTable based on ColReportFormat
            DataTable formattedTable = new DataTable();

            // Adding columns to the formatted table from ColReportFormat
            foreach (string targetColumnName in ColReportFormat)
            {
                formattedTable.Columns.Add(targetColumnName);
            }

            // Create the column mapping based on ColReportFormat dynamically
            var columnMapping = ColReportFormat
                .Take(ColReportSource.Count)
                .Select((targetColumnName, index) => new { SourceColumnName = ColReportSource[index], TargetColumnName = targetColumnName })
                .ToDictionary(x => x.SourceColumnName, x => x.TargetColumnName);

            // Map values from sourceTable to formattedTable
            foreach (DataRow sourceRow in sourceTable.Rows)
            {
                DataRow newRow = formattedTable.NewRow();

                foreach (var map in columnMapping)
                {
                    string sourceColumnName = map.Key;
                    string targetColumnName = map.Value;

                    if (sourceTable.Columns.Contains(sourceColumnName))
                    {
                        object value = sourceRow[sourceColumnName];
                        string valueString = value.ToString(); // Convert value once

                        // Check if the value is "1", "0", or something else
                        if (valueString == "1")
                        {
                            newRow[targetColumnName] = "YES";
                        }
                        else if (valueString == "0")
                        {
                            newRow[targetColumnName] = "NO";
                        }
                        else
                        {
                            newRow[targetColumnName] = value;
                        }
                    }
                }

                formattedTable.Rows.Add(newRow);
            }

            return formattedTable;
        }

        /*
        public DataTable MapRecordData(DataTable sourceTable)
        {
            // Define the structure of the new DataTable based on ColReportFormat
            DataTable formattedTable = new DataTable();

            // Adding columns to the formatted table from ColReportFormat
            foreach (string targetColumnName in ColReportFormat)
            {
                formattedTable.Columns.Add(targetColumnName);
            }

            // Define a manual mapping between ColReplaceFormat indices and ColReportFormat indices
            Dictionary<int, int> columnMapping = new Dictionary<int, int>
            {
                { 0, 0 }, // TERMINAL SN
                { 1, 5 }, // MID
                { 2, 4 }, // MERCHANT NAME
                { 3, 6 }, // DCC
                { 4, 10 },// INSTALLMENT
                { 6, 11 }, // PREAUTH
                { 7, 12 }, // OFFLINE SALE
                { 9, 13 }, // TIP ADJUST
                { 11, 14 }, // WECHAT
                { 12, 15 }, // ALIPAY
                { 15, 2 }, // AUTO SETTLE
                { 16, 1 }  // AUTO SETTLE TIME


                // Add more mappings as needed
            };

            // Map values from sourceTable to formattedTable
            foreach (DataRow sourceRow in sourceTable.Rows)
            {
                DataRow newRow = formattedTable.NewRow();

                foreach (var map in columnMapping)
                {
                    int sourceIndex = map.Key; // Index in ColReplaceFormat
                    int targetIndex = map.Value; // Index in ColReportFormat

                    if (sourceIndex < sourceTable.Columns.Count && targetIndex < ColReportFormat.Count)
                    {
                        string targetColumnName = ColReportFormat[targetIndex];
                        object value = sourceRow[sourceIndex];

                        // Map values based on "1" and "0"
                        if (value.ToString() == "1")
                        {
                            newRow[targetColumnName] = "YES";
                        }
                        else if (value.ToString() == "0")
                        {
                            newRow[targetColumnName] = "NO";
                        }
                        else
                        {
                            newRow[targetColumnName] = value;
                        }
                    }
                }

                formattedTable.Rows.Add(newRow);
            }

            return formattedTable;
        }
        */

        public DataTable LoadCsvAsDataTable(string csvFilePath)
        {
            btn_Process.Enabled = false;

            DataTable dt = new DataTable();

            try
            {
                // Check if file exists
                if (!File.Exists(csvFilePath))
                {
                    throw new FileNotFoundException($"The specified CSV file was not found: {csvFilePath}");
                }

                // Read all lines from the CSV file
                var lines = File.ReadLines(csvFilePath).Skip(3).ToList(); // Skip first 3 rows

                if (lines.Count < 1)
                {
                    throw new Exception("CSV file does not contain enough data after skipping the first 3 rows.");
                }

                // Read the header (4th line) and split it
                string[] headers = lines[0].Split(',');

                if (headers == null || headers.Length == 0)
                {
                    throw new Exception("CSV file headers could not be read.");
                }

                // Ensure unique column names and remove quotes
                HashSet<string> uniqueHeaders = new HashSet<string>();
                foreach (var header in headers)
                {
                    string cleanedHeader = header.Trim().Replace("\"", "");
                    uniqueHeaders.Add(cleanedHeader);
                    dt.Columns.Add(cleanedHeader); // Add as string column
                }

                // Process the data rows (lines[1:] will give us the remaining data rows)
                foreach (var line in lines.Skip(1))
                {
                    string[] rowValues = line.Split(',');

                    // Match row length with columns
                    var processedRow = new List<string>(dt.Columns.Count);

                    // Fill row data, convert types when applicable
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string value = i < rowValues.Length ? rowValues[i].Trim().Replace("\"", "") : string.Empty;

                        // Keep the value as string to avoid scientific notation
                        processedRow.Add(value); // Store the value directly as a string
                    }

                    dt.Rows.Add(processedRow.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // Or handle accordingly
            }
            finally
            {
                btn_Process.Enabled = true;
            }

            return dt;
        }


        private void ExportDataGridViewToExcel(DataGridView dgv)
        {
            // Check if the DataGridView has rows
            if (dgv.Rows.Count == 0 || (dgv.Rows.Count == 1 && dgv.Rows[0].IsNewRow))
            {
                MessageBox.Show("The Grid is empty. Nothing to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show SaveFileDialog to get the file path
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Data As Excel File";

                // If the user selects a file path
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = saveFileDialog.FileName;
                        SaveDataGridViewToExcel(dgv, filePath);

                        // Show MessageBox with the file path
                        MessageBox.Show($"File saved to: {filePath}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Show error message if something goes wrong
                        MessageBox.Show($"Error saving file: {ex.Message}", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveDataGridViewToExcel(DataGridView dgv, string filePath)
        {
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a worksheet to the package
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Write column headers
                for (int col = 0; col < dgv.Columns.Count; col++)
                {
                    worksheet.Cells[1, col + 1].Value = dgv.Columns[col].HeaderText;
                }

                // Write row data
                for (int row = 0; row < dgv.Rows.Count; row++)
                {
                    // Skip the "new row" placeholder
                    if (dgv.Rows[row].IsNewRow) continue;

                    for (int col = 0; col < dgv.Columns.Count; col++)
                    {
                        if (dgv.Rows[row].Cells[col].Value != null)
                        {
                            worksheet.Cells[row + 2, col + 1].Value = dgv.Rows[row].Cells[col].Value.ToString();
                        }
                    }
                }

                // Save the Excel file
                FileInfo fi = new FileInfo(filePath);
                package.SaveAs(fi);
            }
        }


        public DataTable ExtractSpecificColumns(DataTable originalTable)
        {
            DataTable newTable = new DataTable();

            try
            {
                // Use HashSet for fast lookup of column names in FindColValues
                HashSet<string> columnsToExtract = new HashSet<string>(FindColValues);

                // Add the required columns to the new DataTable
                foreach (string columnName in columnsToExtract)
                {
                    if (originalTable.Columns.Contains(columnName))
                    {
                        newTable.Columns.Add(columnName);
                    }
                    else
                    {
                        // Optionally, you can add error handling if a required column is missing
                        throw new Exception($"Column '{columnName}' is missing from the original table.");
                    }
                }

                // Add the data for the extracted columns to the new DataTable
                foreach (DataRow row in originalTable.Rows)
                {
                    DataRow newRow = newTable.NewRow();

                    // Copy values for the specified columns to the new row
                    foreach (string columnName in columnsToExtract)
                    {
                        newRow[columnName] = row[columnName];
                    }

                    newTable.Rows.Add(newRow);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the process
                //MessageBox.Show($"Error extracting columns: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
                
            }

            return newTable;
        }

        // Column No. Based
        /*
        public DataTable CompareMenuData(DataTable DTExport, DataTable MenuData)
        {
            // Add columns from MenuData to DTExport (skip column 0)
            foreach (DataColumn menuColumn in MenuData.Columns)
            {
                // Skip the first column (assumed to be column 0) from being added to DTExport
                if (menuColumn.Ordinal != 0 && !DTExport.Columns.Contains(menuColumn.ColumnName))
                {
                    DTExport.Columns.Add(menuColumn.ColumnName, menuColumn.DataType);
                }
            }

            // Loop through each row in DTExport to compare column 0 with MenuData column 0
            foreach (DataRow exportRow in DTExport.Rows)
            {
                // Find the MenuData row that matches the value in the first column
                DataRow[] matchingRows = MenuData.Select(MenuData.Columns[0].ColumnName + " = '" + exportRow[0] + "'");

                // If a match is found, update the additional columns in DTExport
                if (matchingRows.Length > 0)
                {
                    DataRow menuRow = matchingRows[0]; // Get the first match

                    // Loop through the columns in MenuData (excluding column 0) and update the corresponding columns in DTExport
                    foreach (DataColumn menuColumn in MenuData.Columns)
                    {
                        if (menuColumn.Ordinal != 0 && DTExport.Columns.Contains(menuColumn.ColumnName))
                        {
                            // Set the value from the matching row in MenuData to the respective column in DTExport
                            exportRow[menuColumn.ColumnName] = menuRow[menuColumn.ColumnName];
                        }
                    }
                }
            }

            // Return the updated DTExport
            return DTExport;
        }
        */

        // MENU ID based
        public DataTable CompareMenuData(DataTable DTExport, DataTable MenuData)
        {
            // Ensure "MENU ID" column exists in both DataTables
            if (!DTExport.Columns.Contains("MENU ID"))
            {
                throw new Exception("Rename File must contain the 'MENU ID' column.");
            }
            // Ensure "MENU ID" column exists in both DataTables
            if (!MenuData.Columns.Contains("MENU ID"))
            {
                throw new Exception("Menu File must contain the 'MENU ID' column.");
            }

            // Add columns from MenuData to DTExport (skip column "MENU ID")
            foreach (DataColumn menuColumn in MenuData.Columns)
            {
                // Skip the "MENU ID" column from being added to DTExport
                if (menuColumn.ColumnName != "MENU ID" && !DTExport.Columns.Contains(menuColumn.ColumnName))
                {
                    DTExport.Columns.Add(menuColumn.ColumnName, menuColumn.DataType);
                }
            }

            // Loop through each row in DTExport to compare "MENU ID" with MenuData "MENU ID"
            foreach (DataRow exportRow in DTExport.Rows)
            {
                // Find the MenuData row that matches the value in the "MENU ID" column
                string menuIdValue = exportRow["MENU ID"].ToString(); // Get the value of "MENU ID"

                DataRow[] matchingRows = MenuData.Select("`MENU ID` = '" + menuIdValue + "'");

                // If a match is found, update the additional columns in DTExport
                if (matchingRows.Length > 0)
                {
                    DataRow menuRow = matchingRows[0]; // Get the first match

                    // Loop through the columns in MenuData (excluding "MENU ID") and update the corresponding columns in DTExport
                    foreach (DataColumn menuColumn in MenuData.Columns)
                    {
                        if (menuColumn.ColumnName != "MENU ID" && DTExport.Columns.Contains(menuColumn.ColumnName))
                        {
                            // Set the value from the matching row in MenuData to the respective column in DTExport
                            exportRow[menuColumn.ColumnName] = menuRow[menuColumn.ColumnName];
                        }
                    }
                }
            }

            // Return the updated DTExport
            return DTExport;
        }

        public void CheckFilesAndFolder()
        {
   
            // Ensure the folder exists
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                Console.WriteLine($"Folder created at: {FolderPath}");
            }
            else
            {
                Console.WriteLine("Folder already exists.");
            }


            // Ensure Lookup file exists
            if (!File.Exists(LookupFormatPath))
            {
                File.WriteAllText(LookupFormatPath, ""); // Create empty file
                Console.WriteLine($"File created: {LookupFormatPath}");
            }
            else
            {
                Console.WriteLine("Lookup file already exists.");
            }

            // Ensure Replace file exists
            if (!File.Exists(ReplaceFormatPath))
            {
                File.WriteAllText(ReplaceFormatPath, ""); // Create empty file
                Console.WriteLine($"File created: {ReplaceFormatPath}");
            }
            else
            {
                Console.WriteLine("Replace file already exists.");
            }
        }

        /*
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
                File.WriteAllText(LookupFormatPath, rtx_FindColumns.Text);
                File.WriteAllText(ReplaceFormatPath, rtx_ColFormat.Text);
                File.WriteAllText(ReportFormatPath, rtx_ReportFormat.Text);

                MessageBox.Show($"Saved to: \n{ReplaceFormatPath}\n{LookupFormatPath}\n{ReportFormatPath}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }*/

        private void ProcessData()
        {
            DTFinal = new DataTable();

            btn_Process.Enabled = false;

            foreach (string filePath in filePaths)
            {
                DTExport = new DataTable();
                
                // Load Raw Data
                DTProcess = LoadCsvAsDataTable(filePath);

                // Get Specific Columns and Data
                DTExport = ExtractSpecificColumns(DTProcess);

                if (DTExport?.Rows.Count > 0)
                {
                    if (DTFinal.Columns.Count == 0)
                    {
                        DTFinal = DTExport.Clone();
                    }
                }
                else
                {
                    //Console.WriteLine("DTExport is either null or has no data.");
                    btn_Clear.PerformClick();
                    return;
                }

                // Merge Data
                DTFinal.Merge(DTExport);
            }


            // Display the final merged data
            RenameColumnsInDataTable(DTFinal, FindColValues, ColReplaceFormat);

            // Add Columns For Menu ID.
            DTFinal = CompareMenuData(DTFinal, MenuData);

            // Sort Columns
            DTFinal = FormatDataTable(DTFinal, ColReportSource);

            // Display Final Data
            dgv_File.DataSource = MapRecordsData(DTFinal);

            MessageBox.Show("Data Processed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btn_Import.Enabled = false;
            btn_Process.Enabled = false;
        }

        private void UpdateControlsState()
        {
            // Check if the DataGridView has meaningful rows
            bool hasDataGridViewRows = dgv_File.Rows.Count > 0 && !dgv_File.Rows[0].IsNewRow;

            // Check if the ListBox has items
            bool hasListBoxItems = lbx_FilePath.Items.Count > 0;

            // Enable the clear button if either control has values
            btn_Clear.Enabled = hasDataGridViewRows || hasListBoxItems;

            // Enable or disable other buttons (if needed)
            btn_Export.Enabled = hasDataGridViewRows;

            // Enable or disable the button based on the ListBox items
            btn_Process.Enabled = lbx_FilePath.Items.Count > 0;
        }

        public DataTable FormatDataTable(DataTable sourceTable, List<string> ReportFormat)
        {
            // Create a new DataTable to store the formatted data
            DataTable formattedTable = new DataTable();

            // Ensure the columns are added in the order specified in ReportFormat
            foreach (string targetColumnName in ReportFormat)
            {
                // Add the column if it exists in the sourceTable
                if (sourceTable.Columns.Contains(targetColumnName))
                {
                    formattedTable.Columns.Add(targetColumnName);
                }
                else
                {
                    // Add the missing column manually
                    formattedTable.Columns.Add(targetColumnName);
                }
            }

            // Populate the formatted table with data from the source table
            foreach (DataRow sourceRow in sourceTable.Rows)
            {
                DataRow newRow = formattedTable.NewRow();

                // Map the data from sourceTable to formattedTable
                foreach (string targetColumnName in ReportFormat)
                {
                    if (sourceTable.Columns.Contains(targetColumnName))
                    {
                        // Transfer the value to the new row
                        newRow[targetColumnName] = sourceRow[targetColumnName];
                    }
                    else
                    {
                        // If the column is missing, you can either leave it empty or fill with a default value
                        newRow[targetColumnName] = DBNull.Value; // Or any default value like "" or "N/A"
                    }
                }

                formattedTable.Rows.Add(newRow);
            }

            return formattedTable;
        }


        private void btn_Import_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                openFileDialog.Title = "Select Files";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        if (filePaths.Contains(filePath))
                        {
                            // Prompt if the file is already added
                            MessageBox.Show($"The file \"{filePath}\" is already added.", "Duplicate File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            // Add the file to the list and display it in the ListView
                            filePaths.Add(filePath);
                            //listView1.Items.Add(new ListViewItem(filePath));
                            lbx_FilePath.Items.Add(filePath);
                            UpdateControlsState();

                        }
                    }
                }
            }
        }

        private void btn_Process_Click(object sender, EventArgs e)
        {
            ProcessData();
        }

        private void btn_Export_Click(object sender, EventArgs e)
        {
            ExportDataGridViewToExcel(dgv_File);
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void btn_SaveTxt_Click(object sender, EventArgs e)
        {
            //saveToTextFile();
        }

        private void dgv_File_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            UpdateRowCountLabel();
            UpdateControlsState();
        }

        private void ExportDataToExcel(DataTable dt)
        {
            // Show SaveFileDialog to get the file path
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Data As Excel File";

                // If the user selects a file path
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = saveFileDialog.FileName;
                        SaveDataTableToExcel(dt, filePath);

                        // Show MessageBox with the file path
                        MessageBox.Show($"File saved to: {filePath}", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // Show error message if something goes wrong
                        MessageBox.Show($"Error saving file: {ex.Message}", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveDataTableToExcel(DataTable dt, string filePath)
        {
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a worksheet to the package
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Load the DataTable into the worksheet
                worksheet.Cells["A1"].LoadFromDataTable(dt, PrintHeaders: true);

                // Save the Excel file
                FileInfo fi = new FileInfo(filePath);
                package.SaveAs(fi);
            }
        }

        private void btn_Settings_Click(object sender, EventArgs e)
        {
            Form Settings = new frm_Settings();
            Settings.ShowDialog();
        }

        public void RefreshData()
        {
            LookupPath();

            filePaths.Clear();
            lbx_FilePath.Items.Clear();
            DTFinal?.Clear();
            DTProcess?.Clear();
            DTExport?.Clear();

            dgv_File.DataSource = null;
            btn_Import.Enabled = true;

            UpdateRowCountLabel();
            UpdateControlsState();
        }
    }
}
