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

namespace Paralelismo {
    public partial class NewNotepad : Form {
        public NewNotepad() {
            InitializeComponent();
        }

        private void mNew_Click(object sender, EventArgs e) {
            textBox.Clear();
        }

        private void mOpen_Click(object sender, EventArgs e) {
            dlgOpen.ShowDialog();
            if (String.IsNullOrEmpty(dlgOpen.FileName)) {
                MessageBox.Show("Nenhum nome foi especificado");
                return;
            }

            FileStream fs = new FileStream(dlgOpen.FileName, FileMode.Open, FileAccess.Read);
            using (StreamReader reader = new StreamReader(fs, Encoding.UTF8)){
                textBox.Text = reader.ReadToEnd();
            }
        }

        private void mSave_Click(object sender, EventArgs e) {
            dlgSave.ShowDialog();
            if (String.IsNullOrEmpty(dlgSave.FileName)) {
                MessageBox.Show("Nenhum nome foi especificado");
                return;
            }

            FileStream fs = new FileStream(dlgSave.FileName, FileMode.Create, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8)) {
                writer.Write(textBox.Text);
            }
        }
    }
}
