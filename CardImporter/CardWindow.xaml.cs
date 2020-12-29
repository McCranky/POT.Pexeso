using Microsoft.Win32;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CardImporter
{
    /// <summary>
    /// Interaction logic for CardWindow.xaml
    /// </summary>
    public partial class CardWindow : Window
    {
        public CardBackInfo Card { get; set; } = new CardBackInfo();
        public CardWindow()
        {
            InitializeComponent();
            DataContext = Card;

            var listOfTypes = new List<CardType>();
            listOfTypes.Add(CardType.Color);
            listOfTypes.Add(CardType.Picture);
            cbType.ItemsSource = listOfTypes;
            cbType.SelectedIndex = 0;
        }

        private void cbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Card.Type = (CardType)cbType.SelectedItem;
            Card.Source = "";
            switch (Card.Type) {
                case CardType.Color:
                    spImageInput.Visibility = Visibility.Hidden;
                    tbSource.Visibility = Visibility.Visible;
                    tbSource.Text = "";
                    break;
                case CardType.Picture:
                    spImageInput.Visibility = Visibility.Visible;
                    tbSource.Visibility = Visibility.Hidden;
                    txtImage.Text = "";
                    break;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Card.Source) &&
                !string.IsNullOrWhiteSpace(Card.Name)) {
                DialogResult = true;
            }
        }

        private void btnSource_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.jpg,*.jpeg,*.png)|*.jpg;*.jpeg;*.png";
            dialog.Title = "Please select an image file.";

            if (dialog.ShowDialog() == true) {
                //var img = System.Drawing.Image.FromFile(dialog.FileName);
                //var resizedImg = (System.Drawing.Image)(new Bitmap(img, new System.Drawing.Size { Height = 275, Width = 220 }));
                //using (var ms = new MemoryStream()) {
                //    resizedImg.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //    Card.Source = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
                //}

                txtImage.Text = dialog.SafeFileName;
                var buffer = File.ReadAllBytes(dialog.FileName);
                Card.Source = $"data:image/png;base64,{Convert.ToBase64String(buffer)}";
            }
        }
    }
}
