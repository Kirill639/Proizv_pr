using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;

namespace Proizv_pr
{
    public partial class WorstGradesForm : Form
    {
        private readonly int _teacherId;
        private readonly DbConnection _dbConnection = new DbConnection();
        private const string WorstGradesQuery =
            "SELECT * FROM proizv_pr.get_students_with_worst_grades(@subjectId, @minGrade)";

        public WorstGradesForm(int teacherId)
        {
            _teacherId = teacherId;
            InitializeComponent();
            Text = "Студенты с худшими оценками";
            LoadSubjects();
        }

        private void LoadSubjects()
        {
            try
            {
                _dbConnection.Open();
                var cmd = _dbConnection.CreateCommand(
                    "SELECT subject_id, name FROM proizv_pr.subjects WHERE teacher_id = @teacherId");
                cmd.Parameters.AddWithValue("@teacherId", _teacherId);

                var adapter = new NpgsqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);

                comboBoxSubjects.DataSource = table;
                comboBoxSubjects.DisplayMember = "name";
                comboBoxSubjects.ValueMember = "subject_id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметов: {ex.Message}");
            }
            finally
            {
                _dbConnection.Close();
            }
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBoxSubjects.SelectedItem == null || string.IsNullOrEmpty(txtMinGrade.Text))
            {
                MessageBox.Show("Выберите предмет и укажите минимальную оценку");
                return;
            }

            if (!int.TryParse(txtMinGrade.Text, out int minGrade) || minGrade < 1 || minGrade > 5)
            {
                MessageBox.Show("Введите корректную оценку (1-5)");
                return;
            }

            DataTable table = new DataTable();

            try
            {
                _dbConnection.Open();
                var cmd = _dbConnection.CreateCommand(WorstGradesQuery);
                cmd.Parameters.AddWithValue("@subjectId", (int)comboBoxSubjects.SelectedValue);
                cmd.Parameters.AddWithValue("@minGrade", minGrade);

                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    adapter.Fill(table);
                }

                dataGridViewResults.DataSource = table;

                // Настройка отображения колонок
                if (dataGridViewResults.Columns.Contains("student_id"))
                    dataGridViewResults.Columns["student_id"].Visible = false;
                if (dataGridViewResults.Columns.Contains("group_id"))
                    dataGridViewResults.Columns["group_id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}");
            }
            finally
            {
                _dbConnection.Close();
            }
        }
    }
}
