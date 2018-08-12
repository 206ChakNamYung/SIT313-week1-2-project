
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

namespace MusicPlayer
{
	[Activity(Label = "LayoutActivty", MainLauncher = true)]
	public class LayoutActivty : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here

			SetContentView(Resource.Layout.Layoutt);

			Button button1 = FindViewById<Button>(Resource.Id.button1);
			button1.Click += delegate
			{
				var abc = new Intent(this, typeof(MainActivity));
				StartActivity(abc);
			};
		}
	}
}
