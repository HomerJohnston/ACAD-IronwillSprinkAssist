using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands
{
	internal struct PipeConnection : IComparable
	{
		public Line focusLine
		{
			get; 
			private set; 
		}

		public Line targetLine
		{
			get;
			private set; 
		}

		public BlockReference fitting
		{
			get;
			private set;
		}

		public double anchorPoint
		{
			get;
			private set;
		}

		public PipeConnection(Line inFocusLine, Line inTargetLine, BlockReference inFitting, double inAnchorPoint)
		{
			focusLine = inFocusLine;
			targetLine = inTargetLine;
			fitting = inFitting;
			anchorPoint = inAnchorPoint;
		}

		public int CompareTo(object? obj)
		{
			if (obj == null)
			{
				return 1;
			}

			PipeConnection otherPipeConnection = (PipeConnection)obj;

			return otherPipeConnection.anchorPoint.CompareTo(anchorPoint);
		}
	}
}
