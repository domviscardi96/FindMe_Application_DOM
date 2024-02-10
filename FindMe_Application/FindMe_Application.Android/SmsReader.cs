using Android.App;
using Android.Content;
using FindMe_Application;
using System.Collections.Generic;
using FindMe_Application.Droid;
using System;

[assembly: Xamarin.Forms.Dependency(typeof(SmsReader))]
namespace FindMe_Application.Droid
{
    public class SmsReader : ISmsReader
    {

        public List<string> ReadSms()
        {
            List<string> smsList = new List<string>();
            try
            {
                ContentResolver contentResolver = Application.Context.ContentResolver;
                Android.Database.ICursor cursor = contentResolver.Query(Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, "date DESC");

                if (cursor != null)
                {
                    int count = 0;
                    while (cursor.MoveToNext() && count < 3)
                    {
                        string sender = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                        if (sender.Equals("+19055889628"))
                        {
                            string sms = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                            smsList.Add(sms);
                            count++;
                        }
                    }

                    cursor.Close();
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return smsList;
        }
    }
}