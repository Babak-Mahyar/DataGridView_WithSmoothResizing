using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmoothResizeDataGridView
{
    public partial class DataGridView_WithSmoothResizing : DataGridView
    {
        public DataGridView_WithSmoothResizing()
        {
            InitializeComponent();
            RowHeadersVisible = true;
            ColumnHeadersVisible = true;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        }

        public const int ROW_HEADERS_COLUMN_INDEX = -1;
        public const int INVALID_COLUMN_INDEX = -2;

        public const int COLUMNS_ROW_INDEX = -1;
        public const int INVALID_ROW_INDEX = -2;

        private int f_Base_X = 0;
        private int f_Base_Y = 0;
        private bool f_IsColumnResizing = false;
        private bool f_IsRowResizing = false;
        private int f_Resizing_Column_Index = INVALID_COLUMN_INDEX;
        private int f_Resizing_Row_Index = INVALID_ROW_INDEX;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (IsMouseLeftClick(e))
            {
                ReserveMouseLocation(e);
                if (IsSetResizingColumnCursor)
                    ReserveResizingColumnIndex(e.X);
                else if (IsSetResizingRowCursor)
                    ReserveResizingRowIndex(e.Y);
            }
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            ResetResizingColumnIndex();
            ResetResizingRowIndex();
            base.OnMouseUp(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseLeftClick(e))
            {
                if (f_IsColumnResizing)
                {
                    var DeltaX = e.X - f_Base_X;
                    if (DeltaX != 0)
                        ResizeColumn(f_Resizing_Column_Index, DeltaX);
                }
                else if (f_IsRowResizing)
                {
                    var DeltaY = e.Y - f_Base_Y;
                    if (DeltaY != 0)
                        ResizeRow(f_Resizing_Row_Index, DeltaY);
                }
            }
            ReserveMouseLocation(e);
            base.OnMouseMove(e);
        }
        protected int GetColumnIndex(int X)
        {
            if (IsSetResizingColumnCursor)
            {
                var Relative_X = X - Left;
                for (int ColumnIndex = 0; ColumnIndex < Columns.Count; ColumnIndex++)
                {
                    var IsFirstColumn = (ColumnIndex == 0);
                    var IsLastColumn = (ColumnIndex == Columns.Count - 1);
                    var NextColumnIndex = ColumnIndex + 1;
                    var ThisColumnArea = GetColumnArea(ColumnIndex);
                    var NextColumnArea = IsLastColumn ? ThisColumnArea : GetColumnArea(NextColumnIndex);
                    if (IsFirstColumn && (Relative_X < RowHeadersWidth))
                        return ROW_HEADERS_COLUMN_INDEX;
                    if (IsInsideColumnArea(Relative_X, ThisColumnArea, NextColumnArea))
                        return ColumnIndex;
                }
            }
            return INVALID_COLUMN_INDEX;
        }
        protected int GetRowIndex(int Y)
        {
            if (IsSetResizingRowCursor)
            {
                var Relative_Y = Y; // "Y" is relative, so the "Top" value does not need to be subtracted from "Y".
                for (int RowIndex = 0; RowIndex < Rows.Count; RowIndex++)
                {
                    var IsFirstRow = (RowIndex == 0);
                    var IsLastRow = (RowIndex == Rows.Count - 1); // Including new empty row
                    var NextRowIndex = RowIndex + 1;
                    var ThisRowArea = GetRowArea(RowIndex);
                    var NextRowArea = IsLastRow ? ThisRowArea : GetRowArea(NextRowIndex);
                    if (IsFirstRow && IsInsideRowAreaForColumnsHeader(Relative_Y, ThisRowArea))
                        return COLUMNS_ROW_INDEX;
                    if (IsInsideRowAreaForData(Relative_Y, ThisRowArea, NextRowArea))
                        return RowIndex;
                }
            }
            return INVALID_ROW_INDEX;
        }
        private void ResizeColumn(int ColumnIndex, int DeltaX)
        {
            if (ColumnIndex == ROW_HEADERS_COLUMN_INDEX)
            {
                // Row Headers Resizing                           
                if (RowHeadersWidth + DeltaX > 5)
                    RowHeadersWidth += DeltaX;
            }
            else if ((ColumnIndex > INVALID_COLUMN_INDEX) && (ColumnIndex < Columns.Count))
                Columns[ColumnIndex].Width += DeltaX;
        }
        private void ResizeRow(int RowIndex, int DeltaY)
        {
            if (RowIndex == COLUMNS_ROW_INDEX)
            {
                // Columns Row Resizing                           
                if (ColumnHeadersHeight + DeltaY > 5)
                    ColumnHeadersHeight += DeltaY;
            }
            else if ((RowIndex > INVALID_ROW_INDEX) && (RowIndex < Rows.Count))
                Rows[RowIndex].Height += DeltaY;
        }
        private bool IsMouseLeftClick(MouseEventArgs e)
        {
            return (e.Button == MouseButtons.Left);
        }
        private void ReserveMouseLocation(MouseEventArgs e)
        {
            f_Base_X = e.X;
            f_Base_Y = e.Y;
        }
        private void ReserveResizingColumnIndex(int X)
        {
            f_IsColumnResizing = true;
            f_Resizing_Column_Index = GetColumnIndex(X);
        }
        private void ReserveResizingRowIndex(int Y)
        {
            f_IsRowResizing = true;
            f_Resizing_Row_Index = GetRowIndex(Y);
        }
        private void ResetResizingColumnIndex()
        {
            f_IsColumnResizing = false;
            f_Resizing_Column_Index = INVALID_COLUMN_INDEX;
        }
        private void ResetResizingRowIndex()
        {
            f_IsRowResizing = false;
            f_Resizing_Row_Index = INVALID_ROW_INDEX;
        }
        public bool IsSetResizingColumnCursor
        {
            get
            {
                return (Cursor == Cursors.SizeWE);
            }
        }
        public bool IsSetResizingRowCursor
        {
            get
            {
                return (Cursor == Cursors.SizeNS);
            }
        }
        private Rectangle GetColumnArea(int ColumnIndex)
        {
            return GetCellDisplayRectangle(ColumnIndex, 0, false);
        }
        private Rectangle GetRowArea(int RowIndex)
        {
            return GetCellDisplayRectangle(0, RowIndex, false);
        }
        private bool IsInsideColumnArea(int X, Rectangle ThisColumnRect, Rectangle NextColumnRect)
        {
            return (X > ThisColumnRect.X) && (X < (ThisColumnRect.X + ThisColumnRect.Width + NextColumnRect.Width / 2));
        }
        private bool IsInsideRowAreaForData(int Y, Rectangle ThisRowRect, Rectangle NextRowRect)
        {
            return (Y > ThisRowRect.Y) && (Y < (ThisRowRect.Y + ThisRowRect.Height + NextRowRect.Height / 2));
        }
        private bool IsInsideRowAreaForColumnsHeader(int Y, Rectangle ThisRowRect)
        {
            return (Y < ColumnHeadersHeight + ThisRowRect.Height / 4);
        }
    }
}
