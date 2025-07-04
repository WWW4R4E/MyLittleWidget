using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLittleWidget.CustomBase
{
    internal class WidgetState
    {
        public Guid Id { get; init; } 
        public string Name { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
