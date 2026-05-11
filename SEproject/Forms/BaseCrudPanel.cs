using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AcademicProjectSystem.Forms
{
    public partial class BaseCrudPanel : UserControl
    {
        protected DataGridView grid;
        protected Panel pnlForm;
        protected Button btnAdd, btnSave, btnDelete, btnClear;
        protected Label lblTitle;

        private Panel pnlTop;
        private Panel pnlBottom;
        private Panel pnlButtons;
        private Panel pnlScroll;

        public BaseCrudPanel(string title)
        {
            BackColor = Color.FromArgb(240, 243, 248);
            Dock = DockStyle.Fill;

            pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 243, 248),
                Padding = new Padding(0, 6, 0, 0)
            };

            btnAdd = MakeBtn("➕ New", Color.FromArgb(18, 30, 54));
            btnSave = MakeBtn("💾 Save", Color.FromArgb(40, 167, 69));
            btnDelete = MakeBtn("🗑 Delete", Color.FromArgb(220, 53, 69));
            btnClear = MakeBtn("✖ Clear", Color.Gray);

            btnAdd.Click += (s, e) => ClearFields();
            btnSave.Click += (s, e) => SaveRecord();
            btnDelete.Click += (s, e) => DeleteRecord();
            btnClear.Click += (s, e) => ClearFields();

            var flowBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            flowBtns.Controls.AddRange(new Control[] { btnAdd, btnSave, btnDelete, btnClear });
            pnlButtons.Controls.Add(flowBtns);

            pnlForm = new Panel
            {
                AutoSize = false,
                Width = 900,
                Height = 220,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 8),
                Location = new Point(0, 0)
            };

            pnlScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };
            pnlScroll.Controls.Add(pnlForm);

            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 230,
                BackColor = Color.FromArgb(240, 243, 248)
            };

            pnlBottom.Controls.Add(pnlScroll);
            pnlBottom.Controls.Add(pnlButtons);

            pnlTop = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 243, 248),
                Padding = new Padding(0, 0, 0, 8)
            };

            lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 30, 54),
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(0, 0, 0, 4)
            };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.SelectionChanged += (s, e) => OnRowSelected();

            pnlTop.Controls.Add(grid);
            pnlTop.Controls.Add(lblTitle);

            Controls.Add(pnlTop);
            Controls.Add(pnlBottom);

            LoadData();
        }

        private Button MakeBtn(string text, Color color)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(110, 36),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        protected virtual void LoadData() { }
        protected virtual void SaveRecord() { }
        protected virtual void DeleteRecord() { }
        protected virtual void ClearFields() { }
        protected virtual void OnRowSelected() { }

        protected Label AddLabel(string text, int x, int y)
        {
            var l = new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            pnlForm.Controls.Add(l);
            return l;
        }

        protected TextBox AddTextBox(int x, int y, int w = 200)
        {
            var t = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 28),
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlForm.Controls.Add(t);
            return t;
        }

        protected ComboBox AddComboBox(int x, int y, int w = 200)
        {
            var c = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 28),
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlForm.Controls.Add(c);
            return c;
        }

        protected DateTimePicker AddDatePicker(int x, int y, int w = 200)
        {
            var d = new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(w, 28),
                Font = new Font("Segoe UI", 9),
                Format = DateTimePickerFormat.Short
            };
            pnlForm.Controls.Add(d);
            return d;
        }

        protected int SelectedID(string idColumn)
        {
            if (grid.CurrentRow == null) return 0;
            var val = grid.CurrentRow.Cells[idColumn].Value;
            return val == null || val == DBNull.Value ? 0 : Convert.ToInt32(val);
        }

        protected bool Confirm(string msg) =>
            MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;

        protected void Success(string msg) =>
            MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        protected void Error(string msg) =>
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}