namespace BrunoGUI_Stockfish
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
            this.labelDebugLogFile = new System.Windows.Forms.Label();
            this.labelNumaPolicy = new System.Windows.Forms.Label();
            this.labelThreads = new System.Windows.Forms.Label();
            this.labelHash = new System.Windows.Forms.Label();
            this.labelPonder = new System.Windows.Forms.Label();
            this.labelMultiPV = new System.Windows.Forms.Label();
            this.labelSkillLevel = new System.Windows.Forms.Label();
            this.labelMoveOverhead = new System.Windows.Forms.Label();
            this.labelNodesTime = new System.Windows.Forms.Label();
            this.labelUCI_Chess960 = new System.Windows.Forms.Label();
            this.labelClearHash = new System.Windows.Forms.Label();
            this.labelUCI_LimitStrength = new System.Windows.Forms.Label();
            this.labelUCI_ELO = new System.Windows.Forms.Label();
            this.labelUCI_ShowWDL = new System.Windows.Forms.Label();
            this.labelSyzygyPath = new System.Windows.Forms.Label();
            this.labelSyzygyProbeDepth = new System.Windows.Forms.Label();
            this.labelSyzygy50MoveRule = new System.Windows.Forms.Label();
            this.labelSyzygyProbeLimit = new System.Windows.Forms.Label();
            this.labelEvalFile = new System.Windows.Forms.Label();
            this.labelEvalFileSmall = new System.Windows.Forms.Label();
            this.checkBoxPonder = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.ThreadsUpDown = new System.Windows.Forms.NumericUpDown();
            this.HashSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.MultiPVUpDown = new System.Windows.Forms.NumericUpDown();
            this.SkillLevelUpDown = new System.Windows.Forms.NumericUpDown();
            this.MoveOverheadUpDown = new System.Windows.Forms.NumericUpDown();
            this.NodesTimeUpDown = new System.Windows.Forms.NumericUpDown();
            this.UciEloUpDown = new System.Windows.Forms.NumericUpDown();
            this.SyzygyProbeDepthUpDown = new System.Windows.Forms.NumericUpDown();
            this.SyzygyProbeLimitUpDown = new System.Windows.Forms.NumericUpDown();
            this.NumaPolicyTextBox = new System.Windows.Forms.RichTextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.richTextBox3 = new System.Windows.Forms.RichTextBox();
            this.richTextBox4 = new System.Windows.Forms.RichTextBox();
            this.ClearHashButton = new Krypton.Toolkit.KryptonButton();
            this.ParametresFermer = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.ThreadsUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HashSizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MultiPVUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SkillLevelUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MoveOverheadUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NodesTimeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UciEloUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SyzygyProbeDepthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SyzygyProbeLimitUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // labelDebugLogFile
            // 
            this.labelDebugLogFile.BackColor = System.Drawing.Color.Lavender;
            this.labelDebugLogFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDebugLogFile.Location = new System.Drawing.Point(10, 10);
            this.labelDebugLogFile.Name = "labelDebugLogFile";
            this.labelDebugLogFile.Size = new System.Drawing.Size(180, 20);
            this.labelDebugLogFile.TabIndex = 0;
            this.labelDebugLogFile.Text = "Debug Log ";
            this.labelDebugLogFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNumaPolicy
            // 
            this.labelNumaPolicy.BackColor = System.Drawing.Color.Lavender;
            this.labelNumaPolicy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNumaPolicy.Location = new System.Drawing.Point(350, 10);
            this.labelNumaPolicy.Name = "labelNumaPolicy";
            this.labelNumaPolicy.Size = new System.Drawing.Size(180, 20);
            this.labelNumaPolicy.TabIndex = 1;
            this.labelNumaPolicy.Text = "Numa Policy ";
            this.labelNumaPolicy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelThreads
            // 
            this.labelThreads.BackColor = System.Drawing.Color.Lavender;
            this.labelThreads.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelThreads.Location = new System.Drawing.Point(10, 35);
            this.labelThreads.Name = "labelThreads";
            this.labelThreads.Size = new System.Drawing.Size(180, 20);
            this.labelThreads.TabIndex = 2;
            this.labelThreads.Text = "Threads (1 à 1024)";
            this.labelThreads.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelHash
            // 
            this.labelHash.BackColor = System.Drawing.Color.Lavender;
            this.labelHash.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHash.Location = new System.Drawing.Point(10, 60);
            this.labelHash.Name = "labelHash";
            this.labelHash.Size = new System.Drawing.Size(180, 20);
            this.labelHash.TabIndex = 3;
            this.labelHash.Text = "Hash Size (1 à 33554432)";
            this.labelHash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelPonder
            // 
            this.labelPonder.BackColor = System.Drawing.Color.Lavender;
            this.labelPonder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPonder.Location = new System.Drawing.Point(350, 35);
            this.labelPonder.Name = "labelPonder";
            this.labelPonder.Size = new System.Drawing.Size(180, 20);
            this.labelPonder.TabIndex = 4;
            this.labelPonder.Text = "Ponder";
            this.labelPonder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMultiPV
            // 
            this.labelMultiPV.BackColor = System.Drawing.Color.Lavender;
            this.labelMultiPV.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMultiPV.Location = new System.Drawing.Point(10, 110);
            this.labelMultiPV.Name = "labelMultiPV";
            this.labelMultiPV.Size = new System.Drawing.Size(180, 20);
            this.labelMultiPV.TabIndex = 5;
            this.labelMultiPV.Text = "MultiPV (1 à 256)";
            this.labelMultiPV.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelSkillLevel
            // 
            this.labelSkillLevel.BackColor = System.Drawing.Color.Lavender;
            this.labelSkillLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSkillLevel.Location = new System.Drawing.Point(10, 135);
            this.labelSkillLevel.Name = "labelSkillLevel";
            this.labelSkillLevel.Size = new System.Drawing.Size(180, 20);
            this.labelSkillLevel.TabIndex = 6;
            this.labelSkillLevel.Text = "SkillLevel (0 à 20)";
            this.labelSkillLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMoveOverhead
            // 
            this.labelMoveOverhead.BackColor = System.Drawing.Color.Lavender;
            this.labelMoveOverhead.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMoveOverhead.Location = new System.Drawing.Point(10, 160);
            this.labelMoveOverhead.Name = "labelMoveOverhead";
            this.labelMoveOverhead.Size = new System.Drawing.Size(180, 20);
            this.labelMoveOverhead.TabIndex = 7;
            this.labelMoveOverhead.Text = "Move Overhead (0 à 5000)";
            this.labelMoveOverhead.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNodesTime
            // 
            this.labelNodesTime.BackColor = System.Drawing.Color.Lavender;
            this.labelNodesTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNodesTime.Location = new System.Drawing.Point(10, 185);
            this.labelNodesTime.Name = "labelNodesTime";
            this.labelNodesTime.Size = new System.Drawing.Size(180, 20);
            this.labelNodesTime.TabIndex = 8;
            this.labelNodesTime.Text = "Nodestime (0 à 10000)";
            this.labelNodesTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUCI_Chess960
            // 
            this.labelUCI_Chess960.BackColor = System.Drawing.Color.Lavender;
            this.labelUCI_Chess960.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUCI_Chess960.Location = new System.Drawing.Point(350, 60);
            this.labelUCI_Chess960.Name = "labelUCI_Chess960";
            this.labelUCI_Chess960.Size = new System.Drawing.Size(180, 20);
            this.labelUCI_Chess960.TabIndex = 9;
            this.labelUCI_Chess960.Text = "UCI_Chess960 ";
            this.labelUCI_Chess960.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelClearHash
            // 
            this.labelClearHash.BackColor = System.Drawing.Color.Lavender;
            this.labelClearHash.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelClearHash.Location = new System.Drawing.Point(10, 85);
            this.labelClearHash.Name = "labelClearHash";
            this.labelClearHash.Size = new System.Drawing.Size(180, 20);
            this.labelClearHash.TabIndex = 10;
            this.labelClearHash.Text = "Clear Hash";
            this.labelClearHash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUCI_LimitStrength
            // 
            this.labelUCI_LimitStrength.BackColor = System.Drawing.Color.Lavender;
            this.labelUCI_LimitStrength.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUCI_LimitStrength.Location = new System.Drawing.Point(10, 210);
            this.labelUCI_LimitStrength.Name = "labelUCI_LimitStrength";
            this.labelUCI_LimitStrength.Size = new System.Drawing.Size(180, 20);
            this.labelUCI_LimitStrength.TabIndex = 11;
            this.labelUCI_LimitStrength.Text = "UCI_LimitStrength";
            // 
            // labelUCI_ELO
            // 
            this.labelUCI_ELO.BackColor = System.Drawing.Color.Lavender;
            this.labelUCI_ELO.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUCI_ELO.Location = new System.Drawing.Point(10, 235);
            this.labelUCI_ELO.Name = "labelUCI_ELO";
            this.labelUCI_ELO.Size = new System.Drawing.Size(180, 20);
            this.labelUCI_ELO.TabIndex = 12;
            this.labelUCI_ELO.Text = "UCI_ELO (1320 à 3190)";
            this.labelUCI_ELO.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelUCI_ShowWDL
            // 
            this.labelUCI_ShowWDL.BackColor = System.Drawing.Color.Lavender;
            this.labelUCI_ShowWDL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelUCI_ShowWDL.Location = new System.Drawing.Point(350, 85);
            this.labelUCI_ShowWDL.Name = "labelUCI_ShowWDL";
            this.labelUCI_ShowWDL.Size = new System.Drawing.Size(180, 20);
            this.labelUCI_ShowWDL.TabIndex = 13;
            this.labelUCI_ShowWDL.Text = "UCI_ShowWDL";
            // 
            // labelSyzygyPath
            // 
            this.labelSyzygyPath.BackColor = System.Drawing.Color.Lavender;
            this.labelSyzygyPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSyzygyPath.Location = new System.Drawing.Point(350, 110);
            this.labelSyzygyPath.Name = "labelSyzygyPath";
            this.labelSyzygyPath.Size = new System.Drawing.Size(180, 20);
            this.labelSyzygyPath.TabIndex = 14;
            this.labelSyzygyPath.Text = "SyzygyPath";
            // 
            // labelSyzygyProbeDepth
            // 
            this.labelSyzygyProbeDepth.BackColor = System.Drawing.Color.Lavender;
            this.labelSyzygyProbeDepth.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSyzygyProbeDepth.Location = new System.Drawing.Point(350, 135);
            this.labelSyzygyProbeDepth.Name = "labelSyzygyProbeDepth";
            this.labelSyzygyProbeDepth.Size = new System.Drawing.Size(180, 20);
            this.labelSyzygyProbeDepth.TabIndex = 15;
            this.labelSyzygyProbeDepth.Text = "SyzygyProbeDepth (1 à 100)";
            // 
            // labelSyzygy50MoveRule
            // 
            this.labelSyzygy50MoveRule.BackColor = System.Drawing.Color.Lavender;
            this.labelSyzygy50MoveRule.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSyzygy50MoveRule.Location = new System.Drawing.Point(350, 160);
            this.labelSyzygy50MoveRule.Name = "labelSyzygy50MoveRule";
            this.labelSyzygy50MoveRule.Size = new System.Drawing.Size(180, 20);
            this.labelSyzygy50MoveRule.TabIndex = 16;
            this.labelSyzygy50MoveRule.Text = "Syzygy50MoveRule";
            // 
            // labelSyzygyProbeLimit
            // 
            this.labelSyzygyProbeLimit.BackColor = System.Drawing.Color.Lavender;
            this.labelSyzygyProbeLimit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSyzygyProbeLimit.Location = new System.Drawing.Point(350, 185);
            this.labelSyzygyProbeLimit.Name = "labelSyzygyProbeLimit";
            this.labelSyzygyProbeLimit.Size = new System.Drawing.Size(180, 20);
            this.labelSyzygyProbeLimit.TabIndex = 17;
            this.labelSyzygyProbeLimit.Text = "SyzygyProbeLimit (0 à 7)";
            // 
            // labelEvalFile
            // 
            this.labelEvalFile.BackColor = System.Drawing.Color.Lavender;
            this.labelEvalFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEvalFile.Location = new System.Drawing.Point(350, 210);
            this.labelEvalFile.Name = "labelEvalFile";
            this.labelEvalFile.Size = new System.Drawing.Size(180, 20);
            this.labelEvalFile.TabIndex = 18;
            this.labelEvalFile.Text = "EvalFile";
            // 
            // labelEvalFileSmall
            // 
            this.labelEvalFileSmall.BackColor = System.Drawing.Color.Lavender;
            this.labelEvalFileSmall.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEvalFileSmall.Location = new System.Drawing.Point(350, 235);
            this.labelEvalFileSmall.Name = "labelEvalFileSmall";
            this.labelEvalFileSmall.Size = new System.Drawing.Size(180, 20);
            this.labelEvalFileSmall.TabIndex = 19;
            this.labelEvalFileSmall.Text = "EvalFileSmall";
            // 
            // checkBoxPonder
            // 
            this.checkBoxPonder.AutoSize = true;
            this.checkBoxPonder.BackColor = System.Drawing.Color.DarkBlue;
            this.checkBoxPonder.Location = new System.Drawing.Point(540, 37);
            this.checkBoxPonder.Name = "checkBoxPonder";
            this.checkBoxPonder.Size = new System.Drawing.Size(15, 14);
            this.checkBoxPonder.TabIndex = 20;
            this.checkBoxPonder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxPonder.UseVisualStyleBackColor = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.Color.DarkBlue;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(540, 62);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 21;
            this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = false;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.BackColor = System.Drawing.Color.DarkBlue;
            this.checkBox2.Enabled = false;
            this.checkBox2.Location = new System.Drawing.Point(540, 87);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(15, 14);
            this.checkBox2.TabIndex = 22;
            this.checkBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox2.UseVisualStyleBackColor = false;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.BackColor = System.Drawing.Color.DarkBlue;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Enabled = false;
            this.checkBox3.Location = new System.Drawing.Point(540, 162);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(15, 14);
            this.checkBox3.TabIndex = 23;
            this.checkBox3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox3.UseVisualStyleBackColor = false;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.BackColor = System.Drawing.Color.DarkBlue;
            this.checkBox4.Enabled = false;
            this.checkBox4.Location = new System.Drawing.Point(200, 214);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(15, 14);
            this.checkBox4.TabIndex = 24;
            this.checkBox4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox4.UseVisualStyleBackColor = false;
            // 
            // ThreadsUpDown
            // 
            this.ThreadsUpDown.Location = new System.Drawing.Point(200, 35);
            this.ThreadsUpDown.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.ThreadsUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ThreadsUpDown.Name = "ThreadsUpDown";
            this.ThreadsUpDown.Size = new System.Drawing.Size(120, 20);
            this.ThreadsUpDown.TabIndex = 25;
            this.ThreadsUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // HashSizeUpDown
            // 
            this.HashSizeUpDown.Location = new System.Drawing.Point(200, 60);
            this.HashSizeUpDown.Maximum = new decimal(new int[] {
            33554432,
            0,
            0,
            0});
            this.HashSizeUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.HashSizeUpDown.Name = "HashSizeUpDown";
            this.HashSizeUpDown.Size = new System.Drawing.Size(120, 20);
            this.HashSizeUpDown.TabIndex = 26;
            this.HashSizeUpDown.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // MultiPVUpDown
            // 
            this.MultiPVUpDown.Location = new System.Drawing.Point(200, 110);
            this.MultiPVUpDown.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.MultiPVUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.MultiPVUpDown.Name = "MultiPVUpDown";
            this.MultiPVUpDown.Size = new System.Drawing.Size(120, 20);
            this.MultiPVUpDown.TabIndex = 27;
            this.MultiPVUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // SkillLevelUpDown
            // 
            this.SkillLevelUpDown.Location = new System.Drawing.Point(200, 135);
            this.SkillLevelUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.SkillLevelUpDown.Name = "SkillLevelUpDown";
            this.SkillLevelUpDown.Size = new System.Drawing.Size(120, 20);
            this.SkillLevelUpDown.TabIndex = 28;
            this.SkillLevelUpDown.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // MoveOverheadUpDown
            // 
            this.MoveOverheadUpDown.Location = new System.Drawing.Point(200, 160);
            this.MoveOverheadUpDown.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.MoveOverheadUpDown.Name = "MoveOverheadUpDown";
            this.MoveOverheadUpDown.Size = new System.Drawing.Size(120, 20);
            this.MoveOverheadUpDown.TabIndex = 29;
            this.MoveOverheadUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // NodesTimeUpDown
            // 
            this.NodesTimeUpDown.Location = new System.Drawing.Point(200, 185);
            this.NodesTimeUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.NodesTimeUpDown.Name = "NodesTimeUpDown";
            this.NodesTimeUpDown.Size = new System.Drawing.Size(120, 20);
            this.NodesTimeUpDown.TabIndex = 30;
            // 
            // UciEloUpDown
            // 
            this.UciEloUpDown.Enabled = false;
            this.UciEloUpDown.Location = new System.Drawing.Point(200, 235);
            this.UciEloUpDown.Maximum = new decimal(new int[] {
            3190,
            0,
            0,
            0});
            this.UciEloUpDown.Minimum = new decimal(new int[] {
            1320,
            0,
            0,
            0});
            this.UciEloUpDown.Name = "UciEloUpDown";
            this.UciEloUpDown.Size = new System.Drawing.Size(120, 20);
            this.UciEloUpDown.TabIndex = 31;
            this.UciEloUpDown.Value = new decimal(new int[] {
            1320,
            0,
            0,
            0});
            // 
            // SyzygyProbeDepthUpDown
            // 
            this.SyzygyProbeDepthUpDown.Enabled = false;
            this.SyzygyProbeDepthUpDown.Location = new System.Drawing.Point(540, 135);
            this.SyzygyProbeDepthUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.SyzygyProbeDepthUpDown.Name = "SyzygyProbeDepthUpDown";
            this.SyzygyProbeDepthUpDown.Size = new System.Drawing.Size(120, 20);
            this.SyzygyProbeDepthUpDown.TabIndex = 32;
            this.SyzygyProbeDepthUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // SyzygyProbeLimitUpDown
            // 
            this.SyzygyProbeLimitUpDown.Enabled = false;
            this.SyzygyProbeLimitUpDown.Location = new System.Drawing.Point(540, 185);
            this.SyzygyProbeLimitUpDown.Maximum = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.SyzygyProbeLimitUpDown.Name = "SyzygyProbeLimitUpDown";
            this.SyzygyProbeLimitUpDown.Size = new System.Drawing.Size(120, 20);
            this.SyzygyProbeLimitUpDown.TabIndex = 33;
            this.SyzygyProbeLimitUpDown.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            // 
            // NumaPolicyTextBox
            // 
            this.NumaPolicyTextBox.Enabled = false;
            this.NumaPolicyTextBox.Location = new System.Drawing.Point(540, 10);
            this.NumaPolicyTextBox.Name = "NumaPolicyTextBox";
            this.NumaPolicyTextBox.Size = new System.Drawing.Size(160, 20);
            this.NumaPolicyTextBox.TabIndex = 34;
            this.NumaPolicyTextBox.Text = "auto";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Enabled = false;
            this.richTextBox1.Location = new System.Drawing.Point(540, 210);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(160, 20);
            this.richTextBox1.TabIndex = 35;
            this.richTextBox1.Text = "nn-1111cefa1111.nnue";
            // 
            // richTextBox2
            // 
            this.richTextBox2.Enabled = false;
            this.richTextBox2.Location = new System.Drawing.Point(540, 235);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(160, 20);
            this.richTextBox2.TabIndex = 36;
            this.richTextBox2.Text = "nn-37f18f62d772.nnue";
            // 
            // richTextBox3
            // 
            this.richTextBox3.Enabled = false;
            this.richTextBox3.Location = new System.Drawing.Point(540, 110);
            this.richTextBox3.Name = "richTextBox3";
            this.richTextBox3.Size = new System.Drawing.Size(160, 20);
            this.richTextBox3.TabIndex = 37;
            this.richTextBox3.Text = "<empty>";
            // 
            // richTextBox4
            // 
            this.richTextBox4.Enabled = false;
            this.richTextBox4.Location = new System.Drawing.Point(200, 10);
            this.richTextBox4.Name = "richTextBox4";
            this.richTextBox4.Size = new System.Drawing.Size(100, 20);
            this.richTextBox4.TabIndex = 38;
            this.richTextBox4.Text = "<empty>";
            // 
            // ClearHashButton
            // 
            this.ClearHashButton.Location = new System.Drawing.Point(200, 85);
            this.ClearHashButton.Name = "ClearHashButton";
            this.ClearHashButton.Size = new System.Drawing.Size(120, 20);
            this.ClearHashButton.StateNormal.Back.Color1 = System.Drawing.Color.Lavender;
            this.ClearHashButton.StateNormal.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ClearHashButton.StateNormal.Border.Rounding = 20F;
            this.ClearHashButton.StateNormal.Border.Width = 1;
            this.ClearHashButton.TabIndex = 39;
            this.ClearHashButton.Values.Text = "Vide Hash Tables";
            this.ClearHashButton.Click += new System.EventHandler(this.ClearHashButton_Click);
            // 
            // ParametresFermer
            // 
            this.ParametresFermer.Location = new System.Drawing.Point(270, 260);
            this.ParametresFermer.Name = "ParametresFermer";
            this.ParametresFermer.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.ParametresFermer.Size = new System.Drawing.Size(120, 20);
            this.ParametresFermer.StateNormal.Back.Color1 = System.Drawing.Color.Lavender;
            this.ParametresFermer.StateNormal.Border.Color1 = System.Drawing.Color.Gray;
            this.ParametresFermer.StateNormal.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ParametresFermer.StateNormal.Border.Rounding = 20F;
            this.ParametresFermer.StateNormal.Border.Width = 3;
            this.ParametresFermer.TabIndex = 40;
            this.ParametresFermer.Values.Text = "Fermer";
            this.ParametresFermer.Click += new System.EventHandler(this.ParametresFermer_Click);
            // 
            // ParametresUciStockfish
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(739, 284);
            this.Controls.Add(this.ParametresFermer);
            this.Controls.Add(this.ClearHashButton);
            this.Controls.Add(this.richTextBox4);
            this.Controls.Add(this.richTextBox3);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.NumaPolicyTextBox);
            this.Controls.Add(this.SyzygyProbeLimitUpDown);
            this.Controls.Add(this.SyzygyProbeDepthUpDown);
            this.Controls.Add(this.UciEloUpDown);
            this.Controls.Add(this.NodesTimeUpDown);
            this.Controls.Add(this.MoveOverheadUpDown);
            this.Controls.Add(this.SkillLevelUpDown);
            this.Controls.Add(this.MultiPVUpDown);
            this.Controls.Add(this.HashSizeUpDown);
            this.Controls.Add(this.ThreadsUpDown);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.checkBoxPonder);
            this.Controls.Add(this.labelEvalFileSmall);
            this.Controls.Add(this.labelEvalFile);
            this.Controls.Add(this.labelSyzygyProbeLimit);
            this.Controls.Add(this.labelSyzygy50MoveRule);
            this.Controls.Add(this.labelSyzygyProbeDepth);
            this.Controls.Add(this.labelSyzygyPath);
            this.Controls.Add(this.labelUCI_ShowWDL);
            this.Controls.Add(this.labelUCI_ELO);
            this.Controls.Add(this.labelUCI_LimitStrength);
            this.Controls.Add(this.labelClearHash);
            this.Controls.Add(this.labelUCI_Chess960);
            this.Controls.Add(this.labelNodesTime);
            this.Controls.Add(this.labelMoveOverhead);
            this.Controls.Add(this.labelSkillLevel);
            this.Controls.Add(this.labelMultiPV);
            this.Controls.Add(this.labelPonder);
            this.Controls.Add(this.labelHash);
            this.Controls.Add(this.labelThreads);
            this.Controls.Add(this.labelNumaPolicy);
            this.Controls.Add(this.labelDebugLogFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ParametresUciStockfish";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parametres du Moteur Uci Stockfish";
            ((System.ComponentModel.ISupportInitialize)(this.ThreadsUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HashSizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MultiPVUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SkillLevelUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MoveOverheadUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NodesTimeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UciEloUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SyzygyProbeDepthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SyzygyProbeLimitUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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