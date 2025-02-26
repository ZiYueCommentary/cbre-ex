﻿using CBRE.Localization;

namespace CBRE.Editor.UI
{
    partial class MapTreeWindow
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.MapTree = new System.Windows.Forms.TreeView();
			this.Properties = new System.Windows.Forms.ListView();
			this.KeyColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ValueColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// MapTree
			// 
			this.MapTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MapTree.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MapTree.HideSelection = false;
			this.MapTree.Location = new System.Drawing.Point(0, 0);
			this.MapTree.Name = "MapTree";
			this.MapTree.Size = new System.Drawing.Size(313, 441);
			this.MapTree.TabIndex = 4;
			this.MapTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeSelectionChanged);
			// 
			// Properties
			// 
			this.Properties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Properties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.KeyColumn,
            this.ValueColumn});
			this.Properties.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Properties.HideSelection = false;
			this.Properties.Location = new System.Drawing.Point(319, 0);
			this.Properties.Name = "Properties";
			this.Properties.Size = new System.Drawing.Size(305, 441);
			this.Properties.TabIndex = 5;
			this.Properties.UseCompatibleStateImageBehavior = false;
			this.Properties.View = System.Windows.Forms.View.Details;
			// 
			// KeyColumn
			// 
			this.KeyColumn.Text = Local.LocalString("maptree.key");
			this.KeyColumn.Width = 105;
			// 
			// ValueColumn
			// 
			this.ValueColumn.Text = Local.LocalString("object_properties.value");
			this.ValueColumn.Width = 177;
			// 
			// MapTreeWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 441);
			this.Controls.Add(this.Properties);
			this.Controls.Add(this.MapTree);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "MapTreeWindow";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = Local.LocalString("maptree");
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView MapTree;
        private System.Windows.Forms.ListView Properties;
        private System.Windows.Forms.ColumnHeader KeyColumn;
        private System.Windows.Forms.ColumnHeader ValueColumn;
    }
}