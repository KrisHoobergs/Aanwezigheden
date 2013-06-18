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
    public partial class Overview : ChildWindow
    {
        private int _ploegID;
        public Overview(int ploegID)
        {
            InitializeComponent();
            _ploegID = ploegID;
            var client = new PresenceServiceClient();
            client.getAanwezighedenOverviewCompleted += new EventHandler<getAanwezighedenOverviewCompletedEventArgs>(client_getAanwezighedenOverviewCompleted);
            client.getAantalActiviteitenCompleted +=new EventHandler<getAantalActiviteitenCompletedEventArgs>(client_getAantalActiviteitenCompleted); 
            client.getAanwezighedenOverviewAsync(_ploegID);
            client.getAantalActiviteitenAsync(_ploegID);

        }

        void client_getAantalActiviteitenCompleted(object sender, getAantalActiviteitenCompletedEventArgs e)
        {
            graphWindow.Title = string.Format("{0} wedstrijden en {1} trainingen geregistreerd", e.Result, e.aantalTrainingen);
        }     
        

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        
        void client_getAanwezighedenOverviewCompleted(object sender, getAanwezighedenOverviewCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                AanwezighedencolumnSeries.DataContext = e.Result;
                AfwezighedencolumnSeries.DataContext = e.Result;
                SpeelgelegenheidcolumnSeries.DataContext = e.Result;
            }
            else
            {
                MessageBox.Show(e.Error.ToString());
            }
        }

        private void afwezighedenComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            string documentName = string.Empty;
            PrintDocument document = new PrintDocument();

            document.PrintPage += (s, args) =>
            {
                args.PageVisual = this;
            };

            documentName = "grafiek";

            document.Print(documentName);
        }

       
    }
}

