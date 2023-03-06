using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoogleUploader
{
    public partial class Prompt : Form
    {
        public Prompt()
        {
            InitializeComponent();
        }

        private void Prompt_Load(object sender, EventArgs e)
        {
            Ok.DialogResult = DialogResult.OK;
        }
    }
}
