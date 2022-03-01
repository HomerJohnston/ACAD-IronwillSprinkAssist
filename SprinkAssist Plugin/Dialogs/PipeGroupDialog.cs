using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ironwill
{
	public partial class PipeGroupDialog : Form
	{
		public PipeGroup pipeGroup;

		public PipeGroupDialog()
		{
			InitializeComponent();
		}

		private void PipeGroupDialog_Load(object sender, EventArgs e)
		{/*
			textBoxName.Text = pipeGroup.name.stringValue;
			
			textBoxArmovers.Text = pipeGroup.armoverLabel.stringValue;
			textBoxBranchlines.Text = pipeGroup.branchlineLabel.stringValue;
			textBoxMains.Text = pipeGroup.mainLabel.stringValue;
			textBoxDrains.Text = pipeGroup.drainLabel.stringValue;*/
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			pipeGroup = PipeGroup.Get(textBoxName.Text);

			pipeGroup.armoverLabel.Set(textBoxArmovers.Text);
			pipeGroup.branchlineLabel.Set(textBoxBranchlines.Text);
			pipeGroup.mainLabel.Set(textBoxMains.Text);
			pipeGroup.drainLabel.Set(textBoxDrains.Text);
		}
	}
}
