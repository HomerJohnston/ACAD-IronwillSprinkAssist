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
		public BoolSetting showLength;

		public BoolSetting omitLengthFromShort;

		public DoubleSetting omitLengthFromShortThreshold;

		public BoolSetting breakAtLineEnds;

		public BoolSetting breakAtLineIntersections;

		public BoolSetting breakAtFittings;

		public BoolSetting breakAtSprinklers;

		public BoolSetting connectAcrossBreaks;

		public DoubleSetting connectAcrossBreaksThreshold;

		public BoolSetting ignoreShortLines;

		public DoubleSetting ignoreShortLinesThreshold;

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

			showLength = new BoolSetting(path, "ShowLength", true);
			omitLengthFromShort = new BoolSetting(path, "OmitLengthFromShort", true);
			omitLengthFromShortThreshold = new DoubleSetting(path, "OmitLengthFromShortThreshold", 0.0);

			breakAtLineEnds = new BoolSetting(path, "BreakAtLineEnds", true);
			breakAtLineIntersections = new BoolSetting(path, "BreakAtLineIntersections", false);
			breakAtFittings = new BoolSetting(path, "BreakAtFittings", true);
			breakAtSprinklers = new BoolSetting(path, "BreakAtSprinklers", true);

			connectAcrossBreaks = new BoolSetting(path, "ConnectAcrossBreaks", true);
			connectAcrossBreaksThreshold = new DoubleSetting(path, "ConnectAcrossBreaksThreshold", 0.0);
			ignoreShortLines = new BoolSetting(path, "IgnoreShortLines", true);
			ignoreShortLinesThreshold = new DoubleSetting(path, "IgnoreShortLinesThreshold", 0.0);
		}
	}
}
