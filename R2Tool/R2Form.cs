using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace R2SaveEditor
{    
    public partial class R2Form : Form
    {
        // ==== Save Path & Data ====
        private string _saveDir;
        private string _gameSavPath;
        private byte[] _raw;

        public R2Form()
        {
            InitializeComponent();
        }

        public static class AppLog
        {
            public static Action<string> WriteLine;
        }
        internal static class LuaCompilerPS3
        {
            [DllImport("LuaCompilerR2.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            private static extern int CompileLuaToLcPs3(string inLua, string outLc, StringBuilder err, int errSize);

            public static void Compile(string luaPath, string lcPath)
            {
                var err = new StringBuilder(8192);
                int r = CompileLuaToLcPs3(luaPath, lcPath, err, err.Capacity);
                if (r != 0)
                    throw new Exception(err.Length > 0 ? err.ToString() : $"Lua compilation failed (code {r}).");
                AppLog.WriteLine?.Invoke("Compile: " + luaPath);
            }
        }

        private void R2Form_Load(object sender, EventArgs e)
        {
            AppLog.WriteLine = (msg) =>
            {
                if (IsDisposed) return;

                void append()
                {
                    logsTextBox.AppendText(msg + Environment.NewLine);
                }

                if (logsTextBox.InvokeRequired)
                    logsTextBox.BeginInvoke((Action)append);
                else
                    append();
            };
            AppLog.WriteLine?.Invoke("Created by FormatA3");
        }
        private void logsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (logsCheckBox.Checked)
            {
                this.Size = new Size(400, 550);
            }
            else
            {
                this.Size = new Size(390, 340);
            }
        }

        // ==== Save Buttons ====
        private void openButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) != DialogResult.OK)
                return;

            _saveDir = folderBrowserDialog1.SelectedPath;
            _gameSavPath = Path.Combine(_saveDir, "GAME.SAV");

            if (!File.Exists(_gameSavPath))
            {
                MessageBox.Show("GAME.SAV introuvable dans ce dossier.");
                return;
            }

            _raw = File.ReadAllBytes(_gameSavPath);
            AppLog.WriteLine?.Invoke("Open: " + _gameSavPath);

            // ===== Get values =====
            nudCampaignXp.Value = SaveEditor.GetCampaignXp(_raw);
            nudCompXp.Value = SaveEditor.GetCompXp(_raw);

            nudMedicHead.Value = SaveEditor.GetMedicHeadId(_raw);
            nudSoldierHead.Value = SaveEditor.GetSoldierHeadId(_raw);
            nudSpecHead.Value = SaveEditor.GetSpecHeadId(_raw);

            check_BlackWraith.Checked = SaveEditor.IsBlackWraithEnabled(_raw);
        }
        private void saveButton_Click(object sender, EventArgs e)
        {
            // ===== Set values =====
            SaveEditor.SetCampaignXp(_raw, (int)nudCampaignXp.Value);
            SaveEditor.SetCompXp(_raw, (int)nudCompXp.Value);

            SaveEditor.SetMedicHeadId(_raw, (byte)nudMedicHead.Value);
            SaveEditor.SetSoldierHeadId(_raw, (byte)nudSoldierHead.Value);
            SaveEditor.SetSpecHeadId(_raw, (byte)nudSpecHead.Value);

            SaveEditor.SetBlackWraith(_raw, check_BlackWraith.Checked);

            SaveEditor.UpdateR2Crc(_raw);

            File.WriteAllBytes(_gameSavPath, _raw);
            AppLog.WriteLine?.Invoke("Save: " + _gameSavPath);

            MessageBox.Show($"Saved.");
        }


        // ==== PSARC Buttons ====
        private void unpackButton_Click(object sender, EventArgs e)
        {
            string psarcPath;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "PSARC (*.psarc)|*.psarc|All files (*.*)|*.*";
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                psarcPath = ofd.FileName;
            }

            string unpackRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "unpack_PSARC"
            );

            string name = Path.GetFileNameWithoutExtension(psarcPath);
            string outDir = Path.Combine(unpackRoot, name);
            Directory.CreateDirectory(outDir);

            try
            {
                PSARC.Unpack(psarcPath, outDir);
                MessageBox.Show("Unpack OK:\n" + outDir);
                Process.Start("explorer.exe", unpackRoot);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur:\n" + ex.Message);
            }
        }
        private void repackButton_Click(object sender, EventArgs e)
        {
            string extractedDir;
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Sélectionner le dossier extrait (qui contient fileslist.txt)";
                fbd.SelectedPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "unpack_PSARC"
                );

                if (fbd.ShowDialog(this) != DialogResult.OK)
                    return;

                extractedDir = fbd.SelectedPath;
            }

            string manifestPath = Path.Combine(extractedDir, "fileslist.txt");
            if (!File.Exists(manifestPath))
            {
                MessageBox.Show("files.lst introuvable dans ce dossier.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string repackRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "repack_PSARC"
            );
            Directory.CreateDirectory(repackRoot);

            string archiveName = Path.GetFileName(extractedDir.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar));

            string outputPsarcPath = Path.Combine(repackRoot, archiveName + ".psarc");

            try
            {
                PSARC.RepackFromManifest(extractedDir, manifestPath, outputPsarcPath);

                MessageBox.Show(
                    "Repack terminé avec succès.\n\n" + outputPsarcPath,
                    "PSARC Repack",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                string repackDir = Path.GetDirectoryName(outputPsarcPath);
                Process.Start("explorer.exe", repackDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur lors du repack :\n\n" + ex.Message,
                    "PSARC Repack",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // ==== Assets Buttons ====
        private void openTextureEditorButton_Click(object sender, EventArgs e)
        {
            new TextureEditorForm().Show(this);
        }
        private void extractAssetsButton_Click(object sender, EventArgs e)
        {
            string folder;
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                fbd.Description = "Choisir le dossier qui contient assetlookup.dat et les fichiers .dat";
                if (fbd.ShowDialog(this) != DialogResult.OK) return;
                folder = fbd.SelectedPath;
            }

            try
            {
                var outRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "extract_ASSETS",
                    new DirectoryInfo(folder).Name
                );

                var result = R2AssetFolderExtractor.ExtractFromFolder(folder, outRoot);

                System.Diagnostics.Process.Start("explorer.exe", result.OutputDirectory);

                MessageBox.Show(
                    $"Terminé.\n\n" +
                    $"Sortie: {result.OutputDirectory}\n" +
                    $"Notes: {result.Notes}",
                    "Extract Assets (Folder)"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur:\n" + ex.Message);
            }
        }

        // ==== LUA Buttons ====
        private void compileLUAButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Choisir un fichier Lua à compiler";
                    ofd.Filter = "Lua script (*.lua)|*.lua|Tous les fichiers (*.*)|*.*";
                    ofd.Multiselect = false;

                    if (ofd.ShowDialog() != DialogResult.OK)
                        return;

                    string luaPath = ofd.FileName;

                    string lcPath = Path.Combine(
                        Path.GetDirectoryName(luaPath)!,
                        Path.GetFileNameWithoutExtension(luaPath) + ".lc"
                    );

                    LuaCompilerPS3.Compile(luaPath, lcPath);

                    MessageBox.Show(
                        "Compilation done!\n\nFile path :\n" + lcPath,
                        "Lua Compiler R2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error :\n\n" + ex.Message,
                    "Lua Compiler R2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
