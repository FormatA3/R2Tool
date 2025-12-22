using System;
using System.IO;
using System.Windows.Forms;

namespace R2SaveEditor
{
    public partial class TextureEditorForm : Form
    {
        private InsomniacV2Textures _tex;
        private string _currentFolder;
        private System.Threading.CancellationTokenSource _previewCts;

        public TextureEditorForm()
        {
            InitializeComponent();
        }
        private void openAssetLookupButton_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Sélectionne le dossier contenant assetlookup.dat + highmips.dat (+ autres .dat)";
                if (fbd.ShowDialog(this) != DialogResult.OK) return;

                _currentFolder = fbd.SelectedPath;
                currentFolderLabel.Text = _currentFolder;

                try
                {
                    _tex?.Dispose();
                    _tex = InsomniacV2Textures.LoadFromFolder(_currentFolder);

                    assetListbox.BeginUpdate();
                    assetListbox.Items.Clear();
                    foreach (var t in _tex.Textures)
                        assetListbox.Items.Add(t);
                    assetListbox.EndUpdate();

                    if (assetListbox.Items.Count > 0)
                        assetListbox.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur chargement textures:\n" + ex.Message);
                }
            }
        }
        private void extractButton_Click(object sender, EventArgs e)
        {
            if (_tex == null || assetListbox.SelectedItem == null) return;
            var entry = (InsomniacV2Textures.TextureEntry)assetListbox.SelectedItem;

            string outDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "extract_TEXTURES",
                Path.GetFileName(_currentFolder.TrimEnd(Path.DirectorySeparatorChar))
            );
            Directory.CreateDirectory(outDir);

            string outPath = Path.Combine(outDir, $"tex_{entry.Index:D5}_{entry.Id:X16}.dds");

            try
            {
                _tex.ExtractSelectedToDds(entry, outPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur extraction:\n" + ex.Message);
            }
        }
        private void extractAllButton_Click(object sender, EventArgs e)
        {
            if (_tex == null) return;

            string outDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "extract_TEXTURES",
                Path.GetFileName(_currentFolder.TrimEnd(Path.DirectorySeparatorChar))
            );
            Directory.CreateDirectory(outDir);

            try
            {
                _tex.ExtractAllToFolder(outDir);
                System.Diagnostics.Process.Start("explorer.exe", outDir);
                MessageBox.Show("Extraction terminée !");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur Extract All:\n" + ex.Message);
            }
        }
        private void replaceButton_Click(object sender, EventArgs e)
        {
            if (_tex == null || assetListbox.SelectedItem == null) return;
            var entry = (InsomniacV2Textures.TextureEntry)assetListbox.SelectedItem;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "DDS (*.dds)|*.dds|All files (*.*)|*.*";
                ofd.Title = "Choisir une texture DDS à injecter (mêmes dimensions/format)";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    _tex.ReplaceHighmipFromDds(entry, ofd.FileName, createBackupOnce: true);
                    var old = previewPictureBox.Image;
                    previewPictureBox.Image = _tex.GetPreviewBitmap(entry);
                    old?.Dispose();

                    MessageBox.Show("Replace OK !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur Replace:\n" + ex.Message);
                }
            }
        }
        private async void assetListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tex == null) return;
            if (assetListbox.SelectedItem == null) return;

            _previewCts?.Cancel();
            _previewCts = new System.Threading.CancellationTokenSource();
            var token = _previewCts.Token;

            var entry = (InsomniacV2Textures.TextureEntry)assetListbox.SelectedItem;

            try
            {
                var bmp = await System.Threading.Tasks.Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return _tex.GetPreviewBitmap(entry);
                }, token);

                if (token.IsCancellationRequested)
                {
                    bmp.Dispose();
                    return;
                }

                var old = previewPictureBox.Image;
                previewPictureBox.Image = bmp;
                old?.Dispose();

            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                var old = previewPictureBox.Image;
                previewPictureBox.Image = null;
                old?.Dispose();
            }
        }
    }
}
