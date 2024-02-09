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
                Android.Database.ICursor cursor = contentResolver.Query(Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null);

                if (cursor != null)
                {
                    int count = 0;
                    while (cursor.MoveToNext() && count < 5)
                    {
                        string sms = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                        smsList.Add(sms);
                        count++;
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