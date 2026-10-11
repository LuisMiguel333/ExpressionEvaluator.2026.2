using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Backend;

namespace Frontend.Windows
{
    public partial class Form1 : Form
    {
        private TextBox txtDisplay;
        private string currentExpression = "";
        private const int BaseClientWidth = 595;

        public Form1()
        {
            InitializeComponent();
            BuildCalculatorUI();
        }

        private void BuildCalculatorUI()
        {
            // Configuración de la ventana principal
            this.Text = "Functions Evaluator";
            this.ClientSize = new Size(BaseClientWidth, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;

            // Pantalla (Display) verde superior, anclada a los lados
            txtDisplay = new TextBox();
            txtDisplay.Location = new Point(15, 15);
            txtDisplay.Size = new Size(BaseClientWidth - 30, 55);
            txtDisplay.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDisplay.Font = new Font("Consolas", 20, FontStyle.Bold);
            txtDisplay.BackColor = Color.FromArgb(0, 150, 0);
            txtDisplay.ForeColor = Color.White;
            txtDisplay.TextAlign = HorizontalAlignment.Left;
            txtDisplay.ReadOnly = true;
            this.Controls.Add(txtDisplay);

            string[,] buttons = {
                { "7", "8", "9", "(", ")", "Delete" },
                { "4", "5", "6", "*", "/", "Clear" },
                { "1", "2", "3", "+", "-", "^" },
                { "0", ".", "=", "=", "=", "=" }
            };

            int startX = 15;
            int startY = 85;
            int gap = 8;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    string text = buttons[row, col];

                    if (row == 3 && col > 2) continue;

                    Button btn = new Button();
                    btn.Text = text;
                    btn.Font = new Font("Segoe UI", 14, FontStyle.Bold);

                    int currentWidth = 75;
                    int currentHeight = 65;

                    if (col == 3 || col == 4)
                    {
                        currentWidth = 55;
                    }
                    else if (col == 5)
                    {
                        currentWidth = 190;
                    }

                    if (row == 3 && col == 2)
                    {
                        currentWidth = 75 + gap + 55 + gap + 55 + gap + 190;
                    }

                    btn.Width = currentWidth;
                    btn.Height = currentHeight;

                    int posX = startX;
                    for (int c = 0; c < col; c++)
                    {
                        int wPrev = 75;
                        if (c == 3 || c == 4) wPrev = 55;
                        if (c == 5) wPrev = 190;
                        if (row == 3 && c >= 2) break;
                        posX += wPrev + gap;
                    }

                    if (row == 3 && col > 2)
                    {
                        posX = startX;
                        for (int c = 0; c < 2; c++)
                        {
                            int wPrev = 75;
                            posX += wPrev + gap;
                        }
                    }

                    btn.Left = posX;
                    btn.Top = startY + (row * (currentHeight + gap));

                    if (text == "Delete" || text == "Clear" || text == "^" || text == "*" || text == "/" || text == "+" || text == "-")
                    {
                        btn.BackColor = Color.FromArgb(235, 130, 20);
                        btn.ForeColor = Color.White;
                    }
                    else if (text == "=")
                    {
                        btn.BackColor = Color.FromArgb(245, 120, 10);
                        btn.ForeColor = Color.White;
                    }
                    else
                    {
                        btn.BackColor = Color.FromArgb(245, 245, 245);
                        btn.ForeColor = Color.Black;
                    }

                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Tag = text;
                    btn.Click += CalculatorButton_Click;

                    // Delete, Clear, ^ y = se estiran con la ventana; los demás quedan fijos a la izquierda
                    if (col == 5 || (row == 3 && col == 2))
                    {
                        btn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    }

                    this.Controls.Add(btn);
                }
            }
        }

        // Actualiza la pantalla y estira la ventana SOLO si el texto no cabe
        private void UpdateDisplay(string text)
        {
            txtDisplay.Text = text;

            // Ancho real del texto con la fuente del display
            int textWidth = TextRenderer.MeasureText(text, txtDisplay.Font).Width;

            // + margen interno del TextBox + márgenes laterales (15 + 15)
            int neededWidth = textWidth + 20 + 30;

            // Nunca menor al base (vuelve al tamaño normal si cabe), ni mayor a la pantalla
            int maxWidth = Screen.FromControl(this).WorkingArea.Width;
            int newWidth = Math.Min(Math.Max(BaseClientWidth, neededWidth), maxWidth);

            this.ClientSize = new Size(newWidth, this.ClientSize.Height);

            // Si al crecer se sale de la pantalla por la derecha, lo movemos a la izquierda
            Rectangle area = Screen.FromControl(this).WorkingArea;
            if (this.Right > area.Right)
            {
                this.Left = Math.Max(area.Left, area.Right - this.Width);
            }
        }

        private void CalculatorButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string value = btn.Tag.ToString();

            if (value == "Clear")
            {
                currentExpression = "";
                UpdateDisplay("");
            }
            else if (value == "Delete")
            {
                if (currentExpression.Length > 0)
                {
                    currentExpression = currentExpression.Substring(0, currentExpression.Length - 1);
                    UpdateDisplay(currentExpression);
                }
            }
            else if (value == "=")
            {
                try
                {
                    double result = ExpressionEvaluator.Evalute(currentExpression);

                    // InvariantCulture para que salga con punto (12.3045...) y no con coma
                    string resultText = result.ToString(CultureInfo.InvariantCulture);

                    UpdateDisplay(currentExpression + "=" + resultText);
                    currentExpression = resultText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error en la expresión: " + ex.Message, "Error de cálculo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    currentExpression = "";
                    UpdateDisplay("");
                }
            }
            else
            {
                currentExpression += value;
                UpdateDisplay(currentExpression);
            }
        }
    }
}