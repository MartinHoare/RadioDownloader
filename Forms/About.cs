// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;

namespace RadioDld
{
    internal sealed partial class About : Form
    {
        private void About_Load(object sender, System.EventArgs e)
        {
            // Set the title of the form.
            string applicationTitle = null;
            if (!string.IsNullOrEmpty(new ApplicationBase().Info.Title))
            {
                applicationTitle = new ApplicationBase().Info.Title;
            }
            else
            {
                applicationTitle = System.IO.Path.GetFileNameWithoutExtension(new ApplicationBase().Info.AssemblyName);
            }

            this.Font = SystemFonts.MessageBoxFont;

            this.Text = "About " + applicationTitle;
            this.LabelNameAndVer.Text = new ApplicationBase().Info.ProductName + " " + new ApplicationBase().Info.Version.ToString();
            this.LabelCopyright.Text = new ApplicationBase().Info.Copyright;
        }

        private void OKButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void HomepageLink_Click(object sender, System.EventArgs e)
        {
            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/");
        }

        public About()
        {
            InitializeComponent();
        }
    }
}
