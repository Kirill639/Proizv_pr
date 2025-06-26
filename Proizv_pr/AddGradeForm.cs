using Npgsql;
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
    public partial class AddGradeForm : Form
    {
        public int StudentId { get; private set; }
        public int SubjectId { get; private set; }
        public int Rating { get; private set; }
        public DateTime Date { get; private set; }

        private ComboBox cbSubjects;
        private NumericUpDown nudRating;
        private DateTimePicker dtpDate;
        private Button btnOk;
        private Button btnCancel;

        public AddGradeForm(int studentId, int teacherId)
        {
            StudentId = studentId;
            InitializeComponents(teacherId);
        }

        private void InitializeComponents(int teacherId)
        {
            this.Text = "Добавить оценку";
            this.Size = new Size(300, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Компоненты формы
            var lblSubject = new Label { Text = "Предмет:", Left = 0, Top = 20 };
            cbSubjects = new ComboBox { Left = 100, Top = 20, Width = 150 };

            var lblRating = new Label { Text = "Оценка:", Left = 0, Top = 50 };
            nudRating = new NumericUpDown { Left = 100, Top = 50, Width = 50, Minimum = 1, Maximum = 5 };

            var lblDate = new Label { Text = "Дата:", Left = 0, Top = 80 };
            dtpDate = new DateTimePicker { Left = 100, Top = 80, Width = 150, Value = DateTime.Today };

            btnOk = new Button { Text = "OK", Left = 50, Top = 120, Width = 80 };
            btnCancel = new Button { Text = "Отмена", Left = 150, Top = 120, Width = 80 };

            btnOk.Click += (s, e) =>
            {
                if (cbSubjects.SelectedItem != null)
                {
                    var selectedItem = (dynamic)cbSubjects.SelectedItem;
                    SubjectId = selectedItem.SubjectId;
                    Rating = (int)nudRating.Value;
                    Date = dtpDate.Value;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Выберите предмет!");
                }
            };

            this.Controls.AddRange(new Control[] { lblSubject, cbSubjects, lblRating, nudRating, lblDate, dtpDate, btnOk, btnCancel });

            LoadSubjects(teacherId);
        }

        private void LoadSubjects(int teacherId)
        {
            var dbConnection = new DbConnection();
            try
            {
                dbConnection.Open();
                var cmd = dbConnection.CreateCommand(
                    "SELECT subject_id, name as subject_name FROM proizv_pr.subjects WHERE teacher_id = @teacher_id");
                cmd.Parameters.AddWithValue("@teacher_id", teacherId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cbSubjects.Items.Add(new
                        {
                            SubjectId = reader.GetInt32(0),
                            SubjectName = reader.GetString(1)
                        });
                    }
                }
                cbSubjects.DisplayMember = "SubjectName";
                cbSubjects.ValueMember = "SubjectId";

                if (cbSubjects.Items.Count > 0)
                    cbSubjects.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметов: {ex.Message}");
            }
            finally
            {
                dbConnection.Close();
            }
        }

    }
}
