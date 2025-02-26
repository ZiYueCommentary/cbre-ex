﻿using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CBRE.QuickForms.Items
{
    /// <summary>
    /// A control that shows a checkbox.
    /// </summary>
    public class QuickFormCheckBox : QuickFormItem
    {
        private readonly bool _value;

        public QuickFormCheckBox(string cbname, bool value)
        {
            _value = value;
            Name = cbname;
        }

        public override List<Control> GetControls(QuickForm qf)
        {
            List<Control> controls = new List<Control>();
            CheckBox c = new CheckBox { Text = Name, Name = Name, Checked = _value, Font = SystemFonts.MessageBoxFont, FlatStyle = FlatStyle.System };
            Size(c, qf, 0);
            c.TextAlign = ContentAlignment.MiddleLeft;
            Location(c, qf, true);
            c.Location = new Point(c.Location.X + QuickForm.ItemPadding, c.Location.Y);
            controls.Add(c);
            return controls;
        }
    }
}
