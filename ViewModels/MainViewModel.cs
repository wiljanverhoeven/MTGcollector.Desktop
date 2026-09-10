using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace MTGcollector_app.ViewModels;

public partial class MainWindowViewModel
    : ViewModelBase
{
    [ObservableProperty]
    private object? currentViewModel;

    private readonly CollectionViewModel collectionViewModel;

    public MainWindowViewModel(
        CollectionViewModel collectionViewModel)
    {
        this.collectionViewModel =
            collectionViewModel;

        CurrentViewModel =
            collectionViewModel;
    }

    public async Task InitializeAsync()
    {
        await collectionViewModel
            .InitializeAsync();
    }
}