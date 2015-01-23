namespace StriveEngine.SimpleDemoClient
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button_send = new System.Windows.Forms.Button();
            this.textBox_port = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_IP = new System.Windows.Forms.TextBox();
            this.button_connect = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_autoScrollToButtom = new System.Windows.Forms.CheckBox();
            this.textBox_recv = new System.Windows.Forms.TextBox();
            this.comboBox_cmd = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_send
            // 
            this.button_send.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_send.Enabled = false;
            this.button_send.Location = new System.Drawing.Point(27, 62);
            this.button_send.Name = "button_send";
            this.button_send.Size = new System.Drawing.Size(75, 23);
            this.button_send.TabIndex = 4;
            this.button_send.Text = "发送";
            this.button_send.UseVisualStyleBackColor = true;
            this.button_send.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox_port
            // 
            this.textBox_port.Location = new System.Drawing.Point(288, 14);
            this.textBox_port.Name = "textBox_port";
            this.textBox_port.Size = new System.Drawing.Size(100, 21);
            this.textBox_port.TabIndex = 7;
            this.textBox_port.Text = "9000";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(10, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 23);
            this.label1.TabIndex = 6;
            this.label1.Text = "服务器IP：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(243, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 23);
            this.label4.TabIndex = 6;
            this.label4.Text = "端口：";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBox_IP
            // 
            this.textBox_IP.Location = new System.Drawing.Point(105, 14);
            this.textBox_IP.Name = "textBox_IP";
            this.textBox_IP.Size = new System.Drawing.Size(132, 21);
            this.textBox_IP.TabIndex = 7;
            this.textBox_IP.Text = "127.0.0.1";
            // 
            // button_connect
            // 
            this.button_connect.Location = new System.Drawing.Point(394, 13);
            this.button_connect.Name = "button_connect";
            this.button_connect.Size = new System.Drawing.Size(75, 23);
            this.button_connect.TabIndex = 8;
            this.button_connect.Text = "连接";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button3_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBox_autoScrollToButtom);
            this.groupBox2.Controls.Add(this.textBox_recv);
            this.groupBox2.Location = new System.Drawing.Point(14, 101);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1107, 372);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "来自服务端的消息";
            // 
            // checkBox_autoScrollToButtom
            // 
            this.checkBox_autoScrollToButtom.AutoSize = true;
            this.checkBox_autoScrollToButtom.Checked = true;
            this.checkBox_autoScrollToButtom.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoScrollToButtom.Location = new System.Drawing.Point(13, 21);
            this.checkBox_autoScrollToButtom.Name = "checkBox_autoScrollToButtom";
            this.checkBox_autoScrollToButtom.Size = new System.Drawing.Size(108, 16);
            this.checkBox_autoScrollToButtom.TabIndex = 1;
            this.checkBox_autoScrollToButtom.Text = "自动滚动到底部";
            this.checkBox_autoScrollToButtom.UseVisualStyleBackColor = true;
            // 
            // textBox_recv
            // 
            this.textBox_recv.Location = new System.Drawing.Point(6, 42);
            this.textBox_recv.MaxLength = 65535;
            this.textBox_recv.Multiline = true;
            this.textBox_recv.Name = "textBox_recv";
            this.textBox_recv.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_recv.Size = new System.Drawing.Size(1095, 317);
            this.textBox_recv.TabIndex = 0;
            this.textBox_recv.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // comboBox_cmd
            // 
            this.comboBox_cmd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_cmd.FormattingEnabled = true;
            this.comboBox_cmd.Location = new System.Drawing.Point(120, 64);
            this.comboBox_cmd.Name = "comboBox_cmd";
            this.comboBox_cmd.Size = new System.Drawing.Size(995, 20);
            this.comboBox_cmd.TabIndex = 10;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 500);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1144, 22);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1144, 522);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.comboBox_cmd);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button_send);
            this.Controls.Add(this.button_connect);
            this.Controls.Add(this.textBox_IP);
            this.Controls.Add(this.textBox_port);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StriveEngine Demo 客户端 （基于StriveEngine）";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_send;
        private System.Windows.Forms.TextBox textBox_port;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_IP;
        private System.Windows.Forms.Button button_connect;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox_recv;
        private System.Windows.Forms.CheckBox checkBox_autoScrollToButtom;
        private System.Windows.Forms.ComboBox comboBox_cmd;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}

