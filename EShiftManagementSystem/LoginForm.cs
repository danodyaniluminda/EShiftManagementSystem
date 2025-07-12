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
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // Handle form closing
            this.FormClosing += MainEntryForm_FormClosing;

            // Create main panel
            panelMain = new Panel
            {
                Size = new Size(580, 480),
                Location = new Point(10, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            AddRoundedCorners(panelMain, 15);
            AddShadow(panelMain);

            CreateMainContent();

            // Add main panel to form
            this.Controls.Add(panelMain);

            // Add close button
            CreateCloseButton();
        }

        private void CreateMainContent()
        {
            // Company logo
            pbLogo = new PictureBox
            {
                Size = new Size(100, 100),
                Location = new Point(240, 60), // Moved up to prevent overlap
                BackColor = Color.FromArgb(59, 130, 246),
                BorderStyle = BorderStyle.None
            };
            AddRoundedCorners(pbLogo, 50);

            // Create a simple logo design
            pbLogo.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(Color.FromArgb(59, 130, 246)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(brush, 0, 0, 100, 100);
                    using (var font = new Font("Segoe UI", 24, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        var textSize = e.Graphics.MeasureString("eS", font);
                        float x = (100 - textSize.Width) / 2;
                        float y = (100 - textSize.Height) / 2;
                        e.Graphics.DrawString("eS", font, textBrush, x, y);
                    }
                }
            };

            // Title
            var lblTitle = new Label
            {
                Text = "e-Shift",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235), // Brighter blue
                AutoSize = true,
                BackColor = Color.Transparent, // Ensure transparent background
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Calculate proper center position for title
            lblTitle.Location = new Point((panelMain.Width - lblTitle.PreferredWidth) / 2, 180);

            // Subtitle
            var lblSubtitle = new Label
            {
                Text = "Transport Management System",
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                ForeColor = Color.FromArgb(59, 130, 246),
                AutoSize = true,
                BackColor = Color.Transparent, // Ensure transparent background
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Calculate proper center position for subtitle
            lblSubtitle.Location = new Point((panelMain.Width - lblSubtitle.PreferredWidth) / 2, 240);

            // Welcome message
            var lblWelcome = new Label
            {
                Text = "Please select your account type to continue",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(99, 102, 241),
                AutoSize = true,
                BackColor = Color.Transparent, // Ensure transparent background
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Calculate proper center position for welcome message
            lblWelcome.Location = new Point((panelMain.Width - lblWelcome.PreferredWidth) / 2, 280);

            // Admin Login button (Red)
            btnAdminLogin = new Button
            {
                Text = "👤 Admin Login",
                Location = new Point(110, 340),
                Size = new Size(160, 50),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(220, 38, 38), // Red
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdminLogin.FlatAppearance.BorderSize = 0;
            btnAdminLogin.Click += BtnAdminLogin_Click;
            AddButtonHoverEffect(btnAdminLogin, Color.FromArgb(185, 28, 28)); // Darker red on hover
            AddRoundedCorners(btnAdminLogin, 8);

            // User Login button (Green)
            btnUserLogin = new Button
            {
                Text = "🚗 User Login",
                Location = new Point(310, 340),
                Size = new Size(160, 50),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(22, 163, 74), // Green
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUserLogin.FlatAppearance.BorderSize = 0;
            btnUserLogin.Click += BtnUserLogin_Click;
            AddButtonHoverEffect(btnUserLogin, Color.FromArgb(21, 128, 61)); // Darker green on hover
            AddRoundedCorners(btnUserLogin, 8);

            // Add controls to panel in proper order (back to front)
            panelMain.Controls.AddRange(new Control[] {
        pbLogo, lblTitle, lblSubtitle, lblWelcome, btnAdminLogin, btnUserLogin
    });
        }

        private void CreateCloseButton()
        {
            var btnClose = new Button
            {
                Text = "×",
                Location = new Point(panelMain.Width - 40, 10),
                Size = new Size(30, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(107, 114, 128),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;
            AddButtonHoverEffect(btnClose, Color.FromArgb(239, 68, 68));

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

        private void AddButtonHoverEffect(Button button, Color hoverColor)
        {
            var originalColor = button.BackColor;
            var originalForeColor = button.ForeColor;

            button.MouseEnter += (s, e) =>
            {
                button.BackColor = hoverColor;
                if (button == btnUserLogin)
                {
                    button.ForeColor = Color.White;
                }
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

        private void AddShadow(Control control)
        {
            control.Paint += (s, e) =>
            {
                using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, 5, 5, control.Width, control.Height);
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

            using (var brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(240, 244, 248),
                Color.FromArgb(226, 232, 240),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }
    }
}