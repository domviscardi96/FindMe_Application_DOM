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
                    while (cursor.MoveToNext() && count < 15)
                    {
                        string sender = cursor.GetString(cursor.GetColumnIndexOrThrow("address"));
                        if (sender.Equals("+19055889628"))
                        {
                            string sms = cursor.GetString(cursor.GetColumnIndexOrThrow("body"));
                            long dateInMillis = cursor.GetLong(cursor.GetColumnIndexOrThrow("date"));
                            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                .AddMilliseconds(dateInMillis).ToLocalTime();
                            string formattedDate = date.ToString("dd/MM/yyyy"); // Format date as "dd/MM/yyyy"
                            smsList.Add(sms + "," + formattedDate);
                            
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