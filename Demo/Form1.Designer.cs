
namespace Demo
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonLoadDbc = new System.Windows.Forms.Button();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxNodes = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dataGridViewMsg = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.dataGridViewSig = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMsg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSig)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonLoadDbc
            // 
            this.buttonLoadDbc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLoadDbc.Location = new System.Drawing.Point(760, 20);
            this.buttonLoadDbc.Name = "buttonLoadDbc";
            this.buttonLoadDbc.Size = new System.Drawing.Size(113, 25);
            this.buttonLoadDbc.TabIndex = 0;
            this.buttonLoadDbc.Text = "Load DBC";
            this.buttonLoadDbc.UseVisualStyleBackColor = true;
            this.buttonLoadDbc.Click += new System.EventHandler(this.buttonLoadDbc_Click);
            // 
            // textBoxPath
            // 
            this.textBoxPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPath.Location = new System.Drawing.Point(12, 21);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(740, 23);
            this.textBoxPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Nodes:";
            // 
            // textBoxNodes
            // 
            this.textBoxNodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNodes.Location = new System.Drawing.Point(11, 74);
            this.textBoxNodes.Name = "textBoxNodes";
            this.textBoxNodes.Size = new System.Drawing.Size(861, 23);
            this.textBoxNodes.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Messages:";
            // 
            // dataGridViewMsg
            // 
            this.dataGridViewMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewMsg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMsg.Location = new System.Drawing.Point(11, 128);
            this.dataGridViewMsg.Name = "dataGridViewMsg";
            this.dataGridViewMsg.RowTemplate.Height = 25;
            this.dataGridViewMsg.Size = new System.Drawing.Size(861, 126);
            this.dataGridViewMsg.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 274);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Signals:";
            // 
            // dataGridViewSig
            // 
            this.dataGridViewSig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewSig.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSig.Location = new System.Drawing.Point(11, 292);
            this.dataGridViewSig.Name = "dataGridViewSig";
            this.dataGridViewSig.RowTemplate.Height = 25;
            this.dataGridViewSig.Size = new System.Drawing.Size(861, 307);
            this.dataGridViewSig.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 611);
            this.Controls.Add(this.dataGridViewSig);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dataGridViewMsg);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxNodes);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxPath);
            this.Controls.Add(this.buttonLoadDbc);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMsg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSig)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonLoadDbc;
        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxNodes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dataGridViewMsg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dataGridViewSig;
    }
}

