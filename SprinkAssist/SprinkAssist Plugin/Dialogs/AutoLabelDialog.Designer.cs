namespace Ironwill
{
	partial class AutoLabelDialog
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
			this.listBoxPipeGroup = new System.Windows.Forms.ListBox();
			this.buttonAddNewGroup = new System.Windows.Forms.Button();
			this.buttonRun = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonEditGroup = new System.Windows.Forms.Button();
			this.groupBoxSelectedGroup = new System.Windows.Forms.GroupBox();
			this.labelDrainsDiameter = new System.Windows.Forms.Label();
			this.labelMainsDiameter = new System.Windows.Forms.Label();
			this.labelBranchlinesDiameter = new System.Windows.Forms.Label();
			this.labelArmoversDiameter = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxBreakAtLineEnds = new System.Windows.Forms.CheckBox();
			this.checkBoxConnectAcrossBreaks = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxConnectBreaksInLines = new System.Windows.Forms.TextBox();
			this.checkBoxBreakAtFittings = new System.Windows.Forms.CheckBox();
			this.checkBoxBreakAtLineIntersections = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBoxDontLabelShortLines = new System.Windows.Forms.TextBox();
			this.checkBoxBreakAtSprinklers = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.checkBoxDontLabelShortLines = new System.Windows.Forms.CheckBox();
			this.textBoxOmitLengthsFromShortLines = new System.Windows.Forms.TextBox();
			this.checkBoxShowLengths = new System.Windows.Forms.CheckBox();
			this.label11 = new System.Windows.Forms.Label();
			this.checkBoxOmitLengthsFromShortLines = new System.Windows.Forms.CheckBox();
			this.labelSelectPipeGroup = new System.Windows.Forms.Label();
			this.buttonDeleteGroup = new System.Windows.Forms.Button();
			this.groupBoxSelectedGroup.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// listBoxPipeGroup
			// 
			this.listBoxPipeGroup.FormattingEnabled = true;
			this.listBoxPipeGroup.ItemHeight = 16;
			this.listBoxPipeGroup.Location = new System.Drawing.Point(13, 37);
			this.listBoxPipeGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.listBoxPipeGroup.Name = "listBoxPipeGroup";
			this.listBoxPipeGroup.Size = new System.Drawing.Size(159, 212);
			this.listBoxPipeGroup.TabIndex = 0;
			this.listBoxPipeGroup.SelectedIndexChanged += new System.EventHandler(this.listBoxPipeGroup_SelectedIndexChanged);
			// 
			// buttonAddNewGroup
			// 
			this.buttonAddNewGroup.Location = new System.Drawing.Point(13, 262);
			this.buttonAddNewGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.buttonAddNewGroup.Name = "buttonAddNewGroup";
			this.buttonAddNewGroup.Size = new System.Drawing.Size(160, 28);
			this.buttonAddNewGroup.TabIndex = 1;
			this.buttonAddNewGroup.Text = "Add New Group";
			this.buttonAddNewGroup.UseVisualStyleBackColor = true;
			this.buttonAddNewGroup.Click += new System.EventHandler(this.buttonAddNewGroup_Click);
			// 
			// buttonRun
			// 
			this.buttonRun.BackColor = System.Drawing.Color.PaleGreen;
			this.buttonRun.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonRun.Location = new System.Drawing.Point(96, 375);
			this.buttonRun.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.Size = new System.Drawing.Size(77, 54);
			this.buttonRun.TabIndex = 2;
			this.buttonRun.Text = "Run";
			this.buttonRun.UseVisualStyleBackColor = false;
			// 
			// buttonCancel
			// 
			this.buttonCancel.BackColor = System.Drawing.Color.IndianRed;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(13, 375);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(77, 54);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// buttonEditGroup
			// 
			this.buttonEditGroup.Location = new System.Drawing.Point(13, 298);
			this.buttonEditGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.buttonEditGroup.Name = "buttonEditGroup";
			this.buttonEditGroup.Size = new System.Drawing.Size(160, 28);
			this.buttonEditGroup.TabIndex = 4;
			this.buttonEditGroup.Text = "Edit Group";
			this.buttonEditGroup.UseVisualStyleBackColor = true;
			this.buttonEditGroup.Click += new System.EventHandler(this.buttonEditGroup_Click);
			// 
			// groupBoxSelectedGroup
			// 
			this.groupBoxSelectedGroup.BackColor = System.Drawing.SystemColors.Control;
			this.groupBoxSelectedGroup.Controls.Add(this.labelDrainsDiameter);
			this.groupBoxSelectedGroup.Controls.Add(this.labelMainsDiameter);
			this.groupBoxSelectedGroup.Controls.Add(this.labelBranchlinesDiameter);
			this.groupBoxSelectedGroup.Controls.Add(this.labelArmoversDiameter);
			this.groupBoxSelectedGroup.Controls.Add(this.label5);
			this.groupBoxSelectedGroup.Controls.Add(this.label6);
			this.groupBoxSelectedGroup.Controls.Add(this.label4);
			this.groupBoxSelectedGroup.Controls.Add(this.label1);
			this.groupBoxSelectedGroup.Location = new System.Drawing.Point(195, 10);
			this.groupBoxSelectedGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.groupBoxSelectedGroup.Name = "groupBoxSelectedGroup";
			this.groupBoxSelectedGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.groupBoxSelectedGroup.Size = new System.Drawing.Size(463, 127);
			this.groupBoxSelectedGroup.TabIndex = 5;
			this.groupBoxSelectedGroup.TabStop = false;
			this.groupBoxSelectedGroup.Text = "Selected Group";
			// 
			// labelDrainsDiameter
			// 
			this.labelDrainsDiameter.AutoSize = true;
			this.labelDrainsDiameter.Location = new System.Drawing.Point(101, 97);
			this.labelDrainsDiameter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelDrainsDiameter.Name = "labelDrainsDiameter";
			this.labelDrainsDiameter.Size = new System.Drawing.Size(13, 17);
			this.labelDrainsDiameter.TabIndex = 12;
			this.labelDrainsDiameter.Text = "-";
			// 
			// labelMainsDiameter
			// 
			this.labelMainsDiameter.AutoSize = true;
			this.labelMainsDiameter.Location = new System.Drawing.Point(101, 74);
			this.labelMainsDiameter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelMainsDiameter.Name = "labelMainsDiameter";
			this.labelMainsDiameter.Size = new System.Drawing.Size(13, 17);
			this.labelMainsDiameter.TabIndex = 11;
			this.labelMainsDiameter.Text = "-";
			// 
			// labelBranchlinesDiameter
			// 
			this.labelBranchlinesDiameter.AutoSize = true;
			this.labelBranchlinesDiameter.Location = new System.Drawing.Point(101, 50);
			this.labelBranchlinesDiameter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelBranchlinesDiameter.Name = "labelBranchlinesDiameter";
			this.labelBranchlinesDiameter.Size = new System.Drawing.Size(13, 17);
			this.labelBranchlinesDiameter.TabIndex = 10;
			this.labelBranchlinesDiameter.Text = "-";
			// 
			// labelArmoversDiameter
			// 
			this.labelArmoversDiameter.AutoSize = true;
			this.labelArmoversDiameter.Location = new System.Drawing.Point(101, 27);
			this.labelArmoversDiameter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelArmoversDiameter.Name = "labelArmoversDiameter";
			this.labelArmoversDiameter.Size = new System.Drawing.Size(13, 17);
			this.labelArmoversDiameter.TabIndex = 9;
			this.labelArmoversDiameter.Text = "-";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(8, 74);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(49, 17);
			this.label5.TabIndex = 2;
			this.label5.Text = "Mains:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(8, 97);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(53, 17);
			this.label6.TabIndex = 3;
			this.label6.Text = "Drains:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 50);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(86, 17);
			this.label4.TabIndex = 1;
			this.label4.Text = "Branchlines:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 27);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Armovers:";
			// 
			// checkBoxBreakAtLineEnds
			// 
			this.checkBoxBreakAtLineEnds.AutoSize = true;
			this.checkBoxBreakAtLineEnds.Checked = true;
			this.checkBoxBreakAtLineEnds.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakAtLineEnds.Location = new System.Drawing.Point(12, 95);
			this.checkBoxBreakAtLineEnds.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxBreakAtLineEnds.Name = "checkBoxBreakAtLineEnds";
			this.checkBoxBreakAtLineEnds.Size = new System.Drawing.Size(150, 21);
			this.checkBoxBreakAtLineEnds.TabIndex = 31;
			this.checkBoxBreakAtLineEnds.Text = "Break at Line Ends";
			this.checkBoxBreakAtLineEnds.UseVisualStyleBackColor = true;
			// 
			// checkBoxConnectAcrossBreaks
			// 
			this.checkBoxConnectAcrossBreaks.AutoSize = true;
			this.checkBoxConnectAcrossBreaks.Checked = true;
			this.checkBoxConnectAcrossBreaks.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxConnectAcrossBreaks.Location = new System.Drawing.Point(12, 218);
			this.checkBoxConnectAcrossBreaks.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxConnectAcrossBreaks.Name = "checkBoxConnectAcrossBreaks";
			this.checkBoxConnectAcrossBreaks.Size = new System.Drawing.Size(177, 21);
			this.checkBoxConnectAcrossBreaks.TabIndex = 30;
			this.checkBoxConnectAcrossBreaks.Text = "Connect Across Breaks";
			this.checkBoxConnectAcrossBreaks.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(288, 219);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 17);
			this.label2.TabIndex = 25;
			this.label2.Text = "Search Distance";
			// 
			// textBoxConnectBreaksInLines
			// 
			this.textBoxConnectBreaksInLines.Location = new System.Drawing.Point(237, 215);
			this.textBoxConnectBreaksInLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.textBoxConnectBreaksInLines.Name = "textBoxConnectBreaksInLines";
			this.textBoxConnectBreaksInLines.Size = new System.Drawing.Size(40, 22);
			this.textBoxConnectBreaksInLines.TabIndex = 24;
			// 
			// checkBoxBreakAtFittings
			// 
			this.checkBoxBreakAtFittings.AutoSize = true;
			this.checkBoxBreakAtFittings.Checked = true;
			this.checkBoxBreakAtFittings.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakAtFittings.Location = new System.Drawing.Point(12, 149);
			this.checkBoxBreakAtFittings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxBreakAtFittings.Name = "checkBoxBreakAtFittings";
			this.checkBoxBreakAtFittings.Size = new System.Drawing.Size(132, 21);
			this.checkBoxBreakAtFittings.TabIndex = 23;
			this.checkBoxBreakAtFittings.Text = "Break at Fittings";
			this.checkBoxBreakAtFittings.UseVisualStyleBackColor = true;
			// 
			// checkBoxBreakAtLineIntersections
			// 
			this.checkBoxBreakAtLineIntersections.AutoSize = true;
			this.checkBoxBreakAtLineIntersections.Location = new System.Drawing.Point(12, 122);
			this.checkBoxBreakAtLineIntersections.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxBreakAtLineIntersections.Name = "checkBoxBreakAtLineIntersections";
			this.checkBoxBreakAtLineIntersections.Size = new System.Drawing.Size(198, 21);
			this.checkBoxBreakAtLineIntersections.TabIndex = 22;
			this.checkBoxBreakAtLineIntersections.Text = "Break at Line Intersections";
			this.checkBoxBreakAtLineIntersections.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
			this.groupBox2.Controls.Add(this.textBoxDontLabelShortLines);
			this.groupBox2.Controls.Add(this.checkBoxBreakAtSprinklers);
			this.groupBox2.Controls.Add(this.checkBoxBreakAtLineEnds);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.checkBoxBreakAtLineIntersections);
			this.groupBox2.Controls.Add(this.checkBoxBreakAtFittings);
			this.groupBox2.Controls.Add(this.checkBoxDontLabelShortLines);
			this.groupBox2.Controls.Add(this.textBoxOmitLengthsFromShortLines);
			this.groupBox2.Controls.Add(this.checkBoxShowLengths);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.checkBoxConnectAcrossBreaks);
			this.groupBox2.Controls.Add(this.checkBoxOmitLengthsFromShortLines);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.textBoxConnectBreaksInLines);
			this.groupBox2.Location = new System.Drawing.Point(193, 150);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.groupBox2.Size = new System.Drawing.Size(463, 279);
			this.groupBox2.TabIndex = 33;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Label Generation Options";
			// 
			// textBoxDontLabelShortLines
			// 
			this.textBoxDontLabelShortLines.Location = new System.Drawing.Point(237, 242);
			this.textBoxDontLabelShortLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.textBoxDontLabelShortLines.Name = "textBoxDontLabelShortLines";
			this.textBoxDontLabelShortLines.Size = new System.Drawing.Size(40, 22);
			this.textBoxDontLabelShortLines.TabIndex = 36;
			// 
			// checkBoxBreakAtSprinklers
			// 
			this.checkBoxBreakAtSprinklers.AutoSize = true;
			this.checkBoxBreakAtSprinklers.Checked = true;
			this.checkBoxBreakAtSprinklers.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakAtSprinklers.Location = new System.Drawing.Point(12, 176);
			this.checkBoxBreakAtSprinklers.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxBreakAtSprinklers.Name = "checkBoxBreakAtSprinklers";
			this.checkBoxBreakAtSprinklers.Size = new System.Drawing.Size(150, 21);
			this.checkBoxBreakAtSprinklers.TabIndex = 32;
			this.checkBoxBreakAtSprinklers.Text = "Break at Sprinklers";
			this.checkBoxBreakAtSprinklers.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(288, 246);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(78, 17);
			this.label3.TabIndex = 37;
			this.label3.Text = "Min Length";
			// 
			// checkBoxDontLabelShortLines
			// 
			this.checkBoxDontLabelShortLines.AutoSize = true;
			this.checkBoxDontLabelShortLines.Checked = true;
			this.checkBoxDontLabelShortLines.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxDontLabelShortLines.Location = new System.Drawing.Point(12, 245);
			this.checkBoxDontLabelShortLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxDontLabelShortLines.Name = "checkBoxDontLabelShortLines";
			this.checkBoxDontLabelShortLines.Size = new System.Drawing.Size(178, 21);
			this.checkBoxDontLabelShortLines.TabIndex = 35;
			this.checkBoxDontLabelShortLines.Text = "Don\'t Label Short Lines";
			this.checkBoxDontLabelShortLines.UseVisualStyleBackColor = true;
			// 
			// textBoxOmitLengthsFromShortLines
			// 
			this.textBoxOmitLengthsFromShortLines.Location = new System.Drawing.Point(237, 50);
			this.textBoxOmitLengthsFromShortLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.textBoxOmitLengthsFromShortLines.Name = "textBoxOmitLengthsFromShortLines";
			this.textBoxOmitLengthsFromShortLines.Size = new System.Drawing.Size(40, 22);
			this.textBoxOmitLengthsFromShortLines.TabIndex = 33;
			// 
			// checkBoxShowLengths
			// 
			this.checkBoxShowLengths.AutoSize = true;
			this.checkBoxShowLengths.Checked = true;
			this.checkBoxShowLengths.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowLengths.Location = new System.Drawing.Point(12, 28);
			this.checkBoxShowLengths.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxShowLengths.Name = "checkBoxShowLengths";
			this.checkBoxShowLengths.Size = new System.Drawing.Size(119, 21);
			this.checkBoxShowLengths.TabIndex = 29;
			this.checkBoxShowLengths.Text = "Show Lengths";
			this.checkBoxShowLengths.UseVisualStyleBackColor = true;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(288, 57);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(78, 17);
			this.label11.TabIndex = 34;
			this.label11.Text = "Min Length";
			// 
			// checkBoxOmitLengthsFromShortLines
			// 
			this.checkBoxOmitLengthsFromShortLines.AutoSize = true;
			this.checkBoxOmitLengthsFromShortLines.Checked = true;
			this.checkBoxOmitLengthsFromShortLines.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxOmitLengthsFromShortLines.Location = new System.Drawing.Point(12, 55);
			this.checkBoxOmitLengthsFromShortLines.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.checkBoxOmitLengthsFromShortLines.Name = "checkBoxOmitLengthsFromShortLines";
			this.checkBoxOmitLengthsFromShortLines.Size = new System.Drawing.Size(215, 21);
			this.checkBoxOmitLengthsFromShortLines.TabIndex = 32;
			this.checkBoxOmitLengthsFromShortLines.Text = "Omit Length from Short Lines";
			this.checkBoxOmitLengthsFromShortLines.UseVisualStyleBackColor = true;
			// 
			// labelSelectPipeGroup
			// 
			this.labelSelectPipeGroup.AutoSize = true;
			this.labelSelectPipeGroup.Location = new System.Drawing.Point(12, 10);
			this.labelSelectPipeGroup.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelSelectPipeGroup.Name = "labelSelectPipeGroup";
			this.labelSelectPipeGroup.Size = new System.Drawing.Size(127, 17);
			this.labelSelectPipeGroup.TabIndex = 34;
			this.labelSelectPipeGroup.Text = "Select Pipe Group:";
			// 
			// buttonDeleteGroup
			// 
			this.buttonDeleteGroup.Location = new System.Drawing.Point(13, 334);
			this.buttonDeleteGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.buttonDeleteGroup.Name = "buttonDeleteGroup";
			this.buttonDeleteGroup.Size = new System.Drawing.Size(160, 28);
			this.buttonDeleteGroup.TabIndex = 35;
			this.buttonDeleteGroup.Text = "Delete Group";
			this.buttonDeleteGroup.UseVisualStyleBackColor = true;
			this.buttonDeleteGroup.Click += new System.EventHandler(this.buttonDeleteGroup_Click);
			// 
			// AutoLabelDialog
			// 
			this.AcceptButton = this.buttonRun;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(676, 442);
			this.ControlBox = false;
			this.Controls.Add(this.buttonDeleteGroup);
			this.Controls.Add(this.labelSelectPipeGroup);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBoxSelectedGroup);
			this.Controls.Add(this.buttonEditGroup);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonRun);
			this.Controls.Add(this.buttonAddNewGroup);
			this.Controls.Add(this.listBoxPipeGroup);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AutoLabelDialog";
			this.ShowIcon = false;
			this.Text = "AutoLabelDialog";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.AutoLabelDialog_Load);
			this.groupBoxSelectedGroup.ResumeLayout(false);
			this.groupBoxSelectedGroup.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox listBoxPipeGroup;
		private System.Windows.Forms.Button buttonAddNewGroup;
		private System.Windows.Forms.Button buttonRun;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonEditGroup;
		private System.Windows.Forms.GroupBox groupBoxSelectedGroup;
		private System.Windows.Forms.CheckBox checkBoxBreakAtLineEnds;
		private System.Windows.Forms.CheckBox checkBoxConnectAcrossBreaks;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBoxConnectBreaksInLines;
		private System.Windows.Forms.CheckBox checkBoxBreakAtFittings;
		private System.Windows.Forms.CheckBox checkBoxBreakAtLineIntersections;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxOmitLengthsFromShortLines;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.CheckBox checkBoxOmitLengthsFromShortLines;
		private System.Windows.Forms.CheckBox checkBoxDontLabelShortLines;
		private System.Windows.Forms.TextBox textBoxDontLabelShortLines;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox checkBoxBreakAtSprinklers;
		private System.Windows.Forms.Label labelSelectPipeGroup;
		private System.Windows.Forms.CheckBox checkBoxShowLengths;
		private System.Windows.Forms.Label labelDrainsDiameter;
		private System.Windows.Forms.Label labelMainsDiameter;
		private System.Windows.Forms.Label labelBranchlinesDiameter;
		private System.Windows.Forms.Label labelArmoversDiameter;
		private System.Windows.Forms.Button buttonDeleteGroup;
	}
}