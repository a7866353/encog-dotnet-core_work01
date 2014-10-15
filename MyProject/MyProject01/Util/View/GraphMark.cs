using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyProject01.Util.View
{
    class GraphMark
    {
        private Panel _partenPanel;
        private Brush _color;
        private int _thickness;
        private Point _point;
        private double _width = 10;
        private double _height = 10;
        
        private Shape _referedShape;

        
        public GraphMark( Panel parentPanel, Point point, Brush color, int thickness = 1 )
        {
            this._partenPanel = parentPanel;
            this._point = point;
            this._color = color;
            this._thickness = thickness; 
        }

        public void Update()
        {
            Remvoe();
            _referedShape = new Ellipse();
            _referedShape.StrokeThickness = _thickness;
            _referedShape.Stroke = this._color;
            _referedShape.Width = (int)_width;
            _referedShape.Height = (int)_height;
            _referedShape.Margin = new Thickness(_point.X, _point.Y, 0, 0);

            _partenPanel.Children.Add(_referedShape);
            
        }

        public void Remvoe()
        {
            if (_referedShape != null)
                _partenPanel.Children.Remove(_referedShape);

        }
    }
}
