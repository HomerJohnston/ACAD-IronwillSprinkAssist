using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.AutoCAD.ApplicationServices;

namespace Ironwill
{
	public partial class PipeGroupDialog : Form
	{
		// State
		public PipeGroupDialog()
		{
			InitializeComponent();
		}

		private void PipeGroupDialog_Load(object sender, EventArgs e)
		{

		}

		private void Button_Save_Click(object sender, EventArgs e)
		{
			CreateGroup(
				TextBox_GroupName.Text, 
				TextBox_Mains.Text, 
				TextBox_Branchlines.Text, 
				TextBox_BranchlineRisers.Text, 
				TextBox_Armovers.Text, 
				TextBox_Drains.Text
			);
		}



		public string MainsLabel
		{
			get { return TextBox_Mains.Text; }
		}

		public string BranchlinesLabel
		{
			get { return TextBox_Branchlines.Text; }
		}

		public string BranchlineRisersLabel
		{
			get { return TextBox_BranchlineRisers.Text; }
		}

		public string ArmoversLabel
		{
			get { return TextBox_Armovers.Text; }
		}

		public string DrainsLabel
		{
			get { return TextBox_Drains.Text; }
		}

		void CreateGroup(string groupName, string mainsLabel, string branchlinesLabel, string branchlineRisersLabel, string armoversLabel, string drainsLabel)
		{
			using (DocumentLock x = Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					DBDictionary pipeGroupsDictionary = XRecordLibrary.GetNamedDictionary(transaction, "PipeGroups");

					DBDictionary groupDictionary = XRecordLibrary.GetNamedDictionary(transaction, groupName, pipeGroupsDictionary);

					XRecordLibrary.SetXRecord(transaction, groupDictionary, "MainsLabel", mainsLabel);
					XRecordLibrary.SetXRecord(transaction, groupDictionary, "BranchlinesLabel", branchlinesLabel);
					XRecordLibrary.SetXRecord(transaction, groupDictionary, "BranchlineRisersLabel", branchlineRisersLabel);
					XRecordLibrary.SetXRecord(transaction, groupDictionary, "ArmoversLabel", armoversLabel);
					XRecordLibrary.SetXRecord(transaction, groupDictionary, "DrainsLabel", drainsLabel);

					transaction.Commit();
				}
			}
		}
	}
}
