using Keyboard2XinputLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Diagnostics;

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Keyboard2XinputGui
{
    partial class Keyboard2XinputGui
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(13, 13);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox1";
            this.textBox.Size = new System.Drawing.Size(528, 364);
            this.textBox.TabIndex = 0;
            // 
            // main Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 389);
            this.Controls.Add(this.textBox);
            this.Name = "Keyboard2Xinput";
            this.Text = "Keyboard2Xinput";
            this.ResumeLayout(false);
            this.PerformLayout();

            // 
            k2x = new Keyboard2Xinput(mappingFile);
            _proc = HookCallback;

            SetHook(_proc);
        }

        #endregion

        private System.Windows.Forms.TextBox textBox;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private Keyboard2Xinput k2x;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;
        private String mappingFile;

        private IntPtr SetHook(LowLevelKeyboardProc proc)

        {

            using (Process curProcess = Process.GetCurrentProcess())

            using (ProcessModule curModule = curProcess.MainModule)

            {

                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,

                    GetModuleHandle(curModule.ModuleName), 0);

            }

        }

        private delegate IntPtr LowLevelKeyboardProc(

            int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(

            int nCode, IntPtr wParam, IntPtr lParam)

        {

            //if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Boolean handled = k2x.keyEvent((int)wParam, (Keys)vkCode);
                if (handled)
                {
                    //Console.WriteLine("Intercepted: "+(Keys)vkCode);
                    return (IntPtr)(-1);
                }
                else
                {
                    //Console.WriteLine("Not intercepted: " + (Keys)vkCode);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);

        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr SetWindowsHookEx(int idHook,

            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,

            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

}


