using System;
using System.Windows.Forms;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Ironwill
{
	public partial class PipeLabelDialog : Form
	{
		// State
		public bool okPressed = false;
		static private bool firstRun = true;

		// Left Pane
		static public string armoverLabel = "";
		static public string branchlineLabel = "";
		static public string mainLabel = "";

		static public int groupIDIndex = -1;
		public string groupID
		{
			get
			{
				if (groupIDIndex < 0)
					return String.Empty;

				return listBoxGroup.Items[groupIDIndex] as String;
			}
		}

		static public int slopeRise = 0;
		static public int slopeRun = 12;
		static public double slopeAngle = 0.0;

		// Right Pane
		static public bool breakArmovers = true;
		static public bool breakBranchlines = true;
		static public bool breakMains = true;

		static public bool showArmoverLengths = true;
		static public bool showBranchlineLengths = true;
		static public bool showMainLengths = true;

		static public bool searchBrokenPipes = true;

		static public double omitLengthLabelLength = -1.0;
		static public double ignoreLineLength = -1.0;

		public PipeLabelDialog()
		{
			InitializeComponent();
		}

		private void PipeLabelDialog_Load(object sender, EventArgs e)
		{
			textBoxArmovers.Text = armoverLabel;
			textBoxBranchlines.Text = branchlineLabel;
			textBoxMains.Text = mainLabel;

			listBoxGroup.SelectedIndex = groupIDIndex;

			textBoxSlopeRise.Text = slopeRise.ToString();
			textBoxSlopeRun.Text = slopeRun.ToString();
			textBoxSlopeAngle.Text = slopeAngle.ToString();

			checkBoxBreakArmovers.Checked = breakArmovers;
			checkBoxBreakBranchlines.Checked = breakBranchlines;
			checkBoxBreakMains.Checked = breakMains;

			checkBoxShowArmoverLengths.Checked = showArmoverLengths;
			checkBoxShowBranchlineLengths.Checked = showBranchlineLengths;
			checkBoxShowMainLengths.Checked = showMainLengths;

			checkBoxSearchForBrokenPipes.Checked = searchBrokenPipes;

			if (firstRun)
			{
				switch (Session.GetPrimaryUnits())
				{
					case DrawingUnits.Imperial:
						omitLengthLabelLength = 6;
						ignoreLineLength = 2;
						break;
					case DrawingUnits.Metric:
						omitLengthLabelLength = 150;
						ignoreLineLength = 50;
						break;
				}

				firstRun = false;
			}

			textBoxOmitLengthThreshold.Text = omitLengthLabelLength.ToString();
			textBoxIgnoreLineThreshold.Text = ignoreLineLength.ToString();
		}

		private void textBoxArmovers_TextChanged(object sender, EventArgs e)
		{
			armoverLabel = textBoxArmovers.Text;
			UpdateOkButton();
		}

		private void textBoxBranchlines_TextChanged(object sender, EventArgs e)
		{
			branchlineLabel = textBoxBranchlines.Text;
			UpdateOkButton();
		}

		private void textBoxMains_TextChanged(object sender, EventArgs e)
		{
			mainLabel = textBoxMains.Text;
			UpdateOkButton();
		}

		private void listBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
		{
			groupIDIndex = listBoxGroup.SelectedIndex;
			UpdateOkButton();
		}

		private void checkBoxBreakLinesOnHeads_CheckedChanged(object sender, EventArgs e)
		{
			UpdateOkButton();
		}

		private void checkBoxBreakLinesOnLines_CheckedChanged(object sender, EventArgs e)
		{
			UpdateOkButton();
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			okPressed = true;
			Close();
		}

		private void UpdateOkButton()
		{
			buttonOk.Enabled = CheckValid();
		}

		private bool CheckValid()
		{
			if (armoverLabel == String.Empty && branchlineLabel == String.Empty &&  mainLabel == String.Empty)
				return false;

			if (groupID == String.Empty)
				return false;

			return true;
		}

		private void textBoxSlopeRise_OnLeave(object sender, EventArgs e)
		{
			UpdateInt(textBoxSlopeRise, ref slopeRise);
		}

		private void textBoxSlopeRise_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				UpdateInt(textBoxSlopeRise, ref slopeRise);
			}
		}

		private void textBoxSlopeRun_OnLeave(object sender, EventArgs e)
		{
			UpdateInt(textBoxSlopeRun, ref slopeRun);
		}

		private void textBoxSlopeRun_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				UpdateInt(textBoxSlopeRun, ref slopeRun);
			}
		}

		void UpdateInt(TextBox textBox, ref int val)
		{
			if (CheckTextBoxForInt(textBox))
			{
				int valTemp;
				if (int.TryParse(textBox.Text, out valTemp))
				{
					val = valTemp;
					UpdateSlopeAngle();
				}
			}
		}

		private bool CheckTextBoxForInt(TextBox textBox)
		{
			if (textBox.TextLength == 0)
				return false;

			int val;

			if (!int.TryParse(textBox.Text, out val))
			{
				MessageBox.Show("Please enter integer values only.");
				textBox.Clear();
				return false;
			}

			return true;
		}

		private void textBoxSlopeAngle_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				UpdateDouble(textBoxSlopeAngle, ref slopeAngle);
			}
		}

		private void textBoxSlopeAngle_OnLeave(object sender, EventArgs e)
		{
			UpdateDouble(textBoxSlopeAngle, ref slopeAngle);
		}

		void UpdateDouble(TextBox textBox, ref double val)
		{
			if (CheckTextBoxForDouble(textBox))
			{
				double valTemp;
				if (double.TryParse(textBox.Text, out valTemp))
				{
					val = valTemp;
					UpdateSlopeAngle();
				}
			}
		}

		private bool CheckTextBoxForDouble(TextBox textBox)
		{
			if (textBox.TextLength == 0)
				return false;

			double val;

			if (!double.TryParse(textBox.Text, out val))
			{
				MessageBox.Show("Please enter a number.");
				textBox.Clear();
				return false;
			}

			return true;
		}

		private void UpdateSlopeRiseRun()
		{
			double theta = ParseTextBoxDouble(textBoxSlopeAngle) * Math.PI / 180.0;

			textBoxSlopeRun.Text = (12).ToString();
			textBoxSlopeRise.Text = Math.Round((Math.Tan(theta) * 12.0)).ToString();
		}

		private void UpdateSlopeAngle()
		{ 
			double rise = ParseTextBoxDouble(textBoxSlopeRise);
			double run = ParseTextBoxDouble(textBoxSlopeRun);
			double theta = Math.Atan2(rise, run) * 180.0 / Math.PI;

			textBoxSlopeAngle.Text = theta.ToString();
		}

		public double GetSlopeLengthMultiplier()
		{
			double multiplier = 1.0;
			double theta = ParseTextBoxDouble(textBoxSlopeAngle) * Math.PI / 180.0;

			multiplier = 1.0 / (Math.Cos(theta));

			return multiplier;
		}

		private double ParseTextBoxDouble(TextBox textBox)
		{
			double val;

			if (!double.TryParse(textBox.Text, out val))
			{
				val = 0.0;
			}

			return val;
		}
	}
}
