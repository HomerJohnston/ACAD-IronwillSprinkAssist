using Autodesk.AutoCAD.DatabaseServices;
using System.Collections;
using Ironwill.Definitions;

using Ironwill.Utility.AutoCAD;

namespace Ironwill.Utility.AutoCAD
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

				string? ValAsString = entry.Value as string;

				if (ValAsString != null)
				{
					result.Add((string)entry.Key, ValAsString);
				}
			}

			return result;
		}

		public static string GetCustomProperty(this Database database, string propertyName)
		{
			DatabaseSummaryInfoBuilder summaryInfoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
			IDictionary customProperties = summaryInfoBuilder.CustomPropertyTable;

			string? result = customProperties[propertyName] as string;

			if (result == null)
			{
				throw new Exception($"Custom property {propertyName} not found in drawing summary info!");
			}

			return result;
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
				Line? testLine = dbObject as Line;

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

				Entity? entity = dbObject as Entity;
				
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

		public static List<Entity> GetAllSystemEntities(Transaction transaction)
		{
			List<Entity> entities = new List<Entity>();

			GetMultipleEntities(transaction, new EntityGetter(entities,
				(Entity e) => 
				{
					return Layer.SystemLayers.Contains(e.Layer);
				}
			));

			return entities;
		}
	}
}
