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
using System.Diagnostics;

namespace RecordMenuHook {

    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }

        private IntPtr _GetMsgHook, _KeyBrdHook;

        private struct RecStruc {
            public uint Id;
            public bool IsMenuId;
            public RecStruc(uint Id, bool IsMenuId) {
                this.Id = Id;
                this.IsMenuId = IsMenuId;
            }
        }

        private List<RecStruc> _RecRepo = new List<RecStruc>();

        private void buttonRecord_Click(object sender, EventArgs e) {
            _GetMsgHook = NativeMethod.SetWindowsHookEx(
                NativeEnum.HookType.WH_GETMESSAGE,
                new NativeMethod.HookProc(GetMsgCb), 
                IntPtr.Zero,
                NativeMethod.GetCurrentThreadId()
            );
            _KeyBrdHook = NativeMethod.SetWindowsHookEx(
                NativeEnum.HookType.WH_KEYBOARD_LL,
                new NativeMethod.HookProc(KeyBrdCb),
                IntPtr.Zero,
                0
            );

            if (_GetMsgHook == IntPtr.Zero || _KeyBrdHook == IntPtr.Zero)
                MessageBox.Show("Hook Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else {
                listBox1.Items.Clear();
                textBox1.Clear();
                _RecRepo.Clear();
                buttonRecord.Enabled = false;
                buttonStop.Enabled = true;
                buttonPlay.Enabled = false;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e) {
            NativeMethod.UnhookWindowsHookEx(_GetMsgHook);
            NativeMethod.UnhookWindowsHookEx(_KeyBrdHook);
            _GetMsgHook = _KeyBrdHook = IntPtr.Zero;

            buttonRecord.Enabled = true;
            buttonStop.Enabled = false;
            buttonPlay.Enabled = true;

            StringBuilder sb = new StringBuilder();
            foreach (RecStruc rec in _RecRepo) {
                if (rec.IsMenuId)
                    sb.AppendLine("menuid: " + rec.Id.ToString());
                else
                    sb.AppendLine("vkCode: " + rec.Id.ToString());
            }
            MessageBox.Show("录制成功。\n\n" + sb.ToString(),
                "录制", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonPlay_Click(object sender, EventArgs e) {
            listBox1.Items.Add("");
            textBox1.Clear();

            buttonRecord.Enabled = false;
            buttonStop.Enabled = false;
            buttonPlay.Enabled = false;
            Thread.Sleep(200);

            foreach (RecStruc rec in _RecRepo) {
                if (rec.IsMenuId)
                    NativeMethod.SendMessage(this.Handle, 
                        (uint)NativeEnum.WindowsMessages.WM_COMMAND, (IntPtr)rec.Id, IntPtr.Zero);
                else 
                    NativeMethod.SendMessage(this.textBox1.Handle, 
                        (uint)NativeEnum.WindowsMessages.WM_CHAR, (IntPtr)rec.Id, IntPtr.Zero);
                Thread.Sleep(200);
            }

            MessageBox.Show("执行完毕。\n\n总共有 " + _RecRepo.Count + " 个操作", "执行", MessageBoxButtons.OK, MessageBoxIcon.Information);
            buttonRecord.Enabled = true;
            buttonStop.Enabled = false;
            buttonPlay.Enabled = false;
        }

        private IntPtr GetMsgCb(int code, IntPtr wParam, IntPtr lParam) {
            const uint PM_REMOVE = 0x0001;
            if (code >= 0 && wParam.ToInt32() != PM_REMOVE) {
                NativeMethod.MSG pMsg = (NativeMethod.MSG)Marshal.PtrToStructure(
                    lParam, typeof(NativeMethod.MSG)
                );

                NativeEnum.WindowsMessages msg = (NativeEnum.WindowsMessages)pMsg.message;
                if (msg == NativeEnum.WindowsMessages.WM_COMMAND) {
                    uint id = (uint)pMsg.wParam.ToUInt32() & 0xFFFF;
                    _RecRepo.Add(new RecStruc(id, true));
                }
            }
            return NativeMethod.CallNextHookEx(_GetMsgHook, code, wParam, lParam);
        }

        private IntPtr KeyBrdCb(int code, IntPtr wParam, IntPtr lParam) {
            if (code >= 0 && wParam == (IntPtr)NativeEnum.WindowsMessages.WM_KEYDOWN) {
                NativeMethod.KBDLLHOOKSTRUCT pKb = (NativeMethod.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(
                    lParam, typeof(NativeMethod.KBDLLHOOKSTRUCT)
                );
                listBox1.Items.Add("vkCode: " + pKb.vkCode);
                _RecRepo.Add(new RecStruc(pKb.vkCode, false));
            }
            return NativeMethod.CallNextHookEx(_KeyBrdHook, code, wParam, lParam);
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
            subitem.MenuItems.Add("-");

            MenuItem subsubitem = new MenuItem("MenuSubItem3");
            subsubitem.MenuItems.Add("MenuSubSubItem1", new EventHandler(Menu_Triggered));
            subsubitem.MenuItems.Add("MenuSubSubItem2", new EventHandler(Menu_Triggered));
            subsubitem.MenuItems.Add("MenuSubSubItem3", new EventHandler(Menu_Triggered));
            subitem.MenuItems.Add(subsubitem);

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

            subsubitem = new MenuItem("MenuSubItem7");
            subsubitem.MenuItems.Add("MenuSubSubItem4", new EventHandler(Menu_Triggered));
            subsubitem.MenuItems.Add("MenuSubSubItem5", new EventHandler(Menu_Triggered));
            subsubitem.MenuItems.Add("MenuSubSubItem6", new EventHandler(Menu_Triggered));
            subitem.MenuItems.Add(subsubitem);

            item.MenuItems.Add("-", new EventHandler(Menu_Triggered));
            item.MenuItems.Add("MenuItem6", new EventHandler(Menu_Triggered));

        }
    }
}
