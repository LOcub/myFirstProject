using BookManager.Models;

namespace BookManager.Forms
{
    public partial class OverdueForm : Form
    {
        public OverdueForm()
        {
            InitializeComponent();
            LoadOverdueRecords();
        }

        private void LoadOverdueRecords()
        {
            try
            {
                var records = DbHelper.GetOverdueRecords();
                dgvOverdue.DataSource = records.Select(r => new
                {
                    r.Id,
                    r.UserName,
                    r.BookTitle,
                    r.BorrowDate,
                    r.DueDate,
                    DaysOverdue = (DateTime.Now - r.DueDate).Days
                }).ToList();

                dgvOverdue.Columns["Id"].Visible = false;
                lblCount.Text = $"逾期图书数量: {records.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载逾期记录失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadOverdueRecords();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var records = DbHelper.GetOverdueRecords();

                if (records.Count == 0)
                {
                    MessageBox.Show("没有逾期记录可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "文本文件 (*.txt)|*.txt|XML文件 (*.xml)|*.xml";
                saveDialog.Title = "导出逾期记录";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveDialog.FileName;

                    if (filePath.EndsWith(".txt"))
                    {
                        using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("用户名\t书名\t借阅日期\t到期日期\t逾期天数");
                            foreach (var record in records)
                            {
                                int daysOverdue = (DateTime.Now - record.DueDate).Days;
                                sw.WriteLine($"{record.UserName}\t{record.BookTitle}\t{record.BorrowDate:yyyy-MM-dd}\t{record.DueDate:yyyy-MM-dd}\t{daysOverdue}");
                            }
                        }
                    }
                    else if (filePath.EndsWith(".xml"))
                    {
                        using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                            sw.WriteLine("<OverdueRecords>");
                            foreach (var record in records)
                            {
                                int daysOverdue = (DateTime.Now - record.DueDate).Days;
                                sw.WriteLine($"  <Record>");
                                sw.WriteLine($"    <UserName>{record.UserName}</UserName>");
                                sw.WriteLine($"    <BookTitle>{record.BookTitle}</BookTitle>");
                                sw.WriteLine($"    <BorrowDate>{record.BorrowDate:yyyy-MM-dd}</BorrowDate>");
                                sw.WriteLine($"    <DueDate>{record.DueDate:yyyy-MM-dd}</DueDate>");
                                sw.WriteLine($"    <DaysOverdue>{daysOverdue}</DaysOverdue>");
                                sw.WriteLine($"  </Record>");
                            }
                            sw.WriteLine("</OverdueRecords>");
                        }
                    }

                    MessageBox.Show("导出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.dgvOverdue = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOverdue)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvOverdue
            // 
            this.dgvOverdue.AllowUserToAddRows = false;
            this.dgvOverdue.AllowUserToDeleteRows = false;
            this.dgvOverdue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOverdue.Location = new System.Drawing.Point(10, 40);
            this.dgvOverdue.Name = "dgvOverdue";
            this.dgvOverdue.ReadOnly = true;
            this.dgvOverdue.RowHeadersVisible = false;
            this.dgvOverdue.RowTemplate.Height = 25;
            this.dgvOverdue.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOverdue.Size = new System.Drawing.Size(700, 350);
            this.dgvOverdue.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 400);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 30);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(120, 400);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(100, 30);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "导出记录";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(610, 400);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler((s, e) => Close());
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCount.Location = new System.Drawing.Point(10, 15);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(100, 14);
            this.lblCount.TabIndex = 4;
            this.lblCount.Text = "逾期图书数量: 0";
            // 
            // OverdueForm
            // 
            this.ClientSize = new System.Drawing.Size(720, 440);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.dgvOverdue);
            this.Name = "OverdueForm";
            this.Text = "逾期图书管理";
            ((System.ComponentModel.ISupportInitialize)(this.dgvOverdue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvOverdue;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblCount;
    }
}
