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
using System.Threading;

namespace RecordMenuHook {

    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }

        private IntPtr _GetMsgHook;

        private List<uint> _RecRepo = new List<uint>();

        private void buttonRecord_Click(object sender, EventArgs e) {
            _GetMsgHook = NativeMethod.SetWindowsHookEx(
                NativeEnum.HookType.WH_GETMESSAGE,
                new NativeMethod.HookProc(GetMsgCb), 
                IntPtr.Zero,
                NativeMethod.GetCurrentThreadId()
            );
            if (_GetMsgHook == IntPtr.Zero) {
                MessageBox.Show("Hook Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else {
                listBox1.Items.Clear();
                _RecRepo.Clear();
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

            StringBuilder sb = new StringBuilder();
            foreach (uint id in _RecRepo)
                sb.AppendLine("id: " + id.ToString());
            MessageBox.Show("录制成功。\n\n" + sb.ToString(), "录制", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonPlay_Click(object sender, EventArgs e) {
            listBox1.Items.Add("");

            buttonRecord.Enabled = false;
            buttonStop.Enabled = false;
            buttonPlay.Enabled = false;
            Thread.Sleep(200);

            foreach (uint id in _RecRepo) {
                NativeMethod.SendMessage(this.Handle, (uint) NativeEnum.WindowsMessages.WM_COMMAND, (IntPtr) id, IntPtr.Zero);
                Thread.Sleep(200);
            }

            MessageBox.Show("执行完毕。", "执行", MessageBoxButtons.OK, MessageBoxIcon.Information);
            buttonRecord.Enabled = true;
            buttonStop.Enabled = false;
            buttonPlay.Enabled = false;
        }

        private IntPtr GetMsgCb(int code, IntPtr wParam, IntPtr lParam) {
            if (code >= 0 && wParam.ToInt32() != 0x0001) {
                NativeMethod.MSG pMsg = (NativeMethod.MSG)Marshal.PtrToStructure(lParam, typeof(NativeMethod.MSG));

                // (256) -> 257
                NativeEnum.WindowsMessages msg = (NativeEnum.WindowsMessages)pMsg.message;
                if (msg == NativeEnum.WindowsMessages.WM_COMMAND) {
                    uint id = (uint)pMsg.wParam.ToUInt32() & 0xFFFF;
                    _RecRepo.Add(id);
                    // listBox1.Items.Add(id.ToString());
                    // listBox1.SetSelected(listBox1.Items.Count - 1, true);
                }
            }
            return NativeMethod.CallNextHookEx(_GetMsgHook, code, wParam, lParam);
        }

        private void Menu_Triggered(object sender, EventArgs e) {
            listBox1.Items.Add((sender as MenuItem).Text + " Triggered");
            listBox1.SetSelected(listBox1.Items.Count - 1, true);
        }

        private void MainForm_Load(object sender, EventArgs e) {
            this.Menu = new MainMenu();

            MenuItem item = new MenuItem("Menu1");
            this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("MenuItem1", new EventHandler(Menu_Triggered));

            MenuItem subitem = new MenuItem("MenuItem2");
            subitem.MenuItems.Add("MenuSubItem1", new EventHandler(Menu_Triggered));
            subitem.MenuItems.Add("MenuSubItem2", new EventHandler(Menu_Triggered));
            subitem.MenuItems.Add("MenuSubItem3", new EventHandler(Menu_Triggered));

            item.MenuItems.Add(subitem);
            item.MenuItems.Add("MenuItem3", new EventHandler(Menu_Triggered));
            item.MenuItems.Add("-");
            item.MenuItems.Add("MenuItem4", new EventHandler(Menu_Triggered));

            item = new MenuItem("Menu2");
            this.Menu.MenuItems.Add(item);
            item.MenuItems.Add("MenuItem4", new EventHandler(Menu_Triggered));

            subitem = new MenuItem("MenuItem5");
            item.MenuItems.Add(subitem);

            subitem.MenuItems.Add("MenuSubItem4", new EventHandler(Menu_Triggered));
            subitem.MenuItems.Add("-");
            subitem.MenuItems.Add("MenuSubItem5", new EventHandler(Menu_Triggered));
            subitem.MenuItems.Add("MenuSubItem6", new EventHandler(Menu_Triggered));
        }
    }
}
