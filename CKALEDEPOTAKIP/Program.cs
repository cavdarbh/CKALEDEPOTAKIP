using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using DepoTakipUygulamasi;
using Microsoft.VisualBasic.Devices;

namespace DepoTakipUygulamasi
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }



    public class MainForm : Form
    {
        static string dbPath = Path.Combine(Application.StartupPath, "sqlite DepoDB.sqlite");
        string connectionString = $"Data Source={dbPath};Version=3;Pooling=False;Journal Mode=WAL;";

        DataGridView dgvDevices;
        Button btnAddDevice, btnUpdateDevice, btnDeleteDevice, btnFilter, btnAddType, btnDeleteType, btnClearFilter;
        private ComboBox comboBoxCihazTuru;

        public MainForm()
        {
            this.Text = "Depo Takip Uygulamasý";
            this.Width = 1000;
            this.Height = 700;

            dgvDevices = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(dgvDevices);

            Panel panelButtons = new Panel
            {
                Height = 100,
                Dock = DockStyle.Bottom
            };
            this.Controls.Add(panelButtons);

            btnFilter = new Button { Text = "Filtrele", Width = 150, Left = 10, Top = 10 };
            btnFilter.Click += BtnFilter_Click;
            panelButtons.Controls.Add(btnFilter);

            btnAddDevice = new Button { Text = "Cihaz Ekle", Width = 150, Left = 170, Top = 10 };
            btnAddDevice.Click += BtnAddDevice_Click;
            panelButtons.Controls.Add(btnAddDevice);

            btnUpdateDevice = new Button { Text = "Cihaz Güncelle", Width = 150, Left = 330, Top = 10 };
            btnUpdateDevice.Click += BtnUpdateDevice_Click;
            panelButtons.Controls.Add(btnUpdateDevice);

            btnDeleteDevice = new Button { Text = "Cihaz Sil", Width = 150, Left = 490, Top = 10 };
            btnDeleteDevice.Click += BtnDeleteDevice_Click;
            panelButtons.Controls.Add(btnDeleteDevice);

            btnAddType = new Button { Text = "Cihaz Tipi Ekle", Width = 150, Left = 650, Top = 10 };
            btnAddType.Click += BtnAddType_Click;
            panelButtons.Controls.Add(btnAddType);

            btnDeleteType = new Button { Text = "Cihaz Tipi Sil", Width = 150, Left = 810, Top = 10 };
            btnDeleteType.Click += BtnDeleteType_Click;
            panelButtons.Controls.Add(btnDeleteType);

            btnClearFilter = new Button { Text = "Filtreyi Temizle", Width = 150, Left = 170, Top = 50 };
            btnClearFilter.Click += BtnClearFilter_Click;
            panelButtons.Controls.Add(btnClearFilter);

            ListDevices();
        }

        public void ListDevices(string filterQuery = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT p.*, dt.TypeName AS CihazTipi FROM Properties p 
                             LEFT JOIN Device d ON d.SerialNo = p.SeriNo 
                             LEFT JOIN DeviceType dt ON dt.TypeID = d.TypeID";
                if (!string.IsNullOrEmpty(filterQuery))
                    query += " WHERE " + filterQuery;

                SQLiteDataAdapter da = new SQLiteDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvDevices.DataSource = dt;
            }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            FilterForm filterForm = new FilterForm(this);
            filterForm.ShowDialog();
        }

        private void BtnAddDevice_Click(object sender, EventArgs e)
        {
            AddDeviceForm addForm = new AddDeviceForm(connectionString);
            addForm.ShowDialog();
            ListDevices();
        }

        private void BtnUpdateDevice_Click(object sender, EventArgs e)
        {
            if (dgvDevices.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvDevices.SelectedRows[0].Cells["PropertyID"].Value);
                AddDeviceForm updateForm = new AddDeviceForm(connectionString, id);
                updateForm.ShowDialog();
                ListDevices();
            }
        }

        private void BtnDeleteDevice_Click(object sender, EventArgs e)
        {
            if (dgvDevices.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvDevices.SelectedRows[0].Cells["PropertyID"].Value);
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Properties WHERE PropertyID=@ID", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Cihaz silindi.");
                    ListDevices();
                }
            }
        }

        private void BtnAddType_Click(object sender, EventArgs e)
        {
            string typeName = Microsoft.VisualBasic.Interaction.InputBox("Yeni Cihaz Tipi Adý:", "Cihaz Tipi Ekle", "");
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO DeviceType (TypeName) VALUES (@TypeName)", conn);
                    cmd.Parameters.AddWithValue("@TypeName", typeName);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Cihaz tipi eklendi.");
                }
            }
        }

        private void BtnDeleteType_Click(object sender, EventArgs e)
        {
            DeleteTypeForm deleteTypeForm = new DeleteTypeForm(connectionString);
            deleteTypeForm.ShowDialog();
        }

        private void BtnClearFilter_Click(object sender, EventArgs e)
        {
            ListDevices();
        }
    }



    public class FilterForm : Form
    {
        private FlowLayoutPanel flowPanel;
        private Button btnAddFilter;
        private Button btnApply;
        private MainForm mainForm;

        public FilterForm(MainForm form)
        {
            mainForm = form;
            this.Text = "Geliþmiþ Filtreleme";
            this.Width = 500;
            this.Height = 400;

            flowPanel = new FlowLayoutPanel
            {
                Left = 10,
                Top = 10,
                Width = 460,
                Height = 300,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            this.Controls.Add(flowPanel);

            btnAddFilter = new Button { Text = "Filtre Satýrý Ekle", Left = 10, Top = 320, Width = 150 };
            btnAddFilter.Click += BtnAddFilter_Click;
            this.Controls.Add(btnAddFilter);

            btnApply = new Button { Text = "Filtreyi Uygula", Left = 170, Top = 320, Width = 150 };
            btnApply.Click += BtnApply_Click;
            this.Controls.Add(btnApply);

            AddFilterRow(); // Baþlangýçta 1 filtre satýrý olsun
        }

        private void AddFilterRow()
        {
            Panel panel = new Panel { Width = 450, Height = 30 };

            ComboBox cmbColumns = new ComboBox { Left = 0, Width = 150 };
            cmbColumns.Items.AddRange(new string[]
            {
            "Marka", "Model", "SeriNo", "BarkodNo", "CPU", "GPU", "AgKarti", "Bellek",
            "RamKap", "RamMHz", "RamCinsi", "Depolama", "DiskCinsi", "DiskKapasite", "Disk2",
            "CalisiyorMu", "ArizaliMi", "Aciklama", "Lokasyon"
            });
            cmbColumns.SelectedIndex = 0;

            ComboBox cmbOperator = new ComboBox { Left = 160, Width = 80 };
            cmbOperator.Items.AddRange(new string[] { "=", "LIKE", ">", "<", ">=", "<=" });
            cmbOperator.SelectedIndex = 0;

            TextBox txtValue = new TextBox { Left = 250, Width = 180 };

            panel.Controls.Add(cmbColumns);
            panel.Controls.Add(cmbOperator);
            panel.Controls.Add(txtValue);

            flowPanel.Controls.Add(panel);
        }

        private void BtnAddFilter_Click(object sender, EventArgs e)
        {
            AddFilterRow();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            List<string> filters = new List<string>();

            foreach (Panel panel in flowPanel.Controls)
            {
                var cmbColumns = panel.Controls[0] as ComboBox;
                var cmbOperator = panel.Controls[1] as ComboBox;
                var txtValue = panel.Controls[2] as TextBox;

                if (string.IsNullOrWhiteSpace(txtValue.Text))
                    continue; // Boþsa filtre satýrýný atla

                string col = cmbColumns.SelectedItem.ToString();
                string op = cmbOperator.SelectedItem.ToString();
                string val = txtValue.Text.Replace("'", "''");

                if (op == "LIKE")
                    filters.Add($"{col} LIKE '%{val}%'");
                else
                    filters.Add($"{col} {op} '{val}'");
            }

            string filterQuery = string.Join(" AND ", filters);

            mainForm.ListDevices(filterQuery);
            this.Close();
        }
    }

    public class AddDeviceForm : Form
    {
        string connectionString;
        int? deviceId;
        Dictionary<string, TextBox> textBoxes = new Dictionary<string, TextBox>();
        ComboBox cmbDeviceType;
        CheckBox chkCalisiyorMu, chkArizaliMi;
        Button btnSave;
        private static readonly object dbLock = new object();

        public class ComboBoxItem
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString() => Name; // Display için
        }


        public AddDeviceForm(string connStr, int? id = null)
        {
            connectionString = connStr;
            deviceId = id;

            this.Text = id == null ? "Yeni Cihaz Ekle" : "Cihaz Güncelle";
            this.Width = 600;
            this.Height = 850;
            int top = 20;

            string[] fields = { "Marka", "Model", "SeriNo", "BarkodNo", "CPU", "GPU", "AgKarti", "Bellek", "RamKap", "RamMHz", "RamCinsi", "Depolama", "DiskCinsi", "DiskKapasite", "Disk2", "Aciklama", "Lokasyon" };

            foreach (var field in fields)
            {
                Label lbl = new Label() { Text = field, Left = 20, Top = top, Width = 150 };
                TextBox txt = new TextBox() { Left = 180, Top = top, Width = 350 };
                this.Controls.Add(lbl);
                this.Controls.Add(txt);
                textBoxes.Add(field, txt);
                top += 30;
            }

            // Cihaz tipi
            Label lblType = new Label() { Text = "Cihaz Tipi", Left = 20, Top = top, Width = 150 };
            cmbDeviceType = new ComboBox() { Left = 180, Top = top, Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(lblType);
            this.Controls.Add(cmbDeviceType);
            top += 30;

            LoadDeviceTypes();

            // Checkboxlar
            chkCalisiyorMu = new CheckBox() { Text = "Çalýþýyor Mu", Left = 180, Top = top };
            this.Controls.Add(chkCalisiyorMu);
            top += 30;

            chkArizaliMi = new CheckBox() { Text = "Arýzalý Mý", Left = 180, Top = top };
            this.Controls.Add(chkArizaliMi);
            top += 30;

            // Kaydet butonu
            btnSave = new Button() { Text = "Kaydet", Left = 180, Top = top, Width = 150 };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Varsa veriyi yükle
            if (deviceId != null)
                LoadDeviceData();
            else
            {
                chkCalisiyorMu.Checked = true;
                chkArizaliMi.Checked = false;
            }
        }

        private void LoadDeviceTypes()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("SELECT TypeID, TypeName FROM DeviceType", conn);
                SQLiteDataReader reader = cmd.ExecuteReader();

                List<ComboBoxItem> items = new List<ComboBoxItem>();
                while (reader.Read())
                {
                    items.Add(new ComboBoxItem
                    {
                        Id = Convert.ToInt32(reader["TypeID"]),
                        Name = reader["TypeName"].ToString()
                    });
                }

                cmbDeviceType.DataSource = items;
                cmbDeviceType.DisplayMember = "Name";
                cmbDeviceType.ValueMember = "Id";
            }
        }


        private void LoadDeviceData()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                SQLiteCommand cmd = new SQLiteCommand("SELECT p.*, d.TypeID FROM Properties p LEFT JOIN Device d ON d.SerialNo = p.SeriNo WHERE p.PropertyID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", deviceId);
                SQLiteDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    foreach (var key in textBoxes.Keys)
                        textBoxes[key].Text = reader[key].ToString();

                    if (reader["TypeID"] != DBNull.Value)
                        cmbDeviceType.SelectedValue = Convert.ToInt32(reader["TypeID"]);
                    else
                        cmbDeviceType.SelectedIndex = -1;

                    chkCalisiyorMu.Checked = Convert.ToBoolean(reader["CalisiyorMu"]);
                    chkArizaliMi.Checked = Convert.ToBoolean(reader["ArizaliMi"]);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lock(dbLock)
            {
                if (deviceId == null)
                {
                    MessageBox.Show("Bir cihaz seçmeden güncelleme yapamazsýnýz.");
                    return;
                }

                try
                {
                    using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                    {
                        conn.Open();

                        int typeId = (int)cmbDeviceType.SelectedValue;
                        string seriNo = textBoxes["SeriNo"].Text;

                        // Device tablosunu güncelle
                        using (SQLiteCommand cmdDevice = new SQLiteCommand(
                            "UPDATE Device SET TypeID = @TypeID WHERE SerialNo = @SeriNo", conn))
                        {
                            cmdDevice.Parameters.AddWithValue("@TypeID", typeId);
                            cmdDevice.Parameters.AddWithValue("@SeriNo", seriNo);
                            cmdDevice.ExecuteNonQuery();
                        }

                        // Properties tablosunu güncelle
                        string setClause = string.Join(", ", textBoxes.Keys.Select(k => $"{k} = @{k}")) + ", CalisiyorMu = @CalisiyorMu, ArizaliMi = @ArizaliMi";

                        using (SQLiteCommand cmdUpdate = new SQLiteCommand(
                            $"UPDATE Properties SET {setClause} WHERE PropertyID = @PropertyID", conn))
                        {
                            foreach (var kvp in textBoxes)
                            {
                                cmdUpdate.Parameters.AddWithValue("@" + kvp.Key, string.IsNullOrWhiteSpace(kvp.Value.Text) ? (object)DBNull.Value : kvp.Value.Text);
                            }
                            cmdUpdate.Parameters.AddWithValue("@CalisiyorMu", chkCalisiyorMu.Checked);
                            cmdUpdate.Parameters.AddWithValue("@ArizaliMi", chkArizaliMi.Checked);
                            cmdUpdate.Parameters.AddWithValue("@PropertyID", deviceId.Value);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        MessageBox.Show("Cihaz güncellendi.");
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }


    }

    public class DeleteTypeForm : Form
    {
        ComboBox cmbTypes;
        Button btnDelete;
        string connectionString;

        public DeleteTypeForm(string connStr)
        {
            connectionString = connStr;
            this.Text = "Cihaz Tipi Sil";
            this.Width = 400;
            this.Height = 150;

            cmbTypes = new ComboBox
            {
                Left = 20,
                Top = 20,
                Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbTypes);

            btnDelete = new Button
            {
                Text = "Seçili Tipi Sil",
                Left = 120,
                Top = 60,
                Width = 140
            };
            btnDelete.Click += BtnDelete_Click;
            this.Controls.Add(btnDelete);

            LoadTypes();
        }

        private void LoadTypes()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("SELECT TypeID, TypeName FROM DeviceType", conn);
                SQLiteDataReader reader = cmd.ExecuteReader();

                var types = new Dictionary<long, string>();
                while (reader.Read())
                {
                    long typeId = Convert.ToInt64(reader["TypeID"]);
                    string typeName = reader["TypeName"].ToString();
                    types.Add(typeId, typeName);
                }

                cmbTypes.DataSource = new BindingSource(types, null);
                cmbTypes.DisplayMember = "Value";
                cmbTypes.ValueMember = "Key";
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (cmbTypes.SelectedValue == null)
            {
                MessageBox.Show("Lütfen bir cihaz tipi seçin.");
                return;
            }

            long selectedId = (long)cmbTypes.SelectedValue;

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Geliþmiþ kontrol: sadece eþleþen cihaz varsa engelle
                SQLiteCommand checkCmd = new SQLiteCommand(@"
                    SELECT COUNT(*) 
                    FROM Device d 
                    JOIN Properties p ON p.SeriNo = d.SerialNo 
                    WHERE d.TypeID = @ID", conn);
                checkCmd.Parameters.AddWithValue("@ID", selectedId);

                object countResult = checkCmd.ExecuteScalar();
                long usageCount = 0;

                if (countResult != null && countResult != DBNull.Value)
                    usageCount = Convert.ToInt64(countResult);

                if (usageCount > 0)
                {
                    MessageBox.Show("Bu cihaz tipi kullanýmda olduðu için silinemez.");
                    return;
                }

                // Gerekirse orphan kalmýþ Device kayýtlarýný da sil
                SQLiteCommand delDevice = new SQLiteCommand("DELETE FROM Device WHERE TypeID = @ID", conn);
                delDevice.Parameters.AddWithValue("@ID", selectedId);
                delDevice.ExecuteNonQuery();

                // Cihaz tipi sil
                SQLiteCommand delType = new SQLiteCommand("DELETE FROM DeviceType WHERE TypeID = @ID", conn);
                delType.Parameters.AddWithValue("@ID", selectedId);
                delType.ExecuteNonQuery();

                MessageBox.Show("Cihaz tipi baþarýyla silindi.");
                this.Close();
            }
        }
    }
}
