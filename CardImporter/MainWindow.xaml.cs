using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CardImporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<CardBackInfo> _cards;
        public MainWindow()
        {
            InitializeComponent();

            using (var db = new PexesoDbContext()) {
                _cards = new ObservableCollection<CardBackInfo>();
                var cards = db.Cards;
                foreach (var card in cards) {
                    _cards.Add(card);
                }

                lbCards.ItemsSource = _cards;
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CardWindow();
            if (dialog.ShowDialog() == true) {
                var card = dialog.Card;
                _cards.Add(card);

                using (var db = new PexesoDbContext()) {
                    await db.Cards.AddAsync(card);
                    await db.SaveChangesAsync();
                }
            }
            
        }

        private void lbCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            canvas.Children.Clear();

            var card = (CardBackInfo)lbCards.SelectedItem;
            var rc = new Rectangle();

            rc.Width = 150;
            rc.Height = 250;
            rc.RadiusX = 5;
            rc.RadiusY = 5;
            Canvas.SetLeft(rc, canvas.ActualWidth / 2 - 75);
            Canvas.SetTop(rc, canvas.ActualHeight / 2 - 125);

            if (card.Type == POT.Pexeso.Shared.CardType.Color) {
                rc.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(card.Source));
            } else {
                rc.Fill = new ImageBrush((BitmapSource)new ImageSourceConverter()
                    .ConvertFrom(Convert.FromBase64String(card.Source.Split(",", StringSplitOptions.RemoveEmptyEntries)[1])));
                rc.Stroke = Brushes.Black;
                //var bmp = (BitmapSource)new ImageSourceConverter().ConvertFrom(Convert.FromBase64String(card.Source));
                //rc.Fill = new ImageBrush(bmp);
            }

            canvas.Children.Add(rc);
        }
    }
}
