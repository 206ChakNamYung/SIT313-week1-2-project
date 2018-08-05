using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;

using MusicPlayer.AppUtil;
using System.Threading;
using Android.Graphics;
using System.Threading.Tasks;

namespace MusicPlayer
{
    [Activity(Label = "PlayMusicActivity")]
    public class PlayMusicActivity : Activity
    {
        private static bool isShuffling = false;
        private static SongData songData;
        private static Thread musicThread = new Thread(new ThreadStart(SeekBarHandler));


        private static SeekBar musicSeekbar;


        private static byte repeatStatus = 0;

        private static MediaPlayer mediaPlayer = null;
        private static Song currentPlayingSong = null;
        private static Song displayedSong;

        private ImageView playBtn, playPrevBtn, playNextBtn, shuffleBtn, repeatBtn, songImage;

        private TextView headerTitle, songTitleDisplay;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PlayMusic);



            playBtn.Click += PlayPauseMusic;
            shuffleBtn.Click += ShuffleMusic;
            playNextBtn.Click += SkipSong;
            playPrevBtn.Click += SkipSong;
            repeatBtn.Click += SetMusicRepeat;

            musicSeekbar.ProgressChanged += SeekerBarChanged;
            #endregion

            songData = new SongData();

            //If string object is sent from previous activity.
            if (Intent.GetIntExtra("SONGID", -1) != -1)
            {
                //Recieves the string object from the previous activity
                int currentSongID = Intent.GetIntExtra("SONGID", 0);

                displayedSong = songData.GetSong(currentSongID);
            }

            SetDisplay();
        }

        //Called when the seekbar is moved by user.
        private void SeekerBarChanged(object sender, SeekBar.ProgressChangedEventArgs eventArgs) {
            if (eventArgs.FromUser) {
                mediaPlayer.SeekTo(musicSeekbar.Progress);
            }
        }


        private void MediaPlayerOnCompletion(object sender, EventArgs eventArgs) {

            if (repeatStatus == 0 && !isShuffling)
            {
                //Do nothing.
                return;
            }

            else if (repeatStatus == 1)
            {
  
                PreparePlayer(true);
            }

            else if (repeatStatus == 2)
            {

                if (isShuffling)
                {

                    displayedSong = songData.GetSong(songData.GetList().ElementAt(new Random().Next(0, (songData.GetList().Count - 1))).GetSongID());
                    SetDisplay();
                }
                else
                {

                    if (songData.GetList().ElementAt(songData.GetList().Count - 1) == displayedSong)
                    {
                        //If song is at end of queue, stop playing.
                        return;
                    }
                    else
                    {

                        displayedSong = songData.GetRelativeSong(displayedSong.GetSongID());
                    }
                }
                //Plays the new song.
                PreparePlayer(true);
            }
        }

        private void PreparePlayer(bool playMusic = false) {
            try
            {

                if (musicThread.IsAlive) {
                    ThreadHandler(1);
                }


                if (mediaPlayer != null)
                {
 
                    if (mediaPlayer.IsPlaying)
                    {
                        //Stops the player first.
                        mediaPlayer.Stop();
                    }
                    mediaPlayer.Reset();
                    mediaPlayer.Release();
                }
                //Set the mediaplayer source.
                mediaPlayer = new MediaPlayer();
                mediaPlayer.SetDataSource(displayedSong.GetFileDirectory().Path);
                mediaPlayer.Prepare();

                if (playMusic) {
                    mediaPlayer.Start();
                    currentPlayingSong = displayedSong;
                    Utils.ShowToast(this, "Now playing: " + displayedSong.GetSongTitle());
                    headerTitle.Text = "Now playing~ " + Utils.Truncate(currentPlayingSong.GetSongTitle(), 5) + "..";
                    mediaPlayer.Completion += MediaPlayerOnCompletion;
                    ThreadHandler(0);
                }
            } catch (Exception p) {
                //For debug purpose.
                Utils.ShowToast(this, "ERROR: " + p.Message, false, ToastLength.Long);
            }
        }


        private void SkipSong(object sender, EventArgs eventArgs)
        {

            if (((ImageView)sender).Id == playNextBtn.Id)
            {
                //If the current song is at the end of list and the user clicks on
                //the skip next button.
                if (displayedSong.GetSongID() == (songData.GetList().Count - 1))
                {
                    displayedSong = songData.GetSong(0);
                }
                else
                {

                    displayedSong = songData.GetRelativeSong(displayedSong.GetSongID());
                }
            }
            else
            {

                if (displayedSong.GetSongID() == 0)
                {
                    //Goes to the last song in the list.
                    displayedSong = songData.GetSong((songData.GetList().Count - 1));
                }
                else
                {
                    //Gets the previous song as usual.
                    displayedSong = songData.GetRelativeSong(displayedSong.GetSongID(), false);
                }
            }
            SetDisplay();

            if (mediaPlayer != null)
            {

                if (mediaPlayer.IsPlaying)
                {
                    //Plays the new music.
                    PreparePlayer(true);
                    Utils.ShowToast(this, "Now playing: " + displayedSong.GetSongTitle());
                }
            }
        }

        private void ShuffleMusic(object sender, EventArgs eventArgs)
        {

            if (isShuffling)
            {
                //Changes the image of the shuffle button accordingly.

                shuffleBtn.SetImageDrawable(GetDrawable(Resource.Drawable.shuffle_off));
                isShuffling = false;
                Utils.ShowToast(this, "Shuffle: OFF");
            }
            else
            {
                shuffleBtn.SetImageDrawable(GetDrawable(Resource.Drawable.shuffle_on));
                isShuffling = true;
                Utils.ShowToast(this, "Shuffle: ON");
            }
        }

        private void SetMusicRepeat(object sender, EventArgs eventArgs)
        {
            //If the queue is not repeating.
            if (repeatStatus == 0)
            {
 
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_one));
                repeatStatus = 1;
                Utils.ShowToast(this, "Repeat: Current song");
            }
  
            else if (repeatStatus == 1)
            {

                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_all));
                repeatStatus = 2;
                Utils.ShowToast(this, "Repeat: Current queue");
            }
            else
            {
                //Otherwise, turn repeat off.
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_off));
                repeatStatus = 0;
                Utils.ShowToast(this, "Repeat: OFF");
            }
        }

        private void PlayPauseMusic(object sender, EventArgs eventArgs)
        {
            if (mediaPlayer != null)
            {
 
                if (mediaPlayer.IsPlaying)
                {
                    //If the currently playing song does not match the displayed song.
                    if (currentPlayingSong != displayedSong)
                    {
                        //plays the new song.
                        playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));
                        PreparePlayer(true);
                    }
                    else
                    {

                        mediaPlayer.Pause();
                        playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.play));
                    }
                }
                else
                {

                    playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));

                    //If the music was previously in a paused state.
                    if (mediaPlayer.CurrentPosition != 0 && currentPlayingSong == displayedSong)
                    {
                        mediaPlayer.Start();
                        headerTitle.Text = "Now playing~ " + Utils.Truncate(currentPlayingSong.GetSongTitle(), 5) + "..";
                    }
                    else
                    {
                        PreparePlayer(true);
                    }
                }
            }
            else {
               
                PreparePlayer(true);
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));
            }
        }

        private void SetDisplay()
        {

            //If the mediaplayer is currently playing a song.
            if (currentPlayingSong != null && mediaPlayer != null)
            {
                if (mediaPlayer.IsPlaying)
                {
                    headerTitle.Text = "Now playing~ " + Utils.Truncate(currentPlayingSong.GetSongTitle(), 5) + "..";
                }
            }
            //If there was a song playing.
            else if (currentPlayingSong != null)
            {
                headerTitle.Text = "In queue~ " + currentPlayingSong.GetSongTitle();
            }
            else
            {
                headerTitle.Text = "Song Queue empty";
            }


            songTitleDisplay.Text = (Utils.Truncate(displayedSong.GetSongTitle(), 20)) + " ~ " + displayedSong.GetSongArtist();


            shuffleBtn.SetImageDrawable(GetDrawable(isShuffling ? Resource.Drawable.shuffle_on : Resource.Drawable.shuffle_off));

            //Gets the coverart of the song.
            Bitmap coverArt = Utils.GetAlbumArt(displayedSong.GetAlbumID(), this);
 
            if (coverArt != null)
            {
 
                songImage.SetImageBitmap(coverArt);
            }


            if (repeatStatus == 0)
            {
 
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_off));
            }
            //If queue is only repeating current song.
            else if (repeatStatus == 1)
            {
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_one));
            }
            else
            {
                repeatBtn.SetImageDrawable(GetDrawable(Resource.Drawable.repeat_all));
            }


            if (currentPlayingSong == displayedSong && mediaPlayer.IsPlaying)
            {
                playBtn.SetImageDrawable(GetDrawable(Resource.Drawable.pause));
                //TODO: Seekbar
            }
        }

        private void OnBackBtnPressed(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }


        private void ThreadHandler(byte command) {
            if (command == 0)
            {
                //Starts the thread.
                musicThread = new Thread(new ThreadStart(SeekBarHandler));
                musicThread.Start();
            }
            else if (command == 1)
            {
                //Stops the thread.
                musicThread.Abort();
                musicThread.Join();
            }
            //Used for resuming a thread.
            else if (command == 2) {
                musicThread.Start();
            }
        }

        private static void SeekBarHandler() {
            try
            {
                if (mediaPlayer == null) {
                    return;
                }

                do
                {
                    //Handles the seekerbar and the duration display as the mediaplayer plays.
                    int currentPosition = mediaPlayer.CurrentPosition;
                    int totalDuration = mediaPlayer.Duration;
                    musicSeekbar.Max = totalDuration;
                    musicSeekbar.Progress = currentPosition;
                } while (mediaPlayer.IsPlaying);
            }
            catch (Exception p) {}
        }
    }
}