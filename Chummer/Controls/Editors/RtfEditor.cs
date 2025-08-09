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
using System.Drawing;
using System.Windows.Forms;

namespace Chummer.UI.Editors
{
    public partial class RtfEditor : UserControl
    {
        public RtfEditor()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            if (!Utils.IsDesignerMode && !Utils.IsRunningInVisualStudio)
                tsControls.Visible = false;
        }

        public KeyEventHandler ContentKeyDown { get; set; }

        public void FocusContent()
        {
            rtbContent.Focus();
        }

        private void UpdateFont(object sender, EventArgs e)
        {
            FontStyle eNewFontStyle = FontStyle.Regular;

            if (tsbBold.Checked)
            {
                eNewFontStyle |= FontStyle.Bold;
            }
            if (tsbItalic.Checked)
            {
                eNewFontStyle |= FontStyle.Italic;
            }
            if (tsbUnderline.Checked)
            {
                eNewFontStyle |= FontStyle.Underline;
            }
            if (tsbStrikeout.Checked)
            {
                eNewFontStyle |= FontStyle.Strikeout;
            }
            if (tsbSuperscript.Checked)
            {
                rtbContent.SelectionCharOffset = 3;
            }
            else if (tsbSubscript.Checked)
            {
                rtbContent.SelectionCharOffset = -3;
            }
            else
            {
                rtbContent.SelectionCharOffset = 0;
            }
            if (tsbAlignRight.Checked)
            {
                if (rtbContent.SelectionAlignment != HorizontalAlignment.Right)
                {
                    (rtbContent.SelectionIndent, rtbContent.SelectionRightIndent) = (rtbContent.SelectionRightIndent, rtbContent.SelectionIndent);
                    rtbContent.SelectionAlignment = HorizontalAlignment.Right;
                }
            }
            else if (tsbAlignCenter.Checked)
            {
                if (rtbContent.SelectionAlignment != HorizontalAlignment.Center)
                {
                    rtbContent.SelectionIndent = 0;
                    rtbContent.SelectionRightIndent = 0;
                    rtbContent.SelectionAlignment = HorizontalAlignment.Center;
                }
            }
            else
            {
                if (rtbContent.SelectionAlignment != HorizontalAlignment.Left)
                {
                    (rtbContent.SelectionIndent, rtbContent.SelectionRightIndent) = (rtbContent.SelectionRightIndent, rtbContent.SelectionIndent);
                    rtbContent.SelectionAlignment = HorizontalAlignment.Left;
                }
                if (!tsbAlignLeft.Checked)
                    UpdateButtons(sender, e);
            }
            try
            {
                Font objCurrentFont = rtbContent.SelectionFont ?? DefaultFont;
                rtbContent.SelectionFont = new Font(objCurrentFont.FontFamily, objCurrentFont.Size, eNewFontStyle);
            }
            catch (ArgumentException)
            {
                UpdateButtons(sender, e);
            }

            rtbContent.Focus();
        }

        private void UpdateButtons(object sender, EventArgs e)
        {
            if (rtbContent.SelectionFont != null)
            {
                tsbBold.Checked = rtbContent.SelectionFont.Bold;
                tsbItalic.Checked = rtbContent.SelectionFont.Italic;
                tsbUnderline.Checked = rtbContent.SelectionFont.Underline;
                tsbStrikeout.Checked = rtbContent.SelectionFont.Strikeout;
            }
            else // Backup for weird cases where selection has no font, use the default font of the RichTextBox
            {
                tsbBold.Checked = rtbContent.Font.Bold;
                tsbItalic.Checked = rtbContent.Font.Italic;
                tsbUnderline.Checked = rtbContent.Font.Underline;
                tsbStrikeout.Checked = rtbContent.Font.Strikeout;
            }
            tsbSuperscript.Checked = IsSuperscript;
            tsbSubscript.Checked = IsSubscript;
            tsbAlignLeft.Checked = IsJustifyLeft;
            tsbAlignCenter.Checked = IsJustifyCenter;
            tsbAlignRight.Checked = IsJustifyRight;
            tsbUnorderedList.Checked = rtbContent.SelectionBullet;
            tsbIncreaseIndent.Enabled = !IsJustifyCenter;
            tsbDecreaseIndent.Enabled = !IsJustifyCenter;
            rtbContent.Cursor = rtbContent.SelectionType == RichTextBoxSelectionTypes.Object
                ? Cursors.SizeAll
                : Cursors.IBeam;
        }

        public event EventHandler RtfContentChanged;

        #region Properties

        public string Rtf
        {
            get => rtbContent.Rtf;
            set
            {
                if (value.IsRtf())
                {
                    rtbContent.Rtf = string.IsNullOrWhiteSpace(value.RtfToPlainText()) ? string.Empty : value;
                }
                else
                {
                    rtbContent.Text = value.NormalizeWhiteSpace();
                }
            }
        }

        public override string Text
        {
            get => rtbContent.Text;
            set => rtbContent.Text = value.RtfToPlainText();
        }

        private bool IsSuperscript => rtbContent.SelectionCharOffset > 0;

        private bool IsSubscript => rtbContent.SelectionCharOffset < 0;

        private bool IsJustifyLeft => rtbContent.SelectionAlignment == HorizontalAlignment.Left;

        private bool IsJustifyCenter => rtbContent.SelectionAlignment == HorizontalAlignment.Center;

        private bool IsJustifyRight => rtbContent.SelectionAlignment == HorizontalAlignment.Right;

        #endregion Properties

        #region Control Methods

        private void tsbFont_Click(object sender, EventArgs e)
        {
            using (FontDialog dlgNewFont = new FontDialog())
            {
                dlgNewFont.Font = rtbContent.SelectionFont;
                dlgNewFont.FontMustExist = true;
                if (dlgNewFont.ShowDialog(this) != DialogResult.OK)
                    return;
                rtbContent.SelectionFont = dlgNewFont.Font;
            }
            UpdateButtons(sender, e);
        }

        private void tsbForeColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlgNewColor = new ColorDialog())
            {
                dlgNewColor.Color = rtbContent.SelectionColor;
                if (dlgNewColor.ShowDialog(this) != DialogResult.OK)
                    return;
                rtbContent.SelectionColor = dlgNewColor.Color;
            }
        }

        private void tsbBackColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlgNewColor = new ColorDialog())
            {
                dlgNewColor.Color = rtbContent.SelectionBackColor;
                if (dlgNewColor.ShowDialog(this) != DialogResult.OK)
                    return;
                rtbContent.SelectionBackColor = dlgNewColor.Color;
            }
        }

        private void tsbSuperscript_Click(object sender, EventArgs e)
        {
            if (tsbSuperscript.Checked)
            {
                rtbContent.SelectionCharOffset = 3;
                tsbSubscript.Checked = false;
            }
            else
            {
                rtbContent.SelectionCharOffset = 0;
            }
        }

        private void tsbSubscript_Click(object sender, EventArgs e)
        {
            if (tsbSubscript.Checked)
            {
                rtbContent.SelectionCharOffset = -3;
                tsbSuperscript.Checked = false;
            }
            else
            {
                rtbContent.SelectionCharOffset = 0;
            }
        }

        private void tsbUnorderedList_Click(object sender, EventArgs e)
        {
            rtbContent.SelectionBullet = tsbUnorderedList.Checked;
        }

        private void tsbAlignLeft_Click(object sender, EventArgs e)
        {
            tsbAlignCenter.Checked = false;
            tsbAlignRight.Checked = false;
            UpdateFont(sender, e);
        }

        private void tsbAlignCenter_Click(object sender, EventArgs e)
        {
            tsbAlignLeft.Checked = false;
            tsbAlignRight.Checked = false;
            UpdateFont(sender, e);
        }

        private void tsbAlignRight_Click(object sender, EventArgs e)
        {
            tsbAlignLeft.Checked = false;
            tsbAlignCenter.Checked = false;
            UpdateFont(sender, e);
        }

        private void tsbAlignLeft_CheckedChanged(object sender, EventArgs e)
        {
            tsbAlignLeft.CheckOnClick = !tsbAlignLeft.Checked;
        }

        private void tsbAlignCenter_CheckedChanged(object sender, EventArgs e)
        {
            tsbAlignCenter.CheckOnClick = !tsbAlignCenter.Checked;
        }

        private void tsbAlignRight_CheckedChanged(object sender, EventArgs e)
        {
            tsbAlignRight.CheckOnClick = !tsbAlignRight.Checked;
        }

        private void tsbIncreaseIndent_Click(object sender, EventArgs e)
        {
            if (IsJustifyLeft)
                rtbContent.SelectionIndent += rtbContent.BulletIndent;
            else if (IsJustifyRight)
                rtbContent.SelectionRightIndent += rtbContent.BulletIndent;
        }

        private void tsbDecreaseIndent_Click(object sender, EventArgs e)
        {
            if (IsJustifyLeft)
                rtbContent.SelectionIndent = Math.Max(0, rtbContent.SelectionIndent - rtbContent.BulletIndent);
            else if (IsJustifyRight)
                rtbContent.SelectionRightIndent = Math.Max(0, rtbContent.SelectionRightIndent - rtbContent.BulletIndent);
        }

        private void rtbContent_Enter(object sender, EventArgs e)
        {
            tsControls.Visible = true;
        }

        private void rtbContent_Leave(object sender, EventArgs e)
        {
            tsControls.Visible = false;
        }

        private void rtbContent_KeyDown(object sender, KeyEventArgs e)
        {
            ContentKeyDown?.Invoke(sender, e);
        }

        #endregion Control Methods

        private void rtbContent_TextChanged(object sender, EventArgs e)
        {
            RtfContentChanged?.Invoke(sender, e);
        }
    }
}
