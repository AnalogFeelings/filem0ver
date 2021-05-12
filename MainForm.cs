using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FileM0ver
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void directoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                DialogResult dialogResult = folderBrowser.ShowDialog();

                if (dialogResult == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                {
                    if (!Directory.EnumerateFiles(folderBrowser.SelectedPath).Any())
                    {
                        MessageBox.Show("Selected folder is empty and has no files.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    directoryBox.Text = folderBrowser.SelectedPath;
                }
            }
        }

        private void orderButton_Click(object sender, EventArgs e)
        {
            DisableControls();
            if (!Directory.Exists(directoryBox.Text))
            {
                MessageBox.Show("Selected folder does not exist.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls();
                return;
            }

            //Create regex pattern and check if the pattern is invalid.
            Regex fileRegex;
            try
            {
                fileRegex = new Regex(regexPattern.Text);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Regex pattern is invalid.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls();
                return;
            }

            IEnumerable<string> matchingFiles = Directory.EnumerateFiles(directoryBox.Text).Where(file => fileRegex.IsMatch(file));
            foreach (string singleFile in matchingFiles)
            {
                //Absolute mess of code. Hold tight!
                string cutFile = singleFile.Substring(singleFile.LastIndexOf("_") + 1).Trim(); //Remove everything leaving
                                                                                               //only "ParteX.pdf"
                cutFile = cutFile.Substring(0, cutFile.LastIndexOf(".")).Trim();               //Removes the ".pdf"

                string correspondingFolder = Path.Combine(directoryBox.Text, cutFile);

                //Create the folder for that part.
                Directory.CreateDirectory(correspondingFolder);
                if (!Directory.Exists(correspondingFolder))
                {
                    MessageBox.Show($"Could not create folder for \"{cutFile}\". Please run the program as administrator.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    EnableControls();
                    return;
                }

                //Copy or move the file to that folder (works with multiple files with the name "ParteX")
                try
                {
                    if (copyCheckbox.Checked)
                    {
                        File.Copy(singleFile, Path.Combine(correspondingFolder, Path.GetFileName(singleFile)));
                    }
                    else File.Move(singleFile, Path.Combine(correspondingFolder, Path.GetFileName(singleFile)));
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    EnableControls();
                    return;
                }
            }

            MessageBox.Show("Done!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            EnableControls();
        }

        public void DisableControls()
        {
            directoryBox.Enabled = false;
            directoryButton.Enabled = false;
            regexPattern.Enabled = false;
            orderButton.Enabled = false;
            quitButton.Enabled = false;
        }

        public void EnableControls()
        {
            directoryBox.Enabled = true;
            directoryButton.Enabled = true;
            regexPattern.Enabled = true;
            orderButton.Enabled = true;
            quitButton.Enabled = true;
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to quit?", "Warning", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
