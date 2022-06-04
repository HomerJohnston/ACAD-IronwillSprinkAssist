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
using Autodesk.AutoCAD.ApplicationServices;

[assembly: CommandClass(typeof(Ironwill.AutoLabelDialog))]

namespace Ironwill
{
	public partial class AutoLabelDialog : Form
	{
		// State
		static int selectedGroupIndex;

		// Constructors/Destructors
		public AutoLabelDialog()
		{
			InitializeComponent();
		}

		~AutoLabelDialog()
		{
		}

		private void AutoLabelDialog_Load(object sender, EventArgs e)
		{
			UpdatePipeGroupList();
		}
		
		private void UpdatePipeGroupList()
		{
			listBoxPipeGroup.Items.Clear();

			using (Transaction transaction = Session.StartTransaction())
			{
				DBDictionary pipeGroupsDictionary = XRecordLibrary.GetNamedDictionary(transaction, "PipeGroups");

				if (pipeGroupsDictionary == null)
				{
					return;
				}

				foreach (DBDictionaryEntry pipeGroupEntry in pipeGroupsDictionary)
				{
					string groupName = pipeGroupEntry.Key;

					if (groupName != "___NULLGROUP___")
					{
						listBoxPipeGroup.Items.Add(groupName);
					}
				}

				transaction.Commit();
			}
		}

		private void UpdateDialog()
		{
			return;

			//string selectedGroupName = listBoxPipeGroup.SelectedItem as string;

			//OBSOLETEDictionaryPath pipeGroupsPath = new OBSOLETEDictionaryPath("PipeGroups");

			//DBDictionary pipeGroupsDictionary = OBSOLETEDataStore.GetDictionary(transaction, pipeGroupsPath);
/*
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
			}*/
		}

		private void ListBox_PipeGroup_SelectedIndexChanged(object sender, EventArgs e)
		{
			selectedGroupIndex = listBoxPipeGroup.SelectedIndex;
			UpdateDialog();
		}

		private void Button_AddNewGroup_Click(object sender, EventArgs e)
		{
			PipeGroupDialog pipeGroupDialog = new PipeGroupDialog();

			AcApplication.ShowModalDialog(pipeGroupDialog);

			UpdatePipeGroupList();

			if (pipeGroupDialog.DialogResult == DialogResult.OK)
			{
			}
			else
			{
			}
		}

		private void Button_EditGroup_Click(object sender, EventArgs e)
		{

		}

		private void Button_DeleteGroup_Click(object sender, EventArgs e)
		{
			string groupName = listBoxPipeGroup.Text;

			using (DocumentLock documentLock = Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					DBDictionary pipeGroupsDictionary = XRecordLibrary.GetNamedDictionary(transaction, "PipeGroups");

					DBDictionary dictionaryMutable = transaction.GetObject(pipeGroupsDictionary.ObjectId, OpenMode.ForWrite) as DBDictionary;

					dictionaryMutable.Remove(groupName);

					transaction.Commit();
				}
			}

			UpdatePipeGroupList();
		}

		private void Button_Run_Click(object sender, EventArgs e)
		{

		}

		private void Button_Cancel_Click(object sender, EventArgs e)
		{

		}

		protected PipeGroupDialog ShowPipeGroupDialog()
		{
			PipeGroupDialog autoLabelDialog = new PipeGroupDialog();
			AcApplication.ShowModalDialog(null, autoLabelDialog, false);
			return autoLabelDialog;
		}
	}
}
