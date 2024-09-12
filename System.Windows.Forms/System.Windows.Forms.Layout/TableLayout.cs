//
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

namespace System.Windows.Forms.Layout
{
	internal class TableLayout : LayoutEngine
	{
		internal static readonly TableLayout Instance = new TableLayout();

		private static Control dummy_control = new Control ("Dummy");	// Used as a placeholder for row/col spans
		private static ColumnStyle default_column_style = new ColumnStyle();
		private static RowStyle default_row_style = new RowStyle();

		private TableLayout () : base ()
		{
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
			int[] widths, heights;
			CalculateColumnRowSizes (panel, panel.actual_positions, out widths, out heights, panel.DisplayRectangle.Size, false);
			panel.column_widths = widths;
			panel.row_heights = heights;
			
			// STEP 3:
			// - Size and position each control
			LayoutControls(panel);

#if TABLE_DEBUG
			Console.WriteLine ("-- CalculatedPositions:");
			OutputControlGrid (panel.actual_positions, panel);

			Console.WriteLine ("Finished layout on panel: {0}", panel.Name);
			Console.WriteLine ();
#endif

			return panel.AutoSizeInternal;
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

		private static void CalculateColumnWidths (TableLayoutSettings settings, Control[,] actual_positions, int max_colspan, TableLayoutColumnStyleCollection col_styles, bool auto_size, int[] column_widths, bool minimum_sizes)
		{
			Size proposedSize = minimum_sizes ? new Size(1, 0) : Size.Empty;
			int rows = actual_positions.GetLength(1);
			float total_percent = 0;
			float max_percent_size = 0;

			// First assign all the Absolute sized columns
			int index = 0;
			foreach (ColumnStyle cs in col_styles) {
				if (index >= column_widths.Length)
					break;
				if (cs.SizeType == SizeType.Absolute)
					column_widths[index] = (int)cs.Width;
				else if (cs.SizeType == SizeType.Percent)
					total_percent += cs.Width;
				index++;
			}
			while (index < column_widths.Length) {
				column_widths[index] = 0;
				index++;
			}

			// Next, assign all the AutoSize columns to the width of their widest
			// control.  If the table-layout is auto-sized, then make sure that
			// no column with Percent styling clips its contents.
			// (per http://msdn.microsoft.com/en-us/library/ms171690.aspx)
			for (int colspan = 0; colspan < max_colspan; ++colspan) {
				for (index = colspan; index < column_widths.Length; ++index) {
					ColumnStyle cs = GetStyle(col_styles, index);
					if (cs.SizeType == SizeType.AutoSize || (auto_size && cs.SizeType == SizeType.Percent)) {
						int max_width = column_widths[index];
						// Find the widest control in the column
						for (int i = 0; i < rows; i ++) {
							Control c = actual_positions[index - colspan, i];
							if (c != null && c != dummy_control && c.VisibleInternal) {
								// Skip any controls not being sized in this pass.
								if (settings.GetColumnSpan (c) != colspan + 1)
									continue;
								// Calculate the maximum control width.
								if (cs.SizeType == SizeType.Percent && minimum_sizes)
									max_width = Math.Max(max_width, c.MinimumSize.Width + c.Margin.Horizontal);
								else
									max_width = Math.Max(max_width, GetControlSize (c, proposedSize).Width + c.Margin.Horizontal);
							}
						}

						if (cs.SizeType == SizeType.Percent)
							max_percent_size = Math.Max(max_percent_size, max_width / cs.Width);

						// Subtract the width of prior columns, if any.
						for (int i = Math.Max (index - colspan, 0); i < index; ++i)
							max_width -= column_widths[i];						

						// If necessary, increase this column's width.
						if (max_width > column_widths[index]) {
							bool allAutoSized = true;
							for (int j = Math.Max (index - colspan, 0); j < index; ++j)
								allAutoSized &= GetStyle(col_styles, j).SizeType == SizeType.AutoSize;
							if (allAutoSized)
								column_widths[index] = max_width;
						}
					}
				}
			}

			for (index = 0; index < col_styles.Count && index < column_widths.Length; ++index) {
				ColumnStyle cs = col_styles[index];
				if (cs.SizeType == SizeType.Percent)
					column_widths[index] = Math.Max(column_widths[index], (int)Math.Ceiling(max_percent_size * cs.Width * (100f / total_percent)));
			}
		}

		private static void CalculateRowHeights (TableLayoutSettings settings, Control[,] actual_positions, int max_rowspan, TableLayoutRowStyleCollection row_styles, bool auto_size, int[] column_widths, int[] row_heights, bool minimum_sizes)
		{
			int columns = actual_positions.GetLength(0);
			float total_percent = 0;
			float max_percent_size = 0;

			// First assign all the Absolute sized rows..
			int index = 0;
			foreach (RowStyle rs in row_styles) {
				if (index >= row_heights.Length)
					break;
				if (rs.SizeType == SizeType.Absolute)
					row_heights[index] = (int)rs.Height;
				else if (rs.SizeType == SizeType.Percent)
					total_percent += rs.Height;
				index++;
			}
			while (index < row_heights.Length) {
				row_heights[index] = 0;
				index++;
			}

			// Next, assign all the AutoSize rows to the height of their tallest
			// control.  If the table-layout is auto-sized, then make sure that
			// no row with Percent styling clips its contents.
			// (per http://msdn.microsoft.com/en-us/library/ms171690.aspx)
			for (int rowspan = 0; rowspan < max_rowspan; ++rowspan) {
				for (index = rowspan; index < row_heights.Length; ++index) {
					RowStyle rs = GetStyle(row_styles, index);
					int max_height = row_heights[index];
					// Find the tallest control in the row
					for (int i = 0; i < columns; i++) {
						Control c = actual_positions[i, index - rowspan];
						if (c != null && c != dummy_control && c.VisibleInternal) {
							// Skip any controls not being sized in this pass.
							if (settings.GetRowSpan (c) != rowspan + 1)
								continue;

							int current_width = 0;
							int column_span = settings.GetColumnSpan(c);
							for (int j = i; j < i + column_span && j < column_widths.Length; j++) {
								current_width += column_widths[j];
							}

							// Calculate the maximum control height.
							if (!minimum_sizes || c.AutoSizeInternal)
								max_height = Math.Max (max_height, GetControlSize(c, new Size(current_width - c.Margin.Horizontal, 0)).Height + c.Margin.Vertical);
							else
								max_height = c.MinimumSize.Height;
						}
					}

					if (rs.SizeType == SizeType.Percent)
						max_percent_size = Math.Max(max_percent_size, max_height / rs.Height);

					// Subtract the height of prior rows, if any.
					for (int i = Math.Max (index - rowspan, 0); i < index; ++i)
						max_height -= row_heights[i];

					// If necessary, try to find suitable row and increase it's height.
					if (max_height > row_heights[index]) {
						for (int j = index; j >= index - rowspan; --j) {
							RowStyle style = GetStyle(row_styles, j);
							if (style.SizeType == SizeType.AutoSize || style.SizeType == SizeType.Percent) {
								row_heights[j] += max_height - row_heights[index];
								break;
							}
						}
					}
				}
			}

			for (index = 0; index < row_styles.Count && index < row_heights.Length; ++index) {
				RowStyle rs = row_styles[index];
				if (rs.SizeType == SizeType.Percent)
					row_heights[index] = Math.Max(row_heights[index], (int)Math.Ceiling(max_percent_size * rs.Height * (100f / total_percent)));
			}
		}

		static ColumnStyle GetStyle(TableLayoutColumnStyleCollection styles, int index)
		{
			return index < styles.Count ? styles[index] : default_column_style;
		}

		static RowStyle GetStyle(TableLayoutRowStyleCollection styles, int index)
		{
			return index < styles.Count ? styles[index] : default_row_style;
		}

		private static int RedistributePercents (int overlap, TableLayoutColumnStyleCollection styles, int[] column_widths)
		{
			int saved = 0;

			if (overlap > 0) {
				// Find the total percent (not always 100%)
				float total_percent = 0;
				int total_width = 0;
				int index = 0;
				foreach (ColumnStyle cs in styles) {
					if (index >= column_widths.Length)
						break;
					if (cs.SizeType == SizeType.Percent) {
						total_percent += cs.Width;
						total_width += column_widths[index];
					}
					index++;
				}

				// Divvy up the space..
				index = 0;
				int new_total_width =  total_width + overlap;
				foreach (ColumnStyle cs in styles) {
					if (index >= column_widths.Length)
						break;
					if (cs.SizeType == SizeType.Percent) {
						int new_width = (int)(cs.Width / total_percent * new_total_width);
						if (new_width > column_widths[index]) {
							saved += new_width - column_widths[index];
							column_widths[index] = new_width;
						}
					}
					index++;
				}
			}

			return saved;
		}

		private static int RedistributePercents (int overlap, TableLayoutRowStyleCollection styles, int[] row_heights)
		{
			int saved = 0;

			if (overlap > 0) {
				// Find the total percent (not always 100%)
				float total_percent = 0;
				int total_height = 0;
				int index = 0;
				foreach (RowStyle rs in styles) {
					if (index >= row_heights.Length)
						break;
					if (rs.SizeType == SizeType.Percent) {
						total_percent += rs.Height;
						total_height += row_heights[index];
					}
					index++;
				}

				// Divvy up the space..
				index = 0;
				int new_total_height =  total_height + overlap;
				foreach (RowStyle rs in styles) {
					if (index >= row_heights.Length)
						break;
					if (rs.SizeType == SizeType.Percent) {
						int new_height = (int)(rs.Height / total_percent * new_total_height);
						if (new_height > row_heights[index]) {
							saved += new_height - row_heights[index];
							row_heights[index] = new_height;
						}
					}
					index++;
				}
			}

			return saved;
		}

		private static void CalculateColumnRowSizes (TableLayoutPanel panel, Control[,] actual_positions, out int[] column_widths, out int[] row_heights, Size size, bool measureOnly)
		{
			TableLayoutSettings settings = panel.LayoutSettings;
			int columns = actual_positions.GetLength(0);
			int rows = actual_positions.GetLength(1);
			bool auto_size = panel.AutoSizeInternal && measureOnly;
			bool auto_size_h = auto_size && size.Width == 0;
			bool boundBySize = !measureOnly;

			column_widths = new int[columns];
			row_heights = new int[rows];

			// Calculate the bounded size only if we are in default layout and docked, otherwise calculate unbounded.
			if (measureOnly && size.Width > 0) {
				if (panel.Parent != null && panel.Parent.LayoutEngine is DefaultLayout) {
					boundBySize |= panel.Dock == DockStyle.Top || panel.Dock == DockStyle.Bottom || panel.Dock == DockStyle.Fill;
					boundBySize |= (panel.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right);
				}			
			}

			int border_width = TableLayoutPanel.GetCellBorderWidth (panel.CellBorderStyle);
				
			// Find the largest column-span/row-span values.
			int max_colspan = 0, max_rowspan = 0;
			foreach (Control c in panel.Controls) {
				if (c.VisibleInternal && c != dummy_control) {
					max_colspan = Math.Max(max_colspan, settings.GetColumnSpan(c));
					max_rowspan = Math.Max(max_rowspan, settings.GetRowSpan(c));
				}
			}

			// Figure up all the column widths
			CalculateColumnWidths (settings, actual_positions, max_colspan, settings.ColumnStyles, auto_size_h, column_widths, false);

			// Calculate available width
			int available_width = size.Width - (border_width * (columns + 1));
			foreach (int width in column_widths)
				available_width -= width;

			// Shrink the table horizontally by shrinking it's columns, if necessary
			if (boundBySize && size.Width > 0 && available_width < 0) {
				// Calculate the minimum widths for each column
				int[] col_min_widths = new int[column_widths.Length];
				CalculateColumnWidths (settings, actual_positions, max_colspan, settings.ColumnStyles, auto_size_h, col_min_widths, true);
				available_width += Shrink(column_widths, col_min_widths, -available_width, max_colspan);

				// Shrink columns with Percent sizing first
				for (int i = 0; i < column_widths.Length && i < settings.ColumnStyles.Count; ++i)
					col_min_widths[i] = settings.ColumnStyles[i].SizeType == SizeType.Percent ? 0 : column_widths[i];
				available_width += Shrink(column_widths, col_min_widths, -available_width, max_colspan);

				// Shrink all columns if necessary
				if (available_width < 0) {
					for (int i = 0; i < column_widths.Length && i < settings.ColumnStyles.Count; ++i)
						col_min_widths[i] = 0;
					available_width += Shrink(column_widths, col_min_widths, -available_width, max_rowspan);
				}
			}

			// Finally, assign the remaining space to Percent columns, if any.
			if (available_width > 0)
				available_width -= RedistributePercents(available_width, settings.ColumnStyles, column_widths);

			if (available_width > 0  && column_widths.Length > 0) {
				// Find the last column that isn't an Absolute SizeType, and give it
				// all this free space.  (Absolute sized columns need to retain their
				// absolute width if at all possible!)
				int col = column_widths.Length - 1;
				for (; col >= 0; --col)
					if (col >= settings.ColumnStyles.Count || settings.ColumnStyles[col].SizeType != SizeType.Absolute)
						break;
				if (col < 0)
					col = column_widths.Length - 1;
				column_widths[col] += available_width;
			}

			// Figure up all the row heights
			CalculateRowHeights (settings, actual_positions, max_rowspan, settings.RowStyles, auto_size, column_widths, row_heights, false);

			// Calculate available height
			int available_height = size.Height - (border_width * (rows + 1));
			foreach (int height in row_heights)
				available_height -= height;

			// Shrink the table vertically by shrinking it's rows, if necessary
			if (boundBySize && size.Height > 0 && available_height < 0) {

				int safe_rows = Math.Min(row_heights.Length, settings.RowStyles.Count);

				// Shrink percent rows first
				int[] row_min_heights_perc = new int[safe_rows];
				for (int i = 0; i < safe_rows; ++i)
					row_min_heights_perc[i] = settings.RowStyles[i].SizeType == SizeType.Percent ? 0 : row_heights[i];
				available_height += Shrink(row_heights, row_min_heights_perc, -available_height, max_rowspan);

				// Shrink all types of rows if necessary
				if (available_height < 0) {
					int[] row_min_heights = new int[safe_rows];
					for (int i = 0; i < safe_rows; ++i)
						row_min_heights[i] = 0;
					CalculateRowHeights (settings, actual_positions, max_rowspan, settings.RowStyles, auto_size, column_widths, row_min_heights, true);
					available_height += Shrink(row_heights, row_min_heights, -available_height, max_rowspan);
				}
			}

			// Finally, assign the remaining space to Percent rows, if any.
			if (available_height > 0 && !measureOnly)
				available_height -= RedistributePercents(available_height, settings.RowStyles, row_heights);

			if (available_height > 0 && row_heights.Length > 0 && !measureOnly) {
				// Find the last row that isn't an Absolute SizeType, and give it
				// all this free space.  (Absolute sized rows need to retain their
				// absolute height if at all possible!)
				int row = row_heights.Length - 1;
				for (; row >= 0; --row)
					if (row >= settings.RowStyles.Count || settings.RowStyles[row].SizeType != SizeType.Absolute)
						break;
				if (row < 0)
					row = row_heights.Length - 1;
				row_heights[row] += available_height;
			}
		}

		// Shrinks values in 'sizes' array using values in 'reserves' array, so that the sum of 'sizes' does not exceed the 'max'.
		// The max_span represent max_colspan, max_rowspan values. 
		// The 'min_sizes' array tells the smalles appropriate values for column sizes. 
		static int Shrink(int[] sizes, int[] min_sizes, int overlap, int max_span)
		{
			int safe_length = Math.Min(sizes.Length, min_sizes.Length);

			int n = 0;
			for (int index = 0; index < safe_length; ++index)
				if (sizes[index] > min_sizes[index])
					n++;

			int saved = 0;
			if (n != 0) {
				int step = overlap < n ? 1 : overlap / n;
				for (int span = 0; span < max_span && overlap > 0; ++span) {
					for (int index = span; index < safe_length - span && overlap > 0; ++index) {
						int reserve = sizes[index] - min_sizes[index];
						if (reserve > 0 && reserve != int.MaxValue) {
							int d = step > reserve ? reserve : step;
							sizes[index] -= d;
							overlap -= d;
							saved += d;
						}
					}
				}
			}

			return saved;
		}

		private static Size GetControlSize (Control c, Size proposedSize)
		{
			if (c.AutoSizeInternal) {
				return c.GetPreferredSize (proposedSize);
			} else {
				if (proposedSize.Width == 1)
					return c.MinimumSize;
				return c.ExplicitBounds.Size;				
			}
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
						
						int new_x = 0;
						int new_y = 0;
						int new_width = 0;
						int new_height = 0;

						int column_width = panel.column_widths[x];
						for (int i = 1; i < Math.Min (settings.GetColumnSpan(c), panel.column_widths.Length - x); i++)
							column_width += panel.column_widths[x + i];

						int column_height = panel.row_heights[y];
						for (int i = 1; i < Math.Min (settings.GetRowSpan(c), panel.row_heights.Length - y); i++)
							column_height += panel.row_heights[y + i];

						preferred = GetControlSize(c, new Size(column_width - c.Margin.Horizontal, column_height - c.Margin.Vertical));

						// Figure out the width of the control
						if (c.Dock == DockStyle.Fill || c.Dock == DockStyle.Top || c.Dock == DockStyle.Bottom || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
							new_width = column_width - c.Margin.Left - c.Margin.Right;
						else
							new_width = Math.Min (preferred.Width, column_width - c.Margin.Left - c.Margin.Right);
							
						// Figure out the height of the control
						if (c.Dock == DockStyle.Fill || c.Dock == DockStyle.Left || c.Dock == DockStyle.Right || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top && (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
							new_height = column_height - c.Margin.Top - c.Margin.Bottom;
						else
							new_height = Math.Min (preferred.Height, column_height - c.Margin.Top - c.Margin.Bottom);

						// Figure out the left location of the control
						if (c.Dock == DockStyle.Left || c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left) && c.Dock == DockStyle.None)
							new_x = current_pos.X + c.Margin.Left;
						else if (c.Dock == DockStyle.Right || (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
							new_x = (current_pos.X + column_width) - new_width - c.Margin.Right;
						else	// (center control)
							new_x = (current_pos.X + (column_width - c.Margin.Left - c.Margin.Right) / 2) + c.Margin.Left - (new_width / 2);

						// Figure out the top location of the control
						if (c.Dock == DockStyle.Top || c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top) && c.Dock == DockStyle.None)
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

			int[] column_sizes;
			int[] row_sizes;
			int border_width = TableLayoutPanel.GetCellBorderWidth(panel.CellBorderStyle);
			CalculateColumnRowSizes(panel, actual_positions, out column_sizes, out row_sizes, proposedSize, true);

			int needed_width = (column_sizes.Length + 1) * border_width;
			for (int i = 0; i < column_sizes.Length; i++) {
				needed_width += column_sizes[i];
			}

			int needed_height = (row_sizes.Length + 1) * border_width;
			for (int i = 0; i < row_sizes.Length; i++) {
				needed_height += row_sizes[i];
			}

			return new Size(needed_width, needed_height);
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
