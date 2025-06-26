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
    public partial class Form1 : Form
    {
        private readonly DatabaseHelper _dbHelper;
        public Form1()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var login = txtLogin.Text.Trim();
                var password = txtPassword.Text;

                var authResult = _dbHelper.Authenticate(login, password);

                if (authResult.HasValue)
                {
                    this.Hide();

                    Form userForm;

                    if (authResult.Value.role == "teacher")
                    {
                        userForm = new TeacherForm(authResult.Value.id);
                    }
                    else
                    {
                        userForm = new StudentForm(authResult.Value.id);
                    }

                    userForm.Closed += (s, args) => this.Close();
                    userForm.Show();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
