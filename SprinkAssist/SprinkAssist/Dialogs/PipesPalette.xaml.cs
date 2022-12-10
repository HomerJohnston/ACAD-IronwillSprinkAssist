using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ironwill.Dialogs
{
	internal struct PipeFamily
	{
		public int id;
		public string name;
	}

	/// <summary>
	/// Interaction logic for PipesPalette.xaml
	/// </summary>
	public partial class PipesPalette : UserControl
	{
		List<PipeFamily> pipeFamiliesList = new List<PipeFamily>();

		public PipesPalette()
		{
			InitializeComponent();

			ListBox_Families.ItemsSource = pipeFamiliesList;
			ListBox_Families.DisplayMemberPath = "name";

			UpdateLists();
		}

		private void Button_NewFamily_Click(object sender, RoutedEventArgs e)
		{
			using (DocumentLock doclock = Session.LockDocument())
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					int familyID = Drawing.GenerateUniqueID(transaction);

					PipeFamily pipeFamily1 = new PipeFamily();
					pipeFamily1.id = familyID;
					pipeFamily1.name = "New Family";

					pipeFamiliesList.Add(pipeFamily1);

					DBDictionary pipeFamiliesDictionary = XRecordLibrary.GetNamedDictionary(transaction, "PipeFamilies");

					DBDictionary pipeFamilyDictionary = XRecordLibrary.GetNamedDictionary(transaction, familyID.ToString(), pipeFamiliesDictionary);

					XRecordLibrary.WriteXRecord(transaction, pipeFamilyDictionary, "FamilyName", "NewFamily");

					UpdateLists(transaction);

					transaction.Commit();
				}
			}
		}

		private void Button_NewGroup_Click(object sender, RoutedEventArgs e)
		{
		}

		private void UpdateLists()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				UpdateLists(transaction);
			}
		}

		private void UpdateLists(Transaction transaction)
		{
			DBDictionary pipeFamiliesDictionary = XRecordLibrary.GetNamedDictionary(transaction, "PipeFamilies");

			UpdatePipeFamiliesListBox(transaction, pipeFamiliesDictionary);
		}

		private void UpdatePipeFamiliesListBox(Transaction transaction, DBDictionary pipeFamiliesDictionary)
		{
			//pipeFamiliesList.Clear();

			foreach (DBDictionaryEntry pipeFamily in pipeFamiliesDictionary)
			{
				DBDictionary pipeFamilyDictionary = transaction.GetObject(pipeFamily.Value, OpenMode.ForRead) as DBDictionary;

				if (pipeFamiliesDictionary == null)
				{
					continue;
				}

				string pipeFamilyName = GetPipeFamilyName(transaction, pipeFamilyDictionary);


			//	ListBox_Families.Items.Add(pipeFamilyName);
			}

			Session.LogDebug("There are {0} families", ListBox_Families.Items.Count.ToString());
		}

		private void UpdatePipeGroupsListView()
		{
		}

		private string GetPipeFamilyName(Transaction transaction, DBDictionary pipeFamilyDictionary)
		{
			string name = "UNKNOWN";

			XRecordLibrary.ReadXRecord(transaction, pipeFamilyDictionary, "FamilyName", ref name);

			return name;
		}
	}
}
