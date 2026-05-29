using BookManager.Models;

namespace BookManager.Forms
{
    public partial class MainForm : Form
    {
        private User _currentUser;

        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            lblUserInfo.Text = $"当前用户: {user.Name} ({user.Role})";

            if (user.Role != "admin")
            {
                btnBookManage.Visible = false;
                btnUserManage.Visible = false;
                btnOverdueManage.Visible = false;
            }

            LoadBooks();
        }

        private void LoadBooks()
        {
            try
            {
                var books = DbHelper.GetAllBooks();
                dgvBooks.DataSource = books.Select(b => new
                {
                    b.Id,
                    b.Isbn,
                    b.Title,
                    b.Author,
                    b.Publisher,
                    b.Price,
                    b.Quantity,
                    b.AvailableQuantity,
                    b.Category,
                    b.PublishDate
                }).ToList();

                dgvBooks.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载图书失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string keyword = txtKeyword.Text.Trim();
                string category = cboCategory.Text;
                string author = txtAuthor.Text.Trim();
                string publisher = txtPublisher.Text.Trim();

                var books = DbHelper.SearchBooks(keyword, 
                    string.IsNullOrEmpty(category) ? "" : category,
                    author, publisher);

                dgvBooks.DataSource = books.Select(b => new
                {
                    b.Id,
                    b.Isbn,
                    b.Title,
                    b.Author,
                    b.Publisher,
                    b.Price,
                    b.Quantity,
                    b.AvailableQuantity,
                    b.Category,
                    b.PublishDate
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBorrow_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要借阅的图书", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["Id"].Value);
                string title = dgvBooks.SelectedRows[0].Cells["Title"].Value.ToString() ?? "";
                int available = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["AvailableQuantity"].Value);

                if (available <= 0)
                {
                    MessageBox.Show("该图书已无库存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = DbHelper.BorrowBook(_currentUser.Id, bookId);

                if (success)
                {
                    MessageBox.Show($"成功借阅图书: {title}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBooks();
                }
                else
                {
                    MessageBox.Show("借阅失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("借阅失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMyBorrow_Click(object sender, EventArgs e)
        {
            BorrowRecordsForm form = new BorrowRecordsForm(_currentUser.Id, _currentUser.Role);
            form.ShowDialog();
        }

        private void btnBookManage_Click(object sender, EventArgs e)
        {
            BookManageForm form = new BookManageForm();
            form.ShowDialog();
            LoadBooks();
        }

        private void btnUserManage_Click(object sender, EventArgs e)
        {
            UserManageForm form = new UserManageForm();
            form.ShowDialog();
        }

        private void btnOverdueManage_Click(object sender, EventArgs e)
        {
            OverdueForm form = new OverdueForm();
            form.ShowDialog();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var books = DbHelper.GetAllBooks();
                
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "文本文件 (*.txt)|*.txt|XML文件 (*.xml)|*.xml";
                saveDialog.Title = "导出图书信息";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveDialog.FileName;
                    
                    if (filePath.EndsWith(".txt"))
                    {
                        using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("ISBN\t书名\t作者\t出版社\t价格\t数量\t可借数量\t分类\t出版日期");
                            foreach (var book in books)
                            {
                                sw.WriteLine($"{book.Isbn}\t{book.Title}\t{book.Author}\t{book.Publisher}\t{book.Price}\t{book.Quantity}\t{book.AvailableQuantity}\t{book.Category}\t{book.PublishDate:yyyy-MM-dd}");
                            }
                        }
                    }
                    else if (filePath.EndsWith(".xml"))
                    {
                        using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                            sw.WriteLine("<Books>");
                            foreach (var book in books)
                            {
                                sw.WriteLine($"  <Book>");
                                sw.WriteLine($"    <ISBN>{book.Isbn}</ISBN>");
                                sw.WriteLine($"    <Title>{book.Title}</Title>");
                                sw.WriteLine($"    <Author>{book.Author}</Author>");
                                sw.WriteLine($"    <Publisher>{book.Publisher}</Publisher>");
                                sw.WriteLine($"    <Price>{book.Price}</Price>");
                                sw.WriteLine($"    <Quantity>{book.Quantity}</Quantity>");
                                sw.WriteLine($"    <AvailableQuantity>{book.AvailableQuantity}</AvailableQuantity>");
                                sw.WriteLine($"    <Category>{book.Category}</Category>");
                                sw.WriteLine($"    <PublishDate>{book.PublishDate:yyyy-MM-dd}</PublishDate>");
                                sw.WriteLine($"  </Book>");
                            }
                            sw.WriteLine("</Books>");
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

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBookManage = new System.Windows.Forms.ToolStripMenuItem();
            this.btnUserManage = new System.Windows.Forms.ToolStripMenuItem();
            this.btnOverdueManage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.lblUserInfo = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtKeyword = new System.Windows.Forms.TextBox();
            this.cboCategory = new System.Windows.Forms.ComboBox();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.txtPublisher = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dgvBooks = new System.Windows.Forms.DataGridView();
            this.btnBorrow = new System.Windows.Forms.Button();
            this.btnMyBorrow = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBooks)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(900, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnBookManage,
            this.btnUserManage,
            this.btnOverdueManage});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(67, 21);
            this.toolStripMenuItem1.Text = "管理";
            // 
            // btnBookManage
            // 
            this.btnBookManage.Name = "btnBookManage";
            this.btnBookManage.Size = new System.Drawing.Size(152, 22);
            this.btnBookManage.Text = "图书管理";
            this.btnBookManage.Click += new System.EventHandler(this.btnBookManage_Click);
            // 
            // btnUserManage
            // 
            this.btnUserManage.Name = "btnUserManage";
            this.btnUserManage.Size = new System.Drawing.Size(152, 22);
            this.btnUserManage.Text = "用户管理";
            this.btnUserManage.Click += new System.EventHandler(this.btnUserManage_Click);
            // 
            // btnOverdueManage
            // 
            this.btnOverdueManage.Name = "btnOverdueManage";
            this.btnOverdueManage.Size = new System.Drawing.Size(152, 22);
            this.btnOverdueManage.Text = "逾期管理";
            this.btnOverdueManage.Click += new System.EventHandler(this.btnOverdueManage_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnExport});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(55, 21);
            this.toolStripMenuItem2.Text = "导出";
            // 
            // btnExport
            // 
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(112, 22);
            this.btnExport.Text = "导出图书";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnLogout});
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(55, 21);
            this.toolStripMenuItem3.Text = "系统";
            // 
            // btnLogout
            // 
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(112, 22);
            this.btnLogout.Text = "退出登录";
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // lblUserInfo
            // 
            this.lblUserInfo.Location = new System.Drawing.Point(750, 28);
            this.lblUserInfo.Name = "lblUserInfo";
            this.lblUserInfo.Size = new System.Drawing.Size(140, 20);
            this.lblUserInfo.TabIndex = 1;
            this.lblUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtKeyword);
            this.panel1.Controls.Add(this.cboCategory);
            this.panel1.Controls.Add(this.txtAuthor);
            this.panel1.Controls.Add(this.txtPublisher);
            this.panel1.Controls.Add(this.btnSearch);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Location = new System.Drawing.Point(10, 55);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(880, 60);
            this.panel1.TabIndex = 2;
            // 
            // txtKeyword
            // 
            this.txtKeyword.Location = new System.Drawing.Point(70, 10);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.PlaceholderText = "书名或ISBN";
            this.txtKeyword.Size = new System.Drawing.Size(150, 23);
            this.txtKeyword.TabIndex = 0;
            // 
            // cboCategory
            // 
            this.cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCategory.FormattingEnabled = true;
            this.cboCategory.Items.AddRange(new object[] {
            "",
            "数学",
            "计算机",
            "经济",
            "文学"});
            this.cboCategory.Location = new System.Drawing.Point(290, 10);
            this.cboCategory.Name = "cboCategory";
            this.cboCategory.Size = new System.Drawing.Size(100, 23);
            this.cboCategory.TabIndex = 1;
            // 
            // txtAuthor
            // 
            this.txtAuthor.Location = new System.Drawing.Point(450, 10);
            this.txtAuthor.Name = "txtAuthor";
            this.txtAuthor.PlaceholderText = "作者";
            this.txtAuthor.Size = new System.Drawing.Size(120, 23);
            this.txtAuthor.TabIndex = 2;
            // 
            // txtPublisher
            // 
            this.txtPublisher.Location = new System.Drawing.Point(630, 10);
            this.txtPublisher.Name = "txtPublisher";
            this.txtPublisher.PlaceholderText = "出版社";
            this.txtPublisher.Size = new System.Drawing.Size(120, 23);
            this.txtPublisher.TabIndex = 3;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(770, 8);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(90, 25);
            this.btnSearch.TabIndex = 4;
            this.btnSearch.Text = "查询";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "书名/ISBN";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(240, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "分类";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(410, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "作者";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(580, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "出版社";
            // 
            // dgvBooks
            // 
            this.dgvBooks.AllowUserToAddRows = false;
            this.dgvBooks.AllowUserToDeleteRows = false;
            this.dgvBooks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBooks.Location = new System.Drawing.Point(10, 125);
            this.dgvBooks.Name = "dgvBooks";
            this.dgvBooks.ReadOnly = true;
            this.dgvBooks.RowHeadersVisible = false;
            this.dgvBooks.RowTemplate.Height = 25;
            this.dgvBooks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBooks.Size = new System.Drawing.Size(880, 350);
            this.dgvBooks.TabIndex = 3;
            // 
            // btnBorrow
            // 
            this.btnBorrow.Location = new System.Drawing.Point(10, 485);
            this.btnBorrow.Name = "btnBorrow";
            this.btnBorrow.Size = new System.Drawing.Size(120, 30);
            this.btnBorrow.TabIndex = 4;
            this.btnBorrow.Text = "借阅选中图书";
            this.btnBorrow.UseVisualStyleBackColor = true;
            this.btnBorrow.Click += new System.EventHandler(this.btnBorrow_Click);
            // 
            // btnMyBorrow
            // 
            this.btnMyBorrow.Location = new System.Drawing.Point(140, 485);
            this.btnMyBorrow.Name = "btnMyBorrow";
            this.btnMyBorrow.Size = new System.Drawing.Size(120, 30);
            this.btnMyBorrow.TabIndex = 5;
            this.btnMyBorrow.Text = "我的借阅记录";
            this.btnMyBorrow.UseVisualStyleBackColor = true;
            this.btnMyBorrow.Click += new System.EventHandler(this.btnMyBorrow_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(900, 530);
            this.Controls.Add(this.btnMyBorrow);
            this.Controls.Add(this.btnBorrow);
            this.Controls.Add(this.dgvBooks);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblUserInfo);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "图书管理系统";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBooks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem btnBookManage;
        private System.Windows.Forms.ToolStripMenuItem btnUserManage;
        private System.Windows.Forms.ToolStripMenuItem btnOverdueManage;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem btnExport;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem btnLogout;
        private System.Windows.Forms.Label lblUserInfo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.TextBox txtAuthor;
        private System.Windows.Forms.TextBox txtPublisher;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dgvBooks;
        private System.Windows.Forms.Button btnBorrow;
        private System.Windows.Forms.Button btnMyBorrow;
    }
}
