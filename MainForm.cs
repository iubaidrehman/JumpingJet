using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseJiggler;

public partial class MainForm : Form
{
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;

    private readonly System.Windows.Forms.Timer _tickTimer;
    private int _secondsRemaining;
    private bool _isRunning;

    // UI Controls
    private readonly Label _lblStatus = new();
    private readonly Label _lblCountdown = new();
    private readonly Label _lblLastAction = new();
    private readonly NumericUpDown _numInterval = new();
    private readonly CheckBox _chkPhysicalJiggle = new();
    private readonly Button _btnToggle = new();
    private readonly NotifyIcon _trayIcon = new();
    private readonly ContextMenuStrip _trayMenu = new();

    public MainForm()
    {
        InitializeUX();

        _tickTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _tickTimer.Tick += TickTimer_Tick;
    }

    private void InitializeUX()
    {
        Text = "Jumping Jet";
        ClientSize = new Size(280, 220);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        // Status Label
        _lblStatus.Text = "Status: Stopped";
        _lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _lblStatus.ForeColor = Color.DimGray;
        _lblStatus.Location = new Point(20, 20);
        _lblStatus.AutoSize = true;

        // Interval Setting
        var lblInterval = new Label { Text = "Interval (sec):", Location = new Point(20, 60), AutoSize = true };
        _numInterval.Location = new Point(120, 58);
        _numInterval.Width = 80;
        _numInterval.Minimum = 5;
        _numInterval.Maximum = 600;
        _numInterval.Value = 60;

        // Mode Toggle
        _chkPhysicalJiggle.Text = "Move Physical Cursor (Teams/Slack)";
        _chkPhysicalJiggle.Location = new Point(20, 90);
        _chkPhysicalJiggle.Width = 240;
        _chkPhysicalJiggle.Checked = true;

        // Countdown & Last Action
        _lblCountdown.Text = "Next execution: --";
        _lblCountdown.Location = new Point(20, 120);
        _lblCountdown.AutoSize = true;

        _lblLastAction.Text = "Last execution: Never";
        _lblLastAction.Location = new Point(20, 145);
        _lblLastAction.AutoSize = true;
        _lblLastAction.ForeColor = Color.Teal;

        // Toggle Button
        _btnToggle.Text = "Start System Keep-Awake";
        _btnToggle.Location = new Point(20, 175);
        _btnToggle.Size = new Size(225, 30);
        _btnToggle.BackColor = Color.FromArgb(0, 120, 215);
        _btnToggle.ForeColor = Color.White;
        _btnToggle.FlatStyle = FlatStyle.Flat;
        _btnToggle.Click += BtnToggle_Click;

        // System Tray Integration
        _trayMenu.Items.Add("Start", null, BtnToggle_Click);
        _trayMenu.Items.Add("Stop", null, BtnToggle_Click);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

        _trayIcon.Text = "Active Session Manager";
        _trayIcon.Icon = SystemIcons.Application; // Replace with a custom .ico if available
        _trayIcon.ContextMenuStrip = _trayMenu;
        _trayIcon.Visible = true;
        _trayIcon.DoubleClick += (s, e) => { Show(); WindowState = FormWindowState.Normal; };

        this.Resize += MainForm_Resize;

        Controls.AddRange(new Control[] { _lblStatus, lblInterval, _numInterval, _chkPhysicalJiggle, _lblCountdown, _lblLastAction, _btnToggle });
    }

    private void BtnToggle_Click(object? sender, EventArgs e)
    {
        _isRunning = !_isRunning;

        if (_isRunning)
        {
            _secondsRemaining = (int)_numInterval.Value;
            _tickTimer.Start();

            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);

            _lblStatus.Text = "Status: RUNNING";
            _lblStatus.ForeColor = Color.ForestGreen;
            _btnToggle.Text = "Stop System Keep-Awake";
            _btnToggle.BackColor = Color.IndianRed;
            _numInterval.Enabled = false;
        }
        else
        {
            _tickTimer.Stop();
            SetThreadExecutionState(ES_CONTINUOUS); // Release OS lock

            _lblStatus.Text = "Status: Stopped";
            _lblStatus.ForeColor = Color.DimGray;
            _lblCountdown.Text = "Next execution: --";
            _btnToggle.Text = "Start System Keep-Awake";
            _btnToggle.BackColor = Color.FromArgb(0, 120, 215);
            _numInterval.Enabled = true;
        }
    }

    private void TickTimer_Tick(object? sender, EventArgs e)
    {
        _secondsRemaining--;
        _lblCountdown.Text = $"Next execution: {_secondsRemaining}s";

        if (_secondsRemaining <= 0)
        {
            ExecuteJiggle();
            _secondsRemaining = (int)_numInterval.Value; // Reset countdown
        }
    }

    private void ExecuteJiggle()
    {
        if (_chkPhysicalJiggle.Checked)
        {
            Point currentPos = Cursor.Position;
            Cursor.Position = new Point(currentPos.X + 1, currentPos.Y);
            System.Threading.Thread.Sleep(50);
            Cursor.Position = currentPos;
            SendKeys.SendWait("{F15}");
        }

        _lblLastAction.Text = $"Last execution: {DateTime.Now:hh:mm:ss tt}";
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        // Minimize to tray logic
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            _trayIcon.ShowBalloonTip(2000, "Minimized", "Running in background to keep system awake.", ToolTipIcon.Info);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Ensure OS lock is released before exiting
        if (_isRunning)
        {
            SetThreadExecutionState(ES_CONTINUOUS);
        }
        _trayIcon.Dispose();
        base.OnFormClosing(e);
    }
}