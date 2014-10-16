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
    public class GraphLine
    {        
        private Panel _partenPanel;
        private Brush _color;
        private int _thickness;
        private double _width = 10;
        private double _height = 10;

        private Path _referedShape;
        private List<GraphMark> _graphMarkList;
        private double[] _dataArray;

        public double ScaleX = 1;
        public double ScaleY = 1000;

        public GraphLine(Panel parentPanel, double[] dataArray, Brush color, int thickness = 1)
        {
            this._partenPanel = parentPanel;
            this._dataArray = dataArray;
            this._color = color;
            this._thickness = thickness;

            _graphMarkList = new List<GraphMark>();

        }

        public void AddMark(int index)
        {
            GraphMark mark = new GraphMark(_partenPanel, new Point(index, _dataArray[index]), Brushes.Black, 1);
            mark.ScaleX = ScaleX;
            mark.ScaleY = ScaleY;
            _graphMarkList.Add(mark);
        }

        public void Update()
        {
            bool isNew = false;
            if (_referedShape == null)
            {
                _referedShape = new Path();
                isNew = true;
            }
            _referedShape.StrokeThickness = _thickness;
            _referedShape.Stroke = this._color;
            _referedShape.Data = GetLine();
            if(isNew == true)
                _partenPanel.Children.Add(_referedShape);

            // Update Mark
            foreach(GraphMark mark in _graphMarkList)
            {
                mark.ScaleX = ScaleX;
                mark.ScaleY = ScaleY;
                mark.Update();
            }
            
        }

        public void Remvoe()
        {
            if (_referedShape != null)
            {
                _partenPanel.Children.Remove(_referedShape);
                _referedShape = null;

                foreach (GraphMark mark in _graphMarkList)
                {
                    mark.Remvoe();
                    _graphMarkList.Remove(mark);
                }

            }

        }

        private Geometry GetLine()
        {
            Point[] testPoints = new Point[_dataArray.Length];
            for (int i = 0; i < testPoints.Length; i++)
            {
                testPoints[i] = new Point(i * ScaleX, _dataArray[i] * ScaleY);
            }

            Geometry geo = PathTest.DrawLineByPointArr(testPoints);
            return geo;
        }
    }
}
