using Android.App;
using Android.Widget;
using Android.OS;
using Android.Nfc;
using Android.Content;
using System.Text;
using System;
using Android.Nfc.Tech;

namespace NFC_POC
{
    [Activity(Label = "NFC POC", MainLauncher = true), IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" }, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class MainActivity : Activity
    {
        private TextView content;        
        private TextView infoView;
        private NfcAdapter nfcAdapter;
        public const string MimeType = "application/nfcpoc.devowl.de";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            content = FindViewById<TextView>(Resource.Id.content);
            infoView = FindViewById<TextView>(Resource.Id.infoView);
            try
            {
                nfcAdapter = NfcAdapter.GetDefaultAdapter(this); // Try to get the adapter from the phone
            }
            catch
            {

                Toast.MakeText(this, Resource.String.error_adapater, ToastLength.Long).Show();
            }

        }

        protected override void OnPause()
        {
            if (nfcAdapter != null)
                nfcAdapter.DisableForegroundDispatch(this);
            base.OnPause();

        }

        protected override void OnStart()
        {
            base.OnStart();
            var alert = new AlertDialog.Builder(this).Create();

            //alert.SetMessage("Ready to scan?");
            //alert.SetTitle(Resource.String.ready);
            //alert.SetButton("OK", delegate {
            //    EnableWriteMode();
            //});
            //alert.Show();
        }



        /// <summary> 
        /// Identify to Android that this activity wants to be notified when  
        /// an NFC tag is discovered.
        /// 
        /// 
        /// FROM: https://github.com/xamarin/monodroid-samples/blob/master/NfcSample/MainActivity.cs
        /// </summary> 
        private void EnableWriteMode()
        {
           
            // Create an intent filter for when an NFC tag is discovered.  When 
            // the NFC tag is discovered, Android will u 
            var tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
            var filters = new[] { tagDetected };

            // When an NFC tag is detected, Android will use the PendingIntent to come back to this activity. 
            // The OnNewIntent method will invoked by Android. 
            var intent = new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);


            if (nfcAdapter == null)
            {
                var alert = new AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                alert.SetButton("OK", delegate
                {
                    content.Text = "NFC is not supported on this device.";
                });
                alert.Show();
            }
            else
                nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);

        }


        protected override void OnResume()
        {
            base.OnResume();
            Toast.MakeText(this, Resource.String.ready, ToastLength.Long).Show();
            EnableWriteMode();
        }


        protected override void OnNewIntent(Intent intent)
        {
            if (nfcAdapter == null)
                return;

            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            if (tag == null)
            {
                return;
            }
            var id = tag.GetId();

            infoView.Text = id != null && id.Length > 0 ? Convert.ToBase64String(id) : "No ID";
            var messages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            Console.WriteLine("Fu bar");
            if (messages == null)
            {
                TryAndWriteToTag(tag, new NdefMessage(new[] { new NdefRecord(NdefRecord.TnfWellKnown, //Type is text or something known
                                                                             Encoding.ASCII.GetBytes(MimeType), 
                                                                             id, 
                                                                             Encoding.ASCII.GetBytes("DevOwl Rulez")) }));
            }
            else
            {
                var hominidRecord = (messages[0] as NdefMessage).GetRecords()[0];
                var hominidName = Encoding.ASCII.GetString(hominidRecord.GetPayload());
                content.Text = hominidName;
            }

        }

        /// <summary> 
        /// This method will try and write the specified message to the provided tag.  
        /// </summary> 
        /// <param name="tag">The NFC tag that was detected.</param> 
        /// <param name="ndefMessage">An NDEF message to write.</param> 
        /// FROM: https://github.com/xamarin/monodroid-samples/blob/master/NfcSample/MainActivity.cs
        /// <returns>true if the tag was written to.</returns> 
        private bool TryAndWriteToTag(Tag tag, NdefMessage ndefMessage)
        {


            // This object is used to get information about the NFC tag as  
            // well as perform operations on it. 
            var ndef = Ndef.Get(tag);
            if (ndef != null)
            {
                ndef.Connect();


                // Once written to, a tag can be marked as read-only - check for this. 
                if (!ndef.IsWritable)
                {
                    Toast.MakeText(this, "Tag is read-only.", ToastLength.Long).Show();
                }


                // NFC tags can only store a small amount of data, this depends on the type of tag its. 
                var size = ndefMessage.ToByteArray().Length;
                if (ndef.MaxSize < size)
                {
                    Toast.MakeText(this, "Tag doesn't have enough space.", ToastLength.Long).Show();
                }


                ndef.WriteNdefMessage(ndefMessage);
                Toast.MakeText(this, "Succesfully wrote tag.", ToastLength.Long).Show();
                return true;
            }


            return false;
        }



    }
}

