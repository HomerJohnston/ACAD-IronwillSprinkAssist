using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Ironwill.Commands.Help;

[assembly: CommandClass(typeof(Ironwill.Commands.XRoom))]

namespace Ironwill.Commands
{
	internal class XRoom : SprinkAssistCommand
	{
		const string bathroomKeyword = "Bathroom";
		const string closetKeyword = "Closet";
		const string noneKeyword = "None";

		CommandSetting<string> roomTypeSetting;

		public XRoom()
		{
			roomTypeSetting = new CommandSetting<string>("RoomType", bathroomKeyword, cmdSettings);
		}

		bool IsValid(PromptPointResult promptPointResult)
		{
			if (promptPointResult == null)
			{
				return false;
			}

			PromptStatus status = promptPointResult.Status;

			if (status == PromptStatus.Error || status == PromptStatus.Modeless || status == PromptStatus.Other)
			{
				return false;
			}

			return true;
		}

		protected Point3d GetPoint(Transaction transaction, string prompt, out bool bStopCommand, out bool bAbortIteration)
		{
			PromptPointOptions promptPointOptions = new PromptPointOptions(Environment.NewLine + prompt + " (" + roomTypeSetting.Get(transaction) + ")");
			promptPointOptions.Message = "Check " + roomTypeSetting.Get(transaction);
			promptPointOptions.AllowNone = true;
			promptPointOptions.Keywords.Add("Closet");
			promptPointOptions.Keywords.Add("Bathroom");
			promptPointOptions.Keywords.Add("None");
			//promptPointOptions.Keywords.Default = DrawingSettings.XRoom;

			PromptPointResult promptPointResult = null;

			while (!IsValid(promptPointResult))
			{
				promptPointResult = Session.GetEditor().GetPoint(promptPointOptions);
			}

			Session.Log(promptPointResult.StringResult);

			switch (promptPointResult.Status)
			{
				case PromptStatus.Keyword:
					Session.LogDebug("Keyword");
					roomTypeSetting.Set(transaction, promptPointResult.StringResult);
					bStopCommand = false;
					bAbortIteration = true;
					return new Point3d();
				case PromptStatus.OK:
					bStopCommand = false;
					bAbortIteration = false;
					return promptPointResult.Value.TransformBy(Session.GetEditor().CurrentUserCoordinateSystem);
				case PromptStatus.Cancel:
					Session.LogDebug("Cancel");
					bStopCommand = true;
					bAbortIteration = true;
					return new Point3d();
				case PromptStatus.None:
					Session.LogDebug("None");
					bStopCommand = true;
					bAbortIteration = true;
					return new Point3d();
				default:
					Session.LogDebug("Default");
					bStopCommand = false;
					bAbortIteration = true;
					return new Point3d();
			}
		}

		[CommandDescription("Simple command to draw X's over rectangular rooms.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "XRoom", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoMultiple)]
		public void XRoomCmd()
		{
			bool bStopCommand = false;

			while (!bStopCommand)
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					bool bAbortIteration = false;

					Point3d firstPoint = new Point3d();
					Point3d secondPoint = new Point3d();

					if (!bAbortIteration)
					{
						firstPoint = GetPoint(transaction, "Pick first corner", out bStopCommand, out bAbortIteration);
					}

					if (!bAbortIteration)
					{
						secondPoint = GetPoint(transaction, "Pick opposite corner", out bStopCommand, out bAbortIteration);
					}

					if (!bAbortIteration)
					{
						Matrix3d ucs = Session.GetEditor().CurrentUserCoordinateSystem;

						Point3d firstPointUCS = firstPoint.TransformBy(ucs.Inverse());
						Point3d secondPointUCS = secondPoint.TransformBy(ucs.Inverse());

						bool bNoSprinklerRequired = true;

						if (GetArea(firstPointUCS, secondPointUCS) > GetAreaLimit(transaction))
						{
							bNoSprinklerRequired = false;
						}

						if (GetMinDimension(firstPointUCS, secondPointUCS) > GetMinDimensionLimit(transaction))
						{
							bNoSprinklerRequired = false;
						}

						Point3d thirdPointUCS = new Point3d(firstPointUCS.X, secondPointUCS.Y, 0.0);
						Point3d fourthPointUCS = new Point3d(secondPointUCS.X, firstPointUCS.Y, 0.0);

						Point3d thirdPoint = thirdPointUCS.TransformBy(ucs);
						Point3d fourthPoint = fourthPointUCS.TransformBy(ucs);

						if (bNoSprinklerRequired)
						{
							Line line1 = new Line(firstPoint, secondPoint);
							Line line2 = new Line(thirdPoint, fourthPoint);

							line1.Layer = Layer.Note.Get();
							line2.Layer = Layer.Note.Get();

							Session.AddNewObject(transaction, line1);
							Session.AddNewObject(transaction, line2);
						}
						else
						{
							double offset = 0.0;

							switch (Session.GetPrimaryUnits())
							{
								case DrawingUnits.Imperial:
									offset = 8;
									break;
								case DrawingUnits.Metric:
									offset = 200;
									break;
								case DrawingUnits.Undefined:
									continue;
							}

							Point3d LL = new Point3d(Math.Min(firstPointUCS.X, secondPointUCS.X), Math.Min(firstPointUCS.Y, secondPointUCS.Y), 0.0);
							Point3d LR = new Point3d(Math.Max(firstPointUCS.X, secondPointUCS.X), Math.Min(firstPointUCS.Y, secondPointUCS.Y), 0.0);
							Point3d UL = new Point3d(Math.Min(firstPointUCS.X, secondPointUCS.X), Math.Max(firstPointUCS.Y, secondPointUCS.Y), 0.0);
							Point3d UR = new Point3d(Math.Max(firstPointUCS.X, secondPointUCS.X), Math.Max(firstPointUCS.Y, secondPointUCS.Y), 0.0);

							Point3d CL = Avg(LL, UL);
							Point3d CR = Avg(LR, UR);
							Point3d UC = Avg(UL, UR);
							Point3d LC = Avg(LL, LR);

							Point3d CC = Avg(LL, UR);

							CreatePoint(transaction, LL, +offset, +offset, ucs);
							CreatePoint(transaction, LR, -offset, +offset, ucs);
							CreatePoint(transaction, UL, +offset, -offset, ucs);
							CreatePoint(transaction, UR, -offset, -offset, ucs);

							CreatePoint(transaction, CL, +offset, 0.0, ucs);
							CreatePoint(transaction, CR, -offset, 0.0, ucs);
							CreatePoint(transaction, UC, 0.0, -offset, ucs);
							CreatePoint(transaction, LC, 0.0, +offset, ucs);

							CreatePoint(transaction, CC, 0.0, 0.0, ucs);
						}
					}
					transaction.Commit();
				}
			}
		}

		protected void CreatePoint(Transaction transaction, Point3d point, double offsetX, double offsetY, Matrix3d ucs)
		{
			Point3d P1 = new Point3d(point.X + offsetX, point.Y + offsetY, 0.0).TransformBy(ucs);
			DBPoint p1 = new DBPoint(P1);
			p1.Layer = Layer.DraftAid.Get();
			Session.AddNewObject(transaction, p1);
		}

		protected Point3d Avg(Point3d p1, Point3d p2)
		{
			return new Point3d(0.5 * (p1.X + p2.X), 0.5 * (p1.Y + p2.Y), 0.5 * (p1.Z + p2.Z));

		}

		protected double GetArea(Point3d firstPoint, Point3d secondPoint)
		{
			double area = Math.Abs((secondPoint.X - firstPoint.X) * (secondPoint.Y - firstPoint.Y));
			Session.Log("Area: " + SprinkMath.FormatArea(area, Session.GetPrimaryUnits()) + " [" + SprinkMath.FormatArea(area, Session.GetSecondaryUnits()) + "]");
			return area;
		}

		protected double GetAreaLimit(Transaction transaction)
		{
			DrawingUnits units = Session.GetPrimaryUnits();

			if (units == DrawingUnits.Undefined)
			{
				Session.Log("Warning: undefined drawing units, can't check area limit");
				return -1.0;
			}

			switch (roomTypeSetting.Get(transaction))
			{
				case "Bathroom":
					switch (units)
					{
						case DrawingUnits.Metric:
							return 5109667.0; // Square mm
						case DrawingUnits.Imperial:
							return 7920.0; // Square inches
					}
					return -1.0;
				case "Closet":
					switch (units)
					{
						case DrawingUnits.Metric:
							return 2232558.0; // Square mm
						case DrawingUnits.Imperial:
							return 3456.0; // Square inches
					}
					return -1.0;
				default:
					{
						return double.MaxValue;
					}
			}
		}

		protected double GetMinDimension(Point3d firstPoint, Point3d secondPoint)
		{
			return Math.Min(Math.Abs(secondPoint.X - firstPoint.X), Math.Abs(secondPoint.Y - firstPoint.Y));
		}

		protected double GetMinDimensionLimit(Transaction transaction)
		{
			DrawingUnits units = Session.GetPrimaryUnits();

			if (units == DrawingUnits.Undefined)
			{
				Session.Log("Warning: undefined drawing units, can't check area limit");
				return -1.0;
			}

			switch (roomTypeSetting.Get(transaction))
			{
				case "Bathroom":
					return double.MaxValue;
				case "Closet":
					switch (units)
					{
						case DrawingUnits.Metric:
							return 914.0; // mm
						case DrawingUnits.Imperial:
							return 36.0; // inches
					}
					return -1.0;
				default:
					return double.MaxValue;
			}
		}
	}
}
