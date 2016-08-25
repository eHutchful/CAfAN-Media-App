using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using DivineVerITies.Helpers;
using System;
using System.Collections.Generic;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class SelectTopics : AppCompatActivity
    {
        RecyclerView mRecyclerView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SelectTopics);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Select Topics";

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerview);
            SetUpRecyclerView(mRecyclerView);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            fab.Click += (o, e) =>
            {
                View anchor = o as View;
    
                Intent intent = new Intent(fab.Context, typeof(MainApp));
                StartActivity(intent);         
            };

            ShowWelcomeSelectTopicDialog();
        }

        private void ShowWelcomeSelectTopicDialog()
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Welcome!")
            .SetMessage("Select From The List Of Topics, Podcasts You Want To Be Suscribed To And These Will Be Automatically Added To Your Audio And Video Lists For Quick Access")
            .SetPositiveButton("Got It", delegate { });
            builder.Create().Show();
            
        }

        private void SetUpRecyclerView(RecyclerView mRecyclerView)
        {
            var values = GetRandomSubList(Topics.TopicStrings, 30);

            mRecyclerView.SetLayoutManager(new LinearLayoutManager(mRecyclerView.Context));
            mRecyclerView.SetAdapter(new SimpleStringRecyclerViewAdapter(mRecyclerView.Context, values, this.Resources));

        }

        private List<string> GetRandomSubList(List<string> items, int amount)
        {
            List<string> list = new List<string>();
            Random random = new Random();
            while (list.Count < amount)
            {
                list.Add(items[random.Next(items.Count)]);
            }

            return list;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.skip, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {

                case Resource.Id.action_skip:
                    Intent intent = new Intent(this, typeof(MainApp));
                    StartActivity(intent);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}