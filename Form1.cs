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
using System.Windows.Forms.DataVisualization.Charting;
namespace Unit_4_CSharp
{
    public partial class Form1 : Form
    {
        private string filePathCSV;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            filePathCSV = openFileDialog.FileName;
            ShowCSV_InDGV();
        }
        public void ShowCSV_InDGV()
        {
            dgvTableCSV.Rows.Clear();
            dgvTableCSV.Columns.Clear();

            // Read lines from the CSV file
            string[] lines = File.ReadAllLines(filePathCSV);

            // If there are lines in the file
            if (lines.Length > 0)
            {
                // Get column names from the first record
                string[] columnNames = lines[0].Split(',');

                // Add columns to the DataGridView using the column names from the CSV
                foreach (string columnName in columnNames)
                {
                    dgvTableCSV.Columns.Add(columnName, columnName);
                }

                // Add rows to the DataGridView with the content from the CSV (excluding the first line)
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] fields = lines[i].Split(',');
                    dgvTableCSV.Rows.Add(fields);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePathCSV) || !File.Exists(filePathCSV))
            {
                MessageBox.Show("Select a file to work with.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (txtSearchCSV.Text == "")
            {
                MessageBox.Show("To search, you must use a NAME.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }


            try
            {
                // Read the CSV file line by line
                using (StreamReader reader = new StreamReader(filePathCSV))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] fields = line.Split(',');

                        // Compare the search term with the first field (in this case, the name)
                        if (fields.Length > 0 && fields[0] == txtSearchCSV.Text)
                        {
                            MessageBox.Show("Found: " + string.Join(", ", fields), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error searching in the CSV file: " + ex.Message);
            }
            MessageBox.Show("No matching name found in the file", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Verificar si hay datos en el DataGridView
            if (dgvTableCSV.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para guardar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Obtener la ruta del archivo CSV
                string filePath = filePathCSV;

                // Crear un StringBuilder para construir el contenido del archivo CSV
                StringBuilder csvContent = new StringBuilder();

                // Agregar los encabezados de columna al archivo CSV
                for (int i = 0; i < dgvTableCSV.Columns.Count; i++)
                {
                    csvContent.Append(dgvTableCSV.Columns[i].HeaderText);
                    if (i < dgvTableCSV.Columns.Count - 1)
                    {
                        csvContent.Append(",");
                    }
                }
                csvContent.AppendLine(); // Agregar nueva línea después de los encabezados

                // Obtener los datos y los índices de las filas que tienen valores en la columna "Revenue"
                var revenueData = dgvTableCSV.Rows.Cast<DataGridViewRow>()
                                    .Where(row => row.Cells["Revenue"].Value != null)
                                    .Select(row => new
                                    {
                                        Value = Convert.ToDecimal(row.Cells["Revenue"].Value),
                                        Index = row.Index
                                    })
                                    .OrderByDescending(row => row.Value)
                                    .ToList();

                // Agregar datos de cada fila al archivo CSV en el orden ordenado
                foreach (var rowData in revenueData)
                {
                    DataGridViewRow row = dgvTableCSV.Rows[rowData.Index];
                    bool hasData = false; // Bandera para verificar si la fila tiene celdas no vacías
                    StringBuilder rowDataStr = new StringBuilder();

                    for (int i = 0; i < dgvTableCSV.Columns.Count; i++)
                    {
                        // Verificar si el valor de la celda no es nulo ni está vacío
                        if (row.Cells[i].Value != null && !string.IsNullOrWhiteSpace(row.Cells[i].Value.ToString()))
                        {
                            rowDataStr.Append(row.Cells[i].Value.ToString());
                            hasData = true; // Establecer la bandera en true si la celda tiene un valor no vacío
                        }

                        if (i < dgvTableCSV.Columns.Count - 1)
                        {
                            rowDataStr.Append(",");
                        }
                    }

                    // Agregar los datos de la fila al contenido del CSV si la fila tiene celdas no vacías
                    if (hasData)
                    {
                        csvContent.AppendLine(rowDataStr.ToString());
                    }
                }

                // Escribir el contenido en el archivo CSV
                File.WriteAllText(filePath, csvContent.ToString());

                MessageBox.Show("Los datos se guardaron correctamente en el archivo CSV.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar datos en el archivo CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ShowCSV_InDGV();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Check if a file is selected
            if (string.IsNullOrWhiteSpace(filePathCSV) || !File.Exists(filePathCSV))
            {
                MessageBox.Show("Select a valid file to work with.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Check if a name to search for is entered
            if (string.IsNullOrWhiteSpace(txtSearchCSV.Text))
            {
                MessageBox.Show("Please enter a name to search for.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                // Read the CSV file line by line and write the non-deleted lines to a new temporary file
                string tempFilePath = Path.GetTempFileName();
                using (StreamReader reader = new StreamReader(filePathCSV))
                using (StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Check if the current line contains the name to delete
                        if (!line.Contains(txtSearchCSV.Text))
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                // Replace the original file with the temporary file
                File.Delete(filePathCSV);
                File.Move(tempFilePath, filePathCSV);
                ShowCSV_InDGV();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting line from the CSV file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveNewFile_Click(object sender, EventArgs e)
        {
            // Check if there is data in the DataGridView
            if (dgvTableCSV.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the index of the "Revenue" column
            int revenueColumnIndex = dgvTableCSV.Columns["Revenue"].Index;

            // Obtener los datos y los índices de fila de la columna "Revenue", excluyendo la última fila
            var revenueData = dgvTableCSV.Rows.Cast<DataGridViewRow>()
                .Where(row => row.Index != dgvTableCSV.Rows.Count - 1 && row.Cells[revenueColumnIndex].Value != null)
                .Select(row => Convert.ToDecimal(row.Cells[revenueColumnIndex].Value))
                .ToList();

            var rowIndices = dgvTableCSV.Rows.Cast<DataGridViewRow>()
                .Where(row => row.Index != dgvTableCSV.Rows.Count - 1 && row.Cells[revenueColumnIndex].Value != null)
                .Select(row => row.Index)
                .ToList();

            // Sort the revenue data and row indices in descending order
            var sortedData = revenueData.OrderByDescending(x => x).ToList();
            var sortedIndices = rowIndices.OrderByDescending(x => revenueData[rowIndices.IndexOf(x)]).ToList();

            // Create a SaveFileDialog instance
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            saveFileDialog.Title = "Save as CSV File";

            // Set initial directory and default file name
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePathCSV);
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filePathCSV) + "_new.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            filePathCSV = saveFileDialog.FileName;

            try
            {
                // Create a StringBuilder to build the content of the CSV file
                StringBuilder csvContent = new StringBuilder();

                // Add column headers to the CSV file
                for (int i = 0; i < dgvTableCSV.Columns.Count; i++)
                {
                    csvContent.Append(dgvTableCSV.Columns[i].HeaderText);
                    if (i < dgvTableCSV.Columns.Count - 1)
                    {
                        csvContent.Append(",");
                    }
                }
                csvContent.AppendLine(); // Add new line after headers

                // Add data from each row to the CSV file
                foreach (int rowIndex in sortedIndices)
                {
                    DataGridViewRow row = dgvTableCSV.Rows[rowIndex];
                    bool hasData = false; // Flag to check if the row has any non-empty cells
                    StringBuilder rowData = new StringBuilder();

                    for (int i = 0; i < dgvTableCSV.Columns.Count; i++)
                    {
                        // Check if the cell value is not null or empty
                        if (row.Cells[i].Value != null && !string.IsNullOrWhiteSpace(row.Cells[i].Value.ToString()))
                        {
                            rowData.Append(row.Cells[i].Value.ToString());
                            hasData = true; // Set flag to true if the cell has non-empty value
                        }

                        if (i < dgvTableCSV.Columns.Count - 1)
                        {
                            rowData.Append(",");
                        }
                    }

                    // Add row data to the CSV content if the row has any non-empty cells
                    if (hasData)
                    {
                        csvContent.AppendLine(rowData.ToString());
                    }
                }

                // Write the content to the CSV file
                File.WriteAllText(filePathCSV, csvContent.ToString());

                MessageBox.Show("Data saved successfully to the CSV file.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data to the CSV file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ShowCSV_InDGV();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Obtener los datos del DataGridView
            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            foreach (DataGridViewRow row in dgvTableCSV.Rows)
            {
                // Verificar si la celda "Product" no es nula
                if (row.Cells["Product"].Value != null)
                {
                    string product = row.Cells["Product"].Value.ToString();
                    decimal revenue = 0;

                    // Verificar si la celda "Revenue" no es nula
                    if (row.Cells["Revenue"].Value != null && decimal.TryParse(row.Cells["Revenue"].Value.ToString(), out revenue))
                    {
                        if (!data.ContainsKey(product))
                        {
                            data.Add(product, revenue);
                        }
                    }
                }
            }

            // Limpiar el Chart antes de agregar nuevos datos
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Title = "Productos";
            chart1.ChartAreas[0].AxisY.Title = "Revenue";

            // Definir una lista de colores diferentes para cada serie
            Color[] colors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Yellow };

            // Contador para iterar sobre la lista de colores
            int colorIndex = 0;

            // Agregar datos al Chart
            foreach (var entry in data)
            {
                string product = entry.Key;
                decimal revenue = entry.Value;

                // Agregar una nueva serie al gráfico con el nombre del producto
                chart1.Series.Add(product);

                // Asignar un color diferente a la serie actual
                chart1.Series[product].Color = colors[colorIndex];

                // Agregar el punto (valor de revenue) a la serie actual
                chart1.Series[product].Points.AddY(revenue);

                // Incrementar el índice del color
                colorIndex++;

                // Reiniciar el índice del color si se excede el número de colores definidos
                if (colorIndex >= colors.Length)
                {
                    colorIndex = 0;
                }
            }

            // Personalizar el gráfico (opcional)
            chart1.ChartAreas[0].AxisX.Interval = 1; // Mostrar una etiqueta por cada producto
            chart1.Series[0].ChartType = SeriesChartType.Column; // Tipo de gráfico de barras
        }
    }

}