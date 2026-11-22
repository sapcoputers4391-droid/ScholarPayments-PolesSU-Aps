
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using ScholarPayments_PolesSU_Apps;

namespace ScholarPayments_PolesSU_Aps
{
    public partial class Form1 : Form
    {
        private TextBox txtSearch;
        private Button btnSearch;

        public Form1()
        {
            InitializeComponent();

            btnRefresh.Click += btnRefresh_Click;
            btnAddStudent.Click += btnAddStudent_Click;

            CreateSearchControls();
            CreateMenu();
        }

        private void CreateSearchControls()
        {
            txtSearch = new TextBox();
            txtSearch.Location = new Point(80, 145);
            txtSearch.Size = new Size(250, 25);
            txtSearch.PlaceholderText = "Поиск студента...";

            btnSearch = new Button();
            btnSearch.Location = new Point(340, 140);
            btnSearch.Size = new Size(120, 30);
            btnSearch.Text = "Найти";

            btnSearch.Click += btnSearch_Click;

            dataGridViewStudents.Location = new Point(80, 180);
            dataGridViewStudents.Size = new Size(1336, 600);

            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
        }
        private void LoadStudents(string search = "")
        {
            try
            {
                using (SqlConnection connection =
                       DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query =
                        @"SELECT * FROM Students
                          WHERE FullName LIKE @search
                          OR StudentCardNumber LIKE @search
                          OR StudyGroup LIKE @search";

                    using (SqlCommand command =
                           new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@search", "%" + search + "%");

                        using (SqlDataAdapter adapter =
                               new SqlDataAdapter(command))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            dataGridViewStudents.DataSource = table;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }
        private bool ShowStudentDialog(
            DataRow student,
            out string card,
            out string fullName,
            out string group,
            out string studyForm,
            out decimal grade)
        {
            Form dialog = new Form();
            dialog.Text = student == null
                ? "Добавление студента"
                : "Редактирование студента";

            dialog.Size = new Size(400, 380);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;

            string[] labels =
            {
                "Номер билета:",
                "ФИО:",
                "Группа:",
                "Форма обучения:",
                "Средний балл:"
            };

            TextBox[] fields = new TextBox[5];

            for (int i = 0; i < labels.Length; i++)
            {
                Label label = new Label();
                label.Text = labels[i];
                label.Location = new Point(20, 25 + i * 50);
                label.Size = new Size(130, 25);

                TextBox field = new TextBox();
                field.Location = new Point(160, 22 + i * 50);
                field.Size = new Size(200, 25);

                fields[i] = field;

                dialog.Controls.Add(label);
                dialog.Controls.Add(field);
            }

            if (student != null)
            {
                fields[0].Text = student["StudentCardNumber"].ToString();
                fields[1].Text = student["FullName"].ToString();
                fields[2].Text = student["StudyGroup"].ToString();
                fields[3].Text = student["StudyForm"].ToString();
                fields[4].Text = student["AverageGrade"].ToString();
            }

            Button saveButton = new Button();
            saveButton.Text = "Сохранить";
            saveButton.Location = new Point(160, 285);
            saveButton.Size = new Size(120, 35);
            saveButton.DialogResult = DialogResult.OK;

            dialog.Controls.Add(saveButton);
            dialog.AcceptButton = saveButton;

            card = "";
            fullName = "";
            group = "";
            studyForm = "";
            grade = 0;

            if (dialog.ShowDialog() != DialogResult.OK)
                return false;

            card = fields[0].Text.Trim();
            fullName = fields[1].Text.Trim();
            group = fields[2].Text.Trim();
            studyForm = fields[3].Text.Trim();

            if (card == "" || fullName == "" ||
                group == "" || studyForm == "")
            {
                MessageBox.Show("Заполните все поля.");
                return false;
            }

            if (!decimal.TryParse(fields[4].Text, out grade)
                || grade < 0 || grade > 10)
            {
                MessageBox.Show("Средний балл должен быть от 0 до 10.");
                return false;
            }

            return true;
        }
        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            string card, fullName, group, studyForm;
            decimal grade;

            if (!ShowStudentDialog(
                null, out card, out fullName,
                out group, out studyForm, out grade))
                return;

            try
            {
                using (SqlConnection connection =
                       DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query =
                        @"INSERT INTO Students
                        (StudentCardNumber, FullName, StudyGroup,
                         StudyForm, AverageGrade)
                        VALUES
                        (@card, @name, @group, @form, @grade)";

                    using (SqlCommand command =
                           new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@card", card);
                        command.Parameters.AddWithValue("@name", fullName);
                        command.Parameters.AddWithValue("@group", group);
                        command.Parameters.AddWithValue("@form", studyForm);
                        command.Parameters.AddWithValue("@grade", grade);

                        command.ExecuteNonQuery();
                    }
                }

                LoadStudents();
                MessageBox.Show("Студент успешно добавлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message);
            }
        }
        private void dataGridView1_CellContentClick(
            object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridViewStudents_CellDoubleClick(
            object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataRow student =
                ((DataTable)dataGridViewStudents.DataSource)
                .Rows[e.RowIndex];

            string card, fullName, group, studyForm;
            decimal grade;

            if (!ShowStudentDialog(
                student, out card, out fullName,
                out group, out studyForm, out grade))
                return;

            try
            {
                int id = Convert.ToInt32(student["Id"]);

                using (SqlConnection connection =
                       DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query =
                        @"UPDATE Students SET
                        StudentCardNumber = @card,
                        FullName = @name,
                        StudyGroup = @group,
                        StudyForm = @form,
                        AverageGrade = @grade
                        WHERE Id = @id";

                    using (SqlCommand command =
                           new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@card", card);
                        command.Parameters.AddWithValue("@name", fullName);
                        command.Parameters.AddWithValue("@group", group);
                        command.Parameters.AddWithValue("@form", studyForm);
                        command.Parameters.AddWithValue("@grade", grade);
                        command.Parameters.AddWithValue("@id", id);

                        command.ExecuteNonQuery();
                    }
                }

                LoadStudents();
                MessageBox.Show("Данные студента изменены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка изменения: " + ex.Message);
            }
        }

        private void btnDeleteStudent_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.CurrentRow == null)
            {
                MessageBox.Show("Выберите студента.");
                return;
            }

            int id = Convert.ToInt32(
                dataGridViewStudents.CurrentRow.Cells["Id"].Value);

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить студента?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection connection =
                       DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string checkQuery =
                        "SELECT COUNT(*) FROM PaymentAssignments WHERE StudentId = @id";

                    using (SqlCommand checkCommand =
                           new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@id", id);

                        int count = Convert.ToInt32(
                            checkCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Нельзя удалить студента: у него есть назначенные выплаты.");
                            return;
                        }
                    }

                    string query =
                        "DELETE FROM Students WHERE Id = @id";

                    using (SqlCommand command =
                           new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }
                }

                LoadStudents();
                MessageBox.Show("Студент удалён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка удаления: " + ex.Message);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadStudents(txtSearch.Text.Trim());
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadStudents();
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            LoadStudents();

            dataGridViewStudents.CellDoubleClick +=
                dataGridViewStudents_CellDoubleClick;
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }



        private void CreateMenu()
        {
            MenuStrip menu = new MenuStrip();

            ToolStripMenuItem students =
                new ToolStripMenuItem("Студенты");

            ToolStripMenuItem payments =
                new ToolStripMenuItem("Выплаты");

            ToolStripMenuItem help =
                new ToolStripMenuItem("Справка");

            ToolStripMenuItem studentList =
                new ToolStripMenuItem("Список студентов");

            ToolStripMenuItem paymentTypes =
                new ToolStripMenuItem("Виды выплат");

            ToolStripMenuItem addPaymentType =
                new ToolStripMenuItem("Добавить вид выплаты");

            ToolStripMenuItem rules =
                new ToolStripMenuItem("Правила стипендий");

            ToolStripMenuItem assignments =
                new ToolStripMenuItem("Назначенные выплаты");

            studentList.Click += (s, e) => LoadStudents();

            paymentTypes.Click += (s, e) =>
                LoadTable("SELECT * FROM PaymentTypes");

            addPaymentType.Click += (s, e) => AddPaymentType();

            rules.Click += (s, e) =>
                LoadTable("SELECT * FROM ScholarshipRules");

            assignments.Click += (s, e) =>
                LoadTable("SELECT * FROM PaymentAssignments");

            help.Click += (s, e) =>
                MessageBox.Show(
                    "Система учёта стипендий и прочих выплат студентам ПолесГУ.",
                    "О программе");

            students.DropDownItems.Add(studentList);

            payments.DropDownItems.Add(paymentTypes);
            payments.DropDownItems.Add(addPaymentType);
            payments.DropDownItems.Add(rules);
            payments.DropDownItems.Add(assignments);

            menu.Items.Add(students);
            menu.Items.Add(payments);
            menu.Items.Add(help);

            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private void AddPaymentType()
        {
            Form dialog = new Form();
            dialog.Text = "Добавление вида выплаты";
            dialog.Size = new Size(430, 430);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;

            string[] labels =
            {
        "Название:",
        "Категория:",
        "Описание:",
        "Периодичность:",
        "Метод расчёта:"
    };

            TextBox[] fields = new TextBox[5];

            for (int i = 0; i < labels.Length; i++)
            {
                Label label = new Label();
                label.Text = labels[i];
                label.Location = new Point(20, 25 + i * 55);
                label.Size = new Size(130, 25);

                TextBox field = new TextBox();
                field.Location = new Point(160, 22 + i * 55);
                field.Size = new Size(230, 25);

                fields[i] = field;

                dialog.Controls.Add(label);
                dialog.Controls.Add(field);
            }

            CheckBox active = new CheckBox();
            active.Text = "Вид выплаты активен";
            active.Checked = true;
            active.Location = new Point(160, 300);

            Button save = new Button();
            save.Text = "Сохранить";
            save.Location = new Point(160, 335);
            save.Size = new Size(120, 35);
            save.DialogResult = DialogResult.OK;

            dialog.Controls.Add(active);
            dialog.Controls.Add(save);
            dialog.AcceptButton = save;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Text.Trim() == "")
                {
                    MessageBox.Show("Заполните все поля.");
                    return;
                }
            }

            try
            {
                using (SqlConnection connection =
                       DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query =
                        @"INSERT INTO PaymentTypes
                (Name, Category, Description, Periodicity,
                 CalculationMethod, IsActive)
                VALUES
                (@name, @category, @description, @periodicity,
                 @method, @active)";

                    using (SqlCommand command =
                           new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@name", fields[0].Text.Trim());

                        command.Parameters.AddWithValue(
                            "@category", fields[1].Text.Trim());

                        command.Parameters.AddWithValue(
                            "@description", fields[2].Text.Trim());

                        command.Parameters.AddWithValue(
                            "@periodicity", fields[3].Text.Trim());

                        command.Parameters.AddWithValue(
                            "@method", fields[4].Text.Trim());

                        command.Parameters.AddWithValue(
                            "@active", active.Checked);

                        command.ExecuteNonQuery();
                    }
                }

                LoadTable("SELECT * FROM PaymentTypes");

                MessageBox.Show("Вид выплаты успешно добавлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message);
            }
        }


        private void LoadTable(string query)
        {
            try
            {
                using (SqlConnection connection =
                       DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        dataGridViewStudents.DataSource = table;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

    }
}
