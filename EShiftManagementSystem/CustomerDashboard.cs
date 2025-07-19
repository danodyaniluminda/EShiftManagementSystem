
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EShiftManagementSystem.DAL;
using EShiftManagementSystem.Models;

namespace EShiftManagementSystem
{
    public partial class CustomerDashboard : Form
    {
        private readonly DataManager _dataManager;
        private readonly Customer _currentCustomer;
        private readonly EShiftDbContext _context;

        private List<Job> _customerJobs;
        private List<Load> _customerLoads;
        private TabControl tabControl;
        private TabPage tabMyJobs, tabNewJob, tabMyLoads, tabProfile;
        private ListView lvJobs, lvLoads;
        private Button btnNewJob, btnUpdateProfile, btnLogout;
        private Label lblWelcome, lblTotalJobs, lblPendingJobs, lblCompletedJobs, lblTotalCost;

        //  Color Scheme
        private readonly Color PrimaryColor = Color.FromArgb(34, 197, 94);       // Green-500
        private readonly Color SecondaryColor = Color.FromArgb(59, 130, 246);   // Light Blue
        private readonly Color AccentColor = Color.FromArgb(16, 185, 129);      // Emerald
        private readonly Color DangerColor = Color.FromArgb(239, 68, 68);       // Red
        private readonly Color WarningColor = Color.FromArgb(245, 158, 11);     // Amber
        private readonly Color BackgroundColor = Color.FromArgb(240, 253, 244);  // Green-50
        private readonly Color SurfaceColor = Color.White;
        private readonly Color TextPrimary = Color.FromArgb(17, 24, 39);        // Dark Gray
        private readonly Color TextSecondary = Color.FromArgb(107, 114, 128);   // Medium Gray
        private readonly Color BorderColor = Color.FromArgb(229, 231, 235);     // Light Border

        public CustomerDashboard(DataManager dataManager, EShiftDbContext context, Customer currentCustomer)
        {
            _customerJobs = new List<Job>();
            _customerLoads = new List<Load>();

            _dataManager = dataManager;
            _context = context;
            _currentCustomer = currentCustomer;

            InitializeComponent();
            SetupForm();
            LoadCustomerData();
        }

        private void SetupForm()
        {
            this.Text = "e-Shift - Customer Dashboard";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BackgroundColor;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);


            // Set form icon
            this.Icon = CreateDashboardIcon();

            //  Header Panel
            var headerPanel = CreateHeaderPanel();

            //  Statistics Panel
            var statsPanel = CreateStatsPanel();

            //  Tab Control
            tabControl = CreateTabControl();

            CreateMyJobsTab();
            CreateNewJobTab();
            CreateMyLoadsTab();
            CreateProfileTab();

            // Add controls to form
            this.Controls.Add(tabControl);
            this.Controls.Add(statsPanel);
            this.Controls.Add(headerPanel);
        }

        private Icon CreateDashboardIcon()
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw dashboard icon
                using (var brush = new SolidBrush(PrimaryColor))
                {
                    g.FillRectangle(brush, 4, 4, 10, 10);
                    g.FillRectangle(brush, 18, 4, 10, 10);
                    g.FillRectangle(brush, 4, 18, 10, 10);
                    g.FillRectangle(brush, 18, 18, 10, 10);
                }
            }
            return Icon.FromHandle(bitmap.GetHicon());
        }

        private Bitmap CreateTabIcon(string iconType)
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var brush = new SolidBrush(PrimaryColor))
                {
                    switch (iconType)
                    {
                        case "jobs":
                            // Briefcase icon
                            g.FillRectangle(brush, 2, 6, 12, 8);
                            g.FillRectangle(brush, 6, 4, 4, 3);
                            break;
                        case "newJob":
                            // Plus icon
                            g.FillRectangle(brush, 7, 2, 2, 12);
                            g.FillRectangle(brush, 2, 7, 12, 2);
                            break;
                        case "loads":
                            // Truck icon
                            g.FillRectangle(brush, 1, 8, 10, 6);
                            g.FillRectangle(brush, 11, 10, 4, 4);
                            g.FillEllipse(brush, 3, 12, 2, 2);
                            g.FillEllipse(brush, 11, 12, 2, 2);
                            break;
                        case "profile":
                            // User icon
                            g.FillEllipse(brush, 5, 2, 6, 6);
                            g.FillEllipse(brush, 3, 10, 10, 6);
                            break;
                    }
                }
            }
            return bitmap;
        }

        private Panel CreateHeaderPanel()
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = SurfaceColor
            };

            // Add subtle shadow effect
            headerPanel.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(Color.FromArgb(10, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(brush, 0, headerPanel.Height - 1, headerPanel.Width, 1);
                }
            };

            // Logo/Brand section
            var logoPanel = new Panel
            {
                Location = new Point(30, 0),
                Size = new Size(60, 80),
                BackColor = Color.Transparent
            };

            logoPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // Draw e-Shift logo
                using (var brush = new SolidBrush(PrimaryColor))
                {
                    e.Graphics.FillEllipse(brush, 10, 20, 40, 40);
                }
                using (var brush = new SolidBrush(Color.White))
                {
                    var font = new Font("Segoe UI", 16, FontStyle.Bold);
                    e.Graphics.DrawString("eS", font, brush, 18, 28);
                }
            };

            // Welcome section
            var welcomePanel = new Panel
            {
                Location = new Point(100, 0),
                Size = new Size(600, 80),
                BackColor = Color.Transparent
            };

            lblWelcome = new Label
            {
                Text = $"Welcome back, {_currentCustomer.FirstName}!",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 15),
                Size = new Size(500, 35),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Manage your shipments and track your orders",
                Font = new Font("Segoe UI", 10),
                ForeColor = TextSecondary,
                Location = new Point(0, 50),
                Size = new Size(400, 20),
                AutoSize = true
            };

            welcomePanel.Controls.AddRange(new Control[] { lblWelcome, lblSubtitle });

            // Logout button with icon
            btnLogout = CreateButtonWithIcon("Logout", DangerColor, Color.White, "logout");
            btnLogout.Size = new Size(100, 35);
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Click += BtnLogout_Click;
            btnLogout.Location = new Point(0, 25);

            headerPanel.Controls.AddRange(new Control[] { logoPanel, welcomePanel, btnLogout });
            return headerPanel;
        }

        private Panel CreateStatsPanel()
        {
            var statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(30, 20, 30, 20)
            };

            var cardWidth = 270;
            var cardHeight = 80;
            var cardSpacing = 20;

            Color totalJobsColor = Color.MediumSlateBlue;
            Color pendingJobsColor = Color.OrangeRed;
            Color completedJobsColor = Color.SeaGreen;
            Color totalCostColor = Color.Goldenrod;

            var totalJobsCard = CreateStatCard("Total Jobs", "0", totalJobsColor, 0, "📋");
            var pendingJobsCard = CreateStatCard("Pending Jobs", "0", pendingJobsColor, cardWidth + cardSpacing, "⏳");
            var completedJobsCard = CreateStatCard("Completed Jobs", "0", completedJobsColor, (cardWidth + cardSpacing) * 2, "✅");
            var totalCostCard = CreateStatCard("Total Cost", "$0.00", totalCostColor, (cardWidth + cardSpacing) * 3, "💰");

            lblTotalJobs = (Label)totalJobsCard.Controls[2];
            lblPendingJobs = (Label)pendingJobsCard.Controls[2];
            lblCompletedJobs = (Label)completedJobsCard.Controls[2];
            lblTotalCost = (Label)totalCostCard.Controls[2];

            statsPanel.Controls.AddRange(new Control[] { totalJobsCard, pendingJobsCard, completedJobsCard, totalCostCard });
            return statsPanel;
        }


        private Panel CreateStatCard(string title, string value, Color accentColor, int xPosition, string emoji)
        {
            var card = new Panel
            {
                Location = new Point(xPosition, 0),
                Size = new Size(270, 80),
                BackColor = accentColor,
                Padding = new Padding(20, 15, 20, 15)
            };

            var lblEmoji = new Label
            {
                Text = emoji,
                Font = new Font("Segoe UI", 16),
                Location = new Point(20, 15),
                Size = new Size(30, 30),
                AutoSize = true,
                ForeColor = Color.White
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.WhiteSmoke,
                Location = new Point(60, 20),
                Size = new Size(180, 20),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(55, 35),
                Size = new Size(180, 30),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblEmoji, lblTitle, lblValue });
            return card;
        }


        private TabControl CreateTabControl()
        {
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Padding = new Point(25, 8),
                ImageList = new ImageList { ImageSize = new Size(16, 16) }
            };

            // Add icons to ImageList
            tabControl.ImageList.Images.Add("jobs", CreateTabIcon("jobs"));
            tabControl.ImageList.Images.Add("newJob", CreateTabIcon("newJob"));
            tabControl.ImageList.Images.Add("loads", CreateTabIcon("loads"));
            tabControl.ImageList.Images.Add("profile", CreateTabIcon("profile"));

            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += TabControl_DrawItem;

            return tabControl;
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            var tabPage = tabControl.TabPages[e.Index];
            var tabRect = tabControl.GetTabRect(e.Index);

            // Background
            var backColor = e.State == DrawItemState.Selected ? PrimaryColor : SurfaceColor;
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, tabRect);
            }

            // Icon
            if (tabControl.ImageList != null && tabControl.ImageList.Images.Count > e.Index)
            {
                var icon = tabControl.ImageList.Images[e.Index];
                var iconRect = new Rectangle(tabRect.X + 8, tabRect.Y + (tabRect.Height - 16) / 2, 16, 16);
                e.Graphics.DrawImage(icon, iconRect);
            }

            // Text
            var textColor = e.State == DrawItemState.Selected ? Color.White : TextPrimary;
            var textRect = new Rectangle(tabRect.X + 30, tabRect.Y, tabRect.Width - 30, tabRect.Height);
            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabControl.Font, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private Button CreateButton(string text, Color backgroundColor, Color textColor)
        {
            var button = new Button
            {
                Text = text,
                BackColor = backgroundColor,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backgroundColor, 0.1f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backgroundColor, 0.1f);

            return button;
        }

        private Button CreateButtonWithIcon(string text, Color backgroundColor, Color textColor, string iconType)
        {
            var button = CreateButton(text, backgroundColor, textColor);

            button.Paint += (s, e) =>
            {
                // Draw icon on button
                var iconRect = new Rectangle(8, (button.Height - 16) / 2, 16, 16);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var brush = new SolidBrush(textColor))
                {
                    switch (iconType)
                    {
                        case "logout":
                            // Door with arrow icon
                            e.Graphics.DrawRectangle(new Pen(brush, 1), iconRect.X, iconRect.Y, 12, 16);
                            e.Graphics.DrawLine(new Pen(brush, 2), iconRect.X + 6, iconRect.Y + 8, iconRect.X + 10, iconRect.Y + 8);
                            e.Graphics.DrawLine(new Pen(brush, 1), iconRect.X + 8, iconRect.Y + 6, iconRect.X + 10, iconRect.Y + 8);
                            e.Graphics.DrawLine(new Pen(brush, 1), iconRect.X + 8, iconRect.Y + 10, iconRect.X + 10, iconRect.Y + 8);
                            break;
                        case "refresh":
                            // Refresh circular arrow
                            e.Graphics.DrawArc(new Pen(brush, 2), iconRect, 45, 270);
                            break;
                        case "add":
                            // Plus icon
                            e.Graphics.DrawLine(new Pen(brush, 2), iconRect.X + 8, iconRect.Y + 2, iconRect.X + 8, iconRect.Y + 14);
                            e.Graphics.DrawLine(new Pen(brush, 2), iconRect.X + 2, iconRect.Y + 8, iconRect.X + 14, iconRect.Y + 8);
                            break;
                    }
                }
            };

            return button;
        }

        private ListView CreateListView()
        {
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9),
                BackColor = SurfaceColor,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                OwnerDraw = true
            };

            listView.DrawItem += ListView_DrawItem;
            listView.DrawSubItem += ListView_DrawSubItem;
            listView.DrawColumnHeader += ListView_DrawColumnHeader;

            return listView;
        }

        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var backColor = e.ItemIndex % 2 == 0 ? SurfaceColor : Color.FromArgb(248, 250, 252);
            using (SolidBrush backgroundBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            }

            // Add status color indicators
            if (e.ColumnIndex == 4) // Status column for jobs
            {
                var statusColor = GetStatusColor(e.SubItem.Text);
                var statusRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 8, e.Bounds.Height - 4);
                using (var brush = new SolidBrush(statusColor))
                {
                    e.Graphics.FillRectangle(brush, statusRect);
                }
            }

            var textRect = new Rectangle(e.Bounds.X + (e.ColumnIndex == 4 ? 15 : 5), e.Bounds.Y,
                e.Bounds.Width - (e.ColumnIndex == 4 ? 20 : 10), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text,
                new Font("Segoe UI", 9), textRect, TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private Color GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "pending": return WarningColor;
                case "in progress": return SecondaryColor;
                case "completed": return AccentColor;
                case "cancelled": return DangerColor;
                default: return TextSecondary;
            }
        }

        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb(243, 244, 246)))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            TextRenderer.DrawText(e.Graphics, e.Header.Text,
                new Font("Segoe UI", 9, FontStyle.Bold), e.Bounds, TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void CreateMyJobsTab()
        {
            tabMyJobs = new TabPage("My Jobs");
            tabMyJobs.BackColor = BackgroundColor;
            tabMyJobs.Padding = new Padding(30, 20, 30, 20);
            tabMyJobs.ImageIndex = 0;

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                Padding = new Padding(0, 0, 0, 60)
            };

            lvJobs = CreateListView();
            lvJobs.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader { Text = "Job ID", Width = 80 },
                new ColumnHeader { Text = "Start Location", Width = 180 },
                new ColumnHeader { Text = "Destination", Width = 180 },
                new ColumnHeader { Text = "Description", Width = 250 },
                new ColumnHeader { Text = "Status", Width = 120 },
                new ColumnHeader { Text = "Cost", Width = 100 },
                new ColumnHeader { Text = "Request Date", Width = 140 },
                new ColumnHeader { Text = "Schedule Date", Width = 140 }
            });

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = SurfaceColor,
                Padding = new Padding(20, 15, 20, 15)
            };

            btnNewJob = CreateButtonWithIcon("New Job Request", AccentColor, Color.White, "add");
            btnNewJob.Location = new Point(0, 0);
            btnNewJob.Size = new Size(160, 40);
            btnNewJob.Click += (s, e) => tabControl.SelectedTab = tabNewJob;

            var btnRefreshJobs = CreateButtonWithIcon("Refresh", SecondaryColor, Color.White, "refresh");
            btnRefreshJobs.Location = new Point(180, 0);
            btnRefreshJobs.Size = new Size(100, 40);
            btnRefreshJobs.Click += async (s, e) => await LoadCustomerJobs();

            buttonPanel.Controls.AddRange(new Control[] { btnNewJob, btnRefreshJobs });

            contentPanel.Controls.Add(lvJobs);
            tabMyJobs.Controls.Add(buttonPanel);
            tabMyJobs.Controls.Add(contentPanel);
            tabControl.TabPages.Add(tabMyJobs);
        }

        private void CreateNewJobTab()
        {
            tabNewJob = new TabPage("New Job Request");
            tabNewJob.BackColor = BackgroundColor;
            tabNewJob.Padding = new Padding(30, 20, 30, 20);
            tabNewJob.ImageIndex = 1;

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                Padding = new Padding(40, 40, 40, 40),
                AutoScroll = true
            };

            // Add form header with icon
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(600, 70),
                BackColor = Color.Transparent
            };

            headerPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // Draw form icon
                using (var brush = new SolidBrush(AccentColor))
                {
                    e.Graphics.FillRectangle(brush, 0, 10, 40, 40);
                }
                using (var brush = new SolidBrush(Color.White))
                {
                    var font = new Font("Segoe UI", 20, FontStyle.Bold);
                    e.Graphics.DrawString("+", font, brush, 12, 18);
                }
            };

            var lblTitle = new Label
            {
                Text = "Create New Job Request",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(55, 15),
                Size = new Size(400, 35),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Fill in the details below to submit your job request",
                Font = new Font("Segoe UI", 10),
                ForeColor = TextSecondary,
                Location = new Point(55, 45),
                Size = new Size(400, 20),
                AutoSize = true
            };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // Form fields with better styling
            var fields = new[]
            {
                CreateFormField("Start Location", "Enter pickup location", new Point(0, 90), "txtStartLocation"),
                CreateFormField("Destination", "Enter delivery destination", new Point(0, 170), "txtDestination"),
                CreateFormField("Description", "Describe your shipment requirements", new Point(0, 250), "txtDescription", true)
            };

            var lblScheduleDate = new Label
            {
                Text = "Preferred Date",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 390),
                Size = new Size(120, 20),
                AutoSize = true
            };

            var dtpScheduleDate = new DateTimePicker
            {
                Location = new Point(0, 415),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10),
                Name = "dtpScheduleDate"
            };
            dtpScheduleDate.MinDate = DateTime.Now;

            var btnSubmitJob = CreateButtonWithIcon("Submit Job Request", PrimaryColor, Color.White, "add");
            btnSubmitJob.Location = new Point(0, 470);
            btnSubmitJob.Size = new Size(180, 45);
            btnSubmitJob.Click += async (s, e) => await SubmitJobRequest(
                (TextBox)contentPanel.Controls.Find("txtStartLocation", true)[0],
                (TextBox)contentPanel.Controls.Find("txtDestination", true)[0],
                (TextBox)contentPanel.Controls.Find("txtDescription", true)[0],
                dtpScheduleDate);

            var controls = new List<Control> { headerPanel };
            controls.AddRange(fields.SelectMany(f => f));
            controls.AddRange(new Control[] { lblScheduleDate, dtpScheduleDate, btnSubmitJob });

            contentPanel.Controls.AddRange(controls.ToArray());
            tabNewJob.Controls.Add(contentPanel);
            tabControl.TabPages.Add(tabNewJob);
        }

        private Control[] CreateFormField(string labelText, string placeholder, Point location, string name, bool multiline = false)
        {
            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = location,
                Size = new Size(200, 20),
                AutoSize = true
            };

            var textBox = new TextBox
            {
                Location = new Point(location.X, location.Y + 25),
                Size = new Size(400, multiline ? 60 : 30),
                Font = new Font("Segoe UI", 10),
                Name = name,
                Multiline = multiline,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SurfaceColor,
                ForeColor = TextPrimary
            };

            // Add placeholder effect
            textBox.ForeColor = TextSecondary;
            textBox.Text = placeholder;
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = TextPrimary;
                }
            };
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = TextSecondary;
                }
            };

            return new Control[] { label, textBox };
        }

        private void CreateMyLoadsTab()
        {
            tabMyLoads = new TabPage("My Loads");
            tabMyLoads.BackColor = BackgroundColor;
            tabMyLoads.Padding = new Padding(30, 20, 30, 20);
            tabMyLoads.ImageIndex = 2;

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                Padding = new Padding(0, 0, 0, 60)
            };

            lvLoads = CreateListView();
            lvLoads.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader { Text = "Load ID", Width = 80 },
                new ColumnHeader { Text = "Job ID", Width = 80 },
                new ColumnHeader { Text = "Description", Width = 200 },
                new ColumnHeader { Text = "Weight (kg)", Width = 100 },
                new ColumnHeader { Text = "Volume (m³)", Width = 100 },
                new ColumnHeader { Text = "Category", Width = 120 },
                new ColumnHeader { Text = "Status", Width = 100 },
                new ColumnHeader { Text = "Transport Unit", Width = 140 },
                new ColumnHeader { Text = "Created Date", Width = 140 }
            });

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = SurfaceColor,
                Padding = new Padding(20, 15, 20, 15)
            };

            var btnRefreshLoads = CreateButtonWithIcon("Refresh", SecondaryColor, Color.White, "refresh");
            btnRefreshLoads.Location = new Point(0, 0);
            btnRefreshLoads.Size = new Size(100, 40);
            btnRefreshLoads.Click += async (s, e) => await LoadCustomerLoads();

            buttonPanel.Controls.Add(btnRefreshLoads);

            contentPanel.Controls.Add(lvLoads);
            tabMyLoads.Controls.Add(buttonPanel);
            tabMyLoads.Controls.Add(contentPanel);
            tabControl.TabPages.Add(tabMyLoads);
        }


        private void CreateProfileTab()
        {
            tabProfile = new TabPage("My Profile");
            tabProfile.BackColor = BackgroundColor;
            tabProfile.Padding = new Padding(30, 20, 30, 20);
            tabProfile.ImageIndex = 3;

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                Padding = new Padding(40, 40, 40, 40),
                AutoScroll = true
            };

            var lblTitle = new Label
            {
                Text = "My Profile",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(0, 0),
                Size = new Size(200, 35),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Update your personal information",
                Font = new Font("Segoe UI", 10),
                ForeColor = TextSecondary,
                Location = new Point(0, 40),
                Size = new Size(300, 20),
                AutoSize = true
            };

            var profileFields = new[]
            {
                CreateProfileField("First Name", _currentCustomer.FirstName, new Point(0, 80), "txtFirstName"),
                CreateProfileField("Last Name", _currentCustomer.LastName, new Point(0, 140), "txtLastName"),
                CreateProfileField("Email", _currentCustomer.Email, new Point(0, 200), "txtEmail"),
                CreateProfileField("Phone", _currentCustomer.Phone, new Point(0, 260), "txtPhone"),
                CreateProfileField("Address", _currentCustomer.Address, new Point(0, 320), "txtAddress", true)
            };

            btnUpdateProfile = CreateButton("Update Profile", AccentColor, Color.White);
            btnUpdateProfile.Location = new Point(0, 420);
            btnUpdateProfile.Size = new Size(150, 45);
            btnUpdateProfile.Click += async (s, e) => await UpdateProfile(
                (TextBox)contentPanel.Controls.Find("txtFirstName", true)[0],
                (TextBox)contentPanel.Controls.Find("txtLastName", true)[0],
                (TextBox)contentPanel.Controls.Find("txtEmail", true)[0],
                (TextBox)contentPanel.Controls.Find("txtPhone", true)[0],
                (TextBox)contentPanel.Controls.Find("txtAddress", true)[0]);

            var controls = new List<Control> { lblTitle, lblSubtitle };
            controls.AddRange(profileFields.SelectMany(f => f));
            controls.Add(btnUpdateProfile);

            contentPanel.Controls.AddRange(controls.ToArray());
            tabProfile.Controls.Add(contentPanel);
            tabControl.TabPages.Add(tabProfile);
        }

        private Control[] CreateProfileField(string labelText, string value, Point location, string name, bool multiline = false)
        {
            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = location,
                Size = new Size(200, 20),
                AutoSize = true
            };

            var textBox = new TextBox
            {
                Location = new Point(location.X, location.Y + 25),
                Size = new Size(400, multiline ? 60 : 30),
                Font = new Font("Segoe UI", 10),
                Name = name,
                Text = value ?? "",
                Multiline = multiline,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SurfaceColor,
                ForeColor = TextPrimary
            };

            return new Control[] { label, textBox };
        }


        private async void LoadCustomerData()
        {
            await LoadCustomerJobs();
            await LoadCustomerLoads();
            UpdateStatistics();
        }

        private async Task LoadCustomerJobs()
        {
            try
            {
                _customerJobs = await _dataManager.GetJobsByCustomerIdAsync(_currentCustomer.CustomerId);

                lvJobs.Items.Clear();
                foreach (var job in _customerJobs.OrderByDescending(j => j.RequestDate))
                {
                    var item = new ListViewItem(job.JobId.ToString());
                    item.SubItems.Add(job.StartLocation);
                    item.SubItems.Add(job.Destination);
                    item.SubItems.Add(job.Description);
                    item.SubItems.Add(job.Status);
                    item.SubItems.Add(job.Cost.ToString("F2"));
                    item.SubItems.Add(job.RequestDate.ToString("yyyy-MM-dd"));
                    item.SubItems.Add(job.ScheduleDate?.ToString("yyyy-MM-dd") ?? "Not Scheduled");
                    lvJobs.Items.Add(item);
                }
                lvJobs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadCustomerLoads()
        {
            try
            {
                _customerLoads = await _dataManager.GetLoadsByCustomerIdAsync(_currentCustomer.CustomerId);
                var transportUnits = await _dataManager.GetAllTransportUnitsAsync();

                lvLoads.Items.Clear();
                foreach (var load in _customerLoads.OrderByDescending(l => l.CreatedDate))
                {
                    var transportUnit = load.TransportUnitId.HasValue ?
                        transportUnits.FirstOrDefault(u => u.TransportUnitId == load.TransportUnitId) : null;

                    var item = new ListViewItem(load.LoadId.ToString());
                    item.SubItems.Add(load.JobId.ToString());
                    item.SubItems.Add(load.Description);
                    item.SubItems.Add(load.Weight.ToString());
                    item.SubItems.Add(load.Volume.ToString());
                    item.SubItems.Add(load.Category);
                    item.SubItems.Add(load.Status);
                    item.SubItems.Add(transportUnit?.LicensePlate ?? "Not Assigned");
                    item.SubItems.Add(load.CreatedDate.ToString("yyyy-MM-dd"));
                    lvLoads.Items.Add(item);
                }
                lvLoads.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loads: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatistics()
        {
            if (_customerJobs != null)
            {
                lblTotalJobs.Text = $"Total Jobs: {_customerJobs.Count}";
                lblPendingJobs.Text = $"Pending Jobs: {_customerJobs.Count(j => j.Status == "Pending")}";
                lblCompletedJobs.Text = $"Completed Jobs: {_customerJobs.Count(j => j.Status == "Completed")}";
                lblTotalCost.Text = $"Total Cost: ${_customerJobs.Sum(j => j.Cost):F2}";
            }
        }

        private async Task SubmitJobRequest(TextBox txtStartLocation, TextBox txtDestination, TextBox txtDescription, DateTimePicker dtpScheduleDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtStartLocation.Text) || string.IsNullOrWhiteSpace(txtDestination.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var job = new Job
                {
                    CustomerId = _currentCustomer.CustomerId,
                    StartLocation = txtStartLocation.Text.Trim(),
                    Destination = txtDestination.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    Status = "Pending",
                    Cost = 0, // Will be calculated by admin
                    RequestDate = DateTime.Now,
                    ScheduleDate = dtpScheduleDate.Value
                };

                await _dataManager.AddJobAsync(job);
                MessageBox.Show("Job request submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form
                txtStartLocation.Clear();
                txtDestination.Clear();
                txtDescription.Clear();
                dtpScheduleDate.Value = DateTime.Now;

                // Refresh jobs
                await LoadCustomerJobs();

                // Switch to My Jobs tab
                tabControl.SelectedTab = tabMyJobs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting job request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateProfile(TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, TextBox txtAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _currentCustomer.FirstName = txtFirstName.Text.Trim();
                _currentCustomer.LastName = txtLastName.Text.Trim();
                _currentCustomer.Email = txtEmail.Text.Trim();
                _currentCustomer.Phone = txtPhone.Text.Trim();
                _currentCustomer.Address = txtAddress.Text.Trim();

                await _dataManager.UpdateCustomerAsync(_currentCustomer);
                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Update welcome message
                lblWelcome.Text = $"Welcome, {_currentCustomer.FirstName} {_currentCustomer.LastName}!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var loginForm = new LoginForm(_dataManager, _context);
                loginForm.Show();
                this.Close();
            }
        }
    }
}