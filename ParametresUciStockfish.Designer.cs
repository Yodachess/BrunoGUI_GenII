// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

namespace BrunoGUI_GenII
{
    partial class ParametresUciStockfish
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParametresUciStockfish));
            labelDebugLogFile = new System.Windows.Forms.Label();
            labelNumaPolicy = new System.Windows.Forms.Label();
            labelThreads = new System.Windows.Forms.Label();
            labelHash = new System.Windows.Forms.Label();
            labelPonder = new System.Windows.Forms.Label();
            labelMultiPV = new System.Windows.Forms.Label();
            labelSkillLevel = new System.Windows.Forms.Label();
            labelMoveOverhead = new System.Windows.Forms.Label();
            labelNodesTime = new System.Windows.Forms.Label();
            labelUCI_Chess960 = new System.Windows.Forms.Label();
            labelClearHash = new System.Windows.Forms.Label();
            labelUCI_LimitStrength = new System.Windows.Forms.Label();
            labelUCI_ELO = new System.Windows.Forms.Label();
            labelUCI_ShowWDL = new System.Windows.Forms.Label();
            labelSyzygyPath = new System.Windows.Forms.Label();
            labelSyzygyProbeDepth = new System.Windows.Forms.Label();
            labelSyzygy50MoveRule = new System.Windows.Forms.Label();
            labelSyzygyProbeLimit = new System.Windows.Forms.Label();
            labelEvalFile = new System.Windows.Forms.Label();
            labelEvalFileSmall = new System.Windows.Forms.Label();
            checkBoxPonder = new System.Windows.Forms.CheckBox();
            checkBox1 = new System.Windows.Forms.CheckBox();
            checkBox2 = new System.Windows.Forms.CheckBox();
            checkBox3 = new System.Windows.Forms.CheckBox();
            checkBox4 = new System.Windows.Forms.CheckBox();
            ThreadsUpDown = new System.Windows.Forms.NumericUpDown();
            HashSizeUpDown = new System.Windows.Forms.NumericUpDown();
            MultiPVUpDown = new System.Windows.Forms.NumericUpDown();
            SkillLevelUpDown = new System.Windows.Forms.NumericUpDown();
            MoveOverheadUpDown = new System.Windows.Forms.NumericUpDown();
            NodesTimeUpDown = new System.Windows.Forms.NumericUpDown();
            UciEloUpDown = new System.Windows.Forms.NumericUpDown();
            SyzygyProbeDepthUpDown = new System.Windows.Forms.NumericUpDown();
            SyzygyProbeLimitUpDown = new System.Windows.Forms.NumericUpDown();
            NumaPolicyTextBox = new System.Windows.Forms.RichTextBox();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            richTextBox2 = new System.Windows.Forms.RichTextBox();
            richTextBox3 = new System.Windows.Forms.RichTextBox();
            richTextBox4 = new System.Windows.Forms.RichTextBox();
            ClearHashButton = new Krypton.Toolkit.KryptonButton();
            ParametresFermer = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)ThreadsUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)HashSizeUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MultiPVUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SkillLevelUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MoveOverheadUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NodesTimeUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)UciEloUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SyzygyProbeDepthUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SyzygyProbeLimitUpDown).BeginInit();
            SuspendLayout();
            // 
            // labelDebugLogFile
            // 
            labelDebugLogFile.BackColor = System.Drawing.Color.Lavender;
            labelDebugLogFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelDebugLogFile.Location = new System.Drawing.Point(12, 12);
            labelDebugLogFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDebugLogFile.Name = "labelDebugLogFile";
            labelDebugLogFile.Size = new System.Drawing.Size(210, 23);
            labelDebugLogFile.TabIndex = 0;
            labelDebugLogFile.Text = "Debug Log ";
            labelDebugLogFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNumaPolicy
            // 
            labelNumaPolicy.BackColor = System.Drawing.Color.Lavender;
            labelNumaPolicy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelNumaPolicy.Location = new System.Drawing.Point(408, 12);
            labelNumaPolicy.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelNumaPolicy.Name = "labelNumaPolicy";
            labelNumaPolicy.Size = new System.Drawing.Size(210, 23);
            labelNumaPolicy.TabIndex = 1;
            labelNumaPolicy.Text = "Numa Policy ";
            labelNumaPolicy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelThreads
            // 
            labelThreads.BackColor = System.Drawing.Color.Lavender;
            labelThreads.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelThreads.Location = new System.Drawing.Point(12, 40);
            labelThreads.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelThreads.Name = "labelThreads";
            labelThreads.Size = new System.Drawing.Size(210, 23);
            labelThreads.TabIndex = 2;
            labelThreads.Text = "Threads (1 à 1024)";
            labelThreads.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelHash
            // 
            labelHash.BackColor = System.Drawing.Color.Lavender;
            labelHash.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelHash.Location = new System.Drawing.Point(12, 69);
            labelHash.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelHash.Name = "labelHash";
            labelHash.Size = new System.Drawing.Size(210, 23);
            labelHash.TabIndex = 3;
            labelHash.Text = "Hash Size (1 à 33554432)";
            labelHash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelPonder
            // 
            labelPonder.BackColor = System.Drawing.Color.Lavender;
            labelPonder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelPonder.Location = new System.Drawing.Point(408, 40);
            labelPonder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelPonder.Name = "labelPonder";
            labelPonder.Size = new System.Drawing.Size(210, 23);
            labelPonder.TabIndex = 4;
            labelPonder.Text = "Ponder";
            labelPonder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMultiPV
            // 
            labelMultiPV.BackColor = System.Drawing.Color.Lavender;
            labelMultiPV.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelMultiPV.Location = new System.Drawing.Point(12, 127);
            labelMultiPV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelMultiPV.Name = "labelMultiPV";
            labelMultiPV.Size = new System.Drawing.Size(210, 23);
            labelMultiPV.TabIndex = 5;
            labelMultiPV.Text = "MultiPV (1 à 256)";
            labelMultiPV.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelSkillLevel
            // 
            labelSkillLevel.BackColor = System.Drawing.Color.Lavender;
            labelSkillLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelSkillLevel.Location = new System.Drawing.Point(12, 156);
            labelSkillLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSkillLevel.Name = "labelSkillLevel";
            labelSkillLevel.Size = new System.Drawing.Size(210, 23);
            labelSkillLevel.TabIndex = 6;
            labelSkillLevel.Text = "SkillLevel (0 à 20)";
            labelSkillLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMoveOverhead
            // 
            labelMoveOverhead.BackColor = System.Drawing.Color.Lavender;
            labelMoveOverhead.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelMoveOverhead.Location = new System.Drawing.Point(12, 185);
            labelMoveOverhead.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelMoveOverhead.Name = "labelMoveOverhead";
            labelMoveOverhead.Size = new System.Drawing.Size(210, 23);
            labelMoveOverhead.TabIndex = 7;
            labelMoveOverhead.Text = "Move Overhead (0 à 5000)";
            labelMoveOverhead.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNodesTime
            // 
            labelNodesTime.BackColor = System.Drawing.Color.Lavender;
            labelNodesTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelNodesTime.Location = new System.Drawing.Point(12, 213);
            labelNodesTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelNodesTime.Name = "labelNodesTime";
            labelNodesTime.Size = new System.Drawing.Size(210, 23);
            labelNodesTime.TabIndex = 8;
            labelNodesTime.Text = "Nodestime (0 à 10000)";
            labelNodesTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUCI_Chess960
            // 
            labelUCI_Chess960.BackColor = System.Drawing.Color.Lavender;
            labelUCI_Chess960.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelUCI_Chess960.Location = new System.Drawing.Point(408, 69);
            labelUCI_Chess960.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelUCI_Chess960.Name = "labelUCI_Chess960";
            labelUCI_Chess960.Size = new System.Drawing.Size(210, 23);
            labelUCI_Chess960.TabIndex = 9;
            labelUCI_Chess960.Text = "UCI_Chess960 ";
            labelUCI_Chess960.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelClearHash
            // 
            labelClearHash.BackColor = System.Drawing.Color.Lavender;
            labelClearHash.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelClearHash.Location = new System.Drawing.Point(12, 98);
            labelClearHash.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelClearHash.Name = "labelClearHash";
            labelClearHash.Size = new System.Drawing.Size(210, 23);
            labelClearHash.TabIndex = 10;
            labelClearHash.Text = "Clear Hash";
            labelClearHash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUCI_LimitStrength
            // 
            labelUCI_LimitStrength.BackColor = System.Drawing.Color.Lavender;
            labelUCI_LimitStrength.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelUCI_LimitStrength.Location = new System.Drawing.Point(12, 242);
            labelUCI_LimitStrength.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelUCI_LimitStrength.Name = "labelUCI_LimitStrength";
            labelUCI_LimitStrength.Size = new System.Drawing.Size(210, 23);
            labelUCI_LimitStrength.TabIndex = 11;
            labelUCI_LimitStrength.Text = "UCI_LimitStrength";
            // 
            // labelUCI_ELO
            // 
            labelUCI_ELO.BackColor = System.Drawing.Color.Lavender;
            labelUCI_ELO.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelUCI_ELO.Location = new System.Drawing.Point(12, 271);
            labelUCI_ELO.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelUCI_ELO.Name = "labelUCI_ELO";
            labelUCI_ELO.Size = new System.Drawing.Size(210, 23);
            labelUCI_ELO.TabIndex = 12;
            labelUCI_ELO.Text = "UCI_ELO (1320 à 3190)";
            labelUCI_ELO.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUCI_ShowWDL
            // 
            labelUCI_ShowWDL.BackColor = System.Drawing.Color.Lavender;
            labelUCI_ShowWDL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelUCI_ShowWDL.Location = new System.Drawing.Point(408, 98);
            labelUCI_ShowWDL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelUCI_ShowWDL.Name = "labelUCI_ShowWDL";
            labelUCI_ShowWDL.Size = new System.Drawing.Size(210, 23);
            labelUCI_ShowWDL.TabIndex = 13;
            labelUCI_ShowWDL.Text = "UCI_ShowWDL";
            // 
            // labelSyzygyPath
            // 
            labelSyzygyPath.BackColor = System.Drawing.Color.Lavender;
            labelSyzygyPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelSyzygyPath.Location = new System.Drawing.Point(408, 127);
            labelSyzygyPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSyzygyPath.Name = "labelSyzygyPath";
            labelSyzygyPath.Size = new System.Drawing.Size(210, 23);
            labelSyzygyPath.TabIndex = 14;
            labelSyzygyPath.Text = "SyzygyPath";
            // 
            // labelSyzygyProbeDepth
            // 
            labelSyzygyProbeDepth.BackColor = System.Drawing.Color.Lavender;
            labelSyzygyProbeDepth.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelSyzygyProbeDepth.Location = new System.Drawing.Point(408, 156);
            labelSyzygyProbeDepth.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSyzygyProbeDepth.Name = "labelSyzygyProbeDepth";
            labelSyzygyProbeDepth.Size = new System.Drawing.Size(210, 23);
            labelSyzygyProbeDepth.TabIndex = 15;
            labelSyzygyProbeDepth.Text = "SyzygyProbeDepth (1 à 100)";
            // 
            // labelSyzygy50MoveRule
            // 
            labelSyzygy50MoveRule.BackColor = System.Drawing.Color.Lavender;
            labelSyzygy50MoveRule.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelSyzygy50MoveRule.Location = new System.Drawing.Point(408, 185);
            labelSyzygy50MoveRule.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSyzygy50MoveRule.Name = "labelSyzygy50MoveRule";
            labelSyzygy50MoveRule.Size = new System.Drawing.Size(210, 23);
            labelSyzygy50MoveRule.TabIndex = 16;
            labelSyzygy50MoveRule.Text = "Syzygy50MoveRule";
            // 
            // labelSyzygyProbeLimit
            // 
            labelSyzygyProbeLimit.BackColor = System.Drawing.Color.Lavender;
            labelSyzygyProbeLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelSyzygyProbeLimit.Location = new System.Drawing.Point(408, 213);
            labelSyzygyProbeLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelSyzygyProbeLimit.Name = "labelSyzygyProbeLimit";
            labelSyzygyProbeLimit.Size = new System.Drawing.Size(210, 23);
            labelSyzygyProbeLimit.TabIndex = 17;
            labelSyzygyProbeLimit.Text = "SyzygyProbeLimit (0 à 7)";
            // 
            // labelEvalFile
            // 
            labelEvalFile.BackColor = System.Drawing.Color.Lavender;
            labelEvalFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelEvalFile.Location = new System.Drawing.Point(408, 242);
            labelEvalFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelEvalFile.Name = "labelEvalFile";
            labelEvalFile.Size = new System.Drawing.Size(210, 23);
            labelEvalFile.TabIndex = 18;
            labelEvalFile.Text = "EvalFile";
            // 
            // labelEvalFileSmall
            // 
            labelEvalFileSmall.BackColor = System.Drawing.Color.Lavender;
            labelEvalFileSmall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelEvalFileSmall.Location = new System.Drawing.Point(408, 271);
            labelEvalFileSmall.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelEvalFileSmall.Name = "labelEvalFileSmall";
            labelEvalFileSmall.Size = new System.Drawing.Size(210, 23);
            labelEvalFileSmall.TabIndex = 19;
            labelEvalFileSmall.Text = "EvalFileSmall";
            // 
            // checkBoxPonder
            // 
            checkBoxPonder.AutoSize = true;
            checkBoxPonder.BackColor = System.Drawing.Color.DarkBlue;
            checkBoxPonder.Location = new System.Drawing.Point(630, 43);
            checkBoxPonder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxPonder.Name = "checkBoxPonder";
            checkBoxPonder.Size = new System.Drawing.Size(15, 14);
            checkBoxPonder.TabIndex = 20;
            checkBoxPonder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBoxPonder.UseVisualStyleBackColor = false;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.BackColor = System.Drawing.Color.DarkBlue;
            checkBox1.Enabled = false;
            checkBox1.Location = new System.Drawing.Point(630, 72);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(15, 14);
            checkBox1.TabIndex = 21;
            checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBox1.UseVisualStyleBackColor = false;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.BackColor = System.Drawing.Color.DarkBlue;
            checkBox2.Enabled = false;
            checkBox2.Location = new System.Drawing.Point(630, 100);
            checkBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(15, 14);
            checkBox2.TabIndex = 22;
            checkBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBox2.UseVisualStyleBackColor = false;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.BackColor = System.Drawing.Color.DarkBlue;
            checkBox3.Checked = true;
            checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox3.Enabled = false;
            checkBox3.Location = new System.Drawing.Point(630, 187);
            checkBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new System.Drawing.Size(15, 14);
            checkBox3.TabIndex = 23;
            checkBox3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBox3.UseVisualStyleBackColor = false;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.BackColor = System.Drawing.Color.DarkBlue;
            checkBox4.Enabled = false;
            checkBox4.Location = new System.Drawing.Point(233, 247);
            checkBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new System.Drawing.Size(15, 14);
            checkBox4.TabIndex = 24;
            checkBox4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBox4.UseVisualStyleBackColor = false;
            // 
            // ThreadsUpDown
            // 
            ThreadsUpDown.Location = new System.Drawing.Point(233, 40);
            ThreadsUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ThreadsUpDown.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            ThreadsUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            ThreadsUpDown.Name = "ThreadsUpDown";
            ThreadsUpDown.Size = new System.Drawing.Size(140, 23);
            ThreadsUpDown.TabIndex = 25;
            ThreadsUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // HashSizeUpDown
            // 
            HashSizeUpDown.Location = new System.Drawing.Point(233, 69);
            HashSizeUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            HashSizeUpDown.Maximum = new decimal(new int[] { 33554432, 0, 0, 0 });
            HashSizeUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            HashSizeUpDown.Name = "HashSizeUpDown";
            HashSizeUpDown.Size = new System.Drawing.Size(140, 23);
            HashSizeUpDown.TabIndex = 26;
            HashSizeUpDown.Value = new decimal(new int[] { 16, 0, 0, 0 });
            // 
            // MultiPVUpDown
            // 
            MultiPVUpDown.Location = new System.Drawing.Point(233, 127);
            MultiPVUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MultiPVUpDown.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            MultiPVUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MultiPVUpDown.Name = "MultiPVUpDown";
            MultiPVUpDown.Size = new System.Drawing.Size(140, 23);
            MultiPVUpDown.TabIndex = 27;
            MultiPVUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // SkillLevelUpDown
            // 
            SkillLevelUpDown.Location = new System.Drawing.Point(233, 156);
            SkillLevelUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SkillLevelUpDown.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            SkillLevelUpDown.Name = "SkillLevelUpDown";
            SkillLevelUpDown.Size = new System.Drawing.Size(140, 23);
            SkillLevelUpDown.TabIndex = 28;
            SkillLevelUpDown.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // MoveOverheadUpDown
            // 
            MoveOverheadUpDown.Location = new System.Drawing.Point(233, 185);
            MoveOverheadUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MoveOverheadUpDown.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            MoveOverheadUpDown.Name = "MoveOverheadUpDown";
            MoveOverheadUpDown.Size = new System.Drawing.Size(140, 23);
            MoveOverheadUpDown.TabIndex = 29;
            MoveOverheadUpDown.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // NodesTimeUpDown
            // 
            NodesTimeUpDown.Location = new System.Drawing.Point(233, 213);
            NodesTimeUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NodesTimeUpDown.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            NodesTimeUpDown.Name = "NodesTimeUpDown";
            NodesTimeUpDown.Size = new System.Drawing.Size(140, 23);
            NodesTimeUpDown.TabIndex = 30;
            // 
            // UciEloUpDown
            // 
            UciEloUpDown.Enabled = false;
            UciEloUpDown.Location = new System.Drawing.Point(233, 271);
            UciEloUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            UciEloUpDown.Maximum = new decimal(new int[] { 3190, 0, 0, 0 });
            UciEloUpDown.Minimum = new decimal(new int[] { 1320, 0, 0, 0 });
            UciEloUpDown.Name = "UciEloUpDown";
            UciEloUpDown.Size = new System.Drawing.Size(140, 23);
            UciEloUpDown.TabIndex = 31;
            UciEloUpDown.Value = new decimal(new int[] { 1320, 0, 0, 0 });
            // 
            // SyzygyProbeDepthUpDown
            // 
            SyzygyProbeDepthUpDown.Enabled = false;
            SyzygyProbeDepthUpDown.Location = new System.Drawing.Point(630, 156);
            SyzygyProbeDepthUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SyzygyProbeDepthUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            SyzygyProbeDepthUpDown.Name = "SyzygyProbeDepthUpDown";
            SyzygyProbeDepthUpDown.Size = new System.Drawing.Size(140, 23);
            SyzygyProbeDepthUpDown.TabIndex = 32;
            SyzygyProbeDepthUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // SyzygyProbeLimitUpDown
            // 
            SyzygyProbeLimitUpDown.Enabled = false;
            SyzygyProbeLimitUpDown.Location = new System.Drawing.Point(630, 213);
            SyzygyProbeLimitUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SyzygyProbeLimitUpDown.Maximum = new decimal(new int[] { 7, 0, 0, 0 });
            SyzygyProbeLimitUpDown.Name = "SyzygyProbeLimitUpDown";
            SyzygyProbeLimitUpDown.Size = new System.Drawing.Size(140, 23);
            SyzygyProbeLimitUpDown.TabIndex = 33;
            SyzygyProbeLimitUpDown.Value = new decimal(new int[] { 7, 0, 0, 0 });
            // 
            // NumaPolicyTextBox
            // 
            NumaPolicyTextBox.Enabled = false;
            NumaPolicyTextBox.Location = new System.Drawing.Point(630, 12);
            NumaPolicyTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NumaPolicyTextBox.Name = "NumaPolicyTextBox";
            NumaPolicyTextBox.Size = new System.Drawing.Size(186, 22);
            NumaPolicyTextBox.TabIndex = 34;
            NumaPolicyTextBox.Text = "auto";
            // 
            // richTextBox1
            // 
            richTextBox1.Enabled = false;
            richTextBox1.Location = new System.Drawing.Point(630, 242);
            richTextBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(186, 22);
            richTextBox1.TabIndex = 35;
            richTextBox1.Text = "nn-1111cefa1111.nnue";
            // 
            // richTextBox2
            // 
            richTextBox2.Enabled = false;
            richTextBox2.Location = new System.Drawing.Point(630, 271);
            richTextBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new System.Drawing.Size(186, 22);
            richTextBox2.TabIndex = 36;
            richTextBox2.Text = "nn-37f18f62d772.nnue";
            // 
            // richTextBox3
            // 
            richTextBox3.Enabled = false;
            richTextBox3.Location = new System.Drawing.Point(630, 127);
            richTextBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBox3.Name = "richTextBox3";
            richTextBox3.Size = new System.Drawing.Size(186, 22);
            richTextBox3.TabIndex = 37;
            richTextBox3.Text = "<empty>";
            // 
            // richTextBox4
            // 
            richTextBox4.Enabled = false;
            richTextBox4.Location = new System.Drawing.Point(233, 12);
            richTextBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBox4.Name = "richTextBox4";
            richTextBox4.Size = new System.Drawing.Size(116, 22);
            richTextBox4.TabIndex = 38;
            richTextBox4.Text = "<empty>";
            // 
            // ClearHashButton
            // 
            ClearHashButton.Location = new System.Drawing.Point(233, 98);
            ClearHashButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ClearHashButton.Name = "ClearHashButton";
            ClearHashButton.Size = new System.Drawing.Size(140, 23);
            ClearHashButton.StateNormal.Back.Color1 = System.Drawing.Color.Lavender;
            ClearHashButton.StateNormal.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ClearHashButton.StateNormal.Border.Rounding = 20F;
            ClearHashButton.StateNormal.Border.Width = 1;
            ClearHashButton.TabIndex = 39;
            ClearHashButton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            ClearHashButton.Values.Text = "Vide Hash Tables";
            ClearHashButton.Click += ClearHashButton_Click;
            // 
            // ParametresFermer
            // 
            ParametresFermer.Location = new System.Drawing.Point(315, 300);
            ParametresFermer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ParametresFermer.Name = "ParametresFermer";
            ParametresFermer.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            ParametresFermer.Size = new System.Drawing.Size(140, 23);
            ParametresFermer.StateNormal.Back.Color1 = System.Drawing.Color.Lavender;
            ParametresFermer.StateNormal.Border.Color1 = System.Drawing.Color.Gray;
            ParametresFermer.StateNormal.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ParametresFermer.StateNormal.Border.Rounding = 20F;
            ParametresFermer.StateNormal.Border.Width = 3;
            ParametresFermer.TabIndex = 40;
            ParametresFermer.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            ParametresFermer.Values.Text = "Fermer";
            ParametresFermer.Click += ParametresFermer_Click;
            // 
            // ParametresUciStockfish
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.LightSteelBlue;
            ClientSize = new System.Drawing.Size(862, 328);
            Controls.Add(ParametresFermer);
            Controls.Add(ClearHashButton);
            Controls.Add(richTextBox4);
            Controls.Add(richTextBox3);
            Controls.Add(richTextBox2);
            Controls.Add(richTextBox1);
            Controls.Add(NumaPolicyTextBox);
            Controls.Add(SyzygyProbeLimitUpDown);
            Controls.Add(SyzygyProbeDepthUpDown);
            Controls.Add(UciEloUpDown);
            Controls.Add(NodesTimeUpDown);
            Controls.Add(MoveOverheadUpDown);
            Controls.Add(SkillLevelUpDown);
            Controls.Add(MultiPVUpDown);
            Controls.Add(HashSizeUpDown);
            Controls.Add(ThreadsUpDown);
            Controls.Add(checkBox4);
            Controls.Add(checkBox3);
            Controls.Add(checkBox2);
            Controls.Add(checkBox1);
            Controls.Add(checkBoxPonder);
            Controls.Add(labelEvalFileSmall);
            Controls.Add(labelEvalFile);
            Controls.Add(labelSyzygyProbeLimit);
            Controls.Add(labelSyzygy50MoveRule);
            Controls.Add(labelSyzygyProbeDepth);
            Controls.Add(labelSyzygyPath);
            Controls.Add(labelUCI_ShowWDL);
            Controls.Add(labelUCI_ELO);
            Controls.Add(labelUCI_LimitStrength);
            Controls.Add(labelClearHash);
            Controls.Add(labelUCI_Chess960);
            Controls.Add(labelNodesTime);
            Controls.Add(labelMoveOverhead);
            Controls.Add(labelSkillLevel);
            Controls.Add(labelMultiPV);
            Controls.Add(labelPonder);
            Controls.Add(labelHash);
            Controls.Add(labelThreads);
            Controls.Add(labelNumaPolicy);
            Controls.Add(labelDebugLogFile);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ParametresUciStockfish";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Parametres du Moteur Uci Stockfish";
            ((System.ComponentModel.ISupportInitialize)ThreadsUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)HashSizeUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)MultiPVUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)SkillLevelUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)MoveOverheadUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)NodesTimeUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)UciEloUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)SyzygyProbeDepthUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)SyzygyProbeLimitUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDebugLogFile;
        private System.Windows.Forms.Label labelNumaPolicy;
        private System.Windows.Forms.Label labelThreads;
        private System.Windows.Forms.Label labelHash;
        private System.Windows.Forms.Label labelPonder;
        private System.Windows.Forms.Label labelMultiPV;
        private System.Windows.Forms.Label labelSkillLevel;
        private System.Windows.Forms.Label labelMoveOverhead;
        private System.Windows.Forms.Label labelNodesTime;
        private System.Windows.Forms.Label labelUCI_Chess960;
        private System.Windows.Forms.Label labelClearHash;
        private System.Windows.Forms.Label labelUCI_LimitStrength;
        private System.Windows.Forms.Label labelUCI_ELO;
        private System.Windows.Forms.Label labelUCI_ShowWDL;
        private System.Windows.Forms.Label labelSyzygyPath;
        private System.Windows.Forms.Label labelSyzygyProbeDepth;
        private System.Windows.Forms.Label labelSyzygy50MoveRule;
        private System.Windows.Forms.Label labelSyzygyProbeLimit;
        private System.Windows.Forms.Label labelEvalFile;
        private System.Windows.Forms.Label labelEvalFileSmall;
        private System.Windows.Forms.CheckBox checkBoxPonder;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.NumericUpDown ThreadsUpDown;
        private System.Windows.Forms.NumericUpDown HashSizeUpDown;
        private System.Windows.Forms.NumericUpDown MultiPVUpDown;
        private System.Windows.Forms.NumericUpDown SkillLevelUpDown;
        private System.Windows.Forms.NumericUpDown MoveOverheadUpDown;
        private System.Windows.Forms.NumericUpDown NodesTimeUpDown;
        private System.Windows.Forms.NumericUpDown UciEloUpDown;
        private System.Windows.Forms.NumericUpDown SyzygyProbeDepthUpDown;
        private System.Windows.Forms.NumericUpDown SyzygyProbeLimitUpDown;
        private System.Windows.Forms.RichTextBox NumaPolicyTextBox;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.RichTextBox richTextBox3;
        private System.Windows.Forms.RichTextBox richTextBox4;
        private Krypton.Toolkit.KryptonButton ClearHashButton;
        private Krypton.Toolkit.KryptonButton ParametresFermer;
    }
}