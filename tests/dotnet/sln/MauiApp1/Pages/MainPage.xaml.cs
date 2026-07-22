using MauiApp1.Models;
using MauiApp1.PageModels;

namespace MauiApp1.Pages;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}