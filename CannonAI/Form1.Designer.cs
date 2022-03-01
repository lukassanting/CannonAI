
namespace CannonAI
{
    partial class Screen
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
            this.gameBoard = new System.Windows.Forms.Panel();
            this.updateText = new System.Windows.Forms.Label();
            this.refreshButton = new System.Windows.Forms.Button();
            this.gameType = new System.Windows.Forms.ComboBox();
            this.gameTypeLabel = new System.Windows.Forms.Label();
            this.helpButton = new System.Windows.Forms.Button();
            this.turnLabel = new System.Windows.Forms.Label();
            this.helpLabel = new System.Windows.Forms.Label();
            this.botType = new System.Windows.Forms.ComboBox();
            this.BotTypeLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.historyLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // gameBoard
            // 
            this.gameBoard.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.gameBoard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.gameBoard.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gameBoard.Location = new System.Drawing.Point(0, 0);
            this.gameBoard.Name = "gameBoard";
            this.gameBoard.Size = new System.Drawing.Size(525, 525);
            this.gameBoard.TabIndex = 0;
            // 
            // updateText
            // 
            this.updateText.AutoSize = true;
            this.updateText.Location = new System.Drawing.Point(12, 542);
            this.updateText.Name = "updateText";
            this.updateText.Size = new System.Drawing.Size(57, 15);
            this.updateText.TabIndex = 1;
            this.updateText.Text = "Waiting...";
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(536, 59);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 23);
            this.refreshButton.TabIndex = 2;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            // 
            // gameType
            // 
            this.gameType.FormattingEnabled = true;
            this.gameType.Items.AddRange(new object[] {
            "Player vs Player",
            "Player vs Bot",
            "Bot vs Player"});
            this.gameType.Location = new System.Drawing.Point(574, 30);
            this.gameType.Name = "gameType";
            this.gameType.Size = new System.Drawing.Size(121, 23);
            this.gameType.TabIndex = 3;
            this.gameType.Text = "Player vs Player";
            // 
            // gameTypeLabel
            // 
            this.gameTypeLabel.AutoSize = true;
            this.gameTypeLabel.Location = new System.Drawing.Point(584, 9);
            this.gameTypeLabel.Name = "gameTypeLabel";
            this.gameTypeLabel.Size = new System.Drawing.Size(100, 15);
            this.gameTypeLabel.TabIndex = 4;
            this.gameTypeLabel.Text = "Select game type:";
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(647, 59);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 5;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            // 
            // turnLabel
            // 
            this.turnLabel.AutoSize = true;
            this.turnLabel.Location = new System.Drawing.Point(596, 106);
            this.turnLabel.Name = "turnLabel";
            this.turnLabel.Size = new System.Drawing.Size(69, 15);
            this.turnLabel.TabIndex = 6;
            this.turnLabel.Text = "Turn = Grey";
            // 
            // helpLabel
            // 
            this.helpLabel.AutoSize = true;
            this.helpLabel.Location = new System.Drawing.Point(596, 121);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(62, 15);
            this.helpLabel.TabIndex = 7;
            this.helpLabel.Text = "Help = On";
            // 
            // botType
            // 
            this.botType.FormattingEnabled = true;
            this.botType.Items.AddRange(new object[] {
            "Random",
            "Mark I (MiniMax)",
            "Mark II (Alpha-Beta)",
            "Mark III (AB with TT)",
            "Mark IV (Iterative DS)"});
            this.botType.Location = new System.Drawing.Point(574, 170);
            this.botType.Name = "botType";
            this.botType.Size = new System.Drawing.Size(121, 23);
            this.botType.TabIndex = 8;
            this.botType.Text = "Random";
            // 
            // BotTypeLabel
            // 
            this.BotTypeLabel.AutoSize = true;
            this.BotTypeLabel.Location = new System.Drawing.Point(585, 152);
            this.BotTypeLabel.Name = "BotTypeLabel";
            this.BotTypeLabel.Size = new System.Drawing.Size(89, 15);
            this.BotTypeLabel.TabIndex = 10;
            this.BotTypeLabel.Text = "Select Bot Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1052, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Turn = Grey";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-1052, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Help = On";
            // 
            // historyLabel
            // 
            this.historyLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.historyLabel.AutoSize = true;
            this.historyLabel.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.historyLabel.Location = new System.Drawing.Point(596, 209);
            this.historyLabel.Name = "historyLabel";
            this.historyLabel.Size = new System.Drawing.Size(48, 15);
            this.historyLabel.TabIndex = 12;
            this.historyLabel.Text = "History:";
            // 
            // Screen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 566);
            this.Controls.Add(this.historyLabel);
            this.Controls.Add(this.BotTypeLabel);
            this.Controls.Add(this.botType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.updateText);
            this.Controls.Add(this.turnLabel);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.gameTypeLabel);
            this.Controls.Add(this.gameType);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.gameBoard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Screen";
            this.ShowIcon = false;
            this.Text = "Cannon";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel gameBoard;
        public System.Windows.Forms.Label updateText;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ComboBox gameType;
        private System.Windows.Forms.Label gameTypeLabel;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Label turnLabel;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.ComboBox botType;
        private System.Windows.Forms.Label BotTypeLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label historyLabel;
    }
}

