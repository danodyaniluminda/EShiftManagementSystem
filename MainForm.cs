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
    // Main Form
    public partial class MainForm : Form
    {
        private DataManager _dataManager;
        private TabControl _tabControl;
        private Button _saveButton;
        private Button _exitButton;

        // Cache for data to avoid frequent database calls
        private List<Customer> _customers;
        private List<Job> _jobs;
        private List<Load> _loads;
        private List<TransportUnit> _transportUnits;

        public MainForm(DataManager dataManager)
        {
            InitializeComponent();
            _dataManager = dataManager;
            _customers = new List<Customer>();
            _jobs = new List<Job>();
            _loads = new List<Load>();
            _transportUnits = new List<TransportUnit>();
            SetupForm();
        }

        private async void SetupForm()
        {
            // Create main tab control
            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);

            // Create tabs
            _tabControl.TabPages.Add(CreateCustomerTab());
            _tabControl.TabPages.Add(CreateTransportUnitTab());
            _tabControl.TabPages.Add(CreateJobTab());
            _tabControl.TabPages.Add(CreateLoadTab());
            _tabControl.TabPages.Add(CreateReportsTab());

            // Create bottom panel for buttons
            Panel bottomPanel = new Panel();
            bottomPanel.Height = 60;
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.BackColor = System.Drawing.Color.LightGray;

            _saveButton = new Button();
            _saveButton.Text = "Refresh Data";
            _saveButton.Size = new System.Drawing.Size(100, 35);
            _saveButton.Location = new System.Drawing.Point(20, 12);
            _saveButton.BackColor = System.Drawing.Color.Green;
            _saveButton.ForeColor = System.Drawing.Color.White;
            _saveButton.Click += RefreshButton_Click;

            _exitButton = new Button();
            _exitButton.Text = "Exit";
            _exitButton.Size = new System.Drawing.Size(100, 35);
            _exitButton.Location = new System.Drawing.Point(130, 12);
            _exitButton.BackColor = System.Drawing.Color.Red;
            _exitButton.ForeColor = System.Drawing.Color.White;
            _exitButton.Click += ExitButton_Click;

            bottomPanel.Controls.Add(_saveButton);
            bottomPanel.Controls.Add(_exitButton);

            // Add controls to form
            this.Controls.Add(_tabControl);
            this.Controls.Add(bottomPanel);

            // Load initial data
            await RefreshAllTabsAsync();
        }

        private TabPage CreateCustomerTab()
        {
            TabPage tab = new TabPage("Customers");

            // Create customer form panel
            Panel formPanel = new Panel();
            formPanel.Size = new System.Drawing.Size(400, 300);
            formPanel.Location = new System.Drawing.Point(20, 20);
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            // Customer form controls
            Label lblTitle = new Label() { Text = "Customer Registration", Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(200, 25) };

            Label lblFirstName = new Label() { Text = "First Name:", Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(80, 20) };
            TextBox txtFirstName = new TextBox() { Location = new System.Drawing.Point(100, 48), Size = new System.Drawing.Size(150, 23), Name = "txtFirstName" };

            Label lblLastName = new Label() { Text = "Last Name:", Location = new System.Drawing.Point(10, 80), Size = new System.Drawing.Size(80, 20) };
            TextBox txtLastName = new TextBox() { Location = new System.Drawing.Point(100, 78), Size = new System.Drawing.Size(150, 23), Name = "txtLastName" };

            Label lblEmail = new Label() { Text = "Email:", Location = new System.Drawing.Point(10, 110), Size = new System.Drawing.Size(80, 20) };
            TextBox txtEmail = new TextBox() { Location = new System.Drawing.Point(100, 108), Size = new System.Drawing.Size(200, 23), Name = "txtEmail" };

            Label lblPhone = new Label() { Text = "Phone:", Location = new System.Drawing.Point(10, 140), Size = new System.Drawing.Size(80, 20) };
            TextBox txtPhone = new TextBox() { Location = new System.Drawing.Point(100, 138), Size = new System.Drawing.Size(150, 23), Name = "txtPhone" };

            Label lblAddress = new Label() { Text = "Address:", Location = new System.Drawing.Point(10, 170), Size = new System.Drawing.Size(80, 20) };
            TextBox txtAddress = new TextBox() { Location = new System.Drawing.Point(100, 168), Size = new System.Drawing.Size(250, 60), Multiline = true, Name = "txtAddress" };

            Button btnAddCustomer = new Button() { Text = "Add Customer", Location = new System.Drawing.Point(100, 240), Size = new System.Drawing.Size(100, 30), BackColor = System.Drawing.Color.Blue, ForeColor = System.Drawing.Color.White };
            Button btnClearCustomer = new Button() { Text = "Clear", Location = new System.Drawing.Point(210, 240), Size = new System.Drawing.Size(70, 30) };

            // Customer list
            ListView customerList = new ListView();
            customerList.Location = new System.Drawing.Point(440, 20);
            customerList.Size = new System.Drawing.Size(700, 400);
            customerList.View = View.Details;
            customerList.FullRowSelect = true;
            customerList.GridLines = true;
            customerList.Name = "customerList";

            customerList.Columns.Add("Customer ID", 100);
            customerList.Columns.Add("Name", 150);
            customerList.Columns.Add("Email", 200);
            customerList.Columns.Add("Phone", 120);
            customerList.Columns.Add("Registration Date", 130);

            // Event handlers
            btnAddCustomer.Click += async (s, e) => await AddCustomerAsync(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress, customerList);
            btnClearCustomer.Click += (s, e) => ClearCustomerForm(txtFirstName, txtLastName, txtEmail, txtPhone, txtAddress);

            // Add controls to form panel
            formPanel.Controls.AddRange(new Control[] { lblTitle, lblFirstName, txtFirstName, lblLastName, txtLastName, lblEmail, txtEmail, lblPhone, txtPhone, lblAddress, txtAddress, btnAddCustomer, btnClearCustomer });

            tab.Controls.Add(formPanel);
            tab.Controls.Add(customerList);

            return tab;
        }

        private TabPage CreateTransportUnitTab()
        {
            TabPage tab = new TabPage("Transport Units");

            Panel formPanel = new Panel
            {
                Size = new System.Drawing.Size(400, 350),
                Location = new System.Drawing.Point(20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = "Transport Unit Registration",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(250, 25)
            };

            Label lblLorryNumber = new Label { Text = "Lorry Number:", Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(120, 20) };
            TextBox txtLorryNumber = new TextBox { Location = new System.Drawing.Point(140, 48), Size = new System.Drawing.Size(120, 23), Name = "txtLorryNumber" };

            Label lblDriverName = new Label { Text = "Driver Name:", Location = new System.Drawing.Point(10, 80), Size = new System.Drawing.Size(120, 20) };
            TextBox txtDriverName = new TextBox { Location = new System.Drawing.Point(140, 78), Size = new System.Drawing.Size(180, 23), Name = "txtDriverName" };

            Label lblDriverLicense = new Label { Text = "Driver License:", Location = new System.Drawing.Point(10, 110), Size = new System.Drawing.Size(120, 20) };
            TextBox txtDriverLicense = new TextBox { Location = new System.Drawing.Point(140, 108), Size = new System.Drawing.Size(120, 23), Name = "txtDriverLicense" };

            Label lblAssistantName = new Label { Text = "Assistant:", Location = new System.Drawing.Point(10, 140), Size = new System.Drawing.Size(120, 20) };
            TextBox txtAssistantName = new TextBox { Location = new System.Drawing.Point(140, 138), Size = new System.Drawing.Size(180, 23), Name = "txtAssistantName" };

            Label lblContainerType = new Label { Text = "Container Type:", Location = new System.Drawing.Point(10, 170), Size = new System.Drawing.Size(120, 20) };
            ComboBox cmbContainerType = new ComboBox { Location = new System.Drawing.Point(140, 168), Size = new System.Drawing.Size(120, 23), Name = "cmbContainerType" };
            cmbContainerType.Items.AddRange(new string[] { "Small", "Medium", "Large", "Extra Large" });

            Label lblCapacity = new Label { Text = "Capacity (kg):", Location = new System.Drawing.Point(10, 200), Size = new System.Drawing.Size(120, 20) };
            NumericUpDown numCapacity = new NumericUpDown { Location = new System.Drawing.Point(140, 198), Size = new System.Drawing.Size(100, 23), Name = "numCapacity", Maximum = 50000, Minimum = 100 };

            Button btnAddTransportUnit = new Button
            {
                Text = "Add Transport Unit",
                Location = new System.Drawing.Point(140, 240),
                Size = new System.Drawing.Size(130, 30),
                BackColor = System.Drawing.Color.Blue,
                ForeColor = System.Drawing.Color.White
            };
            Button btnClearTransportUnit = new Button { Text = "Clear", Location = new System.Drawing.Point(280, 240), Size = new System.Drawing.Size(70, 30) };

            ListView transportList = new ListView
            {
                Location = new System.Drawing.Point(440, 20),
                Size = new System.Drawing.Size(700, 400),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Name = "transportList"
            };

            transportList.Columns.Add("Lorry #", 100);
            transportList.Columns.Add("Driver", 120);
            transportList.Columns.Add("License", 100);
            transportList.Columns.Add("Assistant", 120);
            transportList.Columns.Add("Container", 100);
            transportList.Columns.Add("Capacity", 80);
            transportList.Columns.Add("Available", 80);

            btnAddTransportUnit.Click += async (s, e) => await AddTransportUnitAsync(txtLorryNumber, txtDriverName, txtDriverLicense, txtAssistantName, cmbContainerType, numCapacity, transportList);
            btnClearTransportUnit.Click += (s, e) => ClearTransportUnitForm(txtLorryNumber, txtDriverName, txtDriverLicense, txtAssistantName, cmbContainerType, numCapacity);

            formPanel.Controls.AddRange(new Control[] {
        lblTitle, lblLorryNumber, txtLorryNumber,
        lblDriverName, txtDriverName,
        lblDriverLicense, txtDriverLicense,
        lblAssistantName, txtAssistantName,
        lblContainerType, cmbContainerType,
        lblCapacity, numCapacity,
        btnAddTransportUnit, btnClearTransportUnit
    });

            tab.Controls.Add(formPanel);
            tab.Controls.Add(transportList);

            return tab;
        }


        private TabPage CreateJobTab()
        {
            TabPage tab = new TabPage("Jobs");

            // Job form panel
            Panel formPanel = new Panel();
            formPanel.Size = new System.Drawing.Size(400, 300);
            formPanel.Location = new System.Drawing.Point(20, 20);
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitle = new Label() { Text = "Job Management", Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(200, 25) };

            Label lblCustomer = new Label() { Text = "Customer:", Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(80, 20) };
            ComboBox cmbCustomer = new ComboBox() { Location = new System.Drawing.Point(100, 48), Size = new System.Drawing.Size(200, 23), Name = "cmbCustomer", DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblStartLocation = new Label() { Text = "Start Location:", Location = new System.Drawing.Point(10, 80), Size = new System.Drawing.Size(80, 20) };
            TextBox txtStartLocation = new TextBox() { Location = new System.Drawing.Point(100, 78), Size = new System.Drawing.Size(250, 23), Name = "txtStartLocation" };

            Label lblDestination = new Label() { Text = "Destination:", Location = new System.Drawing.Point(10, 110), Size = new System.Drawing.Size(80, 20) };
            TextBox txtDestination = new TextBox() { Location = new System.Drawing.Point(100, 108), Size = new System.Drawing.Size(250, 23), Name = "txtDestination" };

            Label lblScheduledDate = new Label() { Text = "Scheduled Date:", Location = new System.Drawing.Point(10, 140), Size = new System.Drawing.Size(80, 20) };
            DateTimePicker dtpScheduledDate = new DateTimePicker() { Location = new System.Drawing.Point(100, 138), Size = new System.Drawing.Size(200, 23), Name = "dtpScheduledDate" };

            Label lblTotalCost = new Label() { Text = "Total Cost:", Location = new System.Drawing.Point(10, 170), Size = new System.Drawing.Size(80, 20) };
            NumericUpDown numTotalCost = new NumericUpDown() { Location = new System.Drawing.Point(100, 168), Size = new System.Drawing.Size(100, 23), Name = "numTotalCost", Maximum = 1000000, DecimalPlaces = 2 };

            Button btnAddJob = new Button() { Text = "Create Job", Location = new System.Drawing.Point(100, 210), Size = new System.Drawing.Size(100, 30), BackColor = System.Drawing.Color.Blue, ForeColor = System.Drawing.Color.White };
            Button btnClearJob = new Button() { Text = "Clear", Location = new System.Drawing.Point(210, 210), Size = new System.Drawing.Size(70, 30) };

            // Job list
            ListView jobList = new ListView();
            jobList.Location = new System.Drawing.Point(440, 20);
            jobList.Size = new System.Drawing.Size(700, 400);
            jobList.View = View.Details;
            jobList.FullRowSelect = true;
            jobList.GridLines = true;
            jobList.Name = "jobList";

            jobList.Columns.Add("Job ID", 80);
            jobList.Columns.Add("Customer", 120);
            jobList.Columns.Add("Start Location", 150);
            jobList.Columns.Add("Destination", 150);
            jobList.Columns.Add("Status", 80);
            jobList.Columns.Add("Cost", 80);
            jobList.Columns.Add("Request Date", 100);

            // Status update controls
            Label lblStatusUpdate = new Label() { Text = "Update Job Status:", Location = new System.Drawing.Point(440, 440), Size = new System.Drawing.Size(120, 20) };
            ComboBox cmbStatus = new ComboBox() { Location = new System.Drawing.Point(570, 438), Size = new System.Drawing.Size(120, 23), Name = "cmbStatus", DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new string[] { "Pending", "Assigned", "InProgress", "Completed", "Cancelled" });

            Button btnUpdateStatus = new Button() { Text = "Update Status", Location = new System.Drawing.Point(700, 437), Size = new System.Drawing.Size(100, 25), BackColor = System.Drawing.Color.Orange, ForeColor = System.Drawing.Color.White };

            // Event handlers
            btnAddJob.Click += async (s, e) => await AddJobAsync(cmbCustomer, txtStartLocation, txtDestination, dtpScheduledDate, numTotalCost, jobList);
            btnClearJob.Click += (s, e) => ClearJobForm(cmbCustomer, txtStartLocation, txtDestination, dtpScheduledDate, numTotalCost);
            btnUpdateStatus.Click += async (s, e) => await UpdateJobStatusAsync(jobList, cmbStatus);

            formPanel.Controls.AddRange(new Control[] { lblTitle, lblCustomer, cmbCustomer, lblStartLocation, txtStartLocation, lblDestination, txtDestination, lblScheduledDate, dtpScheduledDate, lblTotalCost, numTotalCost, btnAddJob, btnClearJob });

            tab.Controls.Add(formPanel);
            tab.Controls.Add(jobList);
            tab.Controls.Add(lblStatusUpdate);
            tab.Controls.Add(cmbStatus);
            tab.Controls.Add(btnUpdateStatus);

            return tab;
        }

        private TabPage CreateLoadTab()
        {
            TabPage tab = new TabPage("Loads");

            // Load form panel
            Panel formPanel = new Panel();
            formPanel.Size = new System.Drawing.Size(400, 320);
            formPanel.Location = new System.Drawing.Point(20, 20);
            formPanel.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitle = new Label() { Text = "Load Management", Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(200, 25) };

            Label lblJob = new Label() { Text = "Job:", Location = new System.Drawing.Point(10, 50), Size = new System.Drawing.Size(80, 20) };
            ComboBox cmbJob = new ComboBox() { Location = new System.Drawing.Point(100, 48), Size = new System.Drawing.Size(200, 23), Name = "cmbJob", DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblDescription = new Label() { Text = "Description:", Location = new System.Drawing.Point(10, 80), Size = new System.Drawing.Size(80, 20) };
            TextBox txtDescription = new TextBox() { Location = new System.Drawing.Point(100, 78), Size = new System.Drawing.Size(250, 23), Name = "txtDescription" };

            Label lblWeight = new Label() { Text = "Weight (kg):", Location = new System.Drawing.Point(10, 110), Size = new System.Drawing.Size(80, 20) };
            NumericUpDown numWeight = new NumericUpDown() { Location = new System.Drawing.Point(100, 108), Size = new System.Drawing.Size(100, 23), Name = "numWeight", Maximum = 10000, DecimalPlaces = 2 };

            Label lblVolume = new Label() { Text = "Volume (m³):", Location = new System.Drawing.Point(10, 140), Size = new System.Drawing.Size(80, 20) };
            NumericUpDown numVolume = new NumericUpDown() { Location = new System.Drawing.Point(100, 138), Size = new System.Drawing.Size(100, 23), Name = "numVolume", Maximum = 1000, DecimalPlaces = 2 };

            Label lblPackageType = new Label() { Text = "Package Type:", Location = new System.Drawing.Point(10, 170), Size = new System.Drawing.Size(80, 20) };
            ComboBox cmbPackageType = new ComboBox() { Location = new System.Drawing.Point(100, 168), Size = new System.Drawing.Size(120, 23), Name = "cmbPackageType" };
            cmbPackageType.Items.AddRange(new string[] { "Furniture", "Electronics", "Clothing", "Books", "Kitchenware", "Fragile Items", "Other" });

            Label lblTransportUnit = new Label() { Text = "Transport Unit:", Location = new System.Drawing.Point(10, 200), Size = new System.Drawing.Size(80, 20) };
            ComboBox cmbTransportUnit = new ComboBox() { Location = new System.Drawing.Point(100, 198), Size = new System.Drawing.Size(200, 23), Name = "cmbTransportUnit", DropDownStyle = ComboBoxStyle.DropDownList };

            Button btnAddLoad = new Button() { Text = "Add Load", Location = new System.Drawing.Point(100, 240), Size = new System.Drawing.Size(100, 30), BackColor = System.Drawing.Color.Blue, ForeColor = System.Drawing.Color.White };
            Button btnClearLoad = new Button() { Text = "Clear", Location = new System.Drawing.Point(210, 240), Size = new System.Drawing.Size(70, 30) };

            // Load list
            ListView loadList = new ListView();
            loadList.Location = new System.Drawing.Point(440, 20);
            loadList.Size = new System.Drawing.Size(700, 400);
            loadList.View = View.Details;
            loadList.FullRowSelect = true;
            loadList.GridLines = true;
            loadList.Name = "loadList";

            loadList.Columns.Add("Load ID", 80);
            loadList.Columns.Add("Job ID", 80);
            loadList.Columns.Add("Description", 150);
            loadList.Columns.Add("Weight", 80);
            loadList.Columns.Add("Volume", 80);
            loadList.Columns.Add("Package Type", 100);
            loadList.Columns.Add("Transport Unit", 120);

            // Event handlers
            btnAddLoad.Click += async (s, e) => await AddLoadAsync(cmbJob, txtDescription, numWeight, numVolume, cmbPackageType, cmbTransportUnit, loadList);
            btnClearLoad.Click += (s, e) => ClearLoadForm(cmbJob, txtDescription, numWeight, numVolume, cmbPackageType, cmbTransportUnit);

            formPanel.Controls.AddRange(new Control[] { lblTitle, lblJob, cmbJob, lblDescription, txtDescription, lblWeight, numWeight, lblVolume, numVolume, lblPackageType, cmbPackageType, lblTransportUnit, cmbTransportUnit, btnAddLoad, btnClearLoad });

            tab.Controls.Add(formPanel);
            tab.Controls.Add(loadList);

            return tab;
        }

        private TabPage CreateReportsTab()
        {
            TabPage tab = new TabPage("Reports");

            // Reports panel
            Panel reportsPanel = new Panel();
            reportsPanel.Size = new System.Drawing.Size(1100, 500);
            reportsPanel.Location = new System.Drawing.Point(20, 20);
            reportsPanel.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitle = new Label() { Text = "Management Reports", Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(300, 30) };

            // Report buttons
            Button btnCustomerReport = new Button() { Text = "Customer Report", Location = new System.Drawing.Point(20, 50), Size = new System.Drawing.Size(120, 30), BackColor = System.Drawing.Color.Green, ForeColor = System.Drawing.Color.White };
            Button btnJobReport = new Button() { Text = "Job Report", Location = new System.Drawing.Point(150, 50), Size = new System.Drawing.Size(120, 30), BackColor = System.Drawing.Color.Blue, ForeColor = System.Drawing.Color.White };
            Button btnTransportReport = new Button() { Text = "Transport Report", Location = new System.Drawing.Point(280, 50), Size = new System.Drawing.Size(120, 30), BackColor = System.Drawing.Color.Orange, ForeColor = System.Drawing.Color.White };
            Button btnRevenueReport = new Button() { Text = "Revenue Report", Location = new System.Drawing.Point(410, 50), Size = new System.Drawing.Size(120, 30), BackColor = System.Drawing.Color.Purple, ForeColor = System.Drawing.Color.White };

            // Report display area
            RichTextBox rtbReport = new RichTextBox();
            rtbReport.Location = new System.Drawing.Point(20, 100);
            rtbReport.Size = new System.Drawing.Size(1060, 380);
            rtbReport.Font = new System.Drawing.Font("Courier New", 10F);
            rtbReport.ReadOnly = true;
            rtbReport.Name = "rtbReport";

            // Event handlers
            btnCustomerReport.Click += async (s, e) => await GenerateCustomerReportAsync(rtbReport);
            btnJobReport.Click += async (s, e) => await GenerateJobReportAsync(rtbReport);
            btnTransportReport.Click += async (s, e) => await GenerateTransportReportAsync(rtbReport);
            btnRevenueReport.Click += async (s, e) => await GenerateRevenueReportAsync(rtbReport);

            reportsPanel.Controls.AddRange(new Control[] { lblTitle, btnCustomerReport, btnJobReport, btnTransportReport, btnRevenueReport, rtbReport });
            tab.Controls.Add(reportsPanel);

            return tab;
        }

        // Event Handler Methods (Updated for async)
        private async Task AddCustomerAsync(TextBox firstName, TextBox lastName, TextBox email, TextBox phone, TextBox address, ListView list)
        {
            if (string.IsNullOrWhiteSpace(firstName.Text) || string.IsNullOrWhiteSpace(lastName.Text))
            {
                MessageBox.Show("Please enter both first name and last name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var customer = new Customer
                {
                    FirstName = firstName.Text.Trim(),
                    LastName = lastName.Text.Trim(),
                    Email = email.Text.Trim(),
                    Phone = phone.Text.Trim(),
                    Address = address.Text.Trim(),
                    RegistrationDate = DateTime.Now
                };

                await _dataManager.AddCustomerAsync(customer);
                await RefreshCustomerListAsync(list);
                await RefreshCustomerComboBoxesAsync();
                ClearCustomerForm(firstName, lastName, email, phone, address);
                MessageBox.Show("Customer added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddTransportUnitAsync(TextBox lorryNumber, TextBox driverName, TextBox driverLicense, TextBox assistantName, ComboBox containerType, NumericUpDown capacity, ListView list)
        {
            if (string.IsNullOrWhiteSpace(lorryNumber.Text) || string.IsNullOrWhiteSpace(driverName.Text))
            {
                MessageBox.Show("Please enter lorry number and driver name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var transportUnit = new TransportUnit
                {
                    LorryNumber = lorryNumber.Text.Trim(),
                    DriverName = driverName.Text.Trim(),
                    DriverLicense = driverLicense.Text.Trim(),
                    AssistantName = assistantName.Text.Trim(),
                    ContainerType = containerType.Text,
                    Capacity = capacity.Value,
                    IsAvailable = true
                };

                await _dataManager.AddTransportUnitAsync(transportUnit);
                await RefreshTransportUnitListAsync(list);
                await RefreshTransportUnitComboBoxesAsync();
                ClearTransportUnitForm(lorryNumber, driverName, driverLicense, assistantName, containerType, capacity);
                MessageBox.Show("Transport unit added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding transport unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddJobAsync(ComboBox customer, TextBox startLocation, TextBox destination, DateTimePicker scheduledDate, NumericUpDown totalCost, ListView list)
        {
            if (customer.SelectedItem == null || string.IsNullOrWhiteSpace(startLocation.Text) || string.IsNullOrWhiteSpace(destination.Text))
            {
                MessageBox.Show("Please select a customer and enter both start location and destination.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedCustomer = (Customer)customer.SelectedItem;
                var job = new Job
                {
                    CustomerId = selectedCustomer.CustomerId,
                    StartLocation = startLocation.Text.Trim(),
                    Destination = destination.Text.Trim(),
                    ScheduledDate = scheduledDate.Value,
                    TotalCost = totalCost.Value,
                    Status = "Pending",
                    RequestDate = DateTime.Now
                };

                await _dataManager.AddJobAsync(job);
                await RefreshJobListAsync(list);
                await RefreshJobComboBoxesAsync();
                ClearJobForm(customer, startLocation, destination, scheduledDate, totalCost);
                MessageBox.Show("Job added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding job: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddLoadAsync(ComboBox job, TextBox description, NumericUpDown weight, NumericUpDown volume, ComboBox packageType, ComboBox transportUnit, ListView list)
        {
            if (job.SelectedItem == null || string.IsNullOrWhiteSpace(description.Text))
            {
                MessageBox.Show("Please select a job and enter description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedJob = (Job)job.SelectedItem;
                var selectedTransportUnit = (TransportUnit)transportUnit.SelectedItem;

                var load = new Load
                {
                    JobId = selectedJob.JobId,
                    Description = description.Text.Trim(),
                    Weight = weight.Value,
                    Volume = volume.Value,
                    PackageType = packageType.Text,
                    TransportUnitId = selectedTransportUnit?.TransportUnitId ?? 0
                };

                await _dataManager.AddLoadAsync(load);

                // Mark transport unit as unavailable when assigned to a job
                if (selectedTransportUnit != null)
                {
                    await _dataManager.UpdateTransportUnitAvailabilityAsync(selectedTransportUnit.TransportUnitId, false);
                }

                await RefreshLoadListAsync(list);
                await RefreshTransportUnitComboBoxesAsync();
                await RefreshTransportUnitListAsync(FindControl<ListView>("transportList"));
                ClearLoadForm(job, description, weight, volume, packageType, transportUnit);
                MessageBox.Show("Load added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding load: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateJobStatusAsync(ListView jobList, ComboBox statusCombo)
        {
            if (jobList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a job to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (statusCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedItem = jobList.SelectedItems[0];
                int jobId = int.Parse(selectedItem.Text);
                string newStatus = statusCombo.SelectedItem.ToString();

                await _dataManager.UpdateJobStatusAsync(jobId, newStatus);

                // If job is completed or cancelled, make the transport unit available again
                if (newStatus == "Completed" || newStatus == "Cancelled")
                {
                    var jobLoads = await _dataManager.GetLoadsByJobIdAsync(jobId);
                    foreach (var load in jobLoads)
                    {
                        if (load.TransportUnitId > 0)
                        {
                            await _dataManager.UpdateTransportUnitAvailabilityAsync(load.TransportUnitId, true);
                        }
                    }
                }

                await RefreshJobListAsync(jobList);
                await RefreshTransportUnitComboBoxesAsync();
                await RefreshTransportUnitListAsync(FindControl<ListView>("transportList"));
                MessageBox.Show("Job status updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating job status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear form methods
        private void ClearCustomerForm(TextBox firstName, TextBox lastName, TextBox email, TextBox phone, TextBox address)
        {
            firstName.Clear();
            lastName.Clear();
            email.Clear();
            phone.Clear();
            address.Clear();
        }

        private void ClearTransportUnitForm(TextBox lorryNumber, TextBox driverName, TextBox driverLicense, TextBox assistantName, ComboBox containerType, NumericUpDown capacity)
        {
            lorryNumber.Clear();
            driverName.Clear();
            driverLicense.Clear();
            assistantName.Clear();
            containerType.SelectedIndex = -1;
            capacity.Value = capacity.Minimum;
        }

        private void ClearJobForm(ComboBox customer, TextBox startLocation, TextBox destination, DateTimePicker scheduledDate, NumericUpDown totalCost)
        {
            customer.SelectedIndex = -1;
            startLocation.Clear();
            destination.Clear();
            scheduledDate.Value = DateTime.Now;
            totalCost.Value = 0;
        }

        private void ClearLoadForm(ComboBox job, TextBox description, NumericUpDown weight, NumericUpDown volume, ComboBox packageType, ComboBox transportUnit)
        {
            job.SelectedIndex = -1;
            description.Clear();
            weight.Value = 0;
            volume.Value = 0;
            packageType.SelectedIndex = -1;
            transportUnit.SelectedIndex = -1;
        }

        // Refresh methods
        private async Task RefreshAllTabsAsync()
        {
            try
            {
                // Load all data
                _customers = await _dataManager.GetAllCustomersAsync();
                _jobs = await _dataManager.GetAllJobsAsync();
                _loads = await _dataManager.GetAllLoadsAsync();
                _transportUnits = await _dataManager.GetAllTransportUnitsAsync();

                // Refresh all lists
                await RefreshCustomerListAsync(FindControl<ListView>("customerList"));
                await RefreshTransportUnitListAsync(FindControl<ListView>("transportList"));
                await RefreshJobListAsync(FindControl<ListView>("jobList"));
                await RefreshLoadListAsync(FindControl<ListView>("loadList"));

                // Refresh combo boxes
                await RefreshCustomerComboBoxesAsync();
                await RefreshJobComboBoxesAsync();
                await RefreshTransportUnitComboBoxesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshCustomerListAsync(ListView list)
        {
            if (list == null) return;

            list.Items.Clear();
            _customers = await _dataManager.GetAllCustomersAsync();

            foreach (var customer in _customers)
            {
                var item = new ListViewItem(customer.CustomerId.ToString());
                item.SubItems.Add($"{customer.FirstName} {customer.LastName}");
                item.SubItems.Add(customer.Email);
                item.SubItems.Add(customer.Phone);
                item.SubItems.Add(customer.RegistrationDate.ToString("yyyy-MM-dd"));
                item.Tag = customer;
                list.Items.Add(item);
            }
        }

        private async Task RefreshTransportUnitListAsync(ListView list)
        {
            if (list == null) return;

            list.Items.Clear();
            _transportUnits = await _dataManager.GetAllTransportUnitsAsync();

            foreach (var unit in _transportUnits)
            {
                var item = new ListViewItem(unit.LorryNumber);
                item.SubItems.Add(unit.DriverName);
                item.SubItems.Add(unit.DriverLicense);
                item.SubItems.Add(unit.AssistantName);
                item.SubItems.Add(unit.ContainerType);
                item.SubItems.Add(unit.Capacity.ToString());
                item.SubItems.Add(unit.IsAvailable ? "Yes" : "No");
                item.Tag = unit;
                list.Items.Add(item);
            }
        }

        private async Task RefreshJobListAsync(ListView list)
        {
            if (list == null) return;

            list.Items.Clear();
            _jobs = await _dataManager.GetAllJobsAsync();

            foreach (var job in _jobs)
            {
                var customer = _customers.FirstOrDefault(c => c.CustomerId == job.CustomerId);
                var item = new ListViewItem(job.JobId.ToString());
                item.SubItems.Add(customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown");
                item.SubItems.Add(job.StartLocation);
                item.SubItems.Add(job.Destination);
                item.SubItems.Add(job.Status);
                item.SubItems.Add(job.TotalCost.ToString("C"));
                item.SubItems.Add(job.RequestDate.ToString("yyyy-MM-dd"));
                item.Tag = job;
                list.Items.Add(item);
            }
        }

        private async Task RefreshLoadListAsync(ListView list)
        {
            if (list == null) return;

            list.Items.Clear();
            _loads = await _dataManager.GetAllLoadsAsync();

            foreach (var load in _loads)
            {
                var transportUnit = _transportUnits.FirstOrDefault(t => t.TransportUnitId == load.TransportUnitId);
                var item = new ListViewItem(load.LoadId.ToString());
                item.SubItems.Add(load.JobId.ToString());
                item.SubItems.Add(load.Description);
                item.SubItems.Add(load.Weight.ToString("F2"));
                item.SubItems.Add(load.Volume.ToString("F2"));
                item.SubItems.Add(load.PackageType);
                item.SubItems.Add(transportUnit?.LorryNumber ?? "Not Assigned");
                item.Tag = load;
                list.Items.Add(item);
            }
        }

        private async Task RefreshCustomerComboBoxesAsync()
        {
            var customerCombo = FindControl<ComboBox>("cmbCustomer");
            if (customerCombo != null)
            {
                customerCombo.Items.Clear();
                customerCombo.DisplayMember = "FullName";
                customerCombo.ValueMember = "CustomerId";

                foreach (var customer in _customers)
                {
                    customerCombo.Items.Add(customer);
                }
            }
        }

        private async Task RefreshJobComboBoxesAsync()
        {
            var jobCombo = FindControl<ComboBox>("cmbJob");
            if (jobCombo != null)
            {
                jobCombo.Items.Clear();
                jobCombo.DisplayMember = "JobDescription";
                jobCombo.ValueMember = "JobId";

                foreach (var job in _jobs.Where(j => j.Status != "Completed" && j.Status != "Cancelled"))
                {
                    jobCombo.Items.Add(job);
                }
            }
        }

        private async Task RefreshTransportUnitComboBoxesAsync()
        {
            var transportCombo = FindControl<ComboBox>("cmbTransportUnit");
            if (transportCombo != null)
            {
                transportCombo.Items.Clear();
                transportCombo.DisplayMember = "LorryNumber";
                transportCombo.ValueMember = "TransportUnitId";

                // Only show available transport units (not assigned to active jobs)
                var availableUnits = _transportUnits.Where(t => t.IsAvailable).ToList();

                foreach (var unit in availableUnits)
                {
                    transportCombo.Items.Add(unit);
                }
            }
        }

        // Report generation methods
        private async Task GenerateCustomerReportAsync(RichTextBox reportBox)
        {
            try
            {
                var customers = await _dataManager.GetAllCustomersAsync();
                var jobs = await _dataManager.GetAllJobsAsync();

                var report = new StringBuilder();
                report.AppendLine("CUSTOMER REPORT");
                report.AppendLine("================");
                report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine();

                report.AppendLine($"Total Customers: {customers.Count}");
                report.AppendLine();

                foreach (var customer in customers)
                {
                    var customerJobs = jobs.Where(j => j.CustomerId == customer.CustomerId).ToList();
                    var totalRevenue = customerJobs.Sum(j => j.TotalCost);

                    report.AppendLine($"Customer: {customer.FirstName} {customer.LastName}");
                    report.AppendLine($"Email: {customer.Email}");
                    report.AppendLine($"Phone: {customer.Phone}");
                    report.AppendLine($"Registration Date: {customer.RegistrationDate:yyyy-MM-dd}");
                    report.AppendLine($"Total Jobs: {customerJobs.Count}");
                    report.AppendLine($"Total Revenue: {totalRevenue:C}");
                    report.AppendLine(new string('-', 50));
                }

                reportBox.Text = report.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating customer report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task GenerateJobReportAsync(RichTextBox reportBox)
        {
            try
            {
                var jobs = await _dataManager.GetAllJobsAsync();
                var customers = await _dataManager.GetAllCustomersAsync();

                var report = new StringBuilder();
                report.AppendLine("JOB REPORT");
                report.AppendLine("===========");
                report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine();

                var statusGroups = jobs.GroupBy(j => j.Status);
                foreach (var group in statusGroups)
                {
                    report.AppendLine($"{group.Key}: {group.Count()} jobs");
                }
                report.AppendLine();

                foreach (var job in jobs.OrderBy(j => j.RequestDate))
                {
                    var customer = customers.FirstOrDefault(c => c.CustomerId == job.CustomerId);

                    report.AppendLine($"Job ID: {job.JobId}");
                    report.AppendLine($"Customer: {customer?.FirstName} {customer?.LastName}");
                    report.AppendLine($"From: {job.StartLocation}");
                    report.AppendLine($"To: {job.Destination}");
                    report.AppendLine($"Status: {job.Status}");
                    report.AppendLine($"Cost: {job.TotalCost:C}");
                    report.AppendLine($"Request Date: {job.RequestDate:yyyy-MM-dd}");
                    report.AppendLine($"Scheduled Date: {job.ScheduledDate:yyyy-MM-dd}");
                    report.AppendLine(new string('-', 50));
                }

                reportBox.Text = report.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating job report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task GenerateTransportReportAsync(RichTextBox reportBox)
        {
            try
            {
                var transportUnits = await _dataManager.GetAllTransportUnitsAsync();
                var loads = await _dataManager.GetAllLoadsAsync();

                var report = new StringBuilder();
                report.AppendLine("TRANSPORT UNIT REPORT");
                report.AppendLine("=====================");
                report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine();

                report.AppendLine($"Total Transport Units: {transportUnits.Count}");
                report.AppendLine($"Available Units: {transportUnits.Count(t => t.IsAvailable)}");
                report.AppendLine($"In Use: {transportUnits.Count(t => !t.IsAvailable)}");
                report.AppendLine();

                foreach (var unit in transportUnits)
                {
                    var unitLoads = loads.Where(l => l.TransportUnitId == unit.TransportUnitId).ToList();
                    var totalWeight = unitLoads.Sum(l => l.Weight);

                    report.AppendLine($"Lorry Number: {unit.LorryNumber}");
                    report.AppendLine($"Driver: {unit.DriverName}");
                    report.AppendLine($"License: {unit.DriverLicense}");
                    report.AppendLine($"Assistant: {unit.AssistantName}");
                    report.AppendLine($"Container Type: {unit.ContainerType}");
                    report.AppendLine($"Capacity: {unit.Capacity} kg");
                    report.AppendLine($"Available: {(unit.IsAvailable ? "Yes" : "No")}");
                    report.AppendLine($"Total Loads Assigned: {unitLoads.Count}");
                    report.AppendLine($"Total Weight Carried: {totalWeight:F2} kg");
                    report.AppendLine(new string('-', 50));
                }

                reportBox.Text = report.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating transport report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task GenerateRevenueReportAsync(RichTextBox reportBox)
        {
            try
            {
                var jobs = await _dataManager.GetAllJobsAsync();
                var customers = await _dataManager.GetAllCustomersAsync();

                var report = new StringBuilder();
                report.AppendLine("REVENUE REPORT");
                report.AppendLine("==============");
                report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine();

                var totalRevenue = jobs.Sum(j => j.TotalCost);
                var completedJobs = jobs.Where(j => j.Status == "Completed").ToList();
                var pendingRevenue = jobs.Where(j => j.Status == "Pending" || j.Status == "Assigned" || j.Status == "InProgress").Sum(j => j.TotalCost);

                report.AppendLine($"Total Revenue (All Jobs): {totalRevenue:C}");
                report.AppendLine($"Completed Jobs Revenue: {completedJobs.Sum(j => j.TotalCost):C}");
                report.AppendLine($"Pending Revenue: {pendingRevenue:C}");
                report.AppendLine();

                // Monthly breakdown
                var monthlyRevenue = jobs.GroupBy(j => new { j.RequestDate.Year, j.RequestDate.Month })
                                       .Select(g => new {
                                           Year = g.Key.Year,
                                           Month = g.Key.Month,
                                           Revenue = g.Sum(j => j.TotalCost),
                                           Count = g.Count()
                                       })
                                       .OrderBy(x => x.Year).ThenBy(x => x.Month);

                report.AppendLine("MONTHLY BREAKDOWN:");
                report.AppendLine();
                foreach (var month in monthlyRevenue)
                {
                    report.AppendLine($"{month.Year}-{month.Month:D2}: {month.Revenue:C} ({month.Count} jobs)");
                }

                report.AppendLine();
                report.AppendLine("TOP CUSTOMERS BY REVENUE:");
                report.AppendLine();

                var customerRevenue = customers.Select(c => new {
                    Customer = c,
                    Revenue = jobs.Where(j => j.CustomerId == c.CustomerId).Sum(j => j.TotalCost),
                    JobCount = jobs.Count(j => j.CustomerId == c.CustomerId)
                }).Where(x => x.Revenue > 0)
                .OrderByDescending(x => x.Revenue)
                .Take(10);

                foreach (var item in customerRevenue)
                {
                    report.AppendLine($"{item.Customer.FirstName} {item.Customer.LastName}: {item.Revenue:C} ({item.JobCount} jobs)");
                }

                reportBox.Text = report.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating revenue report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Button event handlers
        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await RefreshAllTabsAsync();
            MessageBox.Show("Data refreshed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        // Helper method to find controls
        private T FindControl<T>(string name) where T : Control
        {
            return FindControlRecursive<T>(this, name);
        }

        private T FindControlRecursive<T>(Control parent, string name) where T : Control
        {
            foreach (Control control in parent.Controls)
            {
                if (control is T && control.Name == name)
                {
                    return control as T;
                }

                var found = FindControlRecursive<T>(control, name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}