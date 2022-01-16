using GraphicsLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isDrawing;
        Point2D anchor = new Point2D(-1, -1);
        ShapeType currentShapeType = ShapeType.Line2D;
        string dashStyle = "";
        bool shiftMode;
        List<IShape> shapes = new List<IShape>();
        IShape preview;
        public MainWindow()
        {
            InitializeComponent();
            DllLoader.execute();
            var types = DllLoader.Types;
            KeyDown += new KeyEventHandler(OnButtonKeyDown);
            KeyUp += new KeyEventHandler(OnButtonKeyUp);
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            currentShapeType = ShapeType.Line2D;
        }

        private void Rectangle_Click(object sender, RoutedEventArgs e)
        {
            currentShapeType = ShapeType.Rectangle2D;
        }
        private void Ellipse_Click(object sender, RoutedEventArgs e)
        {
            currentShapeType = ShapeType.Ellipse2D;
        }
        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            preview = (IShape)GetInstance($"{currentShapeType}");
            isDrawing = true;
            Point currenCoord = e.GetPosition(DrawingCanvas);
            anchor.X = currenCoord.X;
            anchor.Y = currenCoord.Y;
            preview.HandleStart(anchor);
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {                                                                                               
            if (isDrawing)
            {
                DrawingCanvas.Children.Clear();
                Point coord = e.GetPosition(DrawingCanvas);

                Point2D point = new Point2D(coord.X, coord.Y);
                preview.HandleEnd(point);
                if (shiftMode)
                {
                    preview.HandleShiftMode();
                }
                preview.DashStyle = dashStyle;
                foreach (var shape in shapes)
                {
                    UIElement element = shape.Draw();
                    DrawingCanvas.Children.Add(element);
                }
                preview.Color = GlobalOptions.previewColor;
                DrawingCanvas.Children.Add(preview.Draw());
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            Point coord = e.GetPosition(DrawingCanvas);
            Point2D point = new Point2D(coord.X, coord.Y);
            preview.Color = GlobalOptions.strokeColor;
            preview.HandleEnd(point);
            if (shiftMode) {
                preview.HandleShiftMode();
            }
            preview.DashStyle = dashStyle;
            shapes.Add(preview);
            DrawingCanvas.Children.Clear();
            foreach (var shape in shapes)
            {
                var element = shape.Draw();
                DrawingCanvas.Children.Add(element);
            }
        }

        private void CreateSaveBitmap(Canvas canvas, string filename)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            canvas.Measure(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight));
            canvas.Arrange(new Rect(new Size((int)canvas.ActualWidth, (int)canvas.ActualHeight)));

            renderBitmap.Render(canvas);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            CreateSaveBitmap(DrawingCanvas, @"D:\out.png");
        }

        private void CreateLoadBitmap(Canvas canvas, string filename)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filename);
            bitmap.EndInit();

            Image image = new Image();
            image.Source = bitmap;
            canvas.Children.Add(image);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            CreateLoadBitmap(DrawingCanvas, @"D:\out.png");
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                shiftMode = true;
            }
        }
        private void OnButtonKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                shiftMode = false;
            }
        }

        public object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type);
            var types = DllLoader.Types;
            foreach (var t in types)
            {
                if (t.Name == strFullyQualifiedName)
                    return Activator.CreateInstance(t);
            }
            return null;
        }

        private void DashSwitch_Checked(object sender, RoutedEventArgs e)
        {
            dashStyle = GlobalOptions.dashStyle;
        }

        private void DashSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            dashStyle = "";
        }
    }
}
