﻿using System;
using System.Windows.Forms;

namespace LuteBot.UI
{
    partial class TrackSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrackSelectionForm));
            this.ChannelsListBox = new System.Windows.Forms.CheckedListBox();
            this.SelectAllChannelsCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SongProfileSaveButton = new System.Windows.Forms.Button();
            this.AutoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.LoadProfileButton = new System.Windows.Forms.Button();
            this.TrackListLabel = new System.Windows.Forms.Label();
            this.SelectAllTracksCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackListBox = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // ChannelsListBox
            // 
            this.ChannelsListBox.FormattingEnabled = true;
            this.ChannelsListBox.Location = new System.Drawing.Point(12, 52);
            this.ChannelsListBox.Name = "ChannelsListBox";
            this.ChannelsListBox.Size = new System.Drawing.Size(207, 304);
            this.ChannelsListBox.TabIndex = 3;
            this.ChannelsListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ChannelListBox_ItemChecked);
            // 
            // SelectAllChannelsCheckBox
            // 
            this.SelectAllChannelsCheckBox.AutoSize = true;
            this.SelectAllChannelsCheckBox.Location = new System.Drawing.Point(12, 29);
            this.SelectAllChannelsCheckBox.Name = "SelectAllChannelsCheckBox";
            this.SelectAllChannelsCheckBox.Size = new System.Drawing.Size(70, 17);
            this.SelectAllChannelsCheckBox.TabIndex = 7;
            this.SelectAllChannelsCheckBox.Text = "Select All";
            this.SelectAllChannelsCheckBox.UseVisualStyleBackColor = true;
            this.SelectAllChannelsCheckBox.CheckedChanged += new System.EventHandler(this.SelectAllChannelsTextBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 17);
            this.label1.TabIndex = 9;
            this.label1.Text = "List of Instruments";
            // 
            // SongProfileSaveButton
            // 
            this.SongProfileSaveButton.Location = new System.Drawing.Point(12, 362);
            this.SongProfileSaveButton.Name = "SongProfileSaveButton";
            this.SongProfileSaveButton.Size = new System.Drawing.Size(207, 30);
            this.SongProfileSaveButton.TabIndex = 10;
            this.SongProfileSaveButton.Text = "Save Song Profile";
            this.SongProfileSaveButton.UseVisualStyleBackColor = true;
            this.SongProfileSaveButton.Click += new System.EventHandler(this.SongProfileSaveButton_Click);
            // 
            // AutoActivateCheckBox
            // 
            this.AutoActivateCheckBox.AutoSize = true;
            this.AutoActivateCheckBox.Checked = true;
            this.AutoActivateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoActivateCheckBox.Location = new System.Drawing.Point(141, 398);
            this.AutoActivateCheckBox.Name = "AutoActivateCheckBox";
            this.AutoActivateCheckBox.Size = new System.Drawing.Size(154, 30);
            this.AutoActivateCheckBox.TabIndex = 11;
            this.AutoActivateCheckBox.Text = "Automatically load a profile \r\non song selection";
            this.AutoActivateCheckBox.UseVisualStyleBackColor = true;
            this.AutoActivateCheckBox.CheckedChanged += new System.EventHandler(this.AutoActivateCheckBox_CheckedChanged);
            // 
            // LoadProfileButton
            // 
            this.LoadProfileButton.Location = new System.Drawing.Point(225, 362);
            this.LoadProfileButton.Name = "LoadProfileButton";
            this.LoadProfileButton.Size = new System.Drawing.Size(209, 30);
            this.LoadProfileButton.TabIndex = 12;
            this.LoadProfileButton.Text = "Load Song Profile";
            this.LoadProfileButton.UseVisualStyleBackColor = true;
            this.LoadProfileButton.Click += new System.EventHandler(this.LoadProfileButton_Click);
            // 
            // TrackListLabel
            // 
            this.TrackListLabel.AutoSize = true;
            this.TrackListLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TrackListLabel.Location = new System.Drawing.Point(225, 9);
            this.TrackListLabel.Name = "TrackListLabel";
            this.TrackListLabel.Size = new System.Drawing.Size(107, 17);
            this.TrackListLabel.TabIndex = 15;
            this.TrackListLabel.Text = "List of Tracks";
            // 
            // SelectAllTracksCheckBox
            // 
            this.SelectAllTracksCheckBox.AutoSize = true;
            this.SelectAllTracksCheckBox.Location = new System.Drawing.Point(225, 29);
            this.SelectAllTracksCheckBox.Name = "SelectAllTracksCheckBox";
            this.SelectAllTracksCheckBox.Size = new System.Drawing.Size(70, 17);
            this.SelectAllTracksCheckBox.TabIndex = 14;
            this.SelectAllTracksCheckBox.Text = "Select All";
            this.SelectAllTracksCheckBox.UseVisualStyleBackColor = true;
            this.SelectAllTracksCheckBox.CheckedChanged += new System.EventHandler(this.SelectAllTracksCheckBox_CheckedChanged);
            // 
            // TrackListBox
            // 
            this.TrackListBox.FormattingEnabled = true;
            this.TrackListBox.Location = new System.Drawing.Point(225, 52);
            this.TrackListBox.Name = "TrackListBox";
            this.TrackListBox.Size = new System.Drawing.Size(207, 304);
            this.TrackListBox.TabIndex = 13;
            this.TrackListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TrackListBox_ItemCheck);
            // 
            // TrackSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 434);
            this.Controls.Add(this.TrackListLabel);
            this.Controls.Add(this.SelectAllTracksCheckBox);
            this.Controls.Add(this.TrackListBox);
            this.Controls.Add(this.LoadProfileButton);
            this.Controls.Add(this.AutoActivateCheckBox);
            this.Controls.Add(this.SongProfileSaveButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SelectAllChannelsCheckBox);
            this.Controls.Add(this.ChannelsListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TrackSelectionForm";
            this.Text = "Track Selection";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TrackSelectionForm_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckedListBox ChannelsListBox;
        private System.Windows.Forms.CheckBox SelectAllChannelsCheckBox;
        private Label label1;
        private Button SongProfileSaveButton;
        private CheckBox AutoActivateCheckBox;
        private Button LoadProfileButton;
        private Label TrackListLabel;
        private CheckBox SelectAllTracksCheckBox;
        private CheckedListBox TrackListBox;
    }
}