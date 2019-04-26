using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServiceReference1.Service1Client _client;
        private delegate void HandleBroadcastCallback(object sender, EventArgs e);

        public void HandleBroadcast(object sender, EventArgs e)
        {
            try
            {
                var eventData = (ServiceReference1.EventDataType)sender;
                if (this.txtEventMessages.Text != "")
                    this.txtEventMessages.Text += "\r\n";
                this.txtEventMessages.Text += string.Format("{0} (from {1})",
                    eventData.EventMessage, eventData.ClientName);
            }
            catch (Exception ex)
            {
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            BroadcastorCallback cb = new BroadcastorCallback();
            cb.SetHandler(this.HandleBroadcast);
        }

        private void btnRegisterClient_Click(object sender, RoutedEventArgs e)
        {
            if ((this._client != null))
            {
                this._client.Abort();
                this._client = null;
            }

            BroadcastorCallback cb = new BroadcastorCallback();
            cb.SetHandler(this.HandleBroadcast);

            System.ServiceModel.InstanceContext context =
                new System.ServiceModel.InstanceContext(cb);
            this._client =
                new ServiceReference1.Service1Client(context);

            this._client.RegisterClient(this.txtClientName.Text);
        }

        private void btnSendEvent_Click(object sender, RoutedEventArgs e)
        {
            if (this._client == null)
            {
                MessageBox.Show(this, "Client is not registered");
                return;
            }

            if (this.txtEventMessage.Text == "")
            {
                MessageBox.Show(this, "Cannot broadcast an empty message");
                return;
            }

            this._client.NotifyServer(
                new ServiceReference1.EventDataType()
                {
                    ClientName = this.txtClientName.Text,
                    EventMessage = this.txtEventMessage.Text
                });
        }
    }

    public class BroadcastorCallback : ServiceReference1.IService1Callback
    {
        private System.Threading.SynchronizationContext syncContext =
            AsyncOperationManager.SynchronizationContext;

        private EventHandler _broadcastorCallBackHandler;
        public void SetHandler(EventHandler handler)
        {
            this._broadcastorCallBackHandler = handler;
        }

        public void BroadcastToClient(ServiceReference1.EventDataType eventData)
        {
            syncContext.Post(new System.Threading.SendOrPostCallback(OnBroadcast),
                   eventData);
        }

        private void OnBroadcast(object eventData)
        {
            this._broadcastorCallBackHandler.Invoke(eventData, null);
        }
    }
}
