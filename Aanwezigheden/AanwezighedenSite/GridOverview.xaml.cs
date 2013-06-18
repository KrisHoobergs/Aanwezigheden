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
using System.Windows.Printing;

namespace AanwezighedenSite
{
    public partial class GridOverview : ChildWindow
    {
        private int _ploegID;
        private DateTime _datum;

        public GridOverview(int ploegID, DateTime datum)
        {
            InitializeComponent();
            _ploegID = ploegID;
            _datum = datum;

            var client = new PresenceServiceClient();
            client.getAanwezighedenOverviewCompleted += new EventHandler<getAanwezighedenOverviewCompletedEventArgs>(client_getAanwezighedenOverviewCompleted);
            client.getAanwezighedenOverviewAsync(_ploegID);
            client.getWeekOverzichtCompleted += new EventHandler<getWeekOverzichtCompletedEventArgs>(client_getWeekOverzichtCompleted);
            client.getWeekOverzichtAsync(_ploegID, _datum);
            client.getAantalActiviteitenCompleted += new EventHandler<getAantalActiviteitenCompletedEventArgs>(client_getAantalActiviteitenCompleted);
            client.getAantalActiviteitenAsync(_ploegID);
        }

        void client_getAantalActiviteitenCompleted(object sender, getAantalActiviteitenCompletedEventArgs e)
        {
            gridWindow.Title = string.Format("{0} wedstrijden en {1} trainingen geregistreerd", e.Result, e.aantalTrainingen);
        }

        void client_getWeekOverzichtCompleted(object sender, getWeekOverzichtCompletedEventArgs e)
        {
            weekOverzichtDataGrid.ItemsSource = e.Result;
            weekoverzicht.Header = string.Format("Weekoverzicht voor {0}", _datum.ToShortDateString());
        }

        void client_getAanwezighedenOverviewCompleted(object sender, getAanwezighedenOverviewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                overviewGrid.DataContext = e.Result;
                overviewGrid.ItemsSource = e.Result;
            }
            else
            {
                MessageBox.Show(e.Error.ToString());
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        
        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            string documentName = string.Empty;
            PrintDocument document = new PrintDocument();

            document.PrintPage += (s, args) =>
            {
                args.PageVisual = this;
            };

            documentName = "overzicht";

            document.Print(documentName);
        }


    }

}

