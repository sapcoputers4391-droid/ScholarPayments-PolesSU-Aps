namespace ScholarPayments_PolesSU_Aps
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridViewStudents = new DataGridView();
            btnAddStudent = new Button();
            label1 = new Label();
            btnDeleteStudent = new Button();
            btnRefresh = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridViewStudents).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewStudents
            // 
            dataGridViewStudents.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewStudents.Location = new Point(80, 169);
            dataGridViewStudents.Name = "dataGridViewStudents";
            dataGridViewStudents.Size = new Size(1336, 615);
            dataGridViewStudents.TabIndex = 0;
            dataGridViewStudents.CellContentClick += dataGridView1_CellContentClick;
            // 
            // btnAddStudent
            // 
            btnAddStudent.Location = new Point(80, 59);
            btnAddStudent.Name = "btnAddStudent";
            btnAddStudent.Size = new Size(286, 74);
            btnAddStudent.TabIndex = 1;
            btnAddStudent.Text = "Добавить";
            btnAddStudent.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(415, 345);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 2;
            label1.Text = "label1";
            label1.Click += label1_Click;
            // 
            // btnDeleteStudent
            // 
            btnDeleteStudent.Location = new Point(599, 59);
            btnDeleteStudent.Name = "btnDeleteStudent";
            btnDeleteStudent.Size = new Size(286, 74);
            btnDeleteStudent.TabIndex = 3;
            btnDeleteStudent.Text = "Удалить";
            btnDeleteStudent.UseVisualStyleBackColor = true;
            btnDeleteStudent.Click += btnDeleteStudent_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(1130, 59);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(286, 74);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "Обновить";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1822, 824);
            Controls.Add(btnRefresh);
            Controls.Add(btnDeleteStudent);
            Controls.Add(label1);
            Controls.Add(btnAddStudent);
            Controls.Add(dataGridViewStudents);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load_1;
            ((System.ComponentModel.ISupportInitialize)dataGridViewStudents).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridViewStudents;
        private Button btnAddStudent;
        private Label label1;
        private Button btnDeleteStudent;
        private Button btnRefresh;
    }
}
