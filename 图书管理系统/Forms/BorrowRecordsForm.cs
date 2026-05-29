using BookManager.Models;

namespace BookManager.Forms
{
    public partial class BorrowRecordsForm : Form
    {
        private int _userId;
        private string _role;

        public BorrowRecordsForm(int userId, string role)
        {
            InitializeComponent();
            _userId = userId;
            _role = role;

            if (_role != "admin")
            {
                btnReturn.Visible = false;
            }

            LoadRecords();
        }

        private void LoadRecords()
        {
            try
            {
                List<BorrowRecord> records;
                if (_role == "admin")
                {
                    records = DbHelper.GetBorrowRecords();
                }
                else
                {
                    records = DbHelper.GetBorrowRecords(_userId);
                }

                dgvRecords.DataSource = records.Select(r => new
                {
                    r.Id,
                    r.UserName,
                    r.BookTitle,
                    r.BorrowDate,
                    r.DueDate,
                    r.ReturnDate,
                    r.Status,
                    IsOverdue = r.Status == "borrowed" && r.DueDate < DateTime.Now
                }).ToList();

                dgvRecords.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载记录失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要归还的记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int recordId = Convert.ToInt32(dgvRecords.SelectedRows[0].Cells["Id"].Value);
                string bookTitle = dgvRecords.SelectedRows[0].Cells["BookTitle"].Value.ToString() ?? "";
                string status = dgvRecords.SelectedRows[0].Cells["Status"].Value.ToString() ?? "";

                if (status == "returned")
                {
                    MessageBox.Show("该图书已归还", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = DbHelper.ReturnBook(recordId);

                if (success)
                {
                    MessageBox.Show($"成功归还图书: {bookTitle}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRecords();
                }
                else
                {
                    MessageBox.Show("归还失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("归还失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.dgvRecords = new System.Windows.Forms.DataGridView();
            this.btnReturn = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecords)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvRecords
            // 
            this.dgvRecords.AllowUserToAddRows = false;
            this.dgvRecords.AllowUserToDeleteRows = false;
            this.dgvRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecords.Location = new System.Drawing.Point(10, 10);
            this.dgvRecords.Name = "dgvRecords";
            this.dgvRecords.ReadOnly = true;
            this.dgvRecords.RowHeadersVisible = false;
            this.dgvRecords.RowTemplate.Height = 25;
            this.dgvRecords.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecords.Size = new System.Drawing.Size(760, 400);
            this.dgvRecords.TabIndex = 0;
            // 
            // btnReturn
            // 
            this.btnReturn.Location = new System.Drawing.Point(10, 420);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(120, 30);
            this.btnReturn.TabIndex = 1;
            this.btnReturn.Text = "归还选中图书";
            this.btnReturn.UseVisualStyleBackColor = true;
            this.btnReturn.Click += new System.EventHandler(this.btnReturn_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(650, 420);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler((s, e) => Close());
            // 
            // BorrowRecordsForm
            // 
            this.ClientSize = new System.Drawing.Size(780, 460);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnReturn);
            this.Controls.Add(this.dgvRecords);
            this.Name = "BorrowRecordsForm";
            this.Text = "借阅记录";
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecords)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridView dgvRecords;
        private System.Windows.Forms.Button btnReturn;
        private System.Windows.Forms.Button btnClose;
    }
}
