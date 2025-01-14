﻿using LuteBot.Config;
using LuteBot.IO.Files;
using LuteBot.TrackSelection;
using LuteBot.UI;
using LuteBot.UI.Utils;

using LuteBot.LuteMod.Indexing;
using System.Data;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Diagnostics;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using LuteBot.UI.PianoRoll;

namespace LuteBot
{
    public partial class PartitionsForm : Form
    {
        public PartitionsForm()
        {
            InitializeComponent();

            this.FormClosing += PartitionsForm_FormClosing1;
            listBoxPartitions.MouseMove += ListBoxPartitions_MouseMove;
            this.DragEnter += PartitionsForm_DragEnter;

            editSelectedButton.Enabled = false;
            reloadSelectedButton.Enabled = false;
            exportSelectedButton.Enabled = false;
            renameSelectedButton.Enabled = false;

            try
            {
                if (!LuteBotForm.Instance.IsLuteModInstalled())
                {
                    using (var popup = new PopupForm("Install LuteMod", "Would you like to update/install LuteMod?", "You need this to play music.\n\nIf you already have a working LuteMod installed, this means there's an important update\n\nThanks to Monty for LuteMod, and cswic for the autoloader\n\nFor more information, see:",
                    new Dictionary<string, string>() {
                    { "What is LuteMod", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod" } ,
                    { "LuteMod mod.io page", "https://mordhau.mod.io/lutemod" },
                    { "Autoloader mod.io page", "https://mordhau.mod.io/clientside-mod-autoloader" },
                    { "Flute and Duets", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Flute_and_Duets" },
                    { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" },
                    }, MessageBoxButtons.YesNo))
                    {
                        popup.ShowDialog(this);
                        if (popup.DialogResult == DialogResult.Yes)
                            LuteBotForm.Instance.InstallLuteMod();
                        else
                            Hide();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            RefreshPartitionList();
        }

        private void PartitionsForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        public static readonly string partitionMidiPath = Path.Combine(LuteBotForm.lutebotPath, "Partition MIDIs");

        private void ListBoxPartitions_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && listBoxPartitions.SelectedItems.Count > 0)
            {
                int mouseIndex = listBoxPartitions.IndexFromPoint(e.Location);
                if (mouseIndex > -1)
                {
                    ListBox.SelectedObjectCollection x = new ListBox.SelectedObjectCollection(listBoxPartitions);
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        int i1 = Math.Min(listBoxPartitions.SelectedIndex, mouseIndex);
                        int i2 = Math.Max(listBoxPartitions.SelectedIndex, mouseIndex);
                        for (int i = i1; i <= i2; ++i)
                        {
                            x.Add(listBoxPartitions.Items[i]);
                        }
                    }
                    else
                    {
                        x = listBoxPartitions.SelectedItems;
                    }
                    var dropResult = DoDragDrop(x, DragDropEffects.Move);
                }
            }
        }



        private void PartitionsForm_FormClosing1(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public PartitionIndex index { get; private set; }

        private void RefreshPartitionList()
        {
            index = new PartitionIndex();
            index.LoadIndex();
            if (!index.Loaded)
            {
                MessageBox.Show("No partition index found.  If LuteMod is installed, you can't add songs until you start Mordhau, go into a game, and kick with a Lute until the menu opens\n\nOr choose Settings -> Install LuteMod, which now includes the partition file, and will update an existing install if necessary");
                Hide();
            }

            PopulateIndexList();
        }

        public async Task PopulateIndexListAsync()
        {
            await this.InvokeAsync(() =>
            {
                PopulateIndexList();
            }).ConfigureAwait(false);
        }

        public void PopulateIndexList()
        {
            SuspendLayout();
            listBoxPartitions.Items.Clear();
            foreach (string item in index.PartitionNames)
            {
                listBoxPartitions.Items.Add(item);
            }
            ResumeLayout();
        }

        private void ContextMenuHelper()
        {
            if (listBoxPartitions.Items.Count > 0 && listBoxPartitions.SelectedIndex >= 0)
            {
                ContextMenuStrip indexContextMenu = new ContextMenuStrip();

                string name = (string)listBoxPartitions.SelectedItems[0];
                if (listBoxPartitions.SelectedItems.Count > 1)
                    name += " + ...";


                if (listBoxPartitions.SelectedIndices.Count == 1)
                {
                    //string midiPath = Path.Combine(partitionMidiPath, listBoxPartitions.SelectedItems[0] + ".mid");
                    //if (File.Exists(midiPath))
                    //{
                    ToolStripItem editItem = indexContextMenu.Items.Add("Edit " + name);
                    editItem.Click += EditItem_Click;
                    //}
                }
                else
                {
                    ToolStripItem exportItem = indexContextMenu.Items.Add("Export " + name);
                    exportItem.Click += ExportItem_Click;
                }

                ToolStripItem deleteItem = indexContextMenu.Items.Add("Delete " + name);
                deleteItem.Click += new EventHandler(DeleteItem_Click);

                ToolStripItem moveItem = indexContextMenu.Items.Add("Move to Top: " + name);
                moveItem.Click += new EventHandler(MoveToTop_Click);

                listBoxPartitions.ContextMenuStrip = indexContextMenu; // TODO: I'd love to make it popup at the selected item, not at mouse pos, but whatever
                indexContextMenu.Show(listBoxPartitions, listBoxPartitions.PointToClient(Cursor.Position));
            }
            else
            {
                listBoxPartitions.ContextMenuStrip = null;
            }
        }

        private async void ExportItem_Click(object sender, EventArgs e)
        {
            await ExportMidis(listBoxPartitions.SelectedItems.Cast<string>()).ConfigureAwait(false);
        }

        private async void EditItem_Click(object sender, EventArgs e)
        {
            string midiPath = Path.Combine(partitionMidiPath, listBoxPartitions.SelectedItems[0] + ".mid");
            await LoadTrackSelectionForMidi(midiPath).ConfigureAwait(false);

        }

        private async Task LoadTrackSelectionForMidi(string midiPath)
        {
            TrackSelectionManager tsm = null;
            if (File.Exists(midiPath))
            {
                tsm = await LuteBotForm.Instance.LoadFile(midiPath).ConfigureAwait(false);
                if (!LuteBotForm.Instance.TrackSelectionForm.Visible)
                {
                    await this.InvokeAsync(() =>
                    {
                        Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                        LuteBotForm.Instance.TrackSelectionForm.Show();
                        LuteBotForm.Instance.TrackSelectionForm.Top = coords.Y;
                        LuteBotForm.Instance.TrackSelectionForm.Left = coords.X;
                    }).ConfigureAwait(false);
                }
                await LuteBotForm.Instance.TrackSelectionForm.InitLists(tsm).ConfigureAwait(false);
                //await this.InvokeAsync(() =>
                //{
                //    try
                //    {
                //        var test = new TestPianoRollForm(tsm);
                //        test.Show();
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.Write(ex);
                //    }
                //}).ConfigureAwait(false);
                
            }
            else
            {
                await LuteBotForm.Instance.HandleErrorAsync(new Exception($"Midi file did not exist: {midiPath}"), $"Midi file did not exist: {midiPath}").ConfigureAwait(false);
                return;
            }
            //await tsm.LoadSavFiles(SaveManager.SaveFilePath, (string)listBoxPartitions.SelectedItems[0]).ConfigureAwait(false);
        }

        private void PartitionIndexBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private async void PartitionIndexBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var filesObject = e.Data.GetData(DataFormats.FileDrop, false);
                if (filesObject != null)
                {
                    var files = (string[])filesObject;
                    await AutoSaveFiles(files).ConfigureAwait(false);
                }
                else if (listBoxPartitions.SelectedIndices.Count > 0)
                {
                    Point point = listBoxPartitions.PointToClient(new Point(e.X, e.Y));
                    int i = this.listBoxPartitions.IndexFromPoint(point);
                    // Try to handle multi-drag-drop, may reorder things
                    var selectedIndices = listBoxPartitions.SelectedIndices.Cast<int>().ToArray();
                    //var selectedItems = listBoxPartitions.SelectedItems.Cast<string>().ToArray();
                    //int[] selectedIndices = e.Data.GetData(typeof(int[])) as int[];
                    string[] selectedItems = new string[selectedIndices.Length];
                    for (int j = 0; j < selectedIndices.Length; j++)
                        selectedItems[j] = (string)listBoxPartitions.Items[selectedIndices[j]];


                    if (selectedItems != null && selectedItems.Length > 0 && i != selectedIndices[0])
                    {
                        foreach (string data in selectedItems)
                        {
                            // First just remove them all
                            this.listBoxPartitions.Items.Remove(data);
                            //this.listBoxPartitions.Items.Insert(i, data);
                            index.PartitionNames.Remove((string)data);
                            //index.PartitionNames.Insert(i, (string)data);

                        }
                        if (i < 0 || i >= listBoxPartitions.Items.Count)
                            i = this.listBoxPartitions.Items.Count - 1;
                        // Then insert them at i+j in their original order
                        for (int j = 0; j < selectedItems.Length; j++)
                        {
                            this.listBoxPartitions.Items.Insert(i + j, selectedItems[j]);
                            index.PartitionNames.Insert(i + j, (string)selectedItems[j]);
                        }
                        // Then re-select all the same things
                        listBoxPartitions.ClearSelected();
                        for (int j = 0; j < selectedItems.Length; j++)
                        {
                            listBoxPartitions.SetSelected(i + j, true);
                        }
                        index.SaveIndex();
                    }
                }
            }
            catch (Exception ex)
            {
                await LuteBotForm.Instance.HandleErrorAsync(ex, "Failed to drag/drop").ConfigureAwait(false);
            }


        }

        private async Task SelectedItemsChanged()
        {
            string filePath = null;
            await this.InvokeAsync(() =>
            {
                if (listBoxPartitions.SelectedIndices.Count == 0)
                {

                    editSelectedButton.Enabled = false;
                    reloadSelectedButton.Enabled = false;
                    exportSelectedButton.Enabled = false;
                    renameSelectedButton.Enabled = false;

                }
                else if (listBoxPartitions.SelectedIndices.Count == 1)
                {
                    editSelectedButton.Enabled = true;
                    reloadSelectedButton.Enabled = true;
                    exportSelectedButton.Enabled = true;
                    renameSelectedButton.Enabled = true;
                    filePath = Path.Combine(partitionMidiPath, (string)(listBoxPartitions.SelectedItems[0]) + ".mid");
                }
                else
                {
                    editSelectedButton.Enabled = false;
                    reloadSelectedButton.Enabled = true;
                    exportSelectedButton.Enabled = true;
                    renameSelectedButton.Enabled = false;
                }
            }).ConfigureAwait(false);
            if (filePath != null)
            {
                await LoadTrackSelectionForMidi(filePath).ConfigureAwait(false);
            }
        }

        private async void PartitionIndexBox_MouseDown(object sender, MouseEventArgs e)
        {

            if (this.listBoxPartitions.SelectedItem == null) return;
            // If something that's already selected was clicked, keep all selected indices that we detected last time
            //Point point = listBoxPartitions.PointToClient(new Point(e.X, e.Y));
            //int i = this.listBoxPartitions.IndexFromPoint(e.Location);
            //if (lastSelectedIndices.Contains(i))
            //{
            //    foreach (var last in lastSelectedIndices)
            //        listBoxPartitions.SetSelected(last, true);
            //}
            // Looked weird and was kindof annoying and not expected behavior.  No longer necessary now that it works without

            // IDK why this works because I supposedly skip if they have a modifier key, but shift+drag to move multi selections is good now
            if (Control.ModifierKeys == Keys.None && e.Button == MouseButtons.Left) // Prevents a bug when multi-selecting
                this.listBoxPartitions.DoDragDrop(listBoxPartitions.SelectedIndices, DragDropEffects.Move);
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuHelper();
            }

            if (e.Button == MouseButtons.Left)
            {
                await Task.Delay(100).ConfigureAwait(false); // There's no good way to detect when selected is changed, and this fires before... so we wait for a sec
                await SelectedItemsChanged().ConfigureAwait(false);
            }
        }

        private void MoveToTop_Click(object sender, EventArgs e)
        {
            var selectedIndices = listBoxPartitions.SelectedIndices.Cast<int>().ToArray();
            //var selectedItems = listBoxPartitions.SelectedItems.Cast<string>().ToArray();
            //int[] selectedIndices = e.Data.GetData(typeof(int[])) as int[];
            string[] selectedItems = new string[selectedIndices.Length];
            for (int j = 0; j < selectedIndices.Length; j++)
                selectedItems[j] = (string)listBoxPartitions.Items[selectedIndices[j]];

            int i = 0;

            if (selectedItems != null && selectedItems.Length > 0 && i != selectedIndices[0])
            {
                foreach (string data in selectedItems)
                {
                    // First just remove them all
                    this.listBoxPartitions.Items.Remove(data);
                    //this.listBoxPartitions.Items.Insert(i, data);
                    index.PartitionNames.Remove((string)data);
                    //index.PartitionNames.Insert(i, (string)data);

                }
                if (i < 0 || i >= listBoxPartitions.Items.Count)
                    i = this.listBoxPartitions.Items.Count - 1;
                // Then insert them at i+j in their original order
                for (int j = 0; j < selectedItems.Length; j++)
                {
                    this.listBoxPartitions.Items.Insert(i + j, selectedItems[j]);
                    index.PartitionNames.Insert(i + j, (string)selectedItems[j]);
                }
                // Then re-select all the same things
                listBoxPartitions.ClearSelected();
                for (int j = 0; j < selectedItems.Length; j++)
                {
                    listBoxPartitions.SetSelected(i + j, true);
                }
                index.SaveIndex();
            }
        }
        private void DeleteItem_Click(object sender, EventArgs e)
        {
            DialogResult confirmResult = MessageBox.Show("Do you want to delete this partition ?",
                                     "Confirm Deletion",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                var selectedList = new List<int>();
                IEnumerable<int> selectedEnum;

                selectedEnum = listBoxPartitions.SelectedIndices.Cast<int>();
                // Just in case some weird shit happens again
                // TLDR, because MouseDown had DoDragDrop, it induced a rare bug in .net framework which made it fail to iterate or AddRange for this type of ListBox
                // We now only DoDragDrop if a key isn't held, so it should be relatively impossible to induce, but if it does, it will silently fail
                try
                {
                    selectedList.AddRange(selectedEnum);
                }
                catch { return; }
                selectedList.Sort((a, b) => b.CompareTo(a)); // Sort largest first so we don't have issues when we remove them
                foreach (int selected in selectedList)
                {
                    try
                    {
                        DeletePartition(selected);
                    }
                    catch { }
                }
                PopulateIndexList();
                index.SaveIndex();
            }
        }

        private void DeletePartition(int selected)
        {
            SaveManager.DeleteData(SaveManager.SaveFilePath + index.PartitionNames[selected]);
            // Also delete the MIDI file
            var midFileName = Path.Combine(partitionMidiPath, index.PartitionNames[selected] + ".mid");
            if (File.Exists(midFileName))
                File.Delete(midFileName);
            index.PartitionNames.RemoveAt(selected);
        }

        public void ImportPartitionButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openSavFileDialog = new OpenFileDialog();
            string[] fileNames;
            openSavFileDialog.DefaultExt = "sav";
            openSavFileDialog.Filter = "SAV files|*.sav";
            openSavFileDialog.Title = "Open SAV file";
            openSavFileDialog.Multiselect = true;
            if (openSavFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNames = openSavFileDialog.FileNames;
                foreach (string fileName in fileNames)
                {
                    index.AddFileInIndex(fileName);
                }
                index.SaveIndex();
                PopulateIndexList();
            }
        }


        private LuteMod.MordhauConverter trackConverter = null;


        public async Task<bool> SavePartitionWithForm(TrackSelectionManager tsm)
        {
            if (tsm.GetTracksAndChannels().Any(c => c.Settings.Any(s => s.Value.Active)))
            {
                var namingForm = new TrackNamingForm(Path.GetFileNameWithoutExtension(tsm.Player.FileName));

                await this.InvokeAsync(() =>
                {
                    namingForm.ShowDialog(this);
                }).ConfigureAwait(false);
                //await Task.Run(() =>
                //{
                if (namingForm.DialogResult == DialogResult.OK)
                    return await SavePartition(namingForm.textBoxPartName.Text, tsm, namingForm.checkBoxOverwrite.Checked).ConfigureAwait(false);

                return false;
            }
            else
            {
                throw new Exception("The song is empty");
            }
        }

        private async Task<bool> SavePartition(string name, string filePath, bool overwrite = false, bool reorderTracks = false, bool autoEnableFlutes = false, bool clearOffsets = false)
        {
            var tsm = await LuteBotForm.Instance.LoadFile(filePath, reorderTracks, autoEnableFlutes, clearOffsets).ConfigureAwait(false);
            return await SavePartition(name, tsm, overwrite).ConfigureAwait(false);
        }

        private SemaphoreSlim IndexSem = new SemaphoreSlim(1, 1);

        private async Task<bool> SavePartition(string name, TrackSelectionManager tsm, bool overwrite = false)
        {
            // We're waiting because we access index a lot in here, and this should be relatively little processing vs loading the file
            // ... Plus the Configuration related stuff isn't threadsafe

            try
            {
                var player = tsm.Player;
                string midiFileName = null;

                var dryWetFile = player.dryWetFile;

                //var quant = new Quantizer();
                //var tempoMap = dryWetFile.GetTempoMap();
                //quant.Quantize(dryWetFile.GetTimedEvents(), new SteppedGrid(new MetricTimeSpan(TimeSpan.FromSeconds(0.017))), tempoMap, new QuantizingSettings { QuantizingLevel = 1, RandomizingSettings = new RandomizingSettings { Filter = t => false } });

                if (name == "Song Name" || name.Trim() == "")
                {
                    throw new Exception("Please name your song");
                }
                else
                {
                    await IndexSem.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        await this.InvokeAsync(() =>
                        {
                            while (index.PartitionNames.Contains(name) && !overwrite)
                            {
                                var namingForm = new TrackNamingForm(name);
                                namingForm.Text = "Name already exists - Enter new Song Name";
                                namingForm.ShowDialog(this);
                                if (namingForm.DialogResult == DialogResult.OK)
                                {
                                    name = namingForm.textBoxPartName.Text;
                                    overwrite = namingForm.checkBoxOverwrite.Checked;
                                }
                                else
                                {
                                    name = null;
                                    return;
                                }
                            }
                        }).ConfigureAwait(false);
                    }
                    finally
                    {
                        IndexSem.Release();
                    }
                    if (name != null)
                    {
                        while (!Regex.IsMatch(name, "^([a-zA-Z0-9][a-zA-Z0-9 -]*[a-zA-Z0-9])$"))
                        {
                            await this.InvokeAsync(() =>
                            {
                                var namingForm = new TrackNamingForm(name);
                                namingForm.Text = "Name contains invalid characters - Enter new Song Name";
                                namingForm.ShowDialog(this);
                                if (namingForm.DialogResult == DialogResult.OK)
                                {
                                    name = namingForm.textBoxPartName.Text;
                                    overwrite = namingForm.checkBoxOverwrite.Checked;
                                }
                                else
                                {
                                    name = null;
                                    return;
                                }
                            }).ConfigureAwait(false);
                        }
                        if (name != null)
                        {
                            var converter = new LuteMod.MordhauConverter();
                            converter.SetDivision(((TicksPerQuarterNoteTimeDivision)dryWetFile.TimeDivision).TicksPerQuarterNote);
                            converter.SetPartitionTempo((int)dryWetFile.GetTempoMap().GetTempoAtTime(new MetricTimeSpan(0)).MicrosecondsPerQuarterNote);
                            converter.IsConversionEnabled = false;
                            var validChannels = tsm.GetValidChannels();
                            var validTracks = tsm.GetValidTracks();
                            
                            // We'll use whichever one has the most fidelity for ordering
                            bool useChannels = TrackSelectionManager.ShouldUseChannels(validChannels, validTracks);

                            MidiChannelItem[] channels;
                            if (useChannels)
                            {
                                channels = validChannels;
                            }
                            else
                            {
                                channels = validTracks;
                            }
                            // Find the lowest tick for a valid note
                            var lowestTick = validChannels.Concat(validTracks).SelectMany(c => c.GetActiveNotes(tsm)).Min(n => n.tickNumber);
                            
                            foreach (var instrument in Instrument.Prefabs)
                            {
                                foreach(var channel in channels.OrderBy(c => c.Settings[instrument.Value.Id].Rank))
                                {
                                    var trackID = converter.AddTrack(instrument.Value.Id);

                                    converter.FillTrack(trackID, instrument.Value, player.ExtractMidiContent(tsm, channel, instrument.Value.Id, lowestTick));
                                }
                            }

                            await IndexSem.WaitAsync().ConfigureAwait(false);
                            try
                            {
                                SaveManager.WriteSaveFile(Path.Combine(SaveManager.SaveFilePath, name), converter.GetPartitionToString());
                                if (overwrite && index.PartitionNames.Contains(name))
                                    index.PartitionNames.Remove(name);

                                index.PartitionNames.Insert(0, name);
                                index.SaveIndex();

                                await PopulateIndexListAsync().ConfigureAwait(false);
                            }
                            finally
                            {
                                IndexSem.Release();
                            }

                        }
                    }
                }
                midiFileName = Path.Combine(partitionMidiPath, name + ".mid");
                Directory.CreateDirectory(partitionMidiPath);
                await tsm.SaveTrackManager(midiFileName).ConfigureAwait(false); // Lutebot doesn't need this anytime soon - and shouldn't offer the option to load it until it exists anyway

            }
            catch (Exception ex)
            {
                await LuteBotForm.Instance.HandleErrorAsync(ex, $"Failed to save {name} to LuteMod").ConfigureAwait(false);
                return false;
            }
            return name != null;
        }

        private void PartitionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowPositionUtils.UpdateBounds(PropertyItem.PartitionListPos, new Point() { X = Left, Y = Top });
            ConfigManager.SaveConfig();
        }

        public void openSaveFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", SaveManager.SaveFilePath);
        }

        public void importPartitions_Click(object sender, EventArgs e)
        {
            OpenFileDialog openSavFileDialog = new OpenFileDialog();
            string[] fileNames;
            openSavFileDialog.DefaultExt = "sav";
            openSavFileDialog.Filter = "SAV files|*.sav";
            openSavFileDialog.Title = "Open SAV file";
            openSavFileDialog.Multiselect = true;
            if (openSavFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNames = openSavFileDialog.FileNames;
                foreach (string fileName in fileNames)
                {
                    index.AddFileInIndex(fileName);
                }
                index.SaveIndex();
                PopulateIndexList();
            }
        }

        public async void saveMultipleSongsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openMidiFileDialog.Title = "Auto-Convert MIDI files to Partitions";
            if (openMidiFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filenames = openMidiFileDialog.FileNames;
                await AutoSaveFiles(filenames).ConfigureAwait(false);
            }
        }

        public void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var trainingForm = new NeuralNetworkForm(this);
            trainingForm.Show(this);

        }

        public void openMidiFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", partitionMidiPath);
        }

        public void exportPartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    string tempDir = Path.Combine(partitionMidiPath, "TempFiles");
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                    Directory.CreateDirectory(tempDir);
                    foreach (var name in index.PartitionNames)
                    {
                        string midiPath = Path.Combine(partitionMidiPath, name + ".mid");
                        if (File.Exists(midiPath))
                            File.Copy(midiPath, Path.Combine(tempDir, name + ".mid"), true);

                        int i = 0;
                        string currentFile = Path.Combine(SaveManager.SaveFilePath, name + "[" + i + "].sav");
                        while (File.Exists(currentFile))
                        {
                            File.Copy(currentFile, Path.Combine(tempDir, name + "[" + i + "].sav"), true);
                            currentFile = Path.Combine(SaveManager.SaveFilePath, name + "[" + ++i + "].sav");
                        }
                    }

                    int c = 0;
                    string partitionFile = Path.Combine(SaveManager.SaveFilePath, "PartitionIndex[" + c + "].sav");
                    while (File.Exists(partitionFile))
                    {
                        File.Copy(partitionFile, Path.Combine(tempDir, "PartitionIndex[" + c + "].sav"));
                        partitionFile = Path.Combine(SaveManager.SaveFilePath, "PartitionIndex[" + ++c + "].sav");
                    }
                    var zipDir = Path.Combine(partitionMidiPath, "Export");
                    Directory.CreateDirectory(zipDir);
                    // Zip the dir
                    ZipFile.CreateFromDirectory(tempDir, Path.Combine(zipDir, DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "-Export.zip"));
                    // Delete temp
                    Directory.Delete(tempDir, true);
                    // Show them the zip
                    Process.Start(zipDir);
                    Invoke((MethodInvoker)delegate { MessageBox.Show($"The export folder `{zipDir}` has been opened", "Export Success"); });
                }
                catch (Exception ex)
                {
                    Invoke((MethodInvoker)delegate { MessageBox.Show(ex.StackTrace, ex.Message); });
                }
            });
        }

        public async void exportMidisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await ExportMidis(index.PartitionNames).ConfigureAwait(false);
        }

        private async Task ExportMidis(IEnumerable<string> partitionNames)
        {
            try
            {
                string tempDir = Path.Combine(partitionMidiPath, "TempFiles");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                Parallel.ForEach(partitionNames, name =>
                {
                    string midiPath = Path.Combine(partitionMidiPath, name + ".mid");
                    if (File.Exists(midiPath))
                        File.Copy(midiPath, Path.Combine(tempDir, name + ".mid"), true);
                });

                var zipDir = Path.Combine(partitionMidiPath, "Export");
                Directory.CreateDirectory(zipDir);
                // Zip the dir
                ZipFile.CreateFromDirectory(tempDir, Path.Combine(zipDir, DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "-Export.zip"));
                // Delete temp
                Directory.Delete(tempDir, true);
                // Show them the zip
                Process.Start(zipDir);
                await this.InvokeAsync(() => { MessageBox.Show(this, $"The export folder `{zipDir}` has been opened", "Export Success"); }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await LuteBotForm.Instance.HandleErrorAsync(ex, "Failed to export midis").ConfigureAwait(false);
            }
        }

        public async Task reloadAll(bool reorderTracks, bool autoEnableFlutes = false, bool clearOffsets = false)
        {
            var filenames = listBoxPartitions.Items.Cast<string>().Reverse().ToArray();

            await AutoSaveFiles(filenames, true, reorderTracks, autoEnableFlutes, clearOffsets).ConfigureAwait(false);
        }

        private async void reloadSelectedButton_Click(object sender, EventArgs e)
        {
            if (listBoxPartitions.SelectedItems.Count > 0)
            {
                var filenames = listBoxPartitions.SelectedItems.Cast<string>().Reverse().ToArray();

                await AutoSaveFiles(filenames, true, true).ConfigureAwait(false);
            }
        }

        private string GetPartitionNameFromPath(string filePath)
        {
            return Regex.Replace(Path.GetFileName(filePath).Replace(".mid", ""), "[^a-zA-Z0-9 -]", "").TrimStart('-',' ').TrimEnd('-',' ');
        }

        public async Task AutoSaveFiles(IEnumerable<string> filenames, bool overwrite = false, bool reorderTracks = false, bool autoEnableFlutes = false, bool clearOffsets = false)
        {
            await this.InvokeAsync(() =>
            {
                savePartitionButton.Enabled = false;
                savePartitionButton.Text = "Processing Midis...";
            }).ConfigureAwait(false);
            try
            {
                var names = filenames.ToArray();// So the first ones are first again
                await Parallel.ForEachAsync(names, async (f, cancel) =>
                {
                    var filePath = f;
                    if (!Path.IsPathRooted(filePath))
                        filePath = Path.Combine(partitionMidiPath, filePath + ".mid");
                    if (!File.Exists(filePath))
                    {
                        await LuteBotForm.Instance.HandleErrorAsync(new Exception($"File {filePath} did not exist"), $"Failed to save {f}, likely because it was saved with an old LuteBot version").ConfigureAwait(false);
                        return;
                    }
                    try
                    {
                        await SavePartition(GetPartitionNameFromPath(filePath), filePath, overwrite, reorderTracks, autoEnableFlutes, clearOffsets).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await LuteBotForm.Instance.HandleErrorAsync(ex, $"Failed to save {f}").ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await LuteBotForm.Instance.HandleErrorAsync(ex, $"Failed to save files").ConfigureAwait(false);
            }
            finally
            {
                await this.InvokeAsync(() =>
                {
                    savePartitionButton.Enabled = true;
                    savePartitionButton.Text = "Add Midis";
                }).ConfigureAwait(false);
            }
        }

        private async void renameSelectedButton_Click(object sender, EventArgs e)
        {
            if (listBoxPartitions.SelectedItems.Count == 1)
            {
                string indexName = (string)listBoxPartitions.SelectedItems[0];

                var filePath = Path.Combine(partitionMidiPath, indexName + ".mid");
                if (!File.Exists(filePath))
                {
                    var message = "Failed to rename; could not load Midi because it was saved with an old LuteBot version";
                    await LuteBotForm.Instance.HandleErrorAsync(new Exception(message), message).ConfigureAwait(false);
                    return;
                }
                var tsm = await LuteBotForm.Instance.LoadFile(filePath).ConfigureAwait(false);

                try
                {
                    var result = await SavePartitionWithForm(tsm).ConfigureAwait(false);
                    if (result)
                    {
                        var partitionIndex = index.PartitionNames.IndexOf(indexName);
                        if (partitionIndex > -1)
                            DeletePartition(partitionIndex);
                        index.SaveIndex();
                        await PopulateIndexListAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await LuteBotForm.Instance.HandleErrorAsync(ex, "Failed to save partition").ConfigureAwait(false);
                }
            }
        }
    }
}
