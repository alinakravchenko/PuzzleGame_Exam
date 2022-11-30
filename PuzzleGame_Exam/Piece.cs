using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace PuzzleGame_Exam
{
    namespace PuzzleGame
    {
        class Piece : Grid
        {
            #region attributes
            Path path;
            string imageUri;
            double col, row;
            int index;
            #endregion

            #region constructor
            public Piece(ImageSource imageSource, double col, double row)
            {
                ImageUri = imageUri;
                Col = col;
                Row = row;

                path = new Path();

                path.Fill = new ImageBrush()
                {
                    ImageSource = imageSource,
                    Stretch = Stretch.Fill,
                    ViewportUnits = BrushMappingMode.Absolute,
                    Viewport = new Rect(0, 0, 100, 100),
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(
                        col * 100,
                        row * 100,
                        100,
                        100
                        )
                };
                path.Data = new RectangleGeometry(new Rect(0, 0, 100, 100));
                this.Children.Add(path);
            }
            #endregion

            #region methods

            #endregion

            #region properties
            public string ImageUri { get { return imageUri; } set { imageUri = value; } }
            public double Col { get { return col; } set { col = value; } }
            public double Row { get { return row; } set { row = value; } }
            public int Index { get { return index; } set { index = value; } }
            #endregion
        }
    }
}
