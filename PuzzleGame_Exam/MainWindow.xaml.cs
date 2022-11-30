using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using PuzzleGame_Exam.PuzzleGame;
using static System.Net.WebRequestMethods;
namespace PuzzleGame_Exam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region attribute
        Image image = new Image();
        BitmapImage imageSource;
        List<Piece> pieces = new List<Piece>();
        List<Piece> currentSelection = new List<Piece>();
        int columns = 1;
        int rows = 1;
        int pieceIndex = 0;
        #endregion

        #region constructor
        public MainWindow()
        {
            InitializeComponent();

            Pole.MouseEnter += new MouseEventHandler(pole_MouseEnter);
            Pole.MouseMove += new MouseEventHandler(pole_MouseMove);
            Pole.MouseLeftButtonUp += new MouseButtonEventHandler(pole_MouseLeftButtonUp);
            Pole.MouseLeave += new MouseEventHandler(pole_MouseLeave);
        }
        #endregion constructor

        #region methods
        public void LoadImage(string uriImage)
        {
            BitmapImage bi = new BitmapImage(new Uri(uriImage));

            columns = (int)Math.Ceiling(bi.PixelWidth / 100.0);
            rows = (int)Math.Ceiling(bi.PixelHeight / 100.0); //Ceiling(Double). Возвращает наименьшее целое число, которое больше или равно заданному числу с плавающей запятой двойной точности.

            RenderTargetBitmap rtb = new RenderTargetBitmap(columns * 100, rows * 100, bi.DpiX, bi.DpiY, PixelFormats.Pbgra32);

            var imgBrush = new ImageBrush(bi)
            {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Stretch = Stretch.Fill
            };

            var rectImage = new Rectangle
            {
                Width = columns * 100,
                Height = rows * 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Fill = imgBrush
            };
            rectImage.Arrange(new Rect(0, 0, columns * 100, rows * 100));

            rtb.Render(rectImage);

            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));

            System.IO.Stream ret = new System.IO.MemoryStream();

            png.Save(ret);

            imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = ret;
            imageSource.EndInit();
            imageSource.Freeze();

        }

        public void CreatePole()
        {
            Pole.IsEnabled = true;
            Pole.Children.Clear();
            Pole.Width = columns * 100;
            Pole.Height = rows * 100;
            Pole.Background = new SolidColorBrush(Colors.WhiteSmoke);
            Pole.Margin = new Thickness(50);
            Pole.Parent.SetValue(Grid.BackgroundProperty, new SolidColorBrush(Colors.WhiteSmoke));
        }

        public void CreatePieces()
        {
            Podbor.Children.Clear();
            pieces.Clear();
            pieceIndex = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    Piece piece = new Piece(imageSource, x, y)
                    {
                        Margin = new Thickness(5)
                    };
                    piece.Index = pieceIndex++;

                    piece.MouseLeftButtonUp += new MouseButtonEventHandler(piece_MouseLeftButtonUp);
                    pieces.Add(piece);
                }
            }

            RandomPiece(Podbor);
        }

        private void RandomPiece(WrapPanel podbor)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < pieces.Count; i++)
            {

                int index = rnd.Next(0, pieces.Count);
                Piece tmp = pieces[i];
                pieces[i] = pieces[index];
                pieces[index] = tmp;
            }
            foreach (var p in pieces)
            {
                podbor.Children.Add(p);
            }
        }

        private bool CanInsertPiece(int cellX, int cellY)
        {
            bool ret = true;
            foreach (Piece piece in Pole.Children)
            {
                foreach (Piece currentPiece in currentSelection)
                {
                    if (currentPiece != piece)
                    {
                        if (piece.Row == cellY && piece.Col == cellX) ret = false;
                    }
                }
            }

            return ret;
        }

        private bool IsCompleted()
        {
            bool ret = true;
            int index = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                ret = false;
                foreach (Piece p in Pole.Children)
                {
                    if (index == (p.Row * rows + p.Col) && index == p.Index)
                    {
                        ret = true;
                        break;
                    }
                }
                if (ret == false) break;
                index++;
            }
            return ret;
        }
        #endregion methods

        #region events
        private void piece_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var chosenPiece = (Piece)sender;

            if (chosenPiece.Parent is WrapPanel)
            {
                if (currentSelection.Count() > 0)
                {
                    var p = currentSelection[0];
                    Pole.Children.Remove(p);
                    p.Visibility = Visibility.Visible;
                    Podbor.Children.Add(p);
                    currentSelection.Clear();
                }
                else
                {
                    Podbor.Children.Remove(chosenPiece);
                    Pole.Children.Add(chosenPiece);
                    chosenPiece.Visibility = Visibility.Hidden;
                    currentSelection.Add(chosenPiece);
                }
            }
        }

        private void pole_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void pole_MouseEnter(object sender, MouseEventArgs e)
        {
            if (currentSelection.Count > 0)
            {
                foreach (var currentPiece in currentSelection)
                {
                    currentPiece.Margin = new Thickness(0);
                    currentPiece.Visibility = Visibility.Visible;
                }
            }
        }

        private void pole_MouseMove(object sender, MouseEventArgs e)
        {
            var newX = Mouse.GetPosition((IInputElement)Pole).X;
            var newY = Mouse.GetPosition((IInputElement)Pole).Y;

            if (currentSelection.Count > 0)
            {
                var firstPiece = currentSelection[0];
                foreach (var currentPiece in currentSelection)
                {
                    double CellX = currentPiece.Row - firstPiece.Row;
                    double CellY = currentPiece.Col - firstPiece.Col;
                    currentPiece.SetValue(Canvas.ZIndexProperty, 2);
                    currentPiece.SetValue(Canvas.LeftProperty, newX - 50 + CellX * 100); // разместить дочерние элементы с помощью координат, относящихся к области Canvas.
                    currentPiece.SetValue(Canvas.TopProperty, newY - 50 + CellY * 100);
                }
            }
        }

        private void pole_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var newX = Mouse.GetPosition(Pole).X;
            var newY = Mouse.GetPosition(Pole).Y;

            double cellX = (int)((newX) / 100);
            double cellY = (int)((newY) / 100);

            if (currentSelection.Count > 0)
            {
                if (CanInsertPiece((int)cellX, (int)cellY))
                {
                    var firstPiece = currentSelection[0];

                    var relativeCellX = currentSelection[0].Col - firstPiece.Col;
                    var relativeCellY = currentSelection[0].Row - firstPiece.Row;

                    double rotatedCellX = relativeCellX;
                    double rotatedCellY = relativeCellY;

                    foreach (Piece currentPiece in currentSelection)
                    {
                        currentPiece.Col = cellX + rotatedCellX;
                        currentPiece.Row = cellY + rotatedCellY;
                        currentPiece.SetValue(Canvas.LeftProperty, currentPiece.Col * 100); // разместить дочерние элементы с помощью координат, относящихся к области Canvas.
                        currentPiece.SetValue(Canvas.TopProperty, currentPiece.Row * 100);
                        currentPiece.SetValue(Canvas.ZIndexProperty, 1);
                    }
                    currentSelection.Clear();
                }
                if (Podbor.Children.Count == 0)
                    if (IsCompleted() == true)
                    {
                        MessageBox.Show("Puzzle is completed");
                        Pole.IsEnabled = false;
                    }
            }
            else
            {
                foreach (Piece p in Pole.Children)
                {
                    if ((p.Col == cellX) && (p.Row == cellY))
                    {
                        p.Visibility = Visibility.Visible;
                        currentSelection.Add(p);
                    }
                }
            }
        }

        private void pole_MouseLeave(object sender, MouseEventArgs e)
        {
            if (currentSelection.Count > 0)
            {
                foreach (var p in currentSelection)
                {
                    Pole.Children.Remove(p);
                    p.Margin = new Thickness(5);
                    Podbor.Children.Add(p);
                }
                currentSelection.Clear();
            }
        }

        private void btnCheckImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                //https://www.c-sharpcorner.com/uploadfile/raj1979/how-to-get-system-environment-information-in-wpf/
                //любое расширение картинки
                Filter = "All Image Files ( JPEG,GIF,BMP,PNG)|*.jpg;*.jpeg;*.gif;*.bmp;*.png|JPEG Files ( *.jpg;*.jpeg )|*.jpg;*.jpeg|GIF Files ( *.gif )|*.gif|BMP Files ( *.bmp )|*.bmp|PNG Files ( *.png )|*.png",
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Title = "Check puzzle image"
            };

            if (openDialog.ShowDialog() == true)
            {
                LoadImage(openDialog.FileName);
                btnShowImage.IsEnabled = true;
                CreatePieces();
                CreatePole();
            }
        }

        private void btnShowImage_Click(object sender, RoutedEventArgs e)
        {
            //https://metanit.com/sharp/wpf/5.1.php
            if (PuzzleGrid.Visibility == Visibility.Visible)
            {
                PuzzleGrid.Visibility = Visibility.Hidden;
                PuzzleImg.Source = imageSource;
                PuzzleImg.Visibility = Visibility.Visible;
                PuzzleImg.Stretch = Stretch.Uniform;
            }
            else
            {
                PuzzleImg.Visibility = Visibility.Collapsed;
                PuzzleGrid.Visibility = Visibility.Visible;
            }

        }
        #endregion events
    }

}

