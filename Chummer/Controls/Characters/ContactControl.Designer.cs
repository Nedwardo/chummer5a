namespace Chummer
{
    partial class ContactControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnbindContactControl();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmsContact = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsContactOpen = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsRemoveCharacter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsAttachCharacter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.cboContactRole = new Chummer.ElasticComboBox();
            this.txtContactName = new System.Windows.Forms.TextBox();
            this.txtContactLocation = new System.Windows.Forms.TextBox();
            this.cmdExpand = new Chummer.DpiFriendlyImagedButton();
            this.lblQuickStats = new System.Windows.Forms.Label();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.cmdDelete = new Chummer.DpiFriendlyImagedButton();
            this.tlpComboBoxes = new System.Windows.Forms.TableLayoutPanel();
            this.lblContactArchtypeLabel = new System.Windows.Forms.Label();
            this.lblContactLocationLabel = new System.Windows.Forms.Label();
            this.lblContactNameLabel = new System.Windows.Forms.Label();
            this.cmdNotes = new Chummer.ButtonWithToolTip();
            this.cmsContact.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.tlpComboBoxes.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmsContact
            // 
            this.cmsContact.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsContactOpen,
            this.tsRemoveCharacter,
            this.tsAttachCharacter});
            this.cmsContact.Name = "cmsContact";
            this.cmsContact.Size = new System.Drawing.Size(172, 70);
            // 
            // tsContactOpen
            // 
            this.tsContactOpen.Image = global::Chummer.Properties.Resources.link_go_16;
            this.tsContactOpen.ImageDpi120 = global::Chummer.Properties.Resources.link_go_20;
            this.tsContactOpen.ImageDpi144 = global::Chummer.Properties.Resources.link_go_24;
            this.tsContactOpen.ImageDpi192 = global::Chummer.Properties.Resources.link_go_32;
            this.tsContactOpen.ImageDpi288 = global::Chummer.Properties.Resources.link_go_48;
            this.tsContactOpen.ImageDpi384 = global::Chummer.Properties.Resources.link_go_64;
            this.tsContactOpen.ImageDpi96 = global::Chummer.Properties.Resources.link_go_16;
            this.tsContactOpen.Name = "tsContactOpen";
            this.tsContactOpen.Size = new System.Drawing.Size(171, 22);
            this.tsContactOpen.Tag = "MenuItem_OpenCharacter";
            this.tsContactOpen.Text = "Open Character";
            this.tsContactOpen.Click += new System.EventHandler(this.tsContactOpen_Click);
            // 
            // tsRemoveCharacter
            // 
            this.tsRemoveCharacter.Image = global::Chummer.Properties.Resources.link_delete_16;
            this.tsRemoveCharacter.ImageDpi120 = global::Chummer.Properties.Resources.link_delete_20;
            this.tsRemoveCharacter.ImageDpi144 = global::Chummer.Properties.Resources.link_delete_24;
            this.tsRemoveCharacter.ImageDpi192 = global::Chummer.Properties.Resources.link_delete_32;
            this.tsRemoveCharacter.ImageDpi288 = global::Chummer.Properties.Resources.link_delete_48;
            this.tsRemoveCharacter.ImageDpi384 = global::Chummer.Properties.Resources.link_delete_64;
            this.tsRemoveCharacter.ImageDpi96 = global::Chummer.Properties.Resources.link_delete_16;
            this.tsRemoveCharacter.Name = "tsRemoveCharacter";
            this.tsRemoveCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsRemoveCharacter.Tag = "MenuItem_RemoveCharacter";
            this.tsRemoveCharacter.Text = "Remove Character";
            this.tsRemoveCharacter.Click += new System.EventHandler(this.tsRemoveCharacter_Click);
            // 
            // tsAttachCharacter
            // 
            this.tsAttachCharacter.Image = global::Chummer.Properties.Resources.link_add_16;
            this.tsAttachCharacter.ImageDpi120 = global::Chummer.Properties.Resources.link_add_20;
            this.tsAttachCharacter.ImageDpi144 = global::Chummer.Properties.Resources.link_add_24;
            this.tsAttachCharacter.ImageDpi192 = global::Chummer.Properties.Resources.link_add_32;
            this.tsAttachCharacter.ImageDpi288 = global::Chummer.Properties.Resources.link_add_48;
            this.tsAttachCharacter.ImageDpi384 = global::Chummer.Properties.Resources.link_add_64;
            this.tsAttachCharacter.ImageDpi96 = global::Chummer.Properties.Resources.link_add_16;
            this.tsAttachCharacter.Name = "tsAttachCharacter";
            this.tsAttachCharacter.Size = new System.Drawing.Size(171, 22);
            this.tsAttachCharacter.Tag = "MenuItem_AttachCharacter";
            this.tsAttachCharacter.Text = "Attach Character";
            this.tsAttachCharacter.Click += new System.EventHandler(this.tsAttachCharacter_Click);
            // 
            // cboContactRole
            // 
            this.cboContactRole.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboContactRole.FormattingEnabled = true;
            this.cboContactRole.Location = new System.Drawing.Point(366, 28);
            this.cboContactRole.Name = "cboContactRole";
            this.cboContactRole.Size = new System.Drawing.Size(177, 21);
            this.cboContactRole.TabIndex = 2;
            this.cboContactRole.TooltipText = "";
            this.cboContactRole.TextChanged += new System.EventHandler(this.cboContactRole_TextChanged);
            // 
            // txtContactName
            // 
            this.txtContactName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactName.Location = new System.Drawing.Point(3, 28);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(175, 20);
            this.txtContactName.TabIndex = 0;
            // 
            // txtContactLocation
            // 
            this.txtContactLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpComboBoxes.SetColumnSpan(this.txtContactLocation, 2);
            this.txtContactLocation.Location = new System.Drawing.Point(184, 28);
            this.txtContactLocation.Name = "txtContactLocation";
            this.txtContactLocation.Size = new System.Drawing.Size(176, 20);
            this.txtContactLocation.TabIndex = 1;
            // 
            // cmdExpand
            // 
            this.cmdExpand.AutoSize = true;
            this.cmdExpand.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdExpand.Image = global::Chummer.Properties.Resources.toggle_expand_16;
            this.cmdExpand.ImageDpi120 = global::Chummer.Properties.Resources.toggle_expand_20;
            this.cmdExpand.ImageDpi144 = global::Chummer.Properties.Resources.toggle_expand_24;
            this.cmdExpand.ImageDpi192 = global::Chummer.Properties.Resources.toggle_expand_32;
            this.cmdExpand.ImageDpi288 = global::Chummer.Properties.Resources.toggle_expand_48;
            this.cmdExpand.ImageDpi384 = global::Chummer.Properties.Resources.toggle_expand_64;
            this.cmdExpand.ImageDpi96 = global::Chummer.Properties.Resources.toggle_expand_16;
            this.cmdExpand.Location = new System.Drawing.Point(3, 3);
            this.cmdExpand.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdExpand.Name = "cmdExpand";
            this.cmdExpand.Padding = new System.Windows.Forms.Padding(1);
            this.cmdExpand.Size = new System.Drawing.Size(24, 24);
            this.cmdExpand.TabIndex = 11;
            this.cmdExpand.UseVisualStyleBackColor = true;
            this.cmdExpand.Click += new System.EventHandler(this.cmdExpand_Click);
            // 
            // lblQuickStats
            // 
            this.lblQuickStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQuickStats.AutoSize = true;
            this.lblQuickStats.Location = new System.Drawing.Point(579, 19);
            this.lblQuickStats.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblQuickStats.MinimumSize = new System.Drawing.Size(40, 0);
            this.lblQuickStats.Name = "lblQuickStats";
            this.tlpMain.SetRowSpan(this.lblQuickStats, 2);
            this.lblQuickStats.Size = new System.Drawing.Size(40, 13);
            this.lblQuickStats.TabIndex = 14;
            this.lblQuickStats.Text = "(00/0)";
            this.lblQuickStats.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 13;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.cmdDelete, 12, 0);
            this.tlpMain.Controls.Add(this.lblQuickStats, 10, 0);
            this.tlpMain.Controls.Add(this.cmdExpand, 0, 0);
            this.tlpMain.Controls.Add(this.tlpComboBoxes, 1, 0);
            this.tlpMain.Controls.Add(this.cmdNotes, 11, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 4;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(682, 52);
            this.tlpMain.TabIndex = 35;
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Image = global::Chummer.Properties.Resources.delete_16;
            this.cmdDelete.ImageDpi120 = global::Chummer.Properties.Resources.delete_20;
            this.cmdDelete.ImageDpi144 = global::Chummer.Properties.Resources.delete_24;
            this.cmdDelete.ImageDpi192 = global::Chummer.Properties.Resources.delete_32;
            this.cmdDelete.ImageDpi288 = global::Chummer.Properties.Resources.delete_48;
            this.cmdDelete.ImageDpi384 = global::Chummer.Properties.Resources.delete_64;
            this.cmdDelete.ImageDpi96 = global::Chummer.Properties.Resources.delete_16;
            this.cmdDelete.Location = new System.Drawing.Point(655, 3);
            this.cmdDelete.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Padding = new System.Windows.Forms.Padding(1);
            this.cmdDelete.Size = new System.Drawing.Size(24, 24);
            this.cmdDelete.TabIndex = 7;
            this.cmdDelete.Tag = "";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // tlpComboBoxes
            // 
            this.tlpComboBoxes.AutoSize = true;
            this.tlpComboBoxes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpComboBoxes.ColumnCount = 4;
            this.tlpMain.SetColumnSpan(this.tlpComboBoxes, 9);
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33332F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpComboBoxes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tlpComboBoxes.Controls.Add(this.lblContactArchtypeLabel, 3, 0);
            this.tlpComboBoxes.Controls.Add(this.lblContactLocationLabel, 1, 0);
            this.tlpComboBoxes.Controls.Add(this.lblContactNameLabel, 0, 0);
            this.tlpComboBoxes.Controls.Add(this.txtContactName, 0, 1);
            this.tlpComboBoxes.Controls.Add(this.txtContactLocation, 1, 1);
            this.tlpComboBoxes.Controls.Add(this.cboContactRole, 3, 1);
            this.tlpComboBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpComboBoxes.Location = new System.Drawing.Point(30, 0);
            this.tlpComboBoxes.Margin = new System.Windows.Forms.Padding(0);
            this.tlpComboBoxes.Name = "tlpComboBoxes";
            this.tlpComboBoxes.RowCount = 2;
            this.tlpMain.SetRowSpan(this.tlpComboBoxes, 2);
            this.tlpComboBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpComboBoxes.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpComboBoxes.Size = new System.Drawing.Size(546, 52);
            this.tlpComboBoxes.TabIndex = 35;
            // 
            // lblContactArchtypeLabel
            // 
            this.lblContactArchtypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactArchtypeLabel.AutoSize = true;
            this.lblContactArchtypeLabel.Location = new System.Drawing.Point(366, 6);
            this.lblContactArchtypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactArchtypeLabel.Name = "lblContactArchtypeLabel";
            this.lblContactArchtypeLabel.Size = new System.Drawing.Size(52, 13);
            this.lblContactArchtypeLabel.TabIndex = 45;
            this.lblContactArchtypeLabel.Tag = "Label_Archetype";
            this.lblContactArchtypeLabel.Text = "Archtype:";
            // 
            // lblContactLocationLabel
            // 
            this.lblContactLocationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactLocationLabel.AutoSize = true;
            this.tlpComboBoxes.SetColumnSpan(this.lblContactLocationLabel, 2);
            this.lblContactLocationLabel.Location = new System.Drawing.Point(184, 6);
            this.lblContactLocationLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactLocationLabel.Name = "lblContactLocationLabel";
            this.lblContactLocationLabel.Size = new System.Drawing.Size(51, 13);
            this.lblContactLocationLabel.TabIndex = 44;
            this.lblContactLocationLabel.Tag = "Label_Location";
            this.lblContactLocationLabel.Text = "Location:";
            // 
            // lblContactNameLabel
            // 
            this.lblContactNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblContactNameLabel.AutoSize = true;
            this.lblContactNameLabel.Location = new System.Drawing.Point(3, 6);
            this.lblContactNameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblContactNameLabel.Name = "lblContactNameLabel";
            this.lblContactNameLabel.Size = new System.Drawing.Size(38, 13);
            this.lblContactNameLabel.TabIndex = 43;
            this.lblContactNameLabel.Tag = "Label_Name";
            this.lblContactNameLabel.Text = "Name:";
            // 
            // cmdNotes
            // 
            this.cmdNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdNotes.AutoSize = true;
            this.cmdNotes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdNotes.FlatAppearance.BorderSize = 0;
            this.cmdNotes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdNotes.Image = global::Chummer.Properties.Resources.note_edit_16;
            this.cmdNotes.ImageDpi120 = global::Chummer.Properties.Resources.note_edit_20;
            this.cmdNotes.ImageDpi144 = global::Chummer.Properties.Resources.note_edit_24;
            this.cmdNotes.ImageDpi192 = global::Chummer.Properties.Resources.note_edit_32;
            this.cmdNotes.ImageDpi288 = global::Chummer.Properties.Resources.note_edit_48;
            this.cmdNotes.ImageDpi384 = global::Chummer.Properties.Resources.note_edit_64;
            this.cmdNotes.ImageDpi96 = global::Chummer.Properties.Resources.note_edit_16;
            this.cmdNotes.Location = new System.Drawing.Point(625, 3);
            this.cmdNotes.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Padding = new System.Windows.Forms.Padding(1);
            this.cmdNotes.Size = new System.Drawing.Size(24, 24);
            this.cmdNotes.TabIndex = 36;
            this.cmdNotes.ToolTipText = "";
            this.cmdNotes.UseVisualStyleBackColor = true;
            this.cmdNotes.Click += new System.EventHandler(this.cmdNotes_Click);
            // 
            // ContactControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(480, 0);
            this.Name = "ContactControl";
            this.Size = new System.Drawing.Size(682, 52);
            this.Load += new System.EventHandler(this.ContactControl_Load);
            this.cmsContact.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.tlpComboBoxes.ResumeLayout(false);
            this.tlpComboBoxes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip cmsContact;
        private ElasticComboBox cboContactRole;
        private System.Windows.Forms.TextBox txtContactName;
        private System.Windows.Forms.TextBox txtContactLocation;
        private DpiFriendlyImagedButton cmdExpand;
        private System.Windows.Forms.Label lblQuickStats;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tlpComboBoxes;
        private System.Windows.Forms.Label lblContactNameLabel;
        private System.Windows.Forms.Label lblContactLocationLabel;
        private System.Windows.Forms.Label lblContactArchtypeLabel;
        private DpiFriendlyImagedButton cmdDelete;
        private DpiFriendlyToolStripMenuItem tsContactOpen;
        private DpiFriendlyToolStripMenuItem tsRemoveCharacter;
        private DpiFriendlyToolStripMenuItem tsAttachCharacter;
        private ButtonWithToolTip cmdNotes;
    }
}
