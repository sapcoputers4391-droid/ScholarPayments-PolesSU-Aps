using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using ScholarPayments_PolesSU_Apps;
using ScholarPayments_PolesSU_Aps.UI;

namespace ScholarPayments_PolesSU_Aps
{
    public partial class Form1 : Form
    {
        private readonly ScholarshipCalculator _calculator = new();

        private string _section = "students";

        private TextBox txtSearch = null!;
        private Button btnSearch = null!;

        // ------------------------------------------------------------------
        // Вспомогательные типы для выпадающих списков и диалогов
        // ------------------------------------------------------------------
        private sealed class Choice
        {
            public Choice(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public string Text { get; }
            public string Value { get; }

            public override string ToString() => Text;
        }

        private sealed class StudentOption
        {
            public int Id { get; init; }
            public string FullName { get; init; } = string.Empty;
            public string Speciality { get; init; } = string.Empty;
            public decimal AverageGrade { get; init; }

            public override string ToString() => FullName;
        }

        private sealed class PaymentTypeOption
        {
            public int Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Periodicity { get; init; } = string.Empty;
            public decimal Amount { get; init; }

            public override string ToString() => $"{Name} — {Amount:N2} руб.";
        }

        private sealed class StudentInput
        {
            public string Card { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Faculty { get; set; } = string.Empty;
            public string Speciality { get; set; } = string.Empty;
            public string StudyGroup { get; set; } = string.Empty;
            public int Course { get; set; } = 1;
            public string StudyForm { get; set; } = "Budget";
            public string FundingType { get; set; } = string.Empty;
            public decimal AverageGrade { get; set; }
        }

        private static readonly Choice[] StudyForms =
        {
            new("Бюджетная", "Budget"),
            new("Платная", "Paid")
        };

        private static readonly Choice[] Categories =
        {
            new("Стипендия", "Scholarship"),
            new("Материальная помощь", "MaterialAid"),
            new("Прочая выплата", "Other")
        };

        private static readonly Choice[] Periodicities =
        {
            new("Ежемесячно", "Monthly"),
            new("Разово", "Once")
        };

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
            txtSearch.PlaceholderText = "Поиск студента...";

            btnSearch = new Button();
            btnSearch.Text = "Найти";
            btnSearch.Click += btnSearch_Click;
        }

        // ============================================================
        // Загрузка разделов
        // ============================================================
        private void LoadStudents(string search = "")
        {
            SetSection("students");

            const string query =
                @"SELECT * FROM Students
                  WHERE FullName LIKE @search
                     OR StudentCardNumber LIKE @search
                     OR StudyGroup LIKE @search
                     OR Faculty LIKE @search
                     OR Speciality LIKE @search
                  ORDER BY FullName";

            LoadData(query, search);
        }

        private void LoadPaymentTypes(string search = "")
        {
            SetSection("types");

            const string query =
                @"SELECT * FROM PaymentTypes
                  WHERE Name LIKE @search
                     OR Description LIKE @search
                     OR FundingSource LIKE @search
                  ORDER BY Name";

            LoadData(query, search);
        }

        private void LoadRules(string search = "")
        {
            SetSection("rules");

            const string query =
                @"SELECT * FROM ScholarshipRules
                  WHERE Speciality LIKE @search
                  ORDER BY Speciality, MinAverageGrade";

            LoadData(query, search);
        }

        private void LoadAssignments(string search = "")
        {
            SetSection("assignments");

            const string query =
                @"SELECT a.Id,
                         s.FullName,
                         pt.Name AS PaymentTypeName,
                         a.DocumentNumber,
                         a.DocumentDate,
                         a.PeriodStart,
                         a.PeriodEnd,
                         a.AverageGrade,
                         a.BaseAmount,
                         a.Coefficient,
                         a.Amount,
                         a.AssignmentDate,
                         a.PaymentDate,
                         a.Status,
                         a.Reason
                  FROM dbo.PaymentAssignments a
                  JOIN dbo.Students s ON s.Id = a.StudentId
                  JOIN dbo.PaymentTypes pt ON pt.Id = a.PaymentTypeId
                  WHERE s.FullName LIKE @search
                     OR pt.Name LIKE @search
                     OR ISNULL(a.DocumentNumber, N'') LIKE @search
                  ORDER BY a.Id DESC";

            LoadData(query, search);
        }

        private void LoadData(string query, string search)
        {
            try
            {
                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@search", SqlDbType.NVarChar, 150).Value =
                    "%" + search + "%";

                using SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridViewStudents.DataSource = table;
            }
            catch (Exception ex)
            {
                ShowError("Ошибка загрузки данных", ex);
            }
        }

        private void SetSection(string section)
        {
            _section = section;

            txtSearch.PlaceholderText = section switch
            {
                "types" => "Поиск по названию или источнику...",
                "rules" => "Поиск по специальности...",
                "assignments" => "ФИО, вид выплаты или документ...",
                _ => "Поиск студента..."
            };

            if (_sectionTitle != null)
            {
                _sectionTitle.Text = section switch
                {
                    "types" => "Виды выплат",
                    "rules" => "Правила стипендий",
                    "assignments" => "Назначенные выплаты",
                    _ => "Студенты"
                };
            }

            UpdateSectionActions(section);
        }

        private void ReloadCurrentSection()
        {
            switch (_section)
            {
                case "types":
                    LoadPaymentTypes();
                    break;
                case "rules":
                    LoadRules();
                    break;
                case "assignments":
                    LoadAssignments();
                    break;
                default:
                    LoadStudents();
                    break;
            }
        }

        // ============================================================
        // Студенты
        // ============================================================
        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            if (_section == "types")
            {
                AddPaymentType();
                return;
            }

            if (_section != "students")
                return;

            StudentInput? input = ShowStudentDialog(null);
            if (input == null)
                return;

            try
            {
                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                const string query =
                    @"INSERT INTO Students
                        (StudentCardNumber, FullName, Faculty, Speciality,
                         StudyGroup, Course, StudyForm, FundingType, AverageGrade)
                      VALUES
                        (@card, @name, @faculty, @speciality,
                         @group, @course, @form, @funding, @grade)";

                using SqlCommand command = new SqlCommand(query, connection);
                AddStudentParameters(command, input);
                command.ExecuteNonQuery();

                LoadStudents();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка добавления студента", ex);
            }
        }

        private void dataGridViewStudents_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _section != "students")
                return;

            if (dataGridViewStudents.DataSource is not DataTable table)
                return;

            if (e.RowIndex >= table.Rows.Count)
                return;

            DataRow student = table.Rows[e.RowIndex];
            StudentInput? input = ShowStudentDialog(student);
            if (input == null)
                return;

            try
            {
                int id = Convert.ToInt32(student["Id"]);

                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                const string query =
                    @"UPDATE Students SET
                        StudentCardNumber = @card,
                        FullName = @name,
                        Faculty = @faculty,
                        Speciality = @speciality,
                        StudyGroup = @group,
                        Course = @course,
                        StudyForm = @form,
                        FundingType = @funding,
                        AverageGrade = @grade
                      WHERE Id = @id";

                using SqlCommand command = new SqlCommand(query, connection);
                AddStudentParameters(command, input);
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                command.ExecuteNonQuery();

                LoadStudents();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка изменения студента", ex);
            }
        }

        private static void AddStudentParameters(SqlCommand command, StudentInput input)
        {
            command.Parameters.Add("@card", SqlDbType.NVarChar, 30).Value = input.Card;
            command.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = input.FullName;
            command.Parameters.Add("@faculty", SqlDbType.NVarChar, 100).Value = input.Faculty;
            command.Parameters.Add("@speciality", SqlDbType.NVarChar, 100).Value = input.Speciality;
            command.Parameters.Add("@group", SqlDbType.NVarChar, 30).Value = input.StudyGroup;
            command.Parameters.Add("@course", SqlDbType.Int).Value = input.Course;
            command.Parameters.Add("@form", SqlDbType.NVarChar, 30).Value = input.StudyForm;
            command.Parameters.Add("@funding", SqlDbType.NVarChar, 60).Value = input.FundingType;

            SqlParameter grade = command.Parameters.Add("@grade", SqlDbType.Decimal);
            grade.Precision = 4;
            grade.Scale = 2;
            grade.Value = input.AverageGrade;
        }

        private StudentInput? ShowStudentDialog(DataRow? student)
        {
            using Form dialog = new Form();
            dialog.Text = "Стипендии и выплаты — ПолесГУ";
            string dialogTitle = student == null
                ? "Добавление студента"
                : "Редактирование студента";

            dialog.ClientSize = new Size(640, 610);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.BackColor = UiTheme.Background;
            dialog.Font = UiTheme.BaseFont;

            TableLayoutPanel formGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                Padding = new Padding(24, 14, 24, 4)
            };
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            TextBox card = new TextBox();
            TextBox fullName = new TextBox();
            TextBox faculty = new TextBox();
            TextBox speciality = new TextBox();
            TextBox group = new TextBox();

            ComboBox course = new ComboBox();
            UiTheme.StyleCombo(course);
            for (int i = 1; i <= 6; i++)
                course.Items.Add(i);
            course.SelectedIndex = 0;

            ComboBox studyForm = new ComboBox();
            UiTheme.StyleCombo(studyForm);
            studyForm.Items.AddRange(StudyForms);
            studyForm.SelectedIndex = 0;

            TextBox fundingType = new TextBox();
            TextBox grade = new TextBox();

            Control[] fields =
            {
                card, fullName, faculty, speciality, group,
                course, studyForm, fundingType, grade
            };

            string[] labels =
            {
                "Номер билета:", "ФИО:", "Факультет:", "Специальность:",
                "Группа:", "Курс:", "Форма обучения:", "Финансирование:",
                "Средний балл:"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
                formGrid.Controls.Add(UiTheme.FieldLabel(labels[i]), 0, i);

                Control field = fields[i];
                if (field is TextBox textBox)
                {
                    UiTheme.StyleField(textBox);
                }
                else
                {
                    field.Dock = DockStyle.Fill;
                }

                formGrid.Controls.Add(field, 1, i);
            }

            if (student != null)
            {
                card.Text = student["StudentCardNumber"].ToString();
                fullName.Text = student["FullName"].ToString();
                faculty.Text = student["Faculty"].ToString();
                speciality.Text = student["Speciality"].ToString();
                group.Text = student["StudyGroup"].ToString();

                int courseNumber = Convert.ToInt32(student["Course"]);
                course.SelectedIndex = Math.Clamp(courseNumber - 1, 0, 5);

                string studyFormCode = student["StudyForm"].ToString() ?? "";
                int formIndex = Array.FindIndex(StudyForms, c => c.Value == studyFormCode);
                studyForm.SelectedIndex = formIndex >= 0 ? formIndex : 0;

                fundingType.Text = student["FundingType"].ToString();
                grade.Text = Convert.ToDecimal(student["AverageGrade"]).ToString("0.00");
            }

            dialog.Controls.Add(formGrid);
            dialog.Controls.Add(CreateDialogFooter(dialog, "Сохранить"));
            dialog.Controls.Add(CreateDialogHeader(dialogTitle));

            if (dialog.ShowDialog() != DialogResult.OK)
                return null;

            if (card.Text.Trim() == "" || fullName.Text.Trim() == "" ||
                group.Text.Trim() == "")
            {
                MessageBox.Show("Заполните номер билета, ФИО и группу.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if (!decimal.TryParse(grade.Text, out decimal averageGrade) ||
                averageGrade < 0 || averageGrade > 10)
            {
                MessageBox.Show("Средний балл должен быть от 0 до 10.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return new StudentInput
            {
                Card = card.Text.Trim(),
                FullName = fullName.Text.Trim(),
                Faculty = faculty.Text.Trim(),
                Speciality = speciality.Text.Trim(),
                StudyGroup = group.Text.Trim(),
                Course = (int)course.SelectedItem!,
                StudyForm = ((Choice)studyForm.SelectedItem!).Value,
                FundingType = fundingType.Text.Trim(),
                AverageGrade = averageGrade
            };
        }

        private void btnDeleteStudent_Click(object sender, EventArgs e)
        {
            if (_section != "students")
                return;

            if (!TryGetSelectedAssignmentId(out int id))
            {
                MessageBox.Show("Выберите студента.",
                    "Удаление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Вы действительно хотите удалить студента?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                using (SqlCommand checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM PaymentAssignments WHERE StudentId = @id",
                    connection))
                {
                    checkCommand.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show(
                            "Нельзя удалить студента: у него есть назначенные выплаты.",
                            "Удаление", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                using (SqlCommand command = new SqlCommand(
                    "DELETE FROM Students WHERE Id = @id", connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    command.ExecuteNonQuery();
                }

                LoadStudents();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка удаления студента", ex);
            }
        }

        // ============================================================
        // Виды выплат
        // ============================================================
        private void AddPaymentType()
        {
            using Form dialog = new Form();
            dialog.Text = "Стипендии и выплаты — ПолесГУ";
            dialog.ClientSize = new Size(620, 545);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.BackColor = UiTheme.Background;
            dialog.Font = UiTheme.BaseFont;

            TableLayoutPanel formGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(24, 14, 24, 4)
            };
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            TextBox name = new TextBox();

            ComboBox category = new ComboBox();
            UiTheme.StyleCombo(category);
            category.Items.AddRange(Categories);
            category.SelectedIndex = 0;

            ComboBox periodicity = new ComboBox();
            UiTheme.StyleCombo(periodicity);
            periodicity.Items.AddRange(Periodicities);
            periodicity.SelectedIndex = 0;

            TextBox fundingSource = new TextBox();
            TextBox amount = new TextBox();
            TextBox description = new TextBox();

            Control[] fields =
            {
                name, category, periodicity, fundingSource, amount, description
            };

            string[] labels =
            {
                "Название:", "Категория:", "Периодичность:",
                "Источник:", "Сумма:", "Описание:"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
                formGrid.Controls.Add(UiTheme.FieldLabel(labels[i]), 0, i);

                Control field = fields[i];
                if (field is TextBox textBox)
                {
                    UiTheme.StyleField(textBox);
                }
                else
                {
                    field.Dock = DockStyle.Fill;
                }

                formGrid.Controls.Add(field, 1, i);
            }

            formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

            CheckBox isActive = new CheckBox
            {
                Text = "Вид выплаты активен",
                Checked = true,
                AutoSize = true,
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextColor,
                Margin = new Padding(0, 18, 0, 0)
            };
            formGrid.Controls.Add(isActive, 1, 6);

            dialog.Controls.Add(formGrid);
            dialog.Controls.Add(CreateDialogFooter(dialog, "Сохранить"));
            dialog.Controls.Add(CreateDialogHeader("Добавление вида выплаты"));

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (name.Text.Trim() == "")
            {
                MessageBox.Show("Укажите название вида выплаты.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(amount.Text, out decimal sum) || sum < 0)
            {
                MessageBox.Show("Сумма должна быть неотрицательным числом.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                const string query =
                    @"INSERT INTO PaymentTypes
                        (Name, Category, Periodicity, FundingSource, Amount, Description, IsActive)
                      VALUES
                        (@name, @category, @periodicity, @funding, @amount, @description, @active)";

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name.Text.Trim();
                command.Parameters.Add("@category", SqlDbType.NVarChar, 30).Value =
                    ((Choice)category.SelectedItem!).Value;
                command.Parameters.Add("@periodicity", SqlDbType.NVarChar, 30).Value =
                    ((Choice)periodicity.SelectedItem!).Value;
                command.Parameters.Add("@funding", SqlDbType.NVarChar, 100).Value =
                    fundingSource.Text.Trim();
                SqlParameter amountParameter = command.Parameters.Add("@amount", SqlDbType.Decimal);
                amountParameter.Precision = 10;
                amountParameter.Scale = 2;
                amountParameter.Value = sum;
                command.Parameters.Add("@description", SqlDbType.NVarChar, 300).Value =
                    description.Text.Trim() == ""
                        ? DBNull.Value
                        : description.Text.Trim();
                command.Parameters.Add("@active", SqlDbType.Bit).Value = isActive.Checked;

                command.ExecuteNonQuery();

                LoadPaymentTypes();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка добавления вида выплаты", ex);
            }
        }

        // ============================================================
        // Назначенные выплаты
        // ============================================================
        private void ShowAssignPaymentDialog()
        {
            List<StudentOption> students;
            List<PaymentTypeOption> paymentTypes;

            try
            {
                students = LoadStudentOptions();
                paymentTypes = LoadPaymentTypeOptions();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка загрузки справочников", ex);
                return;
            }

            if (students.Count == 0)
            {
                MessageBox.Show("Сначала добавьте хотя бы одного студента.",
                    "Назначение выплаты", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (paymentTypes.Count == 0)
            {
                MessageBox.Show("Сначала добавьте хотя бы один вид выплаты.",
                    "Назначение выплаты", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using Form dialog = new Form();
            dialog.Text = "Стипендии и выплаты — ПолесГУ";
            dialog.ClientSize = new Size(640, 620);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.BackColor = UiTheme.Background;
            dialog.Font = UiTheme.BaseFont;

            TableLayoutPanel formGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                Padding = new Padding(24, 14, 24, 4)
            };
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            ComboBox studentBox = new ComboBox();
            UiTheme.StyleCombo(studentBox);
            studentBox.Items.AddRange(students.ToArray());
            studentBox.SelectedIndex = 0;

            ComboBox typeBox = new ComboBox();
            UiTheme.StyleCombo(typeBox);
            typeBox.Items.AddRange(paymentTypes.ToArray());
            typeBox.SelectedIndex = 0;

            DateTimePicker periodStart = new DateTimePicker();
            UiTheme.StyleDate(periodStart);
            periodStart.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            DateTimePicker periodEnd = new DateTimePicker();
            UiTheme.StyleDate(periodEnd);
            periodEnd.Value = periodStart.Value.AddMonths(1).AddDays(-1);

            TextBox documentNumber = new TextBox();

            DateTimePicker documentDate = new DateTimePicker();
            UiTheme.StyleDate(documentDate);

            TextBox grade = new TextBox();
            grade.Text = students[0].AverageGrade.ToString("0.00");

            TextBox coefficient = new TextBox { ReadOnly = true };
            TextBox amount = new TextBox { ReadOnly = true, Font = UiTheme.BoldFont };

            string[] labels =
            {
                "Студент:", "Вид выплаты:", "Период с:", "Период по:",
                "Документ №:", "Дата документа:", "Средний балл:",
                "Коэффициент:", "Сумма к выплате:"
            };

            Control[] fields =
            {
                studentBox, typeBox, periodStart, periodEnd,
                documentNumber, documentDate, grade, coefficient, amount
            };

            for (int i = 0; i < labels.Length; i++)
            {
                formGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
                formGrid.Controls.Add(UiTheme.FieldLabel(labels[i]), 0, i);

                Control field = fields[i];
                if (field is TextBox textBox)
                {
                    UiTheme.StyleField(textBox);
                }
                else
                {
                    field.Dock = DockStyle.Fill;
                }

                formGrid.Controls.Add(field, 1, i);
            }

            amount.Font = UiTheme.BoldFont;
            amount.ForeColor = UiTheme.Primary;

            dialog.Controls.Add(formGrid);
            dialog.Controls.Add(CreateDialogFooter(dialog, "Назначить"));
            dialog.Controls.Add(CreateDialogHeader("Назначение выплаты"));

            void Recalculate()
            {
                coefficient.Text = "";
                amount.Text = "";

                if (studentBox.SelectedItem is not StudentOption student ||
                    typeBox.SelectedItem is not PaymentTypeOption type)
                    return;

                if (!decimal.TryParse(grade.Text, out decimal averageGrade))
                    return;

                if (TryCalculateAmount(
                        student, type, averageGrade,
                        periodStart.Value, periodEnd.Value,
                        out decimal coef, out decimal total, out _))
                {
                    coefficient.Text = coef.ToString("N3");
                    amount.Text = total.ToString("N2") + " руб.";
                }
                else
                {
                    amount.Text = "Правило не найдено";
                }
            }

            studentBox.SelectedIndexChanged += (s, e) =>
            {
                if (studentBox.SelectedItem is StudentOption student)
                    grade.Text = student.AverageGrade.ToString("0.00");
                Recalculate();
            };
            typeBox.SelectedIndexChanged += (s, e) => Recalculate();
            periodStart.ValueChanged += (s, e) => Recalculate();
            periodEnd.ValueChanged += (s, e) => Recalculate();
            grade.Leave += (s, e) => Recalculate();

            Recalculate();

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            if (studentBox.SelectedItem is not StudentOption selectedStudent ||
                typeBox.SelectedItem is not PaymentTypeOption selectedType)
            {
                MessageBox.Show("Выберите студента и вид выплаты.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(grade.Text, out decimal selectedGrade) ||
                selectedGrade < 0 || selectedGrade > 10)
            {
                MessageBox.Show("Средний балл должен быть от 0 до 10.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (periodEnd.Value.Date < periodStart.Value.Date)
            {
                MessageBox.Show("Дата окончания периода раньше даты начала.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryCalculateAmount(
                    selectedStudent, selectedType, selectedGrade,
                    periodStart.Value, periodEnd.Value,
                    out decimal finalCoefficient, out decimal finalAmount, out string error))
            {
                MessageBox.Show(error, "Расчёт выплаты",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                const string query =
                    @"INSERT INTO PaymentAssignments
                        (StudentId, PaymentTypeId, DocumentNumber, DocumentDate,
                         PeriodStart, PeriodEnd, AverageGrade, BaseAmount,
                         Coefficient, Amount, AssignmentDate, PaymentDate, Status, Reason)
                      VALUES
                        (@studentId, @typeId, @documentNumber, @documentDate,
                         @periodStart, @periodEnd, @grade, @baseAmount,
                         @coefficient, @amount, @assignmentDate, NULL, @status, NULL)";

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@studentId", SqlDbType.Int).Value = selectedStudent.Id;
                command.Parameters.Add("@typeId", SqlDbType.Int).Value = selectedType.Id;
                command.Parameters.Add("@documentNumber", SqlDbType.NVarChar, 50).Value =
                    documentNumber.Text.Trim() == ""
                        ? DBNull.Value
                        : documentNumber.Text.Trim();
                command.Parameters.Add("@documentDate", SqlDbType.Date).Value =
                    documentDate.Value.Date;
                command.Parameters.Add("@periodStart", SqlDbType.Date).Value =
                    periodStart.Value.Date;
                command.Parameters.Add("@periodEnd", SqlDbType.Date).Value =
                    periodEnd.Value.Date;
                SqlParameter gradeParameter = command.Parameters.Add("@grade", SqlDbType.Decimal);
                gradeParameter.Precision = 4;
                gradeParameter.Scale = 2;
                gradeParameter.Value = selectedGrade;

                SqlParameter baseAmountParameter =
                    command.Parameters.Add("@baseAmount", SqlDbType.Decimal);
                baseAmountParameter.Precision = 10;
                baseAmountParameter.Scale = 2;
                baseAmountParameter.Value = selectedType.Amount;

                SqlParameter coefficientParameter =
                    command.Parameters.Add("@coefficient", SqlDbType.Decimal);
                coefficientParameter.Precision = 6;
                coefficientParameter.Scale = 3;
                coefficientParameter.Value = finalCoefficient;

                SqlParameter amountParameter =
                    command.Parameters.Add("@amount", SqlDbType.Decimal);
                amountParameter.Precision = 12;
                amountParameter.Scale = 2;
                amountParameter.Value = finalAmount;
                command.Parameters.Add("@assignmentDate", SqlDbType.Date).Value =
                    DateTime.Today;
                command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = "Assigned";

                command.ExecuteNonQuery();

                LoadAssignments();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка назначения выплаты", ex);
            }
        }

        private void MarkSelectedPaymentPaid()
        {
            if (!TryGetSelectedAssignmentId(out int id))
            {
                MessageBox.Show("Выберите назначенную выплату.",
                    "Выплата", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dataGridViewStudents.CurrentRow?.Cells["Status"].Value?.ToString() == "Cancelled")
            {
                MessageBox.Show("Нельзя выплатить отменённую выплату.",
                    "Выплата", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Отметить выплату как выплаченную?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                const string query =
                    @"UPDATE PaymentAssignments
                      SET Status = N'Paid', PaymentDate = @date
                      WHERE Id = @id";

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Today;
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                command.ExecuteNonQuery();

                LoadAssignments();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка изменения статуса", ex);
            }
        }

        private void CancelSelectedPayment()
        {
            if (!TryGetSelectedAssignmentId(out int id))
            {
                MessageBox.Show("Выберите назначенную выплату.",
                    "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dataGridViewStudents.CurrentRow?.Cells["Status"].Value?.ToString() == "Paid")
            {
                MessageBox.Show("Нельзя отменить уже выплаченную выплату.",
                    "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Отменить назначенную выплату?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using SqlConnection connection = DatabaseConnection.GetConnection();
                connection.Open();

                const string query =
                    @"UPDATE PaymentAssignments
                      SET Status = N'Cancelled'
                      WHERE Id = @id";

                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                command.ExecuteNonQuery();

                LoadAssignments();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка отмены выплаты", ex);
            }
        }

        private List<StudentOption> LoadStudentOptions()
        {
            var students = new List<StudentOption>();

            using SqlConnection connection = DatabaseConnection.GetConnection();
            connection.Open();

            using SqlCommand command = new SqlCommand(
                "SELECT Id, FullName, Speciality, AverageGrade FROM Students ORDER BY FullName",
                connection);
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new StudentOption
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Speciality = reader.GetString(2),
                    AverageGrade = reader.GetDecimal(3)
                });
            }

            return students;
        }

        private List<PaymentTypeOption> LoadPaymentTypeOptions()
        {
            var paymentTypes = new List<PaymentTypeOption>();

            using SqlConnection connection = DatabaseConnection.GetConnection();
            connection.Open();

            using SqlCommand command = new SqlCommand(
                @"SELECT Id, Name, Periodicity, Amount
                  FROM PaymentTypes
                  WHERE IsActive = 1
                  ORDER BY Name",
                connection);
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                paymentTypes.Add(new PaymentTypeOption
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Periodicity = reader.GetString(2),
                    Amount = reader.GetDecimal(3)
                });
            }

            return paymentTypes;
        }

        private List<ScholarshipRule> LoadRulesForPaymentType(int paymentTypeId)
        {
            var rules = new List<ScholarshipRule>();

            using SqlConnection connection = DatabaseConnection.GetConnection();
            connection.Open();

            using SqlCommand command = new SqlCommand(
                @"SELECT Id, PaymentTypeId, Speciality, MinAverageGrade, MaxAverageGrade,
                         Coefficient, ValidFrom, ValidTo, IsActive
                  FROM ScholarshipRules
                  WHERE PaymentTypeId = @id AND IsActive = 1",
                connection);
            command.Parameters.Add("@id", SqlDbType.Int).Value = paymentTypeId;

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                rules.Add(new ScholarshipRule
                {
                    Id = reader.GetInt32(0),
                    PaymentTypeId = reader.GetInt32(1),
                    Speciality = reader.GetString(2),
                    MinAverageGrade = reader.GetDecimal(3),
                    MaxAverageGrade = reader.GetDecimal(4),
                    Coefficient = reader.GetDecimal(5),
                    ValidFrom = reader.GetDateTime(6),
                    ValidTo = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    IsActive = reader.GetBoolean(8)
                });
            }

            return rules;
        }

        private bool TryCalculateAmount(
            StudentOption student,
            PaymentTypeOption paymentType,
            decimal averageGrade,
            DateTime periodStart,
            DateTime periodEnd,
            out decimal coefficient,
            out decimal amount,
            out string error)
        {
            coefficient = 0;
            amount = 0;
            error = "";

            if (averageGrade < 0 || averageGrade > 10)
            {
                error = "Средний балл должен быть от 0 до 10.";
                return false;
            }

            if (periodEnd.Date < periodStart.Date)
            {
                error = "Дата окончания периода раньше даты начала.";
                return false;
            }

            try
            {
                List<ScholarshipRule> rules = LoadRulesForPaymentType(paymentType.Id);
                ScholarshipRule? rule = _calculator.FindRule(
                    averageGrade, student.Speciality, rules, periodStart.Date);

                if (rule == null)
                {
                    error = "Для выбранного студента, вида выплаты и периода " +
                            "подходящее правило расчёта не найдено.";
                    return false;
                }

                int months = paymentType.Periodicity == "Monthly"
                    ? ScholarshipCalculator.CountMonths(periodStart.Date, periodEnd.Date)
                    : 1;

                coefficient = rule.Coefficient;
                amount = Math.Round(paymentType.Amount * coefficient * months, 2);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private bool TryGetSelectedAssignmentId(out int id)
        {
            id = 0;

            DataGridViewRow? row = dataGridViewStudents.CurrentRow;
            if (row?.Cells["Id"]?.Value == null)
                return false;

            return int.TryParse(row.Cells["Id"].Value.ToString(), out id);
        }

        // ============================================================
        // Поиск и обновление
        // ============================================================
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();

            switch (_section)
            {
                case "types":
                    LoadPaymentTypes(search);
                    break;
                case "rules":
                    LoadRules(search);
                    break;
                case "assignments":
                    LoadAssignments(search);
                    break;
                default:
                    LoadStudents(search);
                    break;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            ReloadCurrentSection();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            LoadStudents();

            dataGridViewStudents.CellDoubleClick +=
                dataGridViewStudents_CellDoubleClick;
        }

        // ============================================================
        // Меню
        // ============================================================
        private void CreateMenu()
        {
            MenuStrip menu = new MenuStrip();

            ToolStripMenuItem students = new ToolStripMenuItem("Студенты");
            ToolStripMenuItem payments = new ToolStripMenuItem("Выплаты");
            ToolStripMenuItem help = new ToolStripMenuItem("Справка");

            ToolStripMenuItem studentList = new ToolStripMenuItem("Список студентов");
            ToolStripMenuItem paymentTypes = new ToolStripMenuItem("Виды выплат");
            ToolStripMenuItem addPaymentType = new ToolStripMenuItem("Добавить вид выплаты");
            ToolStripMenuItem rules = new ToolStripMenuItem("Правила стипендий");
            ToolStripMenuItem assignPayment = new ToolStripMenuItem("Назначить выплату");
            ToolStripMenuItem assignments = new ToolStripMenuItem("Назначенные выплаты");

            studentList.Click += (s, e) => LoadStudents();
            paymentTypes.Click += (s, e) => LoadPaymentTypes();
            addPaymentType.Click += (s, e) => AddPaymentType();
            rules.Click += (s, e) => LoadRules();
            assignPayment.Click += (s, e) => ShowAssignPaymentDialog();
            assignments.Click += (s, e) => LoadAssignments();

            help.Click += (s, e) =>
                MessageBox.Show(
                    "Система учёта стипендий и прочих выплат студентам ПолесГУ.",
                    "О программе");

            students.DropDownItems.Add(studentList);

            payments.DropDownItems.Add(paymentTypes);
            payments.DropDownItems.Add(addPaymentType);
            payments.DropDownItems.Add(rules);
            payments.DropDownItems.Add(assignPayment);
            payments.DropDownItems.Add(assignments);

            menu.Items.Add(students);
            menu.Items.Add(payments);
            menu.Items.Add(help);

            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        // ============================================================
        // Ошибки
        // ============================================================
        private static void ShowError(string action, Exception ex)
        {
            string message = ex switch
            {
                SqlException sql when sql.Number is 2627 or 2601 =>
                    "Запись с такими же данными уже существует.",
                SqlException sql when sql.Number == 547 =>
                    "Операция нарушает связи между таблицами.",
                SqlException sql when sql.Number == 208 =>
                    "В базе данных отсутствует нужная таблица или столбец.",
                _ => ex.Message
            };

            MessageBox.Show($"{action}: {message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Пустые обработчики, оставшиеся от дизайнера.
        private void dataGridView1_CellContentClick(
            object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
    }
}
