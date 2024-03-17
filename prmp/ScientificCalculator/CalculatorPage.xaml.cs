using MauiScientificCalculator.ViewModels;

namespace ScientificCalculator;

public partial class CalculatorPage : ContentPage
{
    public CalculatorPage()
    {
        InitializeComponent();

        //Initialize the view model
        this.BindingContext = new CalculatorPageViewModel();
    }
}