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
    public partial class UserLoginForm : Form
    {
        private readonly EShiftDbContext _context;
        private readonly DataManager _dataManager;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Button btnBack;
        private PictureBox pbLogo;
        private Panel panelMain;
        private Panel panelLeft;
        private Panel panelRight;

        public UserLoginForm(DataManager dataManager, EShiftDbContext context)
        {
            InitializeComponent();
            SetupCustomControls();
            _dataManager = dataManager;
            _context = context;
        }

        private void SetupCustomControls()
        {
            this.Text = "e-Shift - Customer Login";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 244, 248);

            // Handle form closing
            this.FormClosing += UserLoginForm_FormClosing;

            // Create main panel
            panelMain = new Panel
            {
                Size = new Size(880, 580),
                Location = new Point(10, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            AddRoundedCorners(panelMain, 15);
            AddShadow(panelMain);

            // Left panel for branding
            panelLeft = new Panel
            {
                Size = new Size(400, 580),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(34, 197, 94) // Green theme for users
            };
            AddRoundedCornersLeft(panelLeft, 15);

            // Right panel for form
            panelRight = new Panel
            {
                Size = new Size(480, 580),
                Location = new Point(400, 0),
                BackColor = Color.White
            };
            AddRoundedCornersRight(panelRight, 15);

            // Logo and branding on left panel
            CreateLeftPanelContent();

            // Login form on right panel
            CreateRightPanelContent();

            // Add panels to main panel
            panelMain.Controls.Add(panelLeft);
            panelMain.Controls.Add(panelRight);

            // Add main panel to form
            this.Controls.Add(panelMain);

            // Add close button
            CreateCloseButton();
        }

        private void CreateLeftPanelContent()
        {
            // Company logo placeholder
            pbLogo = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(160, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            AddRoundedCorners(pbLogo, 40);

            // Create a simple logo design
            pbLogo.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(Color.FromArgb(34, 197, 94)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(brush, 20, 20, 40, 40);
                    using (var font = new Font("Segoe UI", 16, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        e.Graphics.DrawString("🚗", font, textBrush, 28, 28);
                    }
                }
            };

            // Title
            var lblTitle = new Label
            {
                Text = "Customer Portal",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(60, 220),
                Size = new Size(300, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle
            var lblSubtitle = new Label
            {
                Text = "Customer Access",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.FromArgb(187, 247, 208),
                Location = new Point(60, 280),
                Size = new Size(280, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Welcome message
            var lblWelcome = new Label
            {
                Text = "Welcome Customer!\nPlease sign in to book your transport",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(134, 239, 172),
                Location = new Point(60, 350),
                Size = new Size(280, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelLeft.Controls.AddRange(new Control[] { pbLogo, lblTitle, lblSubtitle, lblWelcome });
        }

        private void CreateRightPanelContent()
        {
            // Login title
            var lblLoginTitle = new Label
            {
                Text = "Customer Sign In",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(60, 60),
                AutoSize = true // Ensure full text shows
            };

            // Username section
            var lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(60, 130),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new Point(60, 160),
                Size = new Size(360, 35),
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle, // More visible
                ForeColor = Color.Black,
                BackColor = Color.White
            };
            AddTextBoxStyling(txtUsername, "Enter your username");

            // Password section
            var lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(60, 215),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(60, 245),
                Size = new Size(360, 35),
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true,
                ForeColor = Color.Black,
                BackColor = Color.White
            };
            AddTextBoxStyling(txtPassword, "Enter your password");

            // Login button
            btnLogin = new Button
            {
                Text = "Sign In",
                Location = new Point(60, 310),
                Size = new Size(360, 45),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(34, 197, 94),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            AddButtonHoverEffect(btnLogin, Color.FromArgb(22, 163, 74));
            AddRoundedCorners(btnLogin, 8);

            // Register button
            btnRegister = new Button
            {
                Text = "Create Account",
                Location = new Point(60, 370),
                Size = new Size(360, 45),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(34, 197, 94),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderColor = Color.FromArgb(34, 197, 94);
            btnRegister.FlatAppearance.BorderSize = 2;
            btnRegister.Click += BtnRegister_Click;
            AddButtonHoverEffect(btnRegister, Color.FromArgb(240, 253, 244));
            AddRoundedCorners(btnRegister, 8);

            // Back button
            btnBack = new Button
            {
                Text = "← Back to Home",
                Location = new Point(60, 430),
                Size = new Size(360, 45),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(75, 85, 99),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnBack.FlatAppearance.BorderSize = 2;
            btnBack.Click += BtnBack_Click;
            AddButtonHoverEffect(btnBack, Color.FromArgb(249, 250, 251));
            AddRoundedCorners(btnBack, 8);

            // Clear and add controls to panel
            panelRight.Controls.Clear();
            panelRight.Controls.AddRange(new Control[] {
        lblLoginTitle, lblUsername, txtUsername,
        lblPassword, txtPassword,
        btnLogin, btnRegister, btnBack
    });

            // Optional: Enable scroll if content overflows
            panelRight.AutoScroll = true;
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

            panelMain.Controls.Add(btnClose);
            btnClose.BringToFront();
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

        private void BtnBack_Click(object sender, EventArgs e)
        {
            var mainForm = new LoginForm(_dataManager, _context);
            mainForm.Show();
            this.Hide();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm(_dataManager);
            registerForm.ShowDialog();
        }

        private void AddTextBoxStyling(TextBox textBox, string placeholder)
        {
            textBox.Padding = new Padding(15);
            textBox.Height = 35;

            // Add border
            textBox.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(209, 213, 219), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, textBox.Width - 1, textBox.Height - 1);
                }
            };

            // Focus effects
            textBox.GotFocus += (s, e) =>
            {
                textBox.BackColor = Color.White;
                textBox.Invalidate();
            };

            textBox.LostFocus += (s, e) =>
            {
                textBox.BackColor = Color.FromArgb(249, 250, 251);
                textBox.Invalidate();
            };
        }

        private void AddButtonHoverEffect(Button button, Color hoverColor)
        {
            var originalColor = button.BackColor;

            button.MouseEnter += (s, e) =>
            {
                button.BackColor = hoverColor;
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = originalColor;
            };
        }

        private void AddRoundedCorners(Control control, int radius)
        {
            control.Region = CreateRoundedRectangle(control.ClientRectangle, radius);
        }

        private void AddRoundedCornersLeft(Control control, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddLine(radius, 0, control.Width, 0);
            path.AddLine(control.Width, 0, control.Width, control.Height);
            path.AddLine(control.Width, control.Height, radius, control.Height);
            path.AddArc(0, control.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            control.Region = new Region(path);
        }

        private void AddRoundedCornersRight(Control control, int radius)
        {
            var path = new GraphicsPath();
            path.AddLine(0, 0, control.Width - radius, 0);
            path.AddArc(control.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(control.Width - radius * 2, control.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddLine(control.Width - radius, control.Height, 0, control.Height);
            path.AddLine(0, control.Height, 0, 0);
            path.CloseAllFigures();
            control.Region = new Region(path);
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

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    ShowCustomMessageBox("Please enter both username and password.", "Validation Error", MessageBoxIcon.Warning);
                    return;
                }

                var customer = await _dataManager.GetCustomerByUsernameAsync(txtUsername.Text);
                if (customer != null && customer.Password == txtPassword.Text)
                {
                    var customerDashboard = new CustomerDashboard(_dataManager, _context, customer);
                    customerDashboard.Show();
                    this.Hide();
                }
                else
                {
                    ShowCustomMessageBox("Invalid customer credentials.", "Login Error", MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox($"Login error: {ex.Message}", "Error", MessageBoxIcon.Error);
            }
        }

        private void ShowCustomMessageBox(string message, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        private void UserLoginForm_FormClosing(object sender, FormClosingEventArgs e)
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