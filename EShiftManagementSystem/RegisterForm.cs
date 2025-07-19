using System;
using System.Drawing;
using System.Windows.Forms;
using EShiftManagementSystem.DAL;
using EShiftManagementSystem.Models;

namespace EShiftManagementSystem
{
    public partial class RegisterForm : Form
    {
        private readonly DataManager _dataManager;

        private Label lblFirstName, lblLastName, lblEmail, lblPhone, lblAddress, lblUsername, lblPassword;
        private TextBox txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtUsername, txtPassword;
        private Button btnRegister, btnCancel;
        private Panel panelForm;

        public RegisterForm(DataManager dataManager)
        {
            _dataManager = dataManager;
            InitializeComponent();
            SetupForm();
            CreateFormControls();
        }

        private void SetupForm()
        {
            this.Text = "Customer Registration";
            this.Size = new Size(520, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.FromArgb(240, 244, 248);

            panelForm = new Panel
            {
                Size = new Size(480, 560),
                Location = new Point(10, 10),
                BackColor = Color.White,
                AutoScroll = true
            };
            this.Controls.Add(panelForm);
        }

        private void CreateFormControls()
        {
            var startY = 60;
            var gap = 50;

            var lblTitle = new Label
            {
                Text = "Customer Registration",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(70, 10),
                Size = new Size(350, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblFirstName = CreateLabel("First Name:", startY);
            txtFirstName = CreateTextBox(startY);

            lblLastName = CreateLabel("Last Name:", startY + gap);
            txtLastName = CreateTextBox(startY + gap);

            lblEmail = CreateLabel("Email:", startY + gap * 2);
            txtEmail = CreateTextBox(startY + gap * 2);

            lblPhone = CreateLabel("Phone:", startY + gap * 3);
            txtPhone = CreateTextBox(startY + gap * 3);

            lblAddress = CreateLabel("Address:", startY + gap * 4);
            txtAddress = new TextBox
            {
                Location = new Point(170, startY + gap * 4),
                Size = new Size(280, 70),
                Font = new Font("Segoe UI", 12),
                Multiline = true,
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle
            };
            AddTextBoxStyling(txtAddress, "");

            lblUsername = CreateLabel("Username:", startY + gap * 6);
            txtUsername = CreateTextBox(startY + gap * 6);

            lblPassword = CreateLabel("Password:", startY + gap * 7);
            txtPassword = CreateTextBox(startY + gap * 7, true);

            btnRegister = new Button
            {
                Text = "Register",
                Location = new Point(170, startY + gap * 8 + 10),
                Size = new Size(140, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(59, 130, 246),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            AddButtonHoverEffect(btnRegister, Color.FromArgb(37, 99, 235));

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(320, startY + gap * 8 + 10),
                Size = new Size(140, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(59, 130, 246),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(59, 130, 246);
            btnCancel.FlatAppearance.BorderSize = 2;
            btnCancel.Click += (s, e) => this.Close();
            AddButtonHoverEffect(btnCancel, Color.FromArgb(239, 246, 255));

            panelForm.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblFirstName, txtFirstName,
                lblLastName, txtLastName,
                lblEmail, txtEmail,
                lblPhone, txtPhone,
                lblAddress, txtAddress,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                btnRegister, btnCancel
            });
        }

        private Label CreateLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 85, 99),
                Location = new Point(40, y),
                Size = new Size(120, 25)
            };
        }

        private TextBox CreateTextBox(int y, bool isPassword = false)
        {
            var tb = new TextBox
            {
                Location = new Point(170, y),
                Size = new Size(280, 35),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };
            if (isPassword)
                tb.UseSystemPasswordChar = true;
            AddTextBoxStyling(tb, "");
            return tb;
        }

        private void AddTextBoxStyling(TextBox textBox, string placeholder)
        {
            textBox.GotFocus += (s, e) =>
            {
                textBox.BackColor = Color.White;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.Invalidate();
            };

            textBox.LostFocus += (s, e) =>
            {
                textBox.BackColor = Color.FromArgb(249, 250, 251);
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.Invalidate();
            };

            textBox.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(209, 213, 219), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, textBox.Width - 1, textBox.Height - 1);
                }
            };
        }

        private void AddButtonHoverEffect(Button button, Color hoverColor)
        {
            var originalColor = button.BackColor;
            button.MouseEnter += (s, e) => { button.BackColor = hoverColor; };
            button.MouseLeave += (s, e) => { button.BackColor = originalColor; };
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                    string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var customer = new Customer
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    RegistrationDate = DateTime.Now
                };

                await _dataManager.AddCustomerAsync(customer);

                MessageBox.Show("Registration successful! You can now login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
