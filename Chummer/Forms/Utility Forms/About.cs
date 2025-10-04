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
using System.Reflection;
using System.Windows.Forms;

namespace Chummer
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

        }

        #region Assembly Attribute Accessors

        public static string AssemblyTitle
        {
            get
            {
                object[] attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!string.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            }
        }

        public static string AssemblyVersion => Utils.CurrentChummerVersion.ToString(3);

        public static string AssemblyDescription
        {
            get
            {
                object[] attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyDescriptionAttribute)attributes[0]).Description.NormalizeLineEndings();
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product.NormalizeLineEndings().WordWrap();
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright.NormalizeLineEndings().WordWrap();
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyCompanyAttribute)attributes[0]).Company.NormalizeLineEndings().WordWrap();
            }
        }

        #endregion Assembly Attribute Accessors

        #region Controls Methods

        private async void About_Load(object sender, EventArgs e)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
            string strReturn = await LanguageManager.GetStringAsync("Label_About", false).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strReturn))
                strReturn = "About";
            await this.DoThreadSafeAsync(x => x.Text = strReturn + strSpace + AssemblyTitle).ConfigureAwait(false);
            await lblProductName.DoThreadSafeAsync(x => x.Text = AssemblyProduct).ConfigureAwait(false);
            string strReturn2 = await LanguageManager.GetStringAsync("String_Version", false).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strReturn2))
                strReturn2 = "Version";
            await lblVersion.DoThreadSafeAsync(x => x.Text = strReturn2 + strSpace + AssemblyVersion).ConfigureAwait(false);
            string strReturn3 = await LanguageManager.GetStringAsync("About_Copyright_Text", false).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strReturn3))
                strReturn3 = AssemblyCopyright;
            await lblCopyright.DoThreadSafeAsync(x => x.Text = strReturn3).ConfigureAwait(false);
            string strReturn4 = await LanguageManager.GetStringAsync("About_Company_Text", false).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strReturn4))
                strReturn4 = AssemblyCompany;
            await lblCompanyName.DoThreadSafeAsync(x => x.Text = strReturn4).ConfigureAwait(false);
            string strReturn5 = await LanguageManager.GetStringAsync("About_Description_Text", false).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strReturn5))
                strReturn5 = AssemblyDescription;
            await txtDescription.DoThreadSafeAsync(x => x.Text = strReturn5).ConfigureAwait(false);
            await txtContributors.DoThreadSafeAsync(x => x.Text += Environment.NewLine + Environment.NewLine + StringExtensions.JoinFast(Environment.NewLine, Properties.Contributors.Usernames)
                                                                   + Environment.NewLine + "/u/Iridios").ConfigureAwait(false);
            string strDisclaimer = await LanguageManager.GetStringAsync("About_Label_Disclaimer_Text").ConfigureAwait(false);
            await txtDisclaimer.DoThreadSafeAsync(x => x.Text = strDisclaimer).ConfigureAwait(false);
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    DialogResult = DialogResult.OK;
                    Close();
                    break;

                case Keys.A:
                    {
                        if (e.Control)
                        {
                            e.SuppressKeyPress = true;
                            (sender as TextBox)?.SelectAll();
                        }

                        break;
                    }
            }
        }

        #endregion Controls Methods
    }
}
