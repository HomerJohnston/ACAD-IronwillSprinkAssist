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
		public static BlockStruct PipeBreak =							new BlockStruct(	"PipeBreak",					Layers.PipeLabel.Get(),		"Break");
		public static BlockStruct PipeBreak_Single =					new BlockStruct(	"PipeBreak_Single",				Layers.PipeLabel.Get(),		"Break_Single");
		public static BlockStruct PipeSlope =							new BlockStruct(	"PipeLabel_SlopeArrow",			Layers.PipeLabel.Get()		);
		public static BlockStruct PipeLabel =							new BlockStruct(	"PipeLabel_DiameterLength",		Layers.PipeLabel.Get()		);
		
		// Drafting Symbols
		public static BlockStruct RevTriangle =							new BlockStruct(	"RevTriangle",					Layers.Note.Get()			);
		public static BlockStruct WarningTriangle =						new BlockStruct(	"WarningTriangle",				Layers.Note.Get()			);

		// Devices
		public static BlockStruct Device_WaterSupply =					new BlockStruct(	"D_WaterSupply",				Layers.SystemDevice.Get()	);
		
		// Fittings																			
		public static BlockStruct Fitting_GroovedReducingCoupling =		new BlockStruct(	"F_GroovedCoupling_Reducing",	Layers.SystemFitting.Get()	);
		public static BlockStruct Fitting_GroovedCoupling =				new BlockStruct(	"F_GroovedCoupling",			Layers.SystemFitting.Get()	);
		public static BlockStruct Fitting_Elbow =						new BlockStruct(	"F_Elbow",						Layers.SystemFitting.Get()	);
		public static BlockStruct Fitting_Tee =							new BlockStruct(	"F_Tee",						Layers.SystemFitting.Get()	);
		public static BlockStruct Fitting_Cap =							new BlockStruct(	"F_Cap",						Layers.SystemFitting.Get()	);
		public static BlockStruct Fitting_Riser =						new BlockStruct(	"F_Riser",						Layers.SystemFitting.Get()	);
		public static BlockStruct Fitting_ConcentricReducer =			new BlockStruct(	"F_ConcentricReducer",			Layers.SystemFitting.Get()	);
		public static BlockStruct Fitting_EccentricReducer =			new BlockStruct(	"F_EccentricReducer",			Layers.SystemFitting.Get()	);

		// Sprinklers
		public static BlockStruct Sprinkler_Head_01 = 					new BlockStruct(	"S_Head_01",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Slash =				new BlockStruct(	"S_Head_01_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Cross = 			new BlockStruct(	"S_Head_01_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_EC = 				new BlockStruct(	"S_Head_01_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_EC_Slash = 			new BlockStruct(	"S_Head_01_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_EC_Cross = 			new BlockStruct(	"S_Head_01_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Dry = 				new BlockStruct(	"S_Head_01_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_01_Star = 				new BlockStruct(	"S_Head_01_Star",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02 = 					new BlockStruct(	"S_Head_02",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Slash = 			new BlockStruct(	"S_Head_02_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Cross = 			new BlockStruct(	"S_Head_02_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_EC = 				new BlockStruct(	"S_Head_02_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_EC_Slash = 			new BlockStruct(	"S_Head_02_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_EC_Cross = 			new BlockStruct(	"S_Head_02_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Dry = 				new BlockStruct(	"S_Head_02_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_02_Star = 				new BlockStruct(	"S_Head_02_Star",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03 = 					new BlockStruct(	"S_Head_03",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Slash = 			new BlockStruct(	"S_Head_03_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Cross = 			new BlockStruct(	"S_Head_03_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_EC = 				new BlockStruct(	"S_Head_03_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_EC_Slash = 			new BlockStruct(	"S_Head_03_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_EC_Cross = 			new BlockStruct(	"S_Head_03_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Dry = 				new BlockStruct(	"S_Head_03_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_03_Star = 				new BlockStruct(	"S_Head_03_Star",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04 = 					new BlockStruct(	"S_Head_04",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Slash = 			new BlockStruct(	"S_Head_04_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Cross = 			new BlockStruct(	"S_Head_04_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_EC = 				new BlockStruct(	"S_Head_04_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_EC_Slash = 			new BlockStruct(	"S_Head_04_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_EC_Cross = 			new BlockStruct(	"S_Head_04_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Dry = 				new BlockStruct(	"S_Head_04_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_04_Star = 				new BlockStruct(	"S_Head_04_Star",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05 = 					new BlockStruct(	"S_Head_05",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Slash = 			new BlockStruct(	"S_Head_05_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Cross = 			new BlockStruct(	"S_Head_05_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_EC = 				new BlockStruct(	"S_Head_05_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_EC_Slash = 			new BlockStruct(	"S_Head_05_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_EC_Cross = 			new BlockStruct(	"S_Head_05_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Dry = 				new BlockStruct(	"S_Head_05_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_05_Star = 				new BlockStruct(	"S_Head_05_Star",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06 = 					new BlockStruct(	"S_Head_06",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Slash = 			new BlockStruct(	"S_Head_06_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Cross = 			new BlockStruct(	"S_Head_06_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_EC = 				new BlockStruct(	"S_Head_06_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_EC_Slash = 			new BlockStruct(	"S_Head_06_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_EC_Cross = 			new BlockStruct(	"S_Head_06_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Dry = 				new BlockStruct(	"S_Head_06_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_06_Star = 				new BlockStruct(	"S_Head_06_Star",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07 = 					new BlockStruct(	"S_Head_07",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Slash = 			new BlockStruct(	"S_Head_07_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Cross = 			new BlockStruct(	"S_Head_07_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_EC = 				new BlockStruct(	"S_Head_07_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_EC_Slash = 			new BlockStruct(	"S_Head_07_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_EC_Cross = 			new BlockStruct(	"S_Head_07_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Dry = 				new BlockStruct(	"S_Head_07_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_07_Star = 				new BlockStruct(	"S_Head_07_Star",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08 = 					new BlockStruct(	"S_Head_08",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Slash = 			new BlockStruct(	"S_Head_08_Slash",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Cross = 			new BlockStruct(	"S_Head_08_Cross",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_EC = 				new BlockStruct(	"S_Head_08_EC",					Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_EC_Slash = 			new BlockStruct(	"S_Head_08_EC_Slash",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_EC_Cross = 			new BlockStruct(	"S_Head_08_EC_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Dry = 				new BlockStruct(	"S_Head_08_Dry",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_Head_08_Star = 				new BlockStruct(	"S_Head_08_Star",				Layers.SystemHead.Get()		);

		public static BlockStruct Sprinkler_SW_01 = 					new BlockStruct(	"S_Sidewall_01",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Plus = 				new BlockStruct(	"S_Sidewall_01_Plus",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Cross = 				new BlockStruct(	"S_Sidewall_01_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_EC = 					new BlockStruct(	"S_Sidewall_01_EC",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Dry = 				new BlockStruct(	"S_Sidewall_01_Dry",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_01_Vert = 				new BlockStruct(	"S_Sidewall_01_Vert",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02 = 					new BlockStruct(	"S_Sidewall_02",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Plus = 				new BlockStruct(	"S_Sidewall_02_Plus",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Cross = 				new BlockStruct(	"S_Sidewall_02_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_EC = 					new BlockStruct(	"S_Sidewall_02_EC",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Dry = 				new BlockStruct(	"S_Sidewall_02_Dry",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_02_Vert = 				new BlockStruct(	"S_Sidewall_02_Vert",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03 = 					new BlockStruct(	"S_Sidewall_03",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Plus = 				new BlockStruct(	"S_Sidewall_03_Plus",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Cross = 				new BlockStruct(	"S_Sidewall_03_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_EC = 					new BlockStruct(	"S_Sidewall_03_EC",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Dry = 				new BlockStruct(	"S_Sidewall_03_Dry",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_03_Vert = 				new BlockStruct(	"S_Sidewall_03_Vert",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04 = 					new BlockStruct(	"S_Sidewall_04",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Plus = 				new BlockStruct(	"S_Sidewall_04_Plus",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Cross = 				new BlockStruct(	"S_Sidewall_04_Cross",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_EC = 					new BlockStruct(	"S_Sidewall_04_EC",				Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Dry = 				new BlockStruct(	"S_Sidewall_04_Dry",			Layers.SystemHead.Get()		);
		public static BlockStruct Sprinkler_SW_04_Vert = 				new BlockStruct(	"S_Sidewall_04_Vert",			Layers.SystemHead.Get()		);
	}
}
