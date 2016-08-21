
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Exoplayer1 = Com.Google.Android.Exoplayer.ExoPlayer;
using Com.Google.Android.Exoplayer;
using Com.Google.Android.Exoplayer.Audio;
using Android.Media;
using Newtonsoft.Json;
using Android.Graphics;
using System.Net.Http;
using Android.Support.Design.Widget;

namespace DivineVerITies.ExoPlayer.Player
{

    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class Audio_Player : AppCompatActivity
    {
        Bitmap imageBitmap = null;
        private bool isVisible = true;
        private SupportToolbar toolBar;
        private ProgressBar loadingBar;
        private ImageView artworkView;

        private TextView currentPositionView;
        private TextView durationView;

        private SeekBar seekBar;
        private bool shouldSetDuration;
        private bool userInteracting;

        private ImageButton previousButton;
        private ImageButton playPauseButton;
        private ImageButton nextButton;
        private ImageButton downloadButton;

        private mState pState;
        AudioList selectedAudio;

        private View audio_player_view;

        private enum mState
        {
            Idle,
            Initialized,            
            Prepared,
            Started,
            Paused,
            Stopped,
            Preparing,
            End,
            Error,
            PlayBackCompleted
        }
        MediaPlayer _player;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            selectedAudio = JsonConvert.DeserializeObject<AudioList>(Intent.GetStringExtra("selectedItem"));

            // Create your application here
            SetContentView(Resource.Layout.audio_player);

            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Now Playing";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            _player = new MediaPlayer();
            _player.SetAudioStreamType(Stream.Music);            
            pState = mState.Idle;

            playPauseButton = FindViewById<ImageButton>(Resource.Id.audio_player_play_pause);
            playPauseButton.Click += playPauseButton_Click;

            previousButton = FindViewById<ImageButton>(Resource.Id.audio_player_previous);
            previousButton.Click +=previousButton_Click;

            nextButton = FindViewById<ImageButton>(Resource.Id.audio_player_next);
            nextButton.Click += nextButton_Click;

            downloadButton = FindViewById<ImageButton>(Resource.Id.audio_download);
            downloadButton.Click += downloadButton_Click;
                

            _player.Prepared += (sender, args) => 
            {
                _player.Start();
                loadingBar = FindViewById<ProgressBar>(Resource.Id.audio_player_loading);
                loadingBar.Visibility = ViewStates.Gone;
                artworkView.SetImageBitmap(imageBitmap); 
            };
            _player.Completion +=(sender,e)=>{_player.Stop();};

            audio_player_view = FindViewById(Resource.Id.audioPlayerView);
            ShowAudioSnackBar();
        }

        void downloadButton_Click(object sender, System.EventArgs e)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Confirm Download")
           .SetMessage("Are You Sure You Want To Download" + " " + selectedAudio.SubTitle)
           .SetPositiveButton("Yes", delegate { 
                    //Run Download Code
               
              // DownLoadItemNotification();
           })
           .SetNegativeButton("No", delegate { });
            builder.Create().Show();
        }

        private void DownLoadItemNotification()
        {
            // Instantiate the builder and set notification elements:
            Notification.Builder builder = new Notification.Builder(this)
                .SetContentTitle("Downloading Podcast")
                .SetContentText("Download In Progress")
                .SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans72))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                //.SetPriority(NotificationPriority.High)
                .SetVisibility (NotificationVisibility.Public)
                .SetCategory(Notification.CategoryProgress)
                //Initialize the download
                .SetProgress(0, 0, true);


            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);

            //Started download
            int per = 1;
            
            builder.SetContentText("Downloaded (" + per + "/100")
                    .SetProgress(100, per, false);
            // Displays the progress bar for the first time.
            notificationManager.Notify(notificationId, notification);

            //download complete
            builder.SetContentTitle("Done")
                    .SetContentText("Download complete")
                // Removes the progress bar
                   .SetProgress(0, 0, false);
            notificationManager.Notify(notificationId, notification);
        }

        private void ShowAudioSnackBar()
        {
            var text = "";
            switch (pState)
            { 
                case mState.Prepared:
                    text = "prepared";
                    break;
                case mState.End:
                    text = "ended";
                    break;
                case mState.Idle:
                    text = "idle";
                    break;
                case mState.Preparing:
                    text = "preparing";
                    break;
                case mState.Initialized:
                    text = "initialized";
                    break;
                default:
                    text = "unknown";
                    break;
            }
            
            if (text != string.Empty)
            {
                Snackbar.Make(audio_player_view, text, Snackbar.LengthLong)
                    .Show();
            }
        }

        void nextButton_Click(object sender, System.EventArgs e)
        {
            if(pState==mState.Prepared||pState==mState.Paused)
            {
                var i = _player.CurrentPosition;
                i += 1000;
                _player.SeekTo(i);

            }
            else if(pState==mState.PlayBackCompleted){}
            
        }

        private void previousButton_Click(object sender, System.EventArgs e)
        {
            if (pState == mState.Prepared || pState == mState.Paused)
            {
                var i = _player.CurrentPosition;
                i -= 1000;
                _player.SeekTo(i);

            }
            else if (pState == mState.PlayBackCompleted) { }
        }

        void playPauseButton_Click(object sender, System.EventArgs e)
        { 
            if(!_player.IsPlaying)
            {
                _player.Start();
                pState = mState.Started;
            }
            else
            {
                _player.Pause();
                pState = mState.Paused;
            }
        }

        protected async override void OnStart()
        {
            base.OnStart();
            if (isVisible)
            {
                artworkView = FindViewById<ImageView>(Resource.Id.audio_player_image);
                
                using (var client = new HttpClient())
                {
                    var imageBytes = await client.GetByteArrayAsync(selectedAudio.ImageUrl);

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    }
                }
                               
                await _player.SetDataSourceAsync(ApplicationContext,Android.Net.Uri.Parse(selectedAudio.Link));
                _player.PrepareAsync();
                
                isVisible = false;
            }

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.audio_list, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.action_audio_list:
                    Intent intent = new Intent(this, typeof(MainApp));
                    StartActivity(intent);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}