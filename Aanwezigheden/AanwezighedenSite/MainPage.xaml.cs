using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AanwezighedenSite.PresenceService;
using System.Collections.ObjectModel;
using System.Windows.Printing;


namespace AanwezighedenSite
{

    public partial class MainPage : UserControl
    {
        string sActiviteitType;
        Int16 counterAanwezig = 0;
        Int32 _wedstrijdID = 0;
       
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            aanwezighedenDataGrid.Columns[3].Visibility = Visibility.Collapsed;
            aanwezighedenDataGrid.Columns[4].Visibility = Visibility.Collapsed;
            var client = new PresenceServiceClient();
            client.getPloegenCompleted += new EventHandler<getPloegenCompletedEventArgs>(client_getPloegenCompleted);
            client.getPloegenAsync();
        }

        void client_getPloegenCompleted(object sender, getPloegenCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Ploeg xPloeg = new Ploeg();
                xPloeg.Name = "Kies een ploeg";
                xPloeg.PloegId = 0;
                e.Result.Insert(0, xPloeg);
                ploegComboBox.ItemsSource = e.Result;
                ploegComboBox.SelectedIndex = 0;
                bewarenBtn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show(e.Error.ToString());
            }
        }

        private void ploegComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var client = new PresenceServiceClient();
          
            client.getNextDataEntryCompleted += new EventHandler<getNextDataEntryCompletedEventArgs>(client_getNextDataEntryCompleted);
            if (((Ploeg)ploegComboBox.SelectedValue).PloegId != 0)
            {
                client.getNextDataEntryAsync(System.Convert.ToInt32((ploegComboBox.SelectedItem as Ploeg).PloegId));
                bewarenBtn.IsEnabled = true;
            }

        }

        void client_getWedstrijdCompleted(object sender, getWedstrijdCompletedEventArgs e)
        {
            if (e.Result.Count != 0)
            {
                tegenstanderTextBox.Text = ((Wedstrijd)e.Result[0]).Tegenstander;
                tegenstanderTextBox.Text = tegenstanderTextBox.Text + string.Format(" ({0}) ",((Wedstrijd)e.Result[0]).Thuis?"Thuis":"Uit");
                uitslagTextBox.IsEnabled = true;
                uitslagTextBox.Text = ((Wedstrijd)e.Result[0]).Uitslag;
                _wedstrijdID = ((Wedstrijd)e.Result[0]).WedstrijdId;
                ShowPercentage(true);
            }
            else
            {
                tegenstanderTextBox.Text = "Training";
                uitslagTextBox.Text = "Uitslag";
                uitslagTextBox.IsEnabled = false;
                ShowPercentage(false);
            }
        }

        void client_getAanwezighedenByPloegAndDatumCompleted(object sender, getAanwezighedenByPloegAndDatumCompletedEventArgs e)
        {
            aanwezighedenDataGrid.DataContext = e.Result;
            aanwezighedenDataGrid.ItemsSource = e.Result;
            AanwezigheidSpeler _aanwezigheidspeler = ((List<AanwezigheidSpeler>)aanwezighedenDataGrid.ItemsSource)[0];
            if (_aanwezigheidspeler.AanwezigheidId != 0)
                deleteBtn.IsEnabled = true;
            else
                deleteBtn.IsEnabled = false;
        }

        private void bewarenBtn_Click(object sender, RoutedEventArgs e)
        {
            //valideer de data
            Boolean isError = false;
            vsError.Errors.Clear();

            foreach (AanwezigheidSpeler _aanwezigheid in (List<AanwezigheidSpeler>)aanwezighedenDataGrid.ItemsSource)
            {
                if (string.IsNullOrEmpty(_aanwezigheid.Status))
                {
                    vsError.Errors.Add(new ValidationSummaryItem("Gelieve voor elke speler een status op te geven."));
                    isError = true;
                    break;
                }
            }
            if (uitslagTextBox.Text != "Uitslag")
            {
                if (!((uitslagTextBox.Text.IndexOf(" ") < 0 &&
                     uitslagTextBox.Text.IndexOf("-") > 0)
                      ||
                   (uitslagTextBox.Text == "afgelast" ||
                    uitslagTextBox.Text == "ff" ||
                    uitslagTextBox.Text == "1e" ||
                    uitslagTextBox.Text == "2e" ||
                    uitslagTextBox.Text == "3e" ||
                    uitslagTextBox.Text == "4e" ||
                    uitslagTextBox.Text == "5e" ||
                    uitslagTextBox.Text == "6e" ||
                    uitslagTextBox.Text == "7e" ||
                    uitslagTextBox.Text == "8e" ||
                    uitslagTextBox.Text == "9e" ||
                    uitslagTextBox.Text == "10e" ||
                    uitslagTextBox.Text == "11e" ||
                    uitslagTextBox.Text == "12e" ||
                    uitslagTextBox.Text == "13e" ||
                    uitslagTextBox.Text == "14e" ||
                    uitslagTextBox.Text == "15e" ||
                    uitslagTextBox.Text == "16e")
                   ))
                {
                    vsError.Errors.Add(new ValidationSummaryItem("Controleer de formatting. vb 1-2, ff, afgelast, 1e, 2e, 3e, 4e , ..."));
                    isError = true;
                }
            }
            if (isError) return;

            var client = new PresenceServiceClient();
            //het bewaren van de uitslag
            if (uitslagTextBox.Text != "Uitslag")
                client.setUitslagAsync(_wedstrijdID, uitslagTextBox.Text);

            //het eigenlijke bewaren
            client.getNextDataEntryWDatumCompleted += new EventHandler<getNextDataEntryWDatumCompletedEventArgs>(client_getNextDataEntryWDatumCompleted);
            client.setAanwezigheidCompleted += new EventHandler<setAanwezigheidCompletedEventArgs>(client_setAanwezigheidCompleted);
            
            if (((Ploeg)ploegComboBox.SelectedValue).PloegId != 0)
            {
                //zoeken of het over een create of modifiy activity gaat
                string activity = string.Empty;
                List<AanwezigheidSpeler> _aanwezigheidspelers = (List<AanwezigheidSpeler>)aanwezighedenDataGrid.ItemsSource;
                if (_aanwezigheidspelers[0].RowStatus == "N")
                    activity = "CREATE";
                else
                    activity = "MODIFY";
                client.setAanwezigheidAsync(((Ploeg)ploegComboBox.SelectedValue).PloegId, sActiviteitType, datumDatePicker.SelectedDate.Value, "memotest nog niet op schem",activity);
                bewarenBtn.IsEnabled = false;
            }
        }

        void client_setAanwezigheidCompleted(object sender, setAanwezigheidCompletedEventArgs e)
         {
             var client = new PresenceServiceClient();
             List<AanwezigheidSpeler> _aanwezigheidspelers = (List<AanwezigheidSpeler>)aanwezighedenDataGrid.ItemsSource;
             client.setAanwezigheidSpelersAsync((int)e.Result, _aanwezigheidspelers);
             client.getNextDataEntryWDatumCompleted +=new EventHandler<getNextDataEntryWDatumCompletedEventArgs>(client_getNextDataEntryWDatumCompleted);
             client.getNextDataEntryWDatumAsync(System.Convert.ToInt32((ploegComboBox.SelectedItem as Ploeg).PloegId), datumDatePicker.SelectedDate.Value);
         }

        void ShowPercentage(Boolean show)
        {
            if (((Ploeg)ploegComboBox.SelectedValue).DisplaySequence > 130)  //deze regel is maar geldig tot U10
            {
                if (show)
                {
                    aanwezighedenDataGrid.Columns[3].Visibility = Visibility.Visible;
                    aanwezighedenDataGrid.Columns[4].Visibility = Visibility.Visible;
                    vsError.Width = 589;
                    aanwezighedenDataGrid.Width = 591;
                    bewarenBtn.Margin = new Thickness(503, 528, 0, 0);
                    deleteBtn.Margin = new Thickness(465, 528, 0, 0);
                }
                else
                {
                    aanwezighedenDataGrid.Columns[3].Visibility = Visibility.Collapsed;
                    aanwezighedenDataGrid.Columns[4].Visibility = Visibility.Collapsed;
                    vsError.Width = 379;
                    aanwezighedenDataGrid.Width = 381;
                    bewarenBtn.Margin = new Thickness(295, 528, 0, 0);
                    deleteBtn.Margin = new Thickness(255, 528, 0, 0);
                }
            }
        }

        void client_getNextDataEntryCompleted(object sender, getNextDataEntryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                foreach (Aanwezigheid x in e.Result)
                {
                    sActiviteitType = x.Type.ToUpper();
                    if (x.Type.ToUpper() == "W")
                    {
                        tegenstanderTextBox.Text = x.Tegenstander;
                        tegenstanderTextBox.Text = tegenstanderTextBox.Text + string.Format(" ({0}) ", "test");
                        uitslagTextBox.IsEnabled = true;
                        ShowPercentage(true);
                    }
                    else
                    {
                        tegenstanderTextBox.Text = "Training";
                        uitslagTextBox.Text = "Uitslag";
                        uitslagTextBox.IsEnabled = false;
                        ShowPercentage(false);
                    }
                    datumDatePicker.SelectedDate = x.Datum;
                }
            }
        }

        void client_getNextDataEntryWDatumCompleted(object sender, getNextDataEntryWDatumCompletedEventArgs e)
         {
             if (e.Error == null)
             {
                 foreach (Aanwezigheid x in e.Result)
                 {
                     sActiviteitType = x.Type.ToUpper();
                     if (x.Type.ToUpper() == "W")
                     {
                         //datumDatePicker.Background = new SolidColorBrush(Color.FromArgb(255, 38, 186, 8)); //61, 104, 230, 27
                         //ToolTipService.SetToolTip(datumDatePicker, x.Tegenstander);
                         tegenstanderTextBox.Text = x.Tegenstander;
                         uitslagTextBox.IsEnabled = true;
                         ShowPercentage(true);
                     }
                     else
                     {
                         //datumDatePicker.Background = new SolidColorBrush(Colors.White); 
                         //ToolTipService.SetToolTip(datumDatePicker, "Training");
                         tegenstanderTextBox.Text = "Training";
                         uitslagTextBox.Text = "Uitslag";
                         uitslagTextBox.IsEnabled = false;
                         ShowPercentage(false);
                     }
                     datumDatePicker.SelectedDate = x.Datum;

                 }
             }
         }

        private void aanwezighedenDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            vsError.Errors.Clear();
             FrameworkElement fe = (FrameworkElement)e.OriginalSource;
             DataGridColumn column = (DataGridColumn)DataGridColumn.GetColumnContainingElement(fe);
             if (column.Header.ToString() == "Status")
             {
                 foreach (AanwezigheidSpeler _aanwezigheidspeler in aanwezighedenDataGrid.ItemsSource)
                 {
                     _aanwezigheidspeler.Status = "Aanwezig";
                     counterAanwezig++;
                 }
             }
         }

        private void datumDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
         {
             vsError.Errors.Clear();
             if (datumDatePicker.SelectedDate > DateTime.Today)
             {
                 vsError.Errors.Add(new ValidationSummaryItem("De datum voor ingave kan niet in de toekomst liggen"));
                 return;
             }
             var client = new PresenceServiceClient();
             client.getAanwezighedenByPloegAndDatumCompleted += new EventHandler<getAanwezighedenByPloegAndDatumCompletedEventArgs>(client_getAanwezighedenByPloegAndDatumCompleted);
             client.getWedstrijdCompleted += new EventHandler<getWedstrijdCompletedEventArgs>(client_getWedstrijdCompleted);
             if (((Ploeg)ploegComboBox.SelectedValue).PloegId != 0)
             {
                 client.getAanwezighedenByPloegAndDatumAsync(System.Convert.ToInt32((ploegComboBox.SelectedItem as Ploeg).PloegId), datumDatePicker.SelectedDate.Value);
                 client.getWedstrijdAsync(System.Convert.ToInt32((ploegComboBox.SelectedItem as Ploeg).PloegId), datumDatePicker.SelectedDate.Value, false, true, false);
                 bewarenBtn.IsEnabled = true;
             }
         }

        private void grafBtn_Click(object sender, RoutedEventArgs e)
         {
             Overview _overview = new Overview(System.Convert.ToInt32((ploegComboBox.SelectedItem as Ploeg).PloegId));
             _overview.Show();
         }

        private void gridBtn_Click(object sender, RoutedEventArgs e)
        {
            GridOverview _gridOverview = new GridOverview(System.Convert.ToInt32((ploegComboBox.SelectedItem as Ploeg).PloegId),datumDatePicker.SelectedDate.Value);
            _gridOverview.Show();
        }

        private void helpBtn_Click(object sender, RoutedEventArgs e)
        {
            Info _info = new Info();
            _info.Show();
        }

        private void status_Loaded(object sender, RoutedEventArgs e)
        {
            ((ComboBox)sender).IsDropDownOpen = true;  //als dit niet staat 3x clicken
        }

        private void aanwezighedenDataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vsError.Errors.Clear();
            // trick voor singleclick op combobox in grid
             if (aanwezighedenDataGrid.SelectedItem != null)               
             {   
                 if (aanwezighedenDataGrid.CurrentColumn.Header.ToString() == "Status")
                     aanwezighedenDataGrid.BeginEdit();
                 if (aanwezighedenDataGrid.CurrentColumn.Header.ToString() == "Reden")
                     aanwezighedenDataGrid.BeginEdit();
                 if (aanwezighedenDataGrid.CurrentColumn.Header.ToString() == "-50%")
                     aanwezighedenDataGrid.BeginEdit(); 
             } 
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Ben je zeker dat je de aanwezigheden voor deze dag wil verwijderen?", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var client = new PresenceServiceClient();
                client.deleteAanwezigheidCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_deleteAanwezigheidCompleted);
                AanwezigheidSpeler _aanwezigheidspeler = ((List<AanwezigheidSpeler>)aanwezighedenDataGrid.ItemsSource)[0];
                client.deleteAanwezigheidAsync(_aanwezigheidspeler.AanwezigheidId);
            }
        }

        void client_deleteAanwezigheidCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            foreach (AanwezigheidSpeler _aanwezigheidspeler in aanwezighedenDataGrid.ItemsSource)
            {
                _aanwezigheidspeler.Status = string.Empty;
                _aanwezigheidspeler.Reden = string.Empty;
                _aanwezigheidspeler.NietVoldoendeGespeeld = false;
            }
            bewarenBtn.IsEnabled = false;
            deleteBtn.IsEnabled = false;
        }
    }

    public class AanwezighedenTypeList
    {
        public List<string> GetAanwezigheidTypes
        {
            get
            {
                List<string> AanwezighedenTypeList = new List<string>();
                AanwezighedenTypeList.Add("Aanwezig");
                AanwezighedenTypeList.Add("Verwittigd");
                AanwezighedenTypeList.Add("NIET verwittigd");
                AanwezighedenTypeList.Add("Keeperstraining");
                AanwezighedenTypeList.Add("Te laat");
                AanwezighedenTypeList.Add("Blessure");
                AanwezighedenTypeList.Add("Blessure AANWEZIG");
                AanwezighedenTypeList.Add("Ziek");
                AanwezighedenTypeList.Add("Beurtrol");
                AanwezighedenTypeList.Add("Niet geselecteerd");
                AanwezighedenTypeList.Add("Vakantie");
                AanwezighedenTypeList.Add("Geschorst");
                AanwezighedenTypeList.Add("Disciplinair");
                AanwezighedenTypeList.Add("Doorgeschoven");
                AanwezighedenTypeList.Add("Selectie");
                AanwezighedenTypeList.Add("Testen andere club");
                return AanwezighedenTypeList;
            }
        }
    }
}
