using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Ironwill
{
	class Blocks
	{
		// --------------------------------------------------------------------------------	Block Name --------------------	Default Layer -------------	Deprecated Block Names	
		// Drawing Symbols
		public static BlockStruct PipeBreak =							new BlockStruct(	"PipeBreak",					Layer.PipeLabel.Get(),		"Break");
		public static BlockStruct PipeBreak_Single =					new BlockStruct(	"PipeBreak_Single",				Layer.PipeLabel.Get(),		"Break_Single");
		public static BlockStruct PipeSlope =							new BlockStruct(	"PipeLabel_SlopeArrow",			Layer.PipeLabel.Get()		);
		public static BlockStruct PipeLabel =							new BlockStruct(	"PipeLabel_DiameterLength",		Layer.PipeLabel.Get()		);
		
		// Drafting Symbols
		public static BlockStruct RevTriangle =							new BlockStruct(	"RevTriangle",					Layer.Note.Get()			);
		public static BlockStruct WarningTriangle =						new BlockStruct(	"WarningTriangle",				Layer.Note.Get()			);

		// Devices
		public static BlockStruct Device_WaterSupply =					new BlockStruct(	"D_WaterSupply",				Layer.SystemDevice.Get()	);
		
		// Fittings																			
		public static BlockStruct Fitting_GroovedReducingCoupling =		new BlockStruct(	"F_GroovedCoupling_Reducing",	Layer.SystemFitting.Get()	);
		public static BlockStruct Fitting_GroovedCoupling =				new BlockStruct(	"F_GroovedCoupling",			Layer.SystemFitting.Get()	);
		public static BlockStruct Fitting_Elbow =						new BlockStruct(	"F_Elbow",						Layer.SystemFitting.Get()	);
		public static BlockStruct Fitting_Tee =							new BlockStruct(	"F_Tee",						Layer.SystemFitting.Get()	);
		public static BlockStruct Fitting_Cap =							new BlockStruct(	"F_Cap",						Layer.SystemFitting.Get()	);
		public static BlockStruct Fitting_Riser =						new BlockStruct(	"F_Riser",						Layer.SystemFitting.Get()	);
		public static BlockStruct Fitting_ConcentricReducer =			new BlockStruct(	"F_ConcentricReducer",			Layer.SystemFitting.Get()	);
		public static BlockStruct Fitting_EccentricReducer =			new BlockStruct(	"F_EccentricReducer",			Layer.SystemFitting.Get()	);

		/*
		// Sprinklers
		public static BlockStruct Sprinkler_Head_01 = 					new BlockStruct(	"S_Head_01",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Slash =				new BlockStruct(	"S_Head_01_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Cross = 			new BlockStruct(	"S_Head_01_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_EC = 				new BlockStruct(	"S_Head_01_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_EC_Slash = 			new BlockStruct(	"S_Head_01_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_EC_Cross = 			new BlockStruct(	"S_Head_01_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Dry = 				new BlockStruct(	"S_Head_01_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Star = 				new BlockStruct(	"S_Head_01_Star",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02 = 					new BlockStruct(	"S_Head_02",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Slash = 			new BlockStruct(	"S_Head_02_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Cross = 			new BlockStruct(	"S_Head_02_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_EC = 				new BlockStruct(	"S_Head_02_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_EC_Slash = 			new BlockStruct(	"S_Head_02_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_EC_Cross = 			new BlockStruct(	"S_Head_02_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Dry = 				new BlockStruct(	"S_Head_02_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Star = 				new BlockStruct(	"S_Head_02_Star",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03 = 					new BlockStruct(	"S_Head_03",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Slash = 			new BlockStruct(	"S_Head_03_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Cross = 			new BlockStruct(	"S_Head_03_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_EC = 				new BlockStruct(	"S_Head_03_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_EC_Slash = 			new BlockStruct(	"S_Head_03_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_EC_Cross = 			new BlockStruct(	"S_Head_03_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Dry = 				new BlockStruct(	"S_Head_03_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Star = 				new BlockStruct(	"S_Head_03_Star",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04 = 					new BlockStruct(	"S_Head_04",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Slash = 			new BlockStruct(	"S_Head_04_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Cross = 			new BlockStruct(	"S_Head_04_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_EC = 				new BlockStruct(	"S_Head_04_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_EC_Slash = 			new BlockStruct(	"S_Head_04_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_EC_Cross = 			new BlockStruct(	"S_Head_04_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Dry = 				new BlockStruct(	"S_Head_04_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Star = 				new BlockStruct(	"S_Head_04_Star",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05 = 					new BlockStruct(	"S_Head_05",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Slash = 			new BlockStruct(	"S_Head_05_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Cross = 			new BlockStruct(	"S_Head_05_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_EC = 				new BlockStruct(	"S_Head_05_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_EC_Slash = 			new BlockStruct(	"S_Head_05_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_EC_Cross = 			new BlockStruct(	"S_Head_05_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Dry = 				new BlockStruct(	"S_Head_05_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Star = 				new BlockStruct(	"S_Head_05_Star",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06 = 					new BlockStruct(	"S_Head_06",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Slash = 			new BlockStruct(	"S_Head_06_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Cross = 			new BlockStruct(	"S_Head_06_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_EC = 				new BlockStruct(	"S_Head_06_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_EC_Slash = 			new BlockStruct(	"S_Head_06_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_EC_Cross = 			new BlockStruct(	"S_Head_06_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Dry = 				new BlockStruct(	"S_Head_06_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Star = 				new BlockStruct(	"S_Head_06_Star",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07 = 					new BlockStruct(	"S_Head_07",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Slash = 			new BlockStruct(	"S_Head_07_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Cross = 			new BlockStruct(	"S_Head_07_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_EC = 				new BlockStruct(	"S_Head_07_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_EC_Slash = 			new BlockStruct(	"S_Head_07_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_EC_Cross = 			new BlockStruct(	"S_Head_07_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Dry = 				new BlockStruct(	"S_Head_07_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Star = 				new BlockStruct(	"S_Head_07_Star",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08 = 					new BlockStruct(	"S_Head_08",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Slash = 			new BlockStruct(	"S_Head_08_Slash",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Cross = 			new BlockStruct(	"S_Head_08_Cross",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_EC = 				new BlockStruct(	"S_Head_08_EC",					Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_EC_Slash = 			new BlockStruct(	"S_Head_08_EC_Slash",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_EC_Cross = 			new BlockStruct(	"S_Head_08_EC_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Dry = 				new BlockStruct(	"S_Head_08_Dry",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Star = 				new BlockStruct(	"S_Head_08_Star",				Layer.SystemHead.Get()		);

		public static BlockStruct Sprinkler_SW_01 = 					new BlockStruct(	"S_Sidewall_01",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Plus = 				new BlockStruct(	"S_Sidewall_01_Plus",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Cross = 				new BlockStruct(	"S_Sidewall_01_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_EC = 					new BlockStruct(	"S_Sidewall_01_EC",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Dry = 				new BlockStruct(	"S_Sidewall_01_Dry",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Vert = 				new BlockStruct(	"S_Sidewall_01_Vert",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02 = 					new BlockStruct(	"S_Sidewall_02",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Plus = 				new BlockStruct(	"S_Sidewall_02_Plus",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Cross = 				new BlockStruct(	"S_Sidewall_02_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_EC = 					new BlockStruct(	"S_Sidewall_02_EC",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Dry = 				new BlockStruct(	"S_Sidewall_02_Dry",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Vert = 				new BlockStruct(	"S_Sidewall_02_Vert",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03 = 					new BlockStruct(	"S_Sidewall_03",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Plus = 				new BlockStruct(	"S_Sidewall_03_Plus",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Cross = 				new BlockStruct(	"S_Sidewall_03_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_EC = 					new BlockStruct(	"S_Sidewall_03_EC",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Dry = 				new BlockStruct(	"S_Sidewall_03_Dry",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Vert = 				new BlockStruct(	"S_Sidewall_03_Vert",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04 = 					new BlockStruct(	"S_Sidewall_04",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Plus = 				new BlockStruct(	"S_Sidewall_04_Plus",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Cross = 				new BlockStruct(	"S_Sidewall_04_Cross",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_EC = 					new BlockStruct(	"S_Sidewall_04_EC",				Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Dry = 				new BlockStruct(	"S_Sidewall_04_Dry",			Layer.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Vert = 				new BlockStruct(	"S_Sidewall_04_Vert",			Layer.SystemHead.Get()		);
		*/
	}

	public class BlockStruct
	{
		private string Name
		{
			get;
			set;
		}

		private string DefaultLayer
		{
			get;
			set;
		}

		private string[] DeprecatedNames
		{
			get;
			set;
		}

		public BlockStruct(string name, string defaultLayer, params string[] deprecatedNames)
		{
			Name = name;
			DefaultLayer = defaultLayer;
			DeprecatedNames = deprecatedNames;
		}

		public string Get()
		{
			var database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForWrite) as BlockTable;

				if (blockTable == null)
				{
					transaction.Abort();
					return "";
				}

				if (blockTable.Has(Name))
				{
					transaction.Commit();
					return Name;
				}
				else
				{
					foreach (string deprecatedName in DeprecatedNames)
					{
						if (blockTable.Has(deprecatedName))
						{
							transaction.Commit();
							return deprecatedName;
						}
					}

					Session.Log("WARNING: Required block " + Name + " was not found");
					return "";
				}
			}
		}
	}
}
