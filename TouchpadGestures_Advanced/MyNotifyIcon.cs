using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace TouchpadGestures_Advanced
{
    public class MyNotifyIcon
    {
        private NotifyIcon icon;
        private ContextMenuStrip contextMenu;

        public MyNotifyIcon()
        {
            icon = new NotifyIcon();
            icon.Icon = new System.Drawing.Icon(App.TGA_AppData + @"\Icon\TGA_icon.ico");
            icon.Visible = true;
            icon.Text = "TouchpadGestures Advanced";

            //Menuのインスタンス化
            contextMenu = new ContextMenuStrip();

            //MenuItemの作成
            var menuItemExit = new ToolStripMenuItem();
            menuItemExit.Text = "Exit";
            menuItemExit.Click += (s, e) =>
            {
                icon.Dispose();
                System.Windows.Application.Current.Shutdown(0);
            };
            var menuItemSettings = new ToolStripMenuItem();
            menuItemSettings.Text = "Settings";
            menuItemSettings.Click += (s, e) =>
            {
            };

            //MenuにMenuItemを追加
            contextMenu.Items.Add(menuItemSettings);
            contextMenu.Items.Add(menuItemExit);

            //Menuをタスクトレイのアイコンに追加
            icon.ContextMenuStrip = contextMenu;
            icon.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.Invoke(icon, null);
                }
            };
        }

        public void Dispose()
        {
            icon.Dispose();
        }

        ~MyNotifyIcon()
        {
            icon.Dispose();
        }
            
    }
}
