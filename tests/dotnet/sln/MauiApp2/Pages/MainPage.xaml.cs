using MauiApp2.Models;
using MauiApp2.PageModels;

namespace MauiApp2.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}