using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.AutoLabelDialog))]

namespace Ironwill
{
	public partial class AutoLabelDialog : Form
	{
		// State
		Transaction transaction = null;
		static int selectedGroupIndex;

		// Constructors/Destructors
		public AutoLabelDialog(Transaction transaction)
		{
			this.transaction = transaction;
			InitializeComponent();
		}

		~AutoLabelDialog()
		{
			transaction = null;
		}

		// Methods
		private void buttonAddNewGroup_Click(object sender, EventArgs e)
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				//PipeGroup newGroup = PipeGroup.CreateNew();

				PipeGroupDialog pipeGroupDialog = new PipeGroupDialog();
				pipeGroupDialog.TopLevel = true;

				if (AcApplication.ShowModalDialog(pipeGroupDialog) == DialogResult.OK);
				{

				}

				PipeGroup newPipeGroup = pipeGroupDialog.pipeGroup;

				transaction.Commit();
			}

			UpdatePipeGroupList();

			// If OK, save it, otherwise ignore
		}

		private void AutoLabelDialog_Load(object sender, EventArgs e)
		{
			//TestCreatePipeGroup();
			UpdatePipeGroupList();
		}
		
		private void UpdatePipeGroupList()
		{
			listBoxPipeGroup.Items.Clear();

			DictionaryPath pipeGroupsPath = new DictionaryPath("PipeGroups");

			DBDictionary pipeGroupsDictionary = DataStore.GetDictionary(transaction, pipeGroupsPath);

			if (pipeGroupsDictionary == null)
			{
				return;
			}

			foreach(DBDictionaryEntry pipeGroupEntry in pipeGroupsDictionary)
			{
				string groupID = pipeGroupEntry.Key;

				string groupName = DataStore.GetXrecordString(new DictionaryPath(pipeGroupsPath, groupID), "Name", "___NULLGROUP___");
				
				if (groupName != "___NULLGROUP___")
				{
					listBoxPipeGroup.Items.Add(groupName);
				}
			}
		}

		private void UpdateDialog()
		{
			string selectedGroupName = listBoxPipeGroup.SelectedItem as string;

			DictionaryPath pipeGroupsPath = new DictionaryPath("PipeGroups");

			DBDictionary pipeGroupsDictionary = DataStore.GetDictionary(transaction, pipeGroupsPath);

			foreach (DBDictionaryEntry pipeGroupEntry in pipeGroupsDictionary)
			{
				string groupName = pipeGroupEntry.Key;

				if (groupName == selectedGroupName)
				{
					//PipeGroup group = new PipeGroup(groupName);
					PipeGroup group = PipeGroup.Get(groupName);

					labelArmoversDiameter.Text = string.IsNullOrEmpty(group.armoverLabel) ? "UNSET" : group.armoverLabel;
					labelBranchlinesDiameter.Text = string.IsNullOrEmpty(group.branchlineLabel) ? "UNSET" : group.branchlineLabel;
					labelMainsDiameter.Text = string.IsNullOrEmpty(group.mainLabel) ? "UNSET" : group.mainLabel;
					labelDrainsDiameter.Text = string.IsNullOrEmpty(group.drainLabel) ? "UNSET" : group.drainLabel;
				}
			}
		}

		private void buttonEditGroup_Click(object sender, EventArgs e)
		{
			
		}

		private void buttonDeleteGroup_Click(object sender, EventArgs e)
		{

		}

		private void listBoxPipeGroup_SelectedIndexChanged(object sender, EventArgs e)
		{
			selectedGroupIndex = listBoxPipeGroup.SelectedIndex;
			UpdateDialog();
		}
	}
}
