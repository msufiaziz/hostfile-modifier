using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostFile.UI.Modifier.Helpers
{
    static class MenuBuilder
    {
        public static ToolStripMenuItem CreateMenu(string name)
        {
            var menu = new ToolStripMenuItem(name)
            {
                Enabled = false
            };
            return menu;
        }

        public static ToolStripMenuItem AddChildMenu(this ToolStripMenuItem parentMenu, string name)
        {
            parentMenu.Enabled = true;
            parentMenu.DropDownItems.Add(name).Enabled = false;
            return parentMenu;
        }

        public static ToolStripMenuItem AddChildMenu(this ToolStripMenuItem parentMenu, ToolStripMenuItem childMenu)
        {
            parentMenu.Enabled = true;
            parentMenu.DropDownItems.Add(childMenu);
            return parentMenu;
        }

        public static ToolStripMenuItem AddManyChildMenu(this ToolStripMenuItem parentMenu, ToolStripMenuItem[] childMenus)
        {
            parentMenu.Enabled = childMenus.Any();
            parentMenu.DropDownItems.AddRange(childMenus);
            return parentMenu;
        }

        public static ToolStripMenuItem HookClickEvent(this ToolStripMenuItem menu, EventHandler clickEvent)
        {
            menu.Enabled = true;
            menu.Click += clickEvent;
            return menu;
        }

        public static ToolStripSeparator CreateSeparator()
        {
            return new ToolStripSeparator();
        }
    }
}
