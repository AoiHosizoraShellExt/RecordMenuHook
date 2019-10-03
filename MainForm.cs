using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace RecordMenuHook {

    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }

        private IntPtr _GetMsgHook;

        private void buttonRecord_Click(object sender, EventArgs e) {
            _GetMsgHook = NativeMethod.SetWindowsHookEx(
                NativeEnum.HookType.WH_GETMESSAGE,
                new NativeMethod.HookProc(GetMsgCb), 
                IntPtr.Zero,
                NativeMethod.GetCurrentThreadId()
            );
            if (_GetMsgHook == IntPtr.Zero) {

            } else {
                buttonRecord.Enabled = false;
                buttonStop.Enabled = true;
                buttonPlay.Enabled = false;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e) {
            NativeMethod.UnhookWindowsHookEx(_GetMsgHook);
            _GetMsgHook = IntPtr.Zero;

            buttonRecord.Enabled = true;
            buttonStop.Enabled = false;
            buttonPlay.Enabled = true;
        }

        private void buttonPlay_Click(object sender, EventArgs e) {
            // buttonRecord.Enabled = false;
            // buttonStop.Enabled = false;
            // buttonPlay.Enabled = false;
            buttonRecord.Enabled = true;
            buttonStop.Enabled = false;
            buttonPlay.Enabled = false;
        }

        private IntPtr GetMsgCb(int code, IntPtr wParam, IntPtr lParam) {
            if (code >= 0) {
                NativeMethod.MSG pMsg = (NativeMethod.MSG)Marshal.PtrToStructure(lParam, typeof(NativeMethod.MSG));

                NativeEnum.WindowsMessages msg = (NativeEnum.WindowsMessages) pMsg.message;
                string str = msg.ToString();
                if (!str.Equals("WM_NCMOUSEMOVE") && !str.Equals("WM_MOUSEMOVE") &&
                    !str.Equals("WM_NCMOUSEHOVER") && !str.Equals("WM_MOUSEHOVER") &&
                    !str.Equals("WM_NCMOUSELEAVE") && !str.Equals("WM_MOUSELEAVE") &&
                    !str.Equals("WM_TIMER") && !str.Equals("WM_PAINT") && !str.Equals("280"))
                    listBox1.Items.Add(str);

                if (listBox1.Items.Count >= 1)
                    listBox1.SetSelected(listBox1.Items.Count - 1, true);

// 
//                 auto pMsg = (MSG*)lParam;
//                 // if (msg != 0 && pMsg->message != msg && pMsg->message != msg2 && wParam == PM_REMOVE) {
//                 if (msg != 0 && pMsg->message != msg && pMsg->message != msg2) {
//                     if (pMsg->message == WM_SYSCOMMAND) {
//                         SendNotifyMessage(hwndMain, msg, (WPARAM)pMsg->hwnd, pMsg->message);
//                         SendNotifyMessage(hwndMain, msg2, pMsg->wParam, pMsg->lParam);
//                     }
//                 }
            }
            return NativeMethod.CallNextHookEx(_GetMsgHook, code, wParam, lParam);
        }

        private void menuItem4ToolStripMenuItem_Click(object sender, EventArgs e) {
            listBox1.Items.Add("menuItem4");
        }
    }
}
