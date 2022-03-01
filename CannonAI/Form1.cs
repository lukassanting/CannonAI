using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;


namespace CannonAI
{
    public partial class Screen : Form
    {
        // array for stone representation - 1 for red, 2 for black
        int[,] stonesArray;
        int[,] helpArray;
        CannonGame game;
        bool setUp; //whether the board needs to be setup from scratch
        bool help; //whether to visually display possible moves
        int offset = 30; //offset to position graphic display of stones on cross-points
        string[] history;

        public Screen()
        {
            InitializeComponent();
            AllocConsole();

            this.Load += Screen_Load;
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            // prevent screen flicker on update
            gameBoard.DoubleBuffered(true);
            // load background image
            string dir = Directory.GetCurrentDirectory();
            string target = @"\Assets\fullBoard_Noted.bmp";
            gameBoard.BackgroundImage = Image.FromFile(dir + target);
            // variables for first time set up
            game = new CannonGame(gameType.Text, botType.Text);
            stonesArray = game.gameState.Board;
            helpArray = game.HelpArray;
            setUp = true;
            help = true;
            history = new string[10] { "", "", "", "", "", "", "", "", "", "" };

            refreshButton.Click += this.RefreshGame;
            helpButton.Click += this.ToggleHelp;
            gameBoard.Paint += this.GenerateGame;
            gameBoard.Click += this.GameClick;
        }
        // set up the initial board, call draw methods for initial pieces, etc
        private void GenerateGame(object sender, PaintEventArgs e)
        {
            DrawStones(e);

            if (setUp)
            {
                DrawFirstStones(e);
                setUp = false;
            }
        }

        // ----------------------------------------------------------------------
        // ~~~~~~~~~~~~~~~~~~~~~~~~ ON-CLICK FUNCTIONS ~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ----------------------------------------------------------------------

        // on click, update the game according to new CannonGame state
        private void GameClick(object sender, EventArgs e)
        {

            // co-ordinates of mouse pointer on panel
            Point clickPoint = gameBoard.PointToClient(Cursor.Position);

            // divide co-ordinates by 50 to find which cell is being clicked
            int x = (clickPoint.X - offset) / 50;
            int y = (clickPoint.Y - offset) / 50;

            // method in CannonGame class to update game state
            game.GameUpdate(x, y);
            SetLabels();
            gameBoard.Refresh();
            // if a player has won the game
            if (game.IsGameOver)
                MessageBox.Show("Game Over - " + game.Winner.Name + " is victorious! Refresh game to play again.");
        }

        private void RefreshGame(object sender, EventArgs e)
        {
            // refresh game variables, restart game
            game = new CannonGame(gameType.Text, botType.Text);
            stonesArray = game.gameState.Board;
            helpArray = game.HelpArray;
            setUp = true;
            history = new string[10] { "", "", "", "", "", "", "", "", "", "" };
            historyLabel.Text = "History:";
            SetLabels();
            gameBoard.Refresh();
        }

        private void ToggleHelp(object sender, EventArgs e)
        {
            help = !help;
            if (help) helpLabel.Text = "Help = On";
            else helpLabel.Text = "Help = Off";
            gameBoard.Refresh();
        }

        // ----------------------------------------------------------------------
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~ SETTING LABELS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ----------------------------------------------------------------------

        public void SetLabels()
        {
            if (game.updateHistory)
            {
                string update = game.HistoryText;
                if (game.GameType != "Player vs Player")
                {
                    string[] updates = update.Split(new string[] {"\n" }, StringSplitOptions.None);
                    foreach (string u in updates) HistoryLabel(u);
                }
                else HistoryLabel(update);
            }
            updateText.Text = game.LabelText;
            turnLabel.Text = game.TurnText;
            if (help) helpLabel.Text = "Help = On";
            else helpLabel.Text = "Help = Off";
        }

        // TODO: Fix so doesn't double fill an array during PvB
        public void HistoryLabel(string update)
        {
            if (history[9] != "")
            {
                for (int i = 1; i <= 9; i++)
                    history[i - 1] = history[i];
                history[9] = update;
            }
            else
            {
                for (int i = 0; i <= 9; i++)
                {
                    if (history[i] == "")
                    {
                        history[i] = update;
                        break;
                    }
                }
            }
            historyLabel.Text = "History:\n";
            foreach (string s in history)
                historyLabel.Text += $"{s}\n";
        }

        // ----------------------------------------------------------------------
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~ DRAWING STONES ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ----------------------------------------------------------------------

        // Based on the game array, update the graphical representation of the board
        public void DrawStones(PaintEventArgs pea)
        {
            // TODO: add steps to check HelpArray and display in muted colours
            for (int i = 0; i < 10; i++)
                for(int j = 0; j < 10; j++)
                {
                    if(stonesArray[i,j] == 1)
                    {
                        int x = i * 50 + offset;
                        int y = j * 50 + offset;
                        pea.Graphics.FillEllipse(Brushes.Red, x, y, 40, 40);
                    }
                    if (stonesArray[i, j] == 2)
                    {
                        int x = i * 50 + offset;
                        int y = j * 50 + offset;
                        pea.Graphics.FillEllipse(Brushes.DarkSlateGray, x, y, 40, 40);
                    }
                    if (stonesArray[i, j] == 3)
                    {
                        int x = i * 50 + offset;
                        int y = j * 50 + offset;
                        pea.Graphics.FillRectangle(Brushes.Red, x, y, 40, 40);
                    }
                    if (stonesArray[i, j] == 4)
                    {
                        int x = i * 50 + offset;
                        int y = j * 50 + offset;
                        pea.Graphics.FillRectangle(Brushes.DarkSlateGray, x, y, 40, 40);
                    }
                    if (help) // only display possible moves if "help" is selected
                    {
                        if (helpArray[i, j] == 1)
                        {
                            int x = i * 50 + offset;
                            int y = j * 50 + offset;
                            pea.Graphics.FillEllipse(Brushes.Pink, x, y, 40, 40);
                        }
                        if (helpArray[i, j] == 2)
                        {
                            int x = i * 50 + offset;
                            int y = j * 50 + offset;
                            pea.Graphics.FillEllipse(Brushes.Gray, x, y, 40, 40);
                        }
                        if (helpArray[i, j] == 3 || helpArray[i, j] == 4)
                        {
                            int x = i * 50 + offset;
                            int y = j * 50 + offset;
                            pea.Graphics.FillRectangle(Brushes.Orange, x, y, 40, 40);
                        }
                    }
                }
        }

        public void DrawFirstStones(PaintEventArgs pea)
        {
            // Game starts with fifteen stones each, variables named here with (currently incorrect) chess notation
            
            //  --- RED ---
            // Define abstract starting co-ordinates
            int b9x = 1, b9y = 1;
            int b8x = 1, b8y = 2;
            int b7x = 1, b7y = 3;

            int d9x = 3, d9y = 1;
            int d8x = 3, d8y = 2;
            int d7x = 3, d7y = 3;

            int f9x = 5, f9y = 1;
            int f8x = 5, f8y = 2;
            int f7x = 5, f7y = 3;

            int h9x = 7, h9y = 1;
            int h8x = 7, h8y = 2;
            int h7x = 7, h7y = 3;

            int j9x = 9, j9y = 1;
            int j8x = 9, j8y = 2;
            int j7x = 9, j7y = 3;

            // calculate co-ords for Painting
            int red_b9x = b9x * 50 + offset, red_b9y = b9y * 50 + offset;
            int red_b8x = b8x * 50 + offset, red_b8y = b8y * 50 + offset;
            int red_b7x = b8x * 50 + offset, red_b7y = b7y * 50 + offset;

            int red_d9x = d9x * 50 + offset, red_d9y = d9y * 50 + offset;
            int red_d8x = d8x * 50 + offset, red_d8y = d8y * 50 + offset;
            int red_d7x = d7x * 50 + offset, red_d7y = d7y * 50 + offset;

            int red_f9x = f9x * 50 + offset, red_f9y = f9y * 50 + offset;
            int red_f8x = f8x * 50 + offset, red_f8y = f8y * 50 + offset;
            int red_f7x = f7x * 50 + offset, red_f7y = f7y * 50 + offset;

            int red_h9x = h9x * 50 + offset, red_h9y = h9y * 50 + offset;
            int red_h8x = h8x * 50 + offset, red_h8y = h8y * 50 + offset;
            int red_h7x = h7x * 50 + offset, red_h7y = h7y * 50 + offset;

            int red_j9x = j9x * 50 + offset, red_j9y = j9y * 50 + offset;
            int red_j8x = j8x * 50 + offset, red_j8y = j8y * 50 + offset;
            int red_j7x = j7x * 50 + offset, red_j7y = j7y * 50 + offset;

            // draw ellipses
            pea.Graphics.FillEllipse(Brushes.Red, red_b9x, red_b9y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_b8x, red_b8y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_b7x, red_b7y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_d9x, red_d9y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_d8x, red_d8y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_d7x, red_d7y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_f9x, red_f9y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_f8x, red_f8y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_f7x, red_f7y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_h9x, red_h9y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_h8x, red_h8y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_h7x, red_h7y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_j9x, red_j9y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_j8x, red_j8y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.Red, red_j7x, red_j7y, 40, 40);

            // update array
            stonesArray[b9x, b9y] = 1;
            stonesArray[b8x, b8y] = 1;
            stonesArray[b7x, b7y] = 1;
            stonesArray[d9x, d9y] = 1;
            stonesArray[d8x, d8y] = 1;
            stonesArray[d7x, d7y] = 1;
            stonesArray[f9x, f9y] = 1;
            stonesArray[f8x, f8y] = 1;
            stonesArray[f7x, f7y] = 1;
            stonesArray[h9x, h9y] = 1;
            stonesArray[h8x, h8y] = 1;
            stonesArray[h7x, h7y] = 1;
            stonesArray[j9x, j9y] = 1;
            stonesArray[j8x, j8y] = 1;
            stonesArray[j7x, j7y] = 1;

            // --- GREY --- 
            // Define abstract starting co-ordinates
            int a4x = 0, a4y = 6;
            int a3x = 0, a3y = 7;
            int a2x = 0, a2y = 8;

            int c4x = 2, c4y = 6;
            int c3x = 2, c3y = 7;
            int c2x = 2, c2y = 8;

            int e4x = 4, e4y = 6;
            int e3x = 4, e3y = 7;
            int e2x = 4, e2y = 8;

            int g4x = 6, g4y = 6;
            int g3x = 6, g3y = 7;
            int g2x = 6, g2y = 8;

            int i4x = 8, i4y = 6;
            int i3x = 8, i3y = 7;
            int i2x = 8, i2y = 8;

            // calculate co-ords for Painting
            int grey_a4x = a4x * 50 + offset, grey_a4y = a4y * 50 + offset;
            int grey_a3x = a3x * 50 + offset, grey_a3y = a3y * 50 + offset;
            int grey_a2x = a2x * 50 + offset, grey_a2y = a2y * 50 + offset;

            int grey_c4x = c4x * 50 + offset, grey_c4y = c4y * 50 + offset;
            int grey_c3x = c3x * 50 + offset, grey_c3y = c3y * 50 + offset;
            int grey_c2x = c2x * 50 + offset, grey_c2y = c2y * 50 + offset;

            int grey_e4x = e4x * 50 + offset, grey_e4y = e4y * 50 + offset;
            int grey_e3x = e3x * 50 + offset, grey_e3y = e3y * 50 + offset;
            int grey_e2x = e2x * 50 + offset, grey_e2y = e2y * 50 + offset;

            int grey_g4x = g4x * 50 + offset, grey_g4y = g4y * 50 + offset;
            int grey_g3x = g3x * 50 + offset, grey_g3y = g3y * 50 + offset;
            int grey_g2x = g2x * 50 + offset, grey_g2y = g2y * 50 + offset;

            int grey_i4x = i4x * 50 + offset, grey_i4y = i4y * 50 + offset;
            int grey_i3x = i3x * 50 + offset, grey_i3y = i3y * 50 + offset;
            int grey_i2x = i2x * 50 + offset, grey_i2y = i2y * 50 + offset;

            // draw ellipses
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_a4x, grey_a4y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_a3x, grey_a3y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_a2x, grey_a2y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_c4x, grey_c4y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_c3x, grey_c3y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_c2x, grey_c2y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_e4x, grey_e4y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_e3x, grey_e3y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_e2x, grey_e2y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_g4x, grey_g4y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_g3x, grey_g3y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_g2x, grey_g2y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_i4x, grey_i4y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_i3x, grey_i3y, 40, 40);
            pea.Graphics.FillEllipse(Brushes.DarkSlateGray, grey_i2x, grey_i2y, 40, 40);

            // fill array
            stonesArray[a4x, a4y] = 2;
            stonesArray[a3x, a3y] = 2;
            stonesArray[a2x, a2y] = 2;
            stonesArray[c4x, c4y] = 2;
            stonesArray[c3x, c3y] = 2;
            stonesArray[c2x, c2y] = 2;
            stonesArray[e4x, e4y] = 2;
            stonesArray[e3x, e3y] = 2;
            stonesArray[e2x, e2y] = 2;
            stonesArray[g4x, g4y] = 2;
            stonesArray[g3x, g3y] = 2;
            stonesArray[g2x, g2y] = 2;
            stonesArray[i4x, i4y] = 2;
            stonesArray[i3x, i3y] = 2;
            stonesArray[i2x, i2y] = 2;

        }

        // ----------------------------------------------------------------------
        // ~~~~~~~~~~~~~~~~~~~~~~~~~ HELPER FEATURES ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ----------------------------------------------------------------------

        // code for the AllocConsole() method called at start, needed to use Console with Windows Forms
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }



    // The following partial class is code I took from the following site:
    // http://csharp.tips/tip/article/852-how-to-prevent-flicker-in-winforms-control
    // for the purpose preventing screen flickering on update in windows forms by using double buffering
    public static class Extensions
    {
        public static void DoubleBuffered(this Control control, bool enabled)
        {
            var prop = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(control, enabled, null);
        }
    }
}


