namespace Ironwill
{
	partial class PipeLabelDialog
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
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.textBoxSlopeRise = new System.Windows.Forms.TextBox();
			this.textBoxSlopeRun = new System.Windows.Forms.TextBox();
			this.checkBoxBreakBranchlines = new System.Windows.Forms.CheckBox();
			this.textBoxArmovers = new System.Windows.Forms.TextBox();
			this.labelArmovers = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.labelAngleDegrees = new System.Windows.Forms.Label();
			this.labelRiseRun = new System.Windows.Forms.Label();
			this.textBoxSlopeAngle = new System.Windows.Forms.TextBox();
			this.labelRiseRunColon = new System.Windows.Forms.Label();
			this.listBoxGroup = new System.Windows.Forms.ListBox();
			this.groupBoxOptions = new System.Windows.Forms.GroupBox();
			this.checkBoxSearchForBrokenPipes = new System.Windows.Forms.CheckBox();
			this.checkBoxBreakArmovers = new System.Windows.Forms.CheckBox();
			this.checkBoxShowArmoverLengths = new System.Windows.Forms.CheckBox();
			this.checkBoxShowMainLengths = new System.Windows.Forms.CheckBox();
			this.checkBoxShowBranchlineLengths = new System.Windows.Forms.CheckBox();
			this.textBoxIgnoreLineThreshold = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxOmitLengthThreshold = new System.Windows.Forms.TextBox();
			this.checkBoxBreakMains = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.labelBranchlines = new System.Windows.Forms.Label();
			this.textBoxBranchlines = new System.Windows.Forms.TextBox();
			this.labelMains = new System.Windows.Forms.Label();
			this.textBoxMains = new System.Windows.Forms.TextBox();
			this.groupBox2.SuspendLayout();
			this.groupBoxOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(7, 321);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(100, 40);
			this.buttonCancel.TabIndex = 12;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOk
			// 
			this.buttonOk.Enabled = false;
			this.buttonOk.Location = new System.Drawing.Point(259, 321);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(100, 40);
			this.buttonOk.TabIndex = 13;
			this.buttonOk.Text = "Ok";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// textBoxSlopeRise
			// 
			this.textBoxSlopeRise.Location = new System.Drawing.Point(82, 18);
			this.textBoxSlopeRise.Name = "textBoxSlopeRise";
			this.textBoxSlopeRise.Size = new System.Drawing.Size(29, 20);
			this.textBoxSlopeRise.TabIndex = 6;
			this.textBoxSlopeRise.Text = "0";
			this.textBoxSlopeRise.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textBoxSlopeRise.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSlopeRise_OnKeyDown);
			this.textBoxSlopeRise.Leave += new System.EventHandler(this.textBoxSlopeRise_OnLeave);
			// 
			// textBoxSlopeRun
			// 
			this.textBoxSlopeRun.Location = new System.Drawing.Point(118, 18);
			this.textBoxSlopeRun.Name = "textBoxSlopeRun";
			this.textBoxSlopeRun.Size = new System.Drawing.Size(29, 20);
			this.textBoxSlopeRun.TabIndex = 7;
			this.textBoxSlopeRun.Text = "12";
			this.textBoxSlopeRun.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textBoxSlopeRun.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSlopeRun_OnKeyDown);
			this.textBoxSlopeRun.Leave += new System.EventHandler(this.textBoxSlopeRun_OnLeave);
			// 
			// checkBoxBreakBranchlines
			// 
			this.checkBoxBreakBranchlines.AutoSize = true;
			this.checkBoxBreakBranchlines.Checked = true;
			this.checkBoxBreakBranchlines.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakBranchlines.Location = new System.Drawing.Point(9, 42);
			this.checkBoxBreakBranchlines.Name = "checkBoxBreakBranchlines";
			this.checkBoxBreakBranchlines.Size = new System.Drawing.Size(112, 17);
			this.checkBoxBreakBranchlines.TabIndex = 10;
			this.checkBoxBreakBranchlines.Text = "Break Branchlines";
			this.checkBoxBreakBranchlines.UseVisualStyleBackColor = true;
			this.checkBoxBreakBranchlines.CheckedChanged += new System.EventHandler(this.checkBoxBreakLinesOnHeads_CheckedChanged);
			// 
			// textBoxArmovers
			// 
			this.textBoxArmovers.Location = new System.Drawing.Point(86, 11);
			this.textBoxArmovers.Name = "textBoxArmovers";
			this.textBoxArmovers.Size = new System.Drawing.Size(71, 20);
			this.textBoxArmovers.TabIndex = 0;
			this.textBoxArmovers.TextChanged += new System.EventHandler(this.textBoxArmovers_TextChanged);
			// 
			// labelArmovers
			// 
			this.labelArmovers.AutoSize = true;
			this.labelArmovers.Location = new System.Drawing.Point(21, 14);
			this.labelArmovers.Name = "labelArmovers";
			this.labelArmovers.Size = new System.Drawing.Size(62, 13);
			this.labelArmovers.TabIndex = 17;
			this.labelArmovers.Text = "Armovers Ø";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.labelAngleDegrees);
			this.groupBox2.Controls.Add(this.labelRiseRun);
			this.groupBox2.Controls.Add(this.textBoxSlopeAngle);
			this.groupBox2.Controls.Add(this.textBoxSlopeRise);
			this.groupBox2.Controls.Add(this.textBoxSlopeRun);
			this.groupBox2.Controls.Add(this.labelRiseRunColon);
			this.groupBox2.Location = new System.Drawing.Point(7, 226);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(158, 76);
			this.groupBox2.TabIndex = 15;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Slope";
			// 
			// labelAngleDegrees
			// 
			this.labelAngleDegrees.AutoSize = true;
			this.labelAngleDegrees.Location = new System.Drawing.Point(16, 47);
			this.labelAngleDegrees.Name = "labelAngleDegrees";
			this.labelAngleDegrees.Size = new System.Drawing.Size(63, 13);
			this.labelAngleDegrees.TabIndex = 22;
			this.labelAngleDegrees.Text = "Angle (Deg)";
			// 
			// labelRiseRun
			// 
			this.labelRiseRun.AutoSize = true;
			this.labelRiseRun.Location = new System.Drawing.Point(15, 21);
			this.labelRiseRun.Name = "labelRiseRun";
			this.labelRiseRun.Size = new System.Drawing.Size(57, 13);
			this.labelRiseRun.TabIndex = 21;
			this.labelRiseRun.Text = "Rise : Run";
			// 
			// textBoxSlopeAngle
			// 
			this.textBoxSlopeAngle.Location = new System.Drawing.Point(82, 44);
			this.textBoxSlopeAngle.Name = "textBoxSlopeAngle";
			this.textBoxSlopeAngle.Size = new System.Drawing.Size(65, 20);
			this.textBoxSlopeAngle.TabIndex = 9;
			this.textBoxSlopeAngle.Text = "0";
			this.textBoxSlopeAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textBoxSlopeAngle.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSlopeAngle_OnKeyDown);
			this.textBoxSlopeAngle.Leave += new System.EventHandler(this.textBoxSlopeAngle_OnLeave);
			// 
			// labelRiseRunColon
			// 
			this.labelRiseRunColon.AutoSize = true;
			this.labelRiseRunColon.Location = new System.Drawing.Point(110, 21);
			this.labelRiseRunColon.Name = "labelRiseRunColon";
			this.labelRiseRunColon.Size = new System.Drawing.Size(10, 13);
			this.labelRiseRunColon.TabIndex = 20;
			this.labelRiseRunColon.Text = ":";
			// 
			// listBoxGroup
			// 
			this.listBoxGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listBoxGroup.FormattingEnabled = true;
			this.listBoxGroup.Items.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "J"});
			this.listBoxGroup.Location = new System.Drawing.Point(86, 98);
			this.listBoxGroup.Name = "listBoxGroup";
			this.listBoxGroup.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.listBoxGroup.Size = new System.Drawing.Size(71, 121);
			this.listBoxGroup.TabIndex = 4;
			this.listBoxGroup.SelectedIndexChanged += new System.EventHandler(this.listBoxGroup_SelectedIndexChanged);
			// 
			// groupBoxOptions
			// 
			this.groupBoxOptions.Controls.Add(this.checkBoxSearchForBrokenPipes);
			this.groupBoxOptions.Controls.Add(this.checkBoxBreakArmovers);
			this.groupBoxOptions.Controls.Add(this.checkBoxShowArmoverLengths);
			this.groupBoxOptions.Controls.Add(this.checkBoxShowMainLengths);
			this.groupBoxOptions.Controls.Add(this.checkBoxShowBranchlineLengths);
			this.groupBoxOptions.Controls.Add(this.textBoxIgnoreLineThreshold);
			this.groupBoxOptions.Controls.Add(this.label3);
			this.groupBoxOptions.Controls.Add(this.label2);
			this.groupBoxOptions.Controls.Add(this.textBoxOmitLengthThreshold);
			this.groupBoxOptions.Controls.Add(this.checkBoxBreakMains);
			this.groupBoxOptions.Controls.Add(this.checkBoxBreakBranchlines);
			this.groupBoxOptions.Location = new System.Drawing.Point(175, 11);
			this.groupBoxOptions.Name = "groupBoxOptions";
			this.groupBoxOptions.Size = new System.Drawing.Size(184, 291);
			this.groupBoxOptions.TabIndex = 20;
			this.groupBoxOptions.TabStop = false;
			this.groupBoxOptions.Text = "Options";
			// 
			// checkBoxSearchForBrokenPipes
			// 
			this.checkBoxSearchForBrokenPipes.AutoSize = true;
			this.checkBoxSearchForBrokenPipes.Checked = true;
			this.checkBoxSearchForBrokenPipes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSearchForBrokenPipes.Location = new System.Drawing.Point(9, 153);
			this.checkBoxSearchForBrokenPipes.Name = "checkBoxSearchForBrokenPipes";
			this.checkBoxSearchForBrokenPipes.Size = new System.Drawing.Size(141, 17);
			this.checkBoxSearchForBrokenPipes.TabIndex = 21;
			this.checkBoxSearchForBrokenPipes.Text = "Search for Broken Pipes";
			this.checkBoxSearchForBrokenPipes.UseVisualStyleBackColor = true;
			// 
			// checkBoxBreakArmovers
			// 
			this.checkBoxBreakArmovers.AutoSize = true;
			this.checkBoxBreakArmovers.Checked = true;
			this.checkBoxBreakArmovers.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakArmovers.Location = new System.Drawing.Point(9, 20);
			this.checkBoxBreakArmovers.Name = "checkBoxBreakArmovers";
			this.checkBoxBreakArmovers.Size = new System.Drawing.Size(101, 17);
			this.checkBoxBreakArmovers.TabIndex = 20;
			this.checkBoxBreakArmovers.Text = "Break Armovers";
			this.checkBoxBreakArmovers.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowArmoverLengths
			// 
			this.checkBoxShowArmoverLengths.AutoSize = true;
			this.checkBoxShowArmoverLengths.Checked = true;
			this.checkBoxShowArmoverLengths.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowArmoverLengths.Location = new System.Drawing.Point(9, 86);
			this.checkBoxShowArmoverLengths.Name = "checkBoxShowArmoverLengths";
			this.checkBoxShowArmoverLengths.Size = new System.Drawing.Size(136, 17);
			this.checkBoxShowArmoverLengths.TabIndex = 19;
			this.checkBoxShowArmoverLengths.Text = "Show Armover Lengths";
			this.checkBoxShowArmoverLengths.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowMainLengths
			// 
			this.checkBoxShowMainLengths.AutoSize = true;
			this.checkBoxShowMainLengths.Checked = true;
			this.checkBoxShowMainLengths.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowMainLengths.Location = new System.Drawing.Point(9, 130);
			this.checkBoxShowMainLengths.Name = "checkBoxShowMainLengths";
			this.checkBoxShowMainLengths.Size = new System.Drawing.Size(120, 17);
			this.checkBoxShowMainLengths.TabIndex = 18;
			this.checkBoxShowMainLengths.Text = "Show Main Lengths";
			this.checkBoxShowMainLengths.UseVisualStyleBackColor = true;
			// 
			// checkBoxShowBranchlineLengths
			// 
			this.checkBoxShowBranchlineLengths.AutoSize = true;
			this.checkBoxShowBranchlineLengths.Checked = true;
			this.checkBoxShowBranchlineLengths.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowBranchlineLengths.Location = new System.Drawing.Point(9, 108);
			this.checkBoxShowBranchlineLengths.Name = "checkBoxShowBranchlineLengths";
			this.checkBoxShowBranchlineLengths.Size = new System.Drawing.Size(147, 17);
			this.checkBoxShowBranchlineLengths.TabIndex = 17;
			this.checkBoxShowBranchlineLengths.Text = "Show Branchline Lengths";
			this.checkBoxShowBranchlineLengths.UseVisualStyleBackColor = true;
			// 
			// textBoxIgnoreLineThreshold
			// 
			this.textBoxIgnoreLineThreshold.Location = new System.Drawing.Point(9, 204);
			this.textBoxIgnoreLineThreshold.Name = "textBoxIgnoreLineThreshold";
			this.textBoxIgnoreLineThreshold.Size = new System.Drawing.Size(30, 20);
			this.textBoxIgnoreLineThreshold.TabIndex = 16;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(42, 207);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(134, 13);
			this.label3.TabIndex = 15;
			this.label3.Text = "Ignore Pipe if Shorter Than";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(43, 181);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(125, 13);
			this.label2.TabIndex = 14;
			this.label2.Text = "Omit Length if Less Than";
			// 
			// textBoxOmitLengthThreshold
			// 
			this.textBoxOmitLengthThreshold.Location = new System.Drawing.Point(9, 178);
			this.textBoxOmitLengthThreshold.Name = "textBoxOmitLengthThreshold";
			this.textBoxOmitLengthThreshold.Size = new System.Drawing.Size(31, 20);
			this.textBoxOmitLengthThreshold.TabIndex = 13;
			// 
			// checkBoxBreakMains
			// 
			this.checkBoxBreakMains.AutoSize = true;
			this.checkBoxBreakMains.Checked = true;
			this.checkBoxBreakMains.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakMains.Location = new System.Drawing.Point(9, 64);
			this.checkBoxBreakMains.Name = "checkBoxBreakMains";
			this.checkBoxBreakMains.Size = new System.Drawing.Size(85, 17);
			this.checkBoxBreakMains.TabIndex = 12;
			this.checkBoxBreakMains.Text = "Break Mains";
			this.checkBoxBreakMains.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(47, 101);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 13);
			this.label1.TabIndex = 21;
			this.label1.Text = "Group";
			// 
			// labelBranchlines
			// 
			this.labelBranchlines.AutoSize = true;
			this.labelBranchlines.Location = new System.Drawing.Point(10, 40);
			this.labelBranchlines.Name = "labelBranchlines";
			this.labelBranchlines.Size = new System.Drawing.Size(73, 13);
			this.labelBranchlines.TabIndex = 23;
			this.labelBranchlines.Text = "Branchlines Ø";
			// 
			// textBoxBranchlines
			// 
			this.textBoxBranchlines.Location = new System.Drawing.Point(86, 37);
			this.textBoxBranchlines.Name = "textBoxBranchlines";
			this.textBoxBranchlines.Size = new System.Drawing.Size(71, 20);
			this.textBoxBranchlines.TabIndex = 22;
			this.textBoxBranchlines.TextChanged += new System.EventHandler(this.textBoxBranchlines_TextChanged);
			// 
			// labelMains
			// 
			this.labelMains.AutoSize = true;
			this.labelMains.Location = new System.Drawing.Point(37, 66);
			this.labelMains.Name = "labelMains";
			this.labelMains.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.labelMains.Size = new System.Drawing.Size(46, 13);
			this.labelMains.TabIndex = 25;
			this.labelMains.Text = "Mains Ø";
			// 
			// textBoxMains
			// 
			this.textBoxMains.Location = new System.Drawing.Point(86, 63);
			this.textBoxMains.Name = "textBoxMains";
			this.textBoxMains.Size = new System.Drawing.Size(71, 20);
			this.textBoxMains.TabIndex = 24;
			this.textBoxMains.TextChanged += new System.EventHandler(this.textBoxMains_TextChanged);
			// 
			// PipeLabelDialog
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(367, 368);
			this.Controls.Add(this.labelMains);
			this.Controls.Add(this.textBoxMains);
			this.Controls.Add(this.labelBranchlines);
			this.Controls.Add(this.textBoxBranchlines);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.listBoxGroup);
			this.Controls.Add(this.groupBoxOptions);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.labelArmovers);
			this.Controls.Add(this.textBoxArmovers);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.buttonCancel);
			this.MaximumSize = new System.Drawing.Size(383, 407);
			this.MinimumSize = new System.Drawing.Size(383, 407);
			this.Name = "PipeLabelDialog";
			this.Text = "PipeLabelDialog";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.PipeLabelDialog_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBoxOptions.ResumeLayout(false);
			this.groupBoxOptions.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.TextBox textBoxSlopeRise;
		private System.Windows.Forms.TextBox textBoxSlopeRun;
		private System.Windows.Forms.CheckBox checkBoxBreakBranchlines;
		private System.Windows.Forms.TextBox textBoxArmovers;
		private System.Windows.Forms.Label labelArmovers;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label labelRiseRunColon;
		private System.Windows.Forms.ListBox listBoxGroup;
		private System.Windows.Forms.TextBox textBoxSlopeAngle;
		private System.Windows.Forms.GroupBox groupBoxOptions;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelBranchlines;
		private System.Windows.Forms.TextBox textBoxBranchlines;
		private System.Windows.Forms.Label labelMains;
		private System.Windows.Forms.TextBox textBoxMains;
		private System.Windows.Forms.CheckBox checkBoxBreakMains;
		private System.Windows.Forms.TextBox textBoxIgnoreLineThreshold;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxOmitLengthThreshold;
		private System.Windows.Forms.CheckBox checkBoxBreakArmovers;
		private System.Windows.Forms.CheckBox checkBoxShowArmoverLengths;
		private System.Windows.Forms.CheckBox checkBoxShowMainLengths;
		private System.Windows.Forms.CheckBox checkBoxShowBranchlineLengths;
		private System.Windows.Forms.CheckBox checkBoxSearchForBrokenPipes;
		private System.Windows.Forms.Label labelAngleDegrees;
		private System.Windows.Forms.Label labelRiseRun;
	}
}