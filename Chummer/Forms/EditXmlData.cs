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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using NLog;
using System.Diagnostics;

namespace Chummer
{
    public partial class EditXmlData : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private XmlDocument _objBaseXmlDocument;
        private XmlDocument _objAmendmentXmlDocument;
        private XmlDocument _objResultXmlDocument;
        private string _strBaseXmlContent;
        private string _strResultXmlContent;
        private string _strXmlElementTemplate = string.Empty;
        private bool _blnDirty;
        private bool _blnLoading = true;

        private DebuggableSemaphoreSlim _objFormClosingSemaphore = new DebuggableSemaphoreSlim();
        private CancellationTokenSource _objApplyAmendmentCancellationTokenSource = null;
        private CancellationTokenSource _objSaveAmendmentCancellationTokenSource = null;

        #region Form Events

        public EditXmlData()
        {
            InitializeComponent();
            Disposed += (sender, args) =>
            {
                CancellationTokenSource objOldSource = Interlocked.Exchange(ref _objApplyAmendmentCancellationTokenSource, null);
                if (objOldSource != null)
                {
                    objOldSource.Cancel(false);
                    objOldSource.Dispose();
                }
                objOldSource = Interlocked.Exchange(ref _objSaveAmendmentCancellationTokenSource, null);
                if (objOldSource != null)
                {
                    objOldSource.Cancel(false);
                    objOldSource.Dispose();
                }
                Interlocked.Exchange(ref _objFormClosingSemaphore, null)?.Dispose();
            };
        }

        private async void EditXmlData_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadAvailableXmlFilesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                string strTitle = await LanguageManager.GetStringAsync("XmlEditor_Error_LoadingFiles").ConfigureAwait(false);
                Log.Error(ex, strTitle);
                await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), strTitle, MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
            }
            finally
            {
                _blnLoading = false;
            }
        }

        private async void cmdLoadXml_Click(object sender, EventArgs e)
        {
            try
            {
                await LoadSelectedXmlFileAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                string strTitle = await LanguageManager.GetStringAsync("XmlEditor_Error_LoadingFile").ConfigureAwait(false);
                Log.Error(ex, strTitle);
                await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), strTitle, MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
            }
        }

        private async void cmdApplyAmendment_Click(object sender, EventArgs e)
        {
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationToken token = objNewSource.Token;
            CancellationTokenSource objOldSource = Interlocked.Exchange(ref _objApplyAmendmentCancellationTokenSource, objNewSource);
            if (objOldSource != null)
            {
                objOldSource.Cancel(false);
                objOldSource.Dispose();
            }
            try
            {
                await ApplyAmendmentAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                try
                {
                    string strTitle = await LanguageManager.GetStringAsync("XmlEditor_Error_ApplyingAmendment", token: token).ConfigureAwait(false);
                    Log.Error(ex, strTitle);
                    await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), strTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // swallow this
                }
            }
        }

        private async void cmdSaveAmendment_Click(object sender, EventArgs e)
        {
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationToken token = objNewSource.Token;
            CancellationTokenSource objOldSource = Interlocked.Exchange(ref _objSaveAmendmentCancellationTokenSource, objNewSource);
            if (objOldSource != null)
            {
                objOldSource.Cancel(false);
                objOldSource.Dispose();
            }
            try
            {
                await SaveAmendmentAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
            catch (Exception ex)
            {
                ex = ex.Demystify();
                try
                {
                    string strTitle = await LanguageManager.GetStringAsync("XmlEditor_Error_SavingAmendment", token: token).ConfigureAwait(false);
                    Log.Error(ex, strTitle);
                    await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), strTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // swallow this
                }
            }
        }

        private void txtAmendmentXml_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
            {
                _blnDirty = true;
                cmdApplyAmendment.Enabled = _objBaseXmlDocument != null && !string.IsNullOrWhiteSpace(txtAmendmentXml.Text);
            }
        }

        private async void EditXmlData_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                DebuggableSemaphoreSlim objSemaphore = _objFormClosingSemaphore;
                if (objSemaphore?.IsDisposed != false)
                    return;
                await objSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    Form frmSender = sender as Form;
                    if (frmSender != null)
                    {
                        e.Cancel = true; // Always have to cancel because of issues with async FormClosing events
                        await frmSender.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false); // Disable the form to make sure we can't interract with it anymore
                    }
                    try
                    {
                        bool blnDoClose = false;
                        _blnLoading = true;
                        try
                        {
                            // Caller returns and form stays open (weird async FormClosing event issue workaround)
                            await Task.Yield();

                            if (_blnDirty)
                            {
                                DialogResult result = await Program.ShowScrollableMessageBoxAsync(
                                    await LanguageManager.GetStringAsync("XmlEditor_UnsavedChanges").ConfigureAwait(false),
                                    await LanguageManager.GetStringAsync("XmlEditor_UnsavedChangesTitle").ConfigureAwait(false),
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question).ConfigureAwait(false);

                                switch (result)
                                {
                                    case DialogResult.Yes:
                                        {
                                            CancellationTokenSource objNewSource = new CancellationTokenSource();
                                            CancellationTokenSource objOldSource = Interlocked.Exchange(ref _objSaveAmendmentCancellationTokenSource, objNewSource);
                                            if (objOldSource != null)
                                            {
                                                objOldSource.Cancel(false);
                                                objOldSource.Dispose();
                                            }
                                            CancellationToken objNewSourceToken = objNewSource.Token;
                                            try
                                            {
                                                await SaveAmendmentAsync(objNewSourceToken).ConfigureAwait(false);
                                            }
                                            catch (OperationCanceledException)
                                            {
                                                //swallow this
                                            }
                                            catch (Exception ex)
                                            {
                                                ex = ex.Demystify();
                                                Log.Error(ex, "Error saving on form close");
                                                try
                                                {
                                                    await Program.ShowScrollableMessageBoxAsync(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_ErrorSaving").ConfigureAwait(false), ex.Message), await LanguageManager.GetStringAsync("XmlEditor_Error_SavingAmendment", token: objNewSourceToken).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error, token: objNewSourceToken).ConfigureAwait(false);
                                                }
                                                catch (OperationCanceledException)
                                                {
                                                    // swallow this
                                                }
                                                return;
                                            }
                                            break;
                                        }
                                    case DialogResult.Cancel:
                                        return;
                                }
                            }

                            blnDoClose = true;
                        }
                        finally
                        {
                            if (!blnDoClose)
                                _blnLoading = false;
                        }

                        // Now we close the original caller (weird async FormClosing event issue workaround)
                        if (frmSender != null)
                        {
                            await frmSender.DoThreadSafeAsync(x =>
                            {
                                x.FormClosing -= EditXmlData_FormClosing;
                                try
                                {
                                    x.Close();
                                }
                                catch
                                {
                                    // Ignore disposal errors if we are quitting the program anyway
                                    if (Program.MainForm.IsNullOrDisposed() || Program.MainForm.IsClosing)
                                        return;
                                    throw;
                                }
                            }).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        if (frmSender != null)
                            await frmSender.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false); // Doesn't matter if we're closed
                    }
                }
                finally
                {
                    if (objSemaphore?.IsDisposed == false)
                        objSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
        }

        #endregion Form Events

        #region Methods

        private async Task LoadAvailableXmlFilesAsync(CancellationToken token = default)
        {
            try
            {
                string strDataPath = Utils.GetDataFolderPath;
                if (!Directory.Exists(strDataPath))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("XmlEditor_DataDirectoryNotFound", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                    return;
                }

                // Get base XML files (exclude custom files)
                List<string> lstXmlFiles = new List<string>();
                foreach (string strFile in Directory.EnumerateFiles(strDataPath, "*.xml"))
                {
                    string strFileName = Path.GetFileName(strFile);
                    if (!strFileName.StartsWith("amend_", StringComparison.OrdinalIgnoreCase)
                        && !strFileName.StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                        && !strFileName.StartsWith("override_", StringComparison.OrdinalIgnoreCase))
                    {
                        lstXmlFiles.Add(strFile);
                    }
                }
                lstXmlFiles.Sort();
                await cboXmlFiles.DoThreadSafeAsync(x =>
                {
                    x.Items.Clear();
                    x.Items.AddRange(lstXmlFiles.ToArray());
                    if (x.Items.Count > 0)
                        x.SelectedIndex = 0;
                }, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    ex = ex.Demystify();
                    Log.Error(ex, "Error loading available XML files");
                }
                throw;
            }
        }

        private async Task LoadSelectedXmlFileAsync(CancellationToken token = default)
        {
            try
            {
                string strSelectedFile = await cboXmlFiles.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedFile))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("XmlEditor_NoFileSelected", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("XmlEditor_NoFileSelectedTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Load the base XML document (without amendments)
                    string strFilePath = Path.Combine(Utils.GetStartupPath, "data", strSelectedFile);
                    if (!File.Exists(strFilePath))
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_FileNotFound", token: token).ConfigureAwait(false), strFilePath), await LanguageManager.GetStringAsync("XmlEditor_FileNotFoundTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        return;
                    }

                    _objBaseXmlDocument = new XmlDocument();
                    await _objBaseXmlDocument.LoadStandardAsync(strFilePath, true, token: token).ConfigureAwait(false);
                    _blnDirty = false;
                    _strBaseXmlContent = _objBaseXmlDocument.OuterXml;
                    string strText = FormatXml(_strBaseXmlContent);
                    string strTemplate = await GetAmendmentTemplate(token).ConfigureAwait(false);
                    string strDiffPreviewText = await LanguageManager.GetStringAsync("XmlEditor_LoadInstructions", token: token).ConfigureAwait(false);
                    string strTitle = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_Title", token: token).ConfigureAwait(false), strSelectedFile);
                    await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                    try
                    {
                        // Update the UI
                        await txtBaseXml.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
                        await txtAmendmentXml.DoThreadSafeAsync(x => x.Text = strTemplate, token).ConfigureAwait(false);
                        await cmdApplyAmendment.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                        await cmdSaveAmendment.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                        await this.DoThreadSafeAsync(x => x.Text = strTitle, token).ConfigureAwait(false);
                        // Clear result areas
                        await txtResultXml.DoThreadSafeAsync(x => x.Text = string.Empty, token).ConfigureAwait(false);
                        await txtDiffPreview.DoThreadSafeAsync(x => x.Text = strDiffPreviewText, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await this.DoThreadSafeAsync(x => x.ResumeLayout(), token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    ex = ex.Demystify();
                    Log.Error(ex, await LanguageManager.GetStringAsync("XmlEditor_Error_LoadingFile", token: token).ConfigureAwait(false));
                }
                throw;
            }
        }

        private async Task ApplyAmendmentAsync(CancellationToken token = default)
        {
            try
            {
                if (_objBaseXmlDocument == null)
                {
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("XmlEditor_NoBaseXml", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("XmlEditor_NoBaseXmlTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtAmendmentXml.Text))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("XmlEditor_NoAmendment", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("XmlEditor_NoAmendmentTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    // Parse the amendment XML
                    try
                    {
                        _objAmendmentXmlDocument = new XmlDocument();
                        _objAmendmentXmlDocument.LoadXml(txtAmendmentXml.Text);
                    }
                    catch (XmlException ex)
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_InvalidXml", token: token).ConfigureAwait(false), ex.Message), await LanguageManager.GetStringAsync("XmlEditor_InvalidXmlTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        return;
                    }

                    // Create a copy of the base XML to apply amendments to
                    _objResultXmlDocument = new XmlDocument();
                    _objResultXmlDocument.LoadXml(_strBaseXmlContent);
                    // Apply the amendments using the same logic as XmlManager
                    Exception exFromAmend = await ApplyAmendmentOperationsAsync(_objResultXmlDocument, _objAmendmentXmlDocument, token).ConfigureAwait(false);
                    if (exFromAmend == default)
                    {
                        _strResultXmlContent = _objResultXmlDocument.OuterXml;
                        // Update the UI
                        string strResultText = FormatXml(_strResultXmlContent);
                        await txtResultXml.DoThreadSafeAsync(x => x.Text = strResultText, token).ConfigureAwait(false);
                        await cmdSaveAmendment.DoThreadSafeAsync(x => x.Enabled = true, token).ConfigureAwait(false);
                    }
                    else
                    {
                        _objResultXmlDocument = null;
                        _strResultXmlContent = string.Empty;
                        string strResultText = await LanguageManager.GetStringAsync("XmlEditor_Error_ApplyingAmendment", token: token).ConfigureAwait(false) + Environment.NewLine + exFromAmend.ToString();
                        await txtResultXml.DoThreadSafeAsync(x => x.Text = strResultText, token).ConfigureAwait(false);
                        await cmdSaveAmendment.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                    }
                    // Generate diff
                    await UpdateDiffPreviewAsync(token).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    ex = ex.Demystify();
                    Log.Error(ex, await LanguageManager.GetStringAsync("XmlEditor_Error_ApplyingAmendment", token: token).ConfigureAwait(false));
                    await cmdSaveAmendment.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                }
                throw;
            }
        }

        private async Task SaveAmendmentAsync(CancellationToken token = default)
        {
            try
            {
                string strText = await txtAmendmentXml.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(strText))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("XmlEditor_NothingToSave", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("XmlEditor_NothingToSaveTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                string strSelectedFile = await cboXmlFiles.DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strSelectedFile))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("XmlEditor_NoBaseFile", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("XmlEditor_NoBaseFileTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                using (SaveFileDialog dlgSave = new SaveFileDialog())
                {
                    dlgSave.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                    dlgSave.FileName = "amend_" + strSelectedFile;
                    dlgSave.Title = await LanguageManager.GetStringAsync("XmlEditor_SaveTitle", token: token).ConfigureAwait(false);

                    if (dlgSave.ShowDialog(this) == DialogResult.OK)
                    {
                        CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                        try
                        {
                            await FileExtensions.WriteAllTextAsync(dlgSave.FileName, strText, Encoding.UTF8, token).ConfigureAwait(false);
                            
                            await Program.ShowScrollableMessageBoxAsync(this, await LanguageManager.GetStringAsync("XmlEditor_SaveSuccess", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("XmlEditor_SaveSuccessTitle", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    ex = ex.Demystify();
                    Log.Error(ex, await LanguageManager.GetStringAsync("XmlEditor_Error_SavingAmendment", token: token).ConfigureAwait(false));
                }
                throw;
            }
        }


        private async Task UpdateDiffPreviewAsync(CancellationToken token = default)
        {
            try
            {
                if (_objBaseXmlDocument == null || _objResultXmlDocument == null)
                {
                    string strDiffPreviewText = await LanguageManager.GetStringAsync("XmlEditor_DiffInstructions", token: token).ConfigureAwait(false);
                    await txtDiffPreview.DoThreadSafeAsync(x =>
                    {
                        x.Text = strDiffPreviewText;
                        x.ForeColor = ColorManager.WindowText;
                    }, token).ConfigureAwait(false);
                    return;
                }

                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    // Generate a clean, user-friendly diff
                    string strDiffOutput = await GenerateCleanDiffAsync(_strBaseXmlContent, _strResultXmlContent, token).ConfigureAwait(false);

                    await txtDiffPreview.DoThreadSafeAsync(x =>
                    {
                        x.Text = strDiffOutput;
                        x.ForeColor = ColorManager.WindowText;
                    }, token).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                Log.Error(ex, await LanguageManager.GetStringAsync("XmlEditor_Error_UpdatingDiff", token: token).ConfigureAwait(false));
                string strText = string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_ErrorGeneratingDiff", token: token).ConfigureAwait(false), ex.Message);
                await txtDiffPreview.DoThreadSafeAsync(x =>
                {
                    x.Text = strText;
                    x.ForeColor = Color.Red;
                }, token).ConfigureAwait(false);
            }
        }

        private static async Task<string> GenerateCleanDiffAsync(string strBaseXml, string strResultXml, CancellationToken token = default)
        {
            try
            {
                // Parse both XML documents
                XmlDocument objBaseDoc = new XmlDocument();
                objBaseDoc.LoadXml(strBaseXml);

                XmlDocument objResultDoc = new XmlDocument();
                objResultDoc.LoadXml(strResultXml);

                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdOutput))
                {
                    sbdOutput.AppendLine(await LanguageManager.GetStringAsync("XmlEditor_AmendmentChanges", token: token).ConfigureAwait(false));
                    sbdOutput.AppendLine();

                    // Compare the documents and generate a clean diff
                    await CompareXmlDocumentsAsync(objBaseDoc, objResultDoc, sbdOutput, string.Empty, token).ConfigureAwait(false);

                    if (sbdOutput.Length <= 30) // Only header was added
                    {
                        sbdOutput.AppendLine(await LanguageManager.GetStringAsync("XmlEditor_NoChanges", token: token).ConfigureAwait(false));
                        sbdOutput.AppendLine(await LanguageManager.GetStringAsync("XmlEditor_NoChangesDescription", token: token).ConfigureAwait(false));
                    }

                    return sbdOutput.ToString();
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                Log.Error(ex, "Error generating clean diff");
                return string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_ErrorGeneratingCleanDiff", token: token).ConfigureAwait(false), ex.Message);
            }
        }

        private static async Task CompareXmlDocumentsAsync(XmlDocument objBaseDoc, XmlDocument objResultDoc, StringBuilder sbdOutput, string strCurrentPath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                // Compare root elements
                await CompareXmlNodesAsync(objBaseDoc.DocumentElement, objResultDoc.DocumentElement, sbdOutput, strCurrentPath, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                Log.Error(ex, "Error comparing XML documents");
                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_ErrorComparingDocuments", token: token).ConfigureAwait(false), ex.Message).AppendLine();
            }
        }

        private static async Task CompareXmlNodesAsync(XmlNode objBaseNode, XmlNode objResultNode, StringBuilder sbdOutput, string strCurrentPath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                if (objBaseNode == null && objResultNode == null)
                    return;

                string strNodePath = GetNodePath(objBaseNode ?? objResultNode, strCurrentPath);

                // Node was added
                if (objBaseNode == null && objResultNode != null)
                {
                    sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_Added", token: token).ConfigureAwait(false), strNodePath).AppendLine();
                    sbdOutput.AppendLine("  " + await FormatXmlNode(objResultNode, token).ConfigureAwait(false));
                    return;
                }

                // Node was removed
                if (objBaseNode != null && objResultNode == null)
                {
                    sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_Removed", token: token).ConfigureAwait(false), strNodePath).AppendLine();
                    sbdOutput.AppendLine("  " + await FormatXmlNode(objBaseNode, token).ConfigureAwait(false));
                    return;
                }

                // Compare text nodes
                if (objBaseNode.NodeType == XmlNodeType.Text && objResultNode.NodeType == XmlNodeType.Text)
                {
                    if (!string.Equals(objBaseNode.Value, objResultNode.Value, StringComparison.Ordinal))
                    {
                        sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_Modified", token: token).ConfigureAwait(false), strNodePath).AppendLine();
                        sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_OldValue", token: token).ConfigureAwait(false), FormatXmlNode(objBaseNode, token)).AppendLine();
                        sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_NewValue", token: token).ConfigureAwait(false), FormatXmlNode(objResultNode, token)).AppendLine();
                    }
                    return;
                }

                // Compare element nodes
                if (objBaseNode.NodeType == XmlNodeType.Element && objResultNode.NodeType == XmlNodeType.Element)
                {
                    // Compare attributes
                    if (objBaseNode.Attributes != null || objResultNode.Attributes != null)
                    {
                        Dictionary<string, string> baseAttribs = new Dictionary<string, string>(objBaseNode.Attributes?.Count ?? 0);
                        foreach (XmlAttribute objAttribute in objBaseNode.Attributes)
                        {
                            baseAttribs.Add(objAttribute.Name, objAttribute.Value);
                        }
                        Dictionary<string, string> resultAttribs = new Dictionary<string, string>(objResultNode.Attributes?.Count ?? 0);
                        foreach (XmlAttribute objAttribute in objResultNode.Attributes)
                        {
                            resultAttribs.Add(objAttribute.Name, objAttribute.Value);
                        }

                        foreach (KeyValuePair<string, string> kvp in baseAttribs)
                        {
                            if (!resultAttribs.TryGetValue(kvp.Key, out string value) || !resultAttribs.Remove(kvp.Key))
                            {
                                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_RemovedAttribute", token: token).ConfigureAwait(false), strNodePath, kvp.Key, kvp.Value).AppendLine();
                            }
                            else if (!string.Equals(value, kvp.Value, StringComparison.Ordinal))
                            {
                                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_ModifiedAttribute", token: token).ConfigureAwait(false), strNodePath, kvp.Key).AppendLine();
                                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_OldValue", token: token).ConfigureAwait(false), '\"' + kvp.Value + '\"').AppendLine();
                                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_NewValue", token: token).ConfigureAwait(false), '\"' + value + '\"').AppendLine();
                            }
                        }

                        foreach (KeyValuePair<string, string> kvp in resultAttribs)
                        {
                            sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_AddedAttribute", token: token).ConfigureAwait(false), strNodePath, kvp.Key, kvp.Value).AppendLine();
                        }
                    }

                    // Compare text content first (for elements with only text content)
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdBaseText))
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdResultText))
                    {
                        foreach (XmlNode objNode in objBaseNode.ChildNodes)
                        {
                            if (objNode.NodeType == XmlNodeType.Text)
                                sbdBaseText.Append(objNode.Value);
                        }
                        foreach (XmlNode objNode in objResultNode.ChildNodes)
                        {
                            if (objNode.NodeType == XmlNodeType.Text)
                                sbdResultText.Append(objNode.Value);
                        }

                        if (sbdBaseText.Length > 0 || sbdResultText.Length > 0)
                        {
                            string strBaseText = sbdBaseText.ToString();
                            string strResultText = sbdResultText.ToString();

                            if (!string.Equals(strBaseText, strResultText, StringComparison.Ordinal))
                            {
                                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_Modified", token: token).ConfigureAwait(false), strNodePath).AppendLine();
                                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_OldValue", token: token).ConfigureAwait(false), '\"' + strBaseText + '\"').AppendLine();
                                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_NewValue", token: token).ConfigureAwait(false), '\"' + strResultText + '\"').AppendLine();
                            }
                        }
                    }

                    // Compare child element nodes
                    // Group children by their identifying attributes or content
                    Dictionary<string, XmlNode> baseGroups = GroupChildNodes(objBaseNode.ChildNodes);
                    Dictionary<string, XmlNode> resultGroups = GroupChildNodes(objResultNode.ChildNodes);

                    // Find added, removed, and modified nodes
                    foreach (KeyValuePair<string, XmlNode> kvp in resultGroups)
                    {
                        if (!baseGroups.TryGetValue(kvp.Key, out XmlNode value) || !baseGroups.Remove(kvp.Key))
                        {
                            string strChildPath = GetNodePath(kvp.Value, strNodePath);
                            sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_Added", token: token).ConfigureAwait(false), strChildPath).AppendLine();
                            sbdOutput.Append("  " + await FormatXmlNode(kvp.Value, token).ConfigureAwait(false));
                        }
                        else
                        {
                            await CompareXmlNodesAsync(value, kvp.Value, sbdOutput, strNodePath, token).ConfigureAwait(false);
                        }
                    }

                    foreach (KeyValuePair<string, XmlNode> kvp in baseGroups)
                    {
                        string strChildPath = GetNodePath(kvp.Value, strNodePath);
                        sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_Removed", token: token).ConfigureAwait(false), strChildPath).AppendLine();
                        sbdOutput.AppendLine("  " + await FormatXmlNode(kvp.Value, token).ConfigureAwait(false));
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                Log.Error(ex, "Error comparing XML nodes");
                sbdOutput.AppendFormat(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("XmlEditor_ErrorComparingNodes", token: token).ConfigureAwait(false ), strCurrentPath, ex.Message).AppendLine();
            }
        }

        private static Dictionary<string, XmlNode> GroupChildNodes(XmlNodeList childNodes)
        {
            if (childNodes == null)
                return new Dictionary<string, XmlNode>(0);
            Dictionary<string, XmlNode> groups = new Dictionary<string, XmlNode>(childNodes.Count);
            foreach (XmlNode node in childNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                // Try to find an ID attribute first
                string strKey = node.Attributes?["id"]?.Value;
                
                // If no ID, try to use name attribute
                if (string.IsNullOrEmpty(strKey))
                {
                    strKey = node.Attributes?["name"]?.Value;
                    // If still no key, use the node name with index
                    if (string.IsNullOrEmpty(strKey))
                    {
                        strKey = node.Name + '_' + groups.Count.ToString(GlobalSettings.InvariantCultureInfo);
                    }
                }
                
                groups[strKey] = node;
            }
            
            return groups;
        }

        private static string GetNodePath(XmlNode node, string strCurrentPath)
        {
            if (node == null)
                return strCurrentPath;

            string strNodeName = node.Name;
            
            // For element nodes, try to include ID or name attribute
            if (node.NodeType == XmlNodeType.Element)
            {
                string strId = node.Attributes?["id"]?.Value;

                if (!string.IsNullOrEmpty(strId))
                {
                    strNodeName = strNodeName + "[id=" + strId + ']';
                }
                else
                {
                    string strName = node.Attributes?["name"]?.Value;
                    if (!string.IsNullOrEmpty(strName))
                    {
                        strNodeName = strNodeName + "[name=" + strName + ']';
                    }
                }
            }
            
            return string.IsNullOrEmpty(strCurrentPath) ? strNodeName : strCurrentPath + '/' + strNodeName;
        }

        private static async Task<string> FormatXmlNode(XmlNode node, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    return '\"' + node.Value + '\"';
                }
                
                if (node.NodeType == XmlNodeType.Element)
                {
                    // Create a temporary document to format just this node
                    XmlDocument tempDoc = new XmlDocument();
                    XmlNode importedNode = tempDoc.ImportNode(node, true);
                    tempDoc.AppendChild(importedNode);
                    
                    // Format the XML and get the inner content
                    using (StringWriter stringWriter = new StringWriter())
                    {
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter))
                        {
                            xmlWriter.Formatting = Formatting.Indented;
                            xmlWriter.Indentation = 2;
                            importedNode.WriteTo(xmlWriter);
                        }
                        return stringWriter.ToString().Trim();
                    }
                }
                
                return node.ToString();
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                ex = ex.Demystify();
                Log.Error(ex, await LanguageManager.GetStringAsync("XmlEditor_ErrorFormattingNode", token: token).ConfigureAwait(false));
                return node.ToString();
            }
        }

        private static string FormatXml(string strXml)
        {
            try
            {
                XmlDocument objDoc = new XmlDocument();
                objDoc.LoadXml(strXml);
                using (StringWriter objStringWriter = new StringWriter())
                {
                    using (XmlTextWriter objXmlWriter = new XmlTextWriter(objStringWriter))
                    {
                        objXmlWriter.Formatting = Formatting.Indented;
                        objXmlWriter.Indentation = 2;
                        objDoc.WriteTo(objXmlWriter);
                    }
                    return objStringWriter.ToString();
                }
            }
            catch
            {
                return strXml; // Return original if formatting fails
            }
        }

        private async Task<string> GetAmendmentTemplate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(_strXmlElementTemplate))
            {
                _strXmlElementTemplate = await LanguageManager.GetStringAsync("XmlEditor_AmendmentTemplate", token: token).ConfigureAwait(false);
            }
            return _strXmlElementTemplate;
        }

        private static async Task<Exception> ApplyAmendmentOperationsAsync(XmlDocument xmlTargetDoc, XmlDocument xmlAmendmentDoc, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (XmlNodeList xmlNodeList = xmlAmendmentDoc.SelectNodes("/chummer/*"))
            {
                token.ThrowIfCancellationRequested();
                if (xmlNodeList?.Count > 0)
                {
                    token.ThrowIfCancellationRequested();
                    foreach (XmlNode objNode in xmlNodeList)
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            if (await Task.Run(() => XmlManager.AmendNodeChildren(xmlTargetDoc, objNode, "/chummer", token: token), token).ConfigureAwait(false))
                            {
                                Log.Info("Successfully applied amendment operation to node: " + objNode.Name);
                            }
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            // Return exceptions so that they can be displayed to the user (and so that we know we ran into one while applying our operation)
                            ex = ex.Demystify();
                            Log.Error(ex, await LanguageManager.GetStringAsync("XmlEditor_Error_ApplyingAmendment", token: token).ConfigureAwait(false));
                            return ex;
                        }
                    }
                }
            }
            return default;
        }

        #endregion Methods
    }
}
