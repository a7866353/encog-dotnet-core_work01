using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyProject01.Util.View
{

    class RateSetUtility
    {
        public double maxValue;
        public double minValue;
        public double[] rateSetArr;

        public List<int> markList;

        public double posX = 0;
        public double posY = 0;
        public double scaleX = 1.0;
        public double scaleY = 1.0;
        public RateSetUtility(RateSet[] rateSetArr)
        {
            double[] dataArr = new double[rateSetArr.Length];
            for (int i = 0; i < dataArr.Length; i++)
            {
                dataArr[i] = rateSetArr[i].Value;
            }
            setData(dataArr);
            markList = new List<int>();
            
        }

        public RateSetUtility(double[] rateSetArr)
        {
            setData(rateSetArr);
            markList = new List<int>();
        }
        public void AddMark(int markIndex)
        {
            markList.Add(markIndex);
        }

        private void setData(double[] rateSetArr)
        {
            this.rateSetArr = rateSetArr;
            maxValue = minValue = rateSetArr[0];
            for (int i = 0; i < rateSetArr.Length; i++)
            {
                // maxValue
                if (rateSetArr[i] > maxValue)
                    maxValue = rateSetArr[i];

                if (rateSetArr[i] < minValue)
                    minValue = rateSetArr[i];
            }
        }

        public Geometry GetLine()
        {
            Point[] testPoints = new Point[rateSetArr.Length];
            for (int i = 0; i < testPoints.Length; i++)
            {
                testPoints[i] = new Point(posX + i * scaleX, posY + rateSetArr[i] * scaleY);
            }

            Geometry geo = PathTest.DrawLineByPointArr(testPoints);
            return geo;
        }
        public GeometryGroup GetMark()
        {
            double radiusX = 5;
            double radiusY = 20;
            GeometryGroup markGeoGroup = new GeometryGroup();
            foreach (int index in markList)
            {
                Point p = new Point(posX + index * scaleX, posY + rateSetArr[index] * scaleY);
                Geometry geo = new EllipseGeometry(p, radiusX, radiusY);
                markGeoGroup.Children.Add(geo);
            }
            return markGeoGroup;
        }
    }


    /// <summary>
    /// GraphViewer.xaml 的交互逻辑
    /// </summary>
    public partial class GraphViewer : Window
    {
        private Canvas target;
        private TransformGroup transGroup;
        private ScaleTransform scale;
        private TranslateTransform translate;
        private GeometryGroup geometrys;

        private List<GraphLine> _graphLineList;
        //-------------
        private double mouseWheelStep = 0.1;
        private Point mouseLastPoint;

        //------------------------------
        private static GraphViewer _Instance = null;
        public static GraphViewer Instance
        {
            get
            {
                return _Instance;
            }
        }

        //-------------------
        private double transformPosX = 0;
        private double transformPosY = 0;
        private double transformScaleX = 0;
        private double transformScaleY = 0;
        private double transformPosStep = 10;
        private double transformScaleStep = 1.2;

        //-------------------------
        private List<RateSetUtility> RateSetDataList;


        //-------------------------
        private delegate void func();

        public GraphViewer()
        {
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            this.MouseWheel += new MouseWheelEventHandler(MainWindow_MouseWheel);
            this.Loaded += MainWindow_Loaded;
            GraphViewer._Instance = this;

            RateSetDataList = new List<RateSetUtility>();
            _graphLineList = new List<GraphLine>();

        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Init Panel
            target = new Canvas();
            transGroup = new TransformGroup();
            // target.ClipToBounds = true;
            scale = new ScaleTransform(1, 1);
            transGroup.Children.Add(scale);
            translate = new TranslateTransform(0, 0);
            transGroup.Children.Add(translate);
            // target.RenderTransform = transGroup;
            this.Content = target;
            /*
                        target.MaxWidth = 5000;
                        target.MaxHeight = 5000;
              */
            // Create a path to draw a geometry with.
            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.RenderTransform = transGroup;

            target.Children.Add(myPath);
            geometrys = new GeometryGroup();
            myPath.Data = geometrys;

            // DataLoader dataLoader = new DataLoader();
            // AddRateSet(dataLoader.ToArray());

        }


        private void printTrans()
        {
            System.Console.WriteLine("Trans:" + scale.CenterX.ToString() + "\t"
                + scale.CenterY.ToString() + "\t"
                + scale.ScaleX.ToString() + "\t"
                + scale.ScaleY.ToString());
        }
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool isImage = false;
            if (target == null)
                return;

            if (isImage == true)
            {
                Point moucePoint = Mouse.GetPosition(target);
                System.Console.WriteLine("Mouse: " + moucePoint.ToString());
                if (mouseLastPoint == null)
                    mouseLastPoint = moucePoint;
                if (Keyboard.GetKeyStates(Key.LeftShift) == KeyStates.Down)
                {
                    if (e.Delta > 0)
                    {
                        scale.ScaleX += mouseWheelStep;
                    }
                    else if (e.Delta < 0)
                    {
                        scale.ScaleX -= mouseWheelStep;
                    }
                }
                else if (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down)
                {
                    if (e.Delta > 0)
                    {
                        scale.ScaleY += mouseWheelStep;
                    }
                    else if (e.Delta < 0)
                    {
                        scale.ScaleY -= mouseWheelStep;
                    }

                }
                else
                {
                    if (e.Delta > 0)
                    {
                        mouseLastPoint = moucePoint;
                        scale.ScaleX += mouseWheelStep;
                        scale.ScaleY += mouseWheelStep;
                    }
                    else if (e.Delta < 0)
                    {
                        scale.ScaleX -= mouseWheelStep;
                        scale.ScaleY -= mouseWheelStep;
                    }
                }
            }
            else
            {
                if (Keyboard.GetKeyStates(Key.LeftShift) == KeyStates.Down)
                {
                    if (e.Delta > 0)
                    {
                        transformScaleX *= transformScaleStep;
                    }
                    else if (e.Delta < 0)
                    {
                        transformScaleX /= transformScaleStep;
                    }
                }
                else if (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down)
                {
                    if (e.Delta > 0)
                    {
                        transformScaleY *= transformScaleStep;
                    }
                    else if (e.Delta < 0)
                    {
                        transformScaleY /= transformScaleStep;
                    }
                }
                else
                {
                    if (e.Delta > 0)
                    {
                        transformScaleX *= transformScaleStep;
                        transformScaleY *= transformScaleStep;
                    }
                    else if (e.Delta < 0)
                    {
                        transformScaleX /= transformScaleStep;
                        transformScaleY /= transformScaleStep;
                    }
                }
                geometrys.Children.Clear();
                foreach (RateSetUtility set in RateSetDataList)
                {
                    set.scaleX = transformScaleX;
                    set.scaleY = transformScaleY * -1;
                    geometrys.Children.Add(set.GetLine());
                    geometrys.Children.Add(set.GetMark());
                }


            }
            // scale.CenterX = mouseLastPoint.X;
            // scale.CenterY = mouseLastPoint.Y;
            printTrans();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (target == null)
                return;
            const int step = 10;
            switch (e.Key)
            {
                case Key.Left:
                    translate.X -= step;
                    break;
                case Key.Right:
                    translate.X += step;
                    break;
                case Key.Up:
                    translate.Y -= step;
                    break;
                case Key.Down:
                    translate.Y += step;
                    break;

            }
            printTrans();
        }

        #region Pulic_Function
        public GraphLine AddRateSet(RateSet[] rateSetArr)
        {
            double[] dataArray = new double[rateSetArr.Length];
            for (int i = 0; i < dataArray.Length; i++)
                dataArray[i] = rateSetArr[i].Value;
            return AddRateSet(dataArray);


            /*
            RateSetUtility rateSetObj = new RateSetUtility(rateSetArr);
            RateSetDataList.Add(rateSetObj);
            this.Dispatcher.BeginInvoke(new func(delegate
            {
                transformPosX = 0;
                transformPosY = 0;
                transformScaleY = 400 / rateSetObj.maxValue;
                transformScaleX = 0.5;

                rateSetObj.posX = transformPosX;
                rateSetObj.posY = transformPosY;
                rateSetObj.scaleX = transformScaleX;
                rateSetObj.scaleY = transformScaleY;
                geometrys.Children.Add(rateSetObj.GetLine());
            }));
            // Return Index
            return RateSetDataList.Count - 1;
            */
        }
        public GraphLine AddRateSet(double[] rateSetArr)
        {
            /*
            RateSetUtility rateSetObj = new RateSetUtility(rateSetArr);
            RateSetDataList.Add(rateSetObj);
            this.Dispatcher.BeginInvoke(new func(delegate
            {
                transformPosX = 0;
                transformPosY = 0;
                transformScaleY = 400 / rateSetObj.maxValue;
                transformScaleX = 0.5;

                rateSetObj.posX = transformPosX;
                rateSetObj.posY = transformPosY;
                rateSetObj.scaleX = transformScaleX;
                rateSetObj.scaleY = transformScaleY;
                geometrys.Children.Add(rateSetObj.GetLine());
            }));
             // Return Index
            return RateSetDataList.Count - 1;
           */
            GraphLine line = new GraphLine(target, rateSetArr, Brushes.Black, 1);
            _graphLineList.Add(line);
            this.Dispatcher.BeginInvoke(new func(delegate
            {
                line.Update();
            }));
            return line;
        }
        public void AddMark(int index, DealPointInfomation[] markInfoArr)
        {
            int[] markArr = new int[markInfoArr.Length];
            for (int i = 0; i < markInfoArr.Length; i++)
            {
                markArr[i] = markInfoArr[i].Index;
            }
            AddMark(index, markArr);
        }


        public void AddMark(int index, int[] markArr)
        {
            if (RateSetDataList.Count < index + 1)
                return;
            RateSetUtility rateSetObj = RateSetDataList[index];

            foreach (int mark in markArr)
            {
                rateSetObj.markList.Add(mark);
            }
            this.Dispatcher.BeginInvoke(new func(delegate
            {
                geometrys.Children.Add(rateSetObj.GetMark());
            }));

        

        }

        #endregion
    }
}
