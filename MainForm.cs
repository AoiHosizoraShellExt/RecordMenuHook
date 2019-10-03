using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RecordMenuHook {

    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }

        private IntPtr _GetMsgHook;

        private void buttonRecord_Click(object sender, EventArgs e) {
            _GetMsgHook = NativeMethod.SetWindowsHookEx(
                NativeMethod.HookType.WH_GETMESSAGE,
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
            return NativeMethod.CallNextHookEx(_GetMsgHook, code, wParam, lParam);
        }
    }
}
