using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keyboard2XinputGui
{
    public partial class Keyboard2XinputGui : Form
    {
        public Keyboard2XinputGui(string aMappingFile)
        {
            mappingFile = aMappingFile;
            InitializeComponent();
            InitK2x();
        }
        
        private void notifyIcon1_DoubleClick_1(object sender, EventArgs e)
        {
            //Show();
            //WindowState = FormWindowState.Normal;
        }

        private void Keyboard2XinputGui_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                // hide from the task bar
                Hide();
                // show system tray icon
                notifyIcon1.Visible = true;
            }
        }

        private void Keyboard2XinputGui_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void EnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            k2x.ToggleEnabled();
            //EnableToolStripMenuItem.
        }

        private void ContextMenuOpening(object sender, CancelEventArgs e)
        {
            // make sure the 'enabled' item is up to date
            ContextMenuStrip menu = (ContextMenuStrip)sender;
            ToolStripItem[] item = menu.Items.Find("EnableToolStripMenuItem", true);
            if (item.Length > 0)
            {
                ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)item[0];
                toolStripMenuItem.Checked = k2x.IsEnabled();
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }
    }
}
