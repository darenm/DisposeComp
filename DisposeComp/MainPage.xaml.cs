using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;
using Robmikh.CompositionSurfaceFactory;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DisposeComp
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly Compositor _compositor;
        private readonly SurfaceFactory _surfaceFactory;
        private Dictionary<int, TextSurface> _textSurfaces = new Dictionary<int, TextSurface>();

        public MainPage()
        {
            InitializeComponent();
            ImageList.ItemsSource = BuildPhotos();
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _surfaceFactory = SurfaceFactory.GetSharedSurfaceFactoryForCompositor(_compositor);
        }

        private List<Photo> BuildPhotos()
        {
            var random = new Random();
            var photos = new List<Photo>();
            for (var i = 0; i < 1000; i++)
                photos.Add(new Photo {Id = i, ImageUri = $"ms-appx:///Assets/{random.Next(13)}.jpg"});
            return photos;
        }

        private void ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image) sender;
            ElementCompositionPreview.SetElementChildVisual(image, null);

            var photo = image.Tag as Photo;

            var container = _compositor.CreateContainerVisual();
            var backgroundSprite = _compositor.CreateSpriteVisual();
            var textSprite = _compositor.CreateSpriteVisual();

            backgroundSprite.Brush = _compositor.CreateColorBrush(Color.FromArgb(128, 64, 64, 64));
            backgroundSprite.Size = new Vector2(200, 200);

            if (!_textSurfaces.ContainsKey(photo.Id))
            {
                _textSurfaces.Add(photo.Id, _surfaceFactory.CreateTextSurface(photo.Title,            // Text
                                                      200.0f,                       // Desired Width
                                                      200.0f,                         // Desired Height
                                                      "Times New Roman",            // Font Family
                                                      14.0f,                        // Font Size
                                                      FontStyle.Normal,             // Font Style
                                                      TextHorizontalAlignment.Left, // Horizontal Alignment
                                                      TextVerticalAlignment.Center,    // Vertical Alignment
                                                      WordWrapping.WholeWord,       // Wrapping
                                                      new Padding(),                // Padding
                                                      Colors.White,               // Foreground Color
                                                      Colors.Transparent));          // Background Color
            }

            textSprite.Brush = _compositor.CreateSurfaceBrush(_textSurfaces[photo.Id].Surface);
            textSprite.Size = new Vector2(200, 200);

            container.Children.InsertAtBottom(backgroundSprite);
            container.Children.InsertAbove(textSprite, backgroundSprite);

            ElementCompositionPreview.SetElementChildVisual(image, container);
        }

        private void ImageUnloaded(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            var container = ElementCompositionPreview.GetElementChildVisual(image) as ContainerVisual;
            if (container != null)
            {
                foreach (var containerChild in container.Children)
                {
                    containerChild.Dispose();
                }
                container.Dispose();
            }
            var photo = image.Tag as Photo;
            if (_textSurfaces.ContainsKey(photo.Id))
            {
                var textSurface = _textSurfaces[photo.Id];
                _textSurfaces.Remove(photo.Id);
                textSurface.Dispose();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void ImageList_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(Detail), e.ClickedItem);
        }
    }
}