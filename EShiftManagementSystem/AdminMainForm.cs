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
using System.IO;
using System.Drawing.Printing;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;

namespace EShiftManagementSystem
{

    public partial class AdminMainForm : Form
    {
        private DataManager _dataManager;
        private TabControl _tabControl;
        private Button _logoutButton;
        private Button _refreshButton;
        private readonly EShiftDbContext _context;
        private ListView transportUnitListView;
        private ListView jobListView;
        private ListView loadListView;
        private ListView customerListView;
        private ComboBox jobCustomerComboBox;
        private ComboBox loadJobComboBox;
        private ComboBox loadTransportUnitComboBox;

        private ListView activityListView;

        // PDF styling constants
        private const double MARGIN = 40;
        private const double HEADER_HEIGHT = 60;
        private const double FOOTER_HEIGHT = 30;
        private const double LINE_HEIGHT = 16;
        private const double TABLE_ROW_HEIGHT = 20;

        // Cache for data
        private List<Customer> _customers;
        private List<Job> _jobs;
        private List<Load> _loads;
        private List<TransportUnit> _transportUnits;

        private TabPage adminDashboardTab;
        private TabPage customerTab;
        private TabPage transportTab;
        private TabPage jobTab;
        private TabPage loadTab;


        public AdminMainForm(DataManager dataManager, EShiftDbContext context)
        {
            InitializeComponent();
            _dataManager = dataManager;
            _context = context;
            _customers = new List<Customer>();
            _jobs = new List<Job>();
            _loads = new List<Load>();
            _transportUnits = new List<TransportUnit>();
            SetupAdminForm();
        }

        // Method to refresh all tabs instantly
        private async Task RefreshAllTabsAsync()
        {
            await LoadAllDataAsync();
            await RefreshCustomerListAsync();
            await RefreshTransportUnitListAsync();
            await RefreshJobListAsync();
            await RefreshLoadListAsync();
            await RefreshDashboardAsync();
        }

        private async void SetupAdminForm()
        {
            this.Text = "EShift Management System - Admin Dashboard";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;


            // Create main tab control
            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Font = new Font("Segoe UI", 12F, FontStyle.Bold); // larger, modern font
            _tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabControl.ItemSize = new Size(200, 40); // wider & taller tabs
            _tabControl.DrawItem += TabControl_DrawItem;

            // Create tabs
            adminDashboardTab = await CreateAdminDashboardTabAsync();
            _tabControl.TabPages.Add(adminDashboardTab);

            customerTab = await CreateCustomerManagementTabAsync();
            _tabControl.TabPages.Add(customerTab);

            transportTab = await CreateTransportUnitTabAsync();
            _tabControl.TabPages.Add(transportTab);

            jobTab = await CreateJobManagementTabAsync();
            _tabControl.TabPages.Add(jobTab);

            loadTab = await CreateLoadManagementTabAsync();
            _tabControl.TabPages.Add(loadTab);

            _tabControl.TabPages.Add(CreateReportsTab());

            // Create top panel for admin info and logout
            Panel topPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.Red
            };

            Label adminLabel = new Label
            {
                Text = "Admin Dashboard",
                Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold),
                ForeColor = Color.MistyRose,
                Location = new Point(20, 20),
                Size = new Size(250, 25)
            };

            _logoutButton = new Button
            {
                Text = "Logout",
                Size = new Size(80, 35),
                Location = new Point(1100, 12),
                BackColor = Color.Red,
                ForeColor = Color.White
            };
            _logoutButton.Click += LogoutButton_Click;

            topPanel.Controls.Add(adminLabel);
            topPanel.Controls.Add(_logoutButton);

            // Add controls to form
            this.Controls.Add(_tabControl);
            this.Controls.Add(topPanel);

            // Load initial data
            await LoadAllDataAsync();
        }

        // Custom drawing for colorful tab buttons with selected tab highlight
        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage tabPage = _tabControl.TabPages[e.Index];
            Rectangle tabRect = _tabControl.GetTabRect(e.Index);
            bool isSelected = (_tabControl.SelectedIndex == e.Index);

            // Colors for each tab (cycle if more than list)
            Color[] tabColors = { Color.SkyBlue, Color.LightGreen, Color.LightSalmon, Color.Plum, Color.Khaki, Color.LightCoral };
            Color backColor = tabColors[e.Index % tabColors.Length];
            Color borderColor = Color.DarkGray;

            using (SolidBrush brush = new SolidBrush(isSelected ? ControlPaint.Dark(backColor) : backColor))
            {
                e.Graphics.FillRectangle(brush, tabRect);
            }

            using (Pen pen = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(pen, tabRect);
            }

            TextRenderer.DrawText(
                e.Graphics,
                tabPage.Text,
                _tabControl.Font,
                tabRect,
                isSelected ? Color.White : Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private async Task<TabPage> CreateAdminDashboardTabAsync()
        {
            TabPage tab = new TabPage("Dashboard");

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite,
                AutoScroll = true
            };

            Panel summaryPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(1300, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.GreenYellow
            };

            Label titleLabel = new Label
            {
                Text = "Admin Dashboard - System Overview",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(500, 30)
            };

            var stats = await _dataManager.GetDashboardStatsAsync();
            var revenue = await _dataManager.GetTotalRevenueAsync();

            CreateSummaryCard(summaryPanel, "Total Customers", stats["TotalCustomers"].ToString(), Color.Blue, new Point(50, 50));
            CreateSummaryCard(summaryPanel, "Active Jobs", stats["ActiveJobs"].ToString(), Color.Orange, new Point(250, 50));
            CreateSummaryCard(summaryPanel, "Transport Units", stats["TransportUnits"].ToString(), Color.Green, new Point(450, 50));
            CreateSummaryCard(summaryPanel, "Completed Jobs", stats["CompletedJobs"].ToString(), Color.Purple, new Point(650, 50));
            CreateSummaryCard(summaryPanel, "Total Revenue", $"Rs.{revenue:F2}", Color.Red, new Point(850, 50));

            Panel activityPanel = new Panel
            {
                Location = new Point(20, 190),
                Size = new Size(1300, 380),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label activityTitle = new Label
            {
                Text = "Recent Activity",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(250, 25)
            };

            ListView activityList = new ListView
            {
                Location = new Point(10, 40),
                Size = new Size(1280, 320),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                OwnerDraw = true
            };

            activityList.DrawColumnHeader += ListView_DrawColumnHeader;
            activityList.DrawItem += ListView_DrawItem;
            activityList.DrawSubItem += ListView_DrawSubItem;
            activityList.Resize += (s, e) => AdjustLastColumnWidth(activityList);

            activityList.Columns.Add("Date", 150);
            activityList.Columns.Add("Activity", 200);
            activityList.Columns.Add("Details", 600);
            activityList.Columns.Add("Status", 100);

            var recentJobs = await _dataManager.GetRecentJobsAsync();

            foreach (var job in recentJobs)
            {
                string date = job.CreatedDate.ToString("yyyy-MM-dd");
                string activity = "Job Created";
                string details = $"From {job.StartLocation} to {job.Destination}";
                string status = job.Status;

                ListViewItem item = new ListViewItem(date);
                item.SubItems.Add(activity);
                item.SubItems.Add(details);
                item.SubItems.Add(status);

                activityList.Items.Add(item);
            }

            summaryPanel.Controls.Add(titleLabel);
            activityPanel.Controls.Add(activityTitle);
            activityPanel.Controls.Add(activityList);

            mainPanel.Controls.Add(summaryPanel);
            mainPanel.Controls.Add(activityPanel);

            tab.Controls.Add(mainPanel);

            return tab;
        }


        private void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var headerBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            {
                e.Graphics.FillRectangle(headerBrush, e.Bounds);
            }

            TextRenderer.DrawText(e.Graphics, e.Header.Text,
                new Font("Segoe UI", 10, FontStyle.Bold), e.Bounds,
                Color.Black, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Color bgColor = e.ItemIndex % 2 == 0 ? Color.White : Color.FromArgb(245, 245, 245);
            using (SolidBrush backgroundBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            }

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text,
                new Font("Segoe UI", 9), e.Bounds, Color.Black,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void AdjustLastColumnWidth(ListView listView)
        {
            if (listView.Columns.Count < 2) return;

            int totalWidth = listView.ClientSize.Width;
            int usedWidth = 0;

            for (int i = 0; i < listView.Columns.Count - 1; i++)
                usedWidth += listView.Columns[i].Width;

            int lastColWidth = totalWidth - usedWidth - 4;
            if (lastColWidth > 100)
                listView.Columns[listView.Columns.Count - 1].Width = lastColWidth;
        }


        private void CreateSummaryCard(Panel parent, string title, string value, Color color, Point location)
        {
            Panel card = new Panel
            {
                Location = location,
                Size = new Size(150, 80),
                BackColor = color,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(130, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label valueLabel = new Label
            {
                Text = value,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold),
                Location = new Point(10, 35),
                Size = new Size(130, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Name = $"card_{title.Replace(" ", "_")}"
            };

            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            parent.Controls.Add(card);
        }


        private async Task<TabPage> CreateCustomerManagementTabAsync()
        {
            TabPage tab = new TabPage("Customer Management");

            // Main container panel to hold list and form vertically
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite
            };

            // Search panel at the top
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.AntiqueWhite,
                Padding = new Padding(5)
            };

            Label lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                Size = new Size(60, 23),
                Font = new Font("Segoe UI", 10F)
            };

            TextBox txtSearch = new TextBox
            {
                Location = new Point(70, 12),
                Size = new Size(300, 23),
                Name = "txtSearch",
                Font = new Font("Segoe UI", 10F)
            };

            Button btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(380, 10),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };

            Button btnClearSearch = new Button
            {
                Text = "Clear",
                Location = new Point(470, 10),
                Size = new Size(60, 27),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };

            ComboBox cmbSearchBy = new ComboBox
            {
                Location = new Point(540, 12),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cmbSearchBy.Items.AddRange(new string[] { "All Fields", "Name", "Email", "Phone", "Customer ID" });
            cmbSearchBy.SelectedIndex = 0;

            searchPanel.Controls.AddRange(new Control[]
            {
        lblSearch, txtSearch, btnSearch, btnClearSearch, cmbSearchBy
            });

            customerListView = new ListView
            {
                Dock = DockStyle.Top,
                Height = 350,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                OwnerDraw = true
            };

            customerListView.DrawColumnHeader += ListView_DrawColumnHeader;
            customerListView.DrawItem += ListView_DrawItem;
            customerListView.DrawSubItem += ListView_DrawSubItem;

            customerListView.Columns.Add("Customer ID", 100);
            customerListView.Columns.Add("Name", 150);
            customerListView.Columns.Add("Email", 200);
            customerListView.Columns.Add("Phone", 120);
            customerListView.Columns.Add("Registration Date", 130);

            // Form panel below the ListView
            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "Add / Edit Customer",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };

            int yStart = 50;
            int spacing = 35;

            Label lblFirstName = new Label { Text = "First Name:", Location = new Point(10, yStart), Size = new Size(100, 23) };
            TextBox txtFirstName = new TextBox { Location = new Point(120, yStart), Size = new Size(200, 23), Name = "txtFirstName" };

            Label lblLastName = new Label { Text = "Last Name:", Location = new Point(340, yStart), Size = new Size(100, 23) };
            TextBox txtLastName = new TextBox { Location = new Point(450, yStart), Size = new Size(200, 23), Name = "txtLastName" };

            yStart += spacing;

            Label lblEmail = new Label { Text = "Email:", Location = new Point(10, yStart), Size = new Size(100, 23) };
            TextBox txtEmail = new TextBox { Location = new Point(120, yStart), Size = new Size(280, 23), Name = "txtEmail" };

            Label lblPhone = new Label { Text = "Phone:", Location = new Point(420, yStart), Size = new Size(100, 23) };
            TextBox txtPhone = new TextBox { Location = new Point(530, yStart), Size = new Size(120, 23), Name = "txtPhone" };

            yStart += spacing;

            Label lblAddress = new Label { Text = "Address:", Location = new Point(10, yStart), Size = new Size(100, 23) };
            TextBox txtAddress = new TextBox { Location = new Point(120, yStart), Size = new Size(530, 60), Multiline = true, Name = "txtAddress", ScrollBars = ScrollBars.Vertical };

            yStart += 70;

            Label lblPassword = new Label { Text = "Password:", Location = new Point(10, yStart), Size = new Size(100, 23) };
            TextBox txtPassword = new TextBox { Location = new Point(120, yStart), Size = new Size(200, 23), Name = "txtPassword", UseSystemPasswordChar = true };


            Button btnAddCustomer = new Button
            {
                Text = "Add Customer",
                Location = new Point(340, yStart),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnUpdateCustomer = new Button
            {
                Text = "Update",
                Location = new Point(460, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnDeleteCustomer = new Button
            {
                Text = "Delete",
                Location = new Point(550, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Event handlers for buttons
            btnAddCustomer.Click += async (s, e) =>
            {
                await AddCustomerAsync(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword, customerListView);
                ClearCustomerForm(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword, customerListView);
            };

            btnUpdateCustomer.Click += async (s, e) =>
            {
                await UpdateCustomerAsync(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword, customerListView);
                ClearCustomerForm(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword, customerListView);
            };

            btnDeleteCustomer.Click += async (s, e) =>
            {
                await DeleteCustomerAsync(customerListView);
                ClearCustomerForm(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword, customerListView);
            };

            // Search event handlers
            btnSearch.Click += async (s, e) =>
            {
                await SearchCustomersAsync(txtSearch.Text.Trim(), cmbSearchBy.SelectedItem.ToString());
            };

            btnClearSearch.Click += async (s, e) =>
            {
                txtSearch.Clear();
                cmbSearchBy.SelectedIndex = 0;
                await RefreshCustomerListAsync();
            };

            // Enable search on Enter key press
            txtSearch.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    await SearchCustomersAsync(txtSearch.Text.Trim(), cmbSearchBy.SelectedItem.ToString());
                }
            };


            customerListView.SelectedIndexChanged += (s, e) =>
                LoadCustomerForEdit(customerListView, txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword);

            formPanel.Controls.AddRange(new Control[]
            {
        lblTitle,
        lblFirstName, txtFirstName,
        lblLastName, txtLastName,
        lblEmail, txtEmail,
        lblPhone, txtPhone,
        lblAddress, txtAddress,
        lblPassword, txtPassword,
        btnAddCustomer, btnUpdateCustomer, btnDeleteCustomer
            });

            // Add controls to the main panel in order (search panel first)
            mainPanel.Controls.Add(formPanel);
            mainPanel.Controls.Add(customerListView);
            mainPanel.Controls.Add(searchPanel);

            // Add main panel to the tab
            tab.Controls.Add(mainPanel);

            await RefreshCustomerListAsync();

            return tab;
        }

        // Add this method to handle the search functionality
        private async Task SearchCustomersAsync(string searchTerm, string searchBy)
        {
            try
            {
                customerListView.Items.Clear();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    await RefreshCustomerListAsync();
                    return;
                }

                // Search through existing ListView items instead of database query
                // First, refresh the full list to ensure we have all data
                await RefreshCustomerListAsync();

                // Store all items temporarily
                List<ListViewItem> allItems = new List<ListViewItem>();
                foreach (ListViewItem item in customerListView.Items)
                {
                    allItems.Add((ListViewItem)item.Clone());
                }

                // Clear and filter
                customerListView.Items.Clear();

                foreach (ListViewItem item in allItems)
                {
                    bool matchFound = false;

                    switch (searchBy)
                    {
                        case "Name":
                            matchFound = item.SubItems[1].Text.ToLower().Contains(searchTerm.ToLower());
                            break;
                        case "Email":
                            matchFound = item.SubItems[2].Text.ToLower().Contains(searchTerm.ToLower());
                            break;
                        case "Phone":
                            matchFound = item.SubItems[3].Text.Contains(searchTerm);
                            break;
                        case "Customer ID":
                            matchFound = item.SubItems[0].Text.Contains(searchTerm);
                            break;
                        case "All Fields":
                        default:
                            matchFound = item.SubItems[0].Text.Contains(searchTerm) ||
                                       item.SubItems[1].Text.ToLower().Contains(searchTerm.ToLower()) ||
                                       item.SubItems[2].Text.ToLower().Contains(searchTerm.ToLower()) ||
                                       item.SubItems[3].Text.Contains(searchTerm);
                            break;
                    }

                    if (matchFound)
                    {
                        customerListView.Items.Add(item);
                    }
                }

                // Update status or show message if no results found
                if (customerListView.Items.Count == 0)
                {
                    ListViewItem noResultItem = new ListViewItem("No results found");
                    noResultItem.SubItems.Add("");
                    noResultItem.SubItems.Add("");
                    noResultItem.SubItems.Add("");
                    noResultItem.SubItems.Add("");
                    customerListView.Items.Add(noResultItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customers: {ex.Message}", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper to clear form fields
        private void ClearCustomerForm(TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail,
            TextBox txtPhone, TextBox txtAddress, TextBox txtPassword, ListView listView)
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtPassword.Clear();
            listView.SelectedItems.Clear();
        }




        private async Task<TabPage> CreateTransportUnitTabAsync()
        {
            TabPage tab = new TabPage("Transport Units");

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite
            };

            transportUnitListView = new ListView
            {
                Dock = DockStyle.Top,
                Height = 350,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                OwnerDraw = true
            };

            transportUnitListView.DrawColumnHeader += ListView_DrawColumnHeader;
            transportUnitListView.DrawItem += ListView_DrawItem;
            transportUnitListView.DrawSubItem += ListView_DrawSubItem;

            transportUnitListView.Columns.Add("Unit ID", 80);
            transportUnitListView.Columns.Add("Type", 100);
            transportUnitListView.Columns.Add("License Plate", 120);
            transportUnitListView.Columns.Add("Max Weight", 100);
            transportUnitListView.Columns.Add("Max Volume", 100);
            transportUnitListView.Columns.Add("Driver Name", 150);
            transportUnitListView.Columns.Add("Driver Phone", 150);
            transportUnitListView.Columns.Add("Assistant Name", 150);
            transportUnitListView.Columns.Add("Available", 80);
            transportUnitListView.Columns.Add("Created Date", 150);

            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "Transport Unit Management",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };

            int yStart = 50;
            int spacing = 35;

            Label lblUnitType = new Label { Text = "Unit Type:", Location = new Point(10, yStart), Size = new Size(110, 23) };
            ComboBox cmbUnitType = new ComboBox { Location = new Point(150, yStart), Size = new Size(150, 23), Name = "cmbUnitType" };
            cmbUnitType.Items.AddRange(new string[] { "Truck", "Van", "Trailer", "Container" });

            Label lblLicensePlate = new Label { Text = "License Plate:", Location = new Point(330, yStart), Size = new Size(120, 23) };
            TextBox txtLicensePlate = new TextBox { Location = new Point(460, yStart), Size = new Size(150, 23), Name = "txtLicensePlate" };

            yStart += spacing;
            Label lblMaxWeight = new Label { Text = "Max Weight:", Location = new Point(10, yStart), Size = new Size(110, 23) };
            NumericUpDown numMaxWeight = new NumericUpDown { Location = new Point(150, yStart), Size = new Size(100, 23), Name = "numMaxWeight", Maximum = 50000, DecimalPlaces = 2 };

            Label lblMaxVolume = new Label { Text = "Max Volume:", Location = new Point(330, yStart), Size = new Size(110, 23) };
            NumericUpDown numMaxVolume = new NumericUpDown { Location = new Point(460, yStart), Size = new Size(100, 23), Name = "numMaxVolume", Maximum = 10000, DecimalPlaces = 2 };

            yStart += spacing;
            Label lblDriverName = new Label { Text = "Driver Name:", Location = new Point(10, yStart), Size = new Size(120, 23) };
            TextBox txtDriverName = new TextBox { Location = new Point(150, yStart), Size = new Size(180, 23) };

            Label lblDriverPhone = new Label { Text = "Driver Phone:", Location = new Point(330, yStart), Size = new Size(120, 23) };
            TextBox txtDriverPhone = new TextBox { Location = new Point(460, yStart), Size = new Size(150, 23) };


            yStart += spacing;
            Label lblAssistantName = new Label { Text = "Assistant Name:", Location = new Point(10, yStart), Size = new Size(135, 23) };
            TextBox txtAssistantName = new TextBox { Location = new Point(150, yStart), Size = new Size(180, 23), Name = "txtAssistantName" };

            yStart += spacing;
            CheckBox chkAvailable = new CheckBox { Text = "Available", Location = new Point(150, yStart), Size = new Size(110, 23), Name = "chkAvailable", Checked = true };


            Button btnAddUnit = new Button
            {
                Text = "Add Unit",
                Location = new Point(270, yStart),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnUpdateUnit = new Button
            {
                Text = "Update",
                Location = new Point(390, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnDeleteUnit = new Button
            {
                Text = "Delete",
                Location = new Point(480, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnAddUnit.Click += async (s, e) =>
            {
                await AddTransportUnitAsync(cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume,
                    txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable, transportUnitListView);

                ClearTransportUnitForm(cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume,
                    txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable);

                await RefreshTransportUnitListAsync();
            };

            btnUpdateUnit.Click += async (s, e) =>
            {
                await UpdateTransportUnitAsync(cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume,
                    txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable, transportUnitListView);

                ClearTransportUnitForm(cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume,
                    txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable);

                await RefreshTransportUnitListAsync();
            };

            btnDeleteUnit.Click += async (s, e) =>
            {
                await DeleteTransportUnitAsync(transportUnitListView);

                ClearTransportUnitForm(cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume,
                    txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable);

                await RefreshTransportUnitListAsync();
            };

            transportUnitListView.SelectedIndexChanged += (s, e) =>
                LoadTransportUnitForEdit(transportUnitListView, cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume,
                    txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable);

            formPanel.Controls.AddRange(new Control[]
            {
        lblTitle,
        lblUnitType, cmbUnitType,
        lblLicensePlate, txtLicensePlate,
        lblMaxWeight, numMaxWeight,
        lblMaxVolume, numMaxVolume,
        lblDriverName, txtDriverName,
        lblDriverPhone, txtDriverPhone,
        lblAssistantName, txtAssistantName,
        chkAvailable,
        btnAddUnit, btnUpdateUnit, btnDeleteUnit
            });

            mainPanel.Controls.Add(formPanel);
            mainPanel.Controls.Add(transportUnitListView);
            tab.Controls.Add(mainPanel);

            await RefreshTransportUnitListAsync();

            return tab;
        }


        private async Task<TabPage> CreateJobManagementTabAsync()
        {
            TabPage tab = new TabPage("Job Management");

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite
            };

            jobListView = new ListView
            {
                Dock = DockStyle.Top,
                Height = 350,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                OwnerDraw = true
            };

            jobListView.DrawColumnHeader += ListView_DrawColumnHeader;
            jobListView.DrawItem += ListView_DrawItem;
            jobListView.DrawSubItem += ListView_DrawSubItem;

            jobListView.Columns.Add("Job ID", 80);
            jobListView.Columns.Add("Customer", 150);
            jobListView.Columns.Add("Start Location", 150);
            jobListView.Columns.Add("Destination", 150);
            jobListView.Columns.Add("Description", 250);
            jobListView.Columns.Add("Request Date", 120);
            jobListView.Columns.Add("Schedule Date", 120);
            jobListView.Columns.Add("Status", 100);
            jobListView.Columns.Add("Cost", 100);
            jobListView.Columns.Add("Created Date", 120);

            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "Job Management",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };

            int yStart = 50;
            int spacing = 35;

            Label lblCustomer = new Label { Text = "Customer:", Location = new Point(10, yStart), Size = new Size(120, 23) };
            ComboBox cmbCustomer = new ComboBox { Location = new Point(140, yStart), Size = new Size(180, 23), Name = "cmbCustomer", DropDownStyle = ComboBoxStyle.DropDownList };
            Label lblStartLocation = new Label { Text = "Start Location:", Location = new Point(340, yStart), Size = new Size(120, 23) };
            TextBox txtStartLocation = new TextBox { Location = new Point(470, yStart), Size = new Size(180, 23), Name = "txtStartLocation" };
            jobCustomerComboBox = cmbCustomer;
            yStart += spacing;

            Label lblDestination = new Label { Text = "Destination:", Location = new Point(10, yStart), Size = new Size(120, 23) };
            TextBox txtDestination = new TextBox { Location = new Point(140, yStart), Size = new Size(180, 23), Name = "txtDestination" };
            Label lblRequestDate = new Label { Text = "Request Date:", Location = new Point(340, yStart), Size = new Size(120, 23) };
            DateTimePicker dtpRequestDate = new DateTimePicker { Location = new Point(470, yStart), Size = new Size(180, 23), Name = "dtpRequestDate" };
            yStart += spacing;

            Label lblStatus = new Label { Text = "Status:", Location = new Point(10, yStart), Size = new Size(120, 23) };
            ComboBox cmbStatus = new ComboBox { Location = new Point(140, yStart), Size = new Size(180, 23), Name = "cmbStatus", DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new string[] { "Pending", "Accepted", "In Progress", "Completed", "Cancelled" });
            Label lblCost = new Label { Text = "Cost:", Location = new Point(340, yStart), Size = new Size(120, 23) };
            NumericUpDown numCost = new NumericUpDown { Location = new Point(470, yStart), Size = new Size(180, 23), Name = "numCost", Maximum = 100000, DecimalPlaces = 2 };
            yStart += spacing;

            Label lblDescription = new Label { Text = "Description:", Location = new Point(10, yStart), Size = new Size(120, 23) };
            TextBox txtDescription = new TextBox { Location = new Point(140, yStart), Size = new Size(510, 60), Multiline = true, Name = "txtDescription" };
            yStart += 70;


            Button btnAddJob = new Button
            {
                Text = "Add Job",
                Location = new Point(320, yStart),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnUpdateJob = new Button
            {
                Text = "Update",
                Location = new Point(430, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnDeleteJob = new Button
            {
                Text = "Delete",
                Location = new Point(520, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Event handlers
            btnAddJob.Click += async (s, e) =>
            {
                await AddJobAsync(cmbCustomer, txtStartLocation, txtDestination, txtDescription, dtpRequestDate, cmbStatus, numCost, jobListView);
            };

            btnUpdateJob.Click += async (s, e) =>
            {
                await UpdateJobAsync(cmbCustomer, txtStartLocation, txtDestination, txtDescription, dtpRequestDate, cmbStatus, numCost, jobListView);
            };

            btnDeleteJob.Click += async (s, e) =>
            {
                await DeleteJobAsync(jobListView);
            };

            jobListView.SelectedIndexChanged += (s, e) =>
                LoadJobForEdit(jobListView, cmbCustomer, txtStartLocation, txtDestination, txtDescription, dtpRequestDate, cmbStatus, numCost);

            // Add controls
            formPanel.Controls.AddRange(new Control[]
            {
        lblTitle,
        lblCustomer, cmbCustomer,
        lblStartLocation, txtStartLocation,
        lblDestination, txtDestination,
        lblDescription, txtDescription,
        lblRequestDate, dtpRequestDate,
        lblStatus, cmbStatus,
        lblCost, numCost,
        btnAddJob, btnUpdateJob, btnDeleteJob
            });

            mainPanel.Controls.Add(formPanel);
            mainPanel.Controls.Add(jobListView);
            tab.Controls.Add(mainPanel);

            var customers = await _dataManager.GetAllCustomersAsync();
            cmbCustomer.DataSource = customers;
            cmbCustomer.DisplayMember = "FullName";
            cmbCustomer.ValueMember = "CustomerId";

            await RefreshJobListAsync();

            return tab;
        }



        private async Task<TabPage> CreateLoadManagementTabAsync()
        {
            TabPage tab = new TabPage("Load Management");

            // Main container panel for vertical layout
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite
            };

            loadListView = new ListView
            {
                Dock = DockStyle.Top,
                Height = 350,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                OwnerDraw = true
            };

            // If you have owner-draw event handlers like in your ListView, attach them here
            loadListView.DrawColumnHeader += ListView_DrawColumnHeader;
            loadListView.DrawItem += ListView_DrawItem;
            loadListView.DrawSubItem += ListView_DrawSubItem;

            loadListView.Columns.Add("Load ID", 80);
            loadListView.Columns.Add("Job ID", 250);
            loadListView.Columns.Add("Transport Unit", 120);
            loadListView.Columns.Add("Description", 150);
            loadListView.Columns.Add("Weight", 80);
            loadListView.Columns.Add("Volume", 80);
            loadListView.Columns.Add("Category", 100);
            loadListView.Columns.Add("Status", 100);
            loadListView.Columns.Add("Created Date", 100);
            loadListView.Columns.Add("Start Location", 100);

            // Form panel below the ListView
            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.AntiqueWhite,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "Load Management",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(300, 30)
            };

            int yStart = 50;
            int spacing = 35;

            Label lblJob = new Label { Text = "Job:", Location = new Point(10, yStart), Size = new Size(100, 23) };
            ComboBox cmbJob = new ComboBox { Location = new Point(140, yStart), Size = new Size(200, 23), Name = "cmbJob", DropDownStyle = ComboBoxStyle.DropDownList };

            loadJobComboBox = cmbJob;

            Label lblTransportUnit = new Label { Text = "Transport Unit:", Location = new Point(340, yStart), Size = new Size(130, 23) };
            ComboBox cmbTransportUnit = new ComboBox { Location = new Point(470, yStart), Size = new Size(200, 23), Name = "cmbTransportUnit", DropDownStyle = ComboBoxStyle.DropDownList };

            loadTransportUnitComboBox = cmbTransportUnit;

            yStart += spacing;
            Label lblDescription = new Label { Text = "Description:", Location = new Point(10, yStart), Size = new Size(120, 23) };
            TextBox txtLoadDescription = new TextBox { Location = new Point(140, yStart), Size = new Size(530, 60), Multiline = true, Name = "txtLoadDescription" };

            yStart += 70; // Extra space for multiline textbox
            Label lblWeight = new Label { Text = "Weight (kg):", Location = new Point(10, yStart), Size = new Size(120, 23) };
            NumericUpDown numWeight = new NumericUpDown { Location = new Point(140, yStart), Size = new Size(100, 23), Name = "numWeight", Maximum = 50000, DecimalPlaces = 2 };

            Label lblVolume = new Label { Text = "Volume (m³):", Location = new Point(320, yStart), Size = new Size(120, 23) };
            NumericUpDown numVolume = new NumericUpDown { Location = new Point(450, yStart), Size = new Size(100, 23), Name = "numVolume", Maximum = 10000, DecimalPlaces = 2 };

            yStart += spacing;
            Label lblCategory = new Label { Text = "Category:", Location = new Point(10, yStart), Size = new Size(120, 23) };
            ComboBox cmbCategory = new ComboBox { Location = new Point(140, yStart), Size = new Size(150, 23), Name = "cmbCategory" };
            cmbCategory.Items.AddRange(new string[] { "Furniture", "Electronics", "Clothing", "Books", "Fragile", "Heavy", "Other" });

            Label lblLoadStatus = new Label { Text = "Status:", Location = new Point(320, yStart), Size = new Size(120, 23) };
            ComboBox cmbLoadStatus = new ComboBox { Location = new Point(450, yStart), Size = new Size(150, 23), Name = "cmbLoadStatus", DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLoadStatus.Items.AddRange(new string[] { "Assigned", "Delivered" });

            yStart += spacing;
            // Buttons with modern flat style and colors
            Button btnAddLoad = new Button
            {
                Text = "Add Load",
                Location = new Point(320, yStart),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnUpdateLoad = new Button
            {
                Text = "Update",
                Location = new Point(440, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnDeleteLoad = new Button
            {
                Text = "Delete",
                Location = new Point(530, yStart),
                Size = new Size(80, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Event handlers
            btnAddLoad.Click += async (s, e) =>
            {
                await AddLoadAsync(cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus, loadListView);
                ClearLoadForm(cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus);
            };

            btnUpdateLoad.Click += async (s, e) =>
            {
                await UpdateLoadAsync(cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus, loadListView);
                ClearLoadForm(cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus);
            };

            btnDeleteLoad.Click += async (s, e) =>
            {
                await DeleteLoadAsync(loadListView);
                ClearLoadForm(cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus);
            };

            loadListView.SelectedIndexChanged += (s, e) =>
                LoadLoadForEdit(loadListView, cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus);

            // Add controls to form panel
            formPanel.Controls.AddRange(new Control[]
            {
        lblTitle,
        lblJob, cmbJob,
        lblTransportUnit, cmbTransportUnit,
        lblDescription, txtLoadDescription,
        lblWeight, numWeight,
        lblVolume, numVolume,
        lblCategory, cmbCategory,
        lblLoadStatus, cmbLoadStatus,
        btnAddLoad, btnUpdateLoad, btnDeleteLoad
            });

            // Add ListView and formPanel to main container panel
            mainPanel.Controls.Add(formPanel);
            mainPanel.Controls.Add(loadListView);
            tab.Controls.Add(mainPanel);

            // Load data into combo boxes
            var jobs = (await _dataManager.GetAllJobsAsync())
                .Where(j => j.Status == "Accepted")
                .ToList();

            cmbJob.DataSource = jobs;
            cmbJob.DisplayMember = "DisplayName";
            cmbJob.ValueMember = "JobId";


            var units = (await _dataManager.GetAllTransportUnitsAsync())
                .Where(u => u.IsAvailable)
                .ToList();

            cmbTransportUnit.DataSource = units;
            cmbTransportUnit.DisplayMember = "DriverName";
            cmbTransportUnit.ValueMember = "TransportUnitId";

            await RefreshLoadListAsync();

            return tab;
        }

        private TabPage CreateReportsTab()
        {
            TabPage tab = new TabPage("Reports");

            Panel reportPanel = new Panel
            {
                Size = new Size(300, 450),
                Location = new Point(20, 20),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.AntiqueWhite
            };

            Label lblTitle = new Label
            {
                Text = "Reports & Analytics",
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(250, 30)
            };

            Button btnCustomerReport = new Button
            {
                Text = "Generate Customer Report",
                Location = new Point(10, 60),
                Size = new Size(180, 30),
                BackColor = Color.DarkBlue,
                ForeColor = Color.White
            };

            Button btnSaveCustomerPdf = new Button
            {
                Text = "Save as PDF",
                Location = new Point(200, 60),
                Size = new Size(80, 30),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black
            };

            Button btnJobReport = new Button
            {
                Text = "Generate Job Report",
                Location = new Point(10, 110),
                Size = new Size(180, 30),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White
            };

            Button btnSaveJobPdf = new Button
            {
                Text = "Save as PDF",
                Location = new Point(200, 110),
                Size = new Size(80, 30),
                BackColor = Color.LightGreen,
                ForeColor = Color.Black
            };

            Button btnRevenueReport = new Button
            {
                Text = "Generate Revenue Report",
                Location = new Point(10, 160),
                Size = new Size(180, 30),
                BackColor = Color.DarkRed,
                ForeColor = Color.White
            };

            Button btnSaveRevenuePdf = new Button
            {
                Text = "Save as PDF",
                Location = new Point(200, 160),
                Size = new Size(80, 30),
                BackColor = Color.IndianRed,
                ForeColor = Color.Black
            };

            Panel reportDisplayPanel = new Panel
            {
                Location = new Point(340, 20),
                Size = new Size(800, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.AntiqueWhite
            };

            RichTextBox rtbReportDisplay = new RichTextBox
            {
                Location = new Point(5, 5),
                Size = new Size(790, 490),
                Name = "rtbReportDisplay",
                ReadOnly = true,
                Font = new Font("Courier New", 9F),
                BackColor = Color.AntiqueWhite,
                BorderStyle = BorderStyle.None
            };

            btnCustomerReport.Click += async (s, e) =>
            {
                var reportText = await GenerateCustomerReportTextAsync();
                rtbReportDisplay.Text = reportText;
            };

            btnJobReport.Click += async (s, e) =>
            {
                var reportText = await GenerateJobReportTextAsync();
                rtbReportDisplay.Text = reportText;
            };

            btnRevenueReport.Click += async (s, e) =>
            {
                var reportText = await GenerateRevenueReportTextAsync();
                rtbReportDisplay.Text = reportText;
            };

            btnSaveCustomerPdf.Click += async (s, e) =>
            {
                await LoadAllDataAsync();
                SaveCustomerReportToPdf($"CustomerReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            };

            btnSaveJobPdf.Click += async (s, e) =>
            {
                await LoadAllDataAsync();
                SaveJobReportToPdf($"JobReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            };

            btnSaveRevenuePdf.Click += async (s, e) =>
            {
                await LoadAllDataAsync();
                SaveRevenueReportToPdf($"RevenueReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            };

            reportDisplayPanel.Controls.Add(rtbReportDisplay);

            reportPanel.Controls.AddRange(new Control[] {
        lblTitle,
        btnCustomerReport, btnSaveCustomerPdf,
        btnJobReport, btnSaveJobPdf,
        btnRevenueReport, btnSaveRevenuePdf
    });

            tab.Controls.Add(reportPanel);
            tab.Controls.Add(reportDisplayPanel);

            return tab;
        }




        // Customer Management Methods
        private async Task AddCustomerAsync(TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, TextBox txtAddress, TextBox txtPassword, ListView customerList)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate email format
                if (!IsValidEmail(txtEmail.Text.Trim()))
                {
                    MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check for duplicate email in database
                var existingCustomer = await _dataManager.GetCustomerByEmailAsync(txtEmail.Text.Trim());
                if (existingCustomer != null)
                {
                    MessageBox.Show("A customer with this email already exists.", "Duplicate Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var customer = new Customer
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    Password = txtPassword.Text.Trim(),
                    RegistrationDate = DateTime.Now,
                    Username = txtEmail.Text.Trim()
                };

                await _dataManager.AddCustomerAsync(customer);
                await RefreshAllTabsAsync();

                // THIS IS THE KEY ADDITION - Refresh the job customer dropdown
                await RefreshJobCustomerDropdownAsync();

                ClearCustomerForm(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword, customerList);
                MessageBox.Show("Customer added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                string errorMessage = "Database error occurred: ";
                if (dbEx.InnerException != null)
                {
                    if (dbEx.InnerException.Message.Contains("UNIQUE constraint failed"))
                    {
                        errorMessage += "A customer with this email already exists.";
                    }
                    else if (dbEx.InnerException.Message.Contains("NOT NULL constraint failed"))
                    {
                        errorMessage += "Required field is missing.";
                    }
                    else
                    {
                        errorMessage += dbEx.InnerException.Message;
                    }
                }
                else
                {
                    errorMessage += dbEx.Message;
                }
                MessageBox.Show(errorMessage, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string fullError = $"Error adding customer: {ex.Message}";
                if (ex.InnerException != null)
                {
                    fullError += $"\nInner Exception: {ex.InnerException.Message}";
                }
                MessageBox.Show(fullError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateCustomerAsync(TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, TextBox txtAddress, TextBox txtPassword, ListView customerList)
        {
            try
            {
                if (customerList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a customer to update.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedItem = customerList.SelectedItems[0];
                var customerId = int.Parse(selectedItem.Text);
                var customer = _customers.FirstOrDefault(c => c.CustomerId == customerId);

                if (customer != null)
                {
                    customer.FirstName = txtFirstName.Text.Trim();
                    customer.LastName = txtLastName.Text.Trim();
                    customer.Email = txtEmail.Text.Trim();
                    customer.Phone = txtPhone.Text.Trim();
                    customer.Address = txtAddress.Text.Trim();
                    customer.Password = txtPassword.Text.Trim();

                    await _dataManager.UpdateCustomerAsync(customer);
                    await RefreshAllTabsAsync();
                    await RefreshCustomerListAsync();
                    await RefreshJobCustomerDropdownAsync();
                    ClearCustomerForm(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, txtPassword);
                    MessageBox.Show("Customer updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteCustomerAsync(ListView customerList)
        {
            try
            {
                if (customerList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a customer to delete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    var selectedItem = customerList.SelectedItems[0];
                    var customerId = int.Parse(selectedItem.Text);

                    await _dataManager.DeleteCustomerAsync(customerId);
                    await RefreshAllTabsAsync();
                    await RefreshCustomerListAsync();
                    await RefreshJobCustomerDropdownAsync();
                    MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCustomerForEdit(ListView customerList, TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, TextBox txtAddress, TextBox txtPassword)
        {
            if (customerList.SelectedItems.Count > 0)
            {
                var selectedItem = customerList.SelectedItems[0];
                var customerId = int.Parse(selectedItem.Text);
                var customer = _customers.FirstOrDefault(c => c.CustomerId == customerId);

                if (customer != null)
                {
                    txtFirstName.Text = customer.FirstName;
                    txtLastName.Text = customer.LastName;
                    txtEmail.Text = customer.Email;
                    txtPhone.Text = customer.Phone;
                    txtAddress.Text = customer.Address;
                    txtPassword.Text = customer.Password;
                }
            }
        }

        private void ClearCustomerForm(TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, TextBox txtAddress, TextBox txtPassword)
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtPassword.Clear();
        }

        // Transport Unit Management Methods
        private async Task AddTransportUnitAsync(ComboBox cmbUnitType, TextBox txtLicensePlate, NumericUpDown numMaxWeight, NumericUpDown numMaxVolume, TextBox txtDriverName, TextBox txtAssistantName, TextBox txtDriverPhone, CheckBox chkAvailable, ListView unitList)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cmbUnitType.Text) || string.IsNullOrWhiteSpace(txtLicensePlate.Text) ||
                    string.IsNullOrWhiteSpace(txtDriverName.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var unit = new TransportUnit
                {
                    UnitType = cmbUnitType.Text.Trim(),
                    LicensePlate = txtLicensePlate.Text.Trim(),
                    MaxWeight = numMaxWeight.Value,
                    MaxVolume = numMaxVolume.Value,
                    DriverName = txtDriverName.Text.Trim(),
                    DriverPhone = txtDriverPhone.Text.Trim(),
                    AssistantName = txtAssistantName.Text.Trim(),
                    IsAvailable = chkAvailable.Checked,
                    CreatedDate = DateTime.Now
                };

                await _dataManager.AddTransportUnitAsync(unit);
                await RefreshAllTabsAsync();
                await RefreshTransportUnitListAsync();
                await RefreshLoadTransportUnitDropdownAsync();

                ClearTransportUnitForm(cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume, txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable);
                MessageBox.Show("Transport unit added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding transport unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateTransportUnitAsync(ComboBox cmbUnitType, TextBox txtLicensePlate, NumericUpDown numMaxWeight, NumericUpDown numMaxVolume, TextBox txtDriverName, TextBox txtDriverPhone, TextBox txtAssistantName, CheckBox chkAvailable, ListView unitList)
        {
            try
            {
                if (unitList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a transport unit to update.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedItem = unitList.SelectedItems[0];
                var unitId = int.Parse(selectedItem.Text);
                var unit = _transportUnits.FirstOrDefault(u => u.TransportUnitId == unitId);

                if (unit != null)
                {
                    unit.UnitType = cmbUnitType.Text.Trim();
                    unit.LicensePlate = txtLicensePlate.Text.Trim();
                    unit.MaxWeight = numMaxWeight.Value;
                    unit.MaxVolume = numMaxVolume.Value;
                    unit.DriverName = txtDriverName.Text.Trim();
                    unit.DriverPhone = txtDriverPhone.Text.Trim();
                    unit.AssistantName = txtAssistantName.Text.Trim();
                    unit.IsAvailable = chkAvailable.Checked;

                    await _dataManager.UpdateTransportUnitAsync(unit);
                    await RefreshAllTabsAsync();
                    await RefreshTransportUnitListAsync();
                    await RefreshLoadTransportUnitDropdownAsync();

                    ClearTransportUnitForm(cmbUnitType, txtLicensePlate, numMaxWeight, numMaxVolume, txtDriverName, txtDriverPhone, txtAssistantName, chkAvailable);
                    MessageBox.Show("Transport unit updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating transport unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteTransportUnitAsync(ListView unitList)
        {
            try
            {
                if (unitList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a transport unit to delete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this transport unit?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    var selectedItem = unitList.SelectedItems[0];
                    var unitId = int.Parse(selectedItem.Text);

                    await _dataManager.DeleteTransportUnitAsync(unitId);
                    await RefreshAllTabsAsync();
                    await RefreshTransportUnitListAsync();
                    await RefreshLoadTransportUnitDropdownAsync();
                    MessageBox.Show("Transport unit deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting transport unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTransportUnitForEdit(ListView unitList, ComboBox cmbUnitType, TextBox txtLicensePlate, NumericUpDown numMaxWeight, NumericUpDown numMaxVolume, TextBox txtDriverName, TextBox txtDriverPhone, TextBox txtAssistantName, CheckBox chkAvailable)
        {
            if (unitList.SelectedItems.Count > 0)
            {
                var selectedItem = unitList.SelectedItems[0];
                var unitId = int.Parse(selectedItem.Text);
                var unit = _transportUnits.FirstOrDefault(u => u.TransportUnitId == unitId);

                if (unit != null)
                {
                    cmbUnitType.Text = unit.UnitType;
                    txtLicensePlate.Text = unit.LicensePlate;
                    numMaxWeight.Value = unit.MaxWeight;
                    numMaxVolume.Value = unit.MaxVolume;
                    txtDriverName.Text = unit.DriverName;
                    txtDriverPhone.Text = unit.DriverPhone;
                    txtAssistantName.Text = unit.DriverPhone;
                    chkAvailable.Checked = unit.IsAvailable;
                }
            }
        }

        private void ClearTransportUnitForm(ComboBox cmbUnitType, TextBox txtLicensePlate, NumericUpDown numMaxWeight, NumericUpDown numMaxVolume, TextBox txtDriverName, TextBox txtDriverPhone, TextBox txtAssistantName, CheckBox chkAvailable)
        {
            cmbUnitType.SelectedIndex = -1;
            txtLicensePlate.Clear();
            numMaxWeight.Value = 0;
            numMaxVolume.Value = 0;
            txtDriverName.Clear();
            txtDriverPhone.Clear();
            chkAvailable.Checked = true;
        }

        // Job Management Methods
        private async Task AddJobAsync(ComboBox cmbCustomer, TextBox txtStartLocation, TextBox txtDestination, TextBox txtDescription, DateTimePicker dtpRequestDate, ComboBox cmbStatus, NumericUpDown numCost, ListView jobList)
        {
            try
            {
                if (cmbCustomer.SelectedItem == null || string.IsNullOrWhiteSpace(txtStartLocation.Text) || string.IsNullOrWhiteSpace(txtDestination.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var job = new Job
                {
                    CustomerId = ((Customer)cmbCustomer.SelectedItem).CustomerId,
                    StartLocation = txtStartLocation.Text.Trim(),
                    Destination = txtDestination.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    RequestDate = dtpRequestDate.Value,
                    Status = cmbStatus.Text,
                    Cost = numCost.Value,
                    CreatedDate = DateTime.Now
                };

                await _dataManager.AddJobAsync(job);
                await RefreshJobListAsync();
                await RefreshAllTabsAsync();
                await RefreshLoadJobDropdownAsync();
                ClearJobForm(cmbCustomer, txtStartLocation, txtDestination, txtDescription, dtpRequestDate, cmbStatus, numCost);
                MessageBox.Show("Job added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding job: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateJobAsync(ComboBox cmbCustomer, TextBox txtStartLocation, TextBox txtDestination, TextBox txtDescription, DateTimePicker dtpRequestDate, ComboBox cmbStatus, NumericUpDown numCost, ListView jobList)
        {
            try
            {
                if (jobList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a job to update.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedItem = jobList.SelectedItems[0];
                var jobId = int.Parse(selectedItem.Text);
                var job = _jobs.FirstOrDefault(j => j.JobId == jobId);

                if (job != null)
                {
                    job.CustomerId = ((Customer)cmbCustomer.SelectedItem).CustomerId;
                    job.StartLocation = txtStartLocation.Text.Trim();
                    job.Destination = txtDestination.Text.Trim();
                    job.Description = txtDescription.Text.Trim();
                    job.RequestDate = dtpRequestDate.Value;
                    job.Status = cmbStatus.Text;
                    job.Cost = numCost.Value;

                    await _dataManager.UpdateJobAsync(job);
                    await RefreshAllTabsAsync();

                    await RefreshJobListAsync();
                    await RefreshLoadJobDropdownAsync();
                    ClearJobForm(cmbCustomer, txtStartLocation, txtDestination, txtDescription, dtpRequestDate, cmbStatus, numCost);
                    MessageBox.Show("Job updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating job: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteJobAsync(ListView jobList)
        {
            try
            {
                if (jobList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a job to delete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this job?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    var selectedItem = jobList.SelectedItems[0];
                    var jobId = int.Parse(selectedItem.Text);

                    await _dataManager.DeleteJobAsync(jobId);
                    await RefreshAllTabsAsync();
                    await RefreshJobListAsync();
                    await RefreshLoadJobDropdownAsync();
                    MessageBox.Show("Job deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting job: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadJobForEdit(ListView jobList, ComboBox cmbCustomer, TextBox txtStartLocation, TextBox txtDestination, TextBox txtDescription, DateTimePicker dtpRequestDate, ComboBox cmbStatus, NumericUpDown numCost)
        {
            if (jobList.SelectedItems.Count > 0)
            {
                var selectedItem = jobList.SelectedItems[0];
                var jobId = int.Parse(selectedItem.Text);
                var job = _jobs.FirstOrDefault(j => j.JobId == jobId);

                if (job != null)
                {
                    var customer = _customers.FirstOrDefault(c => c.CustomerId == job.CustomerId);
                    if (customer != null)
                    {
                        cmbCustomer.SelectedItem = customer;
                    }
                    txtStartLocation.Text = job.StartLocation;
                    txtDestination.Text = job.Destination;
                    txtDescription.Text = job.Description;
                    dtpRequestDate.Value = job.RequestDate;
                    cmbStatus.Text = job.Status;
                    numCost.Value = job.Cost;
                }
            }
        }

        private void ClearJobForm(ComboBox cmbCustomer, TextBox txtStartLocation, TextBox txtDestination, TextBox txtDescription, DateTimePicker dtpRequestDate, ComboBox cmbStatus, NumericUpDown numCost)
        {
            cmbCustomer.SelectedIndex = -1;
            txtStartLocation.Clear();
            txtDestination.Clear();
            txtDescription.Clear();
            dtpRequestDate.Value = DateTime.Now;
            cmbStatus.SelectedIndex = 0;
            numCost.Value = 0;
        }

        // Load Management Methods
        private async Task AddLoadAsync(ComboBox cmbJob, ComboBox cmbTransportUnit, TextBox txtLoadDescription, NumericUpDown numWeight, NumericUpDown numVolume, ComboBox cmbCategory, ComboBox cmbLoadStatus, ListView loadList)
        {
            try
            {
                if (cmbJob.SelectedItem == null || string.IsNullOrWhiteSpace(txtLoadDescription.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var load = new Load
                {
                    JobId = ((Job)cmbJob.SelectedItem).JobId,
                    TransportUnitId = cmbTransportUnit.SelectedItem != null ? ((TransportUnit)cmbTransportUnit.SelectedItem).TransportUnitId : (int?)null,
                    Description = txtLoadDescription.Text.Trim(),
                    Weight = numWeight.Value,
                    Volume = numVolume.Value,
                    Category = cmbCategory.Text,
                    Status = cmbLoadStatus.Text,
                    CreatedDate = DateTime.Now
                };

                await _dataManager.AddLoadAsync(load);
                await RefreshAllTabsAsync();
                // Set transport unit to unavailable if status is "Assigned"
                if (load.Status == "Assigned")
                {
                    var unit = await _context.TransportUnits.FindAsync(load.TransportUnitId);
                    if (unit != null)
                    {
                        unit.IsAvailable = false;
                        await _context.SaveChangesAsync();
                    }
                }
                await RefreshLoadListAsync();
                ClearLoadForm(cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus);
                MessageBox.Show("Load added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding load: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateLoadAsync(ComboBox cmbJob, ComboBox cmbTransportUnit, TextBox txtLoadDescription, NumericUpDown numWeight, NumericUpDown numVolume, ComboBox cmbCategory, ComboBox cmbLoadStatus, ListView loadList)
        {
            try
            {
                if (loadList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a load to update.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedItem = loadList.SelectedItems[0];
                var loadId = int.Parse(selectedItem.Text);
                var existingLoad = _loads.FirstOrDefault(l => l.LoadId == loadId);

                if (existingLoad != null)
                {
                    int? oldTransportUnitId = existingLoad.TransportUnitId;

                    existingLoad.JobId = ((Job)cmbJob.SelectedItem).JobId;
                    existingLoad.TransportUnitId = cmbTransportUnit.SelectedItem != null ? ((TransportUnit)cmbTransportUnit.SelectedItem).TransportUnitId : (int?)null;
                    existingLoad.Description = txtLoadDescription.Text.Trim();
                    existingLoad.Weight = numWeight.Value;
                    existingLoad.Volume = numVolume.Value;
                    existingLoad.Category = cmbCategory.Text;
                    existingLoad.Status = cmbLoadStatus.Text;

                    await _dataManager.UpdateLoadAsync(existingLoad);

                    // Update old transport unit availability if changed or delivered
                    if (oldTransportUnitId != existingLoad.TransportUnitId || existingLoad.Status == "Delivered")
                    {
                        if (oldTransportUnitId.HasValue)
                        {
                            var oldUnit = await _context.TransportUnits.FindAsync(oldTransportUnitId.Value);
                            if (oldUnit != null)
                            {
                                oldUnit.IsAvailable = true;
                            }
                        }
                    }

                    // If assigned status, mark new unit unavailable
                    if (existingLoad.Status == "Assigned" && existingLoad.TransportUnitId.HasValue)
                    {
                        var newUnit = await _context.TransportUnits.FindAsync(existingLoad.TransportUnitId.Value);
                        if (newUnit != null)
                        {
                            newUnit.IsAvailable = false;
                        }
                    }

                    //  If load is delivered, mark job as completed
                    if (existingLoad.Status == "Delivered")
                    {
                        await _dataManager.UpdateJobStatusAsync(existingLoad.JobId, "Completed");
                    }


                    await _context.SaveChangesAsync();
                    await RefreshAllTabsAsync();
                    await RefreshLoadListAsync();
                    ClearLoadForm(cmbJob, cmbTransportUnit, txtLoadDescription, numWeight, numVolume, cmbCategory, cmbLoadStatus);

                    MessageBox.Show("Load updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating load: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteLoadAsync(ListView loadList)
        {
            try
            {
                if (loadList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a load to delete.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this load?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    var selectedItem = loadList.SelectedItems[0];
                    var loadId = int.Parse(selectedItem.Text);

                    // Get the load from list
                    var load = _loads.FirstOrDefault(l => l.LoadId == loadId);
                    if (load != null && load.TransportUnitId.HasValue)
                    {
                        // Mark the transport unit as available again
                        var unit = await _context.TransportUnits.FindAsync(load.TransportUnitId.Value);
                        if (unit != null)
                        {
                            unit.IsAvailable = true;
                        }
                    }

                    await _dataManager.DeleteLoadAsync(loadId);
                    await RefreshAllTabsAsync();

                    await _context.SaveChangesAsync();

                    await RefreshLoadListAsync();
                    MessageBox.Show("Load deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting load: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadLoadForEdit(ListView loadList, ComboBox cmbJob, ComboBox cmbTransportUnit, TextBox txtLoadDescription, NumericUpDown numWeight, NumericUpDown numVolume, ComboBox cmbCategory, ComboBox cmbLoadStatus)
        {
            if (loadList.SelectedItems.Count > 0)
            {
                var selectedItem = loadList.SelectedItems[0];
                var loadId = int.Parse(selectedItem.Text);
                var load = _loads.FirstOrDefault(l => l.LoadId == loadId);

                if (load != null)
                {
                    var job = _jobs.FirstOrDefault(j => j.JobId == load.JobId);
                    if (job != null)
                    {
                        cmbJob.SelectedItem = job;
                    }

                    if (load.TransportUnitId.HasValue)
                    {
                        var unit = _transportUnits.FirstOrDefault(u => u.TransportUnitId == load.TransportUnitId.Value);
                        if (unit != null)
                        {
                            cmbTransportUnit.SelectedItem = unit;
                        }
                    }

                    txtLoadDescription.Text = load.Description;
                    numWeight.Value = load.Weight;
                    numVolume.Value = load.Volume;
                    cmbCategory.Text = load.Category;
                    cmbLoadStatus.Text = load.Status;
                }
            }
        }

        private void ClearLoadForm(ComboBox cmbJob, ComboBox cmbTransportUnit, TextBox txtLoadDescription, NumericUpDown numWeight, NumericUpDown numVolume, ComboBox cmbCategory, ComboBox cmbLoadStatus)
        {
            cmbJob.SelectedIndex = -1;
            cmbTransportUnit.SelectedIndex = -1;
            txtLoadDescription.Clear();
            numWeight.Value = 0;
            numVolume.Value = 0;
            cmbCategory.SelectedIndex = -1;
            cmbLoadStatus.SelectedIndex = 0;
        }



        // Report Generation Methods
        private async Task<string> GenerateJobReportTextAsync()
        {
            await LoadAllDataAsync();
            var report = new StringBuilder();
            report.AppendLine("=== JOB REPORT ===");
            report.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine($"Total Jobs: {_jobs.Count}");
            report.AppendLine();

            // Job status summary
            var statusGroups = _jobs.GroupBy(j => j.Status).OrderBy(g => g.Key);
            report.AppendLine("Job Status Summary:");
            foreach (var group in statusGroups)
            {
                report.AppendLine($"  {group.Key}: {group.Count()} jobs");
            }
            report.AppendLine();

            // Job details
            foreach (var job in _jobs.OrderByDescending(j => j.CreatedDate))
            {
                report.AppendLine($"Job ID: {job.JobId}");

                var customer = _customers.FirstOrDefault(c => c.CustomerId == job.CustomerId);
                report.AppendLine($"Customer: {(customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown")}");

                report.AppendLine($"From: {job.StartLocation}");
                report.AppendLine($"To: {job.Destination}");
                report.AppendLine($"Status: {job.Status}");
                report.AppendLine($"Cost: Rs.{job.Cost:F2}");
                report.AppendLine($"Created: {job.CreatedDate:yyyy-MM-dd HH:mm}");

                if (job.CompletionDate.HasValue)
                {
                    report.AppendLine($"Completed: {job.CompletionDate.Value:yyyy-MM-dd HH:mm}");
                }

                report.AppendLine(new string('-', 50));
            }

            return report.ToString();
        }

        private async Task<string> GenerateRevenueReportTextAsync()
        {
            await LoadAllDataAsync();
            var report = new StringBuilder();
            report.AppendLine("=== REVENUE REPORT ===");
            report.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            // Overall revenue summary
            var totalRevenue = _jobs.Sum(j => j.Cost);
            var completedJobs = _jobs.Where(j => j.Status == "Completed").ToList();
            var pendingJobs = _jobs.Where(j => j.Status == "Pending").ToList();
            var inProgressJobs = _jobs.Where(j => j.Status == "In Progress").ToList();

            report.AppendLine("REVENUE SUMMARY:");
            report.AppendLine($"Total Revenue: Rs.{totalRevenue:F2}");
            report.AppendLine($"Completed Jobs Revenue: Rs.{completedJobs.Sum(j => j.Cost):F2} ({completedJobs.Count} jobs)");
            report.AppendLine($"Pending Jobs Revenue: Rs.{pendingJobs.Sum(j => j.Cost):F2} ({pendingJobs.Count} jobs)");
            report.AppendLine($"In Progress Jobs Revenue: Rs.{inProgressJobs.Sum(j => j.Cost):F2} ({inProgressJobs.Count} jobs)");
            report.AppendLine();

            // Monthly revenue breakdown
            var monthlyRevenue = _jobs.GroupBy(j => new { j.CreatedDate.Year, j.CreatedDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(j => j.Cost),
                    JobCount = g.Count()
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month);

            report.AppendLine("MONTHLY REVENUE BREAKDOWN:");
            foreach (var month in monthlyRevenue)
            {
                var monthName = new DateTime(month.Year, month.Month, 1).ToString("MMMM yyyy");
                report.AppendLine($"{monthName}: Rs.{month.Revenue:F2} ({month.JobCount} jobs) - Avg: Rs.{(month.Revenue / month.JobCount):F2}");
            }
            report.AppendLine();

            // Top customers by revenue
            var customerRevenue = _jobs.GroupBy(j => j.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Revenue = g.Sum(j => j.Cost),
                    JobCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10);

            report.AppendLine("TOP CUSTOMERS BY REVENUE:");
            foreach (var cr in customerRevenue)
            {
                var customer = _customers.FirstOrDefault(c => c.CustomerId == cr.CustomerId);
                var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown";
                report.AppendLine($"{customerName}: Rs.{cr.Revenue:F2} ({cr.JobCount} jobs) - Avg: Rs.{(cr.Revenue / cr.JobCount):F2}");
            }
            report.AppendLine();

            // Revenue by status
            report.AppendLine("REVENUE BY JOB STATUS:");
            var revenueByStatus = _jobs.GroupBy(j => j.Status)
                .Select(g => new { Status = g.Key, Revenue = g.Sum(j => j.Cost), Count = g.Count() })
                .OrderByDescending(x => x.Revenue);

            foreach (var status in revenueByStatus)
            {
                report.AppendLine($"{status.Status}: Rs.{status.Revenue:F2} ({status.Count} jobs) - Avg: Rs.{(status.Revenue / status.Count):F2}");
            }

            return report.ToString();
        }

        private async Task<string> GenerateCustomerReportTextAsync()
        {
            await LoadAllDataAsync();
            var report = new StringBuilder();
            report.AppendLine("=== CUSTOMER REPORT ===");
            report.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine($"Total Customers: {_customers.Count}");
            report.AppendLine();

            foreach (var customer in _customers.OrderBy(c => c.LastName))
            {
                report.AppendLine($"Customer ID: {customer.CustomerId}");
                report.AppendLine($"Name: {customer.FirstName} {customer.LastName}");
                report.AppendLine($"Email: {customer.Email}");
                report.AppendLine($"Phone: {customer.Phone}");
                report.AppendLine($"Address: {customer.Address}");
                report.AppendLine($"Registration Date: {customer.RegistrationDate:yyyy-MM-dd}");

                var customerJobs = _jobs.Where(j => j.CustomerId == customer.CustomerId).ToList();
                report.AppendLine($"Total Jobs: {customerJobs.Count}");

                if (customerJobs.Any())
                {
                    var totalCost = customerJobs.Sum(j => j.Cost);
                    report.AppendLine($"Total Revenue: Rs.{totalCost:F2}");
                }

                report.AppendLine(new string('-', 50));
            }

            return report.ToString();
        }

        private void SaveCustomerReportToPdf(string defaultFileName)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Save Customer Report as PDF";
                    saveFileDialog.FileName = defaultFileName;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        GenerateCustomerPdfReport(saveFileDialog.FileName);
                        Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving customer report to PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateCustomerPdfReport(string filePath)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Customer Report";
            document.Info.Author = "Transport Management System";
            document.Info.Subject = "Customer Analytics Report";
            document.Info.Creator = "TMS Reporting Module";

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Fonts
            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            var regularFont = new XFont("Arial", 10, XFontStyle.Regular);
            var tableHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
            var tableFont = new XFont("Arial", 8, XFontStyle.Regular);

            double yPosition = MARGIN;

            // Draw Header
            yPosition = DrawReportHeader(gfx, "CUSTOMER REPORT", titleFont, headerFont, yPosition, page.Width);

            // Summary Statistics
            yPosition = DrawSummarySection(gfx, headerFont, regularFont, yPosition,
                $"Total Customers: {_customers.Count}",
                $"Report Generated: {DateTime.Now:MMMM dd, yyyy 'at' HH:mm}");

            // Customer Table
            yPosition = DrawCustomerTable(gfx, tableHeaderFont, tableFont, yPosition, page, document);

            // Footer
            DrawFooter(gfx, regularFont, page);

            document.Save(filePath);
            document.Close();
        }

        private double DrawReportHeader(XGraphics gfx, string title, XFont titleFont, XFont headerFont, double yPosition, double pageWidth)
        {
            // Company Header
            gfx.DrawString("TRANSPORT MANAGEMENT SYSTEM", headerFont, XBrushes.DarkBlue,
                new XRect(MARGIN, yPosition, pageWidth - 2 * MARGIN, 20), XStringFormats.Center);
            yPosition += 25;

            // Title
            gfx.DrawString(title, titleFont, XBrushes.Black,
                new XRect(MARGIN, yPosition, pageWidth - 2 * MARGIN, 25), XStringFormats.Center);
            yPosition += 35;

            // Horizontal line
            gfx.DrawLine(XPens.DarkBlue, MARGIN, yPosition, pageWidth - MARGIN, yPosition);
            yPosition += 20;

            return yPosition;
        }

        private double DrawSummarySection(XGraphics gfx, XFont headerFont, XFont regularFont, double yPosition, params string[] summaryItems)
        {
            gfx.DrawString("SUMMARY", headerFont, XBrushes.DarkBlue,
                new XRect(MARGIN, yPosition, 200, 20), XStringFormats.TopLeft);
            yPosition += 25;

            foreach (var item in summaryItems)
            {
                gfx.DrawString($"• {item}", regularFont, XBrushes.Black,
                    new XRect(MARGIN + 20, yPosition, 400, 16), XStringFormats.TopLeft);
                yPosition += 18;
            }

            yPosition += 10;
            return yPosition;
        }

        private double DrawCustomerTable(XGraphics gfx, XFont headerFont, XFont tableFont, double yPosition, PdfPage page, PdfDocument document)
        {
            // Table headers
            string[] headers = { "Customer ID", "Name", "Email", "Phone", "Jobs", "Revenue" };
            double[] columnWidths = { 80, 120, 150, 100, 60, 80 };
            double tableWidth = columnWidths.Sum();
            double startX = MARGIN;

            // Draw table header
            yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);

            // Draw customer data
            var customers = _customers.OrderBy(c => c.LastName).ToList();
            foreach (var customer in customers)
            {
                if (yPosition + TABLE_ROW_HEIGHT > page.Height - MARGIN - FOOTER_HEIGHT)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = MARGIN;
                    yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);
                }

                var customerJobs = _jobs.Where(j => j.CustomerId == customer.CustomerId).ToList();
                var totalRevenue = customerJobs.Sum(j => j.Cost);

                string[] rowData = {
                customer.CustomerId.ToString(),
                $"{customer.FirstName} {customer.LastName}",
                customer.Email,
                customer.Phone,
                customerJobs.Count.ToString(),
                $"Rs.{totalRevenue:F2}"
            };

                yPosition = DrawTableRow(gfx, rowData, columnWidths, startX, yPosition, tableFont);
            }

            return yPosition + 20;
        }

        private double DrawJobTable(XGraphics gfx, XFont headerFont, XFont tableFont, double yPosition, PdfPage page, PdfDocument document)
        {
            string[] headers = { "Job ID", "Customer", "From", "To", "Status", "Cost", "Date" };
            double[] columnWidths = { 60, 100, 100, 100, 80, 70, 80 };
            double startX = MARGIN;

            yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);

            var jobs = _jobs.OrderByDescending(j => j.CreatedDate).Take(20).ToList(); // Limit for space
            foreach (var job in jobs)
            {
                if (yPosition + TABLE_ROW_HEIGHT > page.Height - MARGIN - FOOTER_HEIGHT)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = MARGIN;
                    yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);
                }

                var customer = _customers.FirstOrDefault(c => c.CustomerId == job.CustomerId);
                string[] rowData = {
                job.JobId.ToString(),
                customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                TruncateText(job.StartLocation, 15),
                TruncateText(job.Destination, 15),
                job.Status,
                $"Rs.{job.Cost:F2}",
                job.CreatedDate.ToString("MM/dd/yyyy")
            };

                yPosition = DrawTableRow(gfx, rowData, columnWidths, startX, yPosition, tableFont);
            }

            return yPosition + 20;
        }

        private double DrawMonthlyRevenueTable(XGraphics gfx, XFont headerFont, XFont tableFont, double yPosition, PdfPage page, PdfDocument document)
        {
            gfx.DrawString("MONTHLY REVENUE BREAKDOWN", headerFont, XBrushes.DarkBlue,
                new XRect(MARGIN, yPosition, 300, 20), XStringFormats.TopLeft);
            yPosition += 25;

            string[] headers = { "Month", "Revenue", "Jobs", "Avg per Job" };
            double[] columnWidths = { 100, 100, 60, 100 };
            double startX = MARGIN;

            yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);

            var monthlyRevenue = _jobs.GroupBy(j => new { j.CreatedDate.Year, j.CreatedDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(j => j.Cost),
                    JobCount = g.Count()
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Take(12);

            foreach (var month in monthlyRevenue)
            {
                if (yPosition + TABLE_ROW_HEIGHT > page.Height - MARGIN - FOOTER_HEIGHT)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = MARGIN;
                    yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);
                }

                string[] rowData = {
                $"{month.Year}-{month.Month:00}",
                $"Rs.{month.Revenue:F2}",
                month.JobCount.ToString(),
                $"Rs.{(month.Revenue / month.JobCount):F2}"
            };

                yPosition = DrawTableRow(gfx, rowData, columnWidths, startX, yPosition, tableFont);
            }

            return yPosition + 30;
        }

        private double DrawTopCustomersTable(XGraphics gfx, XFont headerFont, XFont tableFont, double yPosition, PdfPage page, PdfDocument document)
        {
            gfx.DrawString("TOP CUSTOMERS BY REVENUE", headerFont, XBrushes.DarkBlue,
                new XRect(MARGIN, yPosition, 300, 20), XStringFormats.TopLeft);
            yPosition += 25;

            string[] headers = { "Customer", "Revenue", "Jobs", "Avg per Job" };
            double[] columnWidths = { 150, 100, 60, 100 };
            double startX = MARGIN;

            yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);

            var customerRevenue = _jobs.GroupBy(j => j.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Revenue = g.Sum(j => j.Cost),
                    JobCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10);

            foreach (var cr in customerRevenue)
            {
                if (yPosition + TABLE_ROW_HEIGHT > page.Height - MARGIN - FOOTER_HEIGHT)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = MARGIN;
                    yPosition = DrawTableHeader(gfx, headers, columnWidths, startX, yPosition, headerFont);
                }

                var customer = _customers.FirstOrDefault(c => c.CustomerId == cr.CustomerId);
                string[] rowData = {
                customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown",
                $"Rs.{cr.Revenue:F2}",
                cr.JobCount.ToString(),
                $"Rs.{(cr.Revenue / cr.JobCount):F2}"
            };

                yPosition = DrawTableRow(gfx, rowData, columnWidths, startX, yPosition, tableFont);
            }

            return yPosition + 20;
        }

        private double DrawTableHeader(XGraphics gfx, string[] headers, double[] columnWidths, double startX, double yPosition, XFont font)
        {
            double currentX = startX;

            // Draw header background
            gfx.DrawRectangle(XBrushes.LightGray, startX, yPosition, columnWidths.Sum(), TABLE_ROW_HEIGHT);

            // Draw header borders
            gfx.DrawRectangle(XPens.Black, startX, yPosition, columnWidths.Sum(), TABLE_ROW_HEIGHT);

            for (int i = 0; i < headers.Length; i++)
            {
                // Draw vertical lines
                if (i > 0)
                    gfx.DrawLine(XPens.Black, currentX, yPosition, currentX, yPosition + TABLE_ROW_HEIGHT);

                // Draw header text
                gfx.DrawString(headers[i], font, XBrushes.Black,
                    new XRect(currentX + 5, yPosition + 3, columnWidths[i] - 10, TABLE_ROW_HEIGHT - 6),
                    XStringFormats.CenterLeft);

                currentX += columnWidths[i];
            }

            return yPosition + TABLE_ROW_HEIGHT;
        }

        private double DrawTableRow(XGraphics gfx, string[] rowData, double[] columnWidths, double startX, double yPosition, XFont font)
        {
            double currentX = startX;

            // Draw row border
            gfx.DrawRectangle(XPens.Black, startX, yPosition, columnWidths.Sum(), TABLE_ROW_HEIGHT);

            for (int i = 0; i < rowData.Length; i++)
            {
                // Draw vertical lines
                if (i > 0)
                    gfx.DrawLine(XPens.Black, currentX, yPosition, currentX, yPosition + TABLE_ROW_HEIGHT);

                // Draw cell text
                gfx.DrawString(rowData[i], font, XBrushes.Black,
                    new XRect(currentX + 5, yPosition + 3, columnWidths[i] - 10, TABLE_ROW_HEIGHT - 6),
                    XStringFormats.CenterLeft);

                currentX += columnWidths[i];
            }

            return yPosition + TABLE_ROW_HEIGHT;
        }

        private void DrawFooter(XGraphics gfx, XFont font, PdfPage page)
        {
            double footerY = page.Height - MARGIN;
            gfx.DrawString($"Generated on {DateTime.Now:MMMM dd, yyyy 'at' HH:mm}", font, XBrushes.Gray,
                new XRect(MARGIN, footerY, page.Width - 2 * MARGIN, 20), XStringFormats.CenterLeft);

            gfx.DrawString("Transport Management System", font, XBrushes.Gray,
                new XRect(MARGIN, footerY, page.Width - 2 * MARGIN, 20), XStringFormats.CenterRight);
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        private void SaveJobReportToPdf(string defaultFileName)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Save Job Report as PDF";
                    saveFileDialog.FileName = defaultFileName;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        GenerateJobPdfReport(saveFileDialog.FileName);
                        Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving job report to PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateJobPdfReport(string filePath)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Job Report";
            document.Info.Author = "Transport Management System";

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            var regularFont = new XFont("Arial", 10, XFontStyle.Regular);
            var tableHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
            var tableFont = new XFont("Arial", 8, XFontStyle.Regular);

            double yPosition = MARGIN;

            // Draw Header
            yPosition = DrawReportHeader(gfx, "JOB REPORT", titleFont, headerFont, yPosition, page.Width);

            // Job Statistics
            var statusGroups = _jobs.GroupBy(j => j.Status).OrderBy(g => g.Key);
            var statsText = new List<string> { $"Total Jobs: {_jobs.Count}" };
            foreach (var group in statusGroups)
                statsText.Add($"{group.Key}: {group.Count()} jobs");

            yPosition = DrawSummarySection(gfx, headerFont, regularFont, yPosition, statsText.ToArray());

            // Job Table
            yPosition = DrawJobTable(gfx, tableHeaderFont, tableFont, yPosition, page, document);

            DrawFooter(gfx, regularFont, page);
            document.Save(filePath);
            document.Close();
        }

        private void SaveRevenueReportToPdf(string defaultFileName)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*";
                    saveFileDialog.Title = "Save Revenue Report as PDF";
                    saveFileDialog.FileName = defaultFileName;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        GenerateRevenuePdfReport(saveFileDialog.FileName);
                        Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving revenue report to PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateRevenuePdfReport(string filePath)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Revenue Report";
            document.Info.Author = "Transport Management System";

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            var regularFont = new XFont("Arial", 10, XFontStyle.Regular);
            var tableHeaderFont = new XFont("Arial", 9, XFontStyle.Bold);
            var tableFont = new XFont("Arial", 8, XFontStyle.Regular);

            double yPosition = MARGIN;

            // Draw Header
            yPosition = DrawReportHeader(gfx, "REVENUE REPORT", titleFont, headerFont, yPosition, page.Width);

            // Revenue Summary
            var totalRevenue = _jobs.Sum(j => j.Cost);
            var completedJobs = _jobs.Where(j => j.Status == "Completed").ToList();
            var pendingJobs = _jobs.Where(j => j.Status == "Pending").ToList();

            yPosition = DrawSummarySection(gfx, headerFont, regularFont, yPosition,
                $"Total Revenue: Rs.{totalRevenue:F2}",
                $"Completed Jobs Revenue: Rs.{completedJobs.Sum(j => j.Cost):F2}",
                $"Pending Jobs Revenue: Rs.{pendingJobs.Sum(j => j.Cost):F2}");

            // Monthly Revenue Table
            yPosition = DrawMonthlyRevenueTable(gfx, tableHeaderFont, tableFont, yPosition, page, document);

            // Top Customers Table
            yPosition = DrawTopCustomersTable(gfx, tableHeaderFont, tableFont, yPosition, page, document);

            DrawFooter(gfx, regularFont, page);
            document.Save(filePath);
            document.Close();
        }

        // Validation Methods
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && phone.Length >= 10;
        }

        // Utility Methods
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private DialogResult ShowConfirmationDialog(string message)
        {
            return MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        // Export Methods
        private void ExportToCSV(ListView listView, string filename)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                    saveFileDialog.FileName = filename;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (var writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            // Write headers
                            var headers = new List<string>();
                            foreach (ColumnHeader column in listView.Columns)
                            {
                                headers.Add(column.Text);
                            }
                            writer.WriteLine(string.Join(",", headers));

                            // Write data
                            foreach (ListViewItem item in listView.Items)
                            {
                                var values = new List<string>();
                                for (int i = 0; i < item.SubItems.Count; i++)
                                {
                                    values.Add($"\"{item.SubItems[i].Text}\"");
                                }
                                writer.WriteLine(string.Join(",", values));
                            }
                        }

                        ShowSuccessMessage("Data exported successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error exporting data: {ex.Message}");
            }
        }

        // Data validation and cleanup
        private void ValidateAndCleanupData()
        {
            try
            {
                _customers = _customers.Where(c => !string.IsNullOrEmpty(c.FirstName) && !string.IsNullOrEmpty(c.LastName)).ToList();

                _jobs = _jobs.Where(j => _customers.Any(c => c.CustomerId == j.CustomerId)).ToList();

                _loads = _loads.Where(l => _jobs.Any(j => j.JobId == l.JobId)).ToList();

                foreach (var load in _loads)
                {
                    if (load.TransportUnitId.HasValue && !_transportUnits.Any(u => u.TransportUnitId == load.TransportUnitId))
                    {
                        load.TransportUnitId = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error validating data: {ex.Message}");
            }
        }

        private async Task LoadAllDataAsync()
        {
            try
            {
                _customers = (await _dataManager.GetAllCustomersAsync()).ToList();
                _jobs = (await _dataManager.GetAllJobsAsync()).ToList();
                _loads = (await _dataManager.GetAllLoadsAsync()).ToList();
                _transportUnits = (await _dataManager.GetAllTransportUnitsAsync()).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshCustomerListAsync()
        {
            try
            {
                _customers = await _dataManager.GetAllCustomersAsync();
                customerListView.Items.Clear();

                foreach (var customer in _customers)
                {
                    var item = new ListViewItem(customer.CustomerId.ToString());
                    item.SubItems.Add(customer.FullName);
                    item.SubItems.Add(customer.Email);
                    item.SubItems.Add(customer.Phone ?? "N/A");
                    item.SubItems.Add(customer.RegistrationDate.ToString("yyyy-MM-dd"));
                    customerListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load customers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshTransportUnitListAsync()
        {
            try
            {
                var units = await _dataManager.GetAllTransportUnitsAsync();
                transportUnitListView.Items.Clear();

                foreach (var unit in units)
                {
                    var item = new ListViewItem(unit.TransportUnitId.ToString());
                    item.SubItems.Add(unit.UnitType);
                    item.SubItems.Add(unit.LicensePlate);
                    item.SubItems.Add(unit.MaxWeight.ToString("F2"));
                    item.SubItems.Add(unit.MaxVolume.ToString("F2"));
                    item.SubItems.Add(unit.DriverName);
                    item.SubItems.Add(unit.DriverPhone ?? "N/A");
                    item.SubItems.Add(unit.AssistantName);
                    item.SubItems.Add(unit.IsAvailable ? "Yes" : "No");
                    item.SubItems.Add(unit.CreatedDate.ToString("yyyy-MM-dd"));

                    transportUnitListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load transport units: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshJobListAsync()
        {
            try
            {
                var jobs = await _dataManager.GetAllJobsAsync();
                jobListView.Items.Clear();

                foreach (var job in jobs)
                {
                    var item = new ListViewItem(job.JobId.ToString());
                    item.SubItems.Add(job.Customer?.FullName ?? "N/A");
                    item.SubItems.Add(job.StartLocation);
                    item.SubItems.Add(job.Destination);
                    item.SubItems.Add(job.Description ?? "");
                    item.SubItems.Add(job.RequestDate.ToString("yyyy-MM-dd"));
                    item.SubItems.Add(job.ScheduleDate?.ToString("yyyy-MM-dd") ?? "Not Scheduled");
                    item.SubItems.Add(job.Status);
                    item.SubItems.Add(job.Cost.ToString("F2"));
                    item.SubItems.Add(job.CreatedDate.ToString("yyyy-MM-dd"));

                    jobListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshLoadListAsync()
        {
            try
            {
                var loads = await _dataManager.GetAllLoadsAsync();
                loadListView.Items.Clear();

                foreach (var load in loads)
                {
                    var item = new ListViewItem(load.LoadId.ToString());
                    item.SubItems.Add(load.TransportUnit != null ? load.Job.DisplayName : "N/A");
					item.SubItems.Add(load.TransportUnit != null ? load.TransportUnit.LicensePlate : "N/A");
                    item.SubItems.Add(load.Description);
                    item.SubItems.Add(load.Weight.ToString("N2"));
                    item.SubItems.Add(load.Volume.ToString("N2"));
                    item.SubItems.Add(load.Category ?? "N/A");
                    item.SubItems.Add(load.Status ?? "Pending");
                    item.SubItems.Add(load.CreatedDate.ToString("yyyy-MM-dd"));
                    item.SubItems.Add(load.Job != null ? load.Job.StartLocation : "N/A");

                    loadListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load loads: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshDashboardAsync()
        {
            if (adminDashboardTab != null)
            {
                var stats = await _dataManager.GetDashboardStatsAsync();
                var revenue = await _dataManager.GetTotalRevenueAsync();
                UpdateSummaryCard("Total_Customers", stats["TotalCustomers"].ToString());
                UpdateSummaryCard("Active_Jobs", stats["ActiveJobs"].ToString());
                UpdateSummaryCard("Transport_Units", stats["TransportUnits"].ToString());
                UpdateSummaryCard("Completed_Jobs", stats["CompletedJobs"].ToString());
                UpdateSummaryCard("Total_Revenue", $"Rs.{revenue:F2}");

                if (activityListView != null)
                {
                    activityListView.Items.Clear();
                    var recentJobs = await _dataManager.GetRecentJobsAsync();
                    foreach (var job in recentJobs)
                    {
                        string date = job.CreatedDate.ToString("yyyy-MM-dd");
                        string activity = "Job Created";
                        string details = $"From {job.StartLocation} to {job.Destination}";
                        string status = job.Status;
                        ListViewItem item = new ListViewItem(date);
                        item.SubItems.Add(activity);
                        item.SubItems.Add(details);
                        item.SubItems.Add(status);
                        activityListView.Items.Add(item);
                    }
                }
            }
        }


        // Method to update summary cards on dashboard
        private void UpdateSummaryCard(string cardName, string value)
        {
            if (adminDashboardTab != null)
            {
                var control = adminDashboardTab.Controls.Find($"card_{cardName}", true).FirstOrDefault();
                if (control != null && control is Label)
                {
                    ((Label)control).Text = value;
                }
            }
        }

      
        private void LogoutButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                var loginForm = new LoginForm(_dataManager, _context);
                loginForm.Show();
            }
        }

        private async Task RefreshJobCustomerDropdownAsync()
        {
            if (jobCustomerComboBox != null)
            {
                var customers = await _dataManager.GetAllCustomersAsync();

                // Store the currently selected customer ID (if any)
                object selectedValue = jobCustomerComboBox.SelectedValue;

                // Update the data source
                jobCustomerComboBox.DataSource = customers;
                jobCustomerComboBox.DisplayMember = "FullName";
                jobCustomerComboBox.ValueMember = "CustomerId";

                // Try to restore the previous selection
                if (selectedValue != null)
                {
                    jobCustomerComboBox.SelectedValue = selectedValue;
                }
            }
        }

        private async Task RefreshLoadJobDropdownAsync()
        {
            if (jobCustomerComboBox != null)
            {
                var jobs = (await _dataManager.GetAllJobsAsync())
                    .Where(j => j.Status == "Accepted")
                    .ToList();

                // Store the currently selected customer ID (if any)
                object selectedValue = jobCustomerComboBox.SelectedValue;

                // Update the data source
                loadJobComboBox.DataSource = jobs;
                loadJobComboBox.DisplayMember = "DisplayName";
                loadJobComboBox.ValueMember = "JobId";

                // Try to restore the previous selection
                if (selectedValue != null)
                {
                    jobCustomerComboBox.SelectedValue = selectedValue;
                }
            }
        }


        private async Task RefreshLoadTransportUnitDropdownAsync()
        {
            if (jobCustomerComboBox != null)
            {
                var units = await _dataManager.GetAllTransportUnitsAsync();

                // Store the currently selected customer ID (if any)
                object selectedValue = loadTransportUnitComboBox.SelectedValue;

                // Update the data source
                loadTransportUnitComboBox.DataSource = units;
                loadTransportUnitComboBox.DisplayMember = "DriverName";
                loadTransportUnitComboBox.ValueMember = "TransportUnitId";

                // Try to restore the previous selection
                if (selectedValue != null)
                {
                    jobCustomerComboBox.SelectedValue = selectedValue;
                }
            }
        }
    }
}
