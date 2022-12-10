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
		const string GroupName = "Name";
		const string Mains = "Mains";
		const string Branchlines = "Branchlines";
		const string BranchlineRisers = "BranchlineRisers";
		const string Armovers = "Armovers";
		const string Drains = "Drains";

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
			CreateOrUpdateGroup(
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

		protected void CreateOrUpdateGroup(string groupName, string mainsLabel, string branchlinesLabel, string branchlineRisersLabel, string armoversLabel, string drainsLabel)
		{
			using (DocumentLock x = Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					DBDictionary pipeGroupsDictionary = Commands.AutoLabelNew.GetPipeGroupsDictionary(transaction);

					DBDictionary pipeGroupDictionary = null;

					// Check to see if this group already exists
					foreach (DBDictionaryEntry dicEntry in pipeGroupsDictionary)
					{
						DBDictionary candidateGroupDictionary = transaction.GetObject(dicEntry.Value, OpenMode.ForRead) as DBDictionary;

						if (candidateGroupDictionary != null)
						{
							string name = string.Empty;

							if (XRecordLibrary.ReadXRecord(transaction, candidateGroupDictionary, "Name", ref name))
							{
								if (name == groupName)
								{
									pipeGroupDictionary = candidateGroupDictionary;
									break;
								}
							}
						}
					}

					if (pipeGroupDictionary == null)
					{
						int groupId = Drawing.GenerateUniqueID(transaction);

						pipeGroupDictionary = XRecordLibrary.GetNamedDictionary(transaction, groupId.ToString(), pipeGroupsDictionary);

						XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, GroupName, groupName);
						
						XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Mains, mainsLabel);
						XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Branchlines, branchlinesLabel);
						XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, BranchlineRisers, branchlineRisersLabel);
						XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Armovers, armoversLabel);
						XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Drains, drainsLabel);
					}
					else
					{
						string oldMainsLabel = string.Empty;
						string oldBranchlinesLabel = string.Empty;
						string oldBranchlineRisersLabel = string.Empty;
						string oldArmoversLabel = string.Empty;
						string oldDrainsLabel = string.Empty;

						XRecordLibrary.ReadXRecord(transaction, pipeGroupDictionary, Mains, ref oldMainsLabel);
						XRecordLibrary.ReadXRecord(transaction, pipeGroupDictionary, Branchlines, ref oldMainsLabel);
						XRecordLibrary.ReadXRecord(transaction, pipeGroupDictionary, BranchlineRisers, ref oldMainsLabel);
						XRecordLibrary.ReadXRecord(transaction, pipeGroupDictionary, Armovers, ref oldMainsLabel);
						XRecordLibrary.ReadXRecord(transaction, pipeGroupDictionary, Drains, ref oldMainsLabel);

						if (oldMainsLabel != mainsLabel)
						{
							XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Mains, mainsLabel);
							BroadcastUpdate();
						}

						if (oldBranchlinesLabel != branchlinesLabel)
						{
							XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Branchlines, branchlinesLabel);
							BroadcastUpdate();
						}

						if (oldBranchlineRisersLabel != branchlineRisersLabel)
						{
							XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, BranchlineRisers, branchlineRisersLabel);
							BroadcastUpdate();
						}

						if (oldArmoversLabel != armoversLabel)
						{
							XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Armovers, armoversLabel);
							BroadcastUpdate();
						}

						if (oldDrainsLabel != drainsLabel)
						{
							XRecordLibrary.WriteXRecord(transaction, pipeGroupDictionary, Drains, drainsLabel);
							BroadcastUpdate();
						}
					}

					transaction.Commit();
				}
			}
		}

		void BroadcastUpdate()
		{ 
		}
	}
}
