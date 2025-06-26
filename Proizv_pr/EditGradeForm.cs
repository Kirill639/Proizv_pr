using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proizv_pr
{
    public partial class EditGradeForm : Form
    {
        public int GradeId { get; private set; }
        public int NewRating { get; private set; }
        public DateTime NewDate { get; private set; }

        private NumericUpDown nudRating;
        private DateTimePicker dtpDate;
        private Button btnOk;
        private Button btnCancel;

        public EditGradeForm(int gradeId, int currentRating, DateTime currentDate)
        {
            GradeId = gradeId;
            NewRating = currentRating;
            NewDate = currentDate;
            InitializeComponents(currentRating, currentDate);
        }

        private void InitializeComponents(int currentRating, DateTime currentDate)
        {
            this.Text = "Изменить оценку";
            this.Size = new Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Компоненты формы
            var lblRating = new Label { Text = "Оценка:", Left = 0, Top = 20 };
            nudRating = new NumericUpDown { Left = 100, Top = 20, Width = 50, Minimum = 1, Maximum = 5, Value = currentRating };

            var lblDate = new Label { Text = "Дата:", Left = 0, Top = 50 };
            dtpDate = new DateTimePicker { Left = 100, Top = 50, Width = 150, Value = currentDate };

            btnOk = new Button { Text = "OK", Left = 50, Top = 80, Width = 80 };
            btnCancel = new Button { Text = "Отмена", Left = 150, Top = 80, Width = 80 };

            btnOk.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblRating, nudRating, lblDate, dtpDate, btnOk, btnCancel });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                NewRating = (int)nudRating.Value;
                NewDate = dtpDate.Value.Date;
            }
            base.OnFormClosing(e);
        }
    }
}

