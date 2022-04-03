using FileBoxUI.Models;

namespace FileBoxUI
{
    public partial class FileTable : Form
    {
        private static HttpClientHandler handler;
        private static HttpClient client;
        public FileTable()
        {
            InitializeComponent();
            handler = new HttpClientHandler();
            client = new HttpClient(handler);
            dgvFiles.AllowUserToAddRows = false;
            UpdateDGV().GetAwaiter();

    }

        private async void btnLoadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new())
            {
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (tbxFileName.Text != "")
                    {
                        
                        byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);
                        string name = tbxFileName.Text + Path.GetExtension(openFileDialog.FileName);
                        if (bytes != null)
                        {
                            try
                            {
                                await HttpUpdate.PostAsync(client, bytes, name);
                                await UpdateDGV();
                            }
                            catch
                            {
                                MessageBox.Show("Нет подключения!");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введите название файла");
                    }
                }
            }

        }

        private async void btnDownloadFile_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new())
            {
                if (dgvFiles.RowCount != 0)
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {

                        string path = folderBrowserDialog.SelectedPath;
                        string id = dgvFiles.SelectedRows[0].Cells["cId"].Value.ToString();
                        string fileName = dgvFiles.SelectedRows[0].Cells["cName"].Value.ToString() +
                        dgvFiles.SelectedRows[0].Cells["cType"].Value.ToString();
                        try
                        {
                            await HttpUpdate.GetIdAsync(client, id, path, fileName);
                            MessageBox.Show("Файл загружен!");
                        }
                        catch
                        {
                            MessageBox.Show("Нет подключения!");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите файл"!);
                }

            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvFiles.RowCount != 0)
                {
                    string id = dgvFiles.SelectedRows[0].Cells["cId"].Value.ToString();
                    await HttpUpdate.DeleteAsync(client, id);
                    await UpdateDGV();
                }
                else
                {
                    {
                        MessageBox.Show("Выберите файл!");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Нет подключения!");
            }
        }

        private async Task UpdateDGV()
        {
            List<FileItem> files = await HttpUpdate.GetAsync(client);
            dgvFiles.Rows.Clear();
            foreach (FileItem file in files)
            {
                dgvFiles.Rows.Add(file.Id, file.Name, file.Type, file.Size);
            }

        }
        private void FileTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();

        }

        
    }
}
