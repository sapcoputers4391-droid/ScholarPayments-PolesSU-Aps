using System.Drawing;
using System.Windows.Forms;

namespace ScholarPayments_PolesSU_Aps.UI
{
    /// <summary>
    /// Единая визуальная тема приложения (оформление, не логика).
    /// </summary>
    internal static class UiTheme
    {
        public static readonly Color HeaderBack = Color.FromArgb(22, 50, 79);
        public static readonly Color HeaderSubtle = Color.FromArgb(168, 192, 216);
        public static readonly Color Primary = Color.FromArgb(31, 78, 121);
        public static readonly Color PrimaryHover = Color.FromArgb(24, 62, 98);
        public static readonly Color Accent = Color.FromArgb(244, 180, 0);
        public static readonly Color Danger = Color.FromArgb(178, 58, 58);
        public static readonly Color DangerHover = Color.FromArgb(252, 240, 240);
        public static readonly Color Background = Color.FromArgb(243, 246, 250);
        public static readonly Color Surface = Color.White;
        public static readonly Color Border = Color.FromArgb(213, 221, 231);
        public static readonly Color TextColor = Color.FromArgb(30, 41, 51);
        public static readonly Color Muted = Color.FromArgb(110, 123, 137);
        public static readonly Color RowAlternate = Color.FromArgb(247, 250, 253);
        public static readonly Color Selection = Color.FromArgb(219, 233, 248);
        public static readonly Color NavActive = Color.FromArgb(232, 241, 250);
        public static readonly Color NavHover = Color.FromArgb(241, 246, 252);
        public static readonly Color GridLine = Color.FromArgb(232, 238, 245);

        public static readonly Font BaseFont = new Font("Segoe UI", 9.75F);
        public static readonly Font BoldFont = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 9F);
        public static readonly Font SmallBoldFont = new Font("Segoe UI", 8.25F, FontStyle.Bold);
        public static readonly Font SectionFont = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font TitleFont = new Font("Segoe UI", 15.5F, FontStyle.Bold);
        public static readonly Font DialogTitleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static readonly Font LogoFont = new Font("Segoe UI", 17F, FontStyle.Bold);

        // ------------------------------------------------------------
        // Кнопки
        // ------------------------------------------------------------
        public static Button PrimaryButton(string text, int width = 120) =>
            MakeButton(text, width, Primary, Color.White, PrimaryHover, 0, Color.Empty);

        public static Button OutlineButton(string text, int width = 120, Color? textColor = null) =>
            MakeButton(text, width, Surface, textColor ?? Primary, NavHover, 1, Border);

        public static Button DangerButton(string text, int width = 120) =>
            MakeButton(text, width, Surface, Danger, DangerHover, 1, Color.FromArgb(228, 199, 199));

        private static Button MakeButton(string text, int width, Color back, Color fore,
            Color hover, int borderSize, Color borderColor)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 36,
                BackColor = back,
                ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Font = BaseFont,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TabStop = true
            };
            button.FlatAppearance.BorderSize = borderSize;
            if (borderSize > 0)
                button.FlatAppearance.BorderColor = borderColor;
            button.FlatAppearance.MouseOverBackColor = hover;
            button.FlatAppearance.MouseDownBackColor = hover;
            return button;
        }

        // ------------------------------------------------------------
        // Таблица
        // ------------------------------------------------------------
        public static void StyleGrid(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Surface;
            grid.GridColor = GridLine;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToOrderColumns = true;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 42;
            grid.RowTemplate.Height = 30;
            grid.Font = BaseFont;
            grid.Margin = new Padding(0);

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = HeaderBack,
                ForeColor = Color.White,
                Font = BoldFont,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                SelectionBackColor = HeaderBack,
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Surface,
                ForeColor = TextColor,
                SelectionBackColor = Selection,
                SelectionForeColor = TextColor,
                Padding = new Padding(10, 0, 10, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = RowAlternate,
                ForeColor = TextColor,
                SelectionBackColor = Selection,
                SelectionForeColor = TextColor,
                Padding = new Padding(10, 0, 10, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False
            };
        }

        // ------------------------------------------------------------
        // Поля ввода в диалогах
        // ------------------------------------------------------------
        public static void StyleField(Control control)
        {
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 9, 0, 9);
            control.Font = BaseFont;
        }

        public static void StyleCombo(ComboBox combo)
        {
            combo.Dock = DockStyle.Fill;
            combo.Margin = new Padding(0, 9, 0, 9);
            combo.Font = BaseFont;
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.FlatStyle = FlatStyle.Flat;
            combo.BackColor = Surface;
        }

        public static void StyleDate(DateTimePicker picker)
        {
            picker.Dock = DockStyle.Fill;
            picker.Margin = new Padding(0, 9, 0, 9);
            picker.Font = BaseFont;
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd.MM.yyyy";
        }

        public static Label FieldLabel(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Muted,
            Font = BaseFont
        };
    }
}
