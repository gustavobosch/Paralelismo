using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paralelismo {
    public partial class NewNotepad : Form {

        private CancellationTokenSource cancelSource;

        public NewNotepad() {
            InitializeComponent();
        }

        private void mNew_Click(object sender, EventArgs e) {
            textBox.Clear();
        }

        private void mOpen_Click(object sender, EventArgs e) {
            DialogResult res = dlgOpen.ShowDialog();
            if (String.IsNullOrEmpty(dlgOpen.FileName)) {
                if (res != DialogResult.Cancel) {
                    MessageBox.Show("Nenhum nome foi especificado");
                }
                return;
            }

            textBox.Enabled = false;
            cancelSource = new CancellationTokenSource();
            Task backgroundReadTask = new Task(() => {

                string content = NewNotepad.LoadFile(dlgOpen.FileName, cancelSource);
                NewNotepad.ThreadSafeFormControl(textBox, () => {
                    textBox.Text = (content == null) ? "" : content;
                    textBox.Enabled = true;
                });

            });
            backgroundReadTask.Start();
        }

        private void mSave_Click(object sender, EventArgs e) {
            DialogResult res = dlgSave.ShowDialog();
            if (String.IsNullOrEmpty(dlgSave.FileName)) {
                if (res != DialogResult.Cancel) {
                    MessageBox.Show("Nenhum nome foi especificado");
                }
                return;
            }

            Task backgroundWriteTask = new Task(() => {
                NewNotepad.SaveFile(dlgSave.FileName, textBox.Text, cancelSource);
            });
            backgroundWriteTask.Start();
        }

        private void menuCancel_Click(object sender, EventArgs e) {
            cancelSource.Cancel();
        }

        private static void ThreadSafeFormControl(Control destControl, Action action) {
            if (destControl.InvokeRequired) {
                destControl.Invoke(action);
            } else {
                action.Invoke();
            }
        }

        private static string LoadFile(string filename, CancellationTokenSource cancelSource) {
            FileStream fs;
            try {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            } catch (FileNotFoundException fnfe) {
                MessageBox.Show("Arquivo não encontrado.");
                return null;
            } catch (UnauthorizedAccessException uae) {
                MessageBox.Show("Você não tem permissão para abrir o arquivo especificado.");
                return null;
            } catch (SystemException se) {
                MessageBox.Show("Erro ao abrir o arquivo: " + se.Message);
                return null;
            }

            try {
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8)) {
                    StringBuilder builder = new StringBuilder();
                    char[] buf = new char[128000];
                    while (!reader.EndOfStream && !cancelSource.IsCancellationRequested) {
                        int nChars = reader.ReadBlock(buf, 0, 128000);
                        builder.Append(buf, 0 , nChars);
                    }
                    if (cancelSource.IsCancellationRequested) {
                        return null;
                    }
                    return builder.ToString(0, Math.Min(builder.Length, 65536));
                }
            } catch (IOException ioe) {
                MessageBox.Show("Erro durante a leitura do arquivo.");
                return null;
            }
        }

        private static void SaveFile(string filename, string data, CancellationTokenSource cancelSource) {
            FileStream fs;
            try {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            } catch (FileNotFoundException fnfe) {
                MessageBox.Show("Arquivo não encontrado.");
                return;
            } catch (UnauthorizedAccessException uae) {
                MessageBox.Show("Você não tem permissão para abrir o arquivo especificado.");
                return;
            } catch (SystemException se) {
                MessageBox.Show("Erro ao abrir o arquivo: " + se.Message);
                return;
            }

            try {
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8)) {
                    writer.Write(data);
                }
            } catch (IOException ioe) {
                MessageBox.Show("Erro durante a gravação do arquivo.");
                return;
            }
        }
    }
}