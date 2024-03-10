using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	public class Global
	{
		protected List<string> PipeLayers = new List<string>
		{
			"SpkPipe_Armover", 
			"SpkPipe_Branchline", 
			"SpkPipe_Main", 
			"SpkPipe_Drain" 
		};
	}

	// Each group contains settings for each pipe type
	public class PipeGroup
	{
		// Diameter labels for each pipe layer
		public OBSOLETEStringSetting armoverLabel;

		public OBSOLETEStringSetting branchlineLabel;

		public OBSOLETEStringSetting mainLabel;

		public OBSOLETEStringSetting drainLabel;

		// Generation properties
		public OBSOLETEBoolSetting showLength;

		public OBSOLETEBoolSetting omitLengthFromShort;

		public OBSOLETEDoubleSetting omitLengthFromShortThreshold;

		public OBSOLETEBoolSetting breakAtLineEnds;

		public OBSOLETEBoolSetting breakAtLineIntersections;

		public OBSOLETEBoolSetting breakAtFittings;

		public OBSOLETEBoolSetting breakAtSprinklers;

		public OBSOLETEBoolSetting connectAcrossBreaks;

		public OBSOLETEDoubleSetting connectAcrossBreaksThreshold;

		public OBSOLETEBoolSetting ignoreShortLines;

		public OBSOLETEDoubleSetting ignoreShortLinesThreshold;

		public static PipeGroup Get(string name)
		{
			return new PipeGroup(name);
		}

		private PipeGroup(string name)
		{
			OBSOLETEDictionaryPath path = new OBSOLETEDictionaryPath("PipeGroups", name);

			armoverLabel = new OBSOLETEStringSetting(path, "ArmoverLabel", "");
			branchlineLabel = new OBSOLETEStringSetting(path, "BranchlineLabel", "");
			mainLabel = new OBSOLETEStringSetting(path, "MainLabel", "");
			drainLabel = new OBSOLETEStringSetting(path, "DrainLabel", "");

			showLength = new OBSOLETEBoolSetting(path, "ShowLength", true);
			omitLengthFromShort = new OBSOLETEBoolSetting(path, "OmitLengthFromShort", true);
			omitLengthFromShortThreshold = new OBSOLETEDoubleSetting(path, "OmitLengthFromShortThreshold", 0.0);

			breakAtLineEnds = new OBSOLETEBoolSetting(path, "BreakAtLineEnds", true);
			breakAtLineIntersections = new OBSOLETEBoolSetting(path, "BreakAtLineIntersections", false);
			breakAtFittings = new OBSOLETEBoolSetting(path, "BreakAtFittings", true);
			breakAtSprinklers = new OBSOLETEBoolSetting(path, "BreakAtSprinklers", true);

			connectAcrossBreaks = new OBSOLETEBoolSetting(path, "ConnectAcrossBreaks", true);
			connectAcrossBreaksThreshold = new OBSOLETEDoubleSetting(path, "ConnectAcrossBreaksThreshold", 0.0);
			ignoreShortLines = new OBSOLETEBoolSetting(path, "IgnoreShortLines", true);
			ignoreShortLinesThreshold = new OBSOLETEDoubleSetting(path, "IgnoreShortLinesThreshold", 0.0);
		}
	}
}
