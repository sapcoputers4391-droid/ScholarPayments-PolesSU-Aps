using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScholarPayments_PolesSU_Aps.UI;

namespace ScholarPayments_PolesSU_Aps
{
    /// <summary>
    /// Только оформление главного окна: шапка, боковое меню, панель действий,
    /// оформление таблицы и статус-строка. Работа с базой не меняется.
    /// </summary>
    public partial class Form1
    {
        private sealed class NavEntry
        {
            public Panel Host = null!;
            public Panel Bar = null!;
            public Label Label = null!;
        }

        private static readonly HashSet<string> HiddenColumns = new()
        {
            "Id", "StudentId", "PaymentTypeId"
        };

        private static readonly Dictionary<string, string> ColumnHeaders = new()
        {
            ["StudentCardNumber"] = "Билет",
            ["FullName"] = "ФИО",
            ["Faculty"] = "Факультет",
            ["Speciality"] = "Специальность",
            ["StudyGroup"] = "Группа",
            ["Course"] = "Курс",
            ["StudyForm"] = "Форма обучения",
            ["FundingType"] = "Финансирование",
            ["AverageGrade"] = "Средний балл",
            ["Name"] = "Название",
            ["PaymentTypeName"] = "Вид выплаты",
            ["Category"] = "Категория",
            ["Periodicity"] = "Периодичность",
            ["FundingSource"] = "Источник",
            ["Amount"] = "Сумма",
            ["BaseAmount"] = "Базовая сумма",
            ["Description"] = "Описание",
            ["IsActive"] = "Активен",
            ["MinAverageGrade"] = "Мин. балл",
            ["MaxAverageGrade"] = "Макс. балл",
            ["Coefficient"] = "Коэф.",
            ["ValidFrom"] = "Действует с",
            ["ValidTo"] = "Действует по",
            ["DocumentNumber"] = "Документ",
            ["DocumentDate"] = "Дата документа",
            ["PeriodStart"] = "Период с",
            ["PeriodEnd"] = "Период по",
            ["AssignmentDate"] = "Дата назначения",
            ["PaymentDate"] = "Дата выплаты",
            ["Status"] = "Статус",
            ["Reason"] = "Примечание"
        };

        private readonly List<NavEntry> _navEntries = new();
        private Label _sectionTitle = null!;
        private ToolStripStatusLabel _statusCount = null!;
        private Button _btnAssignPayment = null!;
        private Button _btnMarkPaid = null!;
        private Button _btnCancelPayment = null!;
        private bool _gridHandlersAttached;

        protected override void OnLoad(EventArgs e)
        {
            ApplyModernShell();
            base.OnLoad(e);
        }

        // ============================================================
        // Оболочка окна
        // ============================================================
        private void ApplyModernShell()
        {
            SuspendLayout();

            Text = "Стипендии и прочие выплаты студентам ПолесГУ";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1520, 840);
            MinimumSize = new Size(1100, 680);
            BackColor = UiTheme.Background;
            Font = UiTheme.BaseFont;
            DoubleBuffered = true;
            label1.Visible = false;

            MenuStrip? menu = MainMenuStrip ?? Controls.OfType<MenuStrip>().FirstOrDefault();
            if (menu != null)
                StyleMenu(menu);

            Panel header = BuildHeader();
            Panel content = BuildContent();
            Panel sidebar = BuildSidebar(menu);
            StatusStrip status = BuildStatusBar();

            var main = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background };
            main.Controls.Add(content);
            main.Controls.Add(sidebar);
            main.Controls.Add(header);

            Controls.Clear();
            Controls.Add(main);
            Controls.Add(status);
            if (menu != null)
                Controls.Add(menu);

            if (_navEntries.Count > 0)
                ActivateNav(_navEntries[0], null);

            ResumeLayout(true);
        }

        private static void StyleMenu(MenuStrip menu)
        {
            menu.BackColor = UiTheme.Surface;
            menu.ForeColor = UiTheme.TextColor;
            menu.Font = UiTheme.BaseFont;
            menu.Padding = new Padding(14, 3, 0, 3);
            menu.RenderMode = ToolStripRenderMode.System;

            foreach (ToolStripItem item in menu.Items)
            {
                item.ForeColor = UiTheme.TextColor;
                item.Padding = new Padding(10, 5, 10, 5);
            }
        }

        private static Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = UiTheme.HeaderBack
            };

            var logo = new Panel
            {
                Size = new Size(46, 46),
                Location = new Point(22, 17),
                BackColor = UiTheme.Accent
            };
            logo.Controls.Add(new Label
            {
                Text = "П",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = UiTheme.LogoFont,
                ForeColor = UiTheme.HeaderBack
            });

            var title = new Label
            {
                Text = "Стипендии и прочие выплаты",
                AutoSize = true,
                Location = new Point(84, 14),
                Font = UiTheme.TitleFont,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            var subtitle = new Label
            {
                Text = "Полесский государственный университет · объектный программный модуль",
                AutoSize = true,
                Location = new Point(86, 46),
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.HeaderSubtle,
                BackColor = Color.Transparent
            };

            var right = new Label
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 22, 0),
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.HeaderSubtle,
                Text = "База данных: ScholarshipPaym"
            };

            header.Controls.Add(right);
            header.Controls.Add(logo);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            return header;
        }

        private Panel BuildContent()
        {
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Background,
                Padding = new Padding(22, 14, 22, 16)
            };

            _sectionTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                Text = "Студенты",
                Font = UiTheme.SectionFont,
                ForeColor = UiTheme.TextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                WrapContents = false,
                BackColor = UiTheme.Background,
                Padding = new Padding(0, 6, 0, 10)
            };

            txtSearch.Width = 300;
            txtSearch.Font = new Font("Segoe UI", 10.5F);
            txtSearch.Margin = new Padding(0, 2, 8, 0);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;

            btnSearch.Text = "Найти";
            btnSearch.Font = UiTheme.BaseFont;
            ApplyFlat(btnSearch, UiTheme.OutlineButton("Найти", 92));
            btnSearch.Margin = new Padding(0, 2, 14, 0);

            ApplyFlat(btnAddStudent, UiTheme.PrimaryButton("Добавить", 124));
            btnAddStudent.Margin = new Padding(0, 2, 8, 0);

            ApplyFlat(btnDeleteStudent, UiTheme.DangerButton("Удалить", 118));
            btnDeleteStudent.Margin = new Padding(0, 2, 8, 0);

            ApplyFlat(btnRefresh, UiTheme.OutlineButton("Обновить", 124));
            btnRefresh.Margin = new Padding(0, 2, 8, 0);

            _btnAssignPayment = UiTheme.PrimaryButton("Назначить выплату", 170);
            _btnAssignPayment.Margin = new Padding(0, 2, 8, 0);
            _btnAssignPayment.Click += (s, e) => ShowAssignPaymentDialog();

            _btnMarkPaid = UiTheme.OutlineButton("Выплатить", 124);
            _btnMarkPaid.Margin = new Padding(0, 2, 8, 0);
            _btnMarkPaid.Click += (s, e) => MarkSelectedPaymentPaid();

            _btnCancelPayment = UiTheme.DangerButton("Отменить", 124);
            _btnCancelPayment.Margin = new Padding(0, 2, 8, 0);
            _btnCancelPayment.Click += (s, e) => CancelSelectedPayment();

            toolbar.Controls.Add(txtSearch);
            toolbar.Controls.Add(btnSearch);
            toolbar.Controls.Add(btnAddStudent);
            toolbar.Controls.Add(btnDeleteStudent);
            toolbar.Controls.Add(_btnAssignPayment);
            toolbar.Controls.Add(_btnMarkPaid);
            toolbar.Controls.Add(_btnCancelPayment);
            toolbar.Controls.Add(btnRefresh);

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Border,
                Padding = new Padding(1)
            };

            UiTheme.StyleGrid(dataGridViewStudents);
            dataGridViewStudents.Dock = DockStyle.Fill;
            card.Controls.Add(dataGridViewStudents);

            content.Controls.Add(card);
            content.Controls.Add(toolbar);
            content.Controls.Add(_sectionTitle);

            if (!_gridHandlersAttached)
            {
                dataGridViewStudents.DataBindingComplete += (s, e) =>
                {
                    ApplyGridLook();
                    UpdateStatusCount();
                };
                dataGridViewStudents.CellFormatting += GridCellFormatting;
                dataGridViewStudents.SelectionChanged += (s, e) => UpdateStatusCount();
                dataGridViewStudents.DataError += (s, e) => e.ThrowException = false;
                _gridHandlersAttached = true;
            }

            return content;
        }

        private static void ApplyFlat(Button target, Button source)
        {
            target.BackColor = source.BackColor;
            target.ForeColor = source.ForeColor;
            target.FlatStyle = source.FlatStyle;
            target.FlatAppearance.BorderSize = source.FlatAppearance.BorderSize;
            target.FlatAppearance.BorderColor = source.FlatAppearance.BorderColor;
            target.FlatAppearance.MouseOverBackColor = source.FlatAppearance.MouseOverBackColor;
            target.FlatAppearance.MouseDownBackColor = source.FlatAppearance.MouseDownBackColor;
            target.UseVisualStyleBackColor = false;
            target.Cursor = Cursors.Hand;
            target.Width = source.Width;
            target.Height = 36;
            target.Font = UiTheme.BaseFont;
        }

        /// <summary>
        /// Показывает только те кнопки, которые относятся к открытому разделу.
        /// </summary>
        private void UpdateSectionActions(string section)
        {
            if (btnAddStudent == null || _btnAssignPayment == null)
                return;

            btnAddStudent.Visible = section is "students" or "types";
            btnDeleteStudent.Visible = section == "students";
            _btnAssignPayment.Visible = section == "assignments";
            _btnMarkPaid.Visible = section == "assignments";
            _btnCancelPayment.Visible = section == "assignments";
        }

        private Panel BuildSidebar(MenuStrip? menu)
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 248,
                BackColor = UiTheme.Surface
            };

            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = UiTheme.Surface,
                Padding = new Padding(0, 6, 0, 0)
            };

            var caption = new Label
            {
                Text = "РАЗДЕЛЫ",
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(22, 0, 0, 0),
                Font = UiTheme.SmallBoldFont,
                ForeColor = UiTheme.Muted,
                BackColor = UiTheme.Surface
            };

            var border = new Panel
            {
                Dock = DockStyle.Right,
                Width = 1,
                BackColor = UiTheme.Border
            };

            AddNavItem(nav, "Студенты", FindMenuItem(menu, "Список студентов"));
            AddNavItem(nav, "Виды выплат", FindMenuItem(menu, "Виды выплат"));
            AddNavItem(nav, "Правила стипендий", FindMenuItem(menu, "Правила стипендий"));
            AddNavItem(nav, "Назначенные выплаты", FindMenuItem(menu, "Назначенные выплаты"));

            sidebar.Controls.Add(nav);
            sidebar.Controls.Add(border);
            sidebar.Controls.Add(caption);
            return sidebar;
        }

        private void AddNavItem(FlowLayoutPanel nav, string title, ToolStripItem? target)
        {
            var host = new Panel
            {
                Width = 247,
                Height = 48,
                BackColor = UiTheme.Surface,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };

            var label = new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 8, 0),
                Font = UiTheme.BaseFont,
                ForeColor = UiTheme.TextColor,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var bar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = UiTheme.Surface
            };

            host.Controls.Add(label);
            host.Controls.Add(bar);

            var entry = new NavEntry { Host = host, Bar = bar, Label = label };
            _navEntries.Add(entry);

            EventHandler click = (s, e) => ActivateNav(entry, target);
            host.Click += click;
            label.Click += click;
            bar.Click += click;

            host.MouseEnter += (s, e) => SetNavHover(entry, true);
            host.MouseLeave += (s, e) =>
            {
                if (!host.ClientRectangle.Contains(host.PointToClient(Cursor.Position)))
                    SetNavHover(entry, false);
            };

            nav.Controls.Add(host);
        }

        private void SetNavHover(NavEntry entry, bool hover)
        {
            if (entry.Bar.BackColor == UiTheme.Accent)
                return;

            entry.Host.BackColor = hover ? UiTheme.NavHover : UiTheme.Surface;
        }

        private void ActivateNav(NavEntry active, ToolStripItem? target)
        {
            foreach (NavEntry entry in _navEntries)
            {
                bool isActive = entry == active;
                entry.Bar.BackColor = isActive ? UiTheme.Accent : UiTheme.Surface;
                entry.Host.BackColor = isActive ? UiTheme.NavActive : UiTheme.Surface;
                entry.Label.Font = isActive ? UiTheme.BoldFont : UiTheme.BaseFont;
                entry.Label.ForeColor = isActive ? UiTheme.Primary : UiTheme.TextColor;
            }

            if (_sectionTitle != null)
                _sectionTitle.Text = active.Label.Text;

            target?.PerformClick();
        }

        private static ToolStripItem? FindMenuItem(MenuStrip? menu, string text)
        {
            if (menu == null)
                return null;

            foreach (ToolStripItem item in menu.Items)
            {
                if (item.Text == text)
                    return item;

                if (item is ToolStripMenuItem parent)
                {
                    foreach (ToolStripItem child in parent.DropDownItems)
                    {
                        if (child.Text == text)
                            return child;
                    }
                }
            }
            return null;
        }

        private StatusStrip BuildStatusBar()
        {
            var status = new StatusStrip
            {
                BackColor = UiTheme.Surface,
                SizingGrip = false,
                Font = UiTheme.SmallFont,
                Padding = new Padding(16, 3, 16, 3)
            };

            _statusCount = new ToolStripStatusLabel
            {
                Text = "0 записей",
                ForeColor = UiTheme.Muted
            };

            var spring = new ToolStripStatusLabel { Spring = true };

            var hint = new ToolStripStatusLabel
            {
                Text = "Двойной щелчок по строке — редактирование",
                ForeColor = UiTheme.Muted
            };

            status.Items.Add(_statusCount);
            status.Items.Add(spring);
            status.Items.Add(hint);
            return status;
        }

        // ============================================================
        // Оформление диалогов
        // ============================================================
        private static Panel CreateDialogHeader(string title)
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UiTheme.HeaderBack
            };

            header.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0),
                Font = UiTheme.DialogTitleFont,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            });

            return header;
        }

        private static FlowLayoutPanel CreateDialogFooter(Form dialog, string saveText)
        {
            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 66,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = UiTheme.Background,
                Padding = new Padding(0, 14, 24, 0)
            };

            Button save = UiTheme.PrimaryButton(saveText, 140);
            save.DialogResult = DialogResult.OK;
            save.Margin = new Padding(8, 0, 0, 0);

            Button cancel = UiTheme.OutlineButton("Отмена", 120);
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Margin = new Padding(0);

            footer.Controls.Add(save);
            footer.Controls.Add(cancel);

            dialog.AcceptButton = save;
            dialog.CancelButton = cancel;
            return footer;
        }

        // ============================================================
        // Оформление таблицы данных
        // ============================================================
        private static readonly string[] StretchPriority =
        {
            "Description", "FullName", "Name", "Speciality", "Reason"
        };

        private void ApplyGridLook()
        {
            DataGridView grid = dataGridViewStudents;
            if (grid.Columns.Count == 0)
                return;

            var visible = grid.Columns.Cast<DataGridViewColumn>()
                .Where(c => !HiddenColumns.Contains(c.Name))
                .ToList();
            if (visible.Count == 0)
                return;

            string stretch =
                StretchPriority.Select(name => visible.FirstOrDefault(c => c.Name == name)?.Name)
                    .FirstOrDefault(name => name != null)
                ?? visible[visible.Count - 1].Name;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (HiddenColumns.Contains(column.Name))
                {
                    column.Visible = false;
                    continue;
                }

                if (ColumnHeaders.TryGetValue(column.Name, out string? header))
                    column.HeaderText = header;

                column.SortMode = DataGridViewColumnSortMode.Automatic;

                if (column.Name == stretch)
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    column.MinimumWidth = column.Name == "FullName" ? 240 : 180;
                }
                else
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    column.MinimumWidth = 60;
                }

                switch (column.Name)
                {
                    case "AverageGrade":
                    case "MinAverageGrade":
                    case "MaxAverageGrade":
                        column.DefaultCellStyle.Format = "N2";
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        break;
                    case "Amount":
                    case "BaseAmount":
                        column.DefaultCellStyle.Format = "N2";
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        break;
                    case "Coefficient":
                        column.DefaultCellStyle.Format = "N3";
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        break;
                    case "Course":
                    case "IsActive":
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        break;
                    case "ValidFrom":
                    case "ValidTo":
                    case "DocumentDate":
                    case "PeriodStart":
                    case "PeriodEnd":
                    case "PaymentDate":
                        column.DefaultCellStyle.Format = "dd.MM.yyyy";
                        break;
                }
            }
        }

        private void GridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.Value == null)
                return;

            DataGridView grid = dataGridViewStudents;
            if (e.ColumnIndex >= grid.Columns.Count)
                return;

            if (e.Value is not string raw)
                return;

            string name = grid.Columns[e.ColumnIndex].Name;

            switch (name)
            {
                case "Category":
                    e.Value = raw switch
                    {
                        "Scholarship" => "Стипендия",
                        "MaterialAid" => "Материальная помощь",
                        "Other" => "Прочая выплата",
                        _ => raw
                    };
                    e.FormattingApplied = true;
                    break;

                case "Periodicity":
                    e.Value = raw switch
                    {
                        "Monthly" => "Ежемесячно",
                        "Once" => "Разово",
                        _ => raw
                    };
                    e.FormattingApplied = true;
                    break;

                case "Status":
                    e.Value = raw switch
                    {
                        "Assigned" => "Назначена",
                        "Paid" => "Выплачена",
                        "Cancelled" => "Отменена",
                        _ => raw
                    };
                    e.FormattingApplied = true;
                    break;

                case "StudyForm":
                    e.Value = raw switch
                    {
                        "Budget" => "Бюджетная",
                        "Paid" => "Платная",
                        _ => raw
                    };
                    e.FormattingApplied = true;
                    break;
            }
        }

        private void UpdateStatusCount()
        {
            if (_statusCount == null)
                return;

            int rows = dataGridViewStudents.Rows.Count;
            string word = rows % 10 == 1 && rows % 100 != 11
                ? "запись"
                : rows % 10 >= 2 && rows % 10 <= 4 && (rows % 100 < 12 || rows % 100 > 14)
                    ? "записи"
                    : "записей";

            _statusCount.Text = $"{rows} {word}";
        }
    }
}
