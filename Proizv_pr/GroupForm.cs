using ClosedXML.Excel;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proizv_pr
{
    public partial class GroupForm : Form
    {
        private readonly int _teacherId;
        private readonly int _groupId;
        private DataTable _gradesTable = new DataTable();
        private readonly DbConnection _dbConnection = new DbConnection();

        public GroupForm(int teacherId, int groupId)
        {
            _teacherId = teacherId;
            _groupId = groupId;
            InitializeComponent();
            Text = $"Группа {groupId} - предметы преподавателя";
            LoadData();
        }


        private void LoadData()
        {
            try
            {
                _dbConnection.Open();

                var studentsCmd = _dbConnection.CreateCommand(
                    "SELECT student_id, last_name, first_name FROM proizv_pr.group_students_view WHERE group_id = @groupId");
                studentsCmd.Parameters.AddWithValue("@groupId", _groupId);

                var studentsAdapter = new NpgsqlDataAdapter(studentsCmd);
                var studentsTable = new DataTable();
                studentsAdapter.Fill(studentsTable);
                dataGridViewStudents.DataSource = studentsTable;

                var gradesCmd = _dbConnection.CreateCommand(
                    "SELECT grade_id, student_name, subject_name, date, rating " +
                    "FROM proizv_pr.teacher_group_grades_view " +
                    "WHERE group_id = @groupId AND teacher_id = @teacherId " +
                    "ORDER BY date DESC");
                gradesCmd.Parameters.AddWithValue("@groupId", _groupId);
                gradesCmd.Parameters.AddWithValue("@teacherId", _teacherId);

                var gradesAdapter = new NpgsqlDataAdapter(gradesCmd);
                var gradesTable = new DataTable();
                gradesAdapter.Fill(gradesTable);
                dataGridViewGrades.DataSource = gradesTable;
                _gradesTable = new DataTable();
                gradesAdapter.Fill(_gradesTable);
                if (dataGridViewGrades.Columns.Contains("grade_id"))
                {
                    dataGridViewGrades.Columns["grade_id"].Visible = false;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                _dbConnection.Close();
            }
        }

        private void BtnEdit_Click_1(object sender, EventArgs e)
        {
            if (dataGridViewGrades.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите оценку для изменения");
                return;
            }

            var selectedRow = dataGridViewGrades.SelectedRows[0];
            var editForm = new EditGradeForm(
                (int)selectedRow.Cells["grade_id"].Value,
                (int)selectedRow.Cells["rating"].Value,
                (DateTime)selectedRow.Cells["date"].Value);

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _dbConnection.Open();
                    var cmd = _dbConnection.CreateCommand(
                    "SELECT * FROM proizv_pr.update_grade(@grade_id, @new_rating, @new_date)");

                    cmd.Parameters.AddWithValue("@grade_id", editForm.GradeId);
                    cmd.Parameters.AddWithValue("@new_rating", editForm.NewRating);
                    cmd.Parameters.Add(new NpgsqlParameter("@new_date", NpgsqlTypes.NpgsqlDbType.Date) { Value = editForm.NewDate });

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Оценка изменена успешно");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении оценки: {ex.Message}");
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите студента из списка");
                return;
            }

            var addForm = new AddGradeForm(
                (int)dataGridViewStudents.SelectedRows[0].Cells["student_id"].Value,
                _teacherId);

            if (addForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _dbConnection.Open();
                    var cmd = _dbConnection.CreateCommand(
                        "SELECT proizv_pr.add_grade(@student_id, @subject_id, @teacher_id, @rating, @date::date)");

                    cmd.Parameters.AddWithValue("@student_id", addForm.StudentId);
                    cmd.Parameters.AddWithValue("@subject_id", addForm.SubjectId);
                    cmd.Parameters.AddWithValue("@teacher_id", _teacherId);
                    cmd.Parameters.AddWithValue("@rating", addForm.Rating);
                    cmd.Parameters.AddWithValue("@date", NpgsqlTypes.NpgsqlDbType.Date, addForm.Date);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Оценка добавлена успешно");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении оценки: {ex.Message}");
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewGrades.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите оценку для удаления");
                return;
            }

            var gradeId = (int)dataGridViewGrades.SelectedRows[0].Cells["grade_id"].Value;
            if (MessageBox.Show("Вы уверены, что хотите удалить эту оценку?",
                "Подтверждение удаления", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _dbConnection.Open();
                    var cmd = _dbConnection.CreateCommand(
                        "SELECT proizv_pr.delete_grade(@grade_id)");
                    cmd.Parameters.AddWithValue("@grade_id", gradeId);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Оценка удалена успешно");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении оценки: {ex.Message}");
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
        }
        private void btnExport_Click(object sender, EventArgs e)
        {

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files|*.xlsx";
                saveFileDialog.Title = "Сохранить как Excel файл";
                saveFileDialog.FileName = $"Оценки_группы_{_groupId}_{DateTime.Now:yyyyMMdd}.xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var gradesSheet = workbook.Worksheets.Add("Оценки");
                            ExportDataGridViewToClosedXML(gradesSheet, dataGridViewGrades);

                            var studentsSheet = workbook.Worksheets.Add("Студенты");
                            ExportDataGridViewToClosedXML(studentsSheet, dataGridViewStudents);

                            workbook.SaveAs(saveFileDialog.FileName);

                            MessageBox.Show("Экспорт завершен успешно!", "Экспорт в Excel",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void ExportDataGridViewToClosedXML(IXLWorksheet worksheet, DataGridView dataGridView)
        {
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                if (dataGridView.Columns[i].Visible)
                {
                    worksheet.Cell(1, i + 1).Value = dataGridView.Columns[i].HeaderText;
                }
            }

            for (int row = 0; row < dataGridView.Rows.Count; row++)
            {
                for (int col = 0; col < dataGridView.Columns.Count; col++)
                {
                    if (dataGridView.Columns[col].Visible)
                    {
                        worksheet.Cell(row + 2, col + 1).Value =
                            dataGridView.Rows[row].Cells[col].Value?.ToString();
                    }
                }
            }

            worksheet.Columns().AdjustToContents();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            var searchText = ((TextBox)sender).Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                dataGridViewGrades.DataSource = _gradesTable;
                return;
            }

            var filteredTable = _gradesTable.Clone();
            var rows = _gradesTable.Select($"student_name LIKE '%{searchText}%'");
            foreach (var row in rows)
            {
                filteredTable.ImportRow(row);
            }
            dataGridViewGrades.DataSource = filteredTable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var form = new WorstGradesForm(_teacherId);
            form.ShowDialog();
        }
    }
}
