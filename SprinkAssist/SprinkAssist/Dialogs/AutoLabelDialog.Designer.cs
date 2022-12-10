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
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.labelDrainsDiameter = new System.Windows.Forms.Label();
			this.labelMainsDiameter = new System.Windows.Forms.Label();
			this.labelBranchlinesDiameter = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.Label_Color = new System.Windows.Forms.Label();
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
			this.button1 = new System.Windows.Forms.Button();
			this.listView1 = new System.Windows.Forms.ListView();
			this.groupBoxSelectedGroup.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// listBoxPipeGroup
			// 
			this.listBoxPipeGroup.FormattingEnabled = true;
			this.listBoxPipeGroup.Location = new System.Drawing.Point(10, 30);
			this.listBoxPipeGroup.Name = "listBoxPipeGroup";
			this.listBoxPipeGroup.Size = new System.Drawing.Size(130, 173);
			this.listBoxPipeGroup.TabIndex = 0;
			this.listBoxPipeGroup.SelectedIndexChanged += new System.EventHandler(this.ListBox_PipeGroup_SelectedIndexChanged);
			// 
			// buttonAddNewGroup
			// 
			this.buttonAddNewGroup.Location = new System.Drawing.Point(10, 213);
			this.buttonAddNewGroup.Name = "buttonAddNewGroup";
			this.buttonAddNewGroup.Size = new System.Drawing.Size(120, 23);
			this.buttonAddNewGroup.TabIndex = 1;
			this.buttonAddNewGroup.Text = "Add New Group";
			this.buttonAddNewGroup.UseVisualStyleBackColor = true;
			this.buttonAddNewGroup.Click += new System.EventHandler(this.Button_AddNewGroup_Click);
			// 
			// buttonRun
			// 
			this.buttonRun.BackColor = System.Drawing.Color.PaleGreen;
			this.buttonRun.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonRun.Location = new System.Drawing.Point(10, 579);
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.Size = new System.Drawing.Size(483, 44);
			this.buttonRun.TabIndex = 2;
			this.buttonRun.Text = "Assign Group to Selected";
			this.buttonRun.UseVisualStyleBackColor = false;
			this.buttonRun.Click += new System.EventHandler(this.Button_AssignToSelected_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.BackColor = System.Drawing.Color.IndianRed;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(10, 451);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(482, 44);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Clear Group from Selected";
			this.buttonCancel.UseVisualStyleBackColor = false;
			this.buttonCancel.Click += new System.EventHandler(this.Button_ClearFromSelected_Click);
			// 
			// buttonEditGroup
			// 
			this.buttonEditGroup.Location = new System.Drawing.Point(10, 242);
			this.buttonEditGroup.Name = "buttonEditGroup";
			this.buttonEditGroup.Size = new System.Drawing.Size(120, 23);
			this.buttonEditGroup.TabIndex = 4;
			this.buttonEditGroup.Text = "Edit Group";
			this.buttonEditGroup.UseVisualStyleBackColor = true;
			this.buttonEditGroup.Click += new System.EventHandler(this.Button_EditGroup_Click);
			// 
			// groupBoxSelectedGroup
			// 
			this.groupBoxSelectedGroup.BackColor = System.Drawing.SystemColors.Control;
			this.groupBoxSelectedGroup.Controls.Add(this.button1);
			this.groupBoxSelectedGroup.Controls.Add(this.label8);
			this.groupBoxSelectedGroup.Controls.Add(this.label7);
			this.groupBoxSelectedGroup.Controls.Add(this.labelDrainsDiameter);
			this.groupBoxSelectedGroup.Controls.Add(this.labelMainsDiameter);
			this.groupBoxSelectedGroup.Controls.Add(this.labelBranchlinesDiameter);
			this.groupBoxSelectedGroup.Controls.Add(this.label5);
			this.groupBoxSelectedGroup.Controls.Add(this.label6);
			this.groupBoxSelectedGroup.Controls.Add(this.label4);
			this.groupBoxSelectedGroup.Controls.Add(this.Label_Color);
			this.groupBoxSelectedGroup.Location = new System.Drawing.Point(146, 8);
			this.groupBoxSelectedGroup.Name = "groupBoxSelectedGroup";
			this.groupBoxSelectedGroup.Size = new System.Drawing.Size(347, 103);
			this.groupBoxSelectedGroup.TabIndex = 5;
			this.groupBoxSelectedGroup.TabStop = false;
			this.groupBoxSelectedGroup.Text = "Selected Group";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(267, 41);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(10, 13);
			this.label8.TabIndex = 14;
			this.label8.Text = "-";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(163, 41);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(92, 13);
			this.label7.TabIndex = 13;
			this.label7.Text = "Branchline Risers:";
			// 
			// labelDrainsDiameter
			// 
			this.labelDrainsDiameter.AutoSize = true;
			this.labelDrainsDiameter.Location = new System.Drawing.Point(76, 79);
			this.labelDrainsDiameter.Name = "labelDrainsDiameter";
			this.labelDrainsDiameter.Size = new System.Drawing.Size(10, 13);
			this.labelDrainsDiameter.TabIndex = 12;
			this.labelDrainsDiameter.Text = "-";
			// 
			// labelMainsDiameter
			// 
			this.labelMainsDiameter.AutoSize = true;
			this.labelMainsDiameter.Location = new System.Drawing.Point(76, 60);
			this.labelMainsDiameter.Name = "labelMainsDiameter";
			this.labelMainsDiameter.Size = new System.Drawing.Size(10, 13);
			this.labelMainsDiameter.TabIndex = 11;
			this.labelMainsDiameter.Text = "-";
			// 
			// labelBranchlinesDiameter
			// 
			this.labelBranchlinesDiameter.AutoSize = true;
			this.labelBranchlinesDiameter.Location = new System.Drawing.Point(76, 41);
			this.labelBranchlinesDiameter.Name = "labelBranchlinesDiameter";
			this.labelBranchlinesDiameter.Size = new System.Drawing.Size(10, 13);
			this.labelBranchlinesDiameter.TabIndex = 10;
			this.labelBranchlinesDiameter.Text = "-";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 60);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(38, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Mains:";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 79);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Drains:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 41);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "Branchlines:";
			// 
			// Label_Color
			// 
			this.Label_Color.AutoSize = true;
			this.Label_Color.Location = new System.Drawing.Point(6, 22);
			this.Label_Color.Name = "Label_Color";
			this.Label_Color.Size = new System.Drawing.Size(34, 13);
			this.Label_Color.TabIndex = 0;
			this.Label_Color.Text = "Color:";
			// 
			// checkBoxBreakAtLineEnds
			// 
			this.checkBoxBreakAtLineEnds.AutoSize = true;
			this.checkBoxBreakAtLineEnds.Checked = true;
			this.checkBoxBreakAtLineEnds.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakAtLineEnds.Location = new System.Drawing.Point(9, 77);
			this.checkBoxBreakAtLineEnds.Name = "checkBoxBreakAtLineEnds";
			this.checkBoxBreakAtLineEnds.Size = new System.Drawing.Size(116, 17);
			this.checkBoxBreakAtLineEnds.TabIndex = 31;
			this.checkBoxBreakAtLineEnds.Text = "Break at Line Ends";
			this.checkBoxBreakAtLineEnds.UseVisualStyleBackColor = true;
			// 
			// checkBoxConnectAcrossBreaks
			// 
			this.checkBoxConnectAcrossBreaks.AutoSize = true;
			this.checkBoxConnectAcrossBreaks.Checked = true;
			this.checkBoxConnectAcrossBreaks.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxConnectAcrossBreaks.Location = new System.Drawing.Point(9, 177);
			this.checkBoxConnectAcrossBreaks.Name = "checkBoxConnectAcrossBreaks";
			this.checkBoxConnectAcrossBreaks.Size = new System.Drawing.Size(137, 17);
			this.checkBoxConnectAcrossBreaks.TabIndex = 30;
			this.checkBoxConnectAcrossBreaks.Text = "Connect Across Breaks";
			this.checkBoxConnectAcrossBreaks.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(216, 178);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(86, 13);
			this.label2.TabIndex = 25;
			this.label2.Text = "Search Distance";
			// 
			// textBoxConnectBreaksInLines
			// 
			this.textBoxConnectBreaksInLines.Location = new System.Drawing.Point(178, 175);
			this.textBoxConnectBreaksInLines.Name = "textBoxConnectBreaksInLines";
			this.textBoxConnectBreaksInLines.Size = new System.Drawing.Size(31, 20);
			this.textBoxConnectBreaksInLines.TabIndex = 24;
			// 
			// checkBoxBreakAtFittings
			// 
			this.checkBoxBreakAtFittings.AutoSize = true;
			this.checkBoxBreakAtFittings.Checked = true;
			this.checkBoxBreakAtFittings.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakAtFittings.Location = new System.Drawing.Point(9, 121);
			this.checkBoxBreakAtFittings.Name = "checkBoxBreakAtFittings";
			this.checkBoxBreakAtFittings.Size = new System.Drawing.Size(102, 17);
			this.checkBoxBreakAtFittings.TabIndex = 23;
			this.checkBoxBreakAtFittings.Text = "Break at Fittings";
			this.checkBoxBreakAtFittings.UseVisualStyleBackColor = true;
			// 
			// checkBoxBreakAtLineIntersections
			// 
			this.checkBoxBreakAtLineIntersections.AutoSize = true;
			this.checkBoxBreakAtLineIntersections.Location = new System.Drawing.Point(9, 99);
			this.checkBoxBreakAtLineIntersections.Name = "checkBoxBreakAtLineIntersections";
			this.checkBoxBreakAtLineIntersections.Size = new System.Drawing.Size(152, 17);
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
			this.groupBox2.Location = new System.Drawing.Point(145, 122);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(347, 227);
			this.groupBox2.TabIndex = 33;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Label Generation Options";
			// 
			// textBoxDontLabelShortLines
			// 
			this.textBoxDontLabelShortLines.Location = new System.Drawing.Point(178, 197);
			this.textBoxDontLabelShortLines.Name = "textBoxDontLabelShortLines";
			this.textBoxDontLabelShortLines.Size = new System.Drawing.Size(31, 20);
			this.textBoxDontLabelShortLines.TabIndex = 36;
			// 
			// checkBoxBreakAtSprinklers
			// 
			this.checkBoxBreakAtSprinklers.AutoSize = true;
			this.checkBoxBreakAtSprinklers.Checked = true;
			this.checkBoxBreakAtSprinklers.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxBreakAtSprinklers.Location = new System.Drawing.Point(9, 143);
			this.checkBoxBreakAtSprinklers.Name = "checkBoxBreakAtSprinklers";
			this.checkBoxBreakAtSprinklers.Size = new System.Drawing.Size(115, 17);
			this.checkBoxBreakAtSprinklers.TabIndex = 32;
			this.checkBoxBreakAtSprinklers.Text = "Break at Sprinklers";
			this.checkBoxBreakAtSprinklers.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(216, 200);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 13);
			this.label3.TabIndex = 37;
			this.label3.Text = "Min Length";
			// 
			// checkBoxDontLabelShortLines
			// 
			this.checkBoxDontLabelShortLines.AutoSize = true;
			this.checkBoxDontLabelShortLines.Checked = true;
			this.checkBoxDontLabelShortLines.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxDontLabelShortLines.Location = new System.Drawing.Point(9, 199);
			this.checkBoxDontLabelShortLines.Name = "checkBoxDontLabelShortLines";
			this.checkBoxDontLabelShortLines.Size = new System.Drawing.Size(136, 17);
			this.checkBoxDontLabelShortLines.TabIndex = 35;
			this.checkBoxDontLabelShortLines.Text = "Don\'t Label Short Lines";
			this.checkBoxDontLabelShortLines.UseVisualStyleBackColor = true;
			// 
			// textBoxOmitLengthsFromShortLines
			// 
			this.textBoxOmitLengthsFromShortLines.Location = new System.Drawing.Point(178, 41);
			this.textBoxOmitLengthsFromShortLines.Name = "textBoxOmitLengthsFromShortLines";
			this.textBoxOmitLengthsFromShortLines.Size = new System.Drawing.Size(31, 20);
			this.textBoxOmitLengthsFromShortLines.TabIndex = 33;
			// 
			// checkBoxShowLengths
			// 
			this.checkBoxShowLengths.AutoSize = true;
			this.checkBoxShowLengths.Checked = true;
			this.checkBoxShowLengths.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxShowLengths.Location = new System.Drawing.Point(9, 23);
			this.checkBoxShowLengths.Name = "checkBoxShowLengths";
			this.checkBoxShowLengths.Size = new System.Drawing.Size(94, 17);
			this.checkBoxShowLengths.TabIndex = 29;
			this.checkBoxShowLengths.Text = "Show Lengths";
			this.checkBoxShowLengths.UseVisualStyleBackColor = true;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(216, 46);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(60, 13);
			this.label11.TabIndex = 34;
			this.label11.Text = "Min Length";
			// 
			// checkBoxOmitLengthsFromShortLines
			// 
			this.checkBoxOmitLengthsFromShortLines.AutoSize = true;
			this.checkBoxOmitLengthsFromShortLines.Checked = true;
			this.checkBoxOmitLengthsFromShortLines.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxOmitLengthsFromShortLines.Location = new System.Drawing.Point(9, 45);
			this.checkBoxOmitLengthsFromShortLines.Name = "checkBoxOmitLengthsFromShortLines";
			this.checkBoxOmitLengthsFromShortLines.Size = new System.Drawing.Size(162, 17);
			this.checkBoxOmitLengthsFromShortLines.TabIndex = 32;
			this.checkBoxOmitLengthsFromShortLines.Text = "Omit Length from Short Lines";
			this.checkBoxOmitLengthsFromShortLines.UseVisualStyleBackColor = true;
			// 
			// labelSelectPipeGroup
			// 
			this.labelSelectPipeGroup.AutoSize = true;
			this.labelSelectPipeGroup.Location = new System.Drawing.Point(9, 8);
			this.labelSelectPipeGroup.Name = "labelSelectPipeGroup";
			this.labelSelectPipeGroup.Size = new System.Drawing.Size(96, 13);
			this.labelSelectPipeGroup.TabIndex = 34;
			this.labelSelectPipeGroup.Text = "Select Pipe Group:";
			// 
			// buttonDeleteGroup
			// 
			this.buttonDeleteGroup.Location = new System.Drawing.Point(10, 271);
			this.buttonDeleteGroup.Name = "buttonDeleteGroup";
			this.buttonDeleteGroup.Size = new System.Drawing.Size(120, 23);
			this.buttonDeleteGroup.TabIndex = 35;
			this.buttonDeleteGroup.Text = "Delete Group";
			this.buttonDeleteGroup.UseVisualStyleBackColor = true;
			this.buttonDeleteGroup.Click += new System.EventHandler(this.Button_DeleteGroup_Click);
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Red;
			this.button1.Location = new System.Drawing.Point(69, 19);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(55, 19);
			this.button1.TabIndex = 36;
			this.button1.UseVisualStyleBackColor = false;
			// 
			// listView1
			// 
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(36, 351);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(138, 100);
			this.listView1.TabIndex = 36;
			this.listView1.UseCompatibleStateImageBehavior = false;
			// 
			// AutoLabelDialog
			// 
			this.AcceptButton = this.buttonRun;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(507, 635);
			this.Controls.Add(this.listView1);
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
		private System.Windows.Forms.Label Label_Color;
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
		private System.Windows.Forms.Button buttonDeleteGroup;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ListView listView1;
	}
}