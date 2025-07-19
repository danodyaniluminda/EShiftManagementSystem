using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EShiftManagementSystem.DAL;

namespace EShiftManagementSystem
{
    public partial class LoginForm : Form
    {
        private readonly DataManager _dataManager;
        private readonly EShiftDbContext _context;
        private Button btnAdminLogin;
        private Button btnUserLogin;
        private PictureBox pbLogo;
        private Panel panelMain;
        private Image backgroundImage;

        public LoginForm(DataManager dataManager, EShiftDbContext context)
        {
            InitializeComponent();
            SetupCustomControls();
            _dataManager = dataManager;
            _context = context;
        }



        private void SetupCustomControls()
        {
            this.Text = "e-Shift - Welcome";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42);
            backgroundImage = Properties.Resources.background;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            this.FormClosing += MainEntryForm_FormClosing;

            panelMain = new Panel
            {
                Size = new Size(450, 500),
                Location = new Point((this.Width - 450) / 2, (this.Height - 500) / 2),
                BackColor = Color.FromArgb(90, 255, 255, 255),
                BorderStyle = BorderStyle.None
            };
            AddRoundedCorners(panelMain, 20);
            AddGlassmorphismEffect(panelMain);

            CreateMainContent();

            this.Controls.Add(panelMain);

            CreateCloseButton();
        }


        private void CreateMainContent()
        {
            // Company logo with modern gradient
            pbLogo = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(185, 40),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };

            // Create a modern gradient logo
            pbLogo.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Create gradient brush
                using (var gradientBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, 80, 80),
                    Color.FromArgb(79, 70, 229), // Indigo
                    Color.FromArgb(147, 51, 234), // Purple
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillEllipse(gradientBrush, 0, 0, 80, 80);
                }

                // Add inner glow effect
                using (var glowBrush = new LinearGradientBrush(
                    new Rectangle(10, 10, 60, 60),
                    Color.FromArgb(100, 255, 255, 255),
                    Color.FromArgb(30, 255, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillEllipse(glowBrush, 10, 10, 60, 60);
                }

                // Logo text
                using (var font = new Font("Segoe UI", 20, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.White))
                {
                    var textSize = e.Graphics.MeasureString("eS", font);
                    float x = (80 - textSize.Width) / 2;
                    float y = (80 - textSize.Height) / 2;
                    e.Graphics.DrawString("eS", font, textBrush, x, y);
                }
            };

            // Title with modern styling
            var lblTitle = new Label
            {
                Text = "e-Shift",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59), // Dark slate
                AutoSize = true,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblTitle.Location = new Point((panelMain.Width - lblTitle.PreferredWidth) / 2, 140);

            // Subtitle with gradient effect
            var lblSubtitle = new Label
            {
                Text = "Transport Management System",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 0), // Slate
                AutoSize = true,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblSubtitle.Location = new Point((panelMain.Width - lblSubtitle.PreferredWidth) / 2, 200);

            // Welcome message
            var lblWelcome = new Label
            {
                Text = "Please select your account type to continue",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 0, 0), // Slate
                AutoSize = true,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblWelcome.Location = new Point((panelMain.Width - lblWelcome.PreferredWidth) / 2, 230);

            // Admin Login button with modern gradient
            btnAdminLogin = new Button
            {
                Text = "👤 Admin Login",
                Location = new Point(30, 300),
                Size = new Size(170, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(239, 68, 68), // Red
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdminLogin.FlatAppearance.BorderSize = 0;
            btnAdminLogin.Click += BtnAdminLogin_Click;
            AddModernButtonEffect(btnAdminLogin, Color.FromArgb(220, 38, 38), Color.FromArgb(185, 28, 28));
            AddRoundedCorners(btnAdminLogin, 12);

            // User Login button with modern gradient
            btnUserLogin = new Button
            {
                Text = "🚗 Customer Login",
                Location = new Point(245, 300),
                Size = new Size(170, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(34, 197, 94), // Green
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUserLogin.FlatAppearance.BorderSize = 0;
            btnUserLogin.Click += BtnUserLogin_Click;
            AddModernButtonEffect(btnUserLogin, Color.FromArgb(22, 163, 74), Color.FromArgb(21, 128, 61));
            AddRoundedCorners(btnUserLogin, 12);



            // Add controls to panel
            panelMain.Controls.AddRange(new Control[] {
                pbLogo, lblTitle, lblSubtitle, lblWelcome, btnAdminLogin, btnUserLogin
            });
        }

        private void CreateCloseButton()
        {
            var btnClose = new Button
            {
                Text = "×",
                Location = new Point(panelMain.Width - 45, 15),
                Size = new Size(30, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(107, 114, 128),
                BackColor = Color.FromArgb(50, 255, 255, 255), // Semi-transparent white instead of transparent
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;
            AddRoundedCorners(btnClose, 15); // Make it circular
            AddCloseButtonHoverEffect(btnClose);

            panelMain.Controls.Add(btnClose);
            btnClose.BringToFront();
        }

        private void BtnAdminLogin_Click(object sender, EventArgs e)
        {
            var adminLoginForm = new AdminLoginForm(_dataManager, _context);
            adminLoginForm.Show();
            this.Hide();
        }

        private void BtnUserLogin_Click(object sender, EventArgs e)
        {
            var userLoginForm = new UserLoginForm(_dataManager, _context);
            userLoginForm.Show();
            this.Hide();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit the application?",
                "Exit Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void AddModernButtonEffect(Button button, Color originalColor, Color hoverColor)
        {
            var originalForeColor = button.ForeColor;

            button.MouseEnter += (s, e) =>
            {
                button.BackColor = hoverColor;
                button.ForeColor = Color.White;

                // Add subtle scale effect
                button.Size = new Size(button.Width + 2, button.Height + 2);
                button.Location = new Point(button.Location.X - 1, button.Location.Y - 1);
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = originalColor;
                button.ForeColor = originalForeColor;

                // Reset scale
                button.Size = new Size(button.Width - 2, button.Height - 2);
                button.Location = new Point(button.Location.X + 1, button.Location.Y + 1);
            };
        }

        private void AddCloseButtonHoverEffect(Button button)
        {
            var originalColor = Color.FromArgb(50, 255, 255, 255);
            var originalForeColor = button.ForeColor;

            button.MouseEnter += (s, e) =>
            {
                button.BackColor = Color.FromArgb(239, 68, 68);
                button.ForeColor = Color.White;
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = originalColor;
                button.ForeColor = originalForeColor;
            };
        }

        private void AddRoundedCorners(Control control, int radius)
        {
            control.Region = CreateRoundedRectangle(control.ClientRectangle, radius);
        }

        private Region CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            return new Region(path);
        }

        private void AddGlassmorphismEffect(Control control)
        {
            control.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Create glass effect with blur simulation
                using (var brush = new LinearGradientBrush(
                    control.ClientRectangle,
                    Color.FromArgb(40, 255, 255, 255),
                    Color.FromArgb(20, 255, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, control.ClientRectangle);
                }

                // Add border
                using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1))
                {
                    var rect = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }

                // Add subtle inner glow
                using (var innerBrush = new LinearGradientBrush(
                    new Rectangle(1, 1, control.Width - 2, 30),
                    Color.FromArgb(50, 255, 255, 255),
                    Color.FromArgb(10, 255, 255, 255),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(innerBrush, 1, 1, control.Width - 2, 30);
                }
            };
        }

        private void MainEntryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit the application?",
                "Exit Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (backgroundImage != null)
            {
                // Draw background image with proper scaling
                e.Graphics.DrawImage(backgroundImage, 0, 0, this.Width, this.Height);

                // Add dark overlay for better contrast
                using (var overlay = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(overlay, this.ClientRectangle);
                }
            }
            else
            {
                // Fallback gradient background
                using (var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(15, 23, 42),   // Dark slate
                    Color.FromArgb(30, 41, 59),   // Lighter slate
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }

    }
}