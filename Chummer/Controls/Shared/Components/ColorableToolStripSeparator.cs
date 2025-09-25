/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Chummer
{
    public class ColorableToolStripSeparator : ToolStripSeparator
    {
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _objBackColorBrush?.Dispose();
                _objForeColorPen?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (DefaultColorScheme || !Visible)
                return;
            // Get the separator's width and height.
            int intWidth = Width;
            if (intWidth <= 4)
                return;
            int intHeight = Height;
            if (intHeight <= 0)
                return;
            // Fill the background.
            if (_objBackColorBrush == null)
            {
                SolidBrush objNewBrush = new SolidBrush(BackColor);
                if (Interlocked.CompareExchange(ref _objBackColorBrush, objNewBrush, null) != null)
                    objNewBrush.Dispose();
            }

            e.Graphics.FillRectangle(_objBackColorBrush, 0, 0, intWidth, intHeight);
            // Draw the line.
            if (_objForeColorPen == null)
            {
                Pen objNewPen = new Pen(ForeColor);
                if (Interlocked.CompareExchange(ref _objForeColorPen, objNewPen, null) != null)
                    objNewPen.Dispose();
            }
            int intMargin = (4 * e.Graphics.DpiX / 96.0f).StandardRound();
            e.Graphics.DrawLine(_objForeColorPen, intMargin, intHeight / 2, intWidth - intMargin, intHeight / 2);
        }

        private int _intDefaultColorScheme = ColorManager.IsLightMode.ToInt32();
        private Color _objBackColor;
        private Color _objForeColor;
        private SolidBrush _objBackColorBrush;
        private Pen _objForeColorPen;

        public bool DefaultColorScheme
        {
            get => _intDefaultColorScheme > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intDefaultColorScheme, intNewValue) == intNewValue)
                    return;
                Invalidate();
            }
        }

        /// <inheritdoc />
        public override Color BackColor
        {
            get => _objBackColor;
            set
            {
                if (_objBackColor == value)
                    return;
                _objBackColor = value;
                Interlocked.Exchange(ref _objBackColorBrush, new SolidBrush(value))?.Dispose();
            }
        }

        /// <inheritdoc />
        public override Color ForeColor
        {
            get => _objForeColor;
            set
            {
                if (_objForeColor == value)
                    return;
                _objForeColor = value;
                Interlocked.Exchange(ref _objForeColorPen, new Pen(value))?.Dispose();
            }
        }
    }
}
