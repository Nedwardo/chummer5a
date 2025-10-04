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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace Chummer
{
    public static class ToolTipFactory
    {
        [System.CLSCompliant(false)]
        public static HtmlToolTip ToolTip { get; } = new HtmlToolTip
        {
            AllowLinksHandling = true,
            AutoPopDelay = 3600000,
            BaseStylesheet = null,
            InitialDelay = 250,
            IsBalloon = false,
            MaximumSize = new System.Drawing.Size(0, 0),
            OwnerDraw = true,
            ReshowDelay = 100,
            TooltipCssClass = "htmltooltip",
            UseGdiPlusTextRendering = true,
            //UseAnimation = true,
            //UseFading = true
        };

        public static void SetToolTip(this Control c, string caption)
        {
            caption = caption.CleanForHtml();
            c.DoThreadSafe(x => ToolTip.SetToolTip(x, caption));
        }

        public static async Task SetToolTipAsync(this Control c, string caption, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return;
            caption = caption.CleanForHtml();
            await c.DoThreadSafeAsync(x => ToolTip.SetToolTip(x, caption), token);
        }

        /// <summary>
        /// Clean up the global tooltip to prevent memory leaks.
        /// Call this when the application is shutting down.
        /// </summary>
        public static void DisposeGlobalToolTip()
        {
            ToolTip.RemoveAll();
            ToolTip.Dispose();
        }
    }
}
