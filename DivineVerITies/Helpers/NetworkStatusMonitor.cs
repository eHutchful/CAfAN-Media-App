using System;

using Android.App;
using Android.Content;
using Android.Net;

namespace DivineVerITies.Helpers
{
    public class NetworkStatusMonitor
    {
        private NetworkState _state;
        private NetworkStatusBroadcastReceiver _broadcastReceiver;
        public NetworkStatusMonitor()
        {
        }
        public NetworkState State
        {
            get
            {
                UpdateNetworkStatus();
                return _state;
            }
        }
        public void UpdateNetworkStatus()
        {
            _state = NetworkState.Unknown;
            // Retrieve the connectivity manager service
            var connectivityManager = (ConnectivityManager)
                Application.Context.GetSystemService(
                    Context.ConnectivityService);
            // Check if the network is connected or connecting.
            // This means that it will be available,
            // or become available in a few seconds.
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo != null && activeNetworkInfo.IsConnectedOrConnecting)
            {
                // Now that we know it's connected, determine if we're on WiFi or something else.
                _state = activeNetworkInfo.Type == ConnectivityType.Wifi ?
                    NetworkState.ConnectedWifi : NetworkState.ConnectedData;
            }
            else
            {
                _state = NetworkState.Disconnected;
            }
        }

        public event EventHandler NetworkStatusChanged;
        public void Start()
        {
            if (_broadcastReceiver != null)
            {
                //throw new InvalidOperationException(
                //    "Network status monitoring already active.");
                return;
            }
            // Create the broadcast receiver and bind the event handler
            // so that the app gets updates of the network connectivity status
            _broadcastReceiver = new NetworkStatusBroadcastReceiver();
            _broadcastReceiver.ConnectionStatusChanged += OnNetworkStatusChanged;
            // Register the broadcast receiver
            Application.Context.RegisterReceiver(_broadcastReceiver,
              new IntentFilter(ConnectivityManager.ConnectivityAction));
        }

        void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            var currentStatus = _state;
            UpdateNetworkStatus();
            if (currentStatus != _state && NetworkStatusChanged != null)
            {
                NetworkStatusChanged(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (_broadcastReceiver == null)
            {
                //throw new InvalidOperationException(
                //    "Network status monitoring not active.");
                return;
            }
            // Unregister the receiver so we no longer get updates.
            Application.Context.UnregisterReceiver(_broadcastReceiver);
            // Set the variable to nil, so that we know the receiver is no longer used.
            _broadcastReceiver.ConnectionStatusChanged -= OnNetworkStatusChanged;
            _broadcastReceiver = null;
        }
    }
    public enum NetworkState
    {
        Unknown,
        ConnectedWifi,
        ConnectedData,
        Disconnected
    }
}