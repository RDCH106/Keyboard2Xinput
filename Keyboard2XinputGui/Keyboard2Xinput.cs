using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keyboard2XinputGui
{
    public partial class Keyboard2XinputGui : Form
    {
        public Keyboard2XinputGui(string aMappingFile)
        {
            mappingFile = aMappingFile;
            InitializeComponent();
        }
    }
}
