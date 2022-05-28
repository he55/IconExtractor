using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace IconExtractor
{
    public partial class Form1 : Form
    {
        string _title;
        string _filePath;
        string _recentPath;
        List<string> _recentFiles;
        ImageList _imageList;
        Bitmap _bitmap;
        int _nIconIndex;
        int _iconSize = 64;
        uint _nIcons;

        public Form1()
        {
            InitializeComponent();
            _title = this.Text;
            pictureBox1.Size = new Size(0, 0);
        }

        void LoadFile(string filePath)
        {
            _filePath = filePath;
            _bitmap = null;
            _nIconIndex = 0;
            _nIcons = 0;

            listView1.Items.Clear();
            pictureBox1.Image = null;
            toolStripMenuItem3.Enabled = false;
            toolStripMenuItem4.Enabled = false;
            toolStripStatusLabel1.Text = "0 个图标";

            this.Text = $"{_title} - \"{filePath}\"";
            toolStripStatusLabel3.Text = "正在加载数据...";
            Application.DoEvents();

            if (_recentFiles.Contains(filePath))
                _recentFiles.Remove(filePath);
            _recentFiles.Insert(0, filePath);
            File.WriteAllLines(_recentPath, _recentFiles);

            _nIcons = PInvoke.PrivateExtractIcons(filePath, 0, 0, 0, null, null, 0, 0);
            if (_nIcons == 0)
            {
                toolStripStatusLabel3.Text = "没有图标资源";
                return;
            }

            IntPtr[] phicon = new IntPtr[_nIcons];
            uint[] piconid = new uint[_nIcons];
            uint nIcons = PInvoke.PrivateExtractIcons(filePath, 0, 32, 32, phicon, piconid, _nIcons, 0);

            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(32, 32);
            listView1.LargeImageList = imageList;
            _imageList?.Dispose();
            _imageList = imageList;

            for (int i = 0; i < nIcons; i++)
            {
                Icon icon = Icon.FromHandle(phicon[i]);
                Bitmap bitmap = icon.ToBitmap();
                imageList.Images.Add(bitmap);
                icon.Dispose();

                PInvoke.DestroyIcon(phicon[i]);
            }

            listView1.BeginUpdate();
            for (int i = 0; i < nIcons; i++)
            {
                ListViewItem listViewItem = new ListViewItem($"{i}", i);
                listView1.Items.Add(listViewItem);
            }
            listView1.EndUpdate();

            listView1.SelectedIndices.Add(0);
            toolStripMenuItem3.Enabled = true;
            toolStripMenuItem4.Enabled = true;
            toolStripStatusLabel1.Text = $"{nIcons} 个图标";
            toolStripStatusLabel3.Text = "完成";
        }

        void LoadImage()
        {
            if (_nIcons == 0)
                return;

            IntPtr[] phicon = new IntPtr[1];
            uint[] piconid = new uint[1];
            uint nIcons = PInvoke.PrivateExtractIcons(_filePath, _nIconIndex, _iconSize, _iconSize, phicon, piconid, 1, 0);
            if (nIcons == 0)
            {
                pictureBox1.Image = null;
                return;
            }

            using (Icon icon = Icon.FromHandle(phicon[0]))
            {
                Bitmap bitmap = icon.ToBitmap();
                pictureBox1.Image = bitmap;
                _bitmap?.Dispose();
                _bitmap = bitmap;
            }

            PInvoke.DestroyIcon(phicon[0]);
        }

        #region 控件事件

        private void Form1_Load(object sender, EventArgs e)
        {
            _recentFiles = new List<string>();
            _recentPath = Path.Combine(Application.UserAppDataPath, "recent.txt");
            if (File.Exists(_recentPath))
            {
                string[] paths = File.ReadAllLines(_recentPath);
                _recentFiles.AddRange(paths);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            StringCollection files = ((DataObject)e.Data).GetFileDropList();
            if (files.Count > 0 && (files[0].EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || files[0].EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            StringCollection files = ((DataObject)e.Data).GetFileDropList();
            LoadFile(files[0]);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                _nIconIndex = listView1.SelectedIndices[0];
                LoadImage();
            }
        }

        private void radioButton_Click(object sender, EventArgs e)
        {
            string tag = (string)((RadioButton)sender).Tag;
            if (int.TryParse(tag, out int val))
            {
                _iconSize = val;
                LoadImage();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_bitmap != null)
            {
                string pngFile = Path.Combine(Path.GetTempPath(), $"{Path.GetFileName(_filePath)}.{_nIconIndex}.png");
                _bitmap.Save(pngFile, ImageFormat.Png);

                DataObject dataObject = new DataObject();
                dataObject.SetFileDropList(new StringCollection { pngFile });
                DoDragDrop(dataObject, DragDropEffects.Move);
            }
        }

        #endregion

        #region 菜单事件

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Resource Files|*.exe;*.dll|All Files|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                LoadFile(openFileDialog.FileName);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (_bitmap != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = $"{Path.GetFileName(_filePath)}.{_nIconIndex}.png";
                saveFileDialog.Filter = "PNG Files|*.png";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    _bitmap.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (_nIcons > 0)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string folder = Path.Combine(folderBrowserDialog.SelectedPath, Path.GetFileNameWithoutExtension(_filePath));
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    toolStripStatusLabel3.Text = "正在保存图片...";
                    Application.DoEvents();

                    IntPtr[] phicon = new IntPtr[_nIcons];
                    uint[] piconid = new uint[_nIcons];
                    uint nIcons = PInvoke.PrivateExtractIcons(_filePath, 0, _iconSize, _iconSize, phicon, piconid, _nIcons, 0);

                    for (int i = 0; i < nIcons; i++)
                    {
                        using (Icon icon = Icon.FromHandle(phicon[i]))
                        using (Bitmap bitmap = icon.ToBitmap())
                        {
                            string pngFile = Path.Combine(folder, $"{i}.png");
                            bitmap.Save(pngFile, ImageFormat.Png);
                        }

                        PInvoke.DestroyIcon(phicon[i]);
                    }
                    toolStripStatusLabel3.Text = "完成";
                }
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem6_DropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItem6.DropDownItems.Clear();
            foreach (string item in _recentFiles)
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(item);
                toolStripMenuItem.Click += (object sender_, EventArgs e_) =>
                {
                    LoadFile(((ToolStripMenuItem)sender_).Text);
                };
                toolStripMenuItem6.DropDownItems.Add(toolStripMenuItem);
            }

            toolStripMenuItem7.Enabled = _recentFiles.Count != 0;
            toolStripMenuItem6.DropDownItems.Add(toolStripMenuItem7);
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            _recentFiles.Clear();
            File.WriteAllText(_recentPath, "");
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/he55/IconExtractor",
                UseShellExecute = true
            });
        }

        #endregion
    }
}
