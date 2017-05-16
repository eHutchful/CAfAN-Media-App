
using System;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using DivineVerITies.Helpers;
using Android.Content;
using System.IO;
using Android.Media;
using System.Collections.Generic;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Support.V4.View;
using Android.Runtime;
using Newtonsoft.Json;
using DivineVerITies.ExoPlayer;
using Android.Support.V4.Media.Session;

namespace DivineVerITies.Fragments
{
    public class Downloaded : SupportFragment
    {
        private RecyclerView audioRecyclerView;
        private RecyclerView videoRecyclerView;
        private ProgressBar audioProgressBar;
        private ProgressBar videoProgressBar;
        private TextView audioHeading;
        private TextView videoHeading;
        private List<Media> audioplaylist;
        private List<Media> videoplaylist;
        private MediaRecyclerAdapter videoAdapter;
        private MediaRecyclerAdapter audioAdapter;
        private Android.Support.V7.Widget.SearchView mSearchView;
        private CardView emptyAudioCard;
        private CardView emptyVideoCard;
        private TextView emptyAudioText;
        private TextView emptyVideoText;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            audioplaylist = new List<Media>();
            videoplaylist = new List<Media>();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.OfflinePodcast, container, false) as View;
            audioProgressBar = view.FindViewById<ProgressBar>(Resource.Id.audio_loading);
            videoProgressBar = view.FindViewById<ProgressBar>(Resource.Id.video_loading);
            emptyAudioCard = view.FindViewById<CardView>(Resource.Id.emptyAudioCard);
            emptyVideoCard = view.FindViewById<CardView>(Resource.Id.emptyVideoCard);
            emptyAudioText = view.FindViewById<TextView>(Resource.Id.txtEmptyAudio);
            emptyVideoText = view.FindViewById<TextView>(Resource.Id.txtEmptyVideo);
            audioHeading = view.FindViewById<TextView>(Resource.Id.audio_heading);
            audioHeading.Text = "Downloaded Audios";
            videoHeading = view.FindViewById<TextView>(Resource.Id.video_heading);
            videoHeading.Text = "Downloaded Videos";
            audioRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.audio_recyclerview);
            videoRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.video_recyclerview);
            SetUpAudioRecyclerView(audioRecyclerView);
            SetUpVideoRecyclerView(videoRecyclerView);
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            HasOptionsMenu = true;
        }

        private async void getLists()
        {
            await GetOfflineList("audio");
            await GetOfflineList("video");
            if (audioplaylist.Count != 0)
            {
                audioAdapter = new MediaRecyclerAdapter(audioRecyclerView.Context, audioplaylist, Activity.Resources, "audio", true);
                audioRecyclerView.SetAdapter(audioAdapter);
                emptyAudioCard.Visibility = ViewStates.Gone;
                audioRecyclerView.Visibility = ViewStates.Visible;
                audioProgressBar.Visibility = ViewStates.Gone;
            }
            else
            {
                emptyAudioText.Text = "No Audios Have Been Downloaded Yet.";
                emptyAudioCard.Visibility = ViewStates.Visible;
                audioProgressBar.Visibility = ViewStates.Gone;
            }

            if (videoplaylist.Count != 0)
            {
                videoAdapter = new MediaRecyclerAdapter(audioRecyclerView.Context, videoplaylist, Activity.Resources, "video", true);
                videoRecyclerView.SetAdapter(videoAdapter);
                emptyVideoCard.Visibility = ViewStates.Gone;
                videoRecyclerView.Visibility = ViewStates.Visible;
                videoProgressBar.Visibility = ViewStates.Gone;
            }
            else
            {
                emptyVideoText.Text = "No Videos Have Been Downloaded Yet.";
                emptyVideoCard.Visibility = ViewStates.Visible;
                videoProgressBar.Visibility = ViewStates.Gone;
            }



        }

        public override void OnStart()
        {
            base.OnStart();
            getLists();
            //audioplaylist.Add(new Media { Title = "Test", Album = "You Alone" });
            //audioplaylist.Add(new Media { Title = "Hunger", Album = "Bless You" });
            //audioplaylist.Add(new Media { Title = "Hallelujah", Album = "This is the day" });

            //videoplaylist.Add(new Media { Title = "Test", Album = "You Alone" });
            //videoplaylist.Add(new Media { Title = "Hunger", Album = "Bless You" });
            //videoplaylist.Add(new Media { Title = "Hallelujah", Album = "This is the day" });
            //videoplaylist.Add(new Media { Title = "New day", Album = "Give Thanks" });
        }
        private async Task GetOfflineList(string type)
        {
            string specificDir = "cafan/Podcasts/" + type + "/";
            string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
            if (Directory.Exists(path))
            {
                var playlist = new List<Media>();
                string[] files = Directory.GetFiles(path);
                if (files.Length != 0)
                {
                    MediaMetadataRetriever metaRetriever = new MediaMetadataRetriever();
                    foreach (var file in files)
                    {
                        var media = new Media();
                        string filep = System.IO.Path.Combine(path, file);
                        media.Location = filep;
                        await metaRetriever.SetDataSourceAsync(filep);
                        try
                        {
                            media.Title = metaRetriever.ExtractMetadata(MetadataKey.Title);
                            if (media.Title == null)
                            {
                                media.Title = file;
                            }
                        }
                        catch (Exception exception1)
                        {
                            media.Title = file;
                        }
                        
                        try
                        {
                            media.Artist = metaRetriever.ExtractMetadata(MetadataKey.Artist);
                            if (media.Artist == null)
                            {
                                media.Artist = "Unknown";
                            }
                        }
                        catch (Exception exception1)
                        {
                            media.Artist = "Unknown";
                        }
                        try
                        {
                            media.Album = metaRetriever.ExtractMetadata(MetadataKey.Album);
                            if (media.Album == null)
                            {
                                media.Album = "Unknown";
                            }
                        }
                        catch (Exception exception1)
                        {
                            media.Album = "Unknown";
                        }
                        try
                        {
                            media.Genre = metaRetriever.ExtractMetadata(MetadataKey.Genre);
                            if (media.Genre == null)
                            {
                                media.Genre = "Unknown";
                            }
                        }
                        catch (Exception exception1)
                        {
                            media.Genre = "Unknown";
                        }
                        try
                        {
                            byte[] img = metaRetriever.GetEmbeddedPicture();
                            media.AlbumArt = BitmapFactory.DecodeByteArray(img, 0, img.Length);
                        }
                        catch (Exception exception1)
                        {
                            media.AlbumArt = null;
                        }
                        playlist.Add(media);
                    }
                    if (type == "audio")
                    {
                        audioplaylist = playlist;
                    }
                    else
                    {
                        videoplaylist = playlist;
                    }

                }
            }
        }

        private void SetUpVideoRecyclerView(RecyclerView videoRecyclerView)
        {
            RecyclerView.LayoutManager vLayoutManager =
                //new GridLayoutManager(Activity, 2, LinearLayoutManager.Horizontal, false);
                new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            videoRecyclerView.SetLayoutManager(vLayoutManager);
            //videoRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));
            videoRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            //SnapHelper snapHelper = new LinearSnapHelper();
            //snapHelper.AttachToRecyclerView(videoRecyclerView);

            videoRecyclerView.SetItemClickListener((rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                PlayerActivity.selectedVideo = videoplaylist[position];
                try
                {
                    view.Context.StopService(new Intent(view.Context, typeof(MediaPlayerService)));
                }
                catch (Exception es)
                {
                }
                var mpdIntent = new Intent(view.Context, typeof(PlayerActivity))
                    .SetData(Android.Net.Uri.Parse(videoplaylist[position].Location))
                    .PutExtra(PlayerActivity.ContentIdExtra, 3)
                    .PutExtra(PlayerActivity.ContentTypeExtra, PlayerActivity.TypeOther);
                StartActivity(mpdIntent);

            });
        }

        private void SetUpAudioRecyclerView(RecyclerView audioRecyclerView)
        {
            RecyclerView.LayoutManager aLayoutManager =
                //new GridLayoutManager(Activity, 2, LinearLayoutManager.Horizontal, false);
                new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            audioRecyclerView.SetLayoutManager(aLayoutManager);
            //audioRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));
            audioRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            //SnapHelper snapHelper = new LinearSnapHelper();
            //snapHelper.AttachToRecyclerView(audioRecyclerView);

            audioRecyclerView.SetItemClickListener(async (rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                if (audioAdapter.media == null)
                {
                    var activity = ((MainApp)Activity);
                    if (activity.binder.GetMediaPlayerService().mediaPlayer != null && activity.binder.GetMediaPlayerService().MediaPlayerState == PlaybackStateCompat.StatePlaying)
                    {
                        await activity.binder.GetMediaPlayerService().Stop();
                        MediaPlayerService.selectedAudio = audioplaylist[position];
                        MainApp.visibility = ViewStates.Visible;
                        await activity.binder.GetMediaPlayerService().Play();
                    }
                    else
                    {
                        MediaPlayerService.selectedAudio = audioplaylist[position];
                        MainApp.visibility = ViewStates.Visible;
                        await activity.binder.GetMediaPlayerService().Play();
                    }
                   

                }
                else if (audioAdapter.media.Count == audioplaylist.Count)
                {
                    var activity = ((MainApp)Activity);
                    if (activity.binder.GetMediaPlayerService().mediaPlayer != null && activity.binder.GetMediaPlayerService().MediaPlayerState == PlaybackStateCompat.StatePlaying)
                    {
                        await activity.binder.GetMediaPlayerService().Stop();
                        MediaPlayerService.selectedAudio = audioplaylist[position];
                        MainApp.visibility = ViewStates.Visible;
                        await activity.binder.GetMediaPlayerService().Play();
                    }
                    else
                    {
                        MediaPlayerService.selectedAudio = audioplaylist[position];
                        MainApp.visibility = ViewStates.Visible;
                        await activity.binder.GetMediaPlayerService().Play();
                    }
                }
                else
                {
                    var activity = ((MainApp)Activity);
                    if (activity.binder.GetMediaPlayerService().mediaPlayer != null && activity.binder.GetMediaPlayerService().MediaPlayerState == PlaybackStateCompat.StatePlaying)
                    {
                        await activity.binder.GetMediaPlayerService().Stop();
                        MediaPlayerService.selectedAudio = audioplaylist[position];
                        MainApp.visibility = ViewStates.Visible;
                        await activity.binder.GetMediaPlayerService().Play();
                    }
                    else
                    {
                        MediaPlayerService.selectedAudio = audioplaylist[position];
                        MainApp.visibility = ViewStates.Visible;
                        await activity.binder.GetMediaPlayerService().Play();
                    }
                }

            });
        }

        private int dpToPx(int dp)
        {
            int pixels = (int)((dp) * Resources.DisplayMetrics.Density + 0.5f);
            return pixels;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var item = menu.FindItem(Resource.Id.action_search);
            var searchView = MenuItemCompat.GetActionView(item);
            mSearchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();

            mSearchView.QueryTextChange += (s, e) =>
            {
                if (audioAdapter != null)
                {
                    audioAdapter.Filter.InvokeFilter(e.NewText);
                    audioAdapter.searchString = e.NewText;
                }

                if (videoAdapter != null)
                {
                    videoAdapter.Filter.InvokeFilter(e.NewText);
                    videoAdapter.searchString = e.NewText;
                }
            };

            mSearchView.QueryTextSubmit += (s, e) =>
            {
                // Handle enter/search button on keyboard here
                if (audioAdapter == null && videoAdapter == null)
                {
                    Toast.MakeText(Activity, "No media to search through", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(Activity, "Searched for: " + e.Query, ToastLength.Short).Show();
                }
                e.Handled = true;
            };

            if (true)
            {
                MenuItemCompat.SetOnActionExpandListener(item, new SearchViewExpandListener2(audioAdapter, videoAdapter));
            }
        }
    }


    public class SearchViewExpandListener2 : Java.Lang.Object, MenuItemCompat.IOnActionExpandListener
    {
        private readonly IFilterable _adapter1;
        private readonly IFilterable _adapter2;

        public SearchViewExpandListener2(IFilterable adapter1, IFilterable adapter2)
        {
            _adapter1 = adapter1;
            _adapter2 = adapter2;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            _adapter1?.Filter.InvokeFilter("");
            _adapter2?.Filter.InvokeFilter("");
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            return true;
        }
    }
}
