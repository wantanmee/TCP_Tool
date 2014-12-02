using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TCP_Tool
{
    class ExceptionHandler
    {
        static public void Handle(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
