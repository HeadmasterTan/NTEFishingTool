namespace NTEFishingTool
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnStartFishing = new System.Windows.Forms.Button();
            this.btnStopFishing = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.selLanguage = new System.Windows.Forms.ComboBox();
            this.labelHelp = new System.Windows.Forms.Label();
            this.helpTips = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // btnStartFishing
            // 
            this.btnStartFishing.Location = new System.Drawing.Point(84, 185);
            this.btnStartFishing.Name = "btnStartFishing";
            this.btnStartFishing.Size = new System.Drawing.Size(121, 53);
            this.btnStartFishing.TabIndex = 0;
            this.btnStartFishing.Text = "自动钓鱼";
            this.btnStartFishing.UseVisualStyleBackColor = true;
            this.btnStartFishing.Click += new System.EventHandler(this.btnStartFishing_Click);
            // 
            // btnStopFishing
            // 
            this.btnStopFishing.Enabled = false;
            this.btnStopFishing.Location = new System.Drawing.Point(299, 185);
            this.btnStopFishing.Name = "btnStopFishing";
            this.btnStopFishing.Size = new System.Drawing.Size(121, 53);
            this.btnStopFishing.TabIndex = 1;
            this.btnStopFishing.Text = "停止钓鱼";
            this.btnStopFishing.UseVisualStyleBackColor = true;
            this.btnStopFishing.Click += new System.EventHandler(this.btnStopFishing_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "使用说明：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(241, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "1. 请保持游戏界面完整暴露在桌面";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(361, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "2. 开始自动钓鱼后请勿再操作电脑（除非停止钓鱼）";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(361, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "3. 支持[16:9] [16:10] [24:10] [35:10]等分辨率";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 121);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(391, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "4. 内置自动购买万能鱼饵和自动售出（不帮忙购买鱼竿）";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(81, 154);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(277, 15);
            this.label6.TabIndex = 7;
            this.label6.Text = "确认已在游戏中点击【开始钓鱼】后使用";
            // 
            // selLanguage
            // 
            this.selLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selLanguage.FormattingEnabled = true;
            this.selLanguage.Location = new System.Drawing.Point(378, 12);
            this.selLanguage.Name = "selLanguage";
            this.selLanguage.Size = new System.Drawing.Size(108, 23);
            this.selLanguage.TabIndex = 8;
            this.selLanguage.SelectedIndexChanged += new System.EventHandler(this.selLanguage_SelectedIndexChanged);
            // 
            // labelHelp
            // 
            this.labelHelp.Cursor = System.Windows.Forms.Cursors.Help;
            this.labelHelp.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelHelp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.labelHelp.Location = new System.Drawing.Point(386, 248);
            this.labelHelp.Name = "labelHelp";
            this.labelHelp.Size = new System.Drawing.Size(100, 23);
            this.labelHelp.TabIndex = 9;
            this.labelHelp.Text = "遇到问题？";
            this.labelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 280);
            this.Controls.Add(this.labelHelp);
            this.Controls.Add(this.selLanguage);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStopFishing);
            this.Controls.Add(this.btnStartFishing);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "安魂曲钓鱼工具";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartFishing;
        private System.Windows.Forms.Button btnStopFishing;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox selLanguage;
        private System.Windows.Forms.Label labelHelp;
        private System.Windows.Forms.ToolTip helpTips;
    }
}

