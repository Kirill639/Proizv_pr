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
    public partial class TeacherForm : Form
    {
        private readonly int _teacherId;
        private readonly List<int> _teacherGroups = new List<int>() { 3994, 3995, 3996 }; // Пример групп

        public TeacherForm(int teacherId)
        {
            _teacherId = teacherId;
            InitializeComponent();
            InitializeGroupButtons();
        }

        private void InitializeGroupButtons()
        {
            foreach (var groupId in _teacherGroups)
            {
                var btn = new Button
                {
                    Text = $"Группа {groupId}",
                    Tag = groupId,
                    Width = 100,
                    Height = 50,
                    Margin = new Padding(10)
                };

                btn.Click += (sender, e) => ShowGroupForm((int)((Button)sender).Tag);
                flowLayoutPanel1.Controls.Add(btn);
            }
        }

        private void ShowGroupForm(int groupId)
        {
            var groupForm = new GroupForm(_teacherId, groupId);
            groupForm.Show();
        }
    }
}
