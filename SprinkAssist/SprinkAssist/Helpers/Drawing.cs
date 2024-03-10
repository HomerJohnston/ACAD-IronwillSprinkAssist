using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	public static class Drawing
	{
		public static Dictionary<string, string> GetCustomProperties(this Database database)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			IDictionaryEnumerator dictionaryEnumerator = database.SummaryInfo.CustomProperties;

			while (dictionaryEnumerator.MoveNext())
			{
				DictionaryEntry entry = dictionaryEnumerator.Entry;
				result.Add((string)entry.Key, (string)entry.Value);
			}

			return result;
		}

		public static string GetCustomProperty(this Database database, string propertyName)
		{
			DatabaseSummaryInfoBuilder summaryInfoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
			IDictionary customProperties = summaryInfoBuilder.CustomPropertyTable;

			return (string)customProperties[propertyName];
		}

		public static void SetCustomProperty(this Database database, string key, string value)
		{
			DatabaseSummaryInfoBuilder summaryInfoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
			IDictionary customProperties = summaryInfoBuilder.CustomPropertyTable;

			if (customProperties.Contains(key))
			{
				customProperties[key] = value;
			}
			else
			{
				customProperties.Add(key, value);
			}

			database.SummaryInfo = summaryInfoBuilder.ToDatabaseSummaryInfo();
		}

		public static int GenerateUniqueID(Transaction transaction)
		{
			DBDictionary dictionary = XRecordLibrary.GetSprinkAssistMasterDictionary(transaction);

			int nextId = 0;

			XRecordLibrary.ReadXRecord<int>(transaction, dictionary, "GUID", ref nextId);

			XRecordLibrary.WriteXRecord(transaction, dictionary, "GUID", ++nextId);

			return nextId;
		}

		public static void Purge(this Database database)
		{

		}
	}
}
