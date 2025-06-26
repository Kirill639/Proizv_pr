using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace Proizv_pr
{
    public partial class StudentForm : Form
    {
        private readonly int _studentId;
        private readonly DatabaseHelper _dbHelper;

        public StudentForm(int studentId)
        {
            InitializeComponent();
            _studentId = studentId;
            _dbHelper = new DatabaseHelper();
            LoadStudentData();
        }

        private void LoadStudentData()
        {
            try
            {
                var groupId = GetGroupId();
                if (groupId == null) return;

                var dataTable = GetGroupGrades(groupId.Value);
                dataGridView1.DataSource = dataTable;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                SetFormTitle(groupId.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? GetGroupId()
        {
            using (var connection = new DbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand(
                    "SELECT group_id FROM proizv_pr.students WHERE student_id = @studentId"))
                {
                    command.Parameters.Add(new NpgsqlParameter("@studentId", _studentId));
                    return command.ExecuteScalar() as int?;
                }
            }
        }

        private DataTable GetGroupGrades(int groupId)
        {
            var dataTable = new DataTable();
            using (var connection = new DbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand(
                    @"SELECT 
                        student_name AS ""Студент"",
                        subject_name AS ""Предмет"",
                        teacher_name AS ""Преподаватель"",
                        date AS ""Дата"",
                        rating AS ""Оценка""
                      FROM proizv_pr.group_grades_view
                      WHERE group_id = @groupId
                      ORDER BY student_name, subject_name, date"))
                {
                    command.Parameters.Add(new NpgsqlParameter("@groupId", groupId));
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        private void SetFormTitle(int groupId)
        {
            using (var connection = new DbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand(
                    "SELECT 'Группа ' || group_id FROM proizv_pr.groups WHERE group_id = @groupId"))
                {
                    command.Parameters.Add(new NpgsqlParameter("@groupId", groupId));
                    var groupName = command.ExecuteScalar()?.ToString();
                    this.Text = $"Оценки группы - {groupName}";
                }
            }
        }

      

       
    }
}