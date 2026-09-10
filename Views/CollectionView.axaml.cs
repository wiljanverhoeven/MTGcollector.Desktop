using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using MTGcollector_app.Models;
using MTGcollector_app.ViewModels;

namespace MTGcollector_app.Views
{
    public partial class CollectionView : UserControl
    {
        public CollectionView()
        {
            InitializeComponent();
        }

        public CollectionView(CollectionViewModel? viewModel = null)
        {
            InitializeComponent();
            if (viewModel != null)
            {
                DataContext = viewModel;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnAddToCollectionClicked(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnAddToCollectionClicked fired");

            if (sender is not Button btn)
            {
                System.Diagnostics.Debug.WriteLine("Sender is not a Button");
                return;
            }

            if (btn.Tag is not ScryfallCard card)
            {
                System.Diagnostics.Debug.WriteLine($"Tag is not a ScryfallCard, it is: {btn.Tag?.GetType().Name}");
                return;
            }

            if (this.DataContext is not CollectionViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine("DataContext is not CollectionViewModel");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Adding card: {card.Name}");
            vm.AddScryfallCardCommand?.Execute(card);
        }

        private void OnEditClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;

            if (btn.Tag is not Card card)
                return;

            if (this.DataContext is not CollectionViewModel vm)
                return;

            vm.StartEditCommand?.Execute(card);
        }

        private void OnRemoveClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;

            if (btn.Tag is not Card card)
                return;

            if (this.DataContext is not CollectionViewModel vm)
                return;

            vm.DeleteCardCommand?.Execute(card);
        }
    }
}
