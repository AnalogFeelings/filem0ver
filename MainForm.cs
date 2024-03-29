﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
                string extensionlessFilename = Path.GetFileNameWithoutExtension(singleFile);
                //Remove all except "ParteX"
                string cutFile = extensionlessFilename.Substring(extensionlessFilename.LastIndexOf("_") + 1).Trim();

                string correspondingFolder = Path.Combine(directoryBox.Text, cutFile);
                string filename = Path.GetFileName(singleFile);
                string fileExtension = Path.GetExtension(singleFile);

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
                    string combinedPath = Path.Combine(correspondingFolder, filename);
                    if (copyCheckbox.Checked)
                    {
                        File.Copy(singleFile, combinedPath);

                        //Make a copy of the file to follow the scenario.
                        File.Copy(combinedPath,
                            Path.Combine(correspondingFolder, extensionlessFilename + "_copy" + fileExtension));
                    }
                    else
                    {
                        File.Move(singleFile, combinedPath);

                        //Make a copy of the file to follow the scenario.
                        File.Copy(combinedPath,
                            Path.Combine(correspondingFolder, extensionlessFilename + "_copy" + fileExtension));
                    }
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

        private void aboutButton_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }
    }
}
