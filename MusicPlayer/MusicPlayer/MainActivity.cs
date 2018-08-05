using Android.App;
using Android.Widget;
using Android.OS;

using System.Collections.Generic;
using System;
using Android.Views;
using Android.Content;
using static Android.Widget.AdapterView;

using System.Linq;

namespace MusicPlayer
{
    [Activity(Label = "Music Player", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private static bool startupCalled = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            //If the application was just launched.
            if (!startupCalled)
            {

                new AppUtil.OnInitalize(this);
                startupCalled = true;
            }

            ListView musicList = FindViewById<ListView>(Resource.Id.musicList);


            var songTitles = new SongData().GetList().Select(song => song.GetSongTitle());

            //Populates the listview with the list of song titles.
            musicList.Adapter = new ArrayAdapter<string>(this, Resource.Layout.List_Music, songTitles.ToList());

            musicList.TextFilterEnabled = true;

            musicList.ItemClick += OnSongSelected;
        }

        private void OnSongSelected(object sender, ItemClickEventArgs itemEventArgs)
        {
            string songTitle = ((TextView)itemEventArgs.View).Text;
            Intent intent = new Intent(this, typeof(PlayMusicActivity));

 

            //Sends the Songid to the next activity.
            intent.PutExtra("SONGID", itemEventArgs.Position);

     
            intent.PutExtra("FAVORITE", "YES");
            StartActivity(intent);
        }
    }
}