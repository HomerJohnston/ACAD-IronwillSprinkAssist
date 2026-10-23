using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms.Integration;

[assembly: CommandClass(typeof(Ironwill.Commands.DockableTabTest))]

namespace Ironwill.Commands
{
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	internal class DockableTabTest
	{
		private static PaletteSet palleteSet;
		private static MyUserControl myUserControl;

		[CommandMethod("ShowMyDockableTab")]
		public void ShowMyDockableTab()
		{
			if (palleteSet == null || true)
			{
				// Create a new PaletteSet
				palleteSet = new PaletteSet("My Dockable Tab");

				palleteSet.Style = PaletteSetStyles.ShowPropertiesMenu | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
				palleteSet.DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right);
				palleteSet.MinimumSize = new System.Drawing.Size(200, 200);

				Table table = MakeTable();

				// Create a new instance of your user control
				myUserControl = new MyUserControl(table);
				
				// Add the user control to the PaletteSet
				palleteSet.Add("My User Control", myUserControl);

				palleteSet.Style = PaletteSetStyles.ShowPropertiesMenu | PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton;
				palleteSet.Dock = DockSides.Right;
			}

			palleteSet.Visible = true;
		}

		Table MakeTable()
		{
			var table = new Table();

			table.CellSpacing = 10;
			table.Background = System.Windows.Media.Brushes.White;

			int cols = 6;
			for (int x = 0; x < cols; ++x)
			{
				table.Columns.Add(new TableColumn());

				if (x % 2 == 0)
				{
					table.Columns[x].Background = System.Windows.Media.Brushes.Beige;
				}
				else
				{
					table.Columns[x].Background = System.Windows.Media.Brushes.LightSteelBlue;
				}
			}

			TableRow titleRow = new TableRow();
			titleRow.Background = System.Windows.Media.Brushes.Silver;
			titleRow.FontSize = 40;
			titleRow.FontWeight = FontWeights.Bold;

			TableCell titleCell = new TableCell(new Paragraph(new Run("Test Title")));
			titleRow.Cells.Add(titleCell);
			titleCell.ColumnSpan = 6;

			TableRow headerRow = new TableRow();
			headerRow.FontSize = 18;
			headerRow.FontWeight = FontWeights.Bold;
			headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Product"))));
			headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Quarter 1"))));
			headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Quarter 2"))));
			headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Quarter 3"))));

			System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox();
			BlockUIContainer asd = new BlockUIContainer(tb);
			tb.TextChanged += Test;

			TableRow dataRow = new TableRow();
			dataRow.FontSize = 12;
			dataRow.FontWeight = FontWeights.Normal;
			dataRow.Cells.Add(new TableCell(new Paragraph(new Run("Data 1"))));
			dataRow.Cells.Add(new TableCell(new Paragraph(new Run("Data 2"))));
			dataRow.Cells.Add(new TableCell(new Paragraph(new Run("Data 3"))));
			dataRow.Cells.Add(new TableCell(asd));

			TableRowGroup rowGroup = new TableRowGroup();
			table.RowGroups.Add(rowGroup);
			rowGroup.Rows.Add(titleRow);
			rowGroup.Rows.Add(headerRow);
			rowGroup.Rows.Add(dataRow);

			return table;
		}

		void Test(object sender, TextChangedEventArgs e) 
		{
			Session.Log("Test");
		}
	}

	internal class MyUserControl : System.Windows.Forms.UserControl
	{
		public MyUserControl(Table table)
		{
			InitializeComponent(table);
		}

		void InitializeComponent(Table table)
		{
			SuspendLayout();
			
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Name = "MyUserControl";
			Size = new System.Drawing.Size(400, 300);
			ResumeLayout(false);

			var document = new FlowDocument();
			document.Blocks.Add(table);

			var viewer = new FlowDocumentScrollViewer
			{
				Document = document
			};

			var elementHost = new ElementHost
			{
				Dock = DockStyle.Fill,
				Child = viewer
			};

			Controls.Add(elementHost);
		}
	}
}
