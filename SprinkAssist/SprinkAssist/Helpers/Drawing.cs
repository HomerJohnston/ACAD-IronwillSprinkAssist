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

		public static List<Entity> GetAllPipeLines(Transaction transaction)
		{
			BlockTableRecord blockTableRecord = Session.GetModelSpaceBlockTableRecord(transaction);

			List<Entity> pipeLines = new List<Entity>();

			foreach (ObjectId objectId in blockTableRecord)
			{
				DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
				Line testLine = dbObject as Line;

				if (testLine == null)
				{
					continue;
				}

				if (Layer.PipeLayers.Contains(testLine.Layer))
				{
					pipeLines.Add(testLine);
				}
			}

			return pipeLines;
		}

		public delegate bool EntityMatchDelegate(Entity entity);

		public class EntityGetter
		{
			List<Entity> foundEntities = new List<Entity>();

			EntityMatchDelegate matchFunction;

			public EntityGetter(List<Entity> foundEntities, EntityMatchDelegate matchFunction)
			{
				this.foundEntities = foundEntities;
				this.matchFunction = matchFunction;
			}

			public void TryGet(Entity entity)
			{
				if (matchFunction(entity))
				{
					foundEntities.Add(entity);
				}
			}
		}

		/** Function to find multiple types of entities in a single pass. */
		public static void GetMultipleEntities(Transaction transaction, params EntityGetter[] stuffGetterClasses)
		{
			BlockTableRecord blockTableRecord = Session.GetModelSpaceBlockTableRecord(transaction);

			foreach (ObjectId objectId in blockTableRecord)
			{
				DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForRead);

				Entity entity = dbObject as Entity;
				
				if (entity == null)
				{
					continue;
				}

				foreach (EntityGetter stuffGetter in stuffGetterClasses)
				{
					stuffGetter.TryGet(entity);
				}
			}
		}
	}
}
