﻿//
// TableLayout.cs
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

//#define TABLE_DEBUG

using System;
using System.Drawing;
using System.Collections.Generic;

namespace System.Windows.Forms.Layout
{
	internal class TableLayout : LayoutEngine
	{
		internal static Control dummy_control = new Control ("Dummy");	// Used as a placeholder for row/col spans

		public TableLayout () : base ()
		{
		}
		
		public override void InitLayout (object child, BoundsSpecified specified)
		{
			base.InitLayout (child, specified);
		}

		// There are 3 steps to doing a table layout:
		// 1) Figure out which row/column each control goes into
		// 2) Figure out the sizes of each row/column
		// 3) Size and position each control
		public override bool Layout (object container, LayoutEventArgs args)
		{
			TableLayoutPanel panel = container as TableLayoutPanel;
			TableLayoutSettings settings = panel.LayoutSettings;
			
#if TABLE_DEBUG
			Console.WriteLine ("Beginning layout on panel: {0}, control count: {1}, col/row count: {2}x{3}", panel.Name, panel.Controls.Count, settings.ColumnCount, settings.RowCount);
#endif

			// STEP 1:
			// - Figure out which row/column each control goes into
			// - Store data in the TableLayoutPanel.actual_positions
			panel.actual_positions = CalculateControlPositions (panel, Math.Max (settings.ColumnCount, 1), Math.Max (settings.RowCount, 1));

			// STEP 2:
			// - Figure out the sizes of each row/column
			// - Store data in the TableLayoutPanel.widths/heights
			CalculateColumnRowSizes (panel, panel.actual_positions.GetLength (0), panel.actual_positions.GetLength (1));
			
			// STEP 3:
			// - Size and position each control
			LayoutControls(panel);

#if TABLE_DEBUG
			Console.WriteLine ("-- CalculatedPositions:");
			OutputControlGrid (panel.actual_positions, panel);

			Console.WriteLine ("Finished layout on panel: {0}", panel.Name);
			Console.WriteLine ();
#endif

			return false;
		}

		internal Control[,] CalculateControlPositions (TableLayoutPanel panel, int columns, int rows)
		{
			Control[,] grid = new Control[columns, rows];

			TableLayoutSettings settings = panel.LayoutSettings;

			// First place all controls that have an explicit col/row
			foreach (Control c in panel.Controls) {

				if (!c.VisibleInternal || c == dummy_control )
					continue;

				int col = settings.GetColumn (c);
				int row = settings.GetRow (c);
				if (col >= 0 && row >= 0) {
					if (col >= columns)
						 return CalculateControlPositions (panel, col + 1, rows);
					if (row >= rows)
						 return CalculateControlPositions (panel, columns, row + 1);

					if (grid[col, row] == null) {
						int col_span = Math.Min (settings.GetColumnSpan (c), columns);
						int row_span = Math.Min (settings.GetRowSpan (c), rows);

						if (col + col_span > columns) {
							if (row + 1 < rows) {
								grid[col, row] = dummy_control;
								row++;
								col = 0;
							}
							else if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddColumns)
								return CalculateControlPositions (panel, columns + 1, rows);
							else
								throw new ArgumentException ();
						}

						if (row + row_span > rows) {
							if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddRows)
								return CalculateControlPositions (panel, columns, rows + 1);
							else
								throw new ArgumentException ();
						}

						grid[col, row] = c;

						// Fill in the rest of this control's row/column extent with dummy
						// controls, so that other controls don't get put there.
						for (int i = 0; i < col_span; i++)
							for (int j = 0; j < row_span; j++)
								if (i != 0 || j != 0)
									grid[col + i, row + j] = dummy_control;
					}
				}
			}

			int x_pointer = 0;
			int y_pointer = 0;

			// Fill in gaps with controls that do not have an explicit col/row
			foreach (Control c in panel.Controls) {

				if (!c.VisibleInternal || c == dummy_control )
					continue;

				int col = settings.GetColumn (c);
				int row = settings.GetRow (c);

				if ((col >= 0 && col < columns) && (row >= 0 && row < rows) && (grid[col, row] == c || grid[col, row] == dummy_control))
					continue;

				for (int y = y_pointer; y < rows; y++) {
					y_pointer = y;
					x_pointer = 0;

					for (int x = x_pointer; x < columns; x++) {
						x_pointer = x;

						if (grid[x, y] == null) {
							int col_span = Math.Min (settings.GetColumnSpan (c), columns);
							int row_span = Math.Min (settings.GetRowSpan (c), rows);

							if (x + col_span > columns) {
								if (y + 1 < rows)
									break;
								else if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddColumns)
									return CalculateControlPositions (panel, columns + 1, rows);
								else
									throw new ArgumentException ();
							}

							if (y + row_span > rows) {
								if (x + 1 < columns)
									break;
								else if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddRows)
									return CalculateControlPositions (panel, columns, rows + 1);
								else
									throw new ArgumentException ();
							}

							grid[x, y] = c;

							// Fill in the rest of this control's row/column extent with dummy
							// controls, so that other controls don't get put there.
							for (int i = 0; i < col_span; i++)
								for (int j = 0; j < row_span; j++)
									if (i != 0 || j != 0)
										grid[x + i, y + j] = dummy_control;

							// I know someone will kill me for using a goto, but 
							// sometimes they really are the easiest way...
							goto Found;
						} else {
							// MS adds the controls only to the first row if 
							// GrowStyle is AddColumns and RowCount is 0,
							// so interrupt the search for a free horizontal cell 
							// beyond the first one in the given vertical
							if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddColumns && 
							    settings.RowCount == 0)
								break;
						}
					}
				}

				// MS adds rows instead of columns even when GrowStyle is AddColumns, 
				// but RowCount is 0.
				TableLayoutPanelGrowStyle adjustedGrowStyle = settings.GrowStyle;
				if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddColumns) {
					if (settings.RowCount == 0)
						adjustedGrowStyle = TableLayoutPanelGrowStyle.AddRows;
				}

				switch (adjustedGrowStyle) {
					case TableLayoutPanelGrowStyle.AddColumns:
						return CalculateControlPositions (panel, columns + 1, rows);
					case TableLayoutPanelGrowStyle.AddRows:
					default:
						return CalculateControlPositions (panel, columns, rows + 1);
					case TableLayoutPanelGrowStyle.FixedSize:
						throw new ArgumentException ();
				}

			Found: ;
			}

			return grid;
		}

		private void CalculateColumnRowSizes (TableLayoutPanel panel, int columns, int rows)
		{
			TableLayoutSettings settings = panel.LayoutSettings;

			panel.column_widths = new int[panel.actual_positions.GetLength (0)];
			panel.row_heights = new int[panel.actual_positions.GetLength (1)];

			int border_width = TableLayoutPanel.GetCellBorderWidth (panel.CellBorderStyle);
				
			Rectangle parentDisplayRectangle = panel.DisplayRectangle;

			TableLayoutColumnStyleCollection col_styles = new TableLayoutColumnStyleCollection (panel);
			
			foreach (ColumnStyle cs in settings.ColumnStyles)
				col_styles.Add( new ColumnStyle(cs.SizeType, cs.Width));

			TableLayoutRowStyleCollection row_styles = new TableLayoutRowStyleCollection (panel);

			foreach (RowStyle rs in settings.RowStyles)
				row_styles.Add (new RowStyle (rs.SizeType, rs.Height));
		
			// If we have more columns than columnstyles, temporarily add enough columnstyles
			if (columns > col_styles.Count)
			{
				for (int i = col_styles.Count; i < columns; i++)
					col_styles.Add(new ColumnStyle());			
			}

			// Same for rows..
			if (rows > row_styles.Count) 
			{
				for (int i = row_styles.Count; i < rows; i++)
					row_styles.Add (new RowStyle ());
			}

			while (row_styles.Count > rows)
				row_styles.RemoveAt (row_styles.Count - 1);
			while (col_styles.Count > columns)
				col_styles.RemoveAt (col_styles.Count - 1);
				
			// Find the largest column-span/row-span values.
			int max_colspan = 0, max_rowspan = 0;
			foreach (Control c in panel.Controls)
			{
				if (c.VisibleInternal && c != dummy_control)
				{
					max_colspan = Math.Max(max_colspan, settings.GetColumnSpan(c));
					max_rowspan = Math.Max(max_rowspan, settings.GetRowSpan(c));
				}
			}

			// Figure up all the column widths
			int total_width = parentDisplayRectangle.Width - (border_width * (columns + 1));
			int index = 0;

			// First assign all the Absolute sized columns..
			foreach (ColumnStyle cs in col_styles) {
				if (cs.SizeType == SizeType.Absolute) {
					panel.column_widths[index] = (int)cs.Width;
					total_width -= (int)cs.Width;
				}

				index++;
			}

			// Support for shrinking table horizontally (if it has both AutoSize and Dock.Fill styles and if it exceeds given width).
			int[] col_reserves = new int[panel.column_widths.Length];

			// Next, assign all the AutoSize columns to the width of their widest
			// control.  If the table-layout is auto-sized, then make sure that
			// no column with Percent styling clips its contents.
			// (per http://msdn.microsoft.com/en-us/library/ms171690.aspx)
			for (int colspan = 0; colspan < max_colspan; ++colspan)
			{
				for (index = colspan; index < col_styles.Count - colspan; ++index)
				{
					ColumnStyle cs = col_styles[index];
					if (cs.SizeType == SizeType.AutoSize
					|| (panel.AutoSize && cs.SizeType == SizeType.Percent))
					{
						int max_width = panel.column_widths[index];
						int min_reserve = int.MaxValue;

						// Find the widest control in the column
						for (int i = 0; i < rows; i ++)
						{
							Control c = panel.actual_positions[index - colspan, i];

							if (c != null && c != dummy_control && c.VisibleInternal)
							{
								// Skip any controls not being sized in this pass.
								if (settings.GetColumnSpan (c) != colspan + 1)
									continue;

								// Calculate the maximum control width.
								if (panel.AutoSize && cs.SizeType == SizeType.Percent)
									max_width = Math.Max (max_width, c.MinimumSize.Width + c.Margin.Horizontal);
								else if (c.AutoSize)
									max_width = Math.Max (max_width, c.PreferredSize.Width + c.Margin.Horizontal);
								else
									max_width = Math.Max (max_width, c.ExplicitBounds.Width + c.Margin.Horizontal);
								//max_width = Math.Max (max_width, c.Width + c.Margin.Left + c.Margin.Right);

								// How much can we shrink this column if necessary
								if (c.Dock == DockStyle.Fill)
									min_reserve = Math.Min(min_reserve, c.ExplicitBounds.Width - c.MinimumSize.Width + c.Margin.Horizontal);
							}
						}

						// Subtract the width of prior columns, if any.
						for (int i = Math.Max (index - colspan, 0); i < index; ++i)
							max_width -= panel.column_widths[i];

						// If necessary, increase this column's width.
						if (max_width > panel.column_widths[index])
						{
							max_width -= panel.column_widths[index];
							panel.column_widths[index] += max_width;
							total_width -= max_width;
						}

						// If necessary, decrease the col's reserve
						if (min_reserve < col_reserves[index] || 0 == col_reserves[index])
							col_reserves[index] = min_reserve;
					}
				}
			}

			// Shrink the table horizontally by shrinking it's columns, if necessary.
			if (panel.Dock == DockStyle.Fill)
				total_width -= Shrink(panel.column_widths, col_reserves, parentDisplayRectangle.Width - (border_width * (columns + 1)), max_colspan, col_styles.Count);

			index = 0;
			float total_percent = 0;
			
			// Finally, assign the remaining space to Percent columns, if any.
			if (total_width > 0)
			{
				int percent_width = total_width; 
				
				// Find the total percent (not always 100%)
				foreach (ColumnStyle cs in col_styles) 
				{
					if (cs.SizeType == SizeType.Percent)
						total_percent += cs.Width;
				}

				// Divvy up the space..
				foreach (ColumnStyle cs in col_styles) 
				{
					if (cs.SizeType == SizeType.Percent) 
					{
						int width_change = (int)((cs.Width / total_percent) * percent_width);
						if (width_change > 0)
						{
							panel.column_widths[index] += width_change;
							total_width -= width_change;
						}
					}

					index++;
				}
			}

			if (total_width > 0)
			{
				// Find the last column that isn't an Absolute SizeType, and give it
				// all this free space.  (Absolute sized columns need to retain their
				// absolute width if at all possible!)
				int col = col_styles.Count - 1;
				for (; col >= 0; --col)
				{
					if (col_styles[col].SizeType != SizeType.Absolute)
						break;
				}
				if (col < 0)
					col = col_styles.Count - 1;
				panel.column_widths[col] += total_width;
			}

			// Figure up all the row heights
			int total_height = parentDisplayRectangle.Height - (border_width * (rows + 1));
			index = 0;

			// First assign all the Absolute sized rows..
			foreach (RowStyle rs in row_styles) {
				if (rs.SizeType == SizeType.Absolute) {
					panel.row_heights[index] = (int)rs.Height;
					total_height -= (int)rs.Height;
				}

				index++;
			}

			index = 0;

			// Support for shrinking table vertically (if it has both AutoSize and Dock.Fill styles and if it exceeds parent's height).
			int[] row_reserves = new int[panel.row_heights.Length];

			// Next, assign all the AutoSize rows to the height of their tallest
			// control.  If the table-layout is auto-sized, then make sure that
			// no row with Percent styling clips its contents.
			// (per http://msdn.microsoft.com/en-us/library/ms171690.aspx)
			for (int rowspan = 0; rowspan < max_rowspan; ++rowspan)
			{
				for (index = rowspan; index < row_styles.Count - rowspan; ++index)
				{
					RowStyle rs = row_styles[index];
					if (rs.SizeType == SizeType.AutoSize
					|| (panel.AutoSize && rs.SizeType == SizeType.Percent))
					{
						int max_height = panel.row_heights[index];
						int min_reserve = int.MaxValue;

						// Find the tallest control in the row
						for (int i = 0; i < columns; i++) {
							Control c = panel.actual_positions[i, index - rowspan];

							if (c != null && c != dummy_control && c.VisibleInternal)
							{
								// Skip any controls not being sized in this pass.
								if (settings.GetRowSpan (c) != rowspan + 1)
									continue;

								// Calculate the maximum control height.
								if (panel.AutoSize && rs.SizeType == SizeType.Percent)
									max_height = Math.Max (max_height, c.MinimumSize.Height + c.Margin.Vertical);
								else if (c.AutoSize)
									max_height = Math.Max (max_height, c.PreferredSize.Height + c.Margin.Vertical);
								else
									max_height = Math.Max (max_height, c.ExplicitBounds.Height + c.Margin.Vertical);
								//max_height = Math.Max (max_height, c.Height + c.Margin.Top + c.Margin.Bottom);

								// How much can we shrink this row if necessary
								if (c.Dock == DockStyle.Fill)
									min_reserve = Math.Min(min_reserve, c.ExplicitBounds.Height - c.MinimumSize.Height + c.Margin.Vertical);
							}
						}

						// Subtract the height of prior rows, if any.
						for (int i = Math.Max (index - rowspan, 0); i < index; ++i)
							max_height -= panel.row_heights[i];

						// If necessary, increase this row's height.
						if (max_height > panel.row_heights[index])
						{
							max_height -= panel.row_heights[index];
							panel.row_heights[index] += max_height;
							total_height -= max_height;
						}

						// If necessary, decrease the row's reserve
						if (min_reserve < row_reserves[index] || 0 == row_reserves[index])
							row_reserves[index] = min_reserve;
					}
				}
			}

			// Shrink the table vertically by shrinking it's rows, if necessary
			if (panel.Dock == DockStyle.Fill)
				total_height -= Shrink(panel.row_heights, row_reserves, parentDisplayRectangle.Height - (border_width * (rows + 1)), max_rowspan, row_styles.Count);

			index = 0;
			total_percent = 0;

			// Finally, assign the remaining space to Percent rows, if any.
			if (total_height > 0) {
				int percent_height = total_height;
				
				// Find the total percent (not always 100%)
				foreach (RowStyle rs in row_styles) {
					if (rs.SizeType == SizeType.Percent)
						total_percent += rs.Height;
				}

				// Divvy up the space..
				foreach (RowStyle rs in row_styles) {
					if (rs.SizeType == SizeType.Percent) {
						int height_change = (int)((rs.Height / total_percent) * percent_height);
						if (height_change > 0)
						{
							panel.row_heights[index] += height_change;
							total_height -= height_change;
						}
					}

					index++;
				}
			}

			if (total_height > 0)
			{
				// Find the last row that isn't an Absolute SizeType, and give it
				// all this free space.  (Absolute sized rows need to retain their
				// absolute height if at all possible!)
				int row = row_styles.Count - 1;
				for (; row >= 0; --row)
				{
					if (row_styles[row].SizeType != SizeType.Absolute)
						break;
				}
				if (row < 0)
					row = row_styles.Count - 1;
				panel.row_heights[row] += total_height;
			}
		}

		// Shrinks values in 'sizes' array using values in 'reserves' array, so that the sum of 'sizes' does not exceed the 'max'.
		// The max_span and styles_count represent max_colspan, max_rowspan and row_styles.Count, column_styles.Count values. 
		// The 'reserves' array tells how much can be appropriate values in 'sizes' decreased. The 'int.Max' or '0'
		// values mean the size can not be decreased.
		internal static int Shrink(int[] sizes, int[] reserves, int max, int max_span, int styles_count)
		{
			int saved = 0;
			int sum = (int)Sum(sizes);
			if (sum > max)
			{
				int overlap = sum - max;
				int n = CountNonZeroNonMax(reserves);
				if (n != 0)
				{
					int step = overlap < n ? 1 : overlap / n;
					for (int span = 0; span < max_span && overlap > 0; ++span)
					{
						for (int index = span; index < styles_count - span && overlap > 0; ++index)
						{
							int reserve = reserves[index];
							if (reserve > 0 && reserve != int.MaxValue)
							{
								int d = step > reserve ? reserve : step;
								sizes[index] -= d;
								overlap -= d;
								saved += d;
							}
						}
					}
				}
			}
			return saved;
		}

		internal static long Sum(int[] array)
		{
			long sum = 0;
			foreach (int val in array)
				sum += val;
			return sum;
		}

		internal static int CountNonZeroNonMax(int[] array)
		{
			int n = 0;
			foreach (int val in array)
				if (val != 0 && val != int.MaxValue)
					++n;				
			return n;
		}

		private void LayoutControls (TableLayoutPanel panel)
		{
			TableLayoutSettings settings = panel.LayoutSettings;
			
			int border_width = TableLayoutPanel.GetCellBorderWidth (panel.CellBorderStyle);

			int columns = panel.actual_positions.GetLength(0);
			int rows = panel.actual_positions.GetLength(1);

			Point current_pos = new Point (panel.DisplayRectangle.Left + border_width, panel.DisplayRectangle.Top + border_width);

			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < columns; x++)
				{
					Control c = panel.actual_positions[x,y];
					if(c != null && c != dummy_control && c.VisibleInternal) {
						Size preferred;
						
						if (c.AutoSize)
							preferred = c.PreferredSize;
						else
							preferred = c.ExplicitBounds.Size;
						
						int new_x = 0;
						int new_y = 0;
						int new_width = 0;
						int new_height = 0;
						
						// Figure out the width of the control
						int column_width = panel.column_widths[x];
						
						for (int i = 1; i < Math.Min (settings.GetColumnSpan(c), panel.column_widths.Length); i++)
							column_width += panel.column_widths[x + i];

						if (c.Dock == DockStyle.Fill || c.Dock == DockStyle.Top || c.Dock == DockStyle.Bottom || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
							new_width = column_width - c.Margin.Left - c.Margin.Right;
						else
							new_width = Math.Min (preferred.Width, column_width - c.Margin.Left - c.Margin.Right);
							
						// Figure out the height of the control
						int column_height = panel.row_heights[y];

						for (int i = 1; i < Math.Min (settings.GetRowSpan (c), panel.row_heights.Length); i++)
							column_height += panel.row_heights[y + i];

						if (c.Dock == DockStyle.Fill || c.Dock == DockStyle.Left || c.Dock == DockStyle.Right || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top && (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
							new_height = column_height - c.Margin.Top - c.Margin.Bottom;
						else
							new_height = Math.Min (preferred.Height, column_height - c.Margin.Top - c.Margin.Bottom);

						// Figure out the left location of the control
						if (c.Dock == DockStyle.Left || c.Dock == DockStyle.Fill || (c.Anchor & AnchorStyles.Left) == AnchorStyles.Left)
							new_x = current_pos.X + c.Margin.Left;
						else if (c.Dock == DockStyle.Right || (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
							new_x = (current_pos.X + column_width) - new_width - c.Margin.Right;
						else	// (center control)
							new_x = (current_pos.X + (column_width - c.Margin.Left - c.Margin.Right) / 2) + c.Margin.Left - (new_width / 2);

						// Figure out the top location of the control
						if (c.Dock == DockStyle.Top || c.Dock == DockStyle.Fill || (c.Anchor & AnchorStyles.Top) == AnchorStyles.Top)
							new_y = current_pos.Y + c.Margin.Top;
						else if (c.Dock == DockStyle.Bottom || (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
							new_y = (current_pos.Y + column_height) - new_height - c.Margin.Bottom;
						else	// (center control)
							new_y = (current_pos.Y + (column_height - c.Margin.Top - c.Margin.Bottom) / 2) + c.Margin.Top - (new_height / 2);

						c.SetBoundsInternal (new_x, new_y, new_width, new_height, BoundsSpecified.None);
					}

					current_pos.Offset (panel.column_widths[x] + border_width, 0);
				}

				current_pos.Offset ((-1 * current_pos.X) + border_width + panel.DisplayRectangle.Left, panel.row_heights[y] + border_width);
			}
		}

		internal override Size GetPreferredSize (object container, Size proposedSize)
		{
			TableLayoutPanel panel = container as TableLayoutPanel;

			// If the tablelayoutowner is autosize, we have to make sure it is big enough
			// to hold every non-autosize control
			var actual_positions = CalculateControlPositions (panel, Math.Max (panel.ColumnCount, 1), Math.Max (panel.RowCount, 1));
			
			// Use actual row/column counts, not user set ones
			int actual_cols = actual_positions.GetLength (0);
			int actual_rows = actual_positions.GetLength (1);
			
			// Find the largest column-span/row-span values.  A table entry that spans more than one
			// column (row) should not be treated as though it's width (height) all belongs to the
			// first column (row), but should be spread out across all the columns (rows) that are
			// spanned.  So we need to keep track of the widths (heights) of spans as well as
			// individual columns (rows).
			int max_colspan = 1, max_rowspan = 1;
			foreach (Control c in panel.Controls)
			{
				max_colspan = Math.Max(max_colspan, panel.GetColumnSpan(c));
				max_rowspan = Math.Max(max_rowspan, panel.GetRowSpan(c));
			}

			// Figure out how wide the owner needs to be
			int[] column_widths = new int[actual_cols];
			// Keep track of widths for spans as well as columns. column_span_widths[i,j] stores
			// the maximum width for items column i than have a span of j+1 (ie, covers columns
			// i through i+j).
			int[,] column_span_widths = new int[actual_cols, max_colspan];
			int[] biggest = new int[max_colspan];
			float total_column_percentage = 0f;
			
			// Figure out how wide each column wants to be
			for (int i = 0; i < actual_cols; i++) {
				if (i < panel.ColumnStyles.Count && panel.ColumnStyles[i].SizeType == SizeType.Percent)
					total_column_percentage += panel.ColumnStyles[i].Width;
				int absolute_width = -1;
				if (i < panel.ColumnStyles.Count && panel.ColumnStyles[i].SizeType == SizeType.Absolute)
					absolute_width = (int)panel.ColumnStyles[i].Width;	// use the absolute width if it's absolute!

				for (int s = 0; s < max_colspan; ++s)
					biggest[s] = 0;

				for (int j = 0; j < actual_rows; j++) {
					Control c = actual_positions[i, j];

					if (c != null && c != TableLayout.dummy_control && c.VisibleInternal) {
						int colspan = panel.GetColumnSpan (c);
						if (colspan == 0)
							continue;
						if (colspan == 1 && absolute_width > -1)
							biggest[0] = absolute_width;	// use the absolute width if the column has absolute width assigned!
						else if (!c.AutoSize)
							biggest[colspan-1] = Math.Max (biggest[colspan-1], c.ExplicitBounds.Width + c.Margin.Horizontal);
						else
							biggest[colspan-1] = Math.Max (biggest[colspan-1], c.PreferredSize.Width + c.Margin.Horizontal);
					}
					else if (absolute_width > -1) {
						biggest[0] = absolute_width;
					}
				}

				for (int s = 0; s < max_colspan; ++s)
					column_span_widths[i,s] = biggest[s];
			}

			for (int i = 0; i < actual_cols; ++i) {
				for (int s = 1; s < max_colspan; ++s) {
					if (column_span_widths[i,s] > 0)
						AdjustWidthsForSpans (panel, column_span_widths, i, s);
				}
				column_widths[i] = column_span_widths[i,0];
			}

			// Because percentage based rows divy up the remaining space,
			// we have to make the owner big enough so that all the rows
			// get bigger, even if we only need one to be bigger.
			int non_percent_total_width = 0;
			int percent_total_width = 0;

			for (int i = 0; i < actual_cols; i++) {
				if (i < panel.ColumnStyles.Count && panel.ColumnStyles[i].SizeType == SizeType.Percent)
					percent_total_width = Math.Max (percent_total_width, (int)(column_widths[i] / ((panel.ColumnStyles[i].Width) / total_column_percentage)));
				else
					non_percent_total_width += column_widths[i];
			}

			int border_width = TableLayoutPanel.GetCellBorderWidth (panel.CellBorderStyle);
			int needed_width = non_percent_total_width + percent_total_width + (border_width * (actual_cols + 1));

			// Figure out how tall the owner needs to be
			int[] row_heights = new int[actual_rows];
			int[,] row_span_heights = new int[actual_rows, max_rowspan];
			biggest = new int[max_rowspan];
			float total_row_percentage = 0f;
		
			// Figure out how tall each row wants to be
			for (int j = 0; j < actual_rows; j++) {
				if (j < panel.RowStyles.Count && panel.RowStyles[j].SizeType == SizeType.Percent)
					total_row_percentage += panel.RowStyles[j].Height;
				int absolute_height = -1;
				if (j < panel.RowStyles.Count && panel.RowStyles[j].SizeType == SizeType.Absolute)
					absolute_height = (int)panel.RowStyles[j].Height;	// use the absolute height if it's absolute!
					
				for (int s = 0; s < max_rowspan; ++s)
					biggest[s] = 0;
				
				for (int i = 0; i < actual_cols; i++) {
					Control c = actual_positions[i, j];

					if (c != null && c != TableLayout.dummy_control && c.VisibleInternal) {
						int rowspan = panel.GetRowSpan (c);
						if (rowspan == 0)
							continue;
						if (rowspan == 1 && absolute_height > -1)
							biggest[0] = absolute_height;    // use the absolute height if the row has absolute height assigned!
						else if (!c.AutoSize)
							biggest[rowspan-1] = Math.Max (biggest[rowspan-1], c.ExplicitBounds.Height + c.Margin.Vertical);
						else
							biggest[rowspan-1] = Math.Max (biggest[rowspan-1], c.PreferredSize.Height + c.Margin.Vertical);
					}
					else if (absolute_height > -1) {
						biggest[0] = absolute_height;
					}
				}

				for (int s = 0; s < max_rowspan; ++s)
					row_span_heights[j,s] = biggest[s];
			}

			for (int j = 0; j < actual_rows; ++j) {
				for (int s = 1; s < max_rowspan; ++s) {
					if (row_span_heights[j,s] > 0)
						AdjustHeightsForSpans (panel, row_span_heights, j, s);
				}
				row_heights[j] = row_span_heights[j,0];
			}
			
			// Because percentage based rows divy up the remaining space,
			// we have to make the owner big enough so that all the rows
			// get bigger, even if we only need one to be bigger.
			int non_percent_total_height = 0;
			int percent_total_height = 0;

			for (int j = 0; j < actual_rows; j++) {
				if (j < panel.RowStyles.Count && panel.RowStyles[j].SizeType == SizeType.Percent)
					percent_total_height = Math.Max (percent_total_height, (int)(row_heights[j] / ((panel.RowStyles[j].Height) / total_row_percentage)));
				else
					non_percent_total_height += row_heights[j];
			}

			int needed_height = non_percent_total_height + percent_total_height + (border_width * (actual_rows + 1));

			// When both autosized and anchored to left AND right or top AND bottom, yield anchoring,
			// but use calculated autosize value for the other direction.
			var mask = AnchorStyles.Left | AnchorStyles.Right;
			if (panel.AutoSize && proposedSize.Width != 0 && (panel.Anchor & mask) == mask)
				needed_width = proposedSize.Width - panel.Bounds.Left - panel.DistanceRight;
			mask = AnchorStyles.Top | AnchorStyles.Bottom;
			if (panel.AutoSize && proposedSize.Height != 0 && (panel.Anchor & mask) == mask)
				needed_height = proposedSize.Height - panel.Bounds.Top - panel.DistanceBottom;

			return new Size (needed_width, needed_height);
		}

		/// <summary>
		/// Adjust the widths of the columns underlying a span if necessary.
		/// </summary>
		private static void AdjustWidthsForSpans (TableLayoutPanel panel, int[,] widths, int col, int span)
		{
			// Get the combined width of the columns underlying the span.
			int existing_width = 0;
			for (int i = col; i <= col+span; ++i)
				existing_width += widths[i,0];
			if (widths[col,span] > existing_width)
			{
				// We need to expand one or more of the underlying columns to fit the span,
				// preferably ones that are not Absolute style.
				int excess = widths[col,span] - existing_width;
				int remaining = excess;
				List<int> adjusting = new List<int>();
				List<float> adjusting_widths = new List<float>();
				for (int i = col; i <= col+span; ++i) {
					if (i < panel.ColumnStyles.Count && panel.ColumnStyles[i].SizeType != SizeType.Absolute) {
						adjusting.Add(i);
						adjusting_widths.Add((float)widths[i,0]);
					}
				}
				if (adjusting.Count == 0) {
					// if every column is Absolute, spread the gain across every column
					for (int i = col; i <= col+span; ++i) {
						adjusting.Add(i);
						adjusting_widths.Add((float)widths[i,0]);
					}
				}
				float original_total = 0f;
				foreach (var w in adjusting_widths)
					original_total += w;
				// Divide up the needed additional width proportionally.
				for (int i = 0; i < adjusting.Count; ++i) {
					var idx = adjusting[i];
					var percent = adjusting_widths[i] / original_total;
					var adjust = (int)(percent * excess);
					widths[idx,0] += adjust;
					remaining -= adjust;
				}
				// Any remaining fragment (1 or 2 pixels?) is divided evenly.
				while (remaining > 0) {
					for (int i = 0; i < adjusting.Count && remaining > 0; ++i) {
						++widths[adjusting[i],0];
						--remaining;
					}
				}
			}
		}

		/// <summary>
		/// Adjust the heights of the rows underlying a span if necessary.
		/// </summary>
		private static void AdjustHeightsForSpans (TableLayoutPanel panel, int[,] heights, int row, int span)
		{
			// Get the combined height of the rows underlying the span.
			int existing_height = 0;
			for (int i = row; i <= row+span; ++i)
				existing_height += heights[i,0];
			if (heights[row,span] > existing_height)
			{
				// We need to expand one or more of the underlying rows to fit the span,
				// preferably ones that are not Absolute style.
				int excess = heights[row,span] - existing_height;
				int remaining = excess;
				List<int> adjusting = new List<int>();
				List<float> adjusting_heights = new List<float>();
				for (int i = row; i <= row+span; ++i) {
					if (i < panel.RowStyles.Count && panel.RowStyles[i].SizeType != SizeType.Absolute) {
						adjusting.Add(i);
						adjusting_heights.Add((float)heights[i,0]);
					}
				}
				if (adjusting.Count == 0) {
					// if every row is Absolute, spread the gain across every row
					for (int i = row; i <= row+span; ++i) {
						adjusting.Add(i);
						adjusting_heights.Add((float)heights[i,0]);
					}
				}
				float original_total = 0f;
				foreach (var w in adjusting_heights)
					original_total += w;
				// Divide up the needed additional height proportionally.
				for (int i = 0; i < adjusting.Count; ++i) {
					var idx = adjusting[i];
					var percent = adjusting_heights[i] / original_total;
					var adjust = (int)(percent * excess);
					heights[idx,0] += adjust;
					remaining -= adjust;
				}
				// Any remaining fragment (1 or 2 pixels?) is divided evenly.
				while (remaining > 0) {
					for (int i = 0; i < adjusting.Count && remaining > 0; ++i) {
						++heights[adjusting[i],0];
						--remaining;
					}
				}
			}
		}	
#if TABLE_DEBUG
		private void OutputControlGrid (Control[,] grid, TableLayoutPanel panel)
		{
			Console.WriteLine ("     Size: {0}x{1}", grid.GetLength (0), grid.GetLength (1));

			Console.Write ("        ");

			foreach (int i in panel.column_widths)
				Console.Write (" {0}px  ", i.ToString ().PadLeft (3));

			Console.WriteLine ();
				
			for (int y = 0; y < grid.GetLength (1); y++) {
				Console.Write (" {0}px |", panel.row_heights[y].ToString ().PadLeft (3));
				
				for (int x = 0; x < grid.GetLength (0); x++) {
					if (grid[x, y] == null)
						Console.Write ("  ---  |");
					else if (string.IsNullOrEmpty (grid[x, y].Name))
						Console.Write ("  ???  |");
					else
						Console.Write (" {0} |", grid[x, y].Name.PadRight (5).Substring (0, 5));
				}
				
				Console.WriteLine ();
			}
		}
#endif
	}
}
