namespace Chummer
{
    partial class PetControl
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
                UnbindPetControl();
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
            this.txtContactName = new System.Windows.Forms.TextBox();
            this.cmsContact = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsContactOpen = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsRemoveCharacter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.tsAttachCharacter = new Chummer.DpiFriendlyToolStripMenuItem();
            this.lblName = new System.Windows.Forms.Label();
            this.lblMetatypeLabel = new System.Windows.Forms.Label();
            this.cboMetatype = new Chummer.ElasticComboBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.cmdNotes = new Chummer.ButtonWithToolTip();
            this.cmdLink = new Chummer.ButtonWithToolTip();
            this.cmdDelete = new Chummer.DpiFriendlyImagedButton();
            this.cmsContact.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtContactName
            // 
            this.txtContactName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContactName.BackColor = System.Drawing.SystemColors.Window;
            this.txtContactName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtContactName.Location = new System.Drawing.Point(47, 5);
            this.txtContactName.Name = "txtContactName";
            this.txtContactName.Size = new System.Drawing.Size(271, 20);
            this.txtContactName.TabIndex = 11;
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
            // lblName
            // 
            this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(3, 8);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 22;
            this.lblName.Tag = "Label_CharacterName";
            this.lblName.Text = "Name:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMetatypeLabel
            // 
            this.lblMetatypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblMetatypeLabel.AutoSize = true;
            this.lblMetatypeLabel.Location = new System.Drawing.Point(324, 8);
            this.lblMetatypeLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.lblMetatypeLabel.Name = "lblMetatypeLabel";
            this.lblMetatypeLabel.Size = new System.Drawing.Size(54, 13);
            this.lblMetatypeLabel.TabIndex = 23;
            this.lblMetatypeLabel.Tag = "Label_Metatype";
            this.lblMetatypeLabel.Text = "Metatype:";
            this.lblMetatypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboMetatype
            // 
            this.cboMetatype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboMetatype.FormattingEnabled = true;
            this.cboMetatype.Location = new System.Drawing.Point(384, 4);
            this.cboMetatype.Name = "cboMetatype";
            this.cboMetatype.Size = new System.Drawing.Size(271, 21);
            this.cboMetatype.TabIndex = 24;
            this.cboMetatype.TooltipText = "";
            this.cboMetatype.TextChanged += new System.EventHandler(this.cboMetatype_TextChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.AutoSize = true;
            this.tlpMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpMain.ColumnCount = 7;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.cmdDelete, 6, 0);
            this.tlpMain.Controls.Add(this.cmdNotes, 5, 0);
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.cboMetatype, 3, 0);
            this.tlpMain.Controls.Add(this.txtContactName, 1, 0);
            this.tlpMain.Controls.Add(this.lblMetatypeLabel, 2, 0);
            this.tlpMain.Controls.Add(this.cmdLink, 4, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(744, 30);
            this.tlpMain.TabIndex = 25;
            // 
            // cmdNotes
            // 
            this.cmdNotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
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
            this.cmdNotes.Location = new System.Drawing.Point(689, 4);
            this.cmdNotes.Name = "cmdNotes";
            this.cmdNotes.Size = new System.Drawing.Size(22, 22);
            this.cmdNotes.TabIndex = 26;
            this.cmdNotes.ToolTipText = "";
            this.cmdNotes.UseVisualStyleBackColor = true;
            this.cmdNotes.Click += new System.EventHandler(this.cmdNotes_Click);
            // 
            // cmdLink
            // 
            this.cmdLink.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdLink.AutoSize = true;
            this.cmdLink.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdLink.FlatAppearance.BorderSize = 0;
            this.cmdLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdLink.Image = global::Chummer.Properties.Resources.link_16;
            this.cmdLink.ImageDpi120 = global::Chummer.Properties.Resources.link_20;
            this.cmdLink.ImageDpi144 = global::Chummer.Properties.Resources.link_24;
            this.cmdLink.ImageDpi192 = global::Chummer.Properties.Resources.link_32;
            this.cmdLink.ImageDpi288 = global::Chummer.Properties.Resources.link_48;
            this.cmdLink.ImageDpi384 = global::Chummer.Properties.Resources.link_64;
            this.cmdLink.ImageDpi96 = global::Chummer.Properties.Resources.link_16;
            this.cmdLink.Location = new System.Drawing.Point(661, 4);
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Size = new System.Drawing.Size(22, 22);
            this.cmdLink.TabIndex = 25;
            this.cmdLink.ToolTipText = "";
            this.cmdLink.UseVisualStyleBackColor = true;
            this.cmdLink.Click += new System.EventHandler(this.cmdLink_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmdDelete.AutoSize = true;
            this.cmdDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDelete.Image = global::Chummer.Properties.Resources.delete_16;
            this.cmdDelete.ImageDpi120 = global::Chummer.Properties.Resources.delete_20;
            this.cmdDelete.ImageDpi144 = global::Chummer.Properties.Resources.delete_24;
            this.cmdDelete.ImageDpi192 = global::Chummer.Properties.Resources.delete_32;
            this.cmdDelete.ImageDpi288 = global::Chummer.Properties.Resources.delete_48;
            this.cmdDelete.ImageDpi384 = global::Chummer.Properties.Resources.delete_64;
            this.cmdDelete.ImageDpi96 = global::Chummer.Properties.Resources.delete_16;
            this.cmdDelete.Location = new System.Drawing.Point(717, 3);
            this.cmdDelete.MinimumSize = new System.Drawing.Size(24, 24);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Padding = new System.Windows.Forms.Padding(1);
            this.cmdDelete.Size = new System.Drawing.Size(24, 24);
            this.cmdDelete.TabIndex = 27;
            this.cmdDelete.Tag = "";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // PetControl
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tlpMain);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(0, 30);
            this.Name = "PetControl";
            this.Size = new System.Drawing.Size(744, 30);
            this.Load += new System.EventHandler(this.PetControl_Load);
            this.cmsContact.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtContactName;
        private System.Windows.Forms.ContextMenuStrip cmsContact;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblMetatypeLabel;
        private ElasticComboBox cboMetatype;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private DpiFriendlyToolStripMenuItem tsContactOpen;
        private DpiFriendlyToolStripMenuItem tsRemoveCharacter;
        private DpiFriendlyToolStripMenuItem tsAttachCharacter;
        private ButtonWithToolTip cmdLink;
        private ButtonWithToolTip cmdNotes;
        private DpiFriendlyImagedButton cmdDelete;
    }
}
