using cuatomers.DAL;
using cuatomers.Pages;
using napeans.dal;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfAnimatedGif;


namespace cuatomers
{
    public partial class MainWindow : Window
    {

        public Customers EditedCustomers;


        public MainWindow()
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            InitializeComponent();
            mainFrame.Navigate(new home());

        }
        private void StopAllGifAnimations()
        {
            ImageBehavior.GetAnimationController(imgCustomer)?.Pause();
            ImageBehavior.GetAnimationController(imgProject)?.Pause();
            ImageBehavior.GetAnimationController(imgQuotation)?.Pause();
            ImageBehavior.GetAnimationController(imgInvoice)?.Pause();
            ImageBehavior.GetAnimationController(imgSettings)?.Pause();
        }

        public Frame GetMainFrame()
        {
            return mainFrame;
        }

        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private void btnHome_click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new home());

        }


        private void btnCustomer_click(object sender, RoutedEventArgs e)
        {
            StopAllGifAnimations();
            ImageBehavior.GetAnimationController(imgCustomer)?.Play();
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new CustomerMaster(adoHelper));
            }
            else
            {
                MessageBox.Show("MainWindow not found. Are you running in the Designer?");
            }
        }



        // Backend method for page navigation
        private void btnproject_click(object sender, RoutedEventArgs e)
        {
            StopAllGifAnimations();
            ImageBehavior.GetAnimationController(imgProject)?.Play();
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new ProjectMaster(adoHelper));

            }
            else
            {
                MessageBox.Show("ProjectMaster not found. Are you running in the Designer?");
            }

        }


        private void btnExpense_click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new ExpensePage(adoHelper));

            }
            else
            {
                MessageBox.Show("ProjectMaster not found. Are you running in the Designer?");
            }

        }
       
        private void btnQuotation_click(object sender, RoutedEventArgs e)
        {
            StopAllGifAnimations();
            ImageBehavior.GetAnimationController(imgQuotation)?.Play();
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new QuoteGenerate(adoHelper));

            }
            else
            {
                MessageBox.Show("ProjectMaster not found. Are you running in the Designer?");
            }

        }

        



        private void btnSettings_click(object sender, RoutedEventArgs e)
        {

            StopAllGifAnimations();
            ImageBehavior.GetAnimationController(imgSettings)?.Play();
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new Settings(adoHelper));
            }
            else
            {
                MessageBox.Show("InvoiceGenerator not found. Are you running in the Designer?");
            }

        }


        private void btnInvoice_Click(object sender, RoutedEventArgs e)
        {
            StopAllGifAnimations();
            ImageBehavior.GetAnimationController(imgInvoice)?.Play();
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new InvoiceGenerate(adoHelper));
            }
            else
            {
                MessageBox.Show("InvoiceGenerator not found. Are you running in the Designer?");
            }

        }

        private void btnDC_Click(object sender, RoutedEventArgs e)
        {
            StopAllGifAnimations();
            ImageBehavior.GetAnimationController(imgInvoice)?.Play();
            Window window = Window.GetWindow(this);
            if (window is MainWindow mainWindow)
            {
                IAdoHelper adoHelper = new AdoHelper();
                mainWindow.GetMainFrame().Navigate(new DeliveryChallan(adoHelper));
            }
            else
            {
                MessageBox.Show("InvoiceGenerator not found. Are you running in the Designer?");
            }
        }
    }
}