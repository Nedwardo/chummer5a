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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using Chummer.Properties;
using Timer = System.Windows.Forms.Timer;

namespace Chummer.UI.Skills
{
    [DebuggerDisplay("{_objSkill.Name}")]
    public sealed partial class SkillControl : UserControl
    {
        private readonly bool _blnLoading = true;
        private int _intUpdatingSpec = 1;
        private readonly Skill _objSkill;
        private readonly Timer _tmrSpecChangeTimer;
        private readonly Font _fntNormal;
        private readonly Font _fntItalic;
        private readonly Font _fntNormalName;
        private readonly Font _fntItalicName;
        private readonly Font _fntNormalSpec;
        private readonly Font _fntStrikethroughSpec;
        private CharacterAttrib _objAttributeActive;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Button cmdDelete;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ButtonWithToolTip btnCareerIncrease;

        private readonly Label lblCareerRating;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly NumericUpDownEx nudKarma;

        private readonly NumericUpDownEx nudSkill;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Label lblCareerSpec;

        private readonly ButtonWithToolTip btnAddSpec;
        private readonly ElasticComboBox cboSpec;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ColorableCheckBox chkKarma;

        private readonly ElasticComboBox cboSelectAttribute;

        private readonly CancellationToken _objMyToken;

        public SkillControl(Skill objSkill, CancellationToken objMyToken = default)
        {
            if (objSkill == null)
                return;
            _objMyToken = objMyToken;
            _objSkill = objSkill;
            _objAttributeActive = objSkill.AttributeObject;
            InitializeComponent();
            Disposed += (sender, args) => UnbindSkillControl();
            SuspendLayout();
            pnlAttributes.SuspendLayout();
            tlpMain.SuspendLayout();
            tlpRight.SuspendLayout();
            try
            {
                //Display
                _fntNormalName = lblName.Font;
                _fntItalicName = new Font(_fntNormalName, FontStyle.Italic);
                _fntNormal = btnAttribute.Font;
                _fntItalic = new Font(_fntNormal, FontStyle.Italic);
                Disposed += (sender, args) =>
                {
                    _fntItalicName.Dispose();
                    _fntItalic.Dispose();
                };

                if (!_objSkill.Default)
                    lblName.Font = _fntItalicName;

                // To make sure that the initial load formats the name column properly, we need to set the attribute name in the constructor
                lblName.Text = _objSkill.CurrentDisplayName;

                // Creating controls outside of the designer saves on handles if the controls would be invisible anyway
                if (objSkill.AllowDelete) // For active skills, can only change by going from Create to Career mode, so no databinding necessary
                {
                    cmdDelete = new Button
                    {
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(3, 0, 3, 0),
                        Name = "cmdDelete",
                        Tag = "String_Delete",
                        Text = "Delete",
                        UseVisualStyleBackColor = true
                    };
                    cmdDelete.Click += cmdDelete_Click;
                    tlpRight.Controls.Add(cmdDelete, 4, 0);
                }

                if (objSkill.CharacterObject.Created)
                {
                    lblCareerRating = new Label
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblCareerRating",
                        Text = "00",
                        TextAlign = ContentAlignment.MiddleRight
                    };
                    btnCareerIncrease = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        MinimumSize = new Size(24, 24),
                        Name = "btnCareerIncrease",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
                    btnCareerIncrease.BatchSetImages(Resources.add_16, Resources.add_20, Resources.add_24,
                        Resources.add_32, Resources.add_48, Resources.add_64);
                    btnCareerIncrease.Click += btnCareerIncrease_Click;

                    tlpMain.Controls.Add(lblCareerRating, 2, 0);
                    tlpMain.Controls.Add(btnCareerIncrease, 3, 0);

                    btnAddSpec = new ButtonWithToolTip
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        MinimumSize = new Size(24, 24),
                        Name = "btnAddSpec",
                        Padding = new Padding(1),
                        UseVisualStyleBackColor = true
                    };
                    btnAddSpec.BatchSetImages(Resources.add_16, Resources.add_20, Resources.add_24,
                        Resources.add_32, Resources.add_48, Resources.add_64);
                    btnAddSpec.Click += btnAddSpec_Click;
                    lblCareerSpec = new Label
                    {
                        Anchor = AnchorStyles.Left,
                        AutoSize = true,
                        Margin = new Padding(3, 6, 3, 6),
                        Name = "lblCareerSpec",
                        Text = "[Specializations]",
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    _fntNormalSpec = lblCareerSpec.Font;
                    _fntStrikethroughSpec = new Font(_fntNormalSpec, FontStyle.Strikeout);
                    Disposed += (sender, args) => _fntStrikethroughSpec.Dispose();

                    tlpRight.Controls.Add(lblCareerSpec, 0, 0);
                    tlpRight.Controls.Add(btnAddSpec, 1, 0);

                    cboSelectAttribute = new ElasticComboBox
                    {
                        Dock = DockStyle.Fill,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        FormattingEnabled = true,
                        Margin = new Padding(3, 0, 3, 0),
                        Name = "cboSelectAttribute"
                    };
                    cboSelectAttribute.DropDownClosed += cboSelectAttribute_Closed;
                    pnlAttributes.Controls.Add(cboSelectAttribute);
                }
                else
                {
                    nudSkill = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = GlobalSettings.InterceptMode,
                        Margin = new Padding(3, 2, 3, 2),
                        Name = "nudSkill"
                    };
                    nudKarma = new NumericUpDownEx
                    {
                        Anchor = AnchorStyles.Right,
                        AutoSize = true,
                        InterceptMouseWheel = GlobalSettings.InterceptMode,
                        Margin = new Padding(3, 2, 3, 2),
                        Name = "nudKarma"
                    };

                    // Trick to make it seem like the button is a label (+ onclick method not doing anything in Create mode)
                    btnAttribute.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    btnAttribute.FlatAppearance.MouseOverBackColor = Color.Transparent;

                    tlpMain.Controls.Add(nudSkill, 2, 0);
                    tlpMain.Controls.Add(nudKarma, 3, 0);

                    if (objSkill.IsExoticSkill)
                    {
                        lblCareerSpec = new Label
                        {
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            Margin = new Padding(3, 6, 3, 6),
                            Name = "lblCareerSpec",
                            Text = "[Specializations]",
                            TextAlign = ContentAlignment.MiddleLeft
                        };
                        tlpRight.Controls.Add(lblCareerSpec, 0, 0);
                    }
                    else
                    {
                        cboSpec = new ElasticComboBox
                        {
                            Anchor = AnchorStyles.Left | AnchorStyles.Right,
                            AutoCompleteMode = AutoCompleteMode.Suggest,
                            FormattingEnabled = true,
                            Margin = new Padding(3, 0, 3, 0),
                            Name = "cboSpec",
                            TabStop = false
                        };
                        cboSpec.TextChanged += cboSpec_TextChanged;
                        _tmrSpecChangeTimer = new Timer { Interval = 1000 };
                        _tmrSpecChangeTimer.Tick += SpecChangeTimer_Tick;
                        chkKarma = new ColorableCheckBox
                        {
                            Anchor = AnchorStyles.Left,
                            AutoSize = true,
                            DefaultColorScheme = true,
                            Margin = new Padding(3, 4, 3, 4),
                            Name = "chkKarma",
                            UseVisualStyleBackColor = true
                        };
                        tlpRight.Controls.Add(cboSpec, 0, 0);
                        tlpRight.Controls.Add(chkKarma, 1, 0);
                    }
                }

                this.UpdateLightDarkMode(token: objMyToken);
                this.TranslateWinForm(blnDoResumeLayout: false, token: objMyToken);

                foreach (ToolStripItem tssItem in cmsSkillLabel.Items)
                {
                    tssItem.UpdateLightDarkMode(token: objMyToken);
                    tssItem.TranslateToolStripItemsRecursively(token: objMyToken);
                }
            }
            finally
            {
                _blnLoading = false;
                tlpRight.ResumeLayout();
                tlpMain.ResumeLayout();
                pnlAttributes.ResumeLayout();
                ResumeLayout(true);
            }

            _objSkill.MultiplePropertiesChangedAsync += Skill_PropertyChanged;
            if (_objAttributeActive != null)
            {
                _objAttributeActive.MultiplePropertiesChangedAsync += Attribute_PropertyChanged;
            }
        }

        private void DoDataBindings()
        {
            try
            {
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _objSkill,
                                                       nameof(Skill.CurrentDisplayName),
                                                       x => x.GetCurrentDisplayNameAsync(_objMyToken)
                                                             ,
                                                       _objMyToken);
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.ForeColor = y, _objSkill,
                                                       nameof(Skill.PreferredColor),
                                                       x => x.GetPreferredColorAsync(_objMyToken),
                                                       _objMyToken);
                lblName.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _objSkill,
                                                       nameof(Skill.SkillToolTip),
                                                       x => x.GetSkillToolTipAsync(_objMyToken),
                                                       _objMyToken);

                btnAttribute.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _objSkill,
                                                            nameof(Skill.DisplayAttribute),
                                                            x => x.GetDisplayAttributeAsync(_objMyToken)
                                                                  ,
                                                            _objMyToken);

                RefreshPoolTooltipAndDisplay(_objMyToken);

                if (_objSkill.CharacterObject.Created)
                {
                    lblCareerRating.RegisterOneWayAsyncDataBinding(
                        (x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objSkill,
                        nameof(Skill.Rating),
                        x => x.GetRatingAsync(_objMyToken),
                        _objMyToken);
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                                     nameof(Skill.CanUpgradeCareer),
                                                                     x => x.GetCanUpgradeCareerAsync(
                                                                               _objMyToken)
                                                                           ,
                                                                     _objMyToken);
                    btnCareerIncrease.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _objSkill,
                                                                     nameof(Skill.UpgradeToolTip),
                                                                     x => x.GetUpgradeToolTipAsync(
                                                                               _objMyToken)
                                                                           ,
                                                                     _objMyToken);
                    lblCareerSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _objSkill,
                                                                 nameof(Skill.CurrentDisplaySpecialization),
                                                                 x => x.GetCurrentDisplaySpecializationAsync(
                                                                     _objMyToken),
                                                                 _objMyToken);
                    btnAddSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                              nameof(Skill.CanAffordSpecialization),
                                                              x => x
                                                                   .GetCanAffordSpecializationAsync(
                                                                       _objMyToken)
                                                                   ,
                                                              _objMyToken);
                    btnAddSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y, _objSkill,
                                                              nameof(Skill.CanHaveSpecs),
                                                              x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                    ,
                                                              _objMyToken);
                    btnAddSpec.RegisterOneWayAsyncDataBinding((x, y) => x.ToolTipText = y, _objSkill,
                                                              nameof(Skill.AddSpecToolTip),
                                                              x => x.GetAddSpecToolTipAsync(_objMyToken)
                                                                    ,
                                                              _objMyToken);

                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstAttributeItems))
                    {
                        foreach (string strLoopAttribute in AttributeSection.AttributeStrings)
                        {
                            if (strLoopAttribute == "MAGAdept")
                            {
                                if (!_objSkill.CharacterObject.Settings.MysAdeptSecondMAGAttribute)
                                    continue;
                                lstAttributeItems.Add(new ListItem(strLoopAttribute,
                                                                   LanguageManager.MAGAdeptString(token: _objMyToken)));
                            }
                            else
                            {
                                string strAttributeShort = LanguageManager.GetString(
                                    "String_Attribute" + strLoopAttribute + "Short", GlobalSettings.Language, false,
                                    _objMyToken);
                                lstAttributeItems.Add(new ListItem(strLoopAttribute,
                                                                   !string.IsNullOrEmpty(strAttributeShort)
                                                                       ? strAttributeShort
                                                                       : strLoopAttribute));
                            }
                        }

                        string strAbbrev = _objSkill.AttributeObject.Abbrev;
                        cboSelectAttribute.PopulateWithListItems(lstAttributeItems, token: _objMyToken);
                        if (!string.IsNullOrEmpty(strAbbrev))
                            cboSelectAttribute.DoThreadSafe((x, y) => x.SelectedValue = strAbbrev, token: _objMyToken);
                    }
                }
                else
                {
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Visible = y,
                                                            _objSkill.CharacterObject,
                                                            nameof(Character
                                                                       .EffectiveBuildMethodUsesPriorityTables),
                                                            x => x
                                                                 .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                     _objMyToken),
                                                            _objMyToken);
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                            nameof(Skill.BaseUnlocked),
                                                            x => x.GetBaseUnlockedAsync(_objMyToken),
                                                            _objMyToken);
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Maximum = y, _objSkill,
                        nameof(Skill.RatingMaximum),
                        x => x.GetRatingMaximumAsync(_objMyToken),
                        _objMyToken);
                    nudSkill.RegisterOneWayAsyncDataBinding((x, y) => x.Minimum = y, _objSkill,
                        nameof(Skill.FreeBase),
                        x => x.GetFreeBaseAsync(_objMyToken),
                        _objMyToken);
                    nudKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                            nameof(Skill.KarmaUnlocked),
                                                            x => x.GetKarmaUnlockedAsync(_objMyToken)
                                                                  ,
                                                            _objMyToken);
                    nudKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Maximum = y, _objSkill,
                        nameof(Skill.RatingMaximum),
                        x => x.GetRatingMaximumAsync(_objMyToken),
                        _objMyToken);
                    nudKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Minimum = y, _objSkill,
                        nameof(Skill.FreeKarma),
                        x => x.GetFreeKarmaAsync(_objMyToken),
                        _objMyToken);

                    if (_objSkill.IsExoticSkill)
                    {
                        lblCareerSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Text = y, _objSkill,
                                                                     nameof(
                                                                         Skill.CurrentDisplaySpecialization),
                                                                     x => x
                                                                          .GetCurrentDisplaySpecializationAsync(
                                                                              _objMyToken),
                                                                     _objMyToken);
                    }
                    else
                    {
                        chkKarma.RegisterOneWayAsyncDataBinding(
                            (x, y) => x.Visible = y, _objSkill.CharacterObject,
                            nameof(Character
                                       .EffectiveBuildMethodUsesPriorityTables),
                            x => x.GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                _objMyToken),
                            _objMyToken);
                        chkKarma.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                                nameof(Skill.CanHaveSpecs),
                                                                x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                      ,
                                                                _objMyToken);

                        cboSpec.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                               nameof(Skill.CanHaveSpecs),
                                                               x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                     ,
                                                               _objMyToken);
                        string strDisplaySpec = _objSkill.CurrentDisplaySpecialization;
                        Interlocked.Increment(ref _intUpdatingSpec);
                        try
                        {
                            cboSpec.PopulateWithListItems(_objSkill.CGLSpecializations, token: _objMyToken);
                            cboSpec.DoThreadSafe((x, y) => x.Text = strDisplaySpec, token: _objMyToken);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intUpdatingSpec);
                        }

                        chkKarma.RegisterAsyncDataBinding(x => x.Checked, (x, y) => x.Checked = y, _objSkill,
                            nameof(Skill.BuyWithKarma),
                            (x, y) => x.CheckedChanged += y,
                            x => x.GetBuyWithKarmaAsync(_objMyToken),
                            (x, y) => x.SetBuyWithKarmaAsync(y, _objMyToken),
                            _objMyToken,
                            _objMyToken);
                    }

                    nudKarma.RegisterAsyncDataBindingWithDelay(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _objSkill,
                        nameof(Skill.Karma),
                        (x, y) => x.ValueChanged += y,
                        x => x.GetKarmaAsync(_objMyToken),
                        (x, y) => x.SetKarmaAsync(y, _objMyToken),
                        250,
                        _objMyToken,
                        _objMyToken);
                    nudSkill.RegisterAsyncDataBindingWithDelay(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _objSkill,
                        nameof(Skill.Base),
                        (x, y) => x.ValueChanged += y,
                        x => x.GetBaseAsync(_objMyToken),
                        (x, y) => x.SetBaseAsync(y, _objMyToken),
                        250,
                        _objMyToken,
                        _objMyToken);
                }

                this.RegisterOneWayAsyncDataBinding((x, y) => x.Enabled = y, _objSkill,
                                                    nameof(Skill.Enabled),
                                                    x => x.GetEnabledAsync(_objMyToken),
                                                    _objMyToken);
                this.RegisterOneWayAsyncDataBinding((x, y) => x.BackColor = y, _objSkill,
                                                    nameof(Skill.PreferredControlColor),
                                                    x => x.GetPreferredControlColorAsync(_objMyToken)
                                                          ,
                                                    _objMyToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoDataBindingsAsync(CancellationToken token = default)
        {
            await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objSkill,
                                                                 nameof(Skill.CurrentDisplayName),
                                                                 x => x.GetCurrentDisplayNameAsync(_objMyToken)
                                                                       , token).ConfigureAwait(false);
            await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ForeColor = y, _objSkill,
                                                         nameof(Skill.PreferredColor),
                                                         x => x.GetPreferredColorAsync(_objMyToken),
                                                         token).ConfigureAwait(false);
            await lblName.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _objSkill,
                                                         nameof(Skill.SkillToolTip),
                                                         x => x.GetSkillToolTipAsync(_objMyToken),
                                                         token).ConfigureAwait(false);

            await btnAttribute.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objSkill,
                                                              nameof(Skill.DisplayAttribute),
                                                              x => x.GetDisplayAttributeAsync(_objMyToken)
                                                                    , token).ConfigureAwait(false);

            await RefreshPoolTooltipAndDisplayAsync(token).ConfigureAwait(false);

            if (await _objSkill.CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
            {
                await lblCareerRating.RegisterOneWayAsyncDataBindingAsync(
                    (x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objSkill,
                    nameof(Skill.Rating),
                    x => x.GetRatingAsync(_objMyToken),
                    token).ConfigureAwait(false);
                await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                       nameof(Skill.CanUpgradeCareer),
                                                                       x => x.GetCanUpgradeCareerAsync(
                                                                               _objMyToken)
                                                                           , token)
                                       .ConfigureAwait(false);
                await btnCareerIncrease.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _objSkill,
                                                                       nameof(Skill.UpgradeToolTip),
                                                                       x => x.GetUpgradeToolTipAsync(
                                                                               _objMyToken)
                                                                           , token)
                                       .ConfigureAwait(false);
                await lblCareerSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objSkill,
                                                                   nameof(Skill.CurrentDisplaySpecialization),
                                                                   x => x.GetCurrentDisplaySpecializationAsync(
                                                                       _objMyToken),
                                                                   token)
                                   .ConfigureAwait(false);
                await btnAddSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                nameof(Skill.CanAffordSpecialization),
                                                                x => x
                                                                     .GetCanAffordSpecializationAsync(
                                                                         _objMyToken)
                                                                     , token).ConfigureAwait(false);
                await btnAddSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y, _objSkill,
                                                                nameof(Skill.CanHaveSpecs),
                                                                x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                      , token).ConfigureAwait(false);
                await btnAddSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.ToolTipText = y, _objSkill,
                                                                nameof(Skill.AddSpecToolTip),
                                                                x => x.GetAddSpecToolTipAsync(_objMyToken)
                                                                      , token).ConfigureAwait(false);

                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstAttributeItems))
                {
                    foreach (string strLoopAttribute in AttributeSection.AttributeStrings)
                    {
                        if (strLoopAttribute == "MAGAdept")
                        {
                            if (!await _objSkill.CharacterObject.Settings.GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                                continue;
                            lstAttributeItems.Add(new ListItem(strLoopAttribute,
                                                               await LanguageManager
                                                                     .MAGAdeptStringAsync(token: token)
                                                                     .ConfigureAwait(false)));
                        }
                        else
                        {
                            string strAttributeShort = await LanguageManager.GetStringAsync(
                                "String_Attribute" + strLoopAttribute + "Short", GlobalSettings.Language, false,
                                token).ConfigureAwait(false);
                            lstAttributeItems.Add(new ListItem(strLoopAttribute,
                                                               !string.IsNullOrEmpty(strAttributeShort)
                                                                   ? strAttributeShort
                                                                   : strLoopAttribute));
                        }
                    }

                    string strAbbrev = (await _objSkill.GetAttributeObjectAsync(token).ConfigureAwait(false)).Abbrev;
                    await cboSelectAttribute.PopulateWithListItemsAsync(lstAttributeItems, token: token)
                                            .ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strAbbrev))
                        await cboSelectAttribute
                            .DoThreadSafeAsync(x => x.SelectedValue = strAbbrev, token: token)
                            .ConfigureAwait(false);
                }
            }
            else
            {
                await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Visible = y,
                                                              _objSkill.CharacterObject,
                                                              nameof(Character
                                                                         .EffectiveBuildMethodUsesPriorityTables),
                                                              x => x
                                                                   .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                       _objMyToken),
                                                              token).ConfigureAwait(false);
                await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                              nameof(Skill.BaseUnlocked),
                                                              x => x.GetBaseUnlockedAsync(_objMyToken),
                                                              token).ConfigureAwait(false);
                await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objSkill,
                    nameof(Skill.RatingMaximum),
                    x => x.GetRatingMaximumAsync(_objMyToken),
                    _objMyToken).ConfigureAwait(false);
                await nudSkill.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Minimum = y, _objSkill,
                    nameof(Skill.FreeBase),
                    x => x.GetFreeBaseAsync(_objMyToken),
                    _objMyToken).ConfigureAwait(false);
                await nudKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                              nameof(Skill.KarmaUnlocked),
                                                              x => x.GetKarmaUnlockedAsync(_objMyToken)
                                                                    , token).ConfigureAwait(false);
                await nudKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objSkill,
                    nameof(Skill.RatingMaximum),
                    x => x.GetRatingMaximumAsync(_objMyToken),
                    _objMyToken).ConfigureAwait(false);
                await nudKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Minimum = y, _objSkill,
                    nameof(Skill.FreeKarma),
                    x => x.GetFreeKarmaAsync(_objMyToken),
                    _objMyToken).ConfigureAwait(false);

                if (_objSkill.IsExoticSkill)
                {
                    await lblCareerSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objSkill,
                                                                       nameof(
                                                                           Skill.CurrentDisplaySpecialization),
                                                                       x => x
                                                                           .GetCurrentDisplaySpecializationAsync(
                                                                               _objMyToken),
                                                                       token)
                                       .ConfigureAwait(false);
                }
                else
                {
                    await chkKarma.RegisterOneWayAsyncDataBindingAsync(
                        (x, y) => x.Visible = y, _objSkill.CharacterObject,
                        nameof(Character
                                   .EffectiveBuildMethodUsesPriorityTables),
                        x => x.GetEffectiveBuildMethodUsesPriorityTablesAsync(
                            _objMyToken),
                        token).ConfigureAwait(false);
                    await chkKarma.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                  nameof(Skill.CanHaveSpecs),
                                                                  x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                      , token)
                                  .ConfigureAwait(false);

                    await cboSpec.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                                      nameof(Skill.CanHaveSpecs),
                                                                      x => x.GetCanHaveSpecsAsync(_objMyToken)
                                                                          , token)
                                 .ConfigureAwait(false);
                    string strDisplaySpec = await _objSkill.GetCurrentDisplaySpecializationAsync(_objMyToken)
                                                           .ConfigureAwait(false);
                    Interlocked.Increment(ref _intUpdatingSpec);
                    try
                    {
                        await cboSpec.PopulateWithListItemsAsync(
                                         await _objSkill.GetCGLSpecializationsAsync(_objMyToken)
                                                        .ConfigureAwait(false), token: token)
                                     .ConfigureAwait(false);
                        await cboSpec.DoThreadSafeAsync(x => x.Text = strDisplaySpec, token: token)
                                     .ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _intUpdatingSpec);
                    }

                    await chkKarma.RegisterAsyncDataBindingAsync(x => x.Checked, (x, y) => x.Checked = y, _objSkill,
                        nameof(Skill.BuyWithKarma),
                        (x, y) => x.CheckedChanged += y,
                        x => x.GetBuyWithKarmaAsync(_objMyToken),
                        (x, y) => x.SetBuyWithKarmaAsync(y, _objMyToken),
                        _objMyToken,
                        _objMyToken).ConfigureAwait(false);
                }

                await nudKarma.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _objSkill,
                    nameof(Skill.Karma),
                    (x, y) => x.ValueChanged += y,
                    x => x.GetKarmaAsync(_objMyToken),
                    (x, y) => x.SetKarmaAsync(y, _objMyToken),
                    250,
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
                await nudSkill.RegisterAsyncDataBindingWithDelayAsync(x => x.ValueAsInt, (x, y) => x.ValueAsInt = y, _objSkill,
                    nameof(Skill.Base),
                    (x, y) => x.ValueChanged += y,
                    x => x.GetBaseAsync(_objMyToken),
                    (x, y) => x.SetBaseAsync(y, _objMyToken),
                    250,
                    _objMyToken,
                    _objMyToken).ConfigureAwait(false);
            }

            await this.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objSkill,
                                                      nameof(Skill.Enabled),
                                                      x => x.GetEnabledAsync(_objMyToken),
                                                      token).ConfigureAwait(false);
            await this.RegisterOneWayAsyncDataBindingAsync((x, y) => x.BackColor = y, _objSkill,
                                                      nameof(Skill.PreferredControlColor),
                                                      x => x.GetPreferredControlColorAsync(_objMyToken)
                                                            , token).ConfigureAwait(false);
        }

        private int _intLoaded;

        public async Task DoLoad(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (Interlocked.CompareExchange(ref _intLoaded, 1, 0) > 0)
                return;
            IAsyncDisposable objLocker = await _objSkill.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await DoDataBindingsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            Interlocked.Decrement(ref _intUpdatingSpec);
        }

        private async void SkillControl_Load(object sender, EventArgs e)
        {
            try
            {
                await DoLoad(_objMyToken).ConfigureAwait(false);
                await this.DoThreadSafeAsync(x => x.AdjustForDpi(), token: _objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task Skill_PropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            bool blnAll = e.PropertyNames == default;
            //I learned something from this but i'm not sure it is a good solution
            //scratch that, i'm sure it is a bad solution. (Tooltip manager from tooltip, properties from reflection?
            //if name of changed is null it does magic to change all, otherwise it only does one.
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnAll || e.PropertyNames.Contains(nameof(Skill.DisplayOtherAttribute)))
                {
                    await RefreshPoolTooltipAndDisplayAsync(token).ConfigureAwait(false);
                }
                if (blnAll || e.PropertyNames.Contains(nameof(Skill.Default)))
                {
                    bool blnDefault = await _objSkill.GetDefaultAsync(token).ConfigureAwait(false);
                    await lblName
                        .DoThreadSafeAsync(x => x.Font = !blnDefault ? _fntItalicName : _fntNormalName,
                            token: token).ConfigureAwait(false);
                }
                if (blnAll || e.PropertyNames.Contains(nameof(Skill.DefaultAttribute)))
                {
                    if (cboSelectAttribute != null)
                    {
                        string strAbbrev = (await _objSkill.GetAttributeObjectAsync(token).ConfigureAwait(false)).Abbrev;
                        if (!string.IsNullOrEmpty(strAbbrev))
                            await cboSelectAttribute
                            .DoThreadSafeAsync(x => x.SelectedValue = strAbbrev,
                                token: token).ConfigureAwait(false);
                        await DoSelectAttributeClosed(token).ConfigureAwait(false);
                    }
                    else
                    {
                        await SetAttributeActiveAsync(await _objSkill.GetAttributeObjectAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }
                }

                if ((blnAll || e.PropertyNames.Contains(nameof(Skill.CanHaveSpecs))) && _fntNormalSpec != null)
                {
                    bool blnCanHaveSpecs = await _objSkill.GetCanHaveSpecsAsync(token).ConfigureAwait(false);
                    await lblCareerSpec
                        .DoThreadSafeAsync(x => x.Font = blnCanHaveSpecs ? _fntNormalSpec : _fntStrikethroughSpec,
                            token).ConfigureAwait(false);
                }
                if (cboSpec != null && await cboSpec.DoThreadSafeFuncAsync(x => x.Visible, token: token)
                        .ConfigureAwait(false))
                {
                    if (blnAll || (e.PropertyNames.Contains(nameof(Skill.CGLSpecializations)) &&
                                   e.PropertyNames.Contains(nameof(Skill.TopMostDisplaySpecialization))))
                    {
                        IReadOnlyList<ListItem> lstSpecializations
                            = await _objSkill.GetCGLSpecializationsAsync(token).ConfigureAwait(false);
                        string strOldSpec = lstSpecializations.Count != 0
                            ? await cboSpec
                                .DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token: token)
                                .ConfigureAwait(false)
                            : await cboSpec.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                .ConfigureAwait(false);
                        int intOldUpdating = Interlocked.Increment(ref _intUpdatingSpec);
                        try
                        {
                            if (intOldUpdating == 0)
                            {
                                string strDisplaySpec = await _objSkill
                                    .GetTopMostDisplaySpecializationAsync(token)
                                    .ConfigureAwait(false);
                                await cboSpec.PopulateWithListItemsAsync(lstSpecializations, token: token)
                                    .ConfigureAwait(false);
                                await cboSpec.DoThreadSafeAsync(x =>
                                {
                                    if (string.IsNullOrEmpty(strOldSpec))
                                        x.SelectedIndex = -1;
                                    else
                                    {
                                        x.SelectedValue = strOldSpec;
                                        if (x.SelectedIndex == -1)
                                            x.Text = strOldSpec;
                                    }

                                    x.Text = strDisplaySpec;
                                }, token: token).ConfigureAwait(false);
                            }
                            else
                            {
                                await cboSpec.PopulateWithListItemsAsync(lstSpecializations, token: token)
                                    .ConfigureAwait(false);
                                await cboSpec.DoThreadSafeAsync(x =>
                                {
                                    if (string.IsNullOrEmpty(strOldSpec))
                                        x.SelectedIndex = -1;
                                    else
                                    {
                                        x.SelectedValue = strOldSpec;
                                        if (x.SelectedIndex == -1)
                                            x.Text = strOldSpec;
                                    }
                                }, token: token).ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intUpdatingSpec);
                        }
                    }
                    else
                    {
                        if (e.PropertyNames.Contains(nameof(Skill.CGLSpecializations)))
                        {
                            IReadOnlyList<ListItem> lstSpecializations
                                = await _objSkill.GetCGLSpecializationsAsync(token).ConfigureAwait(false);
                            string strOldSpec = lstSpecializations.Count != 0
                                ? await cboSpec
                                    .DoThreadSafeFuncAsync(x => x.SelectedItem?.ToString(), token: token)
                                    .ConfigureAwait(false)
                                : await cboSpec.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                    .ConfigureAwait(false);
                            Interlocked.Increment(ref _intUpdatingSpec);
                            try
                            {
                                await cboSpec.PopulateWithListItemsAsync(lstSpecializations, token: token)
                                    .ConfigureAwait(false);
                                await cboSpec.DoThreadSafeAsync(x =>
                                {
                                    if (string.IsNullOrEmpty(strOldSpec))
                                        x.SelectedIndex = -1;
                                    else
                                    {
                                        x.SelectedValue = strOldSpec;
                                        if (x.SelectedIndex == -1)
                                            x.Text = strOldSpec;
                                    }
                                }, token: token).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intUpdatingSpec);
                            }
                        }

                        if (e.PropertyNames.Contains(nameof(KnowledgeSkill.TopMostDisplaySpecialization)))
                        {
                            int intOldUpdating = Interlocked.Increment(ref _intUpdatingSpec);
                            try
                            {
                                if (intOldUpdating == 0)
                                {
                                    string strDisplaySpec = await _objSkill
                                        .GetTopMostDisplaySpecializationAsync(token)
                                        .ConfigureAwait(false);
                                    await cboSpec.DoThreadSafeAsync(x => x.Text = strDisplaySpec, token: token)
                                        .ConfigureAwait(false);
                                }
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intUpdatingSpec);
                            }
                        }
                    }
                }
                if ((blnAll || e.PropertyNames.Contains(nameof(KnowledgeSkill.Specializations)))
                    && await Program
                        .GetFormForDialogAsync(_objSkill.CharacterObject, token)
                        .ConfigureAwait(false) is CharacterShared frmParent)
                {
                    frmParent.RequestCharacterUpdate(token);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task Attribute_PropertyChanged(object sender,
            MultiplePropertiesChangedEventArgs propertyChangedEventArgs, CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            try
            {
                token.ThrowIfCancellationRequested();
                if (propertyChangedEventArgs.PropertyNames.Contains(nameof(CharacterAttrib.Abbrev))
                    || propertyChangedEventArgs.PropertyNames.Contains(nameof(CharacterAttrib.TotalValue)))
                {
                    await RefreshPoolTooltipAndDisplayAsync(token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void btnCareerIncrease_Click(object sender, EventArgs e)
        {
            try
            {
                IAsyncDisposable objLocker = await _objSkill.LockObject.EnterUpgradeableReadLockAsync(_objMyToken).ConfigureAwait(false);
                try
                {
                    _objMyToken.ThrowIfCancellationRequested();
                    string strConfirm = string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync(
                                "Message_ConfirmKarmaExpense",
                                token: _objMyToken)
                            .ConfigureAwait(false),
                        await _objSkill.GetCurrentDisplayNameAsync(_objMyToken)
                            .ConfigureAwait(false),
                        await _objSkill.GetRatingAsync(_objMyToken).ConfigureAwait(false)
                        + 1,
                        await _objSkill.GetUpgradeKarmaCostAsync(_objMyToken)
                            .ConfigureAwait(false));

                    if (!await CommonFunctions.ConfirmKarmaExpenseAsync(strConfirm, _objMyToken).ConfigureAwait(false))
                        return;

                    await _objSkill.Upgrade(_objMyToken).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void btnAddSpec_Click(object sender, EventArgs e)
        {
            try
            {
                IAsyncDisposable objLocker = await _objSkill.LockObject.EnterUpgradeableReadLockAsync(_objMyToken).ConfigureAwait(false);
                try
                {
                    _objMyToken.ThrowIfCancellationRequested();
                    Character objCharacter = _objSkill.CharacterObject;
                    int intPrice = await (await objCharacter.GetSettingsAsync(_objMyToken).ConfigureAwait(false))
                        .GetKarmaKnowledgeSpecializationAsync(_objMyToken).ConfigureAwait(false);

                    int intTotalBaseRating = await _objSkill.GetTotalBaseRatingAsync(_objMyToken).ConfigureAwait(false);
                    decimal decSpecCostMultiplier = 1.0m;
                    bool blnCreated
                        = await objCharacter.GetCreatedAsync(_objMyToken).ConfigureAwait(false);
                    decimal decExtraSpecCost = objCharacter.Improvements.Sum(objLoopImprovement =>
                        objLoopImprovement.Minimum <= intTotalBaseRating
                        && (string.IsNullOrEmpty(objLoopImprovement.Condition)
                            || (objLoopImprovement.Condition == "career") == blnCreated
                            || (objLoopImprovement.Condition == "create") != blnCreated)
                        && objLoopImprovement.Enabled
                        && objLoopImprovement.ImprovedName == _objSkill.SkillCategory, objLoopImprovement =>
                    {
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                return objLoopImprovement.Value;

                            case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                break;
                        }

                        return 0;
                    }, token: _objMyToken);

                    if (decSpecCostMultiplier != 1.0m)
                        intPrice = (intPrice * decSpecCostMultiplier + decExtraSpecCost).StandardRound();
                    else
                        intPrice += decExtraSpecCost.StandardRound(); //Spec

                    string strConfirm = string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager
                            .GetStringAsync(
                                "Message_ConfirmKarmaExpenseSkillSpecialization",
                                token: _objMyToken).ConfigureAwait(false), intPrice);

                    if (!await CommonFunctions.ConfirmKarmaExpenseAsync(strConfirm, _objMyToken)
                            .ConfigureAwait(false))
                        return;

                    using (ThreadSafeForm<SelectSpec> selectForm =
                           await ThreadSafeForm<SelectSpec>.GetAsync(() => new SelectSpec(_objSkill), _objMyToken)
                               .ConfigureAwait(false))
                    {
                        if (await selectForm.ShowDialogSafeAsync(objCharacter, _objMyToken)
                                .ConfigureAwait(false) != DialogResult.OK)
                            return;
                        await _objSkill.AddSpecialization(selectForm.MyForm.SelectedItem, _objMyToken)
                            .ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void btnAttribute_Click(object sender, EventArgs e)
        {
            if (cboSelectAttribute != null)
            {
                btnAttribute.Visible = false;
                cboSelectAttribute.Visible = true;
                cboSelectAttribute.DroppedDown = true;
            }
        }

        private async void cboSelectAttribute_Closed(object sender, EventArgs e)
        {
            try
            {
                await DoSelectAttributeClosed(_objMyToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoSelectAttributeClosed(CancellationToken token = default)
        {
            await btnAttribute.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
            await cboSelectAttribute.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
            await SetAttributeActiveAsync(
                await _objSkill.CharacterObject.GetAttributeAsync(
                    await cboSelectAttribute.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token)
                                             .ConfigureAwait(false),
                    token: token).ConfigureAwait(false), token).ConfigureAwait(false);
            string strText = await cboSelectAttribute.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            await btnAttribute.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);
        }

        private CharacterAttrib AttributeActive => _objAttributeActive;

        private async Task SetAttributeActiveAsync(CharacterAttrib value, CancellationToken token = default)
        {
            CharacterAttrib objOldAttrib = Interlocked.Exchange(ref _objAttributeActive, value);
            if (objOldAttrib == value)
                return;
            if (objOldAttrib != null)
            {
                try
                {
                    objOldAttrib.MultiplePropertiesChangedAsync -= Attribute_PropertyChanged;
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
            }
            if (value != null)
                value.MultiplePropertiesChangedAsync += Attribute_PropertyChanged;

            Font objFont = ReferenceEquals(value, await _objSkill.GetAttributeObjectAsync(token).ConfigureAwait(false)) ? _fntNormal : _fntItalic;
            await btnAttribute.DoThreadSafeAsync(x => x.Font = objFont, token).ConfigureAwait(false);
            await RefreshPoolTooltipAndDisplayAsync(token).ConfigureAwait(false);
            if (CustomAttributeChanged != null)
                await CustomAttributeChanged.Invoke(this, EventArgs.Empty, token).ConfigureAwait(false);
        }

        public event EventHandlerExtensions.SafeAsyncEventHandler CustomAttributeChanged;

        public async Task<bool> GetCustomAttributeSet(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return !ReferenceEquals(AttributeActive, await _objSkill.GetAttributeObjectAsync(token).ConfigureAwait(false));
        }

        [UsedImplicitly]
        public int NameWidth => lblName.PreferredWidth + lblName.Margin.Right + pnlAttributes.Margin.Left + pnlAttributes.Width;

        [UsedImplicitly]
        public int NudSkillWidth =>
            !_objSkill.CharacterObject.Created && _objSkill.CharacterObject.EffectiveBuildMethodUsesPriorityTables
                ? nudSkill.Width
                : 0;

        [UsedImplicitly]
        public async Task ResetSelectAttribute(CancellationToken token = default)
        {
            if (cboSelectAttribute == null)
                return;
            if (!await GetCustomAttributeSet(token).ConfigureAwait(false))
                return;
            IAsyncDisposable objLocker = await _objSkill.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetCustomAttributeSet(token).ConfigureAwait(false))
                    return;
                CharacterAttrib objOldAttribute = await _objSkill.GetAttributeObjectAsync(token).ConfigureAwait(false);
                string strNewAttribute = await cboSelectAttribute.DoThreadSafeFuncAsync(x =>
                {
                    x.SelectedValue = objOldAttribute.Abbrev;
                    x.Visible = false;
                    return x.SelectedValue?.ToString() ?? string.Empty;
                }, token: token).ConfigureAwait(false);
                await SetAttributeActiveAsync(
                    await _objSkill.CharacterObject.GetAttributeAsync(strNewAttribute, token: token)
                        .ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            string strText = await cboSelectAttribute.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            await btnAttribute.DoThreadSafeAsync(x =>
            {
                x.Visible = true;
                x.Text = strText;
            }, token: token).ConfigureAwait(false);
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!await _objSkill.GetAllowDeleteAsync(_objMyToken).ConfigureAwait(false))
                    return;
                if (!await CommonFunctions
                           .ConfirmDeleteAsync(
                               await LanguageManager
                                     .GetStringAsync(
                                         _objSkill.IsExoticSkill ? "Message_DeleteExoticSkill" : "Message_DeleteSkill",
                                         token: _objMyToken).ConfigureAwait(false), _objMyToken).ConfigureAwait(false))
                    return;
                await _objSkill.CharacterObject.SkillsSection.Skills.RemoveAsync(_objSkill, _objMyToken)
                               .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void tsSkillLabelNotes_Click(object sender, EventArgs e)
        {
            try
            {
                string strNotes = await _objSkill.GetNotesAsync(_objMyToken).ConfigureAwait(false);
                Color objColor = await _objSkill.GetNotesColorAsync(_objMyToken).ConfigureAwait(false);
                using (ThreadSafeForm<EditNotes> frmItemNotes = await ThreadSafeForm<EditNotes>
                                                                      .GetAsync(
                                                                          () => new EditNotes(
                                                                              strNotes, objColor, _objMyToken),
                                                                          _objMyToken).ConfigureAwait(false))
                {
                    if (await frmItemNotes.ShowDialogSafeAsync(_objSkill.CharacterObject, _objMyToken)
                                          .ConfigureAwait(false) != DialogResult.OK)
                        return;
                    await _objSkill.SetNotesAsync(frmItemNotes.MyForm.Notes, _objMyToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void lblName_Click(object sender, EventArgs e)
        {
            try
            {
                CursorWait objCursorWait
                    = await CursorWait.NewAsync(ParentForm, token: _objMyToken).ConfigureAwait(false);
                try
                {
                    Character objCharacter = _objSkill.CharacterObject;
                    await CommonFunctions.OpenPdf(
                        _objSkill.Source + ' ' + await _objSkill.DisplayPageAsync(GlobalSettings.Language, _objMyToken)
                                                                .ConfigureAwait(false),
                        objCharacter != null
                            ? await objCharacter.GetSettingsAsync(_objMyToken).ConfigureAwait(false)
                            : null, token: _objMyToken).ConfigureAwait(false);
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        [UsedImplicitly]
        public void MoveControls(int intNewNameWidth)
        {
            try
            {
                lblName.MinimumSize =
                        new Size(
                            intNewNameWidth - lblName.Margin.Right - pnlAttributes.Margin.Left - pnlAttributes.Width,
                            lblName.MinimumSize.Height);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void UnbindSkillControl()
        {
            _tmrSpecChangeTimer?.Dispose();
            try
            {
                _objSkill.MultiplePropertiesChangedAsync -= Skill_PropertyChanged;
            }
            catch (ObjectDisposedException)
            {
                //swallow this
            }
            if (AttributeActive != null)
            {
                try
                {
                    AttributeActive.MultiplePropertiesChangedAsync -= Attribute_PropertyChanged;
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
            }

            foreach (Control objControl in Controls)
            {
                objControl.DataBindings.Clear();
            }
        }

        /// <summary>
        /// I'm not super pleased with how this works, but it's functional so w/e.
        /// The goal is for controls to retain the ability to display tooltips even while disabled. IT DOES NOT WORK VERY WELL.
        /// </summary>
        #region ButtonWithToolTip Visibility workaround

        private ButtonWithToolTip _activeButton;

        private ButtonWithToolTip ActiveButton
        {
            get => _activeButton;
            set
            {
                ButtonWithToolTip objOldValue = Interlocked.Exchange(ref _activeButton, value);
                if (objOldValue == value)
                    return;
                objOldValue?.ToolTipObject.Hide(this);
                if (value?.Visible == true)
                {
                    value.ToolTipObject.Show(value.ToolTipText, this);
                }
            }
        }

        private ButtonWithToolTip FindToolTipControl(Point pt)
        {
            return Controls.OfType<ButtonWithToolTip>().FirstOrDefault(c => c.ClientRectangle.Contains(pt));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ActiveButton = FindToolTipControl(e.Location);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ActiveButton = null;
        }

        #endregion ButtonWithToolTip Visibility workaround

        private void SkillControl_DpiChangedAfterParent(object sender, EventArgs e)
        {
            AdjustForDpi();
        }

        private void AdjustForDpi()
        {
            using (Graphics g = CreateGraphics())
            {
                pnlAttributes.MinimumSize = new Size((int)(40 * g.DpiX / 96.0f), 0);
                if (lblCareerRating != null)
                    lblCareerRating.MinimumSize = new Size((int)(25 * g.DpiX / 96.0f), 0);
                lblModifiedRating.MinimumSize = new Size((int)(50 * g.DpiX / 96.0f), 0);
            }
        }

        /// <summary>
        /// Refreshes the Tooltip and Displayed Dice Pool. Can be used in another Thread
        /// </summary>
        private void RefreshPoolTooltipAndDisplay(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string backgroundCalcPool = _objSkill.DisplayOtherAttribute(AttributeActive.Abbrev);
            string backgroundCalcTooltip = _objSkill.CompileDicepoolTooltip(AttributeActive.Abbrev);
            lblModifiedRating.DoThreadSafe((x, y) =>
            {
                x.Text = backgroundCalcPool;
                x.ToolTipText = backgroundCalcTooltip;
            }, token);
        }

        /// <summary>
        /// Refreshes the Tooltip and Displayed Dice Pool. Can be used in another Thread
        /// </summary>
        private async Task RefreshPoolTooltipAndDisplayAsync(CancellationToken token = default)
        {
            string backgroundCalcPool = await _objSkill.DisplayOtherAttributeAsync(AttributeActive.Abbrev, token).ConfigureAwait(false);
            string backgroundCalcTooltip = await _objSkill.CompileDicepoolTooltipAsync(AttributeActive.Abbrev, token: token).ConfigureAwait(false);
            await lblModifiedRating.DoThreadSafeAsync(x => x.Text = backgroundCalcPool, token: token).ConfigureAwait(false);
            await lblModifiedRating.SetToolTipTextAsync(backgroundCalcTooltip, token).ConfigureAwait(false);
        }

        // Hacky solutions to data binding causing cursor to reset whenever the user is typing something in: have text changes start a timer, and have a 1s delay in the timer update fire the text update
        private void cboSpec_TextChanged(object sender, EventArgs e)
        {
            if (_tmrSpecChangeTimer == null)
                return;
            if (_tmrSpecChangeTimer.Enabled)
                _tmrSpecChangeTimer.Stop();
            if (_intUpdatingSpec > 0)
                return;
            _tmrSpecChangeTimer.Start();
        }

        private async void SpecChangeTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _tmrSpecChangeTimer.Stop();
                Interlocked.Increment(ref _intUpdatingSpec);
                try
                {
                    string strSpec = await cboSpec.DoThreadSafeFuncAsync(x => x.Text, token: _objMyToken)
                                                  .ConfigureAwait(false);
                    await _objSkill.SetTopMostDisplaySpecializationAsync(strSpec, _objMyToken).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intUpdatingSpec);
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }
    }
}
