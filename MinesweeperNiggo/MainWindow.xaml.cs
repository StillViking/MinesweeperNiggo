using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace MinesweeperNiggo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int zeilen = 10;
        private int spalten = 10;
        private Minefield Minenfeld;
        
        //Im Konstrktor wird die Anzahl der Spalten und Zeilen gesetzt
        public MainWindow()
        {

            InitializeComponent();

            //es muss eine Zeile mehr erzeugt werden, für die Kopfzeile des Spiels (z. B Buttons (neues Spiel, oder Menü)
            //das Minefield stellt dann den Teil unterhalb der ersten Zeile des Grids dar
            for (int i = 0; i <= zeilen; i++) 
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(1, GridUnitType.Star);   //entspr. im XAML dem width="*"
                myGrid.RowDefinitions.Add(row);
            }

            for (int i = 0; i < spalten; i++)
            {
                myGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            Minenfeld = new Minefield(zeilen, spalten);

            //myGrid.ShowGridLines = true;



        }


        //In der Loaded-Methode werden die Elemente auf das Fenster Verteilt: Jedes Grid-Feld bekommt einen Button
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Neustart-button
            Button btn_restart = new Button();
            btn_restart.Background = Brushes.DarkGray;
            btn_restart.BorderBrush = Brushes.Black;
            btn_restart.BorderThickness = new Thickness(1);
            btn_restart.Click += newGame;
            btn_restart.Content = "Neues Spiel";
            btn_restart.Name = "NeuesSpiel";
            myGrid.Children.Add(btn_restart);
            Grid.SetColumn(btn_restart, 4);
            Grid.SetColumnSpan(btn_restart, 2);
            Grid.SetRow(btn_restart, 0);


            for (int i = 1; i <= zeilen;i++)
            {
                for (int j = 0; j < spalten; j++)
                {
                    Button btn = new Button();
                    btn.Background = Brushes.DarkGray;
                    btn.BorderBrush = Brushes.Black;
                    btn.BorderThickness = new Thickness(1);
                    btn.Click += OnFieldClick;
                    btn.MouseRightButtonUp += SetFlag;
                    //btn dem Grid zufügen
                    myGrid.Children.Add(btn);
                    Grid.SetRow(btn, i);
                    Grid.SetColumn(btn, j);
                }
            }
        }

        private void newGame(object sender, RoutedEventArgs e)
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }



        //Handler des Klick-Eventes mit rechter Maustaste, setzt die Flagge
        private void SetFlag(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            int i = Grid.GetRow(btn);
            int j = Grid.GetColumn(btn);

            Minenfeld.setFlag(i - 1, j);

            Image img = new Image();
            img.Source = new BitmapImage(new Uri("E:\\Projekte\\Niko Programmier Projekte\\Eigene Projekte\\MinesweeperNiggo\\MinesweeperNiggo\\flag.png"));

            if (Minenfeld.field[i-1, j].flagged)
            {
                btn.Content = img;
            }
            else
            {
                btn.Content = "";
            }

        }



        //Handler des einfachen Klick-Events mit linker Maustaste
        private void OnFieldClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            //das wpf-Feld hat eine zeile mehr und erst ab Index 1 beginnt das Minenfeld, daher muss für den Wert i 1 subtrahiert werden
            //denn das field beginnt bei index 0
            int i = Grid.GetRow(btn); 
            int j = Grid.GetColumn(btn);

            Minenfeld.ClickTile(i-1, j);

            this.refresh();
            if (Minenfeld.hitMine) //Feld markieren, auf dem man die Mine getroffen hat
            {
                var hitMineField = myGrid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == j) as Label;
                hitMineField.Background = Brushes.Red;
            }
        }


        //durchlaufe jeden Button bzw dessen Position. übertrage diese auf die i x j Matrix im field, für alle Felder die "clicked" sind, wírd der Button deaktiviert und 
        //der Content entsprechend (Mine = * oder die Zahl "numberOfAdjacentMines) gesetzt
        private void refresh()
        {
            if (Minenfeld.hitMine || Minenfeld.win) //sobald gewonnen(win) oder verloren(hinMine) werden alle Buttons deaktiviert
            {
                foreach(var item in myGrid.Children)
                {
                    if (item is Button)
                    {
                        Button b = (Button)item;
                        if (b.Name == "NeuesSpiel") 
                        { 
                            continue; 
                        }
                        b.IsEnabled = false;
                    }
                }

                
            }



            //Setze für jeden Button, dessen zugehöriges Element in field "clicked" ist seinen Content entsprechend.
            //Entferne für jeden Button
            for (int i = 1; i <= zeilen; i++)
            {
                for (int j = 0; j < spalten; j++)
                {
                    changeButtonToLabel(i, j);
                }
            }
        }





        private void changeButtonToLabel(int i, int j)
        {
            if (Minenfeld.field[i - 1, j].clicked)
            {
                Button btn = myGrid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == j) as Button;
                Label l = new Label();
                l.BorderBrush = Brushes.Black;
                l.BorderThickness = new Thickness(1);

                if (Minenfeld.field[i - 1, j].mine)
                {
                    l.Content = "*";
                    l.FontSize = 30;
                    l.FontWeight = FontWeights.Bold;
                    l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    l.VerticalContentAlignment = VerticalAlignment.Center;
                }
                else
                {
                    if (Minenfeld.field[i - 1, j].numberOfAdjacentMines == 0)
                    {
                        l.Content = "";
                    }
                    else
                    {
                        l.Content = Minenfeld.field[i - 1, j].numberOfAdjacentMines;
                        l.FontSize = 20;
                        l.HorizontalContentAlignment = HorizontalAlignment.Center;
                    }
                }

                myGrid.Children.Remove(btn);
                myGrid.Children.Add(l);
                Grid.SetRow(l, i);
                Grid.SetColumn(l, j);
            }
        }


    }//End Main-Window
} //End Namespace
